using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient_Opus;
using Windows.Storage;

namespace VKClient.Common.Library
{
  public class AudioRecorderViewModel : ViewModelBase
  {
    private readonly short[] _recordSamples = new short[1024];
    private string _recordDuration = "0:00";
    private readonly Microphone _microphone;
    private readonly byte[] _buffer;
    private readonly OpusRuntimeComponent _opusComponent;
    private readonly Action<double> _decibelValueChangedCallback;
    private readonly Action _recordingStoppedCallback;
    private readonly XnaAsyncDispatcher _asyncDispatcher;
    private long _samplesCount;
    private DateTime _recordStartTime;
    private DateTime _recordFinishTime;
    private MemoryStream _stream;
    private string _fileName;
    private bool _wasPlayingMusic;
    private const double SILENCE_VALUE = -50.0;
    private List<int> _waveform;
    private bool _isSending;

    private static BGAudioPlayerWrapper PlayerWrapper
    {
      get
      {
        return BGAudioPlayerWrapper.Instance;
      }
    }

    public int RecordDuration { get; private set; }

    public string RecordDurationStr
    {
      get
      {
        return this._recordDuration;
      }
      private set
      {
        this._recordDuration = value;
        this.NotifyPropertyChanged("RecordDurationStr");
      }
    }

    public int RecordDurationSeconds
    {
      get
      {
        return this.RecordDuration / 1000;
      }
    }

    private static PhoneApplicationService CurrentApplicationService
    {
      get
      {
        return PhoneApplicationService.Current;
      }
    }

    public List<int> Waveform
    {
      get
      {
        return this._waveform ?? (this._waveform = WaveformUtils.GetWaveform(this._recordSamples));
      }
    }

    public AudioRecorderViewModel(Microphone microphone, byte[] buffer, OpusRuntimeComponent opusComponent, Action<double> decibelValueChangedCallback, Action recordingStoppedCallback, Action recordingStoppedWithTimeoutCallback)
    {
      AudioRecorderViewModel recorderViewModel = this;
      this._microphone = microphone;
      this._buffer = buffer;
      this._opusComponent = opusComponent;
      this._decibelValueChangedCallback = decibelValueChangedCallback;
      this._recordingStoppedCallback = recordingStoppedCallback;
      this._asyncDispatcher = new XnaAsyncDispatcher((Action) (() =>
      {
        recorderViewModel._recordFinishTime = DateTime.Now;
        TimeSpan timeSpan = recorderViewModel._recordFinishTime - recorderViewModel._recordStartTime;
        recorderViewModel.RecordDurationStr = timeSpan.ToString(timeSpan.Hours > 0 ? "h\\:m\\:ss" : "m\\:ss");
        recorderViewModel.RecordDuration = (int) timeSpan.TotalMilliseconds;
        if (recorderViewModel.RecordDuration < AudioRecorderViewModel.GetMaxDuration())
          return;
        recorderViewModel.StopRecording();
        Action action = recordingStoppedWithTimeoutCallback;
        if (action == null)
          return;
        action();
      }));
    }

    private static int GetMaxDuration()
    {
      return !AppGlobalStateManager.Current.GlobalState.AudioRecordingMaxDemo ? 300000 : 5000;
    }

    public async Task<StorageFile> GetAudioFileAsync()
    {
      try
      {
        return await CacheManager.GetFileAsync(this._fileName);
      }
      catch
      {
      }
      return  null;
    }

    private void Microphone_OnBufferReady(object sender, EventArgs eventArgs)
    {
      int data = this._microphone.GetData(this._buffer);
      int num1 = data / 1920;
      this._stream.Write(this._buffer, 0, this._buffer.Length);
      for (int index = 0; index < num1; ++index)
      {
        int length = 1920 * (index + 1) > this._buffer.Length ? this._buffer.Length - 1920 * index : 1920;
        this._opusComponent.WriteFrame(AudioRecorderViewModel.SubArray<byte>(this._buffer, 1920 * index, length), length);
      }
      List<short> shortList = new List<short>();
      int startIndex1 = 0;
      while (startIndex1 < this._buffer.Length)
      {
        short int16 = BitConverter.ToInt16(this._buffer, startIndex1);
        shortList.Add(int16);
        startIndex1 += 2;
      }
      double num2 = 0.0;
      try
      {
        long num3 = this._samplesCount + (long) (data / 2);
        int num4 = (int) ((double) this._samplesCount / (double) num3 * (double) this._recordSamples.Length);
        int num5 = this._recordSamples.Length - num4;
        if (num4 != 0)
        {
          float num6 = (float) this._recordSamples.Length / (float) num4;
          float num7 = 0.0f;
          for (int index = 0; index < num4; ++index)
          {
            this._recordSamples[index] = this._recordSamples[(int) num7];
            num7 += num6;
          }
        }
        int index1 = num4;
        float num8 = 0.0f;
        float num9 = (float) data / 2f / (float) num5;
        int startIndex2 = 0;
        for (int index2 = 0; index2 < data / 2; ++index2)
        {
          short int16 = BitConverter.ToInt16(this._buffer, startIndex2);
          double num6 = (double) int16 / 32768.0;
          num2 += num6 * num6;
          if (index2 == (int) num8 && index1 < this._recordSamples.Length)
          {
            this._recordSamples[index1] = int16;
            num8 += num9;
            ++index1;
          }
          startIndex2 += 2;
        }
        this._samplesCount = num3;
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("Audio record failure", ex);
      }
      double num10 = Math.Max(0.0, Math.Min(1.0, 1.0 - Math.Abs(AudioRecorderViewModel.ConvertAmplitudeToDb(Math.Sqrt(num2 / ((double) data / 2.0)))) / Math.Abs(-50.0)));
      Action<double> valueChangedCallback = this._decibelValueChangedCallback;
      if (valueChangedCallback == null)
        return;
      double num11 = num10;
      valueChangedCallback(num11);
    }

