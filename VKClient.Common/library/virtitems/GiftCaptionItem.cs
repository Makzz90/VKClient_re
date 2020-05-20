using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.VirtItems
{
  public class GiftCaptionItem : VirtualizableItemBase
  {
    private readonly double _portraitWidth;
    private readonly double _landscapeWidth;
    private readonly bool _isStickersGift;
    private readonly bool _isForwarded;
    private bool _isLandscape;
    private Border _container;

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

    public GiftCaptionItem(double portraitWidth, double landscapeWidth, bool isLandscape, Thickness margin, bool isStickersGift, bool isForwarded)
        : base(portraitWidth, margin, new Thickness())
    {
      this._portraitWidth = portraitWidth;
      this._landscapeWidth = landscapeWidth;
      this._isLandscape = isLandscape;
      this._isStickersGift = isStickersGift;
      this._isForwarded = isForwarded;
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
      double num1 = 24.0;
      ((FrameworkElement) rectangle1).Width = num1;
      double num2 = 24.0;
      ((FrameworkElement) rectangle1).Height = num2;
      Thickness thickness1 = new Thickness(0.0, 16.0, 0.0, 0.0);
      ((FrameworkElement) rectangle1).Margin = thickness1;
      int num3 = 0;
      ((FrameworkElement) rectangle1).VerticalAlignment = ((VerticalAlignment) num3);
      SolidColorBrush solidColorBrush1 = this._isForwarded ? Application.Current.Resources["PhoneDialogGiftForwardedCaptionForegroundBrush"] as SolidColorBrush : Application.Current.Resources["PhoneDialogGiftCaptionIconBackgroundBrush"] as SolidColorBrush;
      ((Shape) rectangle1).Fill = ((Brush) solidColorBrush1);
      Rectangle rectangle2 = rectangle1;
      ImageBrush imageBrush = new ImageBrush();
      ImageLoader.SetImageBrushMultiResSource(imageBrush, this._isStickersGift ? "/Resources/Smile24px.png" : "/Resources/Gift24px.png");
      ((UIElement) rectangle2).OpacityMask=((Brush) imageBrush);
      TextBlock textBlock1 = new TextBlock();
      Thickness thickness2 = new Thickness(12.0, 13.0, 0.0, 0.0);
      ((FrameworkElement) textBlock1).Margin = thickness2;
      int num4 = 0;
      ((FrameworkElement) textBlock1).VerticalAlignment = ((VerticalAlignment) num4);
      double num5 = 20.0;
      textBlock1.FontSize = num5;
      FontFamily fontFamily = new FontFamily("Segoe WP");
      textBlock1.FontFamily = fontFamily;
      SolidColorBrush solidColorBrush2 = this._isForwarded ? Application.Current.Resources["PhoneDialogGiftForwardedCaptionForegroundBrush"] as SolidColorBrush : Application.Current.Resources["PhoneDialogGiftCaptionForegroundBrush"] as SolidColorBrush;
      textBlock1.Foreground = ((Brush) solidColorBrush2);
      string str = this._isStickersGift ? CommonResources.StickerPack : CommonResources.Gift;
      textBlock1.Text = str;
      TextBlock textBlock2 = textBlock1;
      StackPanel stackPanel1 = new StackPanel();
      double fixedHeight1 = this.FixedHeight;
      ((FrameworkElement) stackPanel1).Height = fixedHeight1;
      int num6 = 1;
      ((FrameworkElement) stackPanel1).HorizontalAlignment = ((HorizontalAlignment) num6);
      int num7 = 1;
      stackPanel1.Orientation=((Orientation) num7);
      StackPanel stackPanel2 = stackPanel1;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) stackPanel2).Children).Add((UIElement) rectangle2);
      ((PresentationFrameworkCollection<UIElement>) ((Panel) stackPanel2).Children).Add((UIElement) textBlock2);
      Border border = new Border();
      double width = this.Width;
      ((FrameworkElement) border).Width = width;
      double fixedHeight2 = this.FixedHeight;
      ((FrameworkElement) border).Height = fixedHeight2;
      StackPanel stackPanel3 = stackPanel2;
      border.Child = ((UIElement) stackPanel3);
      this._container = border;
      this.Children.Add((FrameworkElement) this._container);
    }
  }
}
