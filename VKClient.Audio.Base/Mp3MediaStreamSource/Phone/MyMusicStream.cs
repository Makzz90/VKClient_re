using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using VKClient.Audio.Base.MediaStream;

namespace Mp3MediaStreamSource.Phone
{
  public class MyMusicStream : Stream
  {
    public static readonly string TEMP_FOLDER = "Temp";
    public static readonly string CACHED_AUDIO_FOLDER = "CachedAudio";
    public static readonly int DESIRED_CHUNK_SIZE = 300000;
    public static readonly int FIRST_CHUNK_SIZE = 30000;
    private long _position;
    private string _fileId;
    private long _length;
    private Stream _fileStream;
    private FileDescription _fileDescription;
    private IsolatedStorageFile _iso;
    private string _uri;
    private HttpWebResponse _initialResponse;
    private ChunkDownloader _fileDownloader;
    private bool _disposed;

    public override bool CanRead
    {
      get
      {
        return true;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return true;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return false;
      }
    }

    public override long Length
    {
      get
      {
        return this._length;
      }
    }

    public override long Position
    {
      get
      {
        return this._position;
      }
      set
      {
        if (this._position == value)
          return;
        this._position = value;
        this.EnsureCurrentFileStreamIsClosed();
        this.EnsureBufferizationStarted();
      }
    }

    public MyMusicStream(string fileId, string uri, long length, HttpWebResponse response)
    {
      this._fileId = fileId;
      this._uri = uri;
      this._length = length;
      this._iso = IsolatedStorageFile.GetUserStoreForApplication();
      this._initialResponse = response;
      this.EnsureBufferizationStarted();
    }

    public override void Flush()
    {
      throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      if (this.Position >= this.Length - 1L)
        return 0;
      int num1;
      if (this._fileStream != null)
      {
        num1 = this._fileStream.Read(buffer, offset, count);
        this._position = this._position + (long) num1;
        if (num1 < count)
        {
          int num2 = this.WaitInitializeFileStreamAndRead(buffer, offset + num1, count - num1);
          if (num2 > 0)
          {
            num1 += num2;
            this._position = this._position + (long) num2;
          }
          else
          {
            this._fileStream.Position -= (long) num1;
            this._position = this._position - (long) num1;
            num1 = 0;
          }
        }
      }
      else
      {
        num1 = this.WaitInitializeFileStreamAndRead(buffer, offset, count);
        this._position = this._position + (long) num1;
      }
      return num1;
    }

    private int WaitInitializeFileStreamAndRead(byte[] buffer, int offset, int count)
    {
      this.EnsureCurrentFileStreamIsClosed();
      this._fileDescription = this.GetFileDesc(true);
      int num = 0;
      if (this._fileDescription != null)
      {
        this._fileStream = (Stream) this._iso.OpenFile(this._fileDescription.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        this.AlignFileStream();
        num = this._fileStream.Read(buffer, offset, count);
      }
      return num;
    }

    private void EnsureBufferizationStarted()
    {
      long notDownloadedSpot = DownloadedFilesInfo.Instance.GetNextNotDownloadedSpot(this._fileId, this.Position);
      if (notDownloadedSpot < this.Length && (this._fileDownloader == null || this._fileDownloader.DownloadFailed || (notDownloadedSpot < this._fileDownloader.FromByte || notDownloadedSpot - this._fileDownloader.FromByte > (long) (3 * MyMusicStream.DESIRED_CHUNK_SIZE))))
      {
        if (this._fileDownloader != null)
          this._fileDownloader.SetStopFlag();
        this._fileDownloader = new ChunkDownloader(this._fileId, this._uri, notDownloadedSpot);
        this._initialResponse =  null;
        this._fileDownloader.StartDownloading();
      }
      else
      {
        if (this._initialResponse == null)
          return;
        this._initialResponse.Close();
        this._initialResponse =  null;
      }
    }

    private void EnsureCurrentFileStreamIsClosed()
    {
      if (this._fileStream == null)
        return;
      this._fileStream.Close();
      this._fileDescription =  null;
      this._fileStream =  null;
    }

    private void AlignFileStream()
    {
      this._fileStream.Position = this.Position - this._fileDescription.FromByte;
    }

    private FileDescription GetFileDesc(bool wait)
    {
      return DownloadedFilesInfo.Instance.GetFileFor(this._fileId, this.Position, wait ? 15 : 0);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
      throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }

    protected override void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      if (disposing)
      {
        if (this._fileStream != null)
          this._fileStream.Dispose();
        if (this._fileDownloader != null)
          this._fileDownloader.SetStopFlag();
        if (this._iso != null)
          this._iso.Dispose();
      }
      this._fileStream =  null;
      this._iso =  null;
      this._disposed = true;
    }
  }
}
