using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.ImageViewer
{
  public static class ImageViewerLowProfileImageLoader
  {
    private static readonly Thread _thread = new Thread(new ParameterizedThreadStart(ImageViewerLowProfileImageLoader.WorkerThreadProc));
    private static readonly Queue<ImageViewerLowProfileImageLoader.PendingRequest> _pendingRequests = new Queue<ImageViewerLowProfileImageLoader.PendingRequest>();
    private static readonly Queue<IAsyncResult> _pendingResponses = new Queue<IAsyncResult>();
    private static readonly object _syncBlock = new object();
    public static readonly DependencyProperty UriSourceProperty = DependencyProperty.RegisterAttached("UriSource", typeof(Uri), typeof(ImageViewerLowProfileImageLoader), new PropertyMetadata(new PropertyChangedCallback(ImageViewerLowProfileImageLoader.OnUriSourceChanged)));
    private static bool _enableLog = true;
    private const int WorkItemQuantum = 5;
    private static bool _exiting;

    public static bool IsEnabled { get; set; }

    public static event EventHandler ImageDownloaded;

    static ImageViewerLowProfileImageLoader()
    {
      ImageViewerLowProfileImageLoader._thread.Start();
      Application.Current.Exit += (new EventHandler(ImageViewerLowProfileImageLoader.HandleApplicationExit));
      ImageViewerLowProfileImageLoader.IsEnabled = true;
    }

    public static Uri GetUriSource(Image obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      return (Uri) ((DependencyObject) obj).GetValue(ImageViewerLowProfileImageLoader.UriSourceProperty);
    }

    public static void SetUriSource(Image obj, Uri value)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      ((DependencyObject) obj).SetValue(ImageViewerLowProfileImageLoader.UriSourceProperty, value);
    }

    private static void HandleApplicationExit(object sender, EventArgs e)
    {
      ImageViewerLowProfileImageLoader._exiting = true;
      if (!Monitor.TryEnter(ImageViewerLowProfileImageLoader._syncBlock, 100))
        return;
      Monitor.Pulse(ImageViewerLowProfileImageLoader._syncBlock);
      Monitor.Exit(ImageViewerLowProfileImageLoader._syncBlock);
    }

    private static void WorkerThreadProc(object unused)
    {
        Random random = new Random();
        List<ImageViewerLowProfileImageLoader.PendingRequest> pendingRequestList = new List<ImageViewerLowProfileImageLoader.PendingRequest>();
        Queue<IAsyncResult> asyncResultQueue = new Queue<IAsyncResult>();
        while (!ImageViewerLowProfileImageLoader._exiting)
        {
            lock (ImageViewerLowProfileImageLoader._syncBlock)
            {
                if (ImageViewerLowProfileImageLoader._pendingRequests.Count == 0 && ImageViewerLowProfileImageLoader._pendingResponses.Count == 0 && (pendingRequestList.Count == 0 && asyncResultQueue.Count == 0))
                {
                    Monitor.Wait(ImageViewerLowProfileImageLoader._syncBlock);
                    if (ImageViewerLowProfileImageLoader._exiting)
                        break;
                }
                while (0 < ImageViewerLowProfileImageLoader._pendingRequests.Count)
                {
                    ImageViewerLowProfileImageLoader.PendingRequest local_7 = ImageViewerLowProfileImageLoader._pendingRequests.Dequeue();
                    for (int local_8 = 0; local_8 < pendingRequestList.Count; ++local_8)
                    {
                        if (pendingRequestList[local_8].Image == local_7.Image)
                        {
                            pendingRequestList[local_8] = local_7;
                            local_7 = (ImageViewerLowProfileImageLoader.PendingRequest)null;
                            break;
                        }
                    }
                    if (local_7 != null)
                        pendingRequestList.Add(local_7);
                }
                while (0 < ImageViewerLowProfileImageLoader._pendingResponses.Count)
                    asyncResultQueue.Enqueue(ImageViewerLowProfileImageLoader._pendingResponses.Dequeue());
            }
            Queue<ImageViewerLowProfileImageLoader.PendingCompletion> pendingCompletions = new Queue<ImageViewerLowProfileImageLoader.PendingCompletion>();
            int count = pendingRequestList.Count;
            for (int index1 = 0; 0 < count && index1 < 5; ++index1)
            {
                int index2 = random.Next(count);
                ImageViewerLowProfileImageLoader.PendingRequest pendingRequest = pendingRequestList[index2];
                pendingRequestList[index2] = pendingRequestList[count - 1];
                pendingRequestList.RemoveAt(count - 1);
                --count;
                HttpWebRequest http = WebRequest.CreateHttp(pendingRequest.Uri);
                http.AllowReadStreamBuffering = true;
                http.BeginGetResponse(new AsyncCallback(ImageViewerLowProfileImageLoader.HandleGetResponseResult), (object)new ImageViewerLowProfileImageLoader.ResponseState((WebRequest)http, pendingRequest.Image, pendingRequest.Uri, pendingRequest.Timestamp));
                Thread.Sleep(1);
            }
            for (int index = 0; 0 < asyncResultQueue.Count && index < 5; ++index)
            {
                IAsyncResult asyncResult = asyncResultQueue.Dequeue();
                ImageViewerLowProfileImageLoader.ResponseState responseState = (ImageViewerLowProfileImageLoader.ResponseState)asyncResult.AsyncState;
                try
                {
                    WebResponse response = responseState.WebRequest.EndGetResponse(asyncResult);
                    pendingCompletions.Enqueue(new ImageViewerLowProfileImageLoader.PendingCompletion(responseState.Image, responseState.Uri, response.GetResponseStream(), responseState.Timestamp));
                }
                catch (WebException ex)
                {
                    Logger.Instance.Error(string.Format("LowProfileImageLoader exception when fetching {0}", (object)responseState.Uri.OriginalString), (Exception)ex);
                }
                Thread.Sleep(1);
            }
            if (0 < pendingCompletions.Count)
                Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    while (0 < pendingCompletions.Count)
                    {
                        ImageViewerLowProfileImageLoader.PendingCompletion pendingCompletion = pendingCompletions.Dequeue();
                        if (ImageViewerLowProfileImageLoader.GetUriSource(pendingCompletion.Image) == pendingCompletion.Uri)
                        {
                            BitmapImage bitmapImage;
                            if (pendingCompletion.Image.Source == null)
                                bitmapImage = new BitmapImage()
                                {
                                    CreateOptions = BitmapCreateOptions.BackgroundCreation
                                };
                            else
                                bitmapImage = new BitmapImage();
                            try
                            {
                                bitmapImage.SetSource(pendingCompletion.Stream);
                            }
                            catch (Exception ex)
                            {
                                Logger.Instance.Error("Error of reading image", ex);
                            }
                            pendingCompletion.Image.Source = (ImageSource)bitmapImage;
                            DateTime now = DateTime.Now;
                            if (ImageViewerLowProfileImageLoader.ImageDownloaded != null)
                            {
                                ImageViewerLowProfileImageLoader.ImageDownloaded(null, new EventArgs());
                            }
                            ImageViewerLowProfileImageLoader.Log(string.Format("Downloaded image {0} in {1} ms.", (object)pendingCompletion.Uri.OriginalString, (object)(now - pendingCompletion.Timestamp).TotalMilliseconds));
                        }
                        pendingCompletion.Stream.Dispose();
                    }
                }));
        }
    }

    private static void Log(string info)
    {
      if (!ImageViewerLowProfileImageLoader._enableLog)
        return;
      Logger.Instance.Info(info);
    }

    private static void OnUriSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      Image image = (Image) o;
      // ISSUE: explicit reference operation
      Uri newValue = (Uri) e.NewValue;
      if (newValue ==  null)
      {
        image.Source = ( null);
        ImageViewerLowProfileImageLoader.Log("OnUriSourceChanged uri = NULL");
      }
      else
      {
        ImageViewerLowProfileImageLoader.Log(string.Format("OnUriSourceChanged uri = {0}", newValue));
        Stream cachedImageStream = ImageCache.Current.GetCachedImageStream(newValue.OriginalString);
        if (cachedImageStream != null)
        {
          BitmapImage bitmapImage = new BitmapImage();
          bitmapImage.CreateOptions = ((BitmapCreateOptions) 18);
          ((BitmapSource) bitmapImage).SetSource(cachedImageStream);
          image.Source = ((ImageSource) bitmapImage);
          // ISSUE: reference to a compiler-generated field
          if (ImageViewerLowProfileImageLoader.ImageDownloaded == null)
            return;
          // ISSUE: reference to a compiler-generated field
          ImageViewerLowProfileImageLoader.ImageDownloaded(null, new EventArgs());
        }
        else if (!newValue.IsAbsoluteUri || !ImageViewerLowProfileImageLoader.IsEnabled || DesignerProperties.IsInDesignTool)
        {
          newValue.ToString();
          BitmapImage bitmapImage = new BitmapImage();
          bitmapImage.CreateOptions = ((BitmapCreateOptions) 18);
          bitmapImage.UriSource = newValue;
          image.Source = ((ImageSource) bitmapImage);
          // ISSUE: reference to a compiler-generated field
          if (ImageViewerLowProfileImageLoader.ImageDownloaded == null)
            return;
          // ISSUE: reference to a compiler-generated field
          ImageViewerLowProfileImageLoader.ImageDownloaded(null, new EventArgs());
        }
        else
        {
          lock (ImageViewerLowProfileImageLoader._syncBlock)
          {
            ImageViewerLowProfileImageLoader._pendingRequests.Enqueue(new ImageViewerLowProfileImageLoader.PendingRequest(image, newValue));
            Monitor.Pulse(ImageViewerLowProfileImageLoader._syncBlock);
          }
        }
      }
    }

    private static void HandleGetResponseResult(IAsyncResult result)
    {
      lock (ImageViewerLowProfileImageLoader._syncBlock)
      {
        ImageViewerLowProfileImageLoader._pendingResponses.Enqueue(result);
        Monitor.Pulse(ImageViewerLowProfileImageLoader._syncBlock);
      }
    }

    internal static string FormatUriForGalleryPhoto(string albumId, int photoInd)
    {
      return string.Concat(new object[4]{ "/GalleryPhoto/", albumId, "/", photoInd });
    }

    private class PendingRequest
    {
      public Image Image { get; private set; }

      public Uri Uri { get; private set; }

      public DateTime Timestamp { get; private set; }

      public PendingRequest(Image image, Uri uri)
      {
        this.Image = image;
        this.Uri = uri;
        this.Timestamp = DateTime.Now;
      }
    }

    private class ResponseState
    {
      public WebRequest WebRequest { get; private set; }

      public Image Image { get; private set; }

      public Uri Uri { get; private set; }

      public DateTime Timestamp { get; private set; }

      public ResponseState(WebRequest webRequest, Image image, Uri uri, DateTime startedTime)
      {
        this.WebRequest = webRequest;
        this.Image = image;
        this.Uri = uri;
        this.Timestamp = startedTime;
      }
    }

    private class PendingCompletion
    {
      public Image Image { get; private set; }

      public Uri Uri { get; private set; }

      public Stream Stream { get; private set; }

      public DateTime Timestamp { get; private set; }

      public PendingCompletion(Image image, Uri uri, Stream stream, DateTime startedTime)
      {
        this.Image = image;
        this.Uri = uri;
        this.Stream = stream;
        this.Timestamp = startedTime;
      }
    }
  }
}
