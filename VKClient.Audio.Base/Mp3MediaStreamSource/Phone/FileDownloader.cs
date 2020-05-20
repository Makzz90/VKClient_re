using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;

namespace Mp3MediaStreamSource.Phone
{
  public class FileDownloader
  {
    private byte[] _buffer = new byte[1024];
    private string _fileId;
    private Uri _uri;
    private int _offset;
    private int _maxChunkSize;
    private IReportFileChunkDownloaded _reporter;
    private HttpWebRequest _httpRequest;
    private bool _startedDownloading;
    private string _folderToSave;
    private bool _stopDownloadFlag;
    private long _contentLength;
    private Guid _guid;
    private string _tempFolder;
    private int _cntWroteToFile;
    private int _currentOffset;
    private string _currentPath;
    private Stream _currentChunkFileStream;
    private IsolatedStorageFile _iso;
    private bool _downloadFailed;
    private bool _processedInitialChunk;
    private int _firstChunkSize;
    private HttpWebResponse _initialResponse;
    private DateTime _requestSentTime;
    private DateTime _dtReceivedResponse;
    private HttpWebResponse _response;

    public long ContentLength
    {
      get
      {
        return this._contentLength;
      }
    }

    public int Offset
    {
      get
      {
        return this._offset;
      }
    }

    public bool DownloadFailed
    {
      get
      {
        return this._downloadFailed;
      }
    }

    public FileDownloader(string fileId, string uri, string folderToSave, string tempFolder, int offset, int maxChunkSize, IReportFileChunkDownloaded reporter, long knownContentLength, HttpWebResponse response, int firstChunkSize)
    {
      this._fileId = fileId;
      this._uri = new Uri(uri);
      this._offset = offset;
      this._currentOffset = offset;
      this._maxChunkSize = maxChunkSize;
      this._reporter = reporter;
      this._folderToSave = folderToSave;
      this._tempFolder = tempFolder;
      this._guid = Guid.NewGuid();
      this._contentLength = knownContentLength;
      this._initialResponse = response;
      this._firstChunkSize = firstChunkSize;
    }

    public void SetStopDownloadFlag()
    {
      this._stopDownloadFlag = true;
    }

    public void StartDownloading()
    {
      if (this._startedDownloading)
        return;
      this._startedDownloading = true;
      if (this._initialResponse == null || this._offset > 0)
        this.StartHttpRequest();
      else
        this.ProcessResponse(this._initialResponse);
    }

    private void StartHttpRequest()
    {
      this._httpRequest = WebRequest.CreateHttp(this._uri);
      this._httpRequest.AllowReadStreamBuffering = false;
      if (this._offset > 0 && this._contentLength > (long) this._offset)
        this._httpRequest.Headers["Range"] = "bytes=" + this._offset + "-" + (this._contentLength - 1L);
      if ((long) this._offset >= this._contentLength && this._contentLength > 0L)
        return;
      this._requestSentTime = DateTime.Now;
      this._httpRequest.BeginGetResponse(new AsyncCallback(this.RequestCallback), null);
    }

    private void RequestCallback(IAsyncResult asyncResult)
    {
      try
      {
        this.ProcessResponse((HttpWebResponse) this._httpRequest.EndGetResponse(asyncResult));
      }
      catch (Exception )
      {
        this.CloseStreams( null, true);
      }
    }

    private void ProcessResponse(HttpWebResponse response)
    {
      Stream stream =  null;
      if (this._stopDownloadFlag)
      {
        this._response = response;
        this.CloseStreams( null, true);
      }
      else
      {
        try
        {
          if (this._contentLength == 0L && response.ContentLength > 0L)
          {
            this._contentLength = response.ContentLength;
            if (this._offset > 0)
            {
              response.Close();
              this.StartHttpRequest();
              return;
            }
          }
          this._response = response;
          stream = response.GetResponseStream();
          this._dtReceivedResponse = DateTime.Now;
          stream.BeginRead(this._buffer, 0, this._buffer.Length, new AsyncCallback(this.ReadAsync), stream);
        }
        catch (Exception )
        {
          this._downloadFailed = true;
          this.CloseStreams(stream, true);
        }
      }
    }

