using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Audio.Base.AudioCache;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;

namespace VKClient.Common.UC
{
  public class VoiceMessageAttachmentViewModel : ViewModelBase
  {
    private Doc _doc;
    private DocumentHeader _docHeader;
    private VoiceMessageAttachmentViewModel.State _currentState;
    private readonly string _localFile;
    private Cancellation _currentCancellationToken;
    private double _downloadProgress;

    public VoiceMessageAttachmentViewModel.State CurrentState
    {
      get
      {
        return this._currentState;
      }
      private set
      {
        if (this._currentState == value)
          return;
        this._currentState = value;
        base.NotifyPropertyChanged<VoiceMessageAttachmentViewModel.State>(() => this.CurrentState);
        base.NotifyPropertyChanged<string>(() => this.LocalVideoFile);
      }
    }

    private string VideoUri
    {
      get
      {
        DocPreview preview = this._doc.preview;
        string str;
        if (preview == null)
        {
          str =  null;
        }
        else
        {
          DocPreviewVoiceMessage audioMsg = preview.audio_msg;
          str = audioMsg != null ? audioMsg.link_mp3 :  null;
        }
        return str ?? "";
      }
    }

    public string LocalVideoFile
    {
      get
      {
        if (this.CurrentState != VoiceMessageAttachmentViewModel.State.Playing)
          return "";
        return CacheManager.GetFilePath(this._localFile, CacheManager.DataType.CachedData, "/");
      }
    }

    public double DownloadProgress
    {
      get
      {
        return this._downloadProgress;
      }
      set
      {
        this._downloadProgress = value;
        base.NotifyPropertyChanged<double>(() => this.DownloadProgress);
      }
    }

    public VoiceMessageAttachmentViewModel(Doc doc)
    {
      this._doc = doc;
      this._docHeader = new DocumentHeader(this._doc, 0, false, 0L);
      this._localFile = MediaLRUCache.Instance.GetLocalFile(this.VideoUri);
      this._currentState = MediaLRUCache.Instance.HasLocalFile(this.VideoUri) ? VoiceMessageAttachmentViewModel.State.Downloaded : VoiceMessageAttachmentViewModel.State.NotStarted;
      if (!string.IsNullOrEmpty(this._localFile))
        return;
      this._localFile = Guid.NewGuid().ToString();
    }

    public void Play()
    {
        if (this._currentState == VoiceMessageAttachmentViewModel.State.NotStarted || this._currentState == VoiceMessageAttachmentViewModel.State.Failed)
        {
            this.CurrentState = VoiceMessageAttachmentViewModel.State.Downloading;
            Stream stream = CacheManager.GetStreamForWrite(this._localFile);
            this._currentCancellationToken = new Cancellation();
            JsonWebRequest.Download(this.VideoUri, stream, delegate(bool res)
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    try
                    {
                        if (this._currentCancellationToken.IsSet)
                        {
                            this.CurrentState = VoiceMessageAttachmentViewModel.State.Failed;
                            stream.Close();
                            CacheManager.TryDelete(this._localFile, CacheManager.DataType.CachedData);
                        }
                        else
                        {
                            int fileSize = (int)stream.Length;
                            stream.Close();
                            if (res)
                            {
                                MediaLRUCache.Instance.AddLocalFile(this.VideoUri, this._localFile, fileSize);
                                this.CurrentState = VoiceMessageAttachmentViewModel.State.Downloaded;
                                this.CurrentState = VoiceMessageAttachmentViewModel.State.Playing;
                            }
                            else
                            {
                                this.CurrentState = VoiceMessageAttachmentViewModel.State.Failed;
                                CacheManager.TryDelete(this._localFile, CacheManager.DataType.CachedData);
                            }
                        }
                    }
                    catch
                    {
                        try
                        {
                            stream.Close();
                        }
                        catch
                        {
                        }
                        this.CurrentState = VoiceMessageAttachmentViewModel.State.Failed;
                        CacheManager.TryDelete(this._localFile, CacheManager.DataType.CachedData);
                    }
                });
            }, delegate(double progress)
            {
                this.DownloadProgress = progress;
            }, this._currentCancellationToken);
            return;
        }
        if (this._currentState == VoiceMessageAttachmentViewModel.State.Downloaded)
        {
            this.CurrentState = VoiceMessageAttachmentViewModel.State.Playing;
        }
    }


    public void Pause()
    {
      if (this._currentState != VoiceMessageAttachmentViewModel.State.Playing)
        return;
      this.CurrentState = VoiceMessageAttachmentViewModel.State.Paused;
    }

    public enum State
    {
      NotStarted,
      Downloading,
      Failed,
      Downloaded,
      Playing,
      Paused,
    }
  }
}
