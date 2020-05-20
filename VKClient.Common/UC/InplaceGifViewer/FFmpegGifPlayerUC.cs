using FFmpegInterop;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.UC.InplaceGifViewer
{
  public class FFmpegGifPlayerUC : UserControl
  {
    private static int _instancesCount;
    private string _url;
    private bool _isInitialized;
    private int _handler;
    private int[] _decoderParams;
    private readonly DispatcherTimer _timer;
    private int _w;
    private int _h;
    private int _pts;
    private bool _isPlaying;
    private WriteableBitmap _wb;
    internal Image imageGif;
    private bool _contentLoaded;

    public string VideoUrl
    {
      get
      {
        return this._url;
      }
      set
      {
        this._url = value;
        this.TryStartPlayback();
      }
    }

    public FFmpegGifPlayerUC()
    {
      //base.\u002Ector();
      ++FFmpegGifPlayerUC._instancesCount;
      this.InitializeComponent();
      DispatcherTimer dispatcherTimer = new DispatcherTimer();
      TimeSpan timeSpan = TimeSpan.FromMilliseconds(100.0);
      dispatcherTimer.Interval = timeSpan;
      this._timer = dispatcherTimer;
      this._timer.Tick+=((EventHandler) ((sender, args) => this.HandleTimerTick()));
      // ISSUE: method pointer
      base.Unloaded += (new RoutedEventHandler( this.OnUnloaded));
    }

    ~FFmpegGifPlayerUC()
    {
      //try
      //{
        --FFmpegGifPlayerUC._instancesCount;
      //}
      //finally
      //{
        // ISSUE: explicit finalizer call
        // ISSUE: explicit non-virtual call
      //  this.Finalize();
      //}
    }

    private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
    {
      this.ReleaseResources();
    }

    private void ReleaseResources()
    {
      this.Stop();
      this.Release();
    }

    private void TryStartPlayback()
    {
      string url = this._url;
      if (string.IsNullOrEmpty(url))
        return;
      this.UpdateFileName(url);
    }

    private async void UpdateFileName(string newFileName)
    {
      this.Stop();
      this.Release();
      if (string.IsNullOrEmpty(newFileName))
        return;
      try
      {
        string path = (await CacheManager.GetFileAsync(newFileName)).Path;
        this._decoderParams = new int[3];
        this._handler = FFmpegGifDecoder.CreateDecoder(path, this._decoderParams);
        if (this._handler <= 0)
          return;
        this._w = this._decoderParams[0];
        this._h = this._decoderParams[1];
        if (this._w <= 0 || this._h <= 0)
          return;
        this._wb = new WriteableBitmap(this._w, this._h);
        this._isInitialized = true;
        this.Start();
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("FFmpegGifPlayerUC.UpdateFileName failed", ex);
      }
    }

    private void Start()
    {
      if (!this._isInitialized)
        return;
      this._timer.Interval=(TimeSpan.FromMilliseconds(this._pts == 0 ? 100.0 : (double) this._pts));
      this._timer.Start();
      this._isPlaying = true;
    }

    private void Stop()
    {
      this._timer.Stop();
      this._isPlaying = false;
    }

    private void Release()
    {
      if (this._handler != 0)
      {
        try
        {
          FFmpegGifDecoder.DestroyDecoder(this._handler);
        }
        catch (Exception ex)
        {
          Logger.Instance.Error("FFmpegGifPlayerUC.Release failed", ex);
        }
        this._handler = 0;
      }
      this.imageGif.Source = ( null);
      this._isPlaying = false;
      this._isInitialized = false;
      this._w = 0;
      this._h = 0;
      this._pts = 0;
    }

    private void HandleTimerTick()
    {
      try
      {
        int[] data = new int[3];
        byte[] videoFrame = FFmpegGifDecoder.GetVideoFrame(this._handler, data);
        if (this._pts == 0 && data[2] != 0)
        {
          this._pts = data[2];
          this._timer.Interval=(TimeSpan.FromMilliseconds((double) this._pts));
        }
        if (!this._isPlaying)
          return;
        for (int index1 = 0; index1 < this._wb.Pixels.Length; ++index1)
        {
          int index2 = index1 << 2;
          this._wb.Pixels[index1] = -16777216 | (int) videoFrame[index2] << 16 | (int) videoFrame[index2 + 1] << 8 | (int) videoFrame[index2 + 2];
        }
        this._wb.Invalidate();
        this.imageGif.Source = ((ImageSource) this._wb);
      }
      catch (Exception ex)
      {
        Logger.Instance.Error("FFmpegGifPlayerUC.HandleTimerTick failed", ex);
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/InplaceGifViewer/FFmpegGifPlayerUC.xaml", UriKind.Relative));
      this.imageGif = (Image) base.FindName("imageGif");
    }
  }
}
