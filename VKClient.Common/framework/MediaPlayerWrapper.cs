using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Audio.Base;
using VKClient.Common.Library.Events;

namespace VKClient.Common.Framework
{
  public class MediaPlayerWrapper : IMediaPlayerWrapper
  {
    private static MediaPlayerWrapper _instance;
    private MediaElementState _currentState;

    public static MediaPlayerWrapper Instance
    {
      get
      {
        return MediaPlayerWrapper._instance ?? (MediaPlayerWrapper._instance = new MediaPlayerWrapper());
      }
    }

    private static BGAudioPlayerWrapper BackgroundPlayerWrapper
    {
      get
      {
        return BGAudioPlayerWrapper.Instance;
      }
    }

    public string CurrentOriginalSource { get; set; }

    public MediaElement Player { get; private set; }

    private MediaPlayerWrapper()
    {
      MediaElement mediaElement = new MediaElement();
      int num = 0;
      mediaElement.AutoPlay=(num != 0);
      this.Player = mediaElement;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      // ISSUE: method pointer
      this.Player.CurrentStateChanged+=(delegate(object sender, RoutedEventArgs args)
      {
          MediaElementState currentState = this.Player.CurrentState;
          if (currentState == this._currentState)
          {
              return;
          }
          this._currentState = currentState;
          EventAggregator.Current.Publish(new MediaPlayerStateChangedEvent());
      });
    }

    public void Stop()
    {
      this.Player.Stop();
    }

    public void Pause()
    {
      this.Player.Pause();
    }

    public void Play()
    {
      MediaPlayerWrapper.BackgroundPlayerWrapper.Close();
      this.Player.Play();
    }
  }
}
