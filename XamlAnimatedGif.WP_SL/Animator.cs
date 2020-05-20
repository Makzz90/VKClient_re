using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using XamlAnimatedGif.Decoding;
using XamlAnimatedGif.Decompression;
using XamlAnimatedGif.Extensions;

namespace XamlAnimatedGif
{
    public class Animator : DependencyObject, IDisposable
    {
        private readonly Stream _sourceStream;
        private readonly Uri _sourceUri;
        private readonly GifDataStream _metadata;
        private readonly Image _image;
        private readonly Dictionary<int, Animator.GifPalette> _palettes;
        private readonly WriteableBitmap _bitmap;
        private readonly int _stride;
        private readonly byte[] _previousBackBuffer;
        private readonly byte[] _indexStreamBuffer;
        private readonly TimingManager _timingManager;
        private bool _isStarted;
        private CancellationTokenSource _cancellationTokenSource;
        private int _frameIndex;
        private int _previousFrameIndex;
        private GifFrame _previousFrame;
        private volatile bool _disposing;
        private bool _disposed;

        public int FrameCount
        {
            get
            {
                return this._metadata.Frames.Count;
            }
        }

        public bool IsPaused
        {
            get
            {
                return this._timingManager.IsPaused;
            }
        }

        public bool IsComplete
        {
            get
            {
                if (this._isStarted)
                    return this._timingManager.IsComplete;
                return false;
            }
        }

        public int CurrentFrameIndex
        {
            get
            {
                return this._frameIndex;
            }
            internal set
            {
                this._frameIndex = value;
                this.OnCurrentFrameChanged();
            }
        }

        internal BitmapSource Bitmap
        {
            get
            {
                return (BitmapSource)this._bitmap;
            }
        }

        public static event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;

        public event EventHandler CurrentFrameChanged;

        public event EventHandler AnimationCompleted;

        private Animator(Stream sourceStream, Uri sourceUri, GifDataStream metadata, RepeatBehavior repeatBehavior, Image image)
        {
            //this.\u002Ector();
            this._sourceStream = sourceStream;
            this._sourceUri = sourceUri;
            this._metadata = metadata;
            this._image = image;
            this._palettes = Animator.CreatePalettes(metadata);
            this._bitmap = Animator.CreateBitmap(metadata);
            GifLogicalScreenDescriptor screenDescriptor = metadata.Header.LogicalScreenDescriptor;
            this._stride = 4 * ((screenDescriptor.Width * 32 + 31) / 32);
            this._previousBackBuffer = new byte[screenDescriptor.Height * this._stride];
            this._indexStreamBuffer = Animator.CreateIndexStreamBuffer(metadata, this._sourceStream);
            this._timingManager = this.CreateTimingManager(metadata, repeatBehavior);
        }

        ~Animator()
        {
            //try
            //{
                this.Dispose(false);
            //}
            //finally
            //{
                // ISSUE: explicit finalizer call
                // ISSUE: explicit non-virtual call
            //    this.Finalize();
            //}
        }

        internal static async Task<Animator> CreateAsync(Image image, Uri sourceUri, CancellationToken cancellationToken, RepeatBehavior repeatBehavior = default(RepeatBehavior))
        {
            UriLoader loader = new UriLoader();
            // ISSUE: reference to a compiler-generated field
            loader.DownloadProgressChanged += Animator.DownloadProgressChanged;
            try
            {
                Stream stream = await loader.GetStreamFromUriAsync(sourceUri, cancellationToken);
                try
                {
                    return await Animator.CreateAsync(stream, sourceUri, repeatBehavior, image);
                }
                catch
                {
                    Stream stream1 = stream;
                    if (stream1 != null)
                    {
                        // ISSUE: explicit non-virtual call
                        stream1.Dispose();
                    }
                    throw;
                }
            }
            catch (TaskCanceledException )
            {
            }
            // ISSUE: reference to a compiler-generated field
            loader.DownloadProgressChanged -= Animator.DownloadProgressChanged;
            return null;
        }

        internal static Task<Animator> CreateAsync(Image image, Stream sourceStream, RepeatBehavior repeatBehavior = default(RepeatBehavior))
        {
            return Animator.CreateAsync(sourceStream, null, repeatBehavior, image);
        }

        private static async Task<Animator> CreateAsync(Stream sourceStream, Uri sourceUri, RepeatBehavior repeatBehavior, Image image)
        {
            Stream stream = sourceStream.AsBuffered();
            GifDataStream metadata = await GifDataStream.ReadAsync(stream);
            return new Animator(stream, sourceUri, metadata, repeatBehavior, image);
        }

