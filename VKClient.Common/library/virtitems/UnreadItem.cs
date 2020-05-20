using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.VirtItems
{
  public class UnreadItem : VirtualizableItemBase
  {
    private double _verticalWidth;
    private double _horizontalWidth;
    private bool _isHorizontal;

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
      }
    }

    public override double FixedHeight
    {
      get
      {
        return 31.0;
      }
    }

    public UnreadItem(double verticalWidth, Thickness margin, bool isHorizontal, double horizontalWidth)
        : base(verticalWidth, margin, new Thickness())
    {
      this._verticalWidth = verticalWidth;
      this._horizontalWidth = horizontalWidth;
      this._isHorizontal = isHorizontal;
      this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      SolidColorBrush solidColorBrush1 = Application.Current.Resources["PhoneVKSubtleBrush"] as SolidColorBrush;
      SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneDialogInMessageBackgroundBrush"] as SolidColorBrush;
      TextBlock textBlock1 = new TextBlock();
      FontFamily fontFamily = new FontFamily("Segoe WP Semibold");
      textBlock1.FontFamily = fontFamily;
      double num1 = 20.0;
      textBlock1.FontSize = num1;
      SolidColorBrush solidColorBrush3 = solidColorBrush1;
      textBlock1.Foreground = ((Brush) solidColorBrush3);
      TextBlock textBlock2 = textBlock1;
      textBlock2.Text = (CommonResources.UNREADMESSAGES.ToUpperInvariant());
      double num2 = ((FrameworkElement) textBlock2).ActualWidth + 23.0;
      double num3 = (this.Width - num2) / 2.0;
      ((FrameworkElement) textBlock2).Margin=(new Thickness(num3, 0.0, 0.0, 0.0));
      string uriString = MultiResolutionHelper.Instance.AppendResolutionSuffix("/VKClient.Common;component/Resources/msgs-unreadarrow.png", false, "");
      Border border1 = new Border();
      SolidColorBrush solidColorBrush4 = solidColorBrush1;
      border1.Background = ((Brush) solidColorBrush4);
      double num4 = 13.0;
      ((FrameworkElement) border1).Width = num4;
      double num5 = 16.0;
      ((FrameworkElement) border1).Height = num5;
      Thickness margin = ((FrameworkElement) textBlock2).Margin;
      // ISSUE: explicit reference operation
      Thickness thickness1 = new Thickness(((Thickness) @margin).Left + ((FrameworkElement) textBlock2).ActualWidth + 10.0, 7.0, 0.0, 0.0);
      ((FrameworkElement) border1).Margin = thickness1;
      Border border2 = border1;
      Border border3 = border2;
      ImageBrush imageBrush = new ImageBrush();
      BitmapImage bitmapImage = new BitmapImage(new Uri(uriString, UriKind.Relative));
      imageBrush.ImageSource=((ImageSource) bitmapImage);
      ((UIElement) border3).OpacityMask=((Brush) imageBrush);
      Rectangle rectangle1 = new Rectangle();
      SolidColorBrush solidColorBrush5 = solidColorBrush2;
      ((Shape) rectangle1).Fill = ((Brush) solidColorBrush5);
      double num6 = 3.0;
      ((FrameworkElement) rectangle1).Height = num6;
      double num7 = num3 - 12.0 + 18.0;
      ((FrameworkElement) rectangle1).Width = num7;
      Thickness thickness2 = new Thickness(-18.0, 14.0, 0.0, 0.0);
      ((FrameworkElement) rectangle1).Margin = thickness2;
      Rectangle rectangle2 = rectangle1;
      Rectangle rectangle3 = new Rectangle();
      SolidColorBrush solidColorBrush6 = solidColorBrush2;
      ((Shape) rectangle3).Fill = ((Brush) solidColorBrush6);
      double num8 = 3.0;
      ((FrameworkElement) rectangle3).Height = num8;
      double num9 = num3 - 12.0 + 18.0;
      ((FrameworkElement) rectangle3).Width = num9;
      Thickness thickness3 = new Thickness(num3 + num2 + 12.0, 14.0, 0.0, 0.0);
      ((FrameworkElement) rectangle3).Margin = thickness3;
      Rectangle rectangle4 = rectangle3;
      this.Children.Add((FrameworkElement) textBlock2);
      this.Children.Add((FrameworkElement) border2);
      this.Children.Add((FrameworkElement) rectangle2);
      this.Children.Add((FrameworkElement) rectangle4);
    }
  }
}
