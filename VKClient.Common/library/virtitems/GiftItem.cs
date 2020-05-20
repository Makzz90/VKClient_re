using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Library.VirtItems
{
  public class GiftItem : VirtualizableItemBase
  {
    private const double FIXED_HEIGHT = 224.0;
    private const int IMAGE_WIDTH_HEIGHT = 192;
    private readonly double _verticalWidth;
    private readonly double _horizontalWidth;
    private readonly Gift _gift;
    private bool _isHorizontal;
    private VirtualizableImage _virtImage;
    private Rectangle _rectBackground;

    public bool IsHorizontal
    {
      get
      {
        return this._isHorizontal;
      }
      set
      {
        if (this._isHorizontal == value)
          return;
        this._isHorizontal = value;
        this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
        this.CreateOrUpdateLayout();
        if (this._rectBackground == null)
          return;
        ((FrameworkElement) this._rectBackground).Width = this.Width;
      }
    }

    public override double FixedHeight
    {
      get
      {
        return 224.0;
      }
    }

    public GiftItem(double verticalWidth, Thickness margin, Gift gift, double horizontalWidth, bool isHorizontal)
        : base(horizontalWidth, margin, new Thickness())
    {
      this._verticalWidth = verticalWidth;
      this._horizontalWidth = horizontalWidth;
      this._isHorizontal = isHorizontal;
      this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
      this._gift = gift;
      this.CreateOrUpdateLayout();
      ((UIElement) this._view).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>) ((sender, args) =>
      {
        long stickersProductId = this._gift.stickers_product_id;
        EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.dialog, GiftPurchaseStepsAction.gifts_page));
        if (stickersProductId != 0L)
          EventAggregator.Current.Publish(new StickersTapEvent(stickersProductId, 0));
        else
          Navigator.Current.NavigateToGifts(AppGlobalStateManager.Current.LoggedInUserId, "", "");
      }));
    }

    private void CreateOrUpdateLayout()
    {
      if (this._virtImage == null)
      {
        this._virtImage = new VirtualizableImage(192.0, 192.0, new Thickness((this.Width - 192.0) / 2.0, 16.0, 0.0, 0.0), this._gift.thumb_256,  null, "", false, false, (Stretch) 3,  null, -1.0, false, false);
        this.VirtualizableChildren.Add((IVirtualizable) this._virtImage);
      }
      else
        this._virtImage.Margin = new Thickness((this.Width - 224.0) / 2.0, 16.0, 0.0, 0.0);
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      Rectangle rectangle = new Rectangle();
      double width = this.Width;
      ((FrameworkElement) rectangle).Width = width;
      double num = 224.0;
      ((FrameworkElement) rectangle).Height = num;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.White);
      ((Shape) rectangle).Fill = ((Brush) solidColorBrush);
      this._rectBackground = rectangle;
      this.Children.Add((FrameworkElement) this._rectBackground);
    }
  }
}
