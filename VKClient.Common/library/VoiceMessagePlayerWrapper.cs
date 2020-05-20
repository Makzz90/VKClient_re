using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.UC;

namespace VKClient.Common.Library
{
  public class VoiceMessagePlayerWrapper
  {
    private readonly IVoiceMessage _voiceMessage;
    private readonly VoiceMessagePlayer _player;
    private WaveformControl _waveformControl;
    private TextBlock _textBlockDuration;
    private Border _borderPlay;
    private Border _borderPause;
    private bool _isManipulating;
    private double _waveformWidth;

    public double WaveformWidth
    {
      get
      {
        return this._waveformWidth;
      }
      set
      {
        this._waveformWidth = value;
        this._waveformControl.WaveformWidth = Math.Max(0.0, this._waveformWidth);
      }
    }

    public bool IsPlaying
    {
      get
      {
        return this._player.IsPlaying;
      }
    }

    public Action MediaEnded { get; set; }

    public VoiceMessagePlayerWrapper(Doc doc)
    {
      this._voiceMessage = (IVoiceMessage) doc.preview.audio_msg;
      this._player = new VoiceMessagePlayer(doc)
      {
        PlaybackStarting = new Action(this.SetPosition),
        IsPlayingChanged = new Action(this.UpdateIsPlayingState),
        MediaFailed = new Action(this.OnMediaFailed),
        MediaEnded = new Action(this.OnMediaEnded),
        PositionUpdated = new Action<TimeSpan>(this.OnPositionUpdated),
        ResetCallback = new Action(this.ResetCallback)
      };
    }

    public void Init(WaveformControl waveformControl, TextBlock textBlockDuration, Border borderPlay, Border borderPause)
    {
      this._waveformControl = waveformControl;
      this._textBlockDuration = textBlockDuration;
      this._borderPlay = borderPlay;
      this._borderPause = borderPause;
      this.UnubscribeFromManipulationEvents();
      this.SubscribeToManipulationEvents();
      this._waveformControl.Waveform = this._voiceMessage.Waveform;
      this._player.InitializePlayer();
    }

    private void SubscribeToManipulationEvents()
    {
      ((UIElement) this._waveformControl).ManipulationStarted += (new EventHandler<ManipulationStartedEventArgs>(this.SliderPosition_OnManipulationStarted));
      ((UIElement) this._waveformControl).ManipulationDelta += (new EventHandler<ManipulationDeltaEventArgs>(this.SliderPosition_OnManipulationDelta));
      ((UIElement) this._waveformControl).ManipulationCompleted += (new EventHandler<ManipulationCompletedEventArgs>(this.SliderPosition_OnManipulationCompleted));
    }

    private void UnubscribeFromManipulationEvents()
    {
      ((UIElement) this._waveformControl).ManipulationStarted-=(new EventHandler<ManipulationStartedEventArgs>(this.SliderPosition_OnManipulationStarted));
      ((UIElement) this._waveformControl).ManipulationDelta-=(new EventHandler<ManipulationDeltaEventArgs>(this.SliderPosition_OnManipulationDelta));
      ((UIElement) this._waveformControl).ManipulationCompleted-=(new EventHandler<ManipulationCompletedEventArgs>(this.SliderPosition_OnManipulationCompleted));
    }

    public void SetValues(double maximum, double value, string currentDuration)
    {
      if (this._player.IsCurrentPlayer)
      {
        ((Control) this._waveformControl).IsEnabled = true;
        ((RangeBase) this._waveformControl).Maximum = maximum;
        ((RangeBase) this._waveformControl).Value = value;
        this._textBlockDuration.Text = currentDuration;
      }
      else
        this.ResetValues();
    }

    public void ResetValues()
    {
      ((RangeBase) this._waveformControl).Maximum = 100.0;
      this.SetProgressToEnd();
      this.ResetDurationString();
    }

    public void PlayPause()
    {
      VoiceMessagePlayer player = this._player;
      if (player == null)
        return;
      player.PlayPause();
    }

    private void OnMediaFailed()
    {
      this.SetProgressToEnd();
    }