    private static double ConvertAmplitudeToDb(double amplitude)
    {
      return 20.0 * Math.Log10(amplitude);
    }

    private static T[] SubArray<T>(T[] data, int index, int length)
    {
      T[] objArray = new T[length];
      Array.Copy((Array) data, index, (Array) objArray, 0, length);
      return objArray;
    }

    private async void DeleteTempFile()
    {
      try
      {
        StorageFile fileAsync = await CacheManager.GetFileAsync(this._fileName);
        if (fileAsync == null)
          return;
        await fileAsync.DeleteAsync();
      }
      catch
      {
      }
    }

    public void StartRecording()
    {
        if (AudioRecorderViewModel.PlayerWrapper.PlayerState == Microsoft.Phone.BackgroundAudio.PlayState.Playing)
      {
        this._wasPlayingMusic = true;
        AudioRecorderViewModel.PlayerWrapper.Pause();
      }
      this.RecordDurationStr = "0:00";
      this._fileName = string.Format("{0}.ogg", Guid.NewGuid());
      this._opusComponent.StartRecord(CacheManager.GetFullFilePath(this._fileName, CacheManager.DataType.CachedData));
      this._stream = new MemoryStream();
      this._recordStartTime = DateTime.Now;
      this._recordFinishTime = DateTime.Now;
      this._asyncDispatcher.StartService( null);
      this._microphone.BufferReady += (new EventHandler<EventArgs>(this.Microphone_OnBufferReady));
      this._microphone.Start();
      AudioRecorderViewModel.CurrentApplicationService.UserIdleDetectionMode = ((IdleDetectionMode) 1);
      Logger.Instance.Info("Recording started");
    }

    public void StopRecording()
    {
      this.StopRecordingData();
      Action recordingStoppedCallback = this._recordingStoppedCallback;
      if (recordingStoppedCallback == null)
        return;
      recordingStoppedCallback();
    }

    private void StopRecordingData()
    {
      AudioRecorderViewModel.CurrentApplicationService.UserIdleDetectionMode = ((IdleDetectionMode) 0);
      this._microphone.BufferReady-=(new EventHandler<EventArgs>(this.Microphone_OnBufferReady));
      this._microphone.Stop();
      this._asyncDispatcher.StopService();
      this._opusComponent.StopRecord();
      if (this._wasPlayingMusic)
        AudioRecorderViewModel.PlayerWrapper.Play();
      Logger.Instance.Info("Recording stopped");
    }

    public void Cancel()
    {
      this.StopRecordingData();
      this.DeleteTempFile();
    }

    public async void Send()
    {
      if (this._isSending)
        return;
      if (this._stream == null || this._stream.Length == 0L)
      {
        MessageBox.Show(CommonResources.MicrophoneUnavailable, CommonResources.Error, (MessageBoxButton) 0);
      }
      else
      {
        this._isSending = true;
        try
        {
          StorageFile audioFileAsync = await this.GetAudioFileAsync();
          if (audioFileAsync == null)
            return;
          List<int> waveform = this.Waveform;
          int recordDurationSeconds = this.RecordDurationSeconds;
          EventAggregator.Current.Publish(new VoiceMessageSentEvent(audioFileAsync, recordDurationSeconds, waveform));
        }
        catch (Exception ex)
        {
          Logger.Instance.Error("AudioRecorderViewModel.Send failed", ex);
        }
        finally
        {
          this._isSending = false;
        }
      }
    }
  }
}
