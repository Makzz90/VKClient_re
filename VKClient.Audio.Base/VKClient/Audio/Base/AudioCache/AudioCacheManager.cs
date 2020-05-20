using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;

namespace VKClient.Audio.Base.AudioCache
{
    public class AudioCacheManager : IBinarySerializable, IHandle<AudioPlayerStateChanged>, IHandle
    {
        private readonly int DEFAULT_CHUNK_SIZE = 100000;
        private List<InProgressDownloadInfo> _inProgressDownloads = new List<InProgressDownloadInfo>();
        private Dictionary<string, string> _downloadedDict = new Dictionary<string, string>();
        private List<AudioObj> _downloadedList = new List<AudioObj>();
        private static AudioCacheManager _instance;
        private bool _isDownloading;
        private bool _cacheChanging;

        public static AudioCacheManager Instance
        {
            get
            {
                if (AudioCacheManager._instance == null)
                {
                    AudioCacheManager._instance = new AudioCacheManager();
                    CacheManager.TryDeserialize((IBinarySerializable)AudioCacheManager._instance, "AudioCacheManager", CacheManager.DataType.CachedData);
                }
                return AudioCacheManager._instance;
            }
        }

        public List<AudioObj> CachedListForCurrentUser
        {
            get
            {
                if (!AppGlobalStateManager.Current.GlobalState.IsMusicCachingEnabled)
                    return new List<AudioObj>();
                List<AudioObj> list = this._downloadedList.Where<AudioObj>((Func<AudioObj, bool>)(a => a.owner_id == AppGlobalStateManager.Current.LoggedInUserId)).ToList<AudioObj>();
                list.Reverse();
                return list;
            }
        }

        public bool NetworkStatusInfo { get; set; }

        public AudioCacheManager()
        {
            EventAggregator.Current.Subscribe(this);
        }

        public void Save()
        {
            CacheManager.TrySerialize(this, "AudioCacheManager", false, CacheManager.DataType.CachedData);
        }

        public string GetLocalFileForUniqueId(string uniqueId)
        {
            if (!AppGlobalStateManager.Current.GlobalState.IsMusicCachingEnabled)
                return null;
            if (this._downloadedDict.ContainsKey(uniqueId))
                return this._downloadedDict[uniqueId];
            return null;
        }

        public void EnsureCachingInRunning()
        {
            this.DownloadNextOneIfNeeded();
        }

        public async Task ClearCache(IEnumerable<string> idsForRemoving = null)
        {
            if (this._cacheChanging)
                return;
            this._cacheChanging = true;
            foreach (KeyValuePair<string, string> keyValuePair in new Dictionary<string, string>((IDictionary<string, string>)this._downloadedDict))
            {
                KeyValuePair<string, string> kvp = keyValuePair;
                if (idsForRemoving == null || idsForRemoving.Contains<string>(kvp.Key))
                {
                    if (await CacheManager.TryDeleteAsync(kvp.Value))
                    {
                        this._downloadedDict.Remove(kvp.Key);
                        AudioObj audioObj = this._downloadedList.FirstOrDefault<AudioObj>((Func<AudioObj, bool>)(a => a.UniqueId == kvp.Key));
                        if (audioObj != null)
                            this._downloadedList.Remove(audioObj);
                    }
                }
            }
            //Dictionary<string, string>.Enumerator enumerator = new Dictionary<string, string>.Enumerator();
            this._cacheChanging = false;
        }