        public async void Play()
        {
            try
            {
                if (!this._isStarted)
                {
                    this._cancellationTokenSource = new CancellationTokenSource();
                    this._isStarted = true;
                    if (this._timingManager.IsPaused)
                        this._timingManager.Resume();
                    await this.RunAsync(this._cancellationTokenSource.Token);
                }
                else
                {
                    if (!this._timingManager.IsPaused)
                        return;
                    this._timingManager.Resume();
                }
            }
            catch (OperationCanceledException )
            {
            }
            catch (Exception ex)
            {
                if (this._disposing)
                    return;
                AnimationBehavior.OnError(this._image, ex, AnimationErrorKind.Rendering);
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Task<bool> timing = this._timingManager.NextAsync(cancellationToken);
                Task task = this.RenderFrameAsync(this.CurrentFrameIndex, cancellationToken);
                await Task.WhenAll((Task)timing, task);
                if (timing.Result)
                {
                    this.CurrentFrameIndex = (this.CurrentFrameIndex + 1) % this.FrameCount;
                    timing = null;
                }
                else
                    break;
            }
        }

        public void Pause()
        {
            this._timingManager.Pause();
        }

        protected virtual void OnCurrentFrameChanged()
        {
            // ISSUE: reference to a compiler-generated field
            EventHandler currentFrameChanged = this.CurrentFrameChanged;
            if (currentFrameChanged == null)
                return;
            EventArgs empty = EventArgs.Empty;
            currentFrameChanged(this, empty);
        }

        protected virtual void OnAnimationCompleted()
        {
            // ISSUE: reference to a compiler-generated field
            EventHandler animationCompleted = this.AnimationCompleted;
            if (animationCompleted == null)
                return;
            EventArgs empty = EventArgs.Empty;
            animationCompleted(this, empty);
        }

        private TimingManager CreateTimingManager(GifDataStream metadata, RepeatBehavior repeatBehavior)
        {
            TimingManager timingManager = new TimingManager(repeatBehavior == new RepeatBehavior() ? Animator.GetRepeatBehavior(metadata) : repeatBehavior);
            foreach (GifFrame frame in (IEnumerable<GifFrame>)metadata.Frames)
                timingManager.Add(Animator.GetFrameDelay(frame));
            timingManager.Completed += new EventHandler(this.TimingManagerCompleted);
            return timingManager;
        }

        private void TimingManagerCompleted(object sender, EventArgs e)
        {
            this.OnAnimationCompleted();
        }

        private static WriteableBitmap CreateBitmap(GifDataStream metadata)
        {
            GifLogicalScreenDescriptor screenDescriptor = metadata.Header.LogicalScreenDescriptor;
            return new WriteableBitmap(screenDescriptor.Width, screenDescriptor.Height);
        }

        private static Dictionary<int, Animator.GifPalette> CreatePalettes(GifDataStream metadata)
        {
            Dictionary<int, Animator.GifPalette> dictionary = new Dictionary<int, Animator.GifPalette>();
            Color[] colorArray = (Color[])null;
            if (metadata.Header.LogicalScreenDescriptor.HasGlobalColorTable)
                colorArray = (Color[])Enumerable.ToArray<Color>(Enumerable.Select<GifColor, Color>(metadata.GlobalColorTable, (Func<GifColor, Color>)(gc => Color.FromArgb(byte.MaxValue, gc.R, gc.G, gc.B))));
            for (int index = 0; index < metadata.Frames.Count; ++index)
            {
                GifFrame frame = metadata.Frames[index];
                Color[] colors = colorArray;
                if (frame.Descriptor.HasLocalColorTable)
                    colors = (Color[])Enumerable.ToArray<Color>(Enumerable.Select<GifColor, Color>(frame.LocalColorTable, (Func<GifColor, Color>)(gc => Color.FromArgb(byte.MaxValue, gc.R, gc.G, gc.B))));
                int? transparencyIndex = new int?();
                GifGraphicControlExtension graphicControl = frame.GraphicControl;
                if (graphicControl != null && graphicControl.HasTransparency)
                    transparencyIndex = new int?(graphicControl.TransparencyIndex);
                dictionary[index] = new Animator.GifPalette(transparencyIndex, colors);
            }
            return dictionary;
        }

