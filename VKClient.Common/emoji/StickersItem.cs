using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Shared.ImagePreview;
using VKClient.Common.Utils;

namespace VKClient.Common.Emoji
{
  public class StickersItem : VirtualizableItemBase, ISupportOrientationChange
  {
    private readonly List<Image> _images = new List<Image>();
    private readonly double _verticalWidth;
    private readonly double _horizontalWidth;
    private bool _isHorizontal;
    private readonly List<StickerItemData> _verticalStickers;
    private readonly List<StickerItemData> _horizontalStickers;
    private readonly double _fixedHeight;
    private readonly int _maxCountVertical;
    private readonly int _maxCountHorizontal;
    private const double MARGIN = 12.0;
    private static int TotalCount;
    private bool _recreateImages;

    private int MaxCount
    {
      get
      {
        if (!this._isHorizontal)
          return this._maxCountVertical;
        return this._maxCountHorizontal;
      }
    }

    private bool IsHorizontal
    {
      get
      {
        return this._isHorizontal;
      }
      set
      {
        this._isHorizontal = value;
        this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
      }
    }

    public override double FixedHeight
    {
      get
      {
        if (!this._isHorizontal || this._horizontalStickers.Count != 0)
          return this._fixedHeight + 12.0;
        return 0.0;
      }
    }

    public StickersItem(double verticalWidth, Thickness margin, double horizontalWidth, bool isHorizontal, double fixedHeight, int maxCountVertical, int maxCountHorizontal, List<StickerItemData> verticalStickers, List<StickerItemData> horizontalStickers)
        : base(verticalWidth, margin, new Thickness())
    {
      ++StickersItem.TotalCount;
      this._verticalWidth = verticalWidth;
      this._horizontalWidth = horizontalWidth;
      this._isHorizontal = isHorizontal;
      this._verticalStickers = verticalStickers;
      this._horizontalStickers = horizontalStickers;
      this._fixedHeight = fixedHeight;
      this._maxCountVertical = maxCountVertical;
      this._maxCountHorizontal = maxCountHorizontal;
      if (!this._isHorizontal)
        return;
      this.Width = this._horizontalWidth;
    }

    ~StickersItem()
    {
      --StickersItem.TotalCount;
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      List<StickerItemData> stickerItemDataList = this._isHorizontal ? this._horizontalStickers : this._verticalStickers;
      if (stickerItemDataList.Count == 0)
        return;
      double num1 = (this.Width - 12.0 * (double) (this.MaxCount + 1)) / (double) this.MaxCount;
      double num2 = 0.0;
      if (this._images.Count == 0 || this._recreateImages)
      {
        this._images.Clear();
        foreach (StickerItemData stickerItemData1 in stickerItemDataList)
        {
          double num3 = num2 + 12.0;
          Image image1 = new Image();
          double num4 = num1;
          ((FrameworkElement) image1).Width = num4;
          double fixedHeight = this._fixedHeight;
          ((FrameworkElement) image1).Height = fixedHeight;
          int num5 = 2;
          image1.Stretch=((Stretch) num5);
          Thickness thickness = new Thickness(num3, 12.0, 0.0, 0.0);
          ((FrameworkElement) image1).Margin = thickness;
          StickerItemData stickerItemData2 = stickerItemData1;
          ((FrameworkElement) image1).Tag = stickerItemData2;
          Image image2 = image1;
          Interaction.GetBehaviors((DependencyObject) image2).Add((Behavior) new PreviewBehavior()
          {
            PreviewUri = (ScaleFactor.GetRealScaleFactor() == 100 ? stickerItemData1.LocalPathBig : stickerItemData1.LocalPathExtraBig)
          });
          ((UIElement) image2).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.image_Tap));
          num2 = num3 + num1;
          this._images.Add(image2);
        }
        this._recreateImages = false;
      }
      int index = 0;
      using (List<Image>.Enumerator enumerator = this._images.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          Image current = enumerator.Current;
          if (index < stickerItemDataList.Count)
          {
            StickerItemData stickerItemData = stickerItemDataList[index];
            VeryLowProfileImageLoader.AllowBoostLoading = true;
            ImageLoader.SetUriSource(current, stickerItemData.LocalPath);
          }
          ++index;
          this.Children.Add((FrameworkElement) current);
        }
      }
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      using (List<Image>.Enumerator enumerator = this._images.GetEnumerator())
      {
        while (enumerator.MoveNext())
          ImageLoader.SetUriSource(enumerator.Current,  null);
      }
    }

    private void image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (PreviewBehavior.JustShown)
        return;
      StickerItemData tag = (StickerItemData) ((FrameworkElement) sender).Tag;
      if (!tag.CanSendSticker)
        return;
      EventAggregator.Current.Publish(new StickerItemTapEvent()
      {
        StickerItem = tag
      });
    }

    public void SetIsHorizontal(bool isHorizontal)
    {
      this.IsHorizontal = isHorizontal;
      this._recreateImages = true;
    }
  }
}
