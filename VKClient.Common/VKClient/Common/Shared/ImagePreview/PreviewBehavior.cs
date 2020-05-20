using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Utils;

namespace VKClient.Common.Shared.ImagePreview
{
  public class PreviewBehavior : Behavior<FrameworkElement>
  {
    public static readonly int PUSH_ANIMATION_DURATION = 200;
    public static readonly int HOLD_GESTURE_MS = 150;
    public static readonly IEasingFunction PUSH_ANIMATION_EASING = (IEasingFunction) new CubicEase();
    public static readonly double PUSH_SCALE = 0.8;
    public static readonly DependencyProperty PreviewUriProperty = DependencyProperty.Register("PreviewUri", typeof (string), typeof (PreviewBehavior), new PropertyMetadata((object) ""));
    public static readonly DependencyProperty TopOffsetProperty = DependencyProperty.Register("TopOffset", typeof (int), typeof (PreviewBehavior), new PropertyMetadata((object) 140));
    private static bool _isShowingPreview = false;
    private static DateTime _lastShownTime = DateTime.MinValue;
    private DispatcherTimer _timer = new DispatcherTimer()
    {
      Interval = TimeSpan.FromMilliseconds((double) PreviewBehavior.HOLD_GESTURE_MS)
    };
    private const int DEFAULT_TOP_OFFSET = 140;
    private FullscreenLoader _loader;
    private PreviewImageUC _ucPreview;
    private FrameworkElement _hoveredElement;
    private SupportedPageOrientation _savedSupportedOrientation;
    private DateTime _lastTouchFrameDate;

    public static bool JustShown
    {
      get
      {
        if (!PreviewBehavior._isShowingPreview)
          return (DateTime.Now - PreviewBehavior._lastShownTime).TotalMilliseconds < 100.0;
        return true;
      }
    }

    public string PreviewUri
    {
      get
      {
        return (string) this.GetValue(PreviewBehavior.PreviewUriProperty);
      }
      set
      {
        this.SetValue(PreviewBehavior.PreviewUriProperty, (object) value);
      }
    }

    public int TopOffset
    {
      get
      {
        return (int) this.GetValue(PreviewBehavior.TopOffsetProperty);
      }
      set
      {
        this.SetValue(PreviewBehavior.TopOffsetProperty, (object) value);
      }
    }

