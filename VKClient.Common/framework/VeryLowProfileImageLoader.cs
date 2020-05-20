using System;
using System.Collections;
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
    private static readonly System.Collections.Generic.Queue<VeryLowProfileImageLoader.PendingRequest> _pendingRequests = new System.Collections.Generic.Queue<VeryLowProfileImageLoader.PendingRequest>();
    private static readonly System.Collections.Generic.Queue<IAsyncResult> _pendingResponses = new System.Collections.Generic.Queue<IAsyncResult>();
    private static readonly Dictionary<string, long> _prioritiesDict = new Dictionary<string, long>();
    private static readonly object _syncBlock = new object();
    private static Dictionary<int, Uri> _hashToUriDict = new Dictionary<int, Uri>();
    public static readonly string REQUIRE_CACHING_KEY = "RequireCache";
    public static Dictionary<string, string> _downloadedDictionary = new Dictionary<string, string>();
    public static List<string> _downloadedList = new List<string>();
    public static readonly DependencyProperty UriSourceProperty = DependencyProperty.RegisterAttached("UriSource", typeof(Uri), typeof(VeryLowProfileImageLoader), new PropertyMetadata(new PropertyChangedCallback(VeryLowProfileImageLoader.OnUriSourceChanged)));
    public static readonly DependencyProperty UseBackgroundCreationProperty = DependencyProperty.RegisterAttached("UseBackgroundCreation", typeof(bool), typeof(VeryLowProfileImageLoader), new PropertyMetadata(true, new PropertyChangedCallback(VeryLowProfileImageLoader.OnUseBackgroundCreationChanged)));
    public static readonly DependencyProperty CleanupSourceWhenNewUriPendingProperty = DependencyProperty.RegisterAttached("CleanupSourceWhenNewUriPending", typeof (bool), typeof (VeryLowProfileImageLoader), new PropertyMetadata(true));
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
      return (Uri) ((DependencyObject) obj).GetValue(VeryLowProfileImageLoader.UriSourceProperty);
    }

    public static void SetUriSource(Image obj, Uri value)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      ((DependencyObject) obj).SetValue(VeryLowProfileImageLoader.UriSourceProperty, value);
    }

    public static bool GetUseBackgroundCreation(Image obj)
    {
      return (bool) ((DependencyObject) obj).GetValue(VeryLowProfileImageLoader.UseBackgroundCreationProperty);
    }

    public static void SetUseBackgroundCreation(Image obj, bool value)
    {
      ((DependencyObject) obj).SetValue(VeryLowProfileImageLoader.UseBackgroundCreationProperty, value);
    }

    public static bool GetCleanupSourceWhenNewUriPending(Image obj)
    {
      return (bool) ((DependencyObject) obj).GetValue(VeryLowProfileImageLoader.CleanupSourceWhenNewUriPendingProperty);
    }

    public static void SetCleanupSourceWhenNewUriPending(Image obj, bool value)
    {
      ((DependencyObject) obj).SetValue(VeryLowProfileImageLoader.CleanupSourceWhenNewUriPendingProperty, value);
    }

    private static void OnUseBackgroundCreationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private static void WorkerThreadProc(object unused)
{
	Random rand = new Random();
	List<VeryLowProfileImageLoader.PendingRequest> list = new List<VeryLowProfileImageLoader.PendingRequest>();
	Queue<IAsyncResult> queue = new Queue<IAsyncResult>();
	VeryLowProfileImageLoader.RestoreState();
	while (!VeryLowProfileImageLoader._exiting)
	{
		object syncBlock = VeryLowProfileImageLoader._syncBlock;
		lock (syncBlock)
		{
			int num = VeryLowProfileImageLoader.RemoveStaleAndGetCount();
			if ((VeryLowProfileImageLoader._pendingRequests.Count == 0 && VeryLowProfileImageLoader._pendingResponses.Count == 0 && list.Count == 0 && queue.Count == 0) || num >= VeryLowProfileImageLoader.MaxSimultaneousLoads)
			{
				Monitor.Wait(VeryLowProfileImageLoader._syncBlock, 300);
				if (VeryLowProfileImageLoader._exiting)
				{
					break;
				}
			}
			while (0 < VeryLowProfileImageLoader._pendingRequests.Count)
			{
				VeryLowProfileImageLoader.PendingRequest pendingRequest = VeryLowProfileImageLoader._pendingRequests.Dequeue();
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].Image == pendingRequest.Image)
					{
						list[i]= pendingRequest;
						pendingRequest = null;
						break;
					}
				}
				if (pendingRequest != null)
				{
					list.Add(pendingRequest);
				}
			}
			while (0 < VeryLowProfileImageLoader._pendingResponses.Count)
			{
				queue.Enqueue(VeryLowProfileImageLoader._pendingResponses.Dequeue());
			}
		}
		Queue<VeryLowProfileImageLoader.PendingCompletion> pendingCompletions = new Queue<VeryLowProfileImageLoader.PendingCompletion>();
		int num2 = 0;
		List<VeryLowProfileImageLoader.PendingRequest> list2 = new List<VeryLowProfileImageLoader.PendingRequest>();
		for (int j = 0; j < list.Count; j++)
		{
			VeryLowProfileImageLoader.PendingRequest pendingRequest2 = list[j];
			if (VeryLowProfileImageLoader._hashToUriDict[pendingRequest2.Image.GetHashCode()] != pendingRequest2.Uri)
			{
				list2.Add(pendingRequest2);
			}
		}
		using (List<VeryLowProfileImageLoader.PendingRequest>.Enumerator enumerator = list2.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				VeryLowProfileImageLoader.PendingRequest current = enumerator.Current;
				list.Remove(current);
			}
		}
		if (list2.Count > 0)
		{
			VeryLowProfileImageLoader.Log("Discarded " + list2.Count);
		}
		int num3 = VeryLowProfileImageLoader.RemoveStaleAndGetCount();
		IEnumerable<VeryLowProfileImageLoader.PendingRequest> arg_1F8_0 = list;
        Func<VeryLowProfileImageLoader.PendingRequest, bool> arg_1F8_1 = new Func<VeryLowProfileImageLoader.PendingRequest, bool>((pr) => { return VeryLowProfileImageLoader.HaveInDownloadedDict(pr); });
		
		using (IEnumerator<VeryLowProfileImageLoader.PendingRequest> enumerator2 = Enumerable.Take<VeryLowProfileImageLoader.PendingRequest>(Enumerable.ToList<VeryLowProfileImageLoader.PendingRequest>(Enumerable.Where<VeryLowProfileImageLoader.PendingRequest>(arg_1F8_0, arg_1F8_1)), 3).GetEnumerator())
		{
			while (enumerator2.MoveNext())
			{
				VeryLowProfileImageLoader.PendingRequest current2 = enumerator2.Current;
				HttpWebRequest httpWebRequest = WebRequest.CreateHttp(current2.Uri);
				httpWebRequest.AllowReadStreamBuffering=(true);
				current2.DownloadStaredTimestamp = DateTime.Now;
				httpWebRequest.BeginGetResponse(new AsyncCallback(VeryLowProfileImageLoader.HandleGetResponseResult), new VeryLowProfileImageLoader.ResponseState(httpWebRequest, current2.Image, current2.Uri, current2.DownloadStaredTimestamp, current2.UniqueId, current2.CurrentAttempt));
				list.Remove(current2);
				VeryLowProfileImageLoader.Log("Processed Already downloaded " + current2.Uri);
			}
			goto IL_3DA;
		}
		//goto IL_2B9;