        public void AddFileToDownload(List<RemoteFileInfo> remoteFileInfoList)
        {
            if (!AppGlobalStateManager.Current.GlobalState.IsMusicCachingEnabled)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                foreach (RemoteFileInfo remoteFileInfo1 in remoteFileInfoList)
                {
                    RemoteFileInfo remoteFileInfo = remoteFileInfo1;
                    if (!this._downloadedDict.ContainsKey(remoteFileInfo.UniqueId))
                    {
                        InProgressDownloadInfo progressDownloadInfo1 = this._inProgressDownloads.FirstOrDefault<InProgressDownloadInfo>((Func<InProgressDownloadInfo, bool>)(inPr => inPr.RemoteFile.UniqueId == remoteFileInfo.UniqueId));
                        
                        if (progressDownloadInfo1 != null)
                        {
                            progressDownloadInfo1.RemoteFile.UriStr = remoteFileInfo.UriStr;
                            progressDownloadInfo1.LastDownloadResult = DownloadResult.OK;
                        }
                        else
                            this._inProgressDownloads.Add(new InProgressDownloadInfo(remoteFileInfo));
                    }
                }
                this.DownloadNextOneIfNeeded();
            }));
        }

        private void DownloadNextOneIfNeeded()
        {
            if (!AppGlobalStateManager.Current.GlobalState.IsMusicCachingEnabled || VKClient.Common.Library.NetworkStatusInfo.Instance.NetworkStatus == NetworkStatus.MobileRestricted)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this._isDownloading)
                    return;
                Logger.Instance.Info("AudioCacheManager.DownloadNextOneIfNeeded starting: InProgressDownload.Count == " + this._inProgressDownloads.Count);
                InProgressDownloadInfo currentDownload = this._inProgressDownloads.LastOrDefault<InProgressDownloadInfo>((Func<InProgressDownloadInfo, bool>)(ipd => ipd.LastDownloadResult != DownloadResult.UriNoLongerValid));
                if (currentDownload == null)
                {
                    Logger.Instance.Info("AudioCacheManager.DownloadNextOneIfNeeded: no files to download");
                }
                else
                {
                    Logger.Instance.Info("AudioCacheManager.DownloadNextOneIfNeeded: found download " + currentDownload.ToString());
                    this._isDownloading = true;
                    this.PerformChunkDownload(currentDownload, (Action)(() =>
                    {
                        DownloadResult lastDownloadResult = currentDownload.LastDownloadResult;
                        Logger.Instance.Info("AudioCacheManager.DownloadNextOneIfNeeded: download completed with result " + lastDownloadResult);
                        if (lastDownloadResult != DownloadResult.OK)
                            Execute.ExecuteOnUIThread((Action)(() =>
                            {
                                this._inProgressDownloads.Remove(currentDownload);
                                this._inProgressDownloads.Insert(0, currentDownload);
                            }));
                        else if (currentDownload.DownloadedToByte == currentDownload.Length - 1L)
                        {
                            Logger.Instance.Info("AudioCacheManager.DownloadNextOneIfNeeded: downloaded the full file!");
                            this.ProcessFullyDownloadedFile(currentDownload);
                            Execute.ExecuteOnUIThread((Action)(() => this._inProgressDownloads.Remove(currentDownload)));
                        }
                        //
                        float temp = (float)currentDownload.DownloadedToByte / (float)currentDownload.Length;

                        EventAggregator.Current.Publish(new AudioTrackDownloadProgress
                        {
                            Id = currentDownload.RemoteFile.UniqueId,
                            Progress = (temp * 100f)
                        });
                        //
                        this._isDownloading = false;
                        this.DownloadNextOneIfNeeded();
                    }));
                }
            }));
        }

        private void ProcessFullyDownloadedFile(InProgressDownloadInfo currentDownload)
        {
            List<string> ids = new List<string>();
            ids.Add(currentDownload.RemoteFile.UniqueId);
            Action<BackendResult<List<AudioObj>, ResultCode>> callback = (Action<BackendResult<List<AudioObj>, ResultCode>>)(res =>
            {
                if (res.ResultCode != ResultCode.Succeeded)
                    return;
                this._downloadedDict[currentDownload.RemoteFile.UniqueId] = currentDownload.LocalFileId;
                AudioObj audioObj = res.ResultData[0];
                audioObj.url = currentDownload.LocalFileId;
                this._downloadedList.Add(audioObj);
            });
            AudioService.Instance.GetAudio(ids, callback);
        }

        private void PerformChunkDownload(InProgressDownloadInfo di, Action resultCallback)
        {
            JsonWebRequest.Download(di.RemoteFile.UriStr, di.DownloadedToByte + 1L, di.DownloadedToByte + (long)this.DEFAULT_CHUNK_SIZE, (Action<HttpStatusCode, long, byte[]>)((statusCode, length, dataBytes) =>
            {
                if (statusCode == HttpStatusCode.NotFound)
                    di.LastDownloadResult = DownloadResult.UriNoLongerValid;
                else if (length > 0L && dataBytes != null)
                {
                    CacheManager.TrySaveRawCachedData(dataBytes, di.LocalFileId, FileMode.Append);
                    di.DownloadedToByte += (long)dataBytes.Length;
                    di.Length = length;
                    di.LastDownloadResult = DownloadResult.OK;
                }
                else
                    di.LastDownloadResult = DownloadResult.Failed;
                resultCallback();
            }));
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.WriteList<InProgressDownloadInfo>(this._inProgressDownloads, 10000);
            writer.WriteList<AudioObj>(this._downloadedList, 10000);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this._inProgressDownloads = reader.ReadList<InProgressDownloadInfo>();
            this._downloadedList = reader.ReadList<AudioObj>();
            this._downloadedDict = new Dictionary<string, string>();
            foreach (AudioObj downloaded in this._downloadedList)
                this._downloadedDict[downloaded.UniqueId] = downloaded.url;
        }

        public void Handle(AudioPlayerStateChanged message)
        {
            PlayedFilesInfo playedFilesInfo = new PlayedFilesInfo();
            playedFilesInfo.Restore();
            this.AddFileToDownload(playedFilesInfo.PlayedFiles);
        }
    }
}
