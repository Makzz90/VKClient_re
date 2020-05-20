using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient
{
  public class PrivacyPage : PhoneApplicationPage
  {
    internal Grid LayoutRoot;
    internal StackPanel TitlePanel;
    internal TextBlock ApplicationTitle;
    internal TextBlock PageTitle;
    internal Grid ContentPanel;
    private bool _contentLoaded;

    public PrivacyPage()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient;component/PrivacyPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.TitlePanel = (StackPanel) base.FindName("TitlePanel");
      this.ApplicationTitle = (TextBlock) base.FindName("ApplicationTitle");
      this.PageTitle = (TextBlock) base.FindName("PageTitle");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
    }
  }
}