IL_3DA:
		if (num3 >= VeryLowProfileImageLoader.MaxSimultaneousLoads || list.Count <= 0)
		{
			if (list.Count > 0)
			{
				syncBlock = VeryLowProfileImageLoader._syncBlock;
				lock (syncBlock)
				{
					using (List<VeryLowProfileImageLoader.PendingRequest>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							VeryLowProfileImageLoader.PendingRequest current3 = enumerator.Current;
							VeryLowProfileImageLoader._pendingRequests.Enqueue(current3);
						}
					}
				}
			}
			int num4 = 0;
			while (0 < queue.Count && num4 < 5)
			{
				IAsyncResult asyncResult = queue.Dequeue();
				VeryLowProfileImageLoader.ResponseState responseState = (VeryLowProfileImageLoader.ResponseState)asyncResult.AsyncState;
				try
				{
					WebResponse webResponse = responseState.WebRequest.EndGetResponse(asyncResult);
					pendingCompletions.Enqueue(new VeryLowProfileImageLoader.PendingCompletion(responseState.Image, responseState.Uri, webResponse.GetResponseStream(), responseState.Timestamp, responseState.RequestId));
				}
				catch (WebException e)
				{
					Logger.Instance.Error(string.Format("LowProfileImageLoader exception when fetching {0}", responseState.Uri.OriginalString), e);
					if (responseState.CurrentAttempt <= 3)
					{
						Execute.ExecuteOnUIThread(delegate
						{
							VeryLowProfileImageLoader.AddPendingRequest(responseState.Image, responseState.Uri, responseState.CurrentAttempt + 1);
						});
					}
				}
				Thread.Sleep(1);
				num4++;
			}
			if (0 < pendingCompletions.Count)
			{
				Deployment.Current.Dispatcher.BeginInvoke(delegate
				{
					List<VeryLowProfileImageLoader.PendingCompletion> list3 = new List<VeryLowProfileImageLoader.PendingCompletion>();
					while (0 < pendingCompletions.Count)
					{
						VeryLowProfileImageLoader.PendingCompletion pendingCompletion = pendingCompletions.Dequeue();
						list3.Add(pendingCompletion);
						VeryLowProfileImageLoader.AddToDownloadedDict(pendingCompletion.Uri);
						VeryLowProfileImageLoader.AddToCache(pendingCompletion.Uri, pendingCompletion.Stream);
						if (VeryLowProfileImageLoader.GetUriSource(pendingCompletion.Image) == pendingCompletion.Uri)
						{
							BitmapImage bitmapImage = VeryLowProfileImageLoader.CreateSizedBitmap(pendingCompletion.Image, pendingCompletion.Uri, VeryLowProfileImageLoader.GetUseBackgroundCreation(pendingCompletion.Image));
							try
							{
								bitmapImage.SetSource(pendingCompletion.Stream);
							}
							catch (Exception e2)
							{
								Logger.Instance.Error("Error of reading image", e2);
							}
							pendingCompletion.Image.Source=(bitmapImage);
							DateTime arg_B8_0 = DateTime.Now;
							VeryLowProfileImageLoader._totalDownloadCount++;
							double totalMilliseconds = (arg_B8_0 - pendingCompletion.Timestamp).TotalMilliseconds;
							VeryLowProfileImageLoader.Log(string.Format("Downloaded image {0} in {1} ms. Totally downloaded = {2}", pendingCompletion.Uri.OriginalString, totalMilliseconds, VeryLowProfileImageLoader._totalDownloadCount));
							if (VeryLowProfileImageLoader._enableLog)
							{
								VeryLowProfileImageLoader._statistics.Add(totalMilliseconds);
								if (VeryLowProfileImageLoader._statistics.Count % 30 == 0 && VeryLowProfileImageLoader._statistics.Count > 0)
								{
									double num5 = Enumerable.Max(VeryLowProfileImageLoader._statistics);
									double num6 = Enumerable.Sum(VeryLowProfileImageLoader._statistics) / (double)VeryLowProfileImageLoader._statistics.Count;
									VeryLowProfileImageLoader.Log(string.Format("Statistics downloading: average {0}, max = {1}", num6, num5));
								}
							}
						}
						pendingCompletion.Stream.Dispose();
					}
					try
					{
						using (List<VeryLowProfileImageLoader.PendingCompletion>.Enumerator enumerator3 = list3.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								VeryLowProfileImageLoader.RemoveRequestInProgress(enumerator3.Current.RequestId);
							}
						}
					}
					catch (Exception)
					{
					}
				});
				continue;
			}
			continue;
		}
