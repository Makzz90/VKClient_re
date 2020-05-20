using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.VirtItems
{
  public class NewsTextItem : VirtualizableItemBase
  {
    private readonly ScrollableTextBlock _textBlockPreview = new ScrollableTextBlock()
    {
      DisableHyperlinks = false
    };
    private const int MaxNumberOfSymbolsInPreview = 300;
    private const double LineHeight = 31.6;
    private readonly double _fontSize;
    private readonly FontFamily _fontFamily;
    private readonly double _lineHeight;
    private readonly Brush _foreground;
    private readonly double _horizontalWidth;
    private readonly double _verticalWidth;
    private readonly HorizontalAlignment _horizontalContentAlignment;
    private readonly TextAlignment _textAlignment;
    private bool _supportExpandText;
    private ScrollableTextBlock _textBlockFull;
    private readonly string _textId;
    private string _text;
    private double _verticalHeight;
    private double _horizontalHeight;
    private bool _preview;
    private bool _showReadFull;
    private bool _isHorizontalOrientation;

    public bool IsHorizontalOrientation
    {
      get
      {
        return this._isHorizontalOrientation;
      }
      set
      {
        if (this._isHorizontalOrientation == value)
          return;
        this._isHorizontalOrientation = value;
        this.UpdateLayout();
      }
    }

    public override double FixedHeight
    {
      get
      {
        if (!this._isHorizontalOrientation)
          return this._verticalHeight;
        return this._horizontalHeight;
      }
    }

    public NewsTextItem(double width, Thickness margin, string text, bool preview, Action tapCallback = null, double fontSize = 0.0, FontFamily fontFamily = null, double lineHeight = 0.0, Brush foreground = null, bool isHorizontalOrientation = false, double horizontalWidth = 0.0, HorizontalAlignment horizontalContentAlignment = HorizontalAlignment.Left, string textId = "", TextAlignment textAlignment = TextAlignment.Left, bool supportExpandText = true)
      : base(width, margin, new Thickness())
    {
      this._text = text ?? "";
      this._textId = textId;
      this._fontSize = fontSize;
      this._fontFamily = fontFamily;
      this._lineHeight = lineHeight;
      this._foreground = foreground;
      this._horizontalContentAlignment = horizontalContentAlignment;
      this._textAlignment = textAlignment;
      this._supportExpandText = supportExpandText;
      this._preview = preview;
      this._isHorizontalOrientation = isHorizontalOrientation;
      this._verticalWidth = width;
      this._horizontalWidth = horizontalWidth;
      this.Width = this._isHorizontalOrientation ? horizontalWidth : width;
      this.MeasureText();
    }

    private new void UpdateLayout()
    {
      this.Width = this._isHorizontalOrientation ? this._horizontalWidth : this._verticalWidth;
      if (this._textBlockFull == null)
        return;
      this._textBlockFull.Width = this.Width;
    }

    private void MeasureText()
    {
      string text = this._text;
      if (this._preview)
      {
        ScrollableTextBlock scrollableTextBlock = this._textBlockFull;
        if ((scrollableTextBlock != null ? scrollableTextBlock.Parent : (DependencyObject) null) != null)
          return;
        int length = text.Length;
        if (length > 300)
        {
          text = BrowserNavigationService.CutTextGently(text, 300);
          if (!this._supportExpandText)
            text = text.Trim() + "...";
          this._showReadFull = text.Length != length;
        }
        this._textBlockPreview.TextId = this._textId;
        this._textBlockPreview.Text = "";
        this._textBlockPreview.FontSize = this._fontSize == 0.0 ? 20.0 : this._fontSize;
        this._textBlockPreview.FontFamily = this._fontFamily ?? new FontFamily("Segoe WP");
        this._textBlockPreview.Foreground = this._foreground ?? Application.Current.Resources["PhoneAlmostBlackBrush"] as Brush;
        this._textBlockPreview.TextWrapping = TextWrapping.Wrap;
        this._textBlockPreview.Width = this.Width;
        if (this._lineHeight > 0.0)
          this._textBlockPreview.LineHeight = this._lineHeight;
        this._textBlockPreview.Text = text;
        Grid grid = (Grid) ((UserControl) ((ContentControl) Application.Current.RootVisual).Content).Content;
        Canvas canvas = new Canvas();
        grid.Children.Add((UIElement) canvas);
        canvas.Children.Add((UIElement) this._textBlockPreview);
        canvas.UpdateLayout();
        if (this._horizontalWidth > 0.1)
          this._horizontalHeight = this._textBlockPreview.ActualHeight;
        this._textBlockPreview.Width = this._verticalWidth;
        canvas.UpdateLayout();
        this._verticalHeight = this._textBlockPreview.ActualHeight;
        canvas.Children.Remove((UIElement) this._textBlockPreview);
        grid.Children.Remove((UIElement) canvas);
        if (!this._showReadFull || !this._supportExpandText)
          return;
        this._verticalHeight = this._verticalHeight + 31.6;
      }
      else
      {
        ScrollableTextBlock scrollableTextBlock1 = this._textBlockFull;
        if ((scrollableTextBlock1 != null ? scrollableTextBlock1.Parent : (DependencyObject) null) != null)
          return;
        ScrollableTextBlock scrollableTextBlock2 = new ScrollableTextBlock();
        scrollableTextBlock2.TextId = this._textId;
        int num1 = 0;
        scrollableTextBlock2.VerticalAlignment = (VerticalAlignment) num1;
        int num2 = (int) this._horizontalContentAlignment;
        scrollableTextBlock2.HorizontalContentAlignment = (HorizontalAlignment) num2;
        int num3 = (int) this._textAlignment;
        scrollableTextBlock2.TextAlignment = (TextAlignment) num3;
        string str = text;
        scrollableTextBlock2.Text = str;
        double num4 = this._fontSize == 0.0 ? 20.0 : this._fontSize;
        scrollableTextBlock2.FontSize = num4;
        FontFamily fontFamily = this._fontFamily ?? new FontFamily("Segoe WP");
        scrollableTextBlock2.FontFamily = fontFamily;
        Brush brush = this._foreground ?? Application.Current.Resources["PhoneAlmostBlackBrush"] as Brush;
        scrollableTextBlock2.Foreground = brush;
        this._textBlockFull = scrollableTextBlock2;
        if (this._lineHeight > 0.0)
          this._textBlockFull.LineHeight = this._lineHeight;
        if (this._horizontalWidth > 0.1)
          this._textBlockFull.Width = this._horizontalWidth;
        Grid grid = (Grid) ((UserControl) ((ContentControl) Application.Current.RootVisual).Content).Content;
        Canvas canvas = new Canvas();
        grid.Children.Add((UIElement) canvas);
        canvas.Children.Add((UIElement) this._textBlockFull);
        canvas.UpdateLayout();
        if (this._horizontalWidth > 0.1)
          this._horizontalHeight = this._textBlockFull.ActualHeight;
        this._textBlockFull.Width = this._verticalWidth;
        canvas.UpdateLayout();
        this._verticalHeight = this._textBlockFull.ActualHeight;
        canvas.Children.Remove((UIElement) this._textBlockFull);
        grid.Children.Remove((UIElement) canvas);
      }
    }

    protected override void GenerateChildren()
    {
      if (string.IsNullOrEmpty(this._text))
        return;
      if (this._preview)
      {
        this.Children.Add((FrameworkElement) this._textBlockPreview);
      }
      else
      {
        this.UpdateLayout();
        this.Children.Add((FrameworkElement) this._textBlockFull);
      }
      if (!this._showReadFull || !this._supportExpandText)
        return;
      Border border1 = new Border();
      Thickness thickness1 = new Thickness(-8.0, this._verticalHeight - 31.6, -8.0, -8.0);
      border1.Margin = thickness1;
      SolidColorBrush solidColorBrush1 = new SolidColorBrush(Colors.Transparent);
      border1.Background = (Brush) solidColorBrush1;
      Border border2 = border1;
      TextBlock textBlock1 = new TextBlock();
      Thickness thickness2 = new Thickness(8.0);
      textBlock1.Margin = thickness2;
      SolidColorBrush solidColorBrush2 = Application.Current.Resources["PhoneDarkBlueBrush"] as SolidColorBrush;
      textBlock1.Foreground = (Brush) solidColorBrush2;
      FontWeight semiBold = FontWeights.SemiBold;
      textBlock1.FontWeight = semiBold;
      string str = string.Format("{0}...", (object) CommonResources.ExpandText);
      textBlock1.Text = str;
      TextBlock textBlock2 = textBlock1;
      if (this._fontSize > 0.0)
        textBlock2.FontSize = this._fontSize;
      if (this._fontFamily != null)
        textBlock2.FontFamily = this._fontFamily;
      border2.Child = (UIElement) textBlock2;
      border2.Tap += new EventHandler<GestureEventArgs>(this.TextBlockReadFull_OnTap);
      this.Children.Add((FrameworkElement) border2);
      MetroInMotion.SetTilt((DependencyObject) border2, 1.5);
    }

    private void TextBlockReadFull_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      this._preview = false;
      this._showReadFull = false;
      this.MeasureText();
      this.RegenerateChildren();
      this.NotifyHeightChanged();
    }
  }
}
