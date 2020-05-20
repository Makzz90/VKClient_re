using Mp3MediaStreamSource.Phone;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;

namespace VKClient.Audio.Base.MediaStream
{
  public class ChunkDownloader
  {
    private string _tempFolder = "TempAudio";
    private string _cacheFolder = "CacheAudio";
    private readonly int DEFAULT_CHUNK_SIZE = 100000;
    private string _fileId;
    private string _uri;
    private long _fromByte;
    private long _toByte;
    private bool _startedDownloading;
    private HttpWebRequest _request;
    private long _contentLength;
    private Guid _guid;
    private bool _stopFlag;
    private ChunkDownloader _nextChunkDownloader;
    private bool _downloadFailed;

    public bool DownloadFailed
    {
      get
      {
        if (this._downloadFailed)
          return true;
        if (this._nextChunkDownloader != null)
          return this._nextChunkDownloader.DownloadFailed;
        return false;
      }
    }

    public long FromByte
    {
      get
      {
        return this._fromByte;
      }
    }

    public ChunkDownloader(string fileId, string uri, long fromByte)
    {
      this._fileId = fileId;
      this._uri = uri;
      this._fromByte = fromByte;
      this._guid = Guid.NewGuid();
    }

    public void StartDownloading()
    {
      if (this._startedDownloading)
        return;
      this._startedDownloading = true;
      this._request = WebRequest.CreateHttp(this._uri);
      this._request.Headers["range"] = "bytes=" + this._fromByte + "-" + (this._fromByte + (long) this.DEFAULT_CHUNK_SIZE - 1L).ToString();
      this._request.BeginGetResponse(new AsyncCallback(this.RequestCallback), null);
    }

    public void SetStopFlag()
    {
      this._stopFlag = true;
      if (this._nextChunkDownloader == null)
        return;
      this._nextChunkDownloader.SetStopFlag();
    }

    private void RequestCallback(IAsyncResult asyncResult)
    {
      try
      {
        HttpWebResponse response = (HttpWebResponse) this._request.EndGetResponse(asyncResult);
        this._contentLength = ChunkDownloader.ReadContentLengthFromHeaderValue(response.Headers["content-range"]);
        this._toByte = Math.Min(this._contentLength - 1L, this._fromByte + (long) this.DEFAULT_CHUNK_SIZE - 1L);
        this.ProcessResponse(response);
      }
      catch (Exception )
      {
        this._downloadFailed = true;
      }
    }

    private void ProcessResponse(HttpWebResponse response)
    {
      try
      {
        using (Stream responseStream = ((WebResponse) response).GetResponseStream())
        {
          using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
          {
            this.EnsureFoldersExist(storeForApplication);
            string fileNamePath = this.GetFileNamePath(this._fromByte);
            using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(fileNamePath, FileMode.Create, FileAccess.Write))
              this.ReadWriteFromStreamToStream(responseStream, (Stream) storageFileStream);
            this.ReportDownloadedChunk(fileNamePath);
            if (this._stopFlag || this.CopyAndReportWholeFileIfNeeded() || this._toByte >= this._contentLength - 1L)
              return;
            this._nextChunkDownloader = new ChunkDownloader(this._fileId, this._uri, this._toByte + 1L);
            this._nextChunkDownloader.StartDownloading();
          }
        }
      }
      catch (Exception )
      {
        this._downloadFailed = true;
      }
      finally
      {
        response.Close();
      }
    }

    private long ReadWriteFromStreamToStream(Stream input, Stream output)
    {
      byte[] buffer = new byte[1024];
      long num = 0;
      int count;
      while ((count = input.Read(buffer, 0, 1024)) > 0)
      {
        output.Write(buffer, 0, count);
        num += (long) count;
      }
      return num;
    }

    private void EnsureFoldersExist(IsolatedStorageFile iso)
    {
      if (!iso.DirectoryExists(this._tempFolder))
        iso.CreateDirectory(this._tempFolder);
      if (iso.DirectoryExists(this._cacheFolder))
        return;
      iso.CreateDirectory(this._cacheFolder);
    }

    public static long ReadContentLengthFromHeaderValue(string headerContentRange)
    {
      return long.Parse(headerContentRange.Split('/')[1]);
    }

    private string GetFileNamePath(long offset)
    {
      return this._tempFolder + "\\" + this._fileId + "_" + offset + "_" + this._guid.ToString();
    }

    private void ReportDownloadedChunk(string currentPath)
    {
      DownloadedFilesInfo.Instance.ReportDownloadedFileChunk(this._fileId, currentPath, this._fromByte, this._toByte, false);
    }

    private bool CopyAndReportWholeFileIfNeeded()
    {
      List<FileDescription> coverage = DownloadedFilesInfo.Instance.GetCoverage(this._fileId);
      bool flag = false;
      if (coverage.Count > 0 && coverage.Last<FileDescription>().ToByte == this._contentLength - 1L && !coverage.Last<FileDescription>().IsWholeFile)
      {
        string str = this._cacheFolder + "\\" + this._fileId;
        long num = 0;
        try
        {
          using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
          {
            if (storeForApplication.FileExists(str))
              storeForApplication.DeleteFile(str);
            using (IsolatedStorageFileStream file = storeForApplication.CreateFile(str))
            {
              foreach (FileDescription fileDescription in coverage)
              {
                using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(fileDescription.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                  storageFileStream.Position = num - fileDescription.FromByte;
                  num += this.ReadWriteFromStreamToStream((Stream) storageFileStream, (Stream) file);
                }
              }
            }
          }
          DownloadedFilesInfo.Instance.ReportDownloadedFileChunk(this._fileId, str, 0, num - 1L, true);
          flag = true;
        }
        catch (Exception ex)
        {
        }
      }
      return flag;
    }
  }
}
