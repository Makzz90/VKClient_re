using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using Windows.Storage;

namespace VKClient.Common.UC
{
  public class AudioRecorderPreviewUC : UserControl
  {
    private VoiceMessagePlayerWrapper _playerWrapper;
    internal Border borderPlay;
    internal Border borderPause;
    internal WaveformControl waveformPreview;
    internal TextBlock textBlockDuration;
    private bool _contentLoaded;

    public Action CancelTap { get; set; }

    public Action SendTap { get; set; }

    public AudioRecorderPreviewUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((UIElement) this.borderPause).Visibility = Visibility.Collapsed;
      this.textBlockDuration.Text = ("");
    }

    public void SetData(StorageFile file, int durationSeconds, List<int> waveform)
    {
      string path = file.Path;
      this._playerWrapper = new VoiceMessagePlayerWrapper(new Doc()
      {
        guid = Guid.NewGuid(),
        preview = new DocPreview()
        {
          audio_msg = new DocPreviewVoiceMessage()
          {
            link_ogg = path,
            link_mp3 = path,
            duration = durationSeconds,
            waveform = waveform
          }
        }
      });
      this._playerWrapper.Init(this.waveformPreview, this.textBlockDuration, this.borderPlay, this.borderPause);
      this._playerWrapper.ResetValues();
      ((Control) this.waveformPreview).IsEnabled = false;
    }

    private void Play_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ((Control) this.waveformPreview).IsEnabled = true;
      VoiceMessagePlayerWrapper playerWrapper = this._playerWrapper;
      if (playerWrapper == null)
        return;
      playerWrapper.PlayPause();
    }

    private void Pause_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      VoiceMessagePlayerWrapper playerWrapper = this._playerWrapper;
      if (playerWrapper == null)
        return;
      playerWrapper.PlayPause();
    }

    private void Cancel_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.Pause();
      Action cancelTap = this.CancelTap;
      if (cancelTap == null)
        return;
      cancelTap();
    }

    private void Send_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.Pause();
      Action sendTap = this.SendTap;
      if (sendTap == null)
        return;
      sendTap();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size newSize = e.NewSize;
      // ISSUE: explicit reference operation
      double width = ((Size) @newSize).Width;
      ((FrameworkElement) this.waveformPreview).Width = width;
      this._playerWrapper.WaveformWidth = width;
    }

    public void Pause()
    {
      if (this._playerWrapper == null || !this._playerWrapper.IsPlaying)
        return;
      this._playerWrapper.PlayPause();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/AudioRecorderPreviewUC.xaml", UriKind.Relative));
      this.borderPlay = (Border) base.FindName("borderPlay");
      this.borderPause = (Border) base.FindName("borderPause");
      this.waveformPreview = (WaveformControl) base.FindName("waveformPreview");
      this.textBlockDuration = (TextBlock) base.FindName("textBlockDuration");
    }
  }
}
