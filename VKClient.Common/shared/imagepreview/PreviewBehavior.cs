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
    public static readonly DependencyProperty PreviewUriProperty = DependencyProperty.Register("PreviewUri", typeof (string), typeof (PreviewBehavior), new PropertyMetadata(""));
    public static readonly DependencyProperty TopOffsetProperty = DependencyProperty.Register("TopOffset", typeof (int), typeof (PreviewBehavior), new PropertyMetadata(140));
    private static bool _isShowingPreview = false;
    private static DateTime _lastShownTime = DateTime.MinValue;
    private const int DEFAULT_TOP_OFFSET = 140;
    private DispatcherTimer _timer;
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
        this.SetValue(PreviewBehavior.PreviewUriProperty, value);
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
        this.SetValue(PreviewBehavior.TopOffsetProperty, value);
      }
    }

    public PreviewBehavior()
    {
      DispatcherTimer dispatcherTimer = new DispatcherTimer();
      TimeSpan timeSpan = TimeSpan.FromMilliseconds((double) PreviewBehavior.HOLD_GESTURE_MS);
      dispatcherTimer.Interval = timeSpan;
      this._timer = dispatcherTimer;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
    }

    protected override void OnAttached()
    {
      base.OnAttached();
      ((UIElement) this.AssociatedObject).CacheMode = ((CacheMode) new BitmapCache());
      this.AssociatedObject.UseOptimizedManipulationRouting = false;
      ((UIElement) this.AssociatedObject).ManipulationStarted += (new EventHandler<ManipulationStartedEventArgs>(this.AssociatedObject_ManipulationStarted));
      ((UIElement) this.AssociatedObject).ManipulationDelta += (new EventHandler<ManipulationDeltaEventArgs>(this.AssociatedObject_ManipulationDelta));
      ((UIElement) this.AssociatedObject).ManipulationCompleted += (new EventHandler<ManipulationCompletedEventArgs>(this.AssociatedObject_ManipulationCompleted));
    }

    protected override void OnDetaching()
    {
      base.OnDetaching();
      ((UIElement) this.AssociatedObject).CacheMode = ( null);
      this.AssociatedObject.UseOptimizedManipulationRouting = true;
      ((UIElement) this.AssociatedObject).ManipulationStarted-=(new EventHandler<ManipulationStartedEventArgs>(this.AssociatedObject_ManipulationStarted));
      ((UIElement) this.AssociatedObject).ManipulationDelta-=(new EventHandler<ManipulationDeltaEventArgs>(this.AssociatedObject_ManipulationDelta));
      ((UIElement) this.AssociatedObject).ManipulationCompleted-=(new EventHandler<ManipulationCompletedEventArgs>(this.AssociatedObject_ManipulationCompleted));
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
      this._timer.Tick+=(new EventHandler(this._timer_Tick));
      this._timer.Start();
    }

    private void StopTimer()
    {
      if (!this._timer.IsEnabled)
        return;
      this._timer.Stop();
      this._timer.Tick-=(new EventHandler(this._timer_Tick));
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
        // ISSUE: method pointer
        Touch.FrameReported+=(new TouchFrameEventHandler(this.Touch_FrameReported));
        PageBase currentPage = FramePageUtils.CurrentPage;
        if (currentPage != null)
        {
          this._savedSupportedOrientation = currentPage.SupportedOrientations;
          FramePageUtils.CurrentPage.SupportedOrientations = (currentPage.Orientation == PageOrientation.PortraitUp || currentPage.Orientation == PageOrientation.Portrait ? (SupportedPageOrientation) 1 : (SupportedPageOrientation) 2);
        }
        string previewUri = this.PreviewUri;
        Image associatedObject = this.AssociatedObject as Image;
        BitmapImage originalImage = (associatedObject != null ? associatedObject.Source :  null) as BitmapImage;
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
      AnimationUtil.AnimateSeveral(animInfoList, new int?(0),  null);
    }

    private static List<AnimationInfo> CreateAnimations(FrameworkElement element, bool push)
    {
      if (!(((UIElement) element).RenderTransform is ScaleTransform))
      {
        ScaleTransform scaleTransform = new ScaleTransform();
        scaleTransform.CenterX=(element.ActualWidth / 2.0);
        scaleTransform.CenterY=(element.ActualHeight / 2.0);
        ((UIElement) element).RenderTransform = ((Transform) scaleTransform);
      }
      List<AnimationInfo> animationInfoList = new List<AnimationInfo>();
      AnimationInfo animationInfo1 = new AnimationInfo();
      animationInfo1.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
      animationInfo1.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
      double scaleX = (((UIElement) element).RenderTransform as ScaleTransform).ScaleX;
      animationInfo1.from = scaleX;
      double num1 = push ? PreviewBehavior.PUSH_SCALE : 1.0;
      animationInfo1.to = num1;
      Transform renderTransform1 = ((UIElement) element).RenderTransform;
      animationInfo1.target = (DependencyObject) renderTransform1;
      // ISSUE: variable of the null type
      animationInfo1.propertyPath = ScaleTransform.ScaleXProperty;
      animationInfoList.Add(animationInfo1);
      AnimationInfo animationInfo2 = new AnimationInfo();
      animationInfo2.duration = PreviewBehavior.PUSH_ANIMATION_DURATION;
      animationInfo2.easing = PreviewBehavior.PUSH_ANIMATION_EASING;
      double scaleY = (((UIElement) element).RenderTransform as ScaleTransform).ScaleY;
      animationInfo2.from = scaleY;
      double num2 = push ? PreviewBehavior.PUSH_SCALE : 1.0;
      animationInfo2.to = num2;
      Transform renderTransform2 = ((UIElement) element).RenderTransform;
      animationInfo2.target = (DependencyObject) renderTransform2;
      // ISSUE: variable of the null type
      animationInfo2.propertyPath = ScaleTransform.ScaleYProperty;
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
                BehaviorCollection behaviors1 = Interaction.GetBehaviors((DependencyObject)newHoveredElement);
                PreviewBehavior previewBehavior = (behaviors1 != null ? behaviors1.FirstOrDefault<Behavior>((Func<Behavior, bool>)(b => b is PreviewBehavior)) : (Behavior)null) as PreviewBehavior;
                if (previewBehavior == null && newHoveredElement.Parent != null)
                {
                    BehaviorCollection behaviors2 = Interaction.GetBehaviors(newHoveredElement.Parent);
                    previewBehavior = (behaviors2 != null ? behaviors2.FirstOrDefault<Behavior>((Func<Behavior, bool>)(b => b is PreviewBehavior)) : (Behavior)null) as PreviewBehavior;
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
      this.SetHoveredElement( null);
      PreviewBehavior._isShowingPreview = false;
      // ISSUE: method pointer
      Touch.FrameReported-=(new TouchFrameEventHandler(this.Touch_FrameReported));
      PageBase currentPage = FramePageUtils.CurrentPage;
      if (currentPage != null)
        currentPage.SupportedOrientations = this._savedSupportedOrientation;
      EventAggregator.Current.Publish(new PreviewCompletedEvent());
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
      ((FrameworkElement) this._ucPreview.imagePreview).Margin=(new Thickness(0.0, (double) topOffset, 0.0, 0.0));
    }

    private void HidePreview()
    {
      if (this._ucPreview == null)
        return;
      this._loader.Hide(false);
      this._loader =  null;
      this._ucPreview =  null;
    }

    private static Point GetHostCoordinates(Point point)
    {
      PhoneApplicationFrame rootVisual = (PhoneApplicationFrame) Application.Current.RootVisual;
      PageOrientation orientation = rootVisual.Orientation;
      if (orientation != PageOrientation.LandscapeLeft)
      {
          if (orientation != PageOrientation.LandscapeRight)
          return point;
        // ISSUE: explicit reference operation
        double y = ((Point) point).Y;
        Size renderSize = ((UIElement) rootVisual).RenderSize;
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        double num = ((Size) renderSize).Height - ((Point) point).X;
        return new Point(y, num);
      }
      Size renderSize1 = ((UIElement) rootVisual).RenderSize;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      return new Point(((Size) renderSize1).Width - ((Point) point).Y, ((Point) point).X);
    }
  }
}
