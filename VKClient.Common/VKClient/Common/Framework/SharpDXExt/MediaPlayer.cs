using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using System;
using Windows.Storage.Streams;

namespace VKClient.Common.Framework.SharpDXExt
{
    public class MediaPlayer : IDisposable
    {
        private DXGIDeviceManager dxgiDeviceManager;
        private MediaEngine mediaEngine;
        private MediaEngineEx mediaEngineEx;
        private bool isEndOfStream;
        private bool isVideoStopped;
        private readonly object lockObject;
        private ByteStream byteStream;
        private SharpDXContext sharpDxContext;
        private bool canPlay;
        private DeviceMultithread multithread;
        private MediaEngineAttributes attributes;

        public bool IsPlaying { get; private set; }

        public Color BackgroundColor { get; set; }

        public string Url { get; set; }

        public double Volume
        {
            get
            {
                if (this.mediaEngineEx != null)
                    return this.mediaEngineEx.Volume;
                return 0.0;
            }
            set
            {
                if (this.mediaEngineEx == null)
                    return;
                this.mediaEngineEx.Volume = value;
            }
        }

        public double Balance
        {
            get
            {
                if (this.mediaEngineEx != null)
                    return this.mediaEngineEx.Balance;
                return 0.0;
            }
            set
            {
                if (this.mediaEngineEx == null)
                    return;
                this.mediaEngineEx.Balance = value;
            }
        }

        public bool Mute
        {
            get
            {
                if (this.mediaEngineEx != null)
                    return (bool)this.mediaEngineEx.Muted;
                return false;
            }
            set
            {
                if (this.mediaEngineEx == null)
                    return;
                this.mediaEngineEx.Muted = (Bool)value;
            }
        }

        public double Duration
        {
            get
            {
                double d = 0.0;
                if (this.mediaEngineEx != null)
                {
                    d = this.mediaEngineEx.Duration;
                    if (double.IsNaN(d))
                        d = 0.0;
                }
                return d;
            }
        }

        public bool AutoPlay
        {
            get
            {
                if (this.mediaEngine != null)
                    return (bool)this.mediaEngine.AutoPlay;
                return false;
            }
            set
            {
                if (this.mediaEngine == null)
                    return;
                this.mediaEngine.AutoPlay = (Bool)value;
            }
        }

        public bool CanSeek
        {
            get
            {
                if (this.mediaEngineEx != null && (this.mediaEngineEx.ResourceCharacteristics & ResourceCharacteristics.CanSeek) != ResourceCharacteristics.None)
                    return this.Duration != 0.0;
                return false;
            }
        }

        public double PlaybackPosition
        {
            get
            {
                if (this.mediaEngineEx != null)
                    return this.mediaEngineEx.CurrentTime;
                return 0.0;
            }
            set
            {
                if (this.mediaEngineEx == null)
                    return;
                this.mediaEngineEx.CurrentTime = value;
            }
        }

        public bool IsSeeking
        {
            get
            {
                if (this.mediaEngineEx != null)
                    return (bool)this.mediaEngineEx.IsSeeking;
                return false;
            }
        }

        public MediaPlayer(object lockObject)
        {
            this.lockObject = lockObject;
            this.BackgroundColor = Color.Transparent;
            this.isVideoStopped = true;
        }

        public void Initialize(SharpDXContext context)
        {
            lock (this.lockObject)
            {
                this.sharpDxContext = context;
                MediaManager.Startup(false);
                this.multithread = this.sharpDxContext.D3DContext.QueryInterface<DeviceMultithread>();
                this.multithread.SetMultithreadProtected((Bool)true);
                this.dxgiDeviceManager = new DXGIDeviceManager();
                this.dxgiDeviceManager.ResetDevice((ComObject)this.sharpDxContext.D3DDevice);
                this.attributes = new MediaEngineAttributes(0)
                {
                    DxgiManager = this.dxgiDeviceManager,
                    VideoOutputFormat = 87
                };
                using (MediaEngineClassFactory resource_0 = new MediaEngineClassFactory())
                    this.mediaEngine = new MediaEngine(resource_0, this.attributes, MediaEngineCreateFlags.None, new MediaEngineNotifyDelegate(this.OnMediaEngineEvent));
                this.mediaEngineEx = this.mediaEngine.QueryInterface<MediaEngineEx>();
                this.mediaEngine.Loop = (Bool)true;
                this.mediaEngine.AutoPlay = (Bool)true;
            }
        }

