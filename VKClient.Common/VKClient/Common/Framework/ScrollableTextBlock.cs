using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Events;

namespace VKClient.Common.Framework
{
  public class ScrollableTextBlock : Control, IHandle<HyperlinkClickedEvent>, IHandle
  {
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string), typeof (ScrollableTextBlock), new PropertyMetadata((object) "", new PropertyChangedCallback(ScrollableTextBlock.OnTextPropertyChanged)));
    public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register("LineHeight", typeof (double), typeof (ScrollableTextBlock), (PropertyMetadata) null);
    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof (TextWrapping), typeof (ScrollableTextBlock), new PropertyMetadata((object) TextWrapping.Wrap));
    public static readonly DependencyProperty DisableHyperlinksProperty = DependencyProperty.Register("DisableHyperlinks", typeof (bool), typeof (ScrollableTextBlock), new PropertyMetadata((object) false));
    public static readonly DependencyProperty TextIdProperty = DependencyProperty.Register("TextId", typeof (string), typeof (ScrollableTextBlock), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof (TextAlignment), typeof (ScrollableTextBlock), new PropertyMetadata((object) TextAlignment.Left));
    private readonly int MAX_STR_LENGTH = 1100;
    private DateTime _lastTimeHyperlinkClicked = DateTime.MinValue;
    private StackPanel stackPanel;
    //private TextBlock measureTxt;

    public string TextId
    {
      get
      {
        return (string) this.GetValue(ScrollableTextBlock.TextIdProperty);
      }
      set
      {
        this.SetValue(ScrollableTextBlock.TextIdProperty, (object) value);
      }
    }

    public string Text
    {
      get
      {
        return (string) this.GetValue(ScrollableTextBlock.TextProperty);
      }
      set
      {
        this.SetValue(ScrollableTextBlock.TextProperty, (object) value);
      }
    }

    public bool DisableHyperlinks
    {
      get
      {
        return (bool) this.GetValue(ScrollableTextBlock.DisableHyperlinksProperty);
      }
      set
      {
        this.SetValue(ScrollableTextBlock.DisableHyperlinksProperty, (object) value);
      }
    }

    public double LineHeight
    {
      get
      {
        return (double) this.GetValue(ScrollableTextBlock.LineHeightProperty);
      }
      set
      {
        this.SetValue(ScrollableTextBlock.LineHeightProperty, (object) value);
      }
    }

    public TextWrapping TextWrapping
    {
      get
      {
        return (TextWrapping) this.GetValue(ScrollableTextBlock.TextWrappingProperty);
      }
      set
      {
        this.SetValue(ScrollableTextBlock.TextWrappingProperty, (object) value);
      }
    }

    public TextAlignment TextAlignment
    {
      get
      {
        return (TextAlignment) this.GetValue(ScrollableTextBlock.TextAlignmentProperty);
      }
      set
      {
        this.SetValue(ScrollableTextBlock.TextAlignmentProperty, (object) value);
      }
    }

    public ScrollableTextBlock()
    {
      this.DefaultStyleKey = (object) typeof (ScrollableTextBlock);
      this.HorizontalContentAlignment = HorizontalAlignment.Left;
    }

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((ScrollableTextBlock) d).ParseText((string) e.NewValue);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.stackPanel = this.GetTemplateChild("StackPanel") as StackPanel;
      EventAggregator.Current.Subscribe((object) this);
      this.ParseText(this.Text);
    }

    private void ParseText(string value)
    {
      if (value == null)
        value = "";
      if (this.stackPanel == null)
        return;
      this.stackPanel.Children.Clear();
      if (this.CheckFitInMaxRenderHeight(value))
      {
        RichTextBox textBlock = this.GetTextBlock();
        BrowserNavigationService.SetText((DependencyObject) textBlock, value);
        this.stackPanel.Children.Add((UIElement) textBlock);
      }
      else
        this.ParseLineExtended(value);
    }

    private void ParseLineExtended(string allText)
    {
      if (string.IsNullOrEmpty(allText))
        return;
      int startIndex = this.MAX_STR_LENGTH;
      if (startIndex >= allText.Length)
        startIndex = allText.Length - 1;
      int num1 = allText.IndexOf(".", startIndex);
      if (num1 >= 0 && num1 - startIndex < 200)
      {
        startIndex = num1;
      }
      else
      {
        int num2 = allText.IndexOf(' ', startIndex);
        if (num2 >= 0 && num2 - startIndex < 100)
          startIndex = num2;
      }
      while (startIndex + 1 < allText.Length && (int) allText[startIndex + 1] == 32)
        ++startIndex;
      string str = allText.Substring(0, startIndex + 1);
      RichTextBox textBlock = this.GetTextBlock();
      BrowserNavigationService.SetText((DependencyObject) textBlock, str);
      this.stackPanel.Children.Add((UIElement) textBlock);
      allText = allText.Substring(startIndex + 1);
      if (allText.Length <= 0)
        return;
      this.ParseLineExtended(allText);
    }

    private bool CheckFitInMaxRenderHeight(string value)
    {
      return value.Length <= this.MAX_STR_LENGTH;
    }

    private RichTextBox GetTextBlock()
    {
      RichTextBox richTextBox = new RichTextBox();
      richTextBox.Tap += new EventHandler<GestureEventArgs>(this.textBlock_Tap);
      richTextBox.TextWrapping = this.TextWrapping;
      if (this.LineHeight > 0.0)
      {
        richTextBox.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
        richTextBox.LineHeight = this.LineHeight;
      }
      richTextBox.IsReadOnly = true;
      richTextBox.FontSize = this.FontSize;
      richTextBox.FontFamily = this.FontFamily;
      richTextBox.HorizontalContentAlignment = this.HorizontalContentAlignment;
      richTextBox.TextAlignment = this.TextAlignment;
      richTextBox.Foreground = this.Foreground;
      richTextBox.Padding = new Thickness(-12.0, 0.0, 0.0, 0.0);
      BrowserNavigationService.SetDisableHyperlinks((DependencyObject) richTextBox, this.DisableHyperlinks);
      BrowserNavigationService.SetTextId((DependencyObject) richTextBox, this.TextId);
      return richTextBox;
    }

    private void textBlock_Tap(object sender, GestureEventArgs e)
    {
      if ((DateTime.Now - this._lastTimeHyperlinkClicked).TotalMilliseconds >= 500.0)
        return;
      e.Handled = true;
    }

    void IHandle<HyperlinkClickedEvent>.Handle(HyperlinkClickedEvent message)
    {
      this._lastTimeHyperlinkClicked = DateTime.Now;
    }
  }
}