    protected override void OnAttached()
    {
      base.OnAttached();
      this.AssociatedObject.CacheMode = (CacheMode) new BitmapCache();
      this.AssociatedObject.UseOptimizedManipulationRouting = false;
      this.AssociatedObject.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(this.AssociatedObject_ManipulationStarted);
      this.AssociatedObject.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this.AssociatedObject_ManipulationDelta);
      this.AssociatedObject.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this.AssociatedObject_ManipulationCompleted);
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();
      this.AssociatedObject.CacheMode = (CacheMode) null;
      this.AssociatedObject.UseOptimizedManipulationRouting = true;
      this.AssociatedObject.ManipulationStarted -= new EventHandler<ManipulationStartedEventArgs>(this.AssociatedObject_ManipulationStarted);
      this.AssociatedObject.ManipulationDelta -= new EventHandler<ManipulationDeltaEventArgs>(this.AssociatedObject_ManipulationDelta);
      this.AssociatedObject.ManipulationCompleted -= new EventHandler<ManipulationCompletedEventArgs>(this.AssociatedObject_ManipulationCompleted);
    }

    private void AssociatedObject_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      if (PreviewBehavior._isShowingPreview)
      {
        e.Handled = true;
        this.EnsureHidePreview();
      }
      else
        this.StartTimer();
    }

    private void StartTimer()
    {
      if (this._timer.IsEnabled)
        return;
      this._timer.Tick += new EventHandler(this._timer_Tick);
      this._timer.Start();
    }

    private void StopTimer()
    {
      if (!this._timer.IsEnabled)
        return;
      this._timer.Stop();
      this._timer.Tick -= new EventHandler(this._timer_Tick);
    }

    private void _timer_Tick(object sender, EventArgs e)
    {
      if (PreviewBehavior._isShowingPreview)
      {
        if ((DateTime.Now - this._lastTouchFrameDate).TotalMilliseconds < 750.0)
          return;
        this.EnsureHidePreview();
      }
      else
      {
        if (string.IsNullOrEmpty(this.PreviewUri))
          return;
        PreviewBehavior._isShowingPreview = true;
        Touch.FrameReported += new TouchFrameEventHandler(this.Touch_FrameReported);
        PageBase currentPage = FramePageUtils.CurrentPage;
        if (currentPage != null)
        {
          this._savedSupportedOrientation = currentPage.SupportedOrientations;
          FramePageUtils.CurrentPage.SupportedOrientations = currentPage.Orientation == PageOrientation.PortraitUp || currentPage.Orientation == PageOrientation.Portrait ? SupportedPageOrientation.Portrait : SupportedPageOrientation.Landscape;
        }
        string previewUri = this.PreviewUri;
        Image image = this.AssociatedObject as Image;
        BitmapImage originalImage = (image != null ? image.Source : null) as BitmapImage;
        int topOffset = this.TopOffset;
        this.ShowPreview(previewUri, originalImage, topOffset);
        this.SetHoveredElement(this.AssociatedObject);
      }
    }

    private void Touch_FrameReported(object sender, TouchFrameEventArgs e)
    {
      this._lastTouchFrameDate = DateTime.Now;
    }

    private void SetHoveredElement(FrameworkElement newHoveredElement)
    {
      if (this._hoveredElement == newHoveredElement)
        return;
      List<AnimationInfo> animInfoList = new List<AnimationInfo>();
      if (this._hoveredElement != null)
        animInfoList.AddRange((IEnumerable<AnimationInfo>) PreviewBehavior.CreateAnimations(this._hoveredElement, false));
      this._hoveredElement = newHoveredElement;
      if (this._hoveredElement != null)
        animInfoList.AddRange((IEnumerable<AnimationInfo>) PreviewBehavior.CreateAnimations(this._hoveredElement, true));
      AnimationUtil.AnimateSeveral(animInfoList, new int?(0), null);
    }

    private static List<AnimationInfo> CreateAnimations(FrameworkElement element, bool push)
    {
      if (!(element.RenderTransform is ScaleTransform))
        element.RenderTransform = (Transform) new ScaleTransform()
        {
          CenterX = (element.ActualWidth / 2.0),
          CenterY = (element.ActualHeight / 2.0)
        };
      List<AnimationInfo> animationInfoList = new List<AnimationInfo>();
      AnimationInfo animationInfo1 = new AnimationInfo();
      animationInfo1.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
      animationInfo1.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
      double scaleX = (element.RenderTransform as ScaleTransform).ScaleX;
      animationInfo1.from = scaleX;
      double num1 = push ? PreviewBehavior.PUSH_SCALE : 1.0;
      animationInfo1.to = num1;
      Transform renderTransform1 = element.RenderTransform;
      animationInfo1.target = (DependencyObject) renderTransform1;
      DependencyProperty dependencyProperty1 = ScaleTransform.ScaleXProperty;
      animationInfo1.propertyPath = (object) dependencyProperty1;
      animationInfoList.Add(animationInfo1);
      AnimationInfo animationInfo2 = new AnimationInfo();
      animationInfo2.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
      animationInfo2.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
      double scaleY = (element.RenderTransform as ScaleTransform).ScaleY;
      animationInfo2.from = scaleY;
      double num2 = push ? PreviewBehavior.PUSH_SCALE : 1.0;
      animationInfo2.to = num2;
      Transform renderTransform2 = element.RenderTransform;
      animationInfo2.target = (DependencyObject) renderTransform2;
      DependencyProperty dependencyProperty2 = ScaleTransform.ScaleYProperty;
      animationInfo2.propertyPath = (object) dependencyProperty2;
      animationInfoList.Add(animationInfo2);
      return animationInfoList;
    }

    private void AssociatedObject_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      if (PreviewBehavior._isShowingPreview)
      {
        Point point = new Point(e.ManipulationOrigin.X, e.ManipulationOrigin.Y);
        UIElement rootVisual = Application.Current.RootVisual;
        FrameworkElement newHoveredElement = VisualTreeHelper.FindElementsInHostCoordinates(PreviewBehavior.GetHostCoordinates(e.ManipulationContainer.TransformToVisual(rootVisual).Transform(point)), rootVisual).FirstOrDefault<UIElement>() as FrameworkElement;
        if (newHoveredElement != null)
        {
          BehaviorCollection behaviors1 = Interaction.GetBehaviors((DependencyObject) newHoveredElement);
          PreviewBehavior previewBehavior = (behaviors1 != null ? behaviors1.FirstOrDefault<Behavior>((Func<Behavior, bool>) (b => b is PreviewBehavior)) : (Behavior) null) as PreviewBehavior;
          if (previewBehavior == null && newHoveredElement.Parent != null)
          {
            BehaviorCollection behaviors2 = Interaction.GetBehaviors(newHoveredElement.Parent);
            previewBehavior = (behaviors2 != null ? behaviors2.FirstOrDefault<Behavior>((Func<Behavior, bool>) (b => b is PreviewBehavior)) : (Behavior) null) as PreviewBehavior;
          }
          if (previewBehavior != null)
          {
            Image image = newHoveredElement as Image;
            BitmapImage originalImage = (image != null ? image.Source : null) as BitmapImage;
            this.ShowPreview(previewBehavior.PreviewUri, originalImage, previewBehavior.TopOffset);
            this.SetHoveredElement(newHoveredElement);
          }
        }
        e.Handled = true;
      }
      else
        this.StopTimer();
    }

    private void AssociatedObject_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      if (PreviewBehavior._isShowingPreview)
      {
        e.Handled = true;
        this.EnsureHidePreview();
      }
      else
        this.StopTimer();
    }

    private void EnsureHidePreview()
    {
      if (!PreviewBehavior._isShowingPreview)
        return;
      this.StopTimer();
      PreviewBehavior._lastShownTime = DateTime.Now;
      this.HidePreview();
      this.SetHoveredElement(null);
      PreviewBehavior._isShowingPreview = false;
      Touch.FrameReported -= new TouchFrameEventHandler(this.Touch_FrameReported);
      PageBase currentPage = FramePageUtils.CurrentPage;
      if (currentPage != null)
        currentPage.SupportedOrientations = this._savedSupportedOrientation;
      EventAggregator.Current.Publish((object) new PreviewCompletedEvent());
    }

    private void ShowPreview(string previewUri, BitmapImage originalImage = null, int topOffset = 0)
    {
      if (this._ucPreview == null)
      {
        this._ucPreview = new PreviewImageUC();
        this._loader = new FullscreenLoader();
        this._loader.Show((FrameworkElement) this._ucPreview, false);
      }
      this._ucPreview.SetImageUri(previewUri, originalImage);
      this._ucPreview.imagePreview.Margin = new Thickness(0.0, (double) topOffset, 0.0, 0.0);
    }

    private void HidePreview()
    {
      if (this._ucPreview == null)
        return;
      this._loader.Hide(false);
      this._loader = (FullscreenLoader) null;
      this._ucPreview = (PreviewImageUC) null;
    }

    private static Point GetHostCoordinates(Point point)
    {
      PhoneApplicationFrame applicationFrame = (PhoneApplicationFrame) Application.Current.RootVisual;
      switch (applicationFrame.Orientation)
      {
        case PageOrientation.LandscapeLeft:
          return new Point(applicationFrame.RenderSize.Width - point.Y, point.X);
        case PageOrientation.LandscapeRight:
          return new Point(point.Y, applicationFrame.RenderSize.Height - point.X);
        default:
          return point;
      }
    }
  }
}
