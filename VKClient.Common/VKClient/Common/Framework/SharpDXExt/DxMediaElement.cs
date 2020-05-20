using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Utils;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VKClient.Common.Framework.SharpDXExt
{
    public class DxMediaElement : ContentControl
    {
        public static readonly DependencyProperty VideoPathProperty = DependencyProperty.Register("VideoPath", typeof(string), typeof(DxMediaElement), new PropertyMetadata(new PropertyChangedCallback((o, args) => { ((DxMediaElement)o).OnVideoPathPropertyChanged(); })));
        private readonly object lockObject = new object();
        private readonly DrawingSurface drawingSurface;
        private SharpDXContext context;
        public MediaPlayer mediaPlayer;
        private IRandomAccessStream videoStream;
        private string videoPath;
        private static DxMediaElement activeElement;
        private bool isLoaded;

        public string VideoPath
        {
            get
            {
                return (string)this.GetValue(DxMediaElement.VideoPathProperty);
            }
            set
            {
                this.SetValue(DxMediaElement.VideoPathProperty, value);
            }
        }

        public DxMediaElement()
        {
            DrawingSurface drawingSurface = new DrawingSurface();
            drawingSurface.Visibility = Visibility.Collapsed;
            this.drawingSurface = drawingSurface;
            this.Content = (object)this.drawingSurface;
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            this.VerticalContentAlignment = VerticalAlignment.Stretch;
            this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
            this.Loaded += new RoutedEventHandler(this.OnLoaded);
        }

        private void OnVideoPathPropertyChanged()
        {
            this.videoPath = this.VideoPath;
            this.drawingSurface.Visibility = Visibility.Collapsed;
            this.UpdateActiveElement();
            if (!this.isLoaded)
                return;
            this.TryStartPlaybackAsync();
        }

        private void UpdateActiveElement()
        {
            if (string.IsNullOrEmpty(this.videoPath) || DxMediaElement.activeElement == this)
                return;
            if (DxMediaElement.activeElement != null)
                DxMediaElement.activeElement.VideoPath = null;
            DxMediaElement.activeElement = this;
        }

        private void TryStartPlaybackAsync()// async added
        {
            ThreadPool.QueueUserWorkItem((WaitCallback)(async x =>
            {
                try
                {
                    this.ReleaseResources();
                    if (this.mediaPlayer != null)
                        this.mediaPlayer.StopVideo();
                    if (!string.IsNullOrEmpty(this.videoPath))
                    {
                        DxMediaElement dxMediaElement = this;
                        IRandomAccessStream irandomAccessStream = dxMediaElement.videoStream;
                        IRandomAccessStream videoStream = await this.GetVideoStream();
                        dxMediaElement.videoStream = videoStream;
                        dxMediaElement = null;
                    }
                    if (this.videoStream == null || this.context != null)
                        return;
                    this.InitializeContext();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("DxMediaElement.TryStartPlaybackAsync failed", ex);
                }
            }));
        }

        private async Task<IRandomAccessStream> GetVideoStream()
        {
            try
            {
                return await (await ApplicationData.Current.LocalFolder.GetFileAsync(this.videoPath.Replace('/', '\\'))).OpenAsync(FileAccessMode.Read);
            }
            catch
            {
            }
            return null;
        }

        private void InitializeContext()
        {
            try
            {
                this.context = new SharpDXContext(this.lockObject);
                this.context.Render += new EventHandler(this.ContextOnRender);
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        if (this.context != null && !this.context.IsBound && (this.videoPath != null && this.isLoaded) && this.videoStream != null)
                            this.context.BindToControl(this.drawingSurface);
                        this.drawingSurface.Visibility = Visibility.Visible;
                    }
                    catch
                    {
                    }
                }));
            }
            catch
            {
            }
        }

        private void ContextOnRender(object sender, EventArgs eventArgs)
        {
            lock (this.lockObject)
            {
                if (this.videoStream == null)
                    return;
                if (this.mediaPlayer == null)
                    this.InitializeMediaPlayer();
                if (this.mediaPlayer == null)
                    return;
                try
                {
                    if (this.mediaPlayer.Url != this.videoPath && this.videoPath != null)
                    {
                        this.mediaPlayer.Url = this.videoPath;
                        this.mediaPlayer.SetByteStream(this.videoStream);
                    }
                    this.mediaPlayer.OnRender();
                }
                catch
                {
                }
            }
        }

        private void InitializeMediaPlayer()
        {
            try
            {
                if (this.context == null)
                    return;
                this.mediaPlayer = new MediaPlayer(this.lockObject);
                this.mediaPlayer.Initialize(this.context);
            }
            catch
            {
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.isLoaded = true;
            if (this.VideoPath == null)
                return;
            this.TryStartPlaybackAsync();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.isLoaded = false;
            this.ReleaseResources();
        }

        private void ReleaseResources()
        {
            lock (this.lockObject)
            {
                IRandomAccessStream local_2 = this.videoStream;
                this.videoStream = null;
                if (this.mediaPlayer != null)
                {
                    this.mediaPlayer.Dispose();
                    this.mediaPlayer = null;
                }
                if (this.context != null)
                {
                    this.context.Render -= new EventHandler(this.ContextOnRender);
                    this.context.Dispose();
                    this.context = null;
                }
                if (local_2 == null)
                    return;
                ((IDisposable)local_2).Dispose();
            }
        }

        public void Pause()
        {
            lock (this.lockObject)
            {
                if (this.mediaPlayer == null)
                    return;
                this.mediaPlayer.Pause();
            }
        }

        public void Play()
        {
            lock (this.lockObject)
            {
                if (this.videoPath == null || this.mediaPlayer == null)
                    return;
                this.mediaPlayer.Play();
            }
        }
    }
}
