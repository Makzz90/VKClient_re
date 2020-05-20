using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VKClient.Audio.Base.Core;
using VKClient.Audio.Base.Utils;
using VKClient.Common.ImageViewer;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework
{
    public static class VeryLowProfileImageLoader
    {
        private static readonly int LRU_CACHE_CAPACITY = 7000000;
        private static readonly int STALE_THRESHOLD = 3000;
        private static readonly Thread _thread = new Thread(new ParameterizedThreadStart(VeryLowProfileImageLoader.WorkerThreadProc));
        private static readonly Queue<VeryLowProfileImageLoader.PendingRequest> _pendingRequests = new Queue<VeryLowProfileImageLoader.PendingRequest>();
        private static readonly Queue<IAsyncResult> _pendingResponses = new Queue<IAsyncResult>();
        private static readonly Dictionary<string, long> _prioritiesDict = new Dictionary<string, long>();
        private static readonly object _syncBlock = new object();
        private static Dictionary<int, Uri> _hashToUriDict = new Dictionary<int, Uri>();
        public static readonly string REQUIRE_CACHING_KEY = "RequireCache";
        public static Dictionary<string, string> _downloadedDictionary = new Dictionary<string, string>();
        public static List<string> _downloadedList = new List<string>();
        public static readonly DependencyProperty UriSourceProperty = DependencyProperty.RegisterAttached("UriSource", typeof(Uri), typeof(VeryLowProfileImageLoader), new PropertyMetadata(new PropertyChangedCallback(VeryLowProfileImageLoader.OnUriSourceChanged)));
        public static readonly DependencyProperty UseBackgroundCreationProperty = DependencyProperty.RegisterAttached("UseBackgroundCreation", typeof(bool), typeof(VeryLowProfileImageLoader), new PropertyMetadata((object)true, new PropertyChangedCallback(VeryLowProfileImageLoader.OnUseBackgroundCreationChanged)));
        public static readonly DependencyProperty CleanupSourceWhenNewUriPendingProperty = DependencyProperty.RegisterAttached("CleanupSourceWhenNewUriPending", typeof(bool), typeof(VeryLowProfileImageLoader), new PropertyMetadata((object)true));
        private static List<double> _statistics = new List<double>();
        private static object _downloadedDictLock = new object();
        private static int _totalDownloadCount = 0;
        private static Dictionary<Guid, VeryLowProfileImageLoader.PendingRequest> _requestsInProcess = new Dictionary<Guid, VeryLowProfileImageLoader.PendingRequest>();
        private static object _lockDict = new object();
        private static bool _enableLog = false;
        private const int WorkItemQuantum = 5;
        private static bool _exiting;
        private static LRUCache<Uri, Stream> _lruCache;

        public static bool IsEnabled { get; set; }

        public static bool AllowBoostLoading { get; set; }

        public static int MaxSimultaneousLoads
        {
            get
            {
                return !VeryLowProfileImageLoader.AllowBoostLoading ? 1 : 20;
            }
        }

        static VeryLowProfileImageLoader()
        {
            VeryLowProfileImageLoader._thread.Start();
            VeryLowProfileImageLoader.IsEnabled = true;
        }

        public static Uri GetUriSource(Image obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            return (Uri)obj.GetValue(VeryLowProfileImageLoader.UriSourceProperty);
        }

        public static void SetUriSource(Image obj, Uri value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            obj.SetValue(VeryLowProfileImageLoader.UriSourceProperty, (object)value);
        }

        public static bool GetUseBackgroundCreation(Image obj)
        {
            return (bool)obj.GetValue(VeryLowProfileImageLoader.UseBackgroundCreationProperty);
        }

        public static void SetUseBackgroundCreation(Image obj, bool value)
        {
            obj.SetValue(VeryLowProfileImageLoader.UseBackgroundCreationProperty, (object)value);
        }

        public static bool GetCleanupSourceWhenNewUriPending(Image obj)
        {
            return (bool)obj.GetValue(VeryLowProfileImageLoader.CleanupSourceWhenNewUriPendingProperty);
        }

        public static void SetCleanupSourceWhenNewUriPending(Image obj, bool value)
        {
            obj.SetValue(VeryLowProfileImageLoader.CleanupSourceWhenNewUriPendingProperty, (object)value);
        }

        private static void OnUseBackgroundCreationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void WorkerThreadProc(object unused)
        {
            Random rand = new Random();
            List<VeryLowProfileImageLoader.PendingRequest> pendingRequestList1 = new List<VeryLowProfileImageLoader.PendingRequest>();
            Queue<IAsyncResult> asyncResultQueue = new Queue<IAsyncResult>();
            VeryLowProfileImageLoader.RestoreState();
            while (!VeryLowProfileImageLoader._exiting)
            {
                lock (VeryLowProfileImageLoader._syncBlock)
                {
                    int local_9 = VeryLowProfileImageLoader.RemoveStaleAndGetCount();
                    if (VeryLowProfileImageLoader._pendingRequests.Count == 0 && VeryLowProfileImageLoader._pendingResponses.Count == 0 && (pendingRequestList1.Count == 0 && asyncResultQueue.Count == 0) || local_9 >= VeryLowProfileImageLoader.MaxSimultaneousLoads)
                    {
                        Monitor.Wait(VeryLowProfileImageLoader._syncBlock, 300);
                        if (VeryLowProfileImageLoader._exiting)
                          break;
                    }
                    while (0 < VeryLowProfileImageLoader._pendingRequests.Count)
                    {
                        VeryLowProfileImageLoader.PendingRequest local_10 = VeryLowProfileImageLoader._pendingRequests.Dequeue();
                        for (int local_11 = 0; local_11 < pendingRequestList1.Count; ++local_11)
                        {
                            if (pendingRequestList1[local_11].Image == local_10.Image)
                            {
                                pendingRequestList1[local_11] = local_10;
                                local_10 = null;
                                break;
                            }
                        }
                        if (local_10 != null)
                            pendingRequestList1.Add(local_10);
                    }
                    while (0 < VeryLowProfileImageLoader._pendingResponses.Count)
                        asyncResultQueue.Enqueue(VeryLowProfileImageLoader._pendingResponses.Dequeue());
                }
                Queue<VeryLowProfileImageLoader.PendingCompletion> pendingCompletions = new Queue<VeryLowProfileImageLoader.PendingCompletion>();
                int num1 = 0;
                List<VeryLowProfileImageLoader.PendingRequest> pendingRequestList2 = new List<VeryLowProfileImageLoader.PendingRequest>();
                for (int index = 0; index < pendingRequestList1.Count; ++index)
                {
                    VeryLowProfileImageLoader.PendingRequest pendingRequest = pendingRequestList1[index];
                    if (VeryLowProfileImageLoader._hashToUriDict[pendingRequest.Image.GetHashCode()] != pendingRequest.Uri)
                        pendingRequestList2.Add(pendingRequest);
                }
                foreach (VeryLowProfileImageLoader.PendingRequest pendingRequest in pendingRequestList2)
                    pendingRequestList1.Remove(pendingRequest);
                if (pendingRequestList2.Count > 0)
                    VeryLowProfileImageLoader.Log("Discarded " + (object)pendingRequestList2.Count);
                int count1 = VeryLowProfileImageLoader.RemoveStaleAndGetCount();
                foreach (VeryLowProfileImageLoader.PendingRequest pendingRequest in pendingRequestList1.Where<VeryLowProfileImageLoader.PendingRequest>((Func<VeryLowProfileImageLoader.PendingRequest, bool>)(pr => VeryLowProfileImageLoader.HaveInDownloadedDict(pr))).ToList<VeryLowProfileImageLoader.PendingRequest>().Take<VeryLowProfileImageLoader.PendingRequest>(3))
                {
                    HttpWebRequest http = WebRequest.CreateHttp(pendingRequest.Uri);
                    http.AllowReadStreamBuffering = true;
                    pendingRequest.DownloadStaredTimestamp = DateTime.Now;
                    http.BeginGetResponse(new AsyncCallback(VeryLowProfileImageLoader.HandleGetResponseResult), (object)new VeryLowProfileImageLoader.ResponseState((WebRequest)http, pendingRequest.Image, pendingRequest.Uri, pendingRequest.DownloadStaredTimestamp, pendingRequest.UniqueId, pendingRequest.CurrentAttempt));
                    pendingRequestList1.Remove(pendingRequest);
                    VeryLowProfileImageLoader.Log("Processed Already downloaded " + (object)pendingRequest.Uri);
                }
                while (count1 < VeryLowProfileImageLoader.MaxSimultaneousLoads && pendingRequestList1.Count > 0)
                {
                    if (num1 >= VeryLowProfileImageLoader.MaxSimultaneousLoads)
                        VeryLowProfileImageLoader.Log("!!!!! noOfCycles=" + (object)num1);
                    int count2 = pendingRequestList1.Count;
                    int elementWithMaxPriority = VeryLowProfileImageLoader.GetIndexOfElementWithMaxPriority(pendingRequestList1, rand);
                    VeryLowProfileImageLoader.PendingRequest request = pendingRequestList1[elementWithMaxPriority];
                    pendingRequestList1[elementWithMaxPriority] = pendingRequestList1[count2 - 1];
                    pendingRequestList1.RemoveAt(count2 - 1);
                    HttpWebRequest http = WebRequest.CreateHttp(request.Uri);
                    http.AllowReadStreamBuffering = true;
                    request.DownloadStaredTimestamp = DateTime.Now;
                    http.BeginGetResponse(new AsyncCallback(VeryLowProfileImageLoader.HandleGetResponseResult), (object)new VeryLowProfileImageLoader.ResponseState((WebRequest)http, request.Image, request.Uri, request.DownloadStaredTimestamp, request.UniqueId, request.CurrentAttempt));
                    VeryLowProfileImageLoader.AddRequestInProgress(request);
                    ++count1;
                    VeryLowProfileImageLoader.Log("Loading " + (object)request.Uri + ";processing started in " + (object)(DateTime.Now - request.CreatedTimstamp).TotalMilliseconds + " Currently loading " + (object)count1);
                }
                if (pendingRequestList1.Count > 0)
                {
                    lock (VeryLowProfileImageLoader._syncBlock)
                    {
                        foreach (VeryLowProfileImageLoader.PendingRequest item_1 in pendingRequestList1)
                            VeryLowProfileImageLoader._pendingRequests.Enqueue(item_1);
                    }
                }
                for (int index = 0; 0 < asyncResultQueue.Count && index < 5; ++index)
                {
                    IAsyncResult asyncResult = asyncResultQueue.Dequeue();
                    VeryLowProfileImageLoader.ResponseState responseState = (VeryLowProfileImageLoader.ResponseState)asyncResult.AsyncState;
                    try
                    {
                        WebResponse response = responseState.WebRequest.EndGetResponse(asyncResult);
                        pendingCompletions.Enqueue(new VeryLowProfileImageLoader.PendingCompletion(responseState.Image, responseState.Uri, response.GetResponseStream(), responseState.Timestamp, responseState.RequestId));
                    }
                    catch (WebException ex)
                    {
                        Logger.Instance.Error(string.Format("LowProfileImageLoader exception when fetching {0}", (object)responseState.Uri.OriginalString), (Exception)ex);
                        if (responseState.CurrentAttempt <= 3)
                            Execute.ExecuteOnUIThread((Action)(() => VeryLowProfileImageLoader.AddPendingRequest(responseState.Image, responseState.Uri, responseState.CurrentAttempt + 1)));
                    }
                    Thread.Sleep(1);
                }
                if (0 < pendingCompletions.Count)
                    Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        List<VeryLowProfileImageLoader.PendingCompletion> pendingCompletionList = new List<VeryLowProfileImageLoader.PendingCompletion>();
                        while (0 < pendingCompletions.Count)
                        {
                            VeryLowProfileImageLoader.PendingCompletion pendingCompletion = pendingCompletions.Dequeue();
                            pendingCompletionList.Add(pendingCompletion);
                            VeryLowProfileImageLoader.AddToDownloadedDict(pendingCompletion.Uri);
                            VeryLowProfileImageLoader.AddToCache(pendingCompletion.Uri, pendingCompletion.Stream);
                            if (VeryLowProfileImageLoader.GetUriSource(pendingCompletion.Image) == pendingCompletion.Uri)
                            {
                                BitmapImage sizedBitmap = VeryLowProfileImageLoader.CreateSizedBitmap((FrameworkElement)pendingCompletion.Image, pendingCompletion.Uri, VeryLowProfileImageLoader.GetUseBackgroundCreation(pendingCompletion.Image));
                                try
                                {
                                    sizedBitmap.SetSource(pendingCompletion.Stream);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Instance.Error("Error of reading image", ex);
                                }
                                pendingCompletion.Image.Source = (ImageSource)sizedBitmap;
                                DateTime now = DateTime.Now;
                                ++VeryLowProfileImageLoader._totalDownloadCount;
                                DateTime timestamp = pendingCompletion.Timestamp;
                                double totalMilliseconds = (now - timestamp).TotalMilliseconds;
                                VeryLowProfileImageLoader.Log(string.Format("Downloaded image {0} in {1} ms. Totally downloaded = {2}", (object)pendingCompletion.Uri.OriginalString, (object)totalMilliseconds, (object)VeryLowProfileImageLoader._totalDownloadCount));
                                if (VeryLowProfileImageLoader._enableLog)
                                {
                                    VeryLowProfileImageLoader._statistics.Add(totalMilliseconds);
                                    if (VeryLowProfileImageLoader._statistics.Count % 30 == 0 && VeryLowProfileImageLoader._statistics.Count > 0)
                                    {
                                        double num = VeryLowProfileImageLoader._statistics.Max();
                                        VeryLowProfileImageLoader.Log(string.Format("Statistics downloading: average {0}, max = {1}", (object)(VeryLowProfileImageLoader._statistics.Sum() / (double)VeryLowProfileImageLoader._statistics.Count), (object)num));
                                    }
                                }
                            }
                            pendingCompletion.Stream.Dispose();
                        }
                        try
                        {
                            foreach (VeryLowProfileImageLoader.PendingCompletion pendingCompletion in pendingCompletionList)
                                VeryLowProfileImageLoader.RemoveRequestInProgress(pendingCompletion.RequestId);
                        }
                        catch
                        {
                        }
                    }));
            }
        }

        public static BitmapImage CreateSizedBitmap(FrameworkElement container, Uri uri, bool backgroundCreation = true)
        {
            Dictionary<string, string> queryString = uri.ParseQueryString();
            double result1 = 0.0;
            double result2 = 0.0;
            if (queryString.ContainsKey("wh"))
            {
                string[] strArray = queryString["wh"].Split('_');
                if (strArray.Length >= 2)
                {
                    double.TryParse(strArray[0], out result1);
                    double.TryParse(strArray[1], out result2);
                }
            }
            BitmapImage bitmapImage;
            if (backgroundCreation)
                bitmapImage = new BitmapImage()
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation | BitmapCreateOptions.BackgroundCreation,
                    DecodePixelType = DecodePixelType.Logical
                };
            else
                bitmapImage = new BitmapImage()
                {
                    CreateOptions = BitmapCreateOptions.DelayCreation,
                    DecodePixelType = DecodePixelType.Logical
                };
            if (container.Height > 0.0 && container.Width > 0.0)
            {
                if (result1 == 0.0 || result2 == 0.0)
                {
                    bitmapImage.DecodePixelHeight = (int)container.Height;
                }
                else
                {
                    Rect fill = RectangleUtils.ResizeToFill(new Rect(0.0, 0.0, container.Width, container.Height), new Size(result1, result2));
                    bitmapImage.DecodePixelWidth = (int)fill.Width;
                    bitmapImage.DecodePixelHeight = (int)fill.Height;
                }
            }
            else
                bitmapImage.DecodePixelHeight = 300;
            return bitmapImage;
        }

        private static void AddToCache(Uri uri, Stream stream)
        {
            VeryLowProfileImageLoader.EnsureLRUCache();
            VeryLowProfileImageLoader._lruCache.Add(uri, (Stream)StreamUtils.ReadFully(stream), true);
            stream.Position = 0L;
            if (uri.ParseQueryString().ContainsKey(VeryLowProfileImageLoader.REQUIRE_CACHING_KEY))
                ImageCache.Current.TrySetImageForUri(uri.OriginalString, (Stream)StreamUtils.ReadFully(stream));
            stream.Position = 0L;
        }

        public static bool HasImageInLRUCache(Uri uri)
        {
            if (uri == null)
                return false;
            VeryLowProfileImageLoader.EnsureLRUCache();
            return VeryLowProfileImageLoader._lruCache.Get(uri) != null;
        }

        public static Stream GetFromLRUCache(Uri uri)
        {
            VeryLowProfileImageLoader.EnsureLRUCache();
            Stream input = VeryLowProfileImageLoader._lruCache.Get(uri);
            if (input == null)
                return null;
            MemoryStream memoryStream = StreamUtils.ReadFully(input);
            input.Position = 0L;
            return (Stream)memoryStream;
        }

        private static void EnsureLRUCache()
        {
            if (VeryLowProfileImageLoader._lruCache != null)
                return;
            VeryLowProfileImageLoader._lruCache = new LRUCache<Uri, Stream>(VeryLowProfileImageLoader.LRU_CACHE_CAPACITY, (Func<Stream, int>)(s => (int)s.Length));
        }

        private static int GetIndexOfElementWithMaxPriority(List<VeryLowProfileImageLoader.PendingRequest> pendingRequests, Random rand)
        {
            long num1 = 0;
            int num2 = 0;
            bool flag = false;
            for (int index = 0; index < pendingRequests.Count; ++index)
            {
                string originalString = pendingRequests[index].Uri.OriginalString;
                if (VeryLowProfileImageLoader._prioritiesDict.ContainsKey(originalString) && VeryLowProfileImageLoader._prioritiesDict[originalString] > num1)
                {
                    flag = true;
                    num1 = VeryLowProfileImageLoader._prioritiesDict[originalString];
                    num2 = index;
                }
            }
            if (flag)
                return num2;
            return Math.Min(rand.Next(5), pendingRequests.Count - 1);
        }

        private static bool HaveInDownloadedDict(VeryLowProfileImageLoader.PendingRequest pr)
        {
            return VeryLowProfileImageLoader.HaveInDownloadedDict(pr.Uri);
        }

        public static bool HaveInDownloadedDict(Uri uri)
        {
            lock (VeryLowProfileImageLoader._downloadedDictLock)
                return VeryLowProfileImageLoader._downloadedDictionary.ContainsKey(uri.OriginalString.GetShorterVersion());
        }

        private static void AddToDownloadedDict(Uri uri)
        {
            lock (VeryLowProfileImageLoader._downloadedDictLock)
            {
                string local_2 = uri.OriginalString.GetShorterVersion();
                VeryLowProfileImageLoader._downloadedDictionary[local_2] = "";
                VeryLowProfileImageLoader._downloadedList.Add(local_2);
            }
        }

        public static void SaveState()
        {
            lock (VeryLowProfileImageLoader._downloadedDictLock)
            {
                VeryLowProfileImageLoader.SerializedData local_2 = new VeryLowProfileImageLoader.SerializedData()
                {
                    DownloadedUris = VeryLowProfileImageLoader._downloadedList.Skip<string>(Math.Max(0, VeryLowProfileImageLoader._downloadedList.Count<string>() - 1000)).Take<string>(1000).ToList<string>()
                };
                CacheManager.TrySerialize((IBinarySerializable)local_2, "VeryLowProfileImageLoaderData", false, CacheManager.DataType.CachedData);
                VeryLowProfileImageLoader.Log("VeryLowProfileImageLoader serialized uri count " + (object)local_2.DownloadedUris.Count);
            }
        }

        public static void RestoreState()
        {
            lock (VeryLowProfileImageLoader._downloadedDictLock)
            {
                VeryLowProfileImageLoader.SerializedData local_2 = new VeryLowProfileImageLoader.SerializedData();
                CacheManager.TryDeserialize((IBinarySerializable)local_2, "VeryLowProfileImageLoaderData", CacheManager.DataType.CachedData);
                VeryLowProfileImageLoader._downloadedList = local_2.DownloadedUris;
                foreach (string item_0 in VeryLowProfileImageLoader._downloadedList)
                    VeryLowProfileImageLoader._downloadedDictionary[item_0] = "";
                VeryLowProfileImageLoader.Log("VeryLowProfileImageLoader deserialized uri count " + (object)local_2.DownloadedUris.Count);
            }
        }

        private static void AddRequestInProgress(VeryLowProfileImageLoader.PendingRequest request)
        {
            lock (VeryLowProfileImageLoader._lockDict)
                VeryLowProfileImageLoader._requestsInProcess.Add(request.UniqueId, request);
        }

        private static void RemoveRequestInProgress(Guid uniqueId)
        {
            lock (VeryLowProfileImageLoader._lockDict)
            {
                if (!VeryLowProfileImageLoader._requestsInProcess.ContainsKey(uniqueId))
                    return;
                VeryLowProfileImageLoader._requestsInProcess.Remove(uniqueId);
            }
        }

        private static int RemoveStaleAndGetCount()
        {
            lock (VeryLowProfileImageLoader._lockDict)
            {
                DateTime now = DateTime.Now;
                List<VeryLowProfileImageLoader.PendingRequest> temp_13 = VeryLowProfileImageLoader._requestsInProcess.Values.Where<VeryLowProfileImageLoader.PendingRequest>((Func<VeryLowProfileImageLoader.PendingRequest, bool>)(r => (now - r.DownloadStaredTimestamp).TotalMilliseconds > (double)VeryLowProfileImageLoader.STALE_THRESHOLD)).ToList<VeryLowProfileImageLoader.PendingRequest>();
                int local_3 = 0;
                foreach (VeryLowProfileImageLoader.PendingRequest item_0 in temp_13)
                {
                    VeryLowProfileImageLoader.RemoveRequestInProgress(item_0.UniqueId);
                    VeryLowProfileImageLoader.Log("REMOVED STALE " + item_0.Uri.OriginalString);
                    ++local_3;
                }
                return VeryLowProfileImageLoader._requestsInProcess.Count;
            }
        }

        private static void Log(string info)
        {
            if (!VeryLowProfileImageLoader._enableLog)
                return;
            Logger.Instance.Info(info);
        }

        private static void OnUriSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Image image = (Image)o;
            Uri uri = (Uri)e.NewValue;
            int num = uri == null ? 1 : 0;
            VeryLowProfileImageLoader._hashToUriDict[image.GetHashCode()] = uri;
            if (uri == null)
            {
                BitmapImage img = image.Source as BitmapImage;
                image.Source = null;
                if (img != null)
                    VeryLowProfileImageLoader.DisposeImage(img);
                VeryLowProfileImageLoader.Log("OnUriSourceChanged uri = NULL");
            }
            else
            {
                VeryLowProfileImageLoader.Log(string.Format("OnUriSourceChanged uri = {0}", (object)uri));
                Stream streamSource = VeryLowProfileImageLoader.GetFromLRUCache(uri) ?? ImageCache.Current.GetCachedImageStream(uri.OriginalString);
                if (streamSource != null)
                {
                    BitmapImage sizedBitmap = VeryLowProfileImageLoader.CreateSizedBitmap((FrameworkElement)image, uri, VeryLowProfileImageLoader.GetUseBackgroundCreation(image));
                    sizedBitmap.SetSource(streamSource);
                    image.Source = (ImageSource)sizedBitmap;
                }
                else if (!uri.IsAbsoluteUri || !VeryLowProfileImageLoader.IsEnabled || DesignerProperties.IsInDesignTool)
                {
                    BitmapImage bitmapImage = new BitmapImage()
                    {
                        CreateOptions = BitmapCreateOptions.DelayCreation | BitmapCreateOptions.BackgroundCreation,
                        UriSource = uri
                    };
                    image.Source = (ImageSource)bitmapImage;
                }
                else
                    VeryLowProfileImageLoader.AddPendingRequest(image, uri, 1);
            }
        }

        private static void DisposeImage(BitmapImage img)
        {
            if (img == null)
                return;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(new byte[1]))
                    img.SetSource((Stream)memoryStream);
            }
            catch
            {
            }
        }

        private static void AddPendingRequest(Image image, Uri uri, int currentAttempt)
        {
            if (VeryLowProfileImageLoader.GetCleanupSourceWhenNewUriPending(image))
                image.Source = null;
            lock (VeryLowProfileImageLoader._syncBlock)
            {
                VeryLowProfileImageLoader._pendingRequests.Enqueue(new VeryLowProfileImageLoader.PendingRequest(image, uri, currentAttempt));
                Monitor.Pulse(VeryLowProfileImageLoader._syncBlock);
            }
        }

        private static void HandleGetResponseResult(IAsyncResult result)
        {
            lock (VeryLowProfileImageLoader._syncBlock)
            {
                VeryLowProfileImageLoader.ResponseState temp_6 = (VeryLowProfileImageLoader.ResponseState)result.AsyncState;
                VeryLowProfileImageLoader._pendingResponses.Enqueue(result);
                Monitor.Pulse(VeryLowProfileImageLoader._syncBlock);
            }
        }

        internal static void SetPriority(string uriStr, long priority)
        {
            VeryLowProfileImageLoader._prioritiesDict[uriStr] = priority;
        }

        public class SerializedData : IBinarySerializable
        {
            public List<string> DownloadedUris { get; set; }

            public SerializedData()
            {
                this.DownloadedUris = new List<string>();
            }

            public void Write(BinaryWriter writer)
            {
                writer.WriteList(this.DownloadedUris);
            }

            public void Read(BinaryReader reader)
            {
                this.DownloadedUris = reader.ReadList();
            }
        }

        private class PendingRequest
        {
            public Image Image { get; private set; }

            public Uri Uri { get; private set; }

            public DateTime CreatedTimstamp { get; private set; }

            public DateTime DownloadStaredTimestamp { get; set; }

            public Guid UniqueId { get; private set; }

            public int CurrentAttempt { get; set; }

            public PendingRequest(Image image, Uri uri, int currentAttempt)
            {
                this.Image = image;
                this.Uri = uri;
                this.CreatedTimstamp = DateTime.Now;
                this.UniqueId = Guid.NewGuid();
                this.CurrentAttempt = currentAttempt;
            }
        }

        private class ResponseState
        {
            public WebRequest WebRequest { get; private set; }

            public Image Image { get; private set; }

            public Uri Uri { get; private set; }

            public DateTime Timestamp { get; private set; }

            public Guid RequestId { get; set; }

            public int CurrentAttempt { get; set; }

            public ResponseState(WebRequest webRequest, Image image, Uri uri, DateTime startedTime, Guid requestId, int currentAttempt)
            {
                this.WebRequest = webRequest;
                this.Image = image;
                this.Uri = uri;
                this.Timestamp = startedTime;
                this.RequestId = requestId;
                this.CurrentAttempt = currentAttempt;
            }
        }

        private class PendingCompletion
        {
            public Image Image { get; private set; }

            public Uri Uri { get; private set; }

            public Stream Stream { get; private set; }

            public DateTime Timestamp { get; private set; }

            public Guid RequestId { get; private set; }

            public PendingCompletion(Image image, Uri uri, Stream stream, DateTime startedTime, Guid requestId)
            {
                this.Image = image;
                this.Uri = uri;
                this.Stream = stream;
                this.Timestamp = startedTime;
                this.RequestId = requestId;
            }
        }
    }
}
