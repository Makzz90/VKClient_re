using Microsoft.Phone.Applications.Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.ImageViewer
{
  public class ImageViewer : Canvas
  {
    public static readonly int ANIMATION_DURATION_MS = 250;
    public static readonly int ANIMATION_INOUT_DURATION_MS = 300;
    public static readonly IEasingFunction ANIMATION_EASING;
    public static readonly IEasingFunction ANIMATION_EASING_IN_OUT;
    private static readonly double MARGIN_BETWEEN_IMAGES;
    private static readonly double MOVE_TO_NEXT_VELOCITY_THRESHOLD;
    private static readonly int DURATION_BOUNCING;
    private static readonly int DURATION_MOVE_TO_NEXT;
    //private static readonly double EPS;
    private static readonly double MIN_SCALE;
    private static readonly double MAX_SCALE;
    private static readonly double HIDE_AFTER_VERT_SWIPE_THRESHOLD;
    private static readonly double HIDE_AFTER_VERT_SWIPE_VELOCITY_THRESHOLD;
    //private int dragCount;
    private bool _isInVerticalSwipe;
    private Rect _rectangleFill;
    private ImageViewerMode _mode;
    private bool _isShown;
    private double _maxScale = ImageViewer.MAX_SCALE;
    private bool _allowVerticalSwipe = true;
    private List<Image> _images;
    private ImageAnimator _imageAnimator;
    private Point _first;
    private Point _second;
    private CompositeTransform _previousTransformation;
    private int _currentInd;
    private bool _initializedImages;
    private Func<int, Image> _getImageFunc;
    private Rectangle _blackRectangle;
    private DeviceOrientation _manuallyAppliedOrientation;
    private SupportedPageOrientation _supportedPageOrientation;
    private double _portraitWidth;
    private double _portraitHeight;
    private TextBlock _loadingTextBlock;
    private PhoneApplicationPage _page;
    private bool _loadedAtLeastOnce;
    private DeviceOrientation _previousOrientation;
    private DeviceOrientation _currentOrientation;
    private int _count;
    private Func<int, ImageInfo> _getImageInfoFunc;
    private Action<int, bool> _showHideOriginalImageAction;
    private static int _shownImageViewersCount;
    private FrameworkElement _currentViewControl;
    private Action<int> _setDataContextOnCurrentViewControlAction;
    private bool _initialStatusBarVisibility;
    private bool _isInPinch;
    private bool _showNextPrevious;
    private Rectangle _manipulationRectangle;
    private bool _inDoubleTapAnimation;

    public double HARDCODED_HEIGHT
    {
      get
      {
        return ScaleFactor.GetScaleFactor() == 150 ? 854.0 : 800.0;
      }
    }

    public bool AllowVerticalSwipe
    {
      get
      {
        return this._allowVerticalSwipe;
      }
      set
      {
        this._allowVerticalSwipe = value;
      }
    }

    public bool ChangeIndexBeforeAnimation { get; set; }

    public double MaxScale
    {
      get
      {
        return this._maxScale;
      }
      set
      {
        this._maxScale = value;
      }
    }

    public ImageViewerMode Mode
    {
      get
      {
        return this._mode;
      }
      set
      {
        if (this._mode == value)
          return;
        this._mode = value;
      }
    }

    public bool ForbidResizeInNormalMode { get; set; }

    public Rect RectangleFill
    {
      get
      {
        return this._rectangleFill;
      }
      set
      {
        this._rectangleFill = value;
      }
    }

    public Rect RectangleFillInCurrentImageCoordinates
    {
      get
      {
        Rect fitRectTransformed = this.CurrentImageFitRectTransformed;
        Rect rect1;
        Rect rectangleFill = this.RectangleFill;
        double x = ((Rect) @rectangleFill).X;
        rectangleFill = this.RectangleFill;
        double y = ((Rect) @rectangleFill).Y;
        rectangleFill = this.RectangleFill;
        double width1 = ((Rect) @rectangleFill).Width;
        rectangleFill = this.RectangleFill;
        double height1 = ((Rect) @rectangleFill).Height;
        rect1=new Rect(x, y, width1, height1);
        double num1 = rect1.X - fitRectTransformed.X;
        rect1.X = num1;
        double num2 = rect1.Y - fitRectTransformed.Y;
        rect1.Y = num2;
        ScaleTransform scaleTransform = new ScaleTransform();
        double num3 = 1.0 / this.CurrentImageScale;
        scaleTransform.ScaleX = num3;
        double num4 = 1.0 / this.CurrentImageScale;
        scaleTransform.ScaleY = num4;
        Rect rect2 = rect1;
        Rect rect3 = ((GeneralTransform) scaleTransform).TransformBounds(rect2);
        Size imageSizeSafelyBy = this.GetImageSizeSafelyBy(this._currentInd);
        Point point = new Point();
        Rect imageFitRectOriginal = this.CurrentImageFitRectOriginal;
        double width2 = ((Rect) @imageFitRectOriginal).Width;
        imageFitRectOriginal = this.CurrentImageFitRectOriginal;
        double height2 = ((Rect) @imageFitRectOriginal).Height;
        Size size = new Size(width2, height2);
        return ((GeneralTransform) RectangleUtils.TransformRect(new Rect(point, size), new Rect(new Point(), imageSizeSafelyBy), false)).TransformBounds(rect3);
      }
    }

    public Rect RectangleFillRelative
    {
      get
      {
        Size imageSizeSafelyBy = this.GetImageSizeSafelyBy(this._currentInd);
        Rect imageCoordinates = this.RectangleFillInCurrentImageCoordinates;
        return new Rect(((Rect) @imageCoordinates).X / ((Size) @imageSizeSafelyBy).Width, ((Rect) @imageCoordinates).Y / ((Size) @imageSizeSafelyBy).Height, ((Rect) @imageCoordinates).Width / ((Size) @imageSizeSafelyBy).Width, ((Rect) @imageCoordinates).Height / ((Size) @imageSizeSafelyBy).Height);
      }
    }

    public bool IsInVerticalSwipe
    {
      get
      {
        return this._isInVerticalSwipe;
      }
      private set
      {
        if (this._isInVerticalSwipe == value)
          return;
        this._isInVerticalSwipe = value;
        if (this.IsInVerticalSwipeChanged == null)
          return;
        this.IsInVerticalSwipeChanged();
      }
    }

    public Image CurrentImage
    {
      get
      {
        return this._images[1];
      }
    }

    private CompositeTransform CurrentImageTransform
    {
      get
      {
        return ((UIElement) this.CurrentImage).RenderTransform as CompositeTransform;
      }
    }

    private double CurrentImageScale
    {
      get
      {
        return this.CurrentImageTransform.ScaleX;
      }
    }

    public int CurrentInd
    {
      get
      {
        return this._currentInd;
      }
    }

    private Image OriginalImage
    {
      get
      {
        Func<int, Image> getImageFunc = this._getImageFunc;
        if (getImageFunc == null)
          return  null;
        int currentInd = this._currentInd;
        return getImageFunc(currentInd);
      }
    }

    private double OwnWidth
    {
      get
      {
        return base.ActualWidth;
      }
    }

    public double OwnHeight
    {
      get
      {
        return base.ActualHeight;
      }
    }

    public bool SupportOrientationChange { get; set; }

    public Action HideCallback { get; set; }

    public Action TapCallback { get; set; }

    public Action DoubleTapCallback { get; set; }

    public Action IsInVerticalSwipeChanged { get; set; }

    public Action CurrentIndexChanged { get; set; }

    public Action ManuallyAppliedOrientationChanged { get; set; }

    public DeviceOrientation ManuallyAppliedOrientation
    {
      get
      {
        return this._manuallyAppliedOrientation;
      }
    }

    private PhoneApplicationPage Page
    {
      get
      {
        if (this._page == null)
        {
          FrameworkElement parent = base.Parent as FrameworkElement;
          while (!(parent is PhoneApplicationPage))
            parent = parent.Parent as FrameworkElement;
          this._page = parent as PhoneApplicationPage;
        }
        return this._page;
      }
    }

    public bool ShowNextPrevious
    {
      get
      {
        return this._showNextPrevious;
      }
      set
      {
        this._showNextPrevious = value;
      }
    }

    public Rect CurrentImageFitRectOriginal
    {
      get
      {
        return RectangleUtils.ResizeToFit(new Rect(new Point(), new Size(base.Width, base.Height)), this.GetImageSizeSafelyBy(this.CurrentInd));
      }
    }

    public Rect CurrentImageFitRectTransformed
    {
      get
      {
        return ((GeneralTransform) this.CurrentImageTransform).TransformBounds(this.CurrentImageFitRectOriginal);
      }
    }

    static ImageViewer()
    {
      CubicEase cubicEase = new CubicEase();
      int num1 = 0;
      ((EasingFunctionBase) cubicEase).EasingMode = ((EasingMode) num1);
      VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING = (IEasingFunction) cubicEase;
      QuadraticEase quadraticEase = new QuadraticEase();
      int num2 = 2;
      ((EasingFunctionBase) quadraticEase).EasingMode = ((EasingMode) num2);
      VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING_IN_OUT = (IEasingFunction) quadraticEase;
      VKClient.Common.ImageViewer.ImageViewer.MARGIN_BETWEEN_IMAGES = 12.0;
      VKClient.Common.ImageViewer.ImageViewer.MOVE_TO_NEXT_VELOCITY_THRESHOLD = 100.0;
      VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING = 175;
      VKClient.Common.ImageViewer.ImageViewer.DURATION_MOVE_TO_NEXT = 200;
     // VKClient.Common.ImageViewer.ImageViewer.EPS = 0.0001;
      VKClient.Common.ImageViewer.ImageViewer.MIN_SCALE = 0.5;
      VKClient.Common.ImageViewer.ImageViewer.MAX_SCALE = 4.0;
      VKClient.Common.ImageViewer.ImageViewer.HIDE_AFTER_VERT_SWIPE_THRESHOLD = 100.0;
      VKClient.Common.ImageViewer.ImageViewer.HIDE_AFTER_VERT_SWIPE_VELOCITY_THRESHOLD = 100.0;
    }

    public ImageViewer()
    {
      List<Image> imageList = new List<Image>(3);
      Image image1 = new Image();
      BitmapCache bitmapCache1 = new BitmapCache();
      image1.CacheMode = ((CacheMode)bitmapCache1);
      imageList.Add(image1);
      Image image2 = new Image();
      BitmapCache bitmapCache2 = new BitmapCache();
      ((UIElement) image2).CacheMode = ((CacheMode) bitmapCache2);
      imageList.Add(image2);
      Image image3 = new Image();
      BitmapCache bitmapCache3 = new BitmapCache();
      ((UIElement) image3).CacheMode = ((CacheMode) bitmapCache3);
      imageList.Add(image3);
      this._images = imageList;
      this._showNextPrevious = true;
      //base.\u002Ector();
      base.CacheMode = ((CacheMode) new BitmapCache());
      base.UseOptimizedManipulationRouting = false;
      base.IsHitTestVisible = false;
      this.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.ImageViewer_Tap);
      this.Opacity = 0.0;
      // ISSUE: method pointer
      this.Loaded += new RoutedEventHandler(this.ImageViewer_Loaded);
      this._imageAnimator = new ImageAnimator(VKClient.Common.ImageViewer.ImageViewer.ANIMATION_INOUT_DURATION_MS, VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING_IN_OUT);
    }

    private static string GetImageSource(Image image)
    {
      Uri uriSource = ImageViewerLowProfileImageLoader.GetUriSource(image);
      if (!(uriSource ==  null))
        return uriSource.ToString();
      return "";
    }

    private static void SetImageSource(Image image, object source)
    {
      if (source == null)
      {
        image.Source = ( null);
        ImageViewerLowProfileImageLoader.SetUriSource(image,  null);
      }
      else if (source is BitmapSource)
      {
        image.Source = ((ImageSource) (source as BitmapSource));
      }
      else
      {
        Uri uri = source.ToString().ConvertToUri();
        if (!(VKClient.Common.ImageViewer.ImageViewer.GetImageSource(image).ToString() != uri.ToString()))
          return;
        ImageViewerLowProfileImageLoader.SetUriSource(image, uri);
      }
    }

    public void SetPage(PhoneApplicationPage page)
    {
      this._page = page;
    }

    private void ImageViewer_Loaded(object sender, RoutedEventArgs e)
    {
      if (this._loadedAtLeastOnce)
        return;
      PhoneApplicationPage page = this.Page;
      page.OrientationChanged += (new EventHandler<OrientationChangedEventArgs>(this.page_OrientationChanged));
      this._supportedPageOrientation = page.SupportedOrientations;
      this._portraitWidth = base.Width;
      this._portraitHeight = base.Height;
      this._loadedAtLeastOnce = true;
    }

    private void page_OrientationChanged(object sender, OrientationChangedEventArgs e)
    {
        if (e.Orientation == PageOrientation.Portrait || e.Orientation == PageOrientation.PortraitDown || e.Orientation == PageOrientation.PortraitUp)
        {
            this.Width = this._portraitWidth;
            this.Height = this._portraitHeight;
        }
        else
        {
            this.Width = this._portraitHeight;
            this.Height = this._portraitWidth;
        }
      this.EnsurePrepareImages();
      this.ArrangeImages();
    }

    private void Instance_OrientationChanged(object sender, DeviceOrientationChangedEventArgs e)
    {
      this._previousOrientation = e.PreviousOrientation;
      this._currentOrientation = e.CurrentOrientation;
      this.RespondToOrientationChange();
    }

    private void RespondToOrientationChange()
    {
      ((DependencyObject) Deployment.Current).Dispatcher.BeginInvoke((Action) (() =>
      {
          if (this.Page.SupportedOrientations != SupportedPageOrientation.Portrait)
          return;
        this._manuallyAppliedOrientation = this._currentOrientation == DeviceOrientation.LandscapeLeft || this._previousOrientation == DeviceOrientation.LandscapeLeft && this._currentOrientation != DeviceOrientation.PortraitRightSideUp ? DeviceOrientation.LandscapeLeft : (this._currentOrientation == DeviceOrientation.LandscapeRight || this._previousOrientation == DeviceOrientation.LandscapeRight && this._currentOrientation != DeviceOrientation.PortraitRightSideUp ? DeviceOrientation.LandscapeRight : DeviceOrientation.Unknown);
        if (this.SupportOrientationChange)
        {
          if (this._manuallyAppliedOrientation == DeviceOrientation.LandscapeLeft)
          {
            base.Width = this._portraitHeight;
            base.Height = this._portraitWidth;
            CompositeTransform compositeTransform = base.RenderTransform as CompositeTransform ?? new CompositeTransform();
            compositeTransform.Rotation = 90.0;
            compositeTransform.TranslateY = 0.0;
            compositeTransform.TranslateX = this._portraitWidth;
            base.RenderTransform = ((Transform) compositeTransform);
          }
          else if (this._manuallyAppliedOrientation == DeviceOrientation.LandscapeRight)
          {
            base.Width = this._portraitHeight;
            base.Height = this._portraitWidth;
            CompositeTransform compositeTransform = base.RenderTransform as CompositeTransform ?? new CompositeTransform();
            compositeTransform.Rotation=(-90.0);
            compositeTransform.TranslateX = 0.0;
            compositeTransform.TranslateY = this._portraitHeight;
            base.RenderTransform = ((Transform) compositeTransform);
          }
          else
          {
            base.RenderTransform = ( null);
            base.Width = this._portraitWidth;
            base.Height = this._portraitHeight;
          }
          this.EnsurePrepareImages();
          this.ArrangeImages();
        }
        Action orientationChanged = this.ManuallyAppliedOrientationChanged;
        if (orientationChanged == null)
          return;
        orientationChanged();
      }));
    }

    private void SetDefaultOrientation()
    {
        this.RenderTransform = null;
      this._manuallyAppliedOrientation = DeviceOrientation.Unknown;
      if (this.Page.SupportedOrientations != SupportedPageOrientation.Portrait)
        return;
      this.Width = this._portraitWidth;
      this.Height = this._portraitHeight;
    }

    private void ImageViewer_Tap(object sender,System.Windows.Input.GestureEventArgs e)
    {
        if (this._currentViewControl != null && ((UIElement)this._currentViewControl).Visibility == Visibility.Visible && this._currentViewControl is IHandleTap)
      {
        (this._currentViewControl as IHandleTap).OnTap();
      }
      else
      {
        if (this.TapCallback == null)
          return;
        this.TapCallback();
      }
    }

    private static void UpdateShownCount(bool increment)
    {
      if (increment)
        ++VKClient.Common.ImageViewer.ImageViewer._shownImageViewersCount;
      else if (VKClient.Common.ImageViewer.ImageViewer._shownImageViewersCount > 0)
        --VKClient.Common.ImageViewer.ImageViewer._shownImageViewersCount;
      AccelerometerHelper.Instance.IsActive = (uint) VKClient.Common.ImageViewer.ImageViewer._shownImageViewersCount > 0U;
    }

    public void Initialize(int totalCount, Func<int, ImageInfo> getImageInfoFunc, Func<int, Image> getImageFunc, Action<int, bool> showHideOriginalImageAction, FrameworkElement currentViewControl = null, Action<int> setDataContextOnCurrentViewControlAction = null)
    {
      this._count = totalCount;
      this._getImageInfoFunc = getImageInfoFunc;
      this._getImageFunc = getImageFunc;
      this._showHideOriginalImageAction = showHideOriginalImageAction;
      if (this._currentViewControl != null)
        this.Children.Remove(this._currentViewControl);
      this._currentViewControl = currentViewControl;
      this._setDataContextOnCurrentViewControlAction = setDataContextOnCurrentViewControlAction;
      if (!this._isShown)
        return;
      this.UpdateImagesSources(false, new bool?());
    }

    public Size GetImageSizeSafelyBy(int ind)
    {
      if (ind >= 0 && ind < this._count)
      {
        ImageInfo imageInfo = this._getImageInfoFunc(ind);
        if (imageInfo != null && imageInfo.Width > 0.0 && imageInfo.Height > 0.0)
          return new Size(imageInfo.Width, imageInfo.Height);
      }
      return new Size(100.0, 100.0);
    }

    public void Show(int currentIndex, Action callback = null, bool galleryPhotosMode = false, BitmapImage currentImage = null)
    {
      this._currentInd = currentIndex;
      this.SetCurrentControlDataContextIfApplicable();
      this._isShown = true;
      if (this.CurrentIndexChanged != null)
        this.CurrentIndexChanged();
      this._initialStatusBarVisibility = SystemTray.IsVisible;
      SystemTray.IsVisible = false;
      VKClient.Common.ImageViewer.ImageViewer.UpdateShownCount(true);
      this.EnsurePrepareImages();
      this.ArrangeImages();
      base.Opacity = 1.0;
      if (!galleryPhotosMode)
      {
        VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this.CurrentImage, null);
        Image originalImage = this.OriginalImage;
        if (originalImage != null)
        {
          this._showHideOriginalImageAction(this.CurrentInd, false);
          this.CurrentImage.Source = originalImage.Source;
        }
      }
      else
      {
        if (currentImage == null)
          VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this.CurrentImage, this.GetImageSource(this.CurrentInd, false));
        else
          VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this.CurrentImage, currentImage);
        this._showHideOriginalImageAction(this.CurrentInd, false);
      }
      ((UIElement) this._loadingTextBlock).Opacity = 1.0;
      this._imageAnimator.AnimateIn(this.GetImageSizeSafelyBy(this._currentInd), this.OriginalImage, this.CurrentImage, (Action) (() =>
      {
        base.IsHitTestVisible = true;
        this.UpdateProgressBarVisibility();
        ImageViewerLowProfileImageLoader.ImageDownloaded += new EventHandler(this.ImageViewerLowProfileImageLoader_ImageDownloaded);
        this.UpdateImagesSources(galleryPhotosMode, new bool?());
        this.SetRenderTransformOnCurrentViewControl();
        if (callback == null)
          return;
        callback();
      }), 0);
      ((DependencyObject) this._blackRectangle).Animate(((UIElement) this._blackRectangle).Opacity, 1.0, UIElement.OpacityProperty, VKClient.Common.ImageViewer.ImageViewer.ANIMATION_INOUT_DURATION_MS, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING,  null);
      DeviceOrientationHelper.Instance.OrientationChanged += new EventHandler<DeviceOrientationChangedEventArgs>(this.Instance_OrientationChanged);
    }

    private void ImageViewerLowProfileImageLoader_ImageDownloaded(object sender, EventArgs e)
    {
      this.UpdateProgressBarVisibility();
    }

    private void UpdateProgressBarVisibility()
    {
      ((UIElement) this._loadingTextBlock).Visibility = (this.CurrentImage.Source != null ? Visibility.Collapsed : Visibility.Visible);
    }

    public void Hide(Action callback = null, bool leavingPageImmediately = false)
    {
      int num = this._isShown ? 1 : 0;
      this._isShown = false;
      if (!leavingPageImmediately)
        SystemTray.SetIsVisible((DependencyObject) this.Page, this._initialStatusBarVisibility);
      DeviceOrientationHelper.Instance.OrientationChanged -= new EventHandler<DeviceOrientationChangedEventArgs>(this.Instance_OrientationChanged);
      ImageViewerLowProfileImageLoader.ImageDownloaded -= new EventHandler(this.ImageViewerLowProfileImageLoader_ImageDownloaded);
      if (num != 0)
        VKClient.Common.ImageViewer.ImageViewer.UpdateShownCount(false);
      bool? clockwiseRotation = new bool?();
      if (this.SupportOrientationChange)
      {
        if (this._manuallyAppliedOrientation == DeviceOrientation.LandscapeRight)
          clockwiseRotation = new bool?(true);
        if (this._manuallyAppliedOrientation == DeviceOrientation.LandscapeLeft)
          clockwiseRotation = new bool?(false);
      }
      Image originalImage = this.OriginalImage;
      if (!leavingPageImmediately)
      {
        base.IsHitTestVisible = false;
        if (this._currentViewControl != null)
          ((UIElement) this._currentViewControl).Visibility = Visibility.Collapsed;
        this._imageAnimator.AnimateOut(this.GetImageSizeSafelyBy(this._currentInd), originalImage, this.CurrentImage, clockwiseRotation, (Action) (() =>
        {
          base.Opacity = 0.0;
          this._showHideOriginalImageAction(this.CurrentInd, true);
          this.SetDefaultOrientation();
          this.EnsurePrepareImages();
          this.ArrangeImages();
          this.ResetImageSources();
          if (callback == null)
            return;
          callback();
        }));
        ((DependencyObject) this._blackRectangle).Animate(((UIElement) this._blackRectangle).Opacity, 0.0, UIElement.OpacityProperty, VKClient.Common.ImageViewer.ImageViewer.ANIMATION_INOUT_DURATION_MS, new int?(0),  null,  null);
      }
      else
      {
        if (callback == null)
          return;
        callback();
      }
    }

    public void AnimateToRectangleFill()
    {
      CompositeTransform compositeTransform = RectangleUtils.TransformRect(RectangleUtils.ResizeToFit(new Size(base.Width, base.Height), this.GetImageSizeSafelyBy(this._currentInd)), RectangleUtils.ResizeToFill(this.RectangleFill, this.GetImageSizeSafelyBy(this._currentInd)), false);
      this.AnimateImage(compositeTransform.ScaleX, compositeTransform.ScaleY, compositeTransform.TranslateX, compositeTransform.TranslateY,  null);
    }

    private void i_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
      if (e.PinchManipulation == null)
      {
        Point translation = e.DeltaManipulation.Translation;
        // ISSUE: explicit reference operation
        double x = ((Point) @translation).X;
        translation = e.DeltaManipulation.Translation;
        // ISSUE: explicit reference operation
        double y = ((Point) @translation).Y;
        this.HandleDragDelta(x, y);
      }
      else
      {
        if (this.CurrentImageTransform.ScaleX == 1.0 && (this.CurrentImageTransform.TranslateX != 0.0 || this.CurrentImageTransform.TranslateY != 0.0))
          return;
        if (!this._isInPinch)
        {
          this.HandlePinchStarted(e.PinchManipulation.Original.PrimaryContact, e.PinchManipulation.Original.SecondaryContact);
          this._isInPinch = true;
        }
        this.HandlePinch(e.PinchManipulation.Current.PrimaryContact, e.PinchManipulation.Current.SecondaryContact);
      }
    }

    private void i_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      if (this._inDoubleTapAnimation)
        return;
      e.Handled = true;
      if (this._isInPinch)
      {
        this.HandlePinchCompleted();
        this._isInPinch = false;
      }
      else
      {
        Point linearVelocity1 = e.FinalVelocities.LinearVelocity;
        // ISSUE: explicit reference operation
        double x = ((Point) @linearVelocity1).X;
        Point linearVelocity2 = e.FinalVelocities.LinearVelocity;
        // ISSUE: explicit reference operation
        double y = ((Point) @linearVelocity2).Y;
        this.HandleDragCompleted(x, y);
      }
    }

    private void HandleDragDelta(double hDelta, double vDelta)
    {
      double num1 = hDelta;
      double num2 = vDelta;
      if (this.CurrentImageScale == 1.0 && this._mode == ImageViewerMode.Normal)
      {
        CompositeTransform renderTransform1 = ((UIElement) this.CurrentImage).RenderTransform as CompositeTransform;
        if (renderTransform1.TranslateX == 0.0 && this.AllowVerticalSwipe && (this.IsInVerticalSwipe || num1 == 0.0 && num2 != 0.0 || Math.Abs(num2) / Math.Abs(num1) > 1.2))
        {
          this.IsInVerticalSwipe = true;
          CompositeTransform compositeTransform = renderTransform1;
          double num3 = compositeTransform.TranslateY + num2;
          compositeTransform.TranslateY = num3;
          ((UIElement) this._loadingTextBlock).Opacity = 0.0;
          ((UIElement) this._blackRectangle).Opacity = (Math.Max(0.0, 1.0 - Math.Abs(renderTransform1.TranslateY) / 400.0));
        }
        else
        {
          if (this._currentInd == 0 && num1 > 0.0 && renderTransform1.TranslateX > 0.0 || this._currentInd == this._count - 1 && num1 < 0.0 && renderTransform1.TranslateX < 0.0)
            num1 /= 3.0;
          using (List<Image>.Enumerator enumerator = this._images.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              CompositeTransform renderTransform2 = ((UIElement) enumerator.Current).RenderTransform as CompositeTransform;
              double num3 = renderTransform2.TranslateX + num1;
              renderTransform2.TranslateX = num3;
            }
          }
        }
      }
      else
      {
        CompositeTransform currentImageTransform = this.CurrentImageTransform;
        double num3 = currentImageTransform.TranslateX + num1;
        currentImageTransform.TranslateX = num3;
        double num4 = currentImageTransform.TranslateY + num2;
        currentImageTransform.TranslateY = num4;
      }
    }

    private void HandleDragCompleted(double hVelocity, double vVelocity)
    {
      double num1 = hVelocity;
      double num2 = vVelocity;
      if (this._mode == ImageViewerMode.Normal)
      {
        if (this.CurrentImageScale == 1.0)
        {
          if (this.IsInVerticalSwipe)
          {
            CompositeTransform currentImageTransform = this.CurrentImageTransform;
            if (Math.Abs(currentImageTransform.TranslateY) < VKClient.Common.ImageViewer.ImageViewer.HIDE_AFTER_VERT_SWIPE_THRESHOLD && num2 < VKClient.Common.ImageViewer.ImageViewer.HIDE_AFTER_VERT_SWIPE_VELOCITY_THRESHOLD)
            {
              ((DependencyObject) currentImageTransform).Animate(currentImageTransform.TranslateY, 0.0, CompositeTransform.TranslateYProperty, VKClient.Common.ImageViewer.ImageViewer.ANIMATION_DURATION_MS, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING,  null);
              ((DependencyObject) this._blackRectangle).Animate(((UIElement) this._blackRectangle).Opacity, 1.0, UIElement.OpacityProperty, VKClient.Common.ImageViewer.ImageViewer.ANIMATION_DURATION_MS, new int?(0),  null,  null);
              ((UIElement) this._loadingTextBlock).Opacity = 1.0;
            }
            else
              this.PerformHide();
            this.IsInVerticalSwipe = false;
          }
          else
          {
            bool? moveNext = new bool?();
            double translateX = (((UIElement) this._images[1]).RenderTransform as CompositeTransform).TranslateX;
            double num3 = num1;
            if ((num3 < -VKClient.Common.ImageViewer.ImageViewer.MOVE_TO_NEXT_VELOCITY_THRESHOLD && translateX < 0.0 || translateX <= -base.Width / 2.0) && this._currentInd < this._count - 1)
              moveNext = new bool?(true);
            else if ((num3 > VKClient.Common.ImageViewer.ImageViewer.MOVE_TO_NEXT_VELOCITY_THRESHOLD && translateX > 0.0 || translateX >= base.Width / 2.0) && this._currentInd > 0)
              moveNext = new bool?(false);
            double num4 = 0.0;
            bool? nullable1 = moveNext;
            bool flag1 = true;
            if ((nullable1.GetValueOrDefault() == flag1 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
            {
              num4 = -base.Width - VKClient.Common.ImageViewer.ImageViewer.MARGIN_BETWEEN_IMAGES;
            }
            else
            {
              bool? nullable2 = moveNext;
              bool flag2 = false;
              if ((nullable2.GetValueOrDefault() == flag2 ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
                num4 = base.Width + VKClient.Common.ImageViewer.ImageViewer.MARGIN_BETWEEN_IMAGES;
            }
            double delta = num4 - translateX;
            if (moveNext.HasValue && moveNext.Value)
            {
              ((UIElement) this._images[0]).Visibility = Visibility.Collapsed;
              this.AnimateTwoImagesOnDragComplete(this._images[1], this._images[2], delta, (Action) (() =>
              {
                if (!this.ChangeIndexBeforeAnimation)
                  this.ChangeCurrentInd(moveNext.Value);
                this.MoveToNextOrPrevious(moveNext.Value);
                this.ArrangeImages();
              }), moveNext.HasValue);
              if (!this.ChangeIndexBeforeAnimation)
                return;
              this.ChangeCurrentInd(moveNext.Value);
            }
            else if (moveNext.HasValue && !moveNext.Value)
            {
              ((UIElement) this._images[2]).Visibility = Visibility.Collapsed;
              bool forbidResizeInNormalMode = this.ForbidResizeInNormalMode;
              this.AnimateTwoImagesOnDragComplete(this._images[0], this._images[1], delta, (Action) (() =>
              {
                if (!forbidResizeInNormalMode)
                  this.ChangeCurrentInd(moveNext.Value);
                this.MoveToNextOrPrevious(moveNext.Value);
                this.ArrangeImages();
              }), moveNext.HasValue);
              if (!forbidResizeInNormalMode)
                return;
              this.ChangeCurrentInd(moveNext.Value);
            }
            else
            {
              if (delta == 0.0)
                return;
              this.AnimateImageOnDragComplete(this._images[0], delta,  null, moveNext.HasValue);
              this.AnimateImageOnDragComplete(this._images[1], delta,  null, moveNext.HasValue);
              this.AnimateImageOnDragComplete(this._images[2], delta, new Action(this.ArrangeImages), moveNext.HasValue);
            }
          }
        }
        else
          this.AnimateToEnsureBoundaries();
      }
      else
      {
        if (this._mode != ImageViewerMode.RectangleFill)
          return;
        this.AnimateToEnsureRectangleFill();
      }
    }

    private void AnimateToEnsureBoundaries()
    {
      this.EnsureBoundaries();
    }

    private void AnimateToEnsureRectangleFill()
    {
      this.EnsureBoundaries();
    }

    private void EnsureBoundaries()
    {
      Rect fitRectTransformed = this.CurrentImageFitRectTransformed;
      Rect target = RectangleUtils.AlignRects(this.Mode == ImageViewerMode.RectangleFill ? this.RectangleFill : new Rect(new Point(), new Size(base.Width, base.Height)), fitRectTransformed, this.Mode == ImageViewerMode.RectangleFill);
      if ((target!= fitRectTransformed))
        return;
      CompositeTransform compositeTransform = RectangleUtils.TransformRect(this.CurrentImageFitRectOriginal, target, false);
      this.AnimateImage(compositeTransform.ScaleX, compositeTransform.ScaleY, compositeTransform.TranslateX, compositeTransform.TranslateY,  null);
    }

    private void PerformHide()
    {
      if (this.HideCallback != null)
        this.HideCallback();
      else
        this.Hide( null, false);
    }

    private void AnimateTwoImagesOnDragComplete(Image image1, Image image2, double delta, Action completedCallback, bool movingToNextOrPrevious)
    {
      bool wasHitTestVisible = ((UIElement) this._manipulationRectangle).IsHitTestVisible;
      ((UIElement) this._manipulationRectangle).IsHitTestVisible = false;
      int num = movingToNextOrPrevious ? VKClient.Common.ImageViewer.ImageViewer.DURATION_MOVE_TO_NEXT : VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING;
      List<AnimationInfo> animInfoList = new List<AnimationInfo>();
      CompositeTransform renderTransform1 = ((UIElement) image1).RenderTransform as CompositeTransform;
      CompositeTransform renderTransform2 = ((UIElement) image2).RenderTransform as CompositeTransform;
      animInfoList.Add(new AnimationInfo()
      {
        from = renderTransform1.TranslateX,
        to = renderTransform1.TranslateX + delta,
        propertyPath = CompositeTransform.TranslateXProperty,
        duration = num,
        target = (DependencyObject) renderTransform1,
        easing = VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING
      });
      animInfoList.Add(new AnimationInfo()
      {
        from = renderTransform2.TranslateX,
        to = renderTransform2.TranslateX + delta,
        propertyPath = CompositeTransform.TranslateXProperty,
        duration = num,
        target = (DependencyObject) renderTransform2,
        easing = VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING
      });
      int? startTime = new int?(0);
      Action completed = (Action) (() =>
      {
        ((UIElement) this._manipulationRectangle).IsHitTestVisible = wasHitTestVisible;
        completedCallback();
      });
      AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
    }

    private void AnimateImageOnDragComplete(Image image, double delta, Action completedCallback, bool movingToNextOrPrevious)
    {
      int duration = movingToNextOrPrevious ? VKClient.Common.ImageViewer.ImageViewer.DURATION_MOVE_TO_NEXT : VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING;
      CompositeTransform renderTransform = ((UIElement) image).RenderTransform as CompositeTransform;
      ((DependencyObject) renderTransform).Animate(renderTransform.TranslateX, renderTransform.TranslateX + delta, CompositeTransform.TranslateXProperty, duration, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING, completedCallback);
    }

    private void OnPinchDelta(object sender, PinchGestureEventArgs e)
    {
      this.HandlePinch(e.GetPosition((UIElement) this, 0), e.GetPosition((UIElement) this, 1));
    }

    private void HandlePinch(Point pointFirst, Point pointSecond)
    {
        if (this._mode == ImageViewerMode.Normal && this.ForbidResizeInNormalMode)
            return;
        Point point1 = pointFirst;
        Point point2 = pointSecond;
        double x1 = this._first.X;
        double y1 = this._first.Y;
        double x2 = this._second.X;
        double y2 = this._second.Y;
        double x3 = point1.X;
        double y3 = point1.Y;
        double x4 = point2.X;
        double y4 = point2.Y;
        double x5 = VKClient.Common.ImageViewer.ImageViewer.Clamp(Math.Sqrt(((x4 - x3) * (x4 - x3) + (y4 - y3) * (y4 - y3)) / ((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1))) * this._previousTransformation.ScaleX, VKClient.Common.ImageViewer.ImageViewer.MIN_SCALE / 2.0, this.MaxScale);
        if (x5 < 1.0)
            x5 = VKClient.Common.ImageViewer.ImageViewer.TranslateInterval(x5, VKClient.Common.ImageViewer.ImageViewer.MIN_SCALE / 2.0, 1.0, VKClient.Common.ImageViewer.ImageViewer.MIN_SCALE, 1.0);
        double num1 = x5 / this._previousTransformation.ScaleX;
        double num2 = (x3 - num1 * x1 + (x4 - num1 * x2)) / 2.0;
        double num3 = (y3 - num1 * y1 + (y4 - num1 * y2)) / 2.0;
        CompositeTransform compositeTransform = this.CurrentImage.RenderTransform as CompositeTransform;
        double num4 = num1 * this._previousTransformation.ScaleX;
        compositeTransform.ScaleX = num4;
        double num5 = num1 * this._previousTransformation.ScaleY;
        compositeTransform.ScaleY = num5;
        double num6 = num1 * this._previousTransformation.TranslateX + num2;
        compositeTransform.TranslateX = num6;
        double num7 = num1 * this._previousTransformation.TranslateY + num3;
        compositeTransform.TranslateY = num7;
    }

    private void HandlePinchStarted(Point p1, Point p2)
    {
      if (this._mode == ImageViewerMode.Normal && this.ForbidResizeInNormalMode)
        return;
      this._first = p1;
      this._second = p2;
      CompositeTransform renderTransform = ((UIElement) this.CurrentImage).RenderTransform as CompositeTransform;
      CompositeTransform compositeTransform = new CompositeTransform();
      double translateX = renderTransform.TranslateX;
      compositeTransform.TranslateX = translateX;
      double translateY = renderTransform.TranslateY;
      compositeTransform.TranslateY = translateY;
      double scaleX = renderTransform.ScaleX;
      compositeTransform.ScaleX = scaleX;
      double scaleY = renderTransform.ScaleY;
      compositeTransform.ScaleY = scaleY;
      this._previousTransformation = compositeTransform;
    }

    private void HandlePinchCompleted()
    {
      if (this._mode == ImageViewerMode.Normal && this.ForbidResizeInNormalMode)
        return;
      CompositeTransform renderTransform = ((UIElement) this.CurrentImage).RenderTransform as CompositeTransform;
      if (this.Mode == ImageViewerMode.Normal)
      {
        if (renderTransform.ScaleX < 1.0)
          this.AnimateImage(1.0, 1.0, 0.0, 0.0,  null);
        else
          this.AnimateToEnsureBoundaries();
      }
      else
      {
        if (this.Mode != ImageViewerMode.RectangleFill)
          return;
        this.AnimateToEnsureRectangleFill();
      }
    }

    public void AnimateImage(double toScaleX = 1.0, double toScaleY = 1.0, double toTranslateX = 0.0, double toTranslateY = 0.0, Action completionCallback = null)
    {
      CompositeTransform renderTransform = ((UIElement) this.CurrentImage).RenderTransform as CompositeTransform;
      ((DependencyObject) renderTransform).Animate(renderTransform.ScaleX, toScaleX, CompositeTransform.ScaleXProperty, VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING,  null);
      ((DependencyObject) renderTransform).Animate(renderTransform.ScaleY, toScaleY, CompositeTransform.ScaleYProperty, VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING,  null);
      ((DependencyObject) renderTransform).Animate(renderTransform.TranslateX, toTranslateX, CompositeTransform.TranslateXProperty, VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING,  null);
      ((DependencyObject) renderTransform).Animate(renderTransform.TranslateY, toTranslateY, CompositeTransform.TranslateYProperty, VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING, completionCallback);
    }

    private void ChangeCurrentInd(bool next)
    {
      this._showHideOriginalImageAction(this.CurrentInd, true);
      this._currentInd = !next ? this._currentInd - 1 : this._currentInd + 1;
      this._showHideOriginalImageAction(this.CurrentInd, false);
      if (this.CurrentIndexChanged != null)
        this.CurrentIndexChanged();
      this.SetCurrentControlDataContextIfApplicable();
    }

    private void MoveToNextOrPrevious(bool next)
    {
      if (next)
      {
        VKClient.Common.ImageViewer.ImageViewer.Swap(this._images, 0, 1);
        VKClient.Common.ImageViewer.ImageViewer.Swap(this._images, 1, 2);
      }
      else
      {
        VKClient.Common.ImageViewer.ImageViewer.Swap(this._images, 1, 2);
        VKClient.Common.ImageViewer.ImageViewer.Swap(this._images, 0, 1);
      }
      this.UpdateImagesSources(false, new bool?(next));
      this.UpdateProgressBarVisibility();
    }

    private void ArrangeImages()
    {
      ((UIElement) this._images[0]).Clip=( null);
      ((UIElement) this._images[1]).Clip=( null);
      ((UIElement) this._images[2]).Clip=( null);
      ((UIElement) this._images[0]).Visibility = Visibility.Visible;
      ((UIElement) this._images[1]).Visibility = Visibility.Visible;
      ((UIElement) this._images[2]).Visibility = Visibility.Visible;
      RectangleGeometry rectangleGeometry = new RectangleGeometry();
      Rect rect = new Rect(0.0, 0.0, base.Width, this.HARDCODED_HEIGHT);
      rectangleGeometry.Rect = rect;
      base.Clip=((Geometry) rectangleGeometry);
      ((UIElement) this._images[0]).RenderTransform = ((Transform) new CompositeTransform());
      ((UIElement) this._images[1]).RenderTransform = ((Transform) new CompositeTransform());
      ((UIElement) this._images[2]).RenderTransform = ((Transform) new CompositeTransform());
      for (int index = 0; index < 3; ++index)
      {
        ((FrameworkElement) this._images[index]).Width=(base.Width);
        ((FrameworkElement) this._images[index]).Height=(base.Height);
      }
      ((FrameworkElement) this._loadingTextBlock).Width=(base.Width);
      ((FrameworkElement) this._loadingTextBlock).Margin=(new Thickness(0.0, base.Height / 2.0 - 20.0, 0.0, 0.0));
      ((FrameworkElement) this._blackRectangle).Width=(base.Width);
      ((FrameworkElement) this._blackRectangle).Height = this.HARDCODED_HEIGHT;
      ((FrameworkElement) this._manipulationRectangle).Width=(base.Width);
      ((FrameworkElement) this._manipulationRectangle).Height=(base.Height);
      CompositeTransform renderTransform1 = ((UIElement) this._images[0]).RenderTransform as CompositeTransform;
      double num1 = -(base.Width + VKClient.Common.ImageViewer.ImageViewer.MARGIN_BETWEEN_IMAGES);
      renderTransform1.TranslateX = num1;
      double num2 = 0.0;
      renderTransform1.TranslateY = num2;
      CompositeTransform renderTransform2 = ((UIElement) this._images[1]).RenderTransform as CompositeTransform;
      double num3 = 0.0;
      renderTransform2.TranslateX = num3;
      double num4 = 0.0;
      renderTransform2.TranslateY = num4;
      CompositeTransform renderTransform3 = ((UIElement) this._images[2]).RenderTransform as CompositeTransform;
      double num5 = base.Width + VKClient.Common.ImageViewer.ImageViewer.MARGIN_BETWEEN_IMAGES;
      renderTransform3.TranslateX = num5;
      double num6 = 0.0;
      renderTransform3.TranslateY = num6;
      this.SetRenderTransformOnCurrentViewControl();
    }

    private void SetRenderTransformOnCurrentViewControl()
    {
      if (this._currentViewControl == null)
        return;
      ((UIElement) this._currentViewControl).RenderTransform = (((UIElement) this._images[1]).RenderTransform);
      this._currentViewControl.Width=(((FrameworkElement) this._images[1]).Width);
      this._currentViewControl.Height=(((FrameworkElement) this._images[1]).Height);
    }

    private void UpdateImagesSources(bool keepCurrentAsIs = false, bool? movedForvard = null)
    {
      if (!this.ShowNextPrevious)
      {
        if (!keepCurrentAsIs)
          VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[1], this.GetImageSource(this._currentInd, false));
        VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[0], null);
        VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[2], null);
      }
      else
      {
        if (!keepCurrentAsIs && !movedForvard.HasValue)
          VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[1], this.GetImageSource(this._currentInd, false));
        int num = !movedForvard.HasValue ? 1 : (!movedForvard.HasValue ? 0 : (movedForvard.Value ? 1 : 0));
        if ((!movedForvard.HasValue ? 1 : (!movedForvard.HasValue ? 0 : (!movedForvard.Value ? 1 : 0))) != 0)
        {
          object imageSource = this.GetImageSource(this._currentInd - 1, true);
          VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[0], null);
          VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[0], imageSource);
        }
        if (num == 0)
          return;
        object imageSource1 = this.GetImageSource(this._currentInd + 1, true);
        VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[2], null);
        VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[2], imageSource1);
      }
    }

    private void SetCurrentControlDataContextIfApplicable()
    {
      if (this._currentViewControl == null || this._setDataContextOnCurrentViewControlAction == null)
        return;
      this._setDataContextOnCurrentViewControlAction(this._currentInd);
    }

    private object GetImageSource(int ind, bool allowBackgroundCreation = true)
    {
      if (ind < 0 || ind >= this._count)
        return null;
      ImageInfo imageInfo = this._getImageInfoFunc(ind);
      if (imageInfo != null)
      {
        if (!string.IsNullOrEmpty(imageInfo.Uri))
          return imageInfo.Uri;
        if (imageInfo.GetSourceFunc != null)
          return imageInfo.GetSourceFunc(allowBackgroundCreation);
      }
      return null;
    }

    private void ResetImageSources()
    {
      VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[0], null);
      VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[1], null);
      VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this._images[2], null);
    }

    private void EnsurePrepareImages()
    {
      if (!this._initializedImages)
      {
        Rectangle rectangle1 = new Rectangle();
        double width1 = base.Width;
        ((FrameworkElement) rectangle1).Width = width1;
        double hardcodedHeight = this.HARDCODED_HEIGHT;
        ((FrameworkElement) rectangle1).Height = hardcodedHeight;
        SolidColorBrush solidColorBrush1 = new SolidColorBrush(Colors.Black);
        ((Shape) rectangle1).Fill = ((Brush) solidColorBrush1);
        double num1 = 0.0;
        ((UIElement) rectangle1).Opacity = num1;
        BitmapCache bitmapCache = new BitmapCache();
        ((UIElement) rectangle1).CacheMode = ((CacheMode) bitmapCache);
        int num2 = 0;
        ((UIElement) rectangle1).IsHitTestVisible=(num2 != 0);
        this._blackRectangle = rectangle1;
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this).Children).Add((UIElement) this._blackRectangle);
        TextBlock textBlock = new TextBlock();
        string loading = CommonResources.Loading;
        textBlock.Text = loading;
        SolidColorBrush solidColorBrush2 = new SolidColorBrush(Colors.White);
        textBlock.Foreground = ((Brush) solidColorBrush2);
        int num3 = 0;
        textBlock.TextAlignment=((TextAlignment) num3);
        int num4 = 1;
        ((FrameworkElement) textBlock).VerticalAlignment = ((VerticalAlignment) num4);
        this._loadingTextBlock = textBlock;
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this).Children).Add((UIElement) this._loadingTextBlock);
        using (List<Image>.Enumerator enumerator = this._images.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            Image current = enumerator.Current;
            ((FrameworkElement) current).Width=(base.Width);
            ((FrameworkElement) current).Height=(base.Height);
            ((UIElement) current).RenderTransform = ((Transform) new CompositeTransform());
            ((PresentationFrameworkCollection<UIElement>) ((Panel) this).Children).Add((UIElement) current);
          }
        }
        Rectangle rectangle2 = new Rectangle();
        double width2 = base.Width;
        ((FrameworkElement) rectangle2).Width = width2;
        double height = base.Height;
        ((FrameworkElement) rectangle2).Height = height;
        SolidColorBrush solidColorBrush3 = new SolidColorBrush(Colors.Transparent);
        ((Shape) rectangle2).Fill = ((Brush) solidColorBrush3);
        this._manipulationRectangle = rectangle2;
        this._manipulationRectangle.DoubleTap += new EventHandler<System.Windows.Input.GestureEventArgs>(this._manipulationRectangle_DoubleTap);
        this._manipulationRectangle.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this.i_ManipulationDelta);
        this._manipulationRectangle.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this.i_ManipulationCompleted);
        this.Children.Add((UIElement)this._manipulationRectangle);
        this._initializedImages = true;
      }
      if (this._currentViewControl == null || ((PresentationFrameworkCollection<UIElement>) ((Panel) this).Children).Contains((UIElement) this._currentViewControl))
        return;
      this._currentViewControl.Width=(base.Width);
      this._currentViewControl.Height=(base.Height);
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this).Children).Insert(((PresentationFrameworkCollection<UIElement>) ((Panel) this).Children).Count - 1, (UIElement) this._currentViewControl);
    }

    private void _manipulationRectangle_DoubleTap(object sender,System.Windows.Input.GestureEventArgs e)
    {
      if (this.DoubleTapCallback != null)
        this.DoubleTapCallback();
      Point position = e.GetPosition((UIElement) this.CurrentImage);
      if (this.Mode != ImageViewerMode.Normal || this.ForbidResizeInNormalMode)
        return;
      this._inDoubleTapAnimation = true;
      e.Handled = true;
      if (this.CurrentImageScale == 1.0)
      {
        Rect imageFitRectOriginal = this.CurrentImageFitRectOriginal;
        CompositeTransform compositeTransform1 = new CompositeTransform();
        double num1 = 2.0;
        compositeTransform1.ScaleX = num1;
        double num2 = 2.0;
        compositeTransform1.ScaleY = num2;
        // ISSUE: explicit reference operation
        double num3 = -((Point) @position).X;
        compositeTransform1.TranslateX = num3;
        // ISSUE: explicit reference operation
        double num4 = -((Point) @position).Y;
        compositeTransform1.TranslateY = num4;
        Rect target = RectangleUtils.AlignRects(new Rect(new Point(), new Size(base.Width, base.Height)), ((GeneralTransform) compositeTransform1).TransformBounds(imageFitRectOriginal), false);
        CompositeTransform compositeTransform2 = RectangleUtils.TransformRect(imageFitRectOriginal, target, false);
        this.AnimateImage(compositeTransform2.ScaleX, compositeTransform2.ScaleY, compositeTransform2.TranslateX, compositeTransform2.TranslateY, (Action) (() => this._inDoubleTapAnimation = false));
      }
      else
        this.AnimateImage(1.0, 1.0, 0.0, 0.0, (Action) (() => this._inDoubleTapAnimation = false));
    }

    private Rect GetCurrentImageRect()
    {
        Size imageSizeSafelyBy = this.GetImageSizeSafelyBy(this._currentInd);
        Rect rect = new Rect();
        if ((this.OwnHeight == 0.0 || imageSizeSafelyBy.Height == 0.0 ? 0 : (this.OwnWidth / this.OwnHeight <= imageSizeSafelyBy.Width / imageSizeSafelyBy.Height ? 1 : 0)) != 0)
        {
            rect.Width = this.OwnWidth;
            rect.Height = imageSizeSafelyBy.Width == 0.0 ? 0.0 : imageSizeSafelyBy.Height * this.OwnWidth / imageSizeSafelyBy.Width;
            rect.Y = this.OwnHeight / 2.0 - rect.Height / 2.0;
        }
        else
        {
            rect.Height = this.OwnHeight;
            double height = imageSizeSafelyBy.Height;
            rect.Width = imageSizeSafelyBy.Width * this.OwnHeight / imageSizeSafelyBy.Height;
            rect.X = this.OwnWidth / 2.0 - rect.Width / 2.0;
        }
        rect = (this.CurrentImage.RenderTransform as CompositeTransform).TransformBounds(rect);
        return rect;
    }

    private static string GetItemSafe(List<string> list, int ind)
    {
      if (ind >= 0 && ind < list.Count)
        return list[ind];
      return  null;
    }

    private static void Swap(List<Image> images, int ind1, int ind2)
    {
      Image image = images[ind1];
      images[ind1] = images[ind2];
      images[ind2] = image;
    }

    private static double Clamp(double val, double min, double max)
    {
      if (val <= min)
        return min;
      if (val >= max)
        return max;
      return val;
    }

    private static double TranslateInterval(double x, double a, double b, double toA, double toB)
    {
      return (toB * (x - a) + toA * (b - x)) / (b - a);
    }

    public void ResetOrientation()
    {
      this.SupportOrientationChange = !AppGlobalStateManager.Current.GlobalState.IsPhotoViewerOrientationLocked;
      if (this.SupportOrientationChange)
        this.RespondToOrientationChange();
      else
        this.SetDefaultOrientation();
      this.EnsurePrepareImages();
      this.ArrangeImages();
    }
  }
}