        private static byte[] CreateIndexStreamBuffer(GifDataStream metadata, Stream stream)
        {
            long val2 = stream.Length - ((GifFrame)Enumerable.Last<GifFrame>(metadata.Frames)).ImageData.CompressedDataStartOffset;
            long num = val2;
            if (metadata.Frames.Count > 1)
                num = Math.Max(Enumerable.Max((IEnumerable<long>)Enumerable.Zip<GifFrame, GifFrame, long>((IEnumerable<GifFrame>)metadata.Frames, (IEnumerable<GifFrame>)Enumerable.Skip<GifFrame>((IEnumerable<GifFrame>)metadata.Frames, 1), (Func<GifFrame, GifFrame, long>)((f1, f2) => f2.ImageData.CompressedDataStartOffset - f1.ImageData.CompressedDataStartOffset))), val2);
            return new byte[num + 4L];
        }


        private async Task RenderFrameAsync(int frameIndex, CancellationToken cancellationToken)
        {
            if (frameIndex < 0)
                return;
            GifFrame frame = this._metadata.Frames[frameIndex];
            GifImageDescriptor desc = frame.Descriptor;
            using (Stream indexStreamAsync = await this.GetIndexStreamAsync(frame, cancellationToken))
            {
                if (frameIndex < this._previousFrameIndex)
                    this.ClearArea((IGifRect)this._metadata.Header.LogicalScreenDescriptor);
                else
                    this.DisposePreviousFrame(frame);
                int length = 4 * desc.Width;
                byte[] buffer = new byte[desc.Width];
                byte[] numArray = new byte[length];
                Animator.GifPalette palette = this._palettes[frameIndex];
                int num1 = palette.TransparencyIndex ?? -1;
                foreach (int num2 in frame.Descriptor.Interlace ? Animator.InterlacedRows(frame.Descriptor.Height) : Animator.NormalRows(frame.Descriptor.Height))
                {
                    if (indexStreamAsync.Read(buffer, 0, desc.Width) != desc.Width)
                        throw new EndOfStreamException();
                    int offset = (desc.Top + num2) * this._stride + desc.Left * 4;
                    if (num1 >= 0)
                        Animator.CopyFromBitmap(numArray, this._bitmap, offset, length);
                    for (int index = 0; index < desc.Width; ++index)
                    {
                        byte num3 = buffer[index];
                        int startIndex = 4 * index;
                        if ((int)num3 != num1)
                            Animator.WriteColor(numArray, palette[(int)num3], startIndex);
                    }
                    Animator.CopyToBitmap(numArray, this._bitmap, offset, length);
                }
                this._bitmap.Invalidate();
                this._previousFrame = frame;
                this._previousFrameIndex = frameIndex;
            }
        }

        private static IEnumerable<int> NormalRows(int height)
        {
            return Enumerable.Range(0, height);
        }

        private static IEnumerable<int> InterlacedRows(int height)
        {
            /*
             * 4 passes:
             * Pass 1: rows 0, 8, 16, 24...
             * Pass 2: rows 4, 12, 20, 28...
             * Pass 3: rows 2, 6, 10, 14...
             * Pass 4: rows 1, 3, 5, 7...
             * */
            var passes = new[]
            {
                new { Start = 0, Step = 8 },
                new { Start = 4, Step = 8 },
                new { Start = 2, Step = 4 },
                new { Start = 1, Step = 2 }
            };
            foreach (var pass in passes)
            {
                int y = pass.Start;
                while (y < height)
                {
                    yield return y;
                    y += pass.Step;
                }
            }
        }

        private static void CopyToBitmap(byte[] buffer, WriteableBitmap bitmap, int offset, int length)
        {
            GCHandle gcHandle = GCHandle.Alloc((object)bitmap.Pixels, GCHandleType.Pinned);
            try
            {
                IntPtr num = gcHandle.AddrOfPinnedObject();
                Marshal.Copy(buffer, 0, num + offset, length);
            }
            finally
            {
                if (gcHandle.IsAllocated)
                    gcHandle.Free();
            }
        }

        private static void CopyFromBitmap(byte[] buffer, WriteableBitmap bitmap, int offset, int length)
        {
            GCHandle gcHandle = GCHandle.Alloc((object)bitmap.Pixels, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(gcHandle.AddrOfPinnedObject() + offset, buffer, 0, length);
            }
            finally
            {
                if (gcHandle.IsAllocated)
                    gcHandle.Free();
            }
        }

