using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Events;

namespace VKClient.Common.Framework
{
    public class ScrollableTextBlock : Control, IHandle<HyperlinkClickedEvent>, IHandle
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ScrollableTextBlock), new PropertyMetadata("", new PropertyChangedCallback(ScrollableTextBlock.OnTextPropertyChanged)));
        public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register("LineHeight", typeof(double), typeof(ScrollableTextBlock), null);
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(ScrollableTextBlock), new PropertyMetadata((TextWrapping)2));
        public static readonly DependencyProperty DisableHyperlinksProperty = DependencyProperty.Register("DisableHyperlinks", typeof(bool), typeof(ScrollableTextBlock), new PropertyMetadata(false));
        public static readonly DependencyProperty TextIdProperty = DependencyProperty.Register("TextId", typeof(string), typeof(ScrollableTextBlock), new PropertyMetadata(""));
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(ScrollableTextBlock), new PropertyMetadata((TextAlignment)1));
        public static readonly DependencyProperty HideHyperlinksForegroundProperty = DependencyProperty.Register("HideHyperlinksForeground", typeof(bool), typeof(ScrollableTextBlock), new PropertyMetadata(false));
        private StackPanel stackPanel;
        //private TextBlock measureTxt;
        private readonly int MAX_STR_LENGTH = 1100;
        private DateTime _lastTimeHyperlinkClicked = DateTime.MinValue;

        public string TextId
        {
            get
            {
                return (string)base.GetValue(ScrollableTextBlock.TextIdProperty);
            }
            set
            {
                base.SetValue(ScrollableTextBlock.TextIdProperty, value);
            }
        }

        public string Text
        {
            get
            {
                return (string)base.GetValue(ScrollableTextBlock.TextProperty);
            }
            set
            {
                base.SetValue(ScrollableTextBlock.TextProperty, value);
            }
        }

        public bool DisableHyperlinks
        {
            get
            {
                return (bool)base.GetValue(ScrollableTextBlock.DisableHyperlinksProperty);
            }
            set
            {
                base.SetValue(ScrollableTextBlock.DisableHyperlinksProperty, value);
            }
        }

        public double LineHeight
        {
            get
            {
                return (double)base.GetValue(ScrollableTextBlock.LineHeightProperty);
            }
            set
            {
                base.SetValue(ScrollableTextBlock.LineHeightProperty, value);
            }
        }

        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)base.GetValue(ScrollableTextBlock.TextWrappingProperty);
            }
            set
            {
                base.SetValue(ScrollableTextBlock.TextWrappingProperty, value);
            }
        }

        public TextAlignment TextAlignment
        {
            get
            {
                return (TextAlignment)base.GetValue(ScrollableTextBlock.TextAlignmentProperty);
            }
            set
            {
                base.SetValue(ScrollableTextBlock.TextAlignmentProperty, value);
            }
        }

        public bool HideHyperlinksForeground
        {
            get
            {
                return (bool)base.GetValue(ScrollableTextBlock.HideHyperlinksForegroundProperty);
            }
            set
            {
                base.SetValue(ScrollableTextBlock.HideHyperlinksForegroundProperty, value);
            }
        }

        public ScrollableTextBlock()
        {
            //base.\u002Ector();
            base.DefaultStyleKey = (typeof(ScrollableTextBlock));
            base.HorizontalContentAlignment = ((HorizontalAlignment)0);
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // ISSUE: explicit reference operation
            ((ScrollableTextBlock)d).ParseText((string)((DependencyPropertyChangedEventArgs)e).NewValue);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.stackPanel = this.GetTemplateChild("StackPanel") as StackPanel;
            EventAggregator.Current.Subscribe(this);
            this.ParseText(this.Text);
        }

        private void ParseText(string value)
        {
            if (value == null)
                value = "";
            if (this.stackPanel == null)
                return;
            ((PresentationFrameworkCollection<UIElement>)((Panel)this.stackPanel).Children).Clear();
            if (this.CheckFitInMaxRenderHeight(value))
            {
                RichTextBox textBlock = this.GetTextBlock();
                BrowserNavigationService.SetText((DependencyObject)textBlock, value);
                this.stackPanel.Children.Add((UIElement)textBlock);
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
            while (startIndex + 1 < allText.Length && (int)allText[startIndex + 1] == 32)
                ++startIndex;
            string str = allText.Substring(0, startIndex + 1);
            RichTextBox textBlock = this.GetTextBlock();
            BrowserNavigationService.SetText((DependencyObject)textBlock, str);
            this.stackPanel.Children.Add((UIElement)textBlock);
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
            ((UIElement)richTextBox).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.textBlock_Tap));
            richTextBox.TextWrapping = this.TextWrapping;
            if (this.LineHeight > 0.0)
            {
                richTextBox.LineStackingStrategy = ((LineStackingStrategy)1);
                richTextBox.LineHeight = this.LineHeight;
            }
            richTextBox.IsReadOnly = true;
            richTextBox.FontSize = base.FontSize;
            richTextBox.FontFamily = base.FontFamily;
            richTextBox.HorizontalContentAlignment = base.HorizontalContentAlignment;
            richTextBox.TextAlignment = this.TextAlignment;
            richTextBox.Foreground = base.Foreground;
            richTextBox.Padding = (new Thickness(-12.0, 0.0, -12.0, 0.0));
            BrowserNavigationService.SetDisableHyperlinks((DependencyObject)richTextBox, this.DisableHyperlinks);
            BrowserNavigationService.SetTextId((DependencyObject)richTextBox, this.TextId);
            BrowserNavigationService.SetHideHyperlinksForeground((DependencyObject)richTextBox, this.HideHyperlinksForeground);
            return richTextBox;
        }

        private void textBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
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
