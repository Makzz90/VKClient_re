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
      TextBlock textBlock = new TextBlock()
      {
        FontFamily = new FontFamily("Segoe WP Semibold"),
        FontSize = 20.0,
        Foreground = (Brush) solidColorBrush1
      };
      textBlock.Text = CommonResources.UNREADMESSAGES.ToUpperInvariant();
      double num1 = textBlock.ActualWidth + 23.0;
      double left = (this.Width - num1) / 2.0;
      textBlock.Margin = new Thickness(left, 0.0, 0.0, 0.0);
      string uriString = MultiResolutionHelper.Instance.AppendResolutionSuffix("/VKClient.Common;component/Resources/msgs-unreadarrow.png", false, "");
      Border border1 = new Border();
      border1.Background = (Brush) solidColorBrush1;
      double num2 = 13.0;
      border1.Width = num2;
      double num3 = 16.0;
      border1.Height = num3;
      Thickness thickness1 = new Thickness(textBlock.Margin.Left + textBlock.ActualWidth + 10.0, 7.0, 0.0, 0.0);
      border1.Margin = thickness1;
      Border border2 = border1;
      border2.OpacityMask = (Brush) new ImageBrush()
      {
        ImageSource = (ImageSource) new BitmapImage(new Uri(uriString, UriKind.Relative))
      };
      Rectangle rectangle1 = new Rectangle();
      SolidColorBrush solidColorBrush3 = solidColorBrush2;
      rectangle1.Fill = (Brush) solidColorBrush3;
      double num4 = 3.0;
      rectangle1.Height = num4;
      double num5 = left - 12.0 + 18.0;
      rectangle1.Width = num5;
      Thickness thickness2 = new Thickness(-18.0, 14.0, 0.0, 0.0);
      rectangle1.Margin = thickness2;
      Rectangle rectangle2 = rectangle1;
      Rectangle rectangle3 = new Rectangle();
      SolidColorBrush solidColorBrush4 = solidColorBrush2;
      rectangle3.Fill = (Brush) solidColorBrush4;
      double num6 = 3.0;
      rectangle3.Height = num6;
      double num7 = left - 12.0 + 18.0;
      rectangle3.Width = num7;
      Thickness thickness3 = new Thickness(left + num1 + 12.0, 14.0, 0.0, 0.0);
      rectangle3.Margin = thickness3;
      Rectangle rectangle4 = rectangle3;
      this.Children.Add((FrameworkElement) textBlock);
      this.Children.Add((FrameworkElement) border2);
      this.Children.Add((FrameworkElement) rectangle2);
      this.Children.Add((FrameworkElement) rectangle4);
    }
  }
}
