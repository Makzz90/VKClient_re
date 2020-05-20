using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using XamlAnimatedGif;

namespace VKClient.Common.UC
{
  public class GifViewerUC : UserControl
  {
    private static int _instancesCount;
    private readonly EventHandler<DownloadProgressChangedArgs> _progressChangedHandler;
    private readonly EventHandler _loadedHandler;
    private readonly EventHandler<AnimationErrorEventArgs> _errorHandler;
    private string _url;
    private long _size;
    private bool _isLoaded;
    internal Image imageGif;
    private bool _contentLoaded;

    public Action<double> LoadingProgressChangedHandler { get; set; }

    public Action LoadingCompletedHandler { get; set; }

    public Action LoadingFailedHandler { get; set; }

    public GifViewerUC()
    {
      //base.\u002Ector();
      ++GifViewerUC._instancesCount;
      this.InitializeComponent();
      this._loadedHandler = (EventHandler) ((o, eventArgs) =>
      {
        Action completedHandler = this.LoadingCompletedHandler;
        if (completedHandler == null)
          return;
        completedHandler();
      });
      this._progressChangedHandler = (EventHandler<DownloadProgressChangedArgs>) ((sender, args) =>
      {
        Uri uri = args.Uri;
        if ((uri != null ? uri.ToString() :  null) != this._url)
          return;
        Action<double> progressChangedHandler = this.LoadingProgressChangedHandler;
        if (progressChangedHandler == null)
          return;
        double percentage = args.Percentage;
        progressChangedHandler(percentage);
      });
      this._errorHandler = (EventHandler<AnimationErrorEventArgs>) ((sender, args) =>
      {
        Action loadingFailedHandler = this.LoadingFailedHandler;
        if (loadingFailedHandler == null)
          return;
        loadingFailedHandler();
      });
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.OnLoaded));
      // ISSUE: method pointer
      base.Unloaded += (new RoutedEventHandler( this.OnUnloaded));
    }

    ~GifViewerUC()
    {
      //try
      //{
        --GifViewerUC._instancesCount;
      //}
      //finally
      //{
      //  // ISSUE: explicit finalizer call
      //  // ISSUE: explicit non-virtual call
      //  this.Finalize();
      //}
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      this.HandleUnloaded();
      AnimationBehavior.DownloadProgressChanged += this._progressChangedHandler;
      AnimationBehavior.Loaded += this._loadedHandler;
      AnimationBehavior.Error += this._errorHandler;
      this.SetSourceUri();
      this._isLoaded = true;
    }

    private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
    {
      this.HandleUnloaded();
      this._isLoaded = false;
    }

    private void HandleUnloaded()
    {
      AnimationBehavior.DownloadProgressChanged -= this._progressChangedHandler;
      AnimationBehavior.Loaded -= this._loadedHandler;
      AnimationBehavior.Error -= this._errorHandler;
      AnimationBehavior.CancelLoading();
    }

    public void Init(string url, long size)
    {
      this._url = url;
      this._size = size;
      if (!this._isLoaded)
        return;
      this.SetSourceUri();
    }

    private void SetSourceUri()
    {
      if (!string.IsNullOrEmpty(this._url))
      {
        try
        {
          using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            AnimationBehavior.SetSourceStream((DependencyObject) this.imageGif, (Stream) storeForApplication.OpenFile(this._url, FileMode.Open));
        }
        catch (Exception )
        {
        }
      }
      else
        AnimationBehavior.SetSourceUri(this.imageGif,  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GifViewerUC.xaml", UriKind.Relative));
      this.imageGif = (Image) base.FindName("imageGif");
    }
  }
}
