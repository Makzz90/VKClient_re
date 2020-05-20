using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace XamlAnimatedGif
{
  public static class AnimationBehavior
  {
      public static readonly DependencyProperty SourceUriProperty = DependencyProperty.RegisterAttached("SourceUri", typeof(Uri), typeof(AnimationBehavior), new PropertyMetadata(null, new PropertyChangedCallback( AnimationBehavior.SourceChanged)));
      public static readonly DependencyProperty SourceStreamProperty = DependencyProperty.RegisterAttached("SourceStream", typeof(Stream), typeof(AnimationBehavior), new PropertyMetadata(null, new PropertyChangedCallback( AnimationBehavior.SourceChanged)));
      public static readonly DependencyProperty RepeatBehaviorProperty = DependencyProperty.RegisterAttached("RepeatBehavior", typeof(RepeatBehavior), typeof(AnimationBehavior), new PropertyMetadata(null, new PropertyChangedCallback( AnimationBehavior.SourceChanged)));
    public static readonly DependencyProperty AutoStartProperty = DependencyProperty.RegisterAttached("AutoStart", typeof (bool), typeof (AnimationBehavior), new PropertyMetadata(true));
    public static readonly DependencyProperty AnimateInDesignModeProperty = DependencyProperty.RegisterAttached("AnimateInDesignMode", typeof(bool), typeof(AnimationBehavior), new PropertyMetadata(false, new PropertyChangedCallback( AnimationBehavior.AnimateInDesignModeChanged)));
    public static readonly DependencyProperty AnimatorProperty = DependencyProperty.RegisterAttached("Animator", typeof (Animator), typeof (AnimationBehavior), new PropertyMetadata( null));
    private static readonly DependencyProperty SeqNumProperty = DependencyProperty.RegisterAttached("SeqNum", typeof (int), typeof (AnimationBehavior), new PropertyMetadata(0));
    private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public static event EventHandler<AnimationErrorEventArgs> Error;

    public static event EventHandler Loaded;

    public static event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;

    public static Uri GetSourceUri(Image image)
    {
      return (Uri) ((DependencyObject) image).GetValue(AnimationBehavior.SourceUriProperty);
    }

    public static void SetSourceUri(Image image, Uri value)
    {
      ((DependencyObject) image).SetValue(AnimationBehavior.SourceUriProperty, value);
    }

    public static Stream GetSourceStream(DependencyObject obj)
    {
      return (Stream) obj.GetValue(AnimationBehavior.SourceStreamProperty);
    }

    public static void SetSourceStream(DependencyObject obj, Stream value)
    {
      obj.SetValue(AnimationBehavior.SourceStreamProperty, value);
    }

    public static RepeatBehavior GetRepeatBehavior(DependencyObject obj)
    {
      return (RepeatBehavior) obj.GetValue(AnimationBehavior.RepeatBehaviorProperty);
    }

    public static void SetRepeatBehavior(DependencyObject obj, RepeatBehavior value)
    {
      obj.SetValue(AnimationBehavior.RepeatBehaviorProperty, value);
    }

    public static bool GetAutoStart(DependencyObject obj)
    {
      return (bool) obj.GetValue(AnimationBehavior.AutoStartProperty);
    }

    public static void SetAutoStart(DependencyObject obj, bool value)
    {
      obj.SetValue(AnimationBehavior.AutoStartProperty, value);
    }

    public static bool GetAnimateInDesignMode(DependencyObject obj)
    {
      return (bool) obj.GetValue(AnimationBehavior.AnimateInDesignModeProperty);
    }

    public static void SetAnimateInDesignMode(DependencyObject obj, bool value)
    {
      obj.SetValue(AnimationBehavior.AnimateInDesignModeProperty, value);
    }

    public static Animator GetAnimator(DependencyObject obj)
    {
      return (Animator) obj.GetValue(AnimationBehavior.AnimatorProperty);
    }

    private static void SetAnimator(DependencyObject obj, Animator value)
    {
      obj.SetValue(AnimationBehavior.AnimatorProperty, value);
    }

    internal static void OnError(Image image, Exception exception, AnimationErrorKind kind)
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler<AnimationErrorEventArgs> error = AnimationBehavior.Error;
      if (error == null)
        return;
      Image image1 = image;
      AnimationErrorEventArgs e = new AnimationErrorEventArgs(exception, kind);
      error(image1, e);
    }

    private static void OnLoaded(Image sender)
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler loaded = AnimationBehavior.Loaded;
      if (loaded == null)
        return;
      Image image = sender;
      EventArgs empty = EventArgs.Empty;
      loaded(image, empty);
    }

    private static int GetSeqNum(DependencyObject obj)
    {
      return (int) obj.GetValue(AnimationBehavior.SeqNumProperty);
    }

    private static void SetSeqNum(DependencyObject obj, int value)
    {
      obj.SetValue(AnimationBehavior.SeqNumProperty, value);
    }

    private static void SourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      Image image = o as Image;
      if (image == null)
        return;
      AnimationBehavior.InitAnimation(image);
    }

    private static void AnimateInDesignModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      Image image = d as Image;
      if (image == null)
        return;
      AnimationBehavior.InitAnimation(image);
    }

    private static bool CheckDesignMode(Image image, Uri sourceUri, Stream sourceStream)
    {
      if (!AnimationBehavior.IsInDesignMode((DependencyObject) image) || AnimationBehavior.GetAnimateInDesignMode((DependencyObject) image))
        return true;
      BitmapImage bitmapImage = new BitmapImage();
      if (sourceStream != null)
        ((BitmapSource) bitmapImage).SetSource(sourceStream);
      else if (sourceUri !=  null)
        bitmapImage.UriSource = sourceUri;
      image.Source=((ImageSource) bitmapImage);
      return false;
    }

    private static void InitAnimation(Image image)
    {
      if (AnimationBehavior.IsLoaded((FrameworkElement) image))
      {
        // ISSUE: method pointer
          ((FrameworkElement)image).Unloaded+=(new RoutedEventHandler( AnimationBehavior.Image_Unloaded));
        if (AnimationBehavior._cancellationTokenSource != null)
        {
          AnimationBehavior._cancellationTokenSource.Cancel();
          AnimationBehavior._cancellationTokenSource.Dispose();
          AnimationBehavior._cancellationTokenSource =  null;
        }
        AnimationBehavior._cancellationTokenSource = new CancellationTokenSource();
        int seqNum = AnimationBehavior.GetSeqNum((DependencyObject) image) + 1;
        AnimationBehavior.SetSeqNum((DependencyObject) image, seqNum);
        image.Source=( null);
        AnimationBehavior.ClearAnimatorCore(image);
        try
        {
          Stream sourceStream = AnimationBehavior.GetSourceStream((DependencyObject) image);
          if (sourceStream != null)
          {
            AnimationBehavior.InitAnimationAsync(image, sourceStream, AnimationBehavior.GetRepeatBehavior((DependencyObject) image), seqNum);
          }
          else
          {
            Uri absoluteUri = AnimationBehavior.GetAbsoluteUri(image);
            if (!(absoluteUri !=  null))
              return;
            AnimationBehavior.InitAnimationAsync(image, absoluteUri, AnimationBehavior._cancellationTokenSource.Token, AnimationBehavior.GetRepeatBehavior((DependencyObject) image), seqNum);
          }
        }
        catch (Exception ex)
        {
          AnimationBehavior.OnError(image, ex, AnimationErrorKind.Loading);
        }
      }
      else
      {
        // ISSUE: method pointer
          ((FrameworkElement)image).Loaded+=(new RoutedEventHandler( AnimationBehavior.Image_Loaded));
      }
    }

    private static void Image_Loaded(object sender, RoutedEventArgs e)
    {
      Image image = (Image) sender;
      // ISSUE: method pointer
      RoutedEventHandler routedEventHandler = new RoutedEventHandler( AnimationBehavior.Image_Loaded);
      ((FrameworkElement) image).Loaded+=(routedEventHandler);
      AnimationBehavior.InitAnimation(image);
    }

    private static void Image_Unloaded(object sender, RoutedEventArgs e)
    {
      Image image = (Image) sender;
      // ISSUE: method pointer
      RoutedEventHandler routedEventHandler1 = new RoutedEventHandler( AnimationBehavior.Image_Unloaded);
      ((FrameworkElement) image).Unloaded-=(routedEventHandler1);
      // ISSUE: method pointer
      RoutedEventHandler routedEventHandler2 = new RoutedEventHandler( AnimationBehavior.Image_Loaded);
      ((FrameworkElement)image).Loaded += (routedEventHandler2);
      AnimationBehavior.ClearAnimatorCore(image);
    }

    private static bool IsLoaded(FrameworkElement element)
    {
      return VisualTreeHelper.GetParent((DependencyObject) element) != null;
    }

    private static Uri GetAbsoluteUri(Image image)
    {
      Uri relativeUri = AnimationBehavior.GetSourceUri(image);
      if (relativeUri ==  null)
        return  null;
      if (!relativeUri.IsAbsoluteUri)
      {
        Uri baseUri = new Uri("");
        if (!(baseUri !=  null))
          throw new InvalidOperationException("Relative URI can't be resolved");
        relativeUri = new Uri(baseUri, relativeUri);
      }
      return relativeUri;
    }

    private static async void InitAnimationAsync(Image image, Uri sourceUri, CancellationToken cancellationToken, RepeatBehavior repeatBehavior, int seqNum)
    {
      if (!AnimationBehavior.CheckDesignMode(image, sourceUri,  null))
        return;
      try
      {
        // ISSUE: reference to a compiler-generated field
        Animator.DownloadProgressChanged += AnimationBehavior.DownloadProgressChanged;
        Animator async = await Animator.CreateAsync(image, sourceUri, cancellationToken, repeatBehavior);
        // ISSUE: reference to a compiler-generated field
        Animator.DownloadProgressChanged -= AnimationBehavior.DownloadProgressChanged;
        if (async == null || AnimationBehavior.GetSeqNum((DependencyObject) image) != seqNum)
        {
          if (async == null)
            return;
          // ISSUE: explicit non-virtual call
          async.Dispose();
        }
        else
        {
          AnimationBehavior.SetAnimatorCore(image, async);
          AnimationBehavior.OnLoaded(image);
        }
      }
      catch (Exception ex)
      {
        // ISSUE: reference to a compiler-generated field
        Animator.DownloadProgressChanged -= AnimationBehavior.DownloadProgressChanged;
        AnimationBehavior.OnError(image, ex, AnimationErrorKind.Loading);
      }
    }

    private static async void InitAnimationAsync(Image image, Stream stream, RepeatBehavior repeatBehavior, int seqNum)
    {
      if (!AnimationBehavior.CheckDesignMode(image,  null, stream))
        return;
      try
      {
        Animator async = await Animator.CreateAsync(image, stream, repeatBehavior);
        AnimationBehavior.SetAnimatorCore(image, async);
        if (AnimationBehavior.GetSeqNum((DependencyObject) image) != seqNum)
          async.Dispose();
        else
          AnimationBehavior.OnLoaded(image);
      }
      catch (Exception ex)
      {
        AnimationBehavior.OnError(image, ex, AnimationErrorKind.Loading);
      }
    }

    private static void SetAnimatorCore(Image image, Animator animator)
    {
      AnimationBehavior.SetAnimator((DependencyObject) image, animator);
      image.Source=((ImageSource) animator.Bitmap);
      if (AnimationBehavior.GetAutoStart((DependencyObject) image))
        animator.Play();
      else
        animator.ShowFirstFrame();
    }

    private static void ClearAnimatorCore(Image image)
    {
      Animator animator = AnimationBehavior.GetAnimator((DependencyObject) image);
      if (animator == null)
        return;
      animator.Dispose();
      AnimationBehavior.SetAnimator((DependencyObject) image,  null);
    }

    private static bool IsInDesignMode(DependencyObject obj)
    {
      return DesignerProperties.GetIsInDesignMode(obj);
    }

    public static async Task ClearGifCacheAsync()
    {
      await new UriLoader().ClearCache();
    }

    public static void CancelLoading()
    {
      if (AnimationBehavior._cancellationTokenSource == null || AnimationBehavior._cancellationTokenSource.IsCancellationRequested)
        return;
      AnimationBehavior._cancellationTokenSource.Cancel();
      AnimationBehavior._cancellationTokenSource =  null;
    }
  }
}
