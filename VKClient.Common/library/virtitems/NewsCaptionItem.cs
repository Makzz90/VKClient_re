using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.VirtItems
{
  public class NewsCaptionItem : VirtualizableItemBase
  {
    private const int COMMON_MARGIN_LEFT_RIGHT = 16;
    private const int TEXT_MARGIN_TOP = 16;
    private const int TEXT_MARGIN_BOTTOM = 16;
    private const int TEXT_TOTAL_MARGIN_LEFT_NO_ICON = 16;
    private readonly NewsCaption _caption;
    private double _height;

    public override double FixedHeight
    {
      get
      {
        return this._height;
      }
    }

    public NewsCaptionItem(double width, Thickness margin, NewsCaption caption)
      : base(width, margin, new Thickness())
    {
      this._caption = caption;
      this.CreateLayout();
    }

    private void CreateLayout()
    {
      NewsTextItem newsTextItem = new NewsTextItem(this.Width - 32.0, new Thickness(16.0, 16.0, 0.0, 16.0), this._caption.text, false,  null, 20.0, new FontFamily("Segoe WP"), 24.0, (Brush) Application.Current.Resources["PhoneGray500Brush"], false, 0.0, (HorizontalAlignment) 0, "", (TextAlignment) 1, true,  null, false, true);
      this.VirtualizableChildren.Add((IVirtualizable) newsTextItem);
      this._height = 16.0 + newsTextItem.FixedHeight + 16.0;
    }

    protected override void GenerateChildren()
    {
      if (this._caption == null)
        return;
      Rectangle rectangle1 = new Rectangle();
      double num1 = this.Width - 32.0;
      ((FrameworkElement) rectangle1).Width = num1;
      double num2 = 1.0;
      ((FrameworkElement) rectangle1).Height = num2;
      double num3 = 0.1;
      ((UIElement) rectangle1).Opacity = num3;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneForegroundBrush"];
      ((Shape) rectangle1).Fill = ((Brush) solidColorBrush);
      Rectangle rectangle2 = rectangle1;
      Canvas.SetLeft((UIElement) rectangle2, 16.0);
      Canvas.SetTop((UIElement) rectangle2, this.FixedHeight - 1.0);
      this.Children.Add((FrameworkElement) rectangle2);
    }
  }
}
