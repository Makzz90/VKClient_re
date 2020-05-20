using MediaParsers;
using Mp3MediaStreamSource.Phone;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Media;

namespace Media
{
  public class Mp3MediaStreamSourceX : MediaStreamSource
  {
    private static byte[] buffer = new byte[4096];
    private const int Id3Version1TagSize = 128;
    private Stream audioStream;
    private MediaStreamDescription audioStreamDescription;
    private long currentFrameStartPosition;
    private long audioStreamLength;
    private TimeSpan trackDuration;
    private MpegFrame _currentFrame;
    private long _offsetFirstFrame;
    private string _fileId;
    private string _uri;
    private bool _inAsync;
    private object _streamLock;
    private bool _isClosed;
    private long _pendingSeekToTime;

    private MpegFrame currentFrame
    {
      get
      {
        return this._currentFrame;
      }
      set
      {
        this._currentFrame = value;
        MpegFrame currentFrame = this._currentFrame;
      }
    }

    public MpegLayer3WaveFormat MpegLayer3WaveFormat { get; private set; }

    public event EventHandler StreamComplete;

    public Mp3MediaStreamSourceX(string fileId, string uri, long length, HttpWebResponse response)
    {
      //this.\u002Ector();
      this._fileId = fileId;
      this._uri = uri;
      this.audioStream = (Stream) new MyMusicStream(fileId, this._uri, length, response);
      this.audioStreamLength = length;
    }

    public void ReadPastId3V2Tags(Action<MpegFrame> callback)
    {
      byte[] numArray = new byte[10];
      if (this.audioStream.Read(numArray, 0, 3) == 3)
      {
        if ((int) numArray[0] == 73 && (int) numArray[1] == 68 && (int) numArray[2] == 51)
        {
          if (this.audioStream.Read(numArray, 3, 7) == 7)
          {
            int id3Size = BitTools.ConvertSyncSafeToInt32(numArray, 6);
            int bytesRead = 0;
            MpegFrame mpegFrame;
            ThreadPool.QueueUserWorkItem((WaitCallback) (state =>
            {
              while (id3Size > 0)
              {
                bytesRead = id3Size - Mp3MediaStreamSourceX.buffer.Length > 0 ? this.audioStream.Read(Mp3MediaStreamSourceX.buffer, 0, Mp3MediaStreamSourceX.buffer.Length) : this.audioStream.Read(Mp3MediaStreamSourceX.buffer, 0, id3Size);
                id3Size -= bytesRead;
              }
              this._offsetFirstFrame = this.audioStream.Position;
              mpegFrame = new MpegFrame(this.audioStream);
              callback(mpegFrame);
            }));
            return;
          }
        }
        else if (this.audioStream.Read(numArray, 3, 1) == 1)
        {
          MpegFrame mpegFrame = new MpegFrame(this.audioStream, numArray);
          callback(mpegFrame);
          return;
        }
      }
      throw new Exception("Could not read intial audio stream data");
    }

    protected override void OpenMediaAsync()
    {
      ThreadPool.QueueUserWorkItem((WaitCallback) (o =>
      {
        Dictionary<MediaSourceAttributesKeys, string> mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
        Dictionary<MediaStreamAttributeKeys, string> mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
        List<MediaStreamDescription> mediaStreamDescriptions = new List<MediaStreamDescription>();
        this.ReadPastId3V2Tags((Action<MpegFrame>) (mpegLayer3Frame => this.ReadPastId3v2TagsCallback(mpegLayer3Frame, mediaStreamAttributes, mediaStreamDescriptions, mediaSourceAttributes)));
      }));
    }

