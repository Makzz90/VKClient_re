using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Library;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Audio.UserControls
{
  public class PlaylistUC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBlock textBlockTitle;
    internal ExtendedLongListSelector AllAudios;
    private bool _contentLoaded;

    public PlaylistUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.textBlockTitle.Text = (CommonResources.Audio_Playlist.ToUpperInvariant());
    }

    private void AllAudios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
      AudioHeader audioHeader = (longListSelector != null ? longListSelector.SelectedItem : null) as AudioHeader;
      if (audioHeader == null)
        return;
      audioHeader.TryAssignTrack();
      longListSelector.SelectedItem = null;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Audio;component/UserControls/PlaylistUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.AllAudios = (ExtendedLongListSelector) base.FindName("AllAudios");
    }
  }
}
