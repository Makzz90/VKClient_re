using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Audio.Base.Library
{
  public class BatchDownloadManager
  {
    private static Dictionary<string, BatchDownloadManager> _dict = new Dictionary<string, BatchDownloadManager>();
    private List<RemoteLocalMapping> _downloadList;
    private string _id;
    private bool _isInProgress;
    private double _lastProgress;

    private BatchDownloadManager(string id, List<RemoteLocalMapping> downloadList)
    {
      this._id = id;
      this._downloadList = downloadList;
    }

    public static BatchDownloadManager GetDownloadManager(string id, List<RemoteLocalMapping> downloadList)
    {
      if (!BatchDownloadManager._dict.ContainsKey(id))
        BatchDownloadManager._dict[id] = new BatchDownloadManager(id, downloadList);
      return BatchDownloadManager._dict[id];
    }

    public static bool IsDownloaded(string id)
    {
      if (!IsolatedStorageSettings.ApplicationSettings.Contains("DownloadedBatches2"))
        return false;
      return (IsolatedStorageSettings.ApplicationSettings["DownloadedBatches2"] as List<string>).Contains(id);
    }

    public void Start()
    {
      if (this._isInProgress)
        return;
      this._isInProgress = true;
      this.DownloadByIndex(0);
    }

    private void DownloadByIndex(int ind)
    {
      if (ind >= this._downloadList.Count)
      {
        EventAggregator.Current.Publish(new DownloadSucceededEvent()
        {
          Id = this._id
        });
        this.StoreInSettings();
        this._isInProgress = false;
      }
      else
      {
        string remoteUri = this._downloadList[ind].RemoteUri;
        string localPath = this._downloadList[ind].LocalPath;
        if (this.FileExists(localPath))
        {
          EventAggregator.Current.Publish(new DownloadProgressedEvent()
          {
            Id = this._id,
            Progress = ((double) (ind + 1) * 100.0 / (double) this._downloadList.Count)
          });
          this.DownloadByIndex(ind + 1);
        }
        else
        {
          HttpWebRequest request = WebRequest.CreateHttp(remoteUri);
          request.BeginGetResponse((AsyncCallback) (iAR =>
          {
            try
            {
              using (Stream responseStream = request.EndGetResponse(iAR).GetResponseStream())
              {
                using (MemoryStream memoryStream = BatchDownloadManager.ReadFully(responseStream))
                {
                  memoryStream.Position = 0L;
                  using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                  {
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(localPath, FileMode.OpenOrCreate, FileAccess.Write))
                      BatchDownloadManager.CopyStream((Stream) memoryStream, (Stream) storageFileStream);
                  }
                  memoryStream.Position = 0L;
                  ImageCache.Current.TrySetImageForUri(localPath, (Stream) memoryStream);
                }
              }
              this._lastProgress = (double) (ind + 1) * 100.0 / (double) this._downloadList.Count;
              EventAggregator.Current.Publish(new DownloadProgressedEvent()
              {
                Id = this._id,
                Progress = this._lastProgress
              });
              this.DownloadByIndex(ind + 1);
            }
            catch (Exception ex)
            {
              Logger.Instance.Error("Failed to download batch id = " + this._id, ex);
              this._isInProgress = false;
              EventAggregator.Current.Publish(new DownloadFailedEvent()
              {
                Id = this._id
              });
            }
          }), null);
        }
      }
    }

    private void StoreInSettings()
    {
      if (!IsolatedStorageSettings.ApplicationSettings.Contains("DownloadedBatches2"))
        IsolatedStorageSettings.ApplicationSettings["DownloadedBatches2"]= new List<string>();
      List<string> stringList = IsolatedStorageSettings.ApplicationSettings["DownloadedBatches2"] as List<string>;
      if (stringList.Contains(this._id))
        return;
      stringList.Add(this._id);
    }

    private bool FileExists(string localPath)
    {
      return false;
    }

    public static void CopyStream(Stream input, Stream output)
    {
      byte[] buffer = new byte[32768];
      int count;
      while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
        output.Write(buffer, 0, count);
    }

    public static MemoryStream ReadFully(Stream input)
    {
      byte[] buffer = new byte[16384];
      MemoryStream memoryStream = new MemoryStream();
      int count;
      while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
        memoryStream.Write(buffer, 0, count);
      return memoryStream;
    }

    public static bool IsDownloading(string id)
    {
      if (!BatchDownloadManager._dict.ContainsKey(id))
        return false;
      return BatchDownloadManager._dict[id]._isInProgress;
    }

    public static double DownloadProgress(string id)
    {
      if (BatchDownloadManager.IsDownloaded(id))
        return 100.0;
      if (!BatchDownloadManager._dict.ContainsKey(id))
        return 0.0;
      return BatchDownloadManager._dict[id]._lastProgress;
    }
  }
}
