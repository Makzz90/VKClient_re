using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.AudioCache;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Utils;

namespace VKClient.Common.UC.InplaceGifViewer
{
  public class InplaceGifViewerViewModel : ViewModelBase, IHandle<GifStateChanged>, IHandle
  {
    private Doc _doc;
    private InplaceGifViewerViewModel.State _state;
    private string _localFile;
    private Cancellation _currentCancellationToken;
    private bool _isFullScreen;
    private bool _forceDisableAutoplay;
    private bool _isOneItem;
    private double _downloadProgress;
    private bool _autoplayStatsSent;

    public Visibility DownloadingProgressVisibility
    {
      get
      {
        if (!this.ShowDownloadingRing)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public bool ShowDownloadingRing
    {
      get
      {
        return this.CurrentState == InplaceGifViewerViewModel.State.Downloading;
      }
    }

    public DocumentHeader DocHeader { get;set; }

    public string Text
    {
      get
      {
        if (this._isFullScreen)
          return "";
        string sizeString = this.DocHeader.GetSizeString();
        bool flag = GifsPlayerUtils.ShouldShowSize();
        if (this.PlayIconVisibility == Visibility.Visible)
        {
          if (!flag)
            return "";
          return sizeString;
        }
        if (this._isOneItem)
          return "";
        if (flag)
          return this.DocHeader.Description;
        return "GIF";
      }
    }

    public Visibility HasTextVisibility
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this.Text))
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public Visibility PlayIconVisibility
    {
      get
      {
        if (!this._isFullScreen && !this._isOneItem || (this.CurrentState == InplaceGifViewerViewModel.State.Playing || this.CurrentState == InplaceGifViewerViewModel.State.Downloading))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public InplaceGifViewerViewModel.State CurrentState
    {
      get
      {
        return this._state;
      }
      private set
      {
        if (this._state == value)
          return;
        this._state = value;
        this.NotifyPropertyChanged<InplaceGifViewerViewModel.State>((System.Linq.Expressions.Expression<Func<InplaceGifViewerViewModel.State>>) (() => this.CurrentState));
        EventAggregator.Current.Publish(new GifStateChanged()
        {
          ID = this._doc.id,
          NewState = this._state,
          VMHashcode = this.GetHashCode()
        });
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.LocalVideoFile));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.PlayIconVisibility));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Text));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.HasTextVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.DownloadingProgressVisibility));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.ShowDownloadingRing));
      }
    }

    private string VideoUri
    {
      get
      {
        if (this.UseOldGifPlayer)
          return this._doc.url ?? "";
        DocPreview preview = this._doc.preview;
        string str;
        if (preview == null)
        {
          str =  null;
        }
        else
        {
          DocPreviewVideo video = preview.video;
          str = video != null ? video.src :  null;
        }
        return str ?? this._doc.url ?? "";
      }
    }

    public string LocalVideoFilePath
    {
      get
      {
        if (this.CurrentState != InplaceGifViewerViewModel.State.Playing)
          return "";
        return CacheManager.GetFilePath(this._localFile, CacheManager.DataType.CachedData, "/");
      }
    }

    public string LocalVideoFile
    {
      get
      {
        if (this.CurrentState != InplaceGifViewerViewModel.State.Playing)
          return "";
        return this._localFile;
      }
    }

    public bool UseOldGifPlayer
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
          DocPreviewVideo video = preview.video;
          str = video != null ? video.src :  null;
        }
        return string.IsNullOrWhiteSpace(str);
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
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.DownloadProgress));
      }
    }

    public InplaceGifViewerViewModel(Doc doc, bool isFullScreen, bool forceDisableAutoplay = false, bool isOneItem = false)
    {
      EventAggregator.Current.Subscribe(this);
      this._forceDisableAutoplay = forceDisableAutoplay;
      this._isOneItem = isOneItem;
      this._doc = doc;
      this.DocHeader = new DocumentHeader(this._doc, 0, false, 0L);
      this._isFullScreen = isFullScreen;
      this._localFile = MediaLRUCache.Instance.GetLocalFile(this.VideoUri);
      this._state = MediaLRUCache.Instance.HasLocalFile(this.VideoUri) ? InplaceGifViewerViewModel.State.Downloaded : InplaceGifViewerViewModel.State.NotStarted;
      if (!string.IsNullOrEmpty(this._localFile))
        return;
      this._localFile = Guid.NewGuid().ToString();
    }

    public bool AllowAutoplay()
    {
      if (!AppGlobalStateManager.Current.GlobalState.GifAutoplayFeatureAvailable || this._forceDisableAutoplay || !this._isOneItem)
        return false;
      return GifsPlayerUtils.AllowAutoplay();
    }

    internal void HandleOnScreen()
    {
      if (!this.AllowAutoplay())
        return;
      this.Play(GifPlayStartType.autoplay);
    }

    public void Play(GifPlayStartType startType = GifPlayStartType.manual)
    {
      this.Play(startType, false);
    }

    private void Play(GifPlayStartType startType, bool suppressStats)
    {
        if (this._state == InplaceGifViewerViewModel.State.NotStarted || this._state == InplaceGifViewerViewModel.State.Failed)
        {
            this.CurrentState = InplaceGifViewerViewModel.State.Downloading;
            Stream stream = CacheManager.GetStreamForWrite(this._localFile);
            this._currentCancellationToken = new Cancellation();
            JsonWebRequest.Download(this.VideoUri, stream, (Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                try
                {
                    if (this._currentCancellationToken.IsSet)
                    {
                        this.CurrentState = InplaceGifViewerViewModel.State.Failed;
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
                            this.CurrentState = InplaceGifViewerViewModel.State.Downloaded;
                            this.CurrentState = InplaceGifViewerViewModel.State.Playing;
                            this.SendGifPlayStats(startType, suppressStats);
                        }
                        else
                        {
                            this.CurrentState = InplaceGifViewerViewModel.State.Failed;
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
                    this.CurrentState = InplaceGifViewerViewModel.State.Failed;
                    CacheManager.TryDelete(this._localFile, CacheManager.DataType.CachedData);
                }
            }))), (Action<double>)(progress => this.DownloadProgress = progress), this._currentCancellationToken);
        }
        else
        {
            if (this._state != InplaceGifViewerViewModel.State.Downloaded)
                return;
            this.CurrentState = InplaceGifViewerViewModel.State.Playing;
            this.SendGifPlayStats(startType, suppressStats);
        }
    }

    public void Stop()
    {
      if (this._state == InplaceGifViewerViewModel.State.Playing)
      {
        this.CurrentState = InplaceGifViewerViewModel.State.Downloaded;
      }
      else
      {
        if (this._state != InplaceGifViewerViewModel.State.Downloading)
          return;
        this.CancelCurrentCancellationToken();
      }
    }

    private void SendGifPlayStats(GifPlayStartType startType, bool suppressStats)
    {
      if (this._doc == null | suppressStats)
        return;
      if (startType == GifPlayStartType.autoplay)
      {
        if (this._autoplayStatsSent)
          return;
        this._autoplayStatsSent = true;
      }
      EventAggregator.Current.Publish(new GifPlayEvent(string.Format("{0}_{1}", this._doc.owner_id, this._doc.id), startType, CurrentMediaSource.GifPlaySource));
    }

    internal void HandleTap()
    {
      if (this.CurrentState != InplaceGifViewerViewModel.State.Playing)
        this.Play(GifPlayStartType.manual, true);
      else
        this.Stop();
    }

    public void ReleaseResorces()
    {
      this.Stop();
    }

    public void Handle(GifStateChanged message)
    {
      if (this.GetHashCode() == message.VMHashcode || message.NewState != InplaceGifViewerViewModel.State.Playing && message.NewState != InplaceGifViewerViewModel.State.Downloading)
        return;
      if (this.CurrentState == InplaceGifViewerViewModel.State.Playing)
      {
        this.CurrentState = InplaceGifViewerViewModel.State.Downloaded;
      }
      else
      {
        if (this.CurrentState != InplaceGifViewerViewModel.State.Downloading)
          return;
        this.CancelCurrentCancellationToken();
      }
    }

    private void CancelCurrentCancellationToken()
    {
      Cancellation cancellationToken = this._currentCancellationToken;
      if (cancellationToken == null)
        return;
      cancellationToken.Set();
    }

    public enum State
    {
      NotStarted,
      Downloading,
      Failed,
      Downloaded,
      Playing,
    }
  }
}