        private static void WriteColor(byte[] lineBuffer, Color color, int startIndex)
        {
            lineBuffer[startIndex] = color.B;
            lineBuffer[startIndex + 1] = color.G;
            lineBuffer[startIndex + 2] = color.R;
            lineBuffer[startIndex + 3] = color.A;
        }

        private void DisposePreviousFrame(GifFrame currentFrame)
        {
            GifFrame previousFrame = this._previousFrame;
            GifGraphicControlExtension controlExtension = previousFrame != null ? previousFrame.GraphicControl : null;
            if (controlExtension != null)
            {
                switch (controlExtension.DisposalMethod)
                {
                    case GifFrameDisposalMethod.RestoreBackground:
                        this.ClearArea((IGifRect)this._previousFrame.Descriptor);
                        break;
                    case GifFrameDisposalMethod.RestorePrevious:
                        Animator.CopyToBitmap(this._previousBackBuffer, this._bitmap, 0, this._previousBackBuffer.Length);
                        break;
                }
            }
            GifGraphicControlExtension graphicControl = currentFrame.GraphicControl;
            if (graphicControl == null || graphicControl.DisposalMethod != GifFrameDisposalMethod.RestorePrevious)
                return;
            Animator.CopyFromBitmap(this._previousBackBuffer, this._bitmap, 0, this._previousBackBuffer.Length);
        }

        private void ClearArea(IGifRect rect)
        {
            int length = 4 * rect.Width;
            byte[] buffer = new byte[length];
            for (int index = 0; index < rect.Height; ++index)
            {
                int offset = (rect.Top + index) * this._stride + 4 * rect.Left;
                Animator.CopyToBitmap(buffer, this._bitmap, offset, length);
            }
        }

        private async Task<Stream> GetIndexStreamAsync(GifFrame frame, CancellationToken cancellationToken)
        {
            GifImageData data = frame.ImageData;
            cancellationToken.ThrowIfCancellationRequested();
            this._sourceStream.Seek(data.CompressedDataStartOffset, SeekOrigin.Begin);
            MemoryStream ms = new MemoryStream(this._indexStreamBuffer);
            try
            {
                await GifHelpers.CopyDataBlocksToStreamAsync(this._sourceStream, (Stream)ms, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (ms != null)
                    ((IDisposable)ms).Dispose();
            }
            ms = null;
            return (Stream)new LzwDecompressStream(this._indexStreamBuffer, (int)data.LzwMinimumCodeSize);
        }

        private static TimeSpan GetFrameDelay(GifFrame frame)
        {
            GifGraphicControlExtension graphicControl = frame.GraphicControl;
            if (graphicControl != null && graphicControl.Delay != 0)
                return TimeSpan.FromMilliseconds((double)graphicControl.Delay);
            return TimeSpan.FromMilliseconds(100.0);
        }

        private static RepeatBehavior GetRepeatBehavior(GifDataStream metadata)
        {
            if ((int)metadata.RepeatCount == 0)
                return RepeatBehavior.Forever;
            return new RepeatBehavior((double)metadata.RepeatCount);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
                return;
            this._disposing = true;
            this._timingManager.Completed -= new EventHandler(this.TimingManagerCompleted);
            CancellationTokenSource cancellationTokenSource = this._cancellationTokenSource;
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
            try
            {
                Stream sourceStream = this._sourceStream;
                if (sourceStream != null)
                {
                    // ISSUE: explicit non-virtual call
                    sourceStream.Dispose();
                }
            }
            catch
            {
            }
            this._disposed = true;
        }

        public override string ToString()
        {
            Uri sourceUri = this._sourceUri;
            return string.Concat("GIF: ", (sourceUri != null ? sourceUri.ToString() : null) ?? this._sourceStream.ToString());
        }

        internal async void ShowFirstFrame()
        {
            try
            {
                await this.RenderFrameAsync(0, CancellationToken.None);
                this.CurrentFrameIndex = 0;
                this._timingManager.Pause();
            }
            catch (Exception ex)
            {
                AnimationBehavior.OnError(this._image, ex, AnimationErrorKind.Rendering);
            }
        }

        private class GifPalette
        {
            private readonly Color[] _colors;

            public int? TransparencyIndex { get; set; }

            public Color this[int i]
            {
                get
                {
                    return this._colors[i];
                }
            }

            public GifPalette(int? transparencyIndex, Color[] colors)
            {
                this.TransparencyIndex = transparencyIndex;
                this._colors = colors;
            }
        }
    }
}