    private void ReadAsync(IAsyncResult asyncResult)
    {
      Stream stream =  null;
      try
      {
        stream = asyncResult.AsyncState as Stream;
        int count = stream.EndRead(asyncResult);
        if (!this._stopDownloadFlag)
        {
          if (count > 0)
          {
            if (this._iso == null)
            {
              this._iso = IsolatedStorageFile.GetUserStoreForApplication();
              if (!this._iso.DirectoryExists(this._tempFolder))
                this._iso.CreateDirectory(this._tempFolder);
              if (!this._iso.DirectoryExists(this._folderToSave))
                this._iso.CreateDirectory(this._folderToSave);
            }
            if (string.IsNullOrEmpty(this._currentPath))
              this.ReInitializeFileWrite();
            this._currentChunkFileStream.Write(this._buffer, 0, count);
            this._cntWroteToFile = this._cntWroteToFile + count;
            bool flag = true;
            if (this._cntWroteToFile >= this._maxChunkSize || this._offset == 0 && !this._processedInitialChunk && this._cntWroteToFile >= this._firstChunkSize)
            {
              if (this._offset == 0 && !this._processedInitialChunk)
                this._processedInitialChunk = true;
              this.ReInitializeFileWrite();
              if (this.CopyAndReportWholeFileIfNeeded())
              {
                this.CloseStreams(stream, true);
                flag = false;
              }
            }
            if (!flag)
              return;
            stream.BeginRead(this._buffer, 0, this._buffer.Length, new AsyncCallback(this.ReadAsync), stream);
          }
          else
          {
            this.ReportDownloadedChunk();
            this.CloseStreams(stream, false);
            this.CopyAndReportWholeFileIfNeeded();
            if (this._iso == null)
              return;
            this._iso.Dispose();
          }
        }
        else
          this.CloseStreams(stream, true);
      }
      catch (Exception ex)
      {
        this._downloadFailed = true;
        this.CloseStreams(stream, true);
      }
    }

    private bool CopyAndReportWholeFileIfNeeded()
    {
      List<FileDescription> coverage = DownloadedFilesInfo.Instance.GetCoverage(this._fileId);
      bool flag = false;
      if (coverage.Count > 0 && coverage.Last<FileDescription>().ToByte == this._contentLength - 1L && !coverage.Last<FileDescription>().IsWholeFile)
      {
        string str = this._folderToSave + "\\" + this._fileId;
        try
        {
          if (this._iso.FileExists(str))
            this._iso.DeleteFile(str);
          long byteRangeEnd = 0;
          using (IsolatedStorageFileStream file = this._iso.CreateFile(str))
          {
            foreach (FileDescription fileDescription in coverage)
            {
              using (IsolatedStorageFileStream storageFileStream = this._iso.OpenFile(fileDescription.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
              {
                storageFileStream.Position = byteRangeEnd - fileDescription.FromByte;
                byte[] buffer = new byte[1024];
                int count;
                while ((count = storageFileStream.Read(buffer, 0, 1024)) > 0)
                {
                  file.Write(buffer, 0, count);
                  byteRangeEnd += (long) count;
                }
              }
            }
          }
          this._reporter.ReportDownloadedFileChunk(this._fileId, str, 0, byteRangeEnd, true);
          flag = true;
        }
        catch (Exception )
        {
        }
      }
      return flag;
    }

    private void CloseStreams(Stream stream, bool closeIso = true)
    {
      try
      {
        if (stream != null)
          stream.Close();
        if (this._response != null)
          this._response.Close();
        if (this._currentChunkFileStream != null)
          this._currentChunkFileStream.Close();
        if (!(this._iso != null & closeIso))
          return;
        this._iso.Dispose();
      }
      catch (Exception )
      {
      }
    }

    private void ReInitializeFileWrite()
    {
      if (this._currentChunkFileStream != null)
        this._currentChunkFileStream.Close();
      this.ReportDownloadedChunk();
      this._currentOffset = this._currentOffset + this._cntWroteToFile;
      this._cntWroteToFile = 0;
      this._currentPath = this.GetFileNamePath(this._currentOffset);
      this._currentChunkFileStream = (Stream) this._iso.OpenFile(this._currentPath, FileMode.Create, FileAccess.Write);
    }

    private void ReportDownloadedChunk()
    {
      if (this._cntWroteToFile <= 0)
        return;
      this._reporter.ReportDownloadedFileChunk(this._fileId, this._currentPath, (long) this._currentOffset, (long) (this._currentOffset + this._cntWroteToFile - 1), false);
    }

    private string GetFileNamePath(int offset)
    {
      return this._tempFolder + "\\" + this._fileId + "_" + offset + "_" + this._guid.ToString();
    }

    private string GetWholeFilePath()
    {
      return this._tempFolder + "\\" + this._fileId + "_" + this._guid.ToString();
    }
  }
}
