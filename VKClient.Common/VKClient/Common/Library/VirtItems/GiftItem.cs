using System;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Library.VirtItems
{
  public class GiftItem : VirtualizableItemBase
  {
    private readonly double _fixedHeight = 512.0 / 3.0;
    private double _verticalWidth;
    private double _horizontalWidth;
    private Gift _gift;
    public bool _isHorizontal;
    private VirtualizableImage _virtImage;
    //private Thickness thickness;
    //private Attachment gift;
    //private bool _isHorizontal1;

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
      }
    }

    public override double FixedHeight
    {
      get
      {
        return this._fixedHeight;
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
    }

    private void CreateOrUpdateLayout()
    {
      if (this._virtImage == null)
      {
        Action<VirtualizableImage> callbackOnTap = (Action<VirtualizableImage>) null;
        long stickersProductId = this._gift.stickers_product_id;
        if (stickersProductId != 0L)
          callbackOnTap = (Action<VirtualizableImage>) (image => EventAggregator.Current.Publish((object) new StickersTapEvent(stickersProductId, 0)));
        this._virtImage = new VirtualizableImage(this._fixedHeight, this._fixedHeight, new Thickness((this.Width - this._fixedHeight) / 2.0, 0.0, 0.0, 0.0), this._gift.thumb_256, callbackOnTap, "", false, false, Stretch.UniformToFill, (Brush) null, -1.0, false, false);
        this.VirtualizableChildren.Add((IVirtualizable) this._virtImage);
      }
      else
        this._virtImage.Margin = new Thickness((this.Width - this._fixedHeight) / 2.0, 0.0, 0.0, 0.0);
    }
  }
}
