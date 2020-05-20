using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class SharePostUC : UserControl
  {
    private double _savedHeight;
    private long _excludedCommunityId;
    internal TextBlock textBlockTitle;
    internal ScrollViewer scroll;
    internal TextBox textBoxText;
    internal TextBlock textBlockWatermarkText;
    internal ShareActionUC buttonShare;
    internal ShareActionUC buttonShareCommunity;
    private bool _contentLoaded;

    public string Text
    {
      get
      {
        return this.textBoxText.Text;
      }
    }

    public event EventHandler ShareTap;

    public event EventHandler ShareCommunityTap;

    public event EventHandler SendTap;

    public SharePostUC(long excludedCommunityId = 0)
    {
      //this.\u002Ector();
      this.InitializeComponent();
      this.textBlockTitle.Text = (CommonResources.ShareWallPost_Share.ToUpperInvariant());
      this._excludedCommunityId = excludedCommunityId;
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((UIElement) this.textBlockWatermarkText).Opacity = (this.textBoxText.Text == "" ? 1.0 : 0.0);
      this.ScrollToCursor();
    }

    private void ScrollToCursor()
    {
      base.Dispatcher.BeginInvoke((Action) (() =>
      {
        double actualHeight = ((FrameworkElement) this.textBoxText).ActualHeight;
        Thickness padding = ((Control) this.textBoxText).Padding;
        // ISSUE: explicit reference operation
        double bottom = ((Thickness) @padding).Bottom;
        double num = actualHeight - bottom;
        if (this._savedHeight > 0.0)
        {
          bool flag = false;
          if (num < this._savedHeight && this.scroll.ExtentHeight == this.scroll.VerticalOffset + this.scroll.ViewportHeight)
            flag = true;
          if (!flag)
            this.scroll.ScrollToOffsetWithAnimation(this.scroll.VerticalOffset + num - this._savedHeight, 0.15, false);
        }
        this._savedHeight = num;
      }));
    }

    public void SetShareEnabled(bool isEnabled)
    {
      ((Control) this.buttonShare).IsEnabled = isEnabled;
      ((UIElement) this.buttonShare).Opacity = (isEnabled ? 1.0 : 0.4);
    }

    public void SetShareCommunityEnabled(bool isEnabled)
    {
      ((Control) this.buttonShareCommunity).IsEnabled = isEnabled;
      ((UIElement) this.buttonShareCommunity).Opacity = (isEnabled ? 1.0 : 0.4);
    }

    private void ButtonShare_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.ShareTap == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.ShareTap(this, EventArgs.Empty);
    }

    private void ShareWithCommunity_OnTap(object sender, RoutedEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.ShareCommunityTap == null)
      {
        Navigator.Current.NavigateToGroups(AppGlobalStateManager.Current.LoggedInUserId, "", true, 0, 0, "", false, "", this._excludedCommunityId);
      }
      else
      {
        // ISSUE: reference to a compiler-generated field
        this.ShareCommunityTap(this, EventArgs.Empty);
      }
    }

    private void ButtonSend_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.SendTap == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.SendTap(this, EventArgs.Empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/SharePostUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.scroll = (ScrollViewer) base.FindName("scroll");
      this.textBoxText = (TextBox) base.FindName("textBoxText");
      this.textBlockWatermarkText = (TextBlock) base.FindName("textBlockWatermarkText");
      this.buttonShare = (ShareActionUC) base.FindName("buttonShare");
      this.buttonShareCommunity = (ShareActionUC) base.FindName("buttonShareCommunity");
    }
  }
}
