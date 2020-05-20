using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VKClient.Audio.Base.AudioCache;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class VoiceMessagePlayer : IHandle<VoiceMessageUploaded>, IHandle, IHandle<MediaPlayerStateChangedEvent>
  {
    private Cancellation _downloadCancellation = new Cancellation();
    private const int SAMPLE_RATE = 48000;
    private const int AUDIO_CHANNELS = 1;
    private const int BITS_PER_SAMPLE = 16;
    private DispatcherTimer _timerPlayback;
    private readonly Doc _doc;
    private string _sourceUriStr;
    private string _wavFileName;
    private string _opusFilePath;
    private long _docOwnerId;
    private long _docId;
    private string _playerSourceFilePath;
    private bool _isDownloaded;
    private bool _isDownloading;
    private bool _isPlaying;

    private static MediaPlayerWrapper PlayerWrapper
    {
      get
      {
        return MediaPlayerWrapper.Instance;
      }
    }

    private static MediaElement Player
    {
      get
      {
        return VoiceMessagePlayer.PlayerWrapper.Player;
      }
    }

    public bool IsCurrentPlayer
    {
      get
      {
        return ((FrameworkElement) VoiceMessagePlayer.Player).Tag == this;
      }
    }

    public bool IsCurrentPath
    {
      get
      {
        return Path.GetFileName(VoiceMessagePlayer.Player.Source.LocalPath) == this._playerSourceFilePath;
      }
    }

    public bool CanSeek
    {
      get
      {
        return VoiceMessagePlayer.Player.CanSeek;
      }
    }

    public double TotalDuration
    {
      get
      {
        Duration naturalDuration = VoiceMessagePlayer.Player.NaturalDuration;
        return ((Duration) @naturalDuration).TimeSpan.TotalMilliseconds;
      }
    }

    public Uri Source
    {
      get
      {
        return VoiceMessagePlayer.Player.Source;
      }
      set
      {
        VoiceMessagePlayer.Player.Source = value;
      }
    }

    public TimeSpan Position
    {
      get
      {
        return VoiceMessagePlayer.Player.Position;
      }
      set
      {
        VoiceMessagePlayer.Player.Position = value;
      }
    }

    public Action<TimeSpan> PositionUpdated { get; set; }

    public Action MediaEnded { get; set; }

    public Action MediaFailed { get; set; }

    public Action PlaybackStarting { get; set; }

    public Action IsPlayingChanged { get; set; }

    public Action ResetCallback { get; set; }

    public bool IsPlaying
    {
      get
      {
        return this._isPlaying;
      }
      set
      {
        if (this._isPlaying == value)
          return;
        this._isPlaying = value;
        Action isPlayingChanged = this.IsPlayingChanged;
        if (isPlayingChanged == null)
          return;
        isPlayingChanged();
      }
    }

    public VoiceMessagePlayer(Doc doc)
    {
      this._doc = doc;
      this._sourceUriStr = this._doc.preview.audio_msg.link_ogg;
      if (string.IsNullOrEmpty(this._sourceUriStr))
        return;
      this._wavFileName = MediaLRUCache.Instance.GetLocalFile(this._sourceUriStr);
      this._docOwnerId = this._doc.owner_id;
      this._docId = this._doc.id;
      EventAggregator.Current.Subscribe(this);
    }

    public void InitializePlayer()
    {
      DispatcherTimer dispatcherTimer = new DispatcherTimer();
      TimeSpan timeSpan = TimeSpan.FromMilliseconds(10.0);
      dispatcherTimer.Interval = timeSpan;
      this._timerPlayback = dispatcherTimer;
      this._timerPlayback.Tick-=(new EventHandler(this.TimerPlayback_OnTick));
      this._timerPlayback.Tick+=(new EventHandler(this.TimerPlayback_OnTick));
      if (VoiceMessagePlayer.Player.CurrentState != System.Windows.Media.MediaElementState.Playing)
        return;
      this.UpdatePlayerPosition();
    }

    private void TimerPlayback_OnTick(object sender, EventArgs eventArgs)
    {
      this.UpdatePlayerPosition();
    }

    private void UpdatePlayerPosition()
    {
      if (!this.IsCurrentPlayer)
        return;
      if (this.Source ==  null)
        return;
      try
      {
        TimeSpan position = this.Position;
        if (position.TotalMilliseconds <= 0.0)
          return;
        Action<TimeSpan> positionUpdated = this.PositionUpdated;
        if (positionUpdated == null)
          return;
        TimeSpan timeSpan = position;
        positionUpdated(timeSpan);
      }
      catch
      {
      }
    }

    private void BindToPlayer()
    {
      // ISSUE: method pointer
      VoiceMessagePlayer.Player.MediaOpened+=(new RoutedEventHandler( this.OnMediaOpened));
      // ISSUE: method pointer
      VoiceMessagePlayer.Player.MediaEnded+=(new RoutedEventHandler( this.OnMediaEnded));
      VoiceMessagePlayer.Player.MediaFailed+=(new EventHandler<ExceptionRoutedEventArgs>(this.OnMediaFailed));
      ((FrameworkElement) VoiceMessagePlayer.Player).Tag = this;
    }

    private static void UnbindFromPlayer(VoiceMessagePlayer control)
    {
      // ISSUE: method pointer
      VoiceMessagePlayer.Player.MediaOpened-=(new RoutedEventHandler(control.OnMediaOpened));
      // ISSUE: method pointer
      VoiceMessagePlayer.Player.MediaEnded-=(new RoutedEventHandler(control.OnMediaEnded));
      VoiceMessagePlayer.Player.MediaFailed-=(new EventHandler<ExceptionRoutedEventArgs>(control.OnMediaFailed));
      ((FrameworkElement) VoiceMessagePlayer.Player).Tag = null;
    }

    private void OnMediaOpened(object sender, RoutedEventArgs e)
    {
      this.StartPlayback();
    }

    private void OnMediaEnded(object sender, RoutedEventArgs e)
    {
      this._timerPlayback.Stop();
      Action mediaEnded = this.MediaEnded;
      if (mediaEnded == null)
        return;
      mediaEnded();
    }

    private void OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
    }

    public void SetPosition(double position)
    {
      if (!this.CanSeek)
        return;
      this.Position = TimeSpan.FromMilliseconds(position);
    }

    private void ResetPlayer()
    {
      VoiceMessagePlayer.ResetPlayerData();
      this.BindToPlayer();
    }

    public static void ResetPlayerData()
    {
      VoiceMessagePlayer tag = ((FrameworkElement) VoiceMessagePlayer.Player).Tag as VoiceMessagePlayer;
      if (tag == null)
        return;
      tag._timerPlayback.Stop();
      tag.IsPlaying = false;
      Action resetCallback = tag.ResetCallback;
      if (resetCallback != null)
        resetCallback();
      VoiceMessagePlayer.UnbindFromPlayer(tag);
    }

    public void PlayPause()
    {
      this._isDownloaded = MediaLRUCache.Instance.HasLocalFile(this._sourceUriStr);
      if (this._isDownloaded)
        this.DoPlayPause();
      else if (this._isDownloading)
      {
        this._downloadCancellation.Set();
      }
      else
      {
        string str = Guid.NewGuid().ToString();
        this._wavFileName = string.Format("{0}.wav", str);
        if (this._sourceUriStr.IsExternal())
        {
          this._isDownloading = true;
          string fileId = string.Format("{0}.ogg", str);
          this._opusFilePath = CacheManager.GetFullFilePath(fileId, CacheManager.DataType.CachedData);
          this._downloadCancellation = new Cancellation();
          Stream opusStream = CacheManager.GetStreamForWrite(fileId);
          JsonWebRequest.Download(this._sourceUriStr, opusStream, (Action<bool>) (isDownloaded =>
          {
            opusStream.Close();
            Execute.ExecuteOnUIThread((Action) (() =>
            {
              try
              {
                if (!isDownloaded)
                  return;
                this.ConvertOpusToWavAndPlay();
              }
              finally
              {
                this._isDownloading = false;
              }
            }));
          }), (Action<double>) (progress => {}), this._downloadCancellation);
        }
        else
        {
          this._opusFilePath = this._sourceUriStr;
          this.ConvertOpusToWavAndPlay();
        }
      }
    }

    private void ConvertOpusToWavAndPlay()
    {
      MemoryStream asWavStream = OpusExtensions.GetAsWavStream(this._opusFilePath, 48000, 1, 16);
      if (asWavStream == null)
        return;
      int length = (int) asWavStream.Length;
      using (Stream streamForWrite = CacheManager.GetStreamForWrite(this._wavFileName))
      {
        asWavStream.Seek(0, SeekOrigin.Begin);
        asWavStream.CopyTo(streamForWrite);
        streamForWrite.Flush();
      }
      MediaLRUCache.Instance.AddLocalFile(this._sourceUriStr, this._wavFileName, length);
      this.DoPlayPause();
    }

    private void DoPlayPause()
    {
      if (!this.IsCurrentPlayer)
        this.ResetPlayer();
      string wavFileName = this._wavFileName;
      string filePath = CacheManager.GetFilePath(wavFileName, CacheManager.DataType.CachedData, "/");
      if (string.IsNullOrEmpty(filePath))
        return;
      if (!this.IsPlaying)
      {
        if (this.Source ==  null || Path.GetFileName(this.Source.OriginalString) != wavFileName)
        {
          this.Source =  null;
          this._playerSourceFilePath = wavFileName;
          using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
          {
            if (!storeForApplication.FileExists(filePath))
            {
              this.IsPlaying = false;
            }
            else
            {
              using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(filePath, FileMode.Open, FileAccess.Read))
                VoiceMessagePlayer.Player.SetSource((Stream) storageFileStream);
            }
          }
        }
        else
        {
          this._playerSourceFilePath = wavFileName;
          this.StartPlayback();
        }
      }
      else
        this.PausePlayback();
    }

    private void StartPlayback()
    {
      Action playbackStarting = this.PlaybackStarting;
      if (playbackStarting != null)
        playbackStarting();
      try
      {
        VoiceMessagePlayer.PlayerWrapper.CurrentOriginalSource = this._sourceUriStr;
        VoiceMessagePlayer.PlayerWrapper.Play();
        this._timerPlayback.Start();
        if (this._docId == 0L || this._docOwnerId == 0L)
          return;
        EventAggregator.Current.Publish(new AudioMessagePlayEvent(string.Format("{0}_{1}", this._docOwnerId, this._docId)));
      }
      catch
      {
      }
    }

    private void PausePlayback()
    {
      VoiceMessagePlayer.PlayerWrapper.Pause();
      this._timerPlayback.Stop();
    }

    public void Handle(VoiceMessageUploaded message)
    {
      Doc voiceMessageDoc = message.VoiceMessageDoc;
      if (this._doc == null || this._doc.guid == Guid.Empty || this._doc.guid != voiceMessageDoc.guid)
        return;
      DocPreview preview = this._doc.preview;
      string str1;
      if (preview == null)
      {
        str1 =  null;
      }
      else
      {
        DocPreviewVoiceMessage audioMsg = preview.audio_msg;
        str1 = audioMsg != null ? audioMsg.link_ogg :  null;
      }
      string str2 = str1;
      if (string.IsNullOrEmpty(str2))
        return;
      if (VoiceMessagePlayer.PlayerWrapper.CurrentOriginalSource == this._sourceUriStr)
        VoiceMessagePlayer.PlayerWrapper.CurrentOriginalSource = str2;
      this._sourceUriStr = str2;
      int size = (int) this._doc.size;
      if (!string.IsNullOrEmpty(this._wavFileName))
        MediaLRUCache.Instance.AddLocalFile(this._sourceUriStr, this._wavFileName, size);
      this._docOwnerId = voiceMessageDoc.owner_id;
      this._docId = voiceMessageDoc.id;
    }

    public void Handle(MediaPlayerStateChangedEvent message)
    {
      if (!this.IsCurrentPlayer)
        return;
      this.IsPlaying = VoiceMessagePlayer.Player.CurrentState == System.Windows.Media.MediaElementState.Playing;
    }
  }
}