//		IL_2B9:
		if (num2 >= VeryLowProfileImageLoader.MaxSimultaneousLoads)
		{
			VeryLowProfileImageLoader.Log("!!!!! noOfCycles=" + num2);
		}
		int count = list.Count;
		int indexOfElementWithMaxPriority = VeryLowProfileImageLoader.GetIndexOfElementWithMaxPriority(list, rand);
		VeryLowProfileImageLoader.PendingRequest pendingRequest3 = list[indexOfElementWithMaxPriority];
		list[indexOfElementWithMaxPriority]= list[count - 1];
		list.RemoveAt(count - 1);
		HttpWebRequest httpWebRequest2 = WebRequest.CreateHttp(pendingRequest3.Uri);
		httpWebRequest2.AllowReadStreamBuffering=(true);
		pendingRequest3.DownloadStaredTimestamp = DateTime.Now;
		httpWebRequest2.BeginGetResponse(new AsyncCallback(VeryLowProfileImageLoader.HandleGetResponseResult), new VeryLowProfileImageLoader.ResponseState(httpWebRequest2, pendingRequest3.Image, pendingRequest3.Uri, pendingRequest3.DownloadStaredTimestamp, pendingRequest3.UniqueId, pendingRequest3.CurrentAttempt));
		VeryLowProfileImageLoader.AddRequestInProgress(pendingRequest3);
		num3++;
		VeryLowProfileImageLoader.Log(string.Concat(new object[]
		{
			"Loading ",
			pendingRequest3.Uri,
			";processing started in ",
			(DateTime.Now - pendingRequest3.CreatedTimstamp).TotalMilliseconds,
			" Currently loading ",
			num3
		}));
		goto IL_3DA;
	}
}

    public static BitmapImage CreateSizedBitmap(FrameworkElement container, Uri uri, bool backgroundCreation = true)
    {
      Dictionary<string, string> queryString = UriExtensions.ParseQueryString(uri);
      double result1 = 0.0;
      double result2 = 0.0;
      if (queryString.ContainsKey("wh"))
      {
        string[] strArray = queryString["wh"].Split((char[]) new char[1]{ '_' });
        if (strArray.Length >= 2)
        {
          double.TryParse(strArray[0], out result1);
          double.TryParse(strArray[1], out result2);
        }
      }
      BitmapImage bitmapImage1;
      if (backgroundCreation)
      {
        BitmapImage bitmapImage2 = new BitmapImage();
        int num1 = 18;
        bitmapImage2.CreateOptions = ((BitmapCreateOptions) num1);
        int num2 = 1;
        bitmapImage2.DecodePixelType=((DecodePixelType) num2);
        bitmapImage1 = bitmapImage2;
      }
      else
      {
        BitmapImage bitmapImage2 = new BitmapImage();
        int num1 = 2;
        bitmapImage2.CreateOptions = ((BitmapCreateOptions) num1);
        int num2 = 1;
        bitmapImage2.DecodePixelType=((DecodePixelType) num2);
        bitmapImage1 = bitmapImage2;
      }
      if (container.Height > 0.0 && container.Width > 0.0)
      {
        if (result1 == 0.0 || result2 == 0.0)
        {
          bitmapImage1.DecodePixelHeight=((int) container.Height);
        }
        else
        {
          Rect fill = RectangleUtils.ResizeToFill(new Rect(0.0, 0.0, container.Width, container.Height), new Size(result1, result2));
          // ISSUE: explicit reference operation
          bitmapImage1.DecodePixelWidth=((int) ((Rect) @fill).Width);
          // ISSUE: explicit reference operation
          bitmapImage1.DecodePixelHeight=((int) ((Rect) @fill).Height);
        }
      }
      else
        bitmapImage1.DecodePixelHeight = 300;
      return bitmapImage1;
    }

    private static void AddToCache(Uri uri, Stream stream)
    {
      VeryLowProfileImageLoader.EnsureLRUCache();
      VeryLowProfileImageLoader._lruCache.Add(uri, (Stream) StreamUtils.ReadFully(stream), true);
      stream.Position = 0L;
      if (UriExtensions.ParseQueryString(uri).ContainsKey(VeryLowProfileImageLoader.REQUIRE_CACHING_KEY))
        ImageCache.Current.TrySetImageForUri(uri.OriginalString, (Stream) StreamUtils.ReadFully(stream));
      stream.Position = 0L;
    }

    public static bool HasImageInLRUCache(Uri uri)
    {
      if (uri ==  null)
        return false;
      VeryLowProfileImageLoader.EnsureLRUCache();
      return VeryLowProfileImageLoader._lruCache.Get(uri) != null;
    }

    public static Stream GetFromLRUCache(Uri uri)
    {
      VeryLowProfileImageLoader.EnsureLRUCache();
      Stream stream = VeryLowProfileImageLoader._lruCache.Get(uri);
      if (stream == null)
        return  null;
      MemoryStream memoryStream = StreamUtils.ReadFully(stream);
      stream.Position = 0L;
      return (Stream) memoryStream;
    }

    private static void EnsureLRUCache()
    {
      if (VeryLowProfileImageLoader._lruCache != null)
        return;
      VeryLowProfileImageLoader._lruCache = new LRUCache<Uri, Stream>(VeryLowProfileImageLoader.LRU_CACHE_CAPACITY, (Func<Stream, int>) (s => (int) s.Length));
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
        string shorterVersion = uri.OriginalString.GetShorterVersion();
        VeryLowProfileImageLoader._downloadedDictionary[shorterVersion] = "";
        VeryLowProfileImageLoader._downloadedList.Add(shorterVersion);
      }
    }

    public static void SaveState()
    {
      lock (VeryLowProfileImageLoader._downloadedDictLock)
      {
        VeryLowProfileImageLoader.SerializedData serializedData = new VeryLowProfileImageLoader.SerializedData() { DownloadedUris = (List<string>) Enumerable.ToList<string>(Enumerable.Take<string>(Enumerable.Skip<string>(VeryLowProfileImageLoader._downloadedList, Math.Max(0, Enumerable.Count<string>(VeryLowProfileImageLoader._downloadedList) - 1000)), 1000)) };
        CacheManager.TrySerialize((IBinarySerializable) serializedData, "VeryLowProfileImageLoaderData", false, CacheManager.DataType.CachedData);
        VeryLowProfileImageLoader.Log("VeryLowProfileImageLoader serialized uri count " + serializedData.DownloadedUris.Count);
      }
    }

    public static void RestoreState()
    {
      lock (VeryLowProfileImageLoader._downloadedDictLock)
      {
        VeryLowProfileImageLoader.SerializedData serializedData = new VeryLowProfileImageLoader.SerializedData();
        CacheManager.TryDeserialize((IBinarySerializable) serializedData, "VeryLowProfileImageLoaderData", CacheManager.DataType.CachedData);
        VeryLowProfileImageLoader._downloadedList = serializedData.DownloadedUris;
        foreach (string downloaded in VeryLowProfileImageLoader._downloadedList)
          VeryLowProfileImageLoader._downloadedDictionary[downloaded] = "";
        VeryLowProfileImageLoader.Log("VeryLowProfileImageLoader deserialized uri count " + serializedData.DownloadedUris.Count);
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
        List<VeryLowProfileImageLoader.PendingRequest> list = Enumerable.ToList<VeryLowProfileImageLoader.PendingRequest>(Enumerable.Where<VeryLowProfileImageLoader.PendingRequest>(VeryLowProfileImageLoader._requestsInProcess.Values, (Func<VeryLowProfileImageLoader.PendingRequest, bool>)(r => (now - r.DownloadStaredTimestamp).TotalMilliseconds > (double)VeryLowProfileImageLoader.STALE_THRESHOLD)));
        int num = 0;
        foreach (VeryLowProfileImageLoader.PendingRequest pendingRequest in (List<VeryLowProfileImageLoader.PendingRequest>) list)
        {
          VeryLowProfileImageLoader.RemoveRequestInProgress(pendingRequest.UniqueId);
          VeryLowProfileImageLoader.Log("REMOVED STALE " + pendingRequest.Uri.OriginalString);
          ++num;
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
      Image image = (Image) o;
      // ISSUE: explicit reference operation
      Uri newValue = (Uri) e.NewValue;
      int num1 = newValue ==  null ? 1 : 0;
      VeryLowProfileImageLoader._hashToUriDict[(image).GetHashCode()] = newValue;
      if (newValue ==  null)
      {
        BitmapImage source = image.Source as BitmapImage;
        image.Source = ( null);
        if (source != null)
          VeryLowProfileImageLoader.DisposeImage(source);
        VeryLowProfileImageLoader.Log("OnUriSourceChanged uri = NULL");
      }
      else
      {
        VeryLowProfileImageLoader.Log(string.Format("OnUriSourceChanged uri = {0}", newValue));
        Stream stream = VeryLowProfileImageLoader.GetFromLRUCache(newValue) ?? ImageCache.Current.GetCachedImageStream(newValue.OriginalString);
        if (stream != null)
        {
          BitmapImage sizedBitmap = VeryLowProfileImageLoader.CreateSizedBitmap((FrameworkElement) image, newValue, VeryLowProfileImageLoader.GetUseBackgroundCreation(image));
          ((BitmapSource) sizedBitmap).SetSource(stream);
          image.Source = ((ImageSource) sizedBitmap);
        }
        else if (!newValue.IsAbsoluteUri || !VeryLowProfileImageLoader.IsEnabled || DesignerProperties.IsInDesignTool)
        {
          BitmapImage bitmapImage1 = new BitmapImage();
          int num2 = 18;
          bitmapImage1.CreateOptions = ((BitmapCreateOptions) num2);
          Uri uri = newValue;
          bitmapImage1.UriSource = uri;
          BitmapImage bitmapImage2 = bitmapImage1;
          image.Source = ((ImageSource) bitmapImage2);
        }
        else
          VeryLowProfileImageLoader.AddPendingRequest(image, newValue, 1);
      }
    }

    private static void DisposeImage(BitmapImage img)
    {
      if (img == null)
        return;
      try
      {
        using (MemoryStream memoryStream = new MemoryStream((byte[]) new byte[1]))
          ((BitmapSource) img).SetSource((Stream) memoryStream);
      }
      catch
      {
      }
    }

    private static void AddPendingRequest(Image image, Uri uri, int currentAttempt)
    {
      if (VeryLowProfileImageLoader.GetCleanupSourceWhenNewUriPending(image))
        image.Source = ( null);
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
        VeryLowProfileImageLoader.ResponseState asyncState = (VeryLowProfileImageLoader.ResponseState) result.AsyncState;
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
        BinarySerializerExtensions.WriteList(writer, this.DownloadedUris);
      }

      public void Read(BinaryReader reader)
      {
        this.DownloadedUris = BinarySerializerExtensions.ReadList(reader);
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