    private void OnMediaEnded()
    {
      if (this._isManipulating)
        return;
      this.SetProgressToEnd();
      this.ResetDurationString();
      Action mediaEnded = this.MediaEnded;
      if (mediaEnded == null)
        return;
      mediaEnded();
    }

    private void OnPositionUpdated(TimeSpan playerPosition)
    {
      if (this._isManipulating)
        return;
      ((RangeBase) this._waveformControl).Value = playerPosition.TotalMilliseconds;
      this.SetDurationString(playerPosition);
      this.UpdateIsPlayingState();
    }

    private void ResetCallback()
    {
      ((Control) this._waveformControl).IsEnabled = false;
      this.SetProgressToEnd();
      this.ResetDurationString();
    }

    private void SetProgressToEnd()
    {
      ((RangeBase) this._waveformControl).Value=(((RangeBase) this._waveformControl).Maximum);
    }

    private void SetDurationString(TimeSpan timeSpan)
    {
      this._textBlockDuration.Text = (VoiceMessagePlayerWrapper.GetDurationString(timeSpan));
    }

    private void ResetDurationString()
    {
      this._textBlockDuration.Text = (VoiceMessagePlayerWrapper.GetDurationString(TimeSpan.FromSeconds((double) this._voiceMessage.Duration)));
    }

    private static string GetDurationString(TimeSpan timeSpan)
    {
      return timeSpan.ToString(timeSpan.Hours > 0 ? "h\\:m\\:ss" : "m\\:ss");
    }

    private void UpdatePlayerSliderValues()
    {
      double totalDuration = this._player.TotalDuration;
      if (!double.IsNaN(totalDuration) && !double.IsInfinity(totalDuration) || totalDuration == 0.0)
      {
        ((RangeBase) this._waveformControl).Maximum = totalDuration;
        double d = ((RangeBase) this._waveformControl).Value * (totalDuration / ((RangeBase) this._waveformControl).Maximum);
        if (double.IsNaN(d) || double.IsInfinity(d))
          return;
        ((RangeBase) this._waveformControl).Value = d;
      }
      else
      {
        ((RangeBase) this._waveformControl).Maximum = 100.0;
        ((RangeBase) this._waveformControl).Value = 0.0;
      }
    }

    private void SetPosition()
    {
      ((Control) this._waveformControl).IsEnabled = true;
      this.UpdatePlayerSliderValues();
      double num = ((RangeBase) this._waveformControl).Value * (this._player.TotalDuration / ((RangeBase) this._waveformControl).Maximum);
      ((RangeBase) this._waveformControl).SmallChange=(((RangeBase) this._waveformControl).Maximum / 10.0);
      ((RangeBase) this._waveformControl).LargeChange=(((RangeBase) this._waveformControl).Maximum / 10.0);
      if (((RangeBase) this._waveformControl).Value >= ((RangeBase) this._waveformControl).Maximum)
        num = 0.0;
      if (double.IsNaN(num) || double.IsInfinity(num))
        num = 0.0;
      ((RangeBase) this._waveformControl).Value = num;
      this._player.SetPosition(num);
    }

    private void SliderPosition_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      if (!this._player.CanSeek || !this._player.IsCurrentPath)
        e.Handled = true;
      else
        this._isManipulating = true;
    }

    private void SliderPosition_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      this.SetDurationString(TimeSpan.FromMilliseconds(((RangeBase) this._waveformControl).Value));
    }

    private void SliderPosition_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      double num = ((RangeBase) this._waveformControl).Value;
      this._isManipulating = false;
      if (this._player.Source !=  null && this._player.CanSeek && this._player.IsCurrentPath)
      {
        if (num >= ((RangeBase) this._waveformControl).Maximum)
          num = ((RangeBase) this._waveformControl).Maximum - 0.01;
        this._player.Position = TimeSpan.FromMilliseconds(num);
        this.SetDurationString(this._player.Position);
      }
      else
        ((RangeBase) this._waveformControl).Value = 0.0;
    }

    private void UpdateIsPlayingState()
    {
      bool isPlaying = this._player.IsPlaying;
      ((UIElement) this._borderPause).Visibility = (isPlaying.ToVisiblity());
      ((UIElement) this._borderPlay).Visibility = ((!isPlaying).ToVisiblity());
    }
  }
}
