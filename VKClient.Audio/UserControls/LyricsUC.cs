using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;

namespace VKClient.Audio.UserControls
{
  public class LyricsUC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBlock textBlockNowPlayingTitle;
    internal ScrollableTextBlock textBlockLyrics;
    private bool _contentLoaded;

    public LyricsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void CopyClick(object sender, RoutedEventArgs e)
    {
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Audio;component/UserControls/LyricsUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBlockNowPlayingTitle = (TextBlock) base.FindName("textBlockNowPlayingTitle");
      this.textBlockLyrics = (ScrollableTextBlock) base.FindName("textBlockLyrics");
    }
  }
}
