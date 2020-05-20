using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Stickers.ViewModels;

namespace VKClient.Common.Library.VirtItems
{
  public class GiftStickersButtonItem : VirtualizableItemBase
  {
    private readonly double _portraitWidth;
    private readonly double _landscapeWidth;
    private bool _isLandscape;
    private readonly long _stickersProductId;
    private Grid _container;

    public bool IsLandscape
    {
      get
      {
        return this._isLandscape;
      }
      set
      {
        if (this._isLandscape == value)
          return;
        this._isLandscape = value;
        this.UpdateLayout();
      }
    }

    public override double FixedHeight
    {
      get
      {
        return 56.0;
      }
    }

    public GiftStickersButtonItem(double portraitWidth, double landscapeWidth, bool isLandscape, Thickness margin, long stickersProductId)
      : base(portraitWidth, margin, new Thickness())
    {
      this._portraitWidth = portraitWidth;
      this._landscapeWidth = landscapeWidth;
      this._isLandscape = isLandscape;
      this._stickersProductId = stickersProductId;
      this.Width = this._isLandscape ? this._landscapeWidth : this._portraitWidth;
    }

    private new void UpdateLayout()
    {
      this.Width = this._isLandscape ? this._landscapeWidth : this._portraitWidth;
      if (this._container == null)
        return;
      ((FrameworkElement) this._container).Width = this.Width;
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      Rectangle rectangle1 = new Rectangle();
      double num1 = 1.0;
      ((FrameworkElement) rectangle1).Height = num1;
      int num2 = 0;
      ((FrameworkElement) rectangle1).VerticalAlignment = ((VerticalAlignment) num2);
      double num3 = 0.1;
      ((UIElement) rectangle1).Opacity = num3;
      SolidColorBrush solidColorBrush1 = Application.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush;
      ((Shape) rectangle1).Fill = ((Brush) solidColorBrush1);
      Rectangle rectangle2 = rectangle1;
      TextBlock textBlock1 = new TextBlock();
      Thickness thickness = new Thickness(0.0, 13.0, 0.0, 0.0);
      ((FrameworkElement) textBlock1).Margin = thickness;
      int num4 = 0;
      ((FrameworkElement) textBlock1).VerticalAlignment = ((VerticalAlignment) num4);
      int num5 = 1;
      ((FrameworkElement) textBlock1).HorizontalAlignment = ((HorizontalAlignment) num5);
      SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneBlue300_GrayBlue100Brush"] as SolidColorBrush;
      textBlock1.Foreground = ((Brush) solidColorBrush2);
      string openStickerPack = CommonResources.OpenStickerPack;
      textBlock1.Text = openStickerPack;
      TextBlock textBlock2 = textBlock1;
      Border border1 = new Border();
      SolidColorBrush solidColorBrush3 = new SolidColorBrush(Colors.Transparent);
      border1.Background = ((Brush) solidColorBrush3);
      TextBlock textBlock3 = textBlock2;
      border1.Child = ((UIElement) textBlock3);
      Border border2 = border1;
      MetroInMotion.SetTilt((DependencyObject) border2, 1.5);
      Grid grid = new Grid();
      double width = this.Width;
      ((FrameworkElement) grid).Width = width;
      double fixedHeight = this.FixedHeight;
      ((FrameworkElement) grid).Height = fixedHeight;
      SolidColorBrush solidColorBrush4 = new SolidColorBrush(Colors.Transparent);
      ((Panel) grid).Background = ((Brush) solidColorBrush4);
      this._container = grid;
      ((UIElement) this._container).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>) ((sender, args) =>
      {
        if (this._stickersProductId == 0L)
          return;
        EventAggregator.Current.Publish(new StickersTapEvent(this._stickersProductId, 0));
      }));
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._container).Children).Add((UIElement) border2);
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._container).Children).Add((UIElement) rectangle2);
      this.Children.Add((FrameworkElement) this._container);
    }
  }
}
