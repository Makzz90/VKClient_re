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
          double left = num2 + 12.0;
          Image image1 = new Image();
          double num3 = num1;
          image1.Width = num3;
          double num4 = this._fixedHeight;
          image1.Height = num4;
          int num5 = 2;
          image1.Stretch = (Stretch) num5;
          Thickness thickness = new Thickness(left, 12.0, 0.0, 0.0);
          image1.Margin = thickness;
          StickerItemData stickerItemData2 = stickerItemData1;
          image1.Tag = (object) stickerItemData2;
          Image image2 = image1;
          Interaction.GetBehaviors((DependencyObject) image2).Add((Behavior) new PreviewBehavior()
          {
            PreviewUri = (ScaleFactor.GetRealScaleFactor() == 100 ? stickerItemData1.LocalPathBig : stickerItemData1.LocalPathExtraBig)
          });
          image2.Tap += new EventHandler<GestureEventArgs>(this.image_Tap);
          num2 = left + num1;
          this._images.Add(image2);
        }
        this._recreateImages = false;
      }
      int index = 0;
      foreach (Image image in this._images)
      {
        if (index < stickerItemDataList.Count)
        {
          StickerItemData stickerItemData = stickerItemDataList[index];
          VeryLowProfileImageLoader.AllowBoostLoading = true;
          ImageLoader.SetUriSource(image, stickerItemData.LocalPath);
        }
        ++index;
        this.Children.Add((FrameworkElement) image);
      }
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      foreach (Image image in this._images)
        ImageLoader.SetUriSource(image, null);
    }

    private void image_Tap(object sender, GestureEventArgs e)
    {
      if (PreviewBehavior.JustShown)
        return;
      StickerItemData stickerItemData = (StickerItemData) ((FrameworkElement) sender).Tag;
      if (!stickerItemData.CanSendSticker)
        return;
      EventAggregator.Current.Publish((object) new StickerItemTapEvent()
      {
        StickerItem = stickerItemData
      });
    }

    public void SetIsHorizontal(bool isHorizontal)
    {
      this.IsHorizontal = isHorizontal;
      this._recreateImages = true;
    }
  }
}