        public void OnRender()
        {
            lock (this.lockObject)
            {
                long local_2;
                if (this.isVideoStopped || this.mediaEngineEx == null || (this.sharpDxContext.BackBuffer == null || !this.mediaEngineEx.OnVideoStreamTick(out local_2)))
                    return;
                Texture2D local_3 = this.sharpDxContext.BackBuffer;
                if (local_3 == null)
                    return;
                Texture2DDescription local_4 = local_3.Description;
                Rectangle local_5 = new Rectangle(0, 0, local_4.Width, local_4.Height);
                this.mediaEngineEx.TransferVideoFrame((ComObject)local_3, new VideoNormalizedRect?(), local_5, new ColorBGRA?((ColorBGRA)this.BackgroundColor));
            }
        }

        public void SetByteStream(IRandomAccessStream streamHandle)
        {
            lock (this.lockObject)
            {
                this.canPlay = false;
                this.byteStream = new ByteStream(streamHandle);
                this.mediaEngineEx.SetSourceFromByteStream(this.byteStream, this.Url);
            }
        }

        public void Play()
        {
            if (this.mediaEngineEx == null)
                return;
            if (this.canPlay && (bool)this.mediaEngineEx.HasVideo() && this.isVideoStopped)
                this.isVideoStopped = false;
            if (this.isEndOfStream)
            {
                this.PlaybackPosition = 0.0;
                this.IsPlaying = true;
            }
            else
                this.mediaEngineEx.Play();
            this.isEndOfStream = false;
        }

        public void Pause()
        {
            if (this.mediaEngineEx == null)
                return;
            this.mediaEngineEx.Pause();
        }

        public void FrameStep(bool forward)
        {
            if (this.mediaEngineEx == null)
                return;
            this.mediaEngineEx.FrameStep((Bool)forward);
        }

        public void StopVideo()
        {
            lock (this.lockObject)
            {
                this.isVideoStopped = true;
                this.IsPlaying = false;
            }
        }

        protected void OnMediaEngineEvent(MediaEngineEvent mediaEvent, long param1, int param2)
        {
            switch (mediaEvent)
            {
                case MediaEngineEvent.Ended:
                    if ((bool)this.mediaEngineEx.HasVideo())
                        this.StopVideo();
                    this.isEndOfStream = true;
                    break;
                case MediaEngineEvent.Play:
                    this.IsPlaying = true;
                    break;
                case MediaEngineEvent.Pause:
                    this.IsPlaying = false;
                    break;
                case MediaEngineEvent.LoadedMetadata:
                    this.isEndOfStream = false;
                    break;
                case MediaEngineEvent.CanPlay:
                    this.canPlay = true;
                    this.Play();
                    break;
            }
        }

        public void Dispose()
        {
            lock (this.lockObject)
            {
                this.StopVideo();
                this.mediaEngineEx.Shutdown();
                Utilities.Dispose<MediaEngineEx>(ref this.mediaEngineEx);
                Utilities.Dispose<MediaEngine>(ref this.mediaEngine);
                Utilities.Dispose<DXGIDeviceManager>(ref this.dxgiDeviceManager);
                Utilities.Dispose<ByteStream>(ref this.byteStream);
                Utilities.Dispose<DeviceMultithread>(ref this.multithread);
                Utilities.Dispose<MediaEngineAttributes>(ref this.attributes);
            }
        }
    }
}
