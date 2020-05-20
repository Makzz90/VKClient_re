using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class TextItem : VirtualizableItemBase
  {
    private Thickness _textMargin;
    private string _text;
    private string _fontFamily;
    private double _fontSize;
    private SolidColorBrush _foreground;
    private bool _wrap;
    //private double _lineHeight;
    private double _height;
    //private Thickness _realMargin;
    private bool _isHitTestVisible;
    private readonly Action _tapAction;

    //public Thickness RealMargin
    //{
    //  get
    //  {
    //    return this._realMargin;
    //  }
    //}

    public Thickness TextMargin
    {
      get
      {
        return this._textMargin;
      }
    }

    public override double FixedHeight
    {
      get
      {
        return this._height;
      }
    }

    public TextItem(double width, Thickness textMargin, string text, bool wrap, double fontSize, string fontFamily, double lineHeight, SolidColorBrush foreground = null, bool isHitTestVisible = true, Action tapAction = null)
      : base(width, textMargin, new Thickness())
    {
      this._text = text;
      this._textMargin = textMargin;
      this._fontFamily = fontFamily;
      this._fontSize = fontSize;
      this._foreground = foreground;
      this._wrap = wrap;
      this._isHitTestVisible = isHitTestVisible;
      this._tapAction = tapAction;
      this.MeasureHeight();
    }

    private void MeasureHeight()
    {
      this._height = this.CreateTextBlock().ActualHeight;
    }

    private TextBlock CreateTextBlock()
    {
      TextBlock textBlock = new TextBlock();
      textBlock.Text = this._text;
      textBlock.FontFamily = new FontFamily(this._fontFamily);
      textBlock.FontSize = this._fontSize;
      textBlock.IsHitTestVisible = this._isHitTestVisible;
      if (this._foreground != null)
        textBlock.Foreground = (Brush) this._foreground;
      textBlock.TextWrapping = this._wrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
      //if (this._lineHeight != 0.0)
      //{
      //  textBlock.LineHeight = this._lineHeight;
      //  textBlock.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
      //}
      textBlock.Width = this.Width;
      if (this._tapAction != null)
        textBlock.Tap += (EventHandler<GestureEventArgs>) ((sender, args) => this._tapAction());
      return textBlock;
    }

    protected override void GenerateChildren()
    {
      this.Children.Add((FrameworkElement) this.CreateTextBlock());
    }

    internal void UpdateText(string updatedText)
    {
      this._text = updatedText;
      if (this.CurrentState == VirtualizableState.Unloaded)
        return;
      TextBlock textBlock = this.Children.FirstOrDefault<FrameworkElement>((Func<FrameworkElement, bool>) (c => c is TextBlock)) as TextBlock;
      if (textBlock == null)
        return;
      textBlock.Text = this._text;
    }
  }
}
