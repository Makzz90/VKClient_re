using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.ViewModels;

namespace VKClient.Common.UC
{
  public class MiniPlayerUC : UserControl
  {
    internal Grid trackPanel;
    private bool _contentLoaded;

    private AudioPlayerViewModel ViewModel
    {
      get
      {
        return base.DataContext as AudioPlayerViewModel;
      }
    }

    public MiniPlayerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      base.DataContext = (new AudioPlayerViewModel());
      // ISSUE: method pointer
      base.Loaded+=(delegate(object o, RoutedEventArgs e)
      {
          this.ViewModel.Activate(true);
      });
    }

    private void PlayButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ViewModel.Play();
    }

    private void PauseButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ViewModel.Pause();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MiniPlayerUC.xaml", UriKind.Relative));
      this.trackPanel = (Grid) base.FindName("trackPanel");
    }
  }
}
