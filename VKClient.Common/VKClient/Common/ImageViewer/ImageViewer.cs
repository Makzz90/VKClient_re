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
                Rect currentImageFitRectTransformed = this.CurrentImageFitRectTransformed;
                Rect rect = new Rect(this.RectangleFill.X, this.RectangleFill.Y, this.RectangleFill.Width, this.RectangleFill.Height);
                rect.X = rect.X - currentImageFitRectTransformed.X;
                rect.Y = rect.Y - currentImageFitRectTransformed.Y;
                ScaleTransform expr_79 = new ScaleTransform();
                expr_79.ScaleX = 1.0 / this.CurrentImageScale;
                expr_79.ScaleY = 1.0 / this.CurrentImageScale;
                Rect rect2 = expr_79.TransformBounds(rect);
                Size imageSizeSafelyBy = this.GetImageSizeSafelyBy(this._currentInd);
                return RectangleUtils.TransformRect(new Rect(default(Point), new Size(this.CurrentImageFitRectOriginal.Width, this.CurrentImageFitRectOriginal.Height)), new Rect(default(Point), imageSizeSafelyBy), false).TransformBounds(rect2);
            }
        }

        public Rect RectangleFillRelative
        {
            get
            {
                Size imageSizeSafelyBy = this.GetImageSizeSafelyBy(this._currentInd);
                Rect imageCoordinates = this.RectangleFillInCurrentImageCoordinates;
                return new Rect(imageCoordinates.X / imageSizeSafelyBy.Width, imageCoordinates.Y / imageSizeSafelyBy.Height, imageCoordinates.Width / imageSizeSafelyBy.Width, imageCoordinates.Height / imageSizeSafelyBy.Height);
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
                return this.CurrentImage.RenderTransform as CompositeTransform;
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
                Func<int, Image> func = this._getImageFunc;
                if (func == null)
                    return (Image)null;
                int num = this._currentInd;
                return func(num);
            }
        }

        private double OwnWidth
        {
            get
            {
                return this.ActualWidth;
            }
        }

        public double OwnHeight
        {
            get
            {
                return this.ActualHeight;
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
                    FrameworkElement frameworkElement = this.Parent as FrameworkElement;
                    while (!(frameworkElement is PhoneApplicationPage))
                        frameworkElement = frameworkElement.Parent as FrameworkElement;
                    this._page = frameworkElement as PhoneApplicationPage;
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
                return RectangleUtils.ResizeToFit(new Rect(new Point(), new Size(this.Width, this.Height)), this.GetImageSizeSafelyBy(this.CurrentInd));
            }
        }

        public Rect CurrentImageFitRectTransformed
        {
            get
            {
                return this.CurrentImageTransform.TransformBounds(this.CurrentImageFitRectOriginal);
            }
        }

        static ImageViewer()
        {
            CubicEase cubicEase = new CubicEase();
            cubicEase.EasingMode = EasingMode.EaseOut;
            VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING = (IEasingFunction)cubicEase;
            QuadraticEase quadraticEase = new QuadraticEase();
            quadraticEase.EasingMode = EasingMode.EaseInOut;
            VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING_IN_OUT = (IEasingFunction)quadraticEase;
            VKClient.Common.ImageViewer.ImageViewer.MARGIN_BETWEEN_IMAGES = 12.0;
            VKClient.Common.ImageViewer.ImageViewer.MOVE_TO_NEXT_VELOCITY_THRESHOLD = 100.0;
            VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING = 175;
            VKClient.Common.ImageViewer.ImageViewer.DURATION_MOVE_TO_NEXT = 200;
            //VKClient.Common.ImageViewer.ImageViewer.EPS = 0.0001;
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
            image1.CacheMode = (CacheMode)bitmapCache1;
            imageList.Add(image1);
            Image image2 = new Image();
            BitmapCache bitmapCache2 = new BitmapCache();
            image2.CacheMode = (CacheMode)bitmapCache2;
            imageList.Add(image2);
            Image image3 = new Image();
            BitmapCache bitmapCache3 = new BitmapCache();
            image3.CacheMode = (CacheMode)bitmapCache3;
            imageList.Add(image3);
            this._images = imageList;
            this._showNextPrevious = true;
            
            this.CacheMode = (CacheMode)new BitmapCache();
            this.UseOptimizedManipulationRouting = false;
            this.IsHitTestVisible = false;
            this.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(this.ImageViewer_Tap);
            this.Opacity = 0.0;
            this.Loaded += new RoutedEventHandler(this.ImageViewer_Loaded);
            this._imageAnimator = new ImageAnimator(VKClient.Common.ImageViewer.ImageViewer.ANIMATION_INOUT_DURATION_MS, VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING_IN_OUT);
        }

        private static string GetImageSource(Image image)
        {
            Uri uriSource = ImageViewerLowProfileImageLoader.GetUriSource(image);
            if (!(uriSource == null))
                return uriSource.ToString();
            return "";
        }

        private static void SetImageSource(Image image, object source)
        {
            if (source == null)
            {
                image.Source = null;
                ImageViewerLowProfileImageLoader.SetUriSource(image, null);
            }
            else if (source is BitmapSource)
            {
                image.Source = (ImageSource)(source as BitmapSource);
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
            page.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(this.page_OrientationChanged);
            this._supportedPageOrientation = page.SupportedOrientations;
            this._portraitWidth = this.Width;
            this._portraitHeight = this.Height;
            this._loadedAtLeastOnce = true;
        }

        private void page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (!this.SupportOrientationChange)
                return;
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
            Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!this.SupportOrientationChange || this.Page.SupportedOrientations != SupportedPageOrientation.Portrait)
                    return;
                if (e.CurrentOrientation == DeviceOrientation.LandscapeLeft || e.PreviousOrientation == DeviceOrientation.LandscapeLeft && e.CurrentOrientation != DeviceOrientation.PortraitRightSideUp)
                {
                    this.Width = this._portraitHeight;
                    this.Height = this._portraitWidth;
                    CompositeTransform compositeTransform = this.RenderTransform is CompositeTransform ? this.RenderTransform as CompositeTransform : new CompositeTransform();
                    compositeTransform.Rotation = 90.0;
                    compositeTransform.TranslateY = 0.0;
                    compositeTransform.TranslateX = this._portraitWidth;
                    this.RenderTransform = (Transform)compositeTransform;
                    this._manuallyAppliedOrientation = DeviceOrientation.LandscapeLeft;
                }
                else if (e.CurrentOrientation == DeviceOrientation.LandscapeRight || e.PreviousOrientation == DeviceOrientation.LandscapeRight && e.CurrentOrientation != DeviceOrientation.PortraitRightSideUp)
                {
                    this.Width = this._portraitHeight;
                    this.Height = this._portraitWidth;
                    CompositeTransform compositeTransform = this.RenderTransform is CompositeTransform ? this.RenderTransform as CompositeTransform : new CompositeTransform();
                    compositeTransform.Rotation = -90.0;
                    compositeTransform.TranslateX = 0.0;
                    compositeTransform.TranslateY = this._portraitHeight;
                    this.RenderTransform = (Transform)compositeTransform;
                    this._manuallyAppliedOrientation = DeviceOrientation.LandscapeRight;
                }
                else
                    this.SetDefaultOrientation();
                if (this.ManuallyAppliedOrientationChanged != null)
                    this.ManuallyAppliedOrientationChanged();
                this.EnsurePrepareImages();
                this.ArrangeImages();
            }));
        }

        private void SetDefaultOrientation()
        {
            this.RenderTransform = (Transform)null;
            this._manuallyAppliedOrientation = DeviceOrientation.Unknown;
            if (this.Page.SupportedOrientations != SupportedPageOrientation.Portrait)
                return;
            this.Width = this._portraitWidth;
            this.Height = this._portraitHeight;
        }

        private void ImageViewer_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this._currentViewControl != null && this._currentViewControl.Visibility == Visibility.Visible && this._currentViewControl is IHandleTap)
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
            if (VKClient.Common.ImageViewer.ImageViewer._shownImageViewersCount == 0)
                AccelerometerHelper.Instance.IsActive = false;
            else
                AccelerometerHelper.Instance.IsActive = true;
        }

        public void Initialize(int totalCount, Func<int, ImageInfo> getImageInfoFunc, Func<int, Image> getImageFunc, Action<int, bool> showHideOriginalImageAction, FrameworkElement currentViewControl = null, Action<int> setDataContextOnCurrentViewControlAction = null)
        {
            this._count = totalCount;
            this._getImageInfoFunc = getImageInfoFunc;
            this._getImageFunc = getImageFunc;
            this._showHideOriginalImageAction = showHideOriginalImageAction;
            if (this._currentViewControl != null)
                this.Children.Remove((UIElement)this._currentViewControl);
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
            this.Opacity = 1.0;
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
                    VKClient.Common.ImageViewer.ImageViewer.SetImageSource(this.CurrentImage, (object)currentImage);
                this._showHideOriginalImageAction(this.CurrentInd, false);
            }
            this._loadingTextBlock.Opacity = 1.0;
            this._imageAnimator.AnimateIn(this.GetImageSizeSafelyBy(this._currentInd), this.OriginalImage, this.CurrentImage, (Action)(() =>
            {
                this.IsHitTestVisible = true;
                this.UpdateProgressBarVisibility();
                ImageViewerLowProfileImageLoader.ImageDownloaded += new EventHandler(this.ImageViewerLowProfileImageLoader_ImageDownloaded);
                this.UpdateImagesSources(galleryPhotosMode, new bool?());
                this.SetRenderTransformOnCurrentViewControl();
                if (callback == null)
                    return;
                callback();
            }), 0);
            this._blackRectangle.Animate(this._blackRectangle.Opacity, 1.0, (object)UIElement.OpacityProperty, VKClient.Common.ImageViewer.ImageViewer.ANIMATION_INOUT_DURATION_MS, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING, null);
            DeviceOrientationHelper.Instance.OrientationChanged += new EventHandler<DeviceOrientationChangedEventArgs>(this.Instance_OrientationChanged);
        }

        private void ImageViewerLowProfileImageLoader_ImageDownloaded(object sender, EventArgs e)
        {
            this.UpdateProgressBarVisibility();
        }

        private void UpdateProgressBarVisibility()
        {
            this._loadingTextBlock.Visibility = this.CurrentImage.Source != null ? Visibility.Collapsed : Visibility.Visible;
        }

        public void Hide(Action callback = null, bool leavingPageImmediately = false)
        {
            this._isShown = false;
            if (!leavingPageImmediately)
                SystemTray.SetIsVisible((DependencyObject)this.Page, this._initialStatusBarVisibility);
            DeviceOrientationHelper.Instance.OrientationChanged -= new EventHandler<DeviceOrientationChangedEventArgs>(this.Instance_OrientationChanged);
            ImageViewerLowProfileImageLoader.ImageDownloaded -= new EventHandler(this.ImageViewerLowProfileImageLoader_ImageDownloaded);
            VKClient.Common.ImageViewer.ImageViewer.UpdateShownCount(false);
            bool? clockwiseRotation = new bool?();
            if (this._manuallyAppliedOrientation == DeviceOrientation.LandscapeRight)
                clockwiseRotation = new bool?(true);
            if (this._manuallyAppliedOrientation == DeviceOrientation.LandscapeLeft)
                clockwiseRotation = new bool?(false);
            Image originalImage = this.OriginalImage;
            if (!leavingPageImmediately)
            {
                this.IsHitTestVisible = false;
                if (this._currentViewControl != null)
                    this._currentViewControl.Visibility = Visibility.Collapsed;
                this._imageAnimator.AnimateOut(this.GetImageSizeSafelyBy(this._currentInd), originalImage, this.CurrentImage, clockwiseRotation, (Action)(() =>
                {
                    this.Opacity = 0.0;
                    this._showHideOriginalImageAction(this.CurrentInd, true);
                    this.SetDefaultOrientation();
                    this.EnsurePrepareImages();
                    this.ArrangeImages();
                    this.ResetImageSources();
                    if (callback == null)
                        return;
                    callback();
                }));
                this._blackRectangle.Animate(this._blackRectangle.Opacity, 0.0, (object)UIElement.OpacityProperty, VKClient.Common.ImageViewer.ImageViewer.ANIMATION_INOUT_DURATION_MS, new int?(0), (IEasingFunction)null, null);
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
            CompositeTransform compositeTransform = RectangleUtils.TransformRect(RectangleUtils.ResizeToFit(new Size(this.Width, this.Height), this.GetImageSizeSafelyBy(this._currentInd)), RectangleUtils.ResizeToFill(this.RectangleFill, this.GetImageSizeSafelyBy(this._currentInd)), false);
            this.AnimateImage(compositeTransform.ScaleX, compositeTransform.ScaleY, compositeTransform.TranslateX, compositeTransform.TranslateY, null);
        }

        private void i_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;
            if (e.PinchManipulation == null)
            {
                Point translation = e.DeltaManipulation.Translation;
                double x = translation.X;
                translation = e.DeltaManipulation.Translation;
                double y = translation.Y;
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
                this.HandleDragCompleted(e.FinalVelocities.LinearVelocity.X, e.FinalVelocities.LinearVelocity.Y);
        }

        private void HandleDragDelta(double hDelta, double vDelta)
        {
            double num1 = hDelta;
            double num2 = vDelta;
            if (this.CurrentImageScale == 1.0 && this._mode == ImageViewerMode.Normal)
            {
                CompositeTransform compositeTransform = this.CurrentImage.RenderTransform as CompositeTransform;
                if (compositeTransform.TranslateX == 0.0 && this.AllowVerticalSwipe && (this.IsInVerticalSwipe || num1 == 0.0 && num2 != 0.0 || Math.Abs(num2) / Math.Abs(num1) > 1.2))
                {
                    this.IsInVerticalSwipe = true;
                    compositeTransform.TranslateY += num2;
                    this._loadingTextBlock.Opacity = 0.0;
                    this._blackRectangle.Opacity = Math.Max(0.0, 1.0 - Math.Abs(compositeTransform.TranslateY) / 400.0);
                }
                else
                {
                    if (this._currentInd == 0 && num1 > 0.0 && compositeTransform.TranslateX > 0.0 || this._currentInd == this._count - 1 && num1 < 0.0 && compositeTransform.TranslateX < 0.0)
                        num1 /= 3.0;
                    foreach (UIElement image in this._images)
                        (image.RenderTransform as CompositeTransform).TranslateX += num1;
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
                            currentImageTransform.Animate(currentImageTransform.TranslateY, 0.0, (object)CompositeTransform.TranslateYProperty, VKClient.Common.ImageViewer.ImageViewer.ANIMATION_DURATION_MS, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING, null);
                            this._blackRectangle.Animate(this._blackRectangle.Opacity, 1.0, (object)UIElement.OpacityProperty, VKClient.Common.ImageViewer.ImageViewer.ANIMATION_DURATION_MS, new int?(0), (IEasingFunction)null, null);
                            this._loadingTextBlock.Opacity = 1.0;
                        }
                        else
                            this.PerformHide();
                        this.IsInVerticalSwipe = false;
                    }
                    else
                    {
                        bool? moveNext = new bool?();
                        double translateX = (this._images[1].RenderTransform as CompositeTransform).TranslateX;
                        double num3 = num1;
                        if ((num3 < -VKClient.Common.ImageViewer.ImageViewer.MOVE_TO_NEXT_VELOCITY_THRESHOLD && translateX < 0.0 || translateX <= -this.Width / 2.0) && this._currentInd < this._count - 1)
                            moveNext = new bool?(true);
                        else if ((num3 > VKClient.Common.ImageViewer.ImageViewer.MOVE_TO_NEXT_VELOCITY_THRESHOLD && translateX > 0.0 || translateX >= this.Width / 2.0) && this._currentInd > 0)
                            moveNext = new bool?(false);
                        double num4 = 0.0;
                        bool? nullable1 = moveNext;
                        bool flag1 = true;
                        if ((nullable1.GetValueOrDefault() == flag1 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
                        {
                            num4 = -this.Width - VKClient.Common.ImageViewer.ImageViewer.MARGIN_BETWEEN_IMAGES;
                        }
                        else
                        {
                            bool? nullable2 = moveNext;
                            bool flag2 = false;
                            if ((nullable2.GetValueOrDefault() == flag2 ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
                                num4 = this.Width + VKClient.Common.ImageViewer.ImageViewer.MARGIN_BETWEEN_IMAGES;
                        }
                        double delta = num4 - translateX;
                        if (moveNext.HasValue && moveNext.Value)
                        {
                            this._images[0].Visibility = Visibility.Collapsed;
                            this.AnimateTwoImagesOnDragComplete(this._images[1], this._images[2], delta, (Action)(() =>
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
                            this._images[2].Visibility = Visibility.Collapsed;
                            bool forbidResizeInNormalMode = this.ForbidResizeInNormalMode;
                            this.AnimateTwoImagesOnDragComplete(this._images[0], this._images[1], delta, (Action)(() =>
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
                            this.AnimateImageOnDragComplete(this._images[0], delta, null, moveNext.HasValue);
                            this.AnimateImageOnDragComplete(this._images[1], delta, null, moveNext.HasValue);
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
            Rect target = RectangleUtils.AlignRects(this.Mode == ImageViewerMode.RectangleFill ? this.RectangleFill : new Rect(new Point(), new Size(this.Width, this.Height)), fitRectTransformed, this.Mode == ImageViewerMode.RectangleFill);
            if (!(target != fitRectTransformed))
                return;
            CompositeTransform compositeTransform = RectangleUtils.TransformRect(this.CurrentImageFitRectOriginal, target, false);
            this.AnimateImage(compositeTransform.ScaleX, compositeTransform.ScaleY, compositeTransform.TranslateX, compositeTransform.TranslateY, null);
        }

        private void PerformHide()
        {
            if (this.HideCallback != null)
                this.HideCallback();
            else
                this.Hide(null, false);
        }

        private void AnimateTwoImagesOnDragComplete(Image image1, Image image2, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            bool wasHitTestVisible = this._manipulationRectangle.IsHitTestVisible;
            this._manipulationRectangle.IsHitTestVisible = false;
            int num = movingToNextOrPrevious ? VKClient.Common.ImageViewer.ImageViewer.DURATION_MOVE_TO_NEXT : VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            CompositeTransform compositeTransform1 = image1.RenderTransform as CompositeTransform;
            CompositeTransform compositeTransform2 = image2.RenderTransform as CompositeTransform;
            animInfoList.Add(new AnimationInfo()
            {
                from = compositeTransform1.TranslateX,
                to = compositeTransform1.TranslateX + delta,
                propertyPath = (object)CompositeTransform.TranslateXProperty,
                duration = num,
                target = (DependencyObject)compositeTransform1,
                easing = VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationInfo()
            {
                from = compositeTransform2.TranslateX,
                to = compositeTransform2.TranslateX + delta,
                propertyPath = (object)CompositeTransform.TranslateXProperty,
                duration = num,
                target = (DependencyObject)compositeTransform2,
                easing = VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING
            });
            int? startTime = new int?(0);
            Action completed = (Action)(() =>
            {
                this._manipulationRectangle.IsHitTestVisible = wasHitTestVisible;
                completedCallback();
            });
            AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        }

        private void AnimateImageOnDragComplete(Image image, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            int duration = movingToNextOrPrevious ? VKClient.Common.ImageViewer.ImageViewer.DURATION_MOVE_TO_NEXT : VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING;
            CompositeTransform target = image.RenderTransform as CompositeTransform;
            target.Animate(target.TranslateX, target.TranslateX + delta, (object)CompositeTransform.TranslateXProperty, duration, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING, completedCallback);
        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            this.HandlePinch(e.GetPosition((UIElement)this, 0), e.GetPosition((UIElement)this, 1));
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
            CompositeTransform compositeTransform = this.CurrentImage.RenderTransform as CompositeTransform;
            this._previousTransformation = new CompositeTransform()
            {
                TranslateX = compositeTransform.TranslateX,
                TranslateY = compositeTransform.TranslateY,
                ScaleX = compositeTransform.ScaleX,
                ScaleY = compositeTransform.ScaleY
            };
        }

        private void HandlePinchCompleted()
        {
            if (this._mode == ImageViewerMode.Normal && this.ForbidResizeInNormalMode)
                return;
            CompositeTransform compositeTransform = this.CurrentImage.RenderTransform as CompositeTransform;
            if (this.Mode == ImageViewerMode.Normal)
            {
                if (compositeTransform.ScaleX < 1.0)
                    this.AnimateImage(1.0, 1.0, 0.0, 0.0, null);
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
            CompositeTransform target = this.CurrentImage.RenderTransform as CompositeTransform;
            target.Animate(target.ScaleX, toScaleX, (object)CompositeTransform.ScaleXProperty, VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING, null);
            target.Animate(target.ScaleY, toScaleY, (object)CompositeTransform.ScaleYProperty, VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING, null);
            target.Animate(target.TranslateX, toTranslateX, (object)CompositeTransform.TranslateXProperty, VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING, null);
            target.Animate(target.TranslateY, toTranslateY, (object)CompositeTransform.TranslateYProperty, VKClient.Common.ImageViewer.ImageViewer.DURATION_BOUNCING, new int?(0), VKClient.Common.ImageViewer.ImageViewer.ANIMATION_EASING, completionCallback);
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
            this._images[0].Clip = (Geometry)null;
            this._images[1].Clip = (Geometry)null;
            this._images[2].Clip = (Geometry)null;
            this._images[0].Visibility = Visibility.Visible;
            this._images[1].Visibility = Visibility.Visible;
            this._images[2].Visibility = Visibility.Visible;
            this.Clip = (Geometry)new RectangleGeometry()
            {
                Rect = new Rect(0.0, 0.0, this.Width, this.HARDCODED_HEIGHT)
            };
            this._images[0].RenderTransform = (Transform)new CompositeTransform();
            this._images[1].RenderTransform = (Transform)new CompositeTransform();
            this._images[2].RenderTransform = (Transform)new CompositeTransform();
            for (int index = 0; index < 3; ++index)
            {
                this._images[index].Width = this.Width;
                this._images[index].Height = this.Height;
            }
            this._loadingTextBlock.Width = this.Width;
            this._loadingTextBlock.Margin = new Thickness(0.0, this.Height / 2.0 - 20.0, 0.0, 0.0);
            this._blackRectangle.Width = this.Width;
            this._blackRectangle.Height = this.HARDCODED_HEIGHT;
            this._manipulationRectangle.Width = this.Width;
            this._manipulationRectangle.Height = this.Height;
            CompositeTransform compositeTransform1 = this._images[0].RenderTransform as CompositeTransform;
            double num1 = -(this.Width + VKClient.Common.ImageViewer.ImageViewer.MARGIN_BETWEEN_IMAGES);
            compositeTransform1.TranslateX = num1;
            double num2 = 0.0;
            compositeTransform1.TranslateY = num2;
            CompositeTransform compositeTransform2 = this._images[1].RenderTransform as CompositeTransform;
            double num3 = 0.0;
            compositeTransform2.TranslateX = num3;
            double num4 = 0.0;
            compositeTransform2.TranslateY = num4;
            CompositeTransform compositeTransform3 = this._images[2].RenderTransform as CompositeTransform;
            double num5 = this.Width + VKClient.Common.ImageViewer.ImageViewer.MARGIN_BETWEEN_IMAGES;
            compositeTransform3.TranslateX = num5;
            double num6 = 0.0;
            compositeTransform3.TranslateY = num6;
            this.SetRenderTransformOnCurrentViewControl();
        }

        private void SetRenderTransformOnCurrentViewControl()
        {
            if (this._currentViewControl == null)
                return;
            this._currentViewControl.RenderTransform = this._images[1].RenderTransform;
            this._currentViewControl.Width = this._images[1].Width;
            this._currentViewControl.Height = this._images[1].Height;
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
                    return (object)imageInfo.Uri;
                if (imageInfo.GetSourceFunc != null)
                    return (object)imageInfo.GetSourceFunc(allowBackgroundCreation);
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
                double width1 = this.Width;
                rectangle1.Width = width1;
                double hardcodedHeight = this.HARDCODED_HEIGHT;
                rectangle1.Height = hardcodedHeight;
                SolidColorBrush solidColorBrush1 = new SolidColorBrush(Colors.Black);
                rectangle1.Fill = (Brush)solidColorBrush1;
                double num1 = 0.0;
                rectangle1.Opacity = num1;
                BitmapCache bitmapCache = new BitmapCache();
                rectangle1.CacheMode = (CacheMode)bitmapCache;
                int num2 = 0;
                rectangle1.IsHitTestVisible = num2 != 0;
                this._blackRectangle = rectangle1;
                this.Children.Add((UIElement)this._blackRectangle);
                TextBlock textBlock = new TextBlock();
                textBlock.Text = CommonResources.Loading;
                SolidColorBrush solidColorBrush2 = new SolidColorBrush(Colors.White);
                textBlock.Foreground = (Brush)solidColorBrush2;
                int num3 = 0;
                textBlock.TextAlignment = (TextAlignment)num3;
                int num4 = 1;
                textBlock.VerticalAlignment = (VerticalAlignment)num4;
                this._loadingTextBlock = textBlock;
                this.Children.Add((UIElement)this._loadingTextBlock);
                foreach (Image image in this._images)
                {
                    image.Width = this.Width;
                    image.Height = this.Height;
                    image.RenderTransform = (Transform)new CompositeTransform();
                    this.Children.Add((UIElement)image);
                }
                Rectangle rectangle2 = new Rectangle();
                double width2 = this.Width;
                rectangle2.Width = width2;
                double height = this.Height;
                rectangle2.Height = height;
                SolidColorBrush solidColorBrush3 = new SolidColorBrush(Colors.Transparent);
                rectangle2.Fill = (Brush)solidColorBrush3;
                this._manipulationRectangle = rectangle2;
                this._manipulationRectangle.DoubleTap += new EventHandler<System.Windows.Input.GestureEventArgs>(this._manipulationRectangle_DoubleTap);
                this._manipulationRectangle.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this.i_ManipulationDelta);
                this._manipulationRectangle.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this.i_ManipulationCompleted);
                this.Children.Add((UIElement)this._manipulationRectangle);
                this._initializedImages = true;
            }
            if (this._currentViewControl == null || this.Children.Contains((UIElement)this._currentViewControl))
                return;
            this._currentViewControl.Width = this.Width;
            this._currentViewControl.Height = this.Height;
            this.Children.Insert(this.Children.Count - 1, (UIElement)this._currentViewControl);
        }

        private void _manipulationRectangle_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.DoubleTapCallback != null)
                this.DoubleTapCallback();
            Point position = e.GetPosition((UIElement)this.CurrentImage);
            if (this.Mode != ImageViewerMode.Normal || this.ForbidResizeInNormalMode)
                return;
            this._inDoubleTapAnimation = true;
            e.Handled = true;
            if (this.CurrentImageScale == 1.0)
            {
                Rect imageFitRectOriginal = this.CurrentImageFitRectOriginal;
                CompositeTransform compositeTransform1 = new CompositeTransform();
                compositeTransform1.ScaleX = 2.0;
                compositeTransform1.ScaleY = 2.0;
                double num1 = -position.X;
                compositeTransform1.TranslateX = num1;
                double num2 = -position.Y;
                compositeTransform1.TranslateY = num2;
                Rect target = RectangleUtils.AlignRects(new Rect(new Point(), new Size(this.Width, this.Height)), compositeTransform1.TransformBounds(imageFitRectOriginal), false);
                CompositeTransform compositeTransform2 = RectangleUtils.TransformRect(imageFitRectOriginal, target, false);
                this.AnimateImage(compositeTransform2.ScaleX, compositeTransform2.ScaleY, compositeTransform2.TranslateX, compositeTransform2.TranslateY, (Action)(() => this._inDoubleTapAnimation = false));
            }
            else
                this.AnimateImage(1.0, 1.0, 0.0, 0.0, (Action)(() => this._inDoubleTapAnimation = false));
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
            return null;
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
    }
}
