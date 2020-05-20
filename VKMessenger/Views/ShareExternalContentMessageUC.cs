using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Utils;

namespace VKMessenger.Views
{
  public class ShareExternalContentMessageUC : UserControl
  {
    private double _savedHeight;
    internal ScrollViewer scrollViewerMessage;
    internal TextBox textBoxMessage;
    internal TextBlock textBlockWatermarkText;
    private bool _contentLoaded;

    public ShareExternalContentMessageUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void TextBoxMessage_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((UIElement) this.textBlockWatermarkText).Opacity = (this.textBoxMessage.Text == "" ? 1.0 : 0.0);
      base.Dispatcher.BeginInvoke((Action) (() =>
      {
        double actualHeight = ((FrameworkElement) this.textBoxMessage).ActualHeight;
        Thickness padding = ((Control) this.textBoxMessage).Padding;
        // ISSUE: explicit reference operation
        double bottom = ((Thickness) @padding).Bottom;
        double num = actualHeight - bottom;
        if (this._savedHeight > 0.0)
        {
          bool flag = false;
          if (num < this._savedHeight && this.scrollViewerMessage.ExtentHeight == this.scrollViewerMessage.VerticalOffset + this.scrollViewerMessage.ViewportHeight)
            flag = true;
          if (!flag)
            this.scrollViewerMessage.ScrollToOffsetWithAnimation(this.scrollViewerMessage.VerticalOffset + num - this._savedHeight, 0.15, false);
        }
        this._savedHeight = num;
      }));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/ShareExternalContentMessageUC.xaml", UriKind.Relative));
      this.scrollViewerMessage = (ScrollViewer) base.FindName("scrollViewerMessage");
      this.textBoxMessage = (TextBox) base.FindName("textBoxMessage");
      this.textBlockWatermarkText = (TextBlock) base.FindName("textBlockWatermarkText");
    }
  }
}