    protected override void GetSampleAsync(MediaStreamType mediaStreamType)
    {
      if (this._isClosed)
        return;
      lock (this._streamLock)
      {
        Dictionary<MediaSampleAttributeKeys, string> dictionary = new Dictionary<MediaSampleAttributeKeys, string>();
        if (this.audioStream.Position < this.audioStream.Length && (!this.HaveEnoughDataInBuffer() || !this.SeekToTimeIfNeeded()))
        {
          this.ReportGetSampleProgress(0.5);
          ThreadPool.QueueUserWorkItem((WaitCallback) (obj =>
          {
            Thread.Sleep(1000);
            this.GetSampleAsync(mediaStreamType);
          }));
        }
        else if (this.currentFrame != null)
        {
          TimeSpan timeSpan = TimeSpan.FromSeconds((double) this.currentFrameStartPosition / (double) this.currentFrame.Bitrate * 8.0);
          this.currentFrame.CopyHeader(Mp3MediaStreamSourceX.buffer);
          int count = this.currentFrame.FrameSize - 4;
          int num = this.audioStream.Read(Mp3MediaStreamSourceX.buffer, 4, count);
          if (num != count)
          {
            this.currentFrame =  null;
            this.ReportGetSampleCompleted(new MediaStreamSample(this.audioStreamDescription,  null, 0, 0, 0, (IDictionary<MediaSampleAttributeKeys, string>) dictionary));
          }
          else
          {
            this.currentFrameStartPosition = this.currentFrameStartPosition + (long) num;
            using (MemoryStream memoryStream = new MemoryStream(Mp3MediaStreamSourceX.buffer))
            {
              this.ReportGetSampleCompleted(new MediaStreamSample(this.audioStreamDescription, (Stream) memoryStream, 0, (long) this.currentFrame.FrameSize, timeSpan.Ticks, (IDictionary<MediaSampleAttributeKeys, string>) dictionary));
              MpegFrame mpegFrame = new MpegFrame(this.audioStream);
              if ((mpegFrame.Version == 1 || mpegFrame.Version == 2) && mpegFrame.Layer == 3)
              {
                this.currentFrameStartPosition = this.currentFrameStartPosition + 4L;
                this.currentFrame = mpegFrame;
              }
              else
                this.currentFrame =  null;
            }
          }
        }
        else
          this.ReportGetSampleCompleted(new MediaStreamSample(this.audioStreamDescription,  null, 0, 0, 0, (IDictionary<MediaSampleAttributeKeys, string>) dictionary));
      }
    }

    private bool HaveEnoughDataInBuffer()
    {
      long position = this.audioStream.Position;
      int num = position < 10000L ? MyMusicStream.FIRST_CHUNK_SIZE / 2 : MyMusicStream.DESIRED_CHUNK_SIZE / 2;
      if (DownloadedFilesInfo.Instance.GetFileFor(this._fileId, position, 0) != null)
        return DownloadedFilesInfo.Instance.GetFileFor(this._fileId, Math.Min(position + (long) num, this.audioStreamLength - 1L), 0) != null;
      return false;
    }

    protected override void CloseMedia()
    {
      this._isClosed = true;
      try
      {
        this.audioStream.Close();
        // ISSUE: reference to a compiler-generated field
        if (this.StreamComplete == null)
          return;
        // ISSUE: reference to a compiler-generated field
        this.StreamComplete(this, EventArgs.Empty);
      }
      catch (CryptographicException )
      {
      }
      catch (Exception )
      {
      }
    }

    protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
    {
      throw new NotImplementedException();
    }

    protected override void SeekAsync(long seekToTime)
    {
      this._pendingSeekToTime = seekToTime;
      this.ReportSeekCompleted(seekToTime);
    }

    private bool SeekToTimeIfNeeded()
    {
      long pendingSeekToTime = this._pendingSeekToTime;
      if (pendingSeekToTime >= 0L && this._currentFrame != null)
      {
        long num1 = pendingSeekToTime / 10000000L * (long) this._currentFrame.Bitrate / 8L + this._offsetFirstFrame;
        long position1 = this.audioStream.Position;
        this.audioStream.Position = num1;
        this.currentFrameStartPosition = this.currentFrameStartPosition + (this.audioStream.Position - position1);
        if (!this.HaveEnoughDataInBuffer())
          return false;
        int num2;
        long position2;
        while (true)
        {
          do
          {
            do
            {
              num2 = this.audioStream.ReadByte();
              this.currentFrameStartPosition = this.currentFrameStartPosition + 1L;
              if (num2 == -1)
                goto label_11;
            }
            while (num2 != (int) byte.MaxValue);
            num2 = this.audioStream.ReadByte();
            this.currentFrameStartPosition = this.currentFrameStartPosition + 1L;
          }
          while ((num2 & 240) != 240);
          position2 = this.audioStream.Position;
          this.audioStream.Position -= 2L;
          MpegFrame mpegFrame = new MpegFrame(this.audioStream);
          if (mpegFrame.Bitrate > 0 && mpegFrame.SamplingRate > 0 && mpegFrame.FrameSize > 0)
          {
            this.audioStream.Position += (long) (mpegFrame.FrameSize - 4);
            if (this.audioStream.ReadByte() == (int) byte.MaxValue && (this.audioStream.ReadByte() & 240) == 240)
              break;
          }
          this.audioStream.Position = position2;
        }
        this.audioStream.Position = position2;
label_11:
        if (num2 != -1)
        {
          this.audioStream.Position = this.audioStream.Position - 2L;
          this.currentFrameStartPosition = this.currentFrameStartPosition - 2L;
          MpegFrame mpegFrame = new MpegFrame(this.audioStream);
          if ((mpegFrame.Version == 1 || mpegFrame.Version == 2) && mpegFrame.Layer == 3)
          {
            this.currentFrameStartPosition = this.currentFrameStartPosition + 4L;
            this.currentFrame = mpegFrame;
          }
          else
            this.currentFrame =  null;
        }
        else
          this.currentFrame =  null;
      }
      this._pendingSeekToTime = -1L;
      return true;
    }

    protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
    {
      throw new NotImplementedException();
    }

    private void ReadPastId3v2TagsCallback(MpegFrame mpegLayer3Frame, Dictionary<MediaStreamAttributeKeys, string> mediaStreamAttributes, List<MediaStreamDescription> mediaStreamDescriptions, Dictionary<MediaSourceAttributesKeys, string> mediaSourceAttributes)
    {
      if (mpegLayer3Frame.FrameSize <= 0)
        throw new InvalidOperationException("MpegFrame's FrameSize cannot be negative");
      WaveFormatExtensible formatExtensible = new WaveFormatExtensible();
      this.MpegLayer3WaveFormat = new MpegLayer3WaveFormat();
      this.MpegLayer3WaveFormat.WaveFormatExtensible = formatExtensible;
      this.MpegLayer3WaveFormat.WaveFormatExtensible.FormatTag = (short) 85;
      this.MpegLayer3WaveFormat.WaveFormatExtensible.Channels = mpegLayer3Frame.Channels == Channel.SingleChannel ? (short) 1 : (short) 2;
      this.MpegLayer3WaveFormat.WaveFormatExtensible.SamplesPerSec = mpegLayer3Frame.SamplingRate;
      this.MpegLayer3WaveFormat.WaveFormatExtensible.AverageBytesPerSecond = mpegLayer3Frame.Bitrate / 8;
      this.MpegLayer3WaveFormat.WaveFormatExtensible.BlockAlign = (short) 1;
      this.MpegLayer3WaveFormat.WaveFormatExtensible.BitsPerSample = (short) 0;
      this.MpegLayer3WaveFormat.WaveFormatExtensible.ExtraDataSize = (short) 12;
      this.MpegLayer3WaveFormat.Id = (short) 1;
      this.MpegLayer3WaveFormat.BitratePaddingMode = 0;
      this.MpegLayer3WaveFormat.FramesPerBlock = (short) 1;
      this.MpegLayer3WaveFormat.BlockSize = (short) mpegLayer3Frame.FrameSize;
      this.MpegLayer3WaveFormat.CodecDelay = (short) 0;
      mediaStreamAttributes[(MediaStreamAttributeKeys) 0] = this.MpegLayer3WaveFormat.ToHexString();
      this.audioStreamDescription = new MediaStreamDescription((MediaStreamType) 0, (IDictionary<MediaStreamAttributeKeys, string>) mediaStreamAttributes);
      mediaStreamDescriptions.Add(this.audioStreamDescription);
      this.trackDuration = new TimeSpan(0, 0, (int) (this.audioStreamLength / (long) this.MpegLayer3WaveFormat.WaveFormatExtensible.AverageBytesPerSecond));
      mediaSourceAttributes[(MediaSourceAttributesKeys) 1] = this.trackDuration.Ticks.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      if (this.audioStream.CanSeek)
        mediaSourceAttributes[(MediaSourceAttributesKeys) 0] = "True";
      else
        mediaSourceAttributes[(MediaSourceAttributesKeys) 0] = "False";
      this.ReportOpenMediaCompleted((IDictionary<MediaSourceAttributesKeys, string>) mediaSourceAttributes, (IEnumerable<MediaStreamDescription>) mediaStreamDescriptions);
      this.currentFrame = mpegLayer3Frame;
      this.currentFrameStartPosition = 4L;
    }
  }
}
