using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework.CodeForFun
{
  public class DialogService : IFlyout
  {
    private static readonly object Lockobj = new object();
    private DialogService.AnimationTypes _animationTypeOverlay = DialogService.AnimationTypes.Fade;
    private const string SlideDownInStoryboard = "\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"-150\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"0\" To=\"1\" Duration=\"0:0:0.350\">\r\n                <DoubleAnimation.EasingFunction>\r\n                    <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                </DoubleAnimation.EasingFunction>\r\n            </DoubleAnimation>\r\n        </Storyboard>";
    private const string SlideUpOutStoryboard = "\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.25\" Value=\"-150\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"1\" To=\"0\" Duration=\"0:0:0.25\">\r\n                <DoubleAnimation.EasingFunction>\r\n                    <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                </DoubleAnimation.EasingFunction>\r\n            </DoubleAnimation>\r\n        </Storyboard>";
    private const string SlideUpInStoryboard = "\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\">\r\n                <SplineDoubleKeyFrame KeyTime=\"0\" Value=\"800\"/>\r\n                <SplineDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\">\r\n                    <SplineDoubleKeyFrame.KeySpline>\r\n                        <KeySpline>\r\n                            <KeySpline.ControlPoint1>\r\n                                <Point X=\"0.10000000149011612\" Y=\"0.89999997615811421\" />\r\n                            </KeySpline.ControlPoint1>\r\n                            <KeySpline.ControlPoint2>\r\n                                <Point X=\"0.20000000298023224\" Y=\"1\" />\r\n                            </KeySpline.ControlPoint2>\r\n                        </KeySpline>\r\n                    </SplineDoubleKeyFrame.KeySpline>\r\n                </SplineDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"0\" To=\"1\" Duration=\"0\" />\r\n        </Storyboard>";
    private const string SlideDownOutStoryboard = "\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"800\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n        </Storyboard>";
    private const string SlideHorizontalInStoryboard = "\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.X)\" >\r\n                    <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"-150\"/>\r\n                    <EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\">\r\n                        <EasingDoubleKeyFrame.EasingFunction>\r\n                            <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                        </EasingDoubleKeyFrame.EasingFunction>\r\n                    </EasingDoubleKeyFrame>\r\n                </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"0\" To=\"1\" Duration=\"0:0:0.350\" >\r\n                <DoubleAnimation.EasingFunction>\r\n                    <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                </DoubleAnimation.EasingFunction>\r\n            </DoubleAnimation>\r\n        </Storyboard>";
    private const string SlideHorizontalOutStoryboard = "\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.X)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.25\" Value=\"150\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"1\" To=\"0\" Duration=\"0:0:0.25\">\r\n                <DoubleAnimation.EasingFunction>\r\n                    <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                </DoubleAnimation.EasingFunction>\r\n            </DoubleAnimation>\r\n        </Storyboard>";
    private const string SwivelInStoryboard = "<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Projection).(PlaneProjection.RotationX)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.0\" Value=\"-45\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Opacity)\">\r\n                <DiscreteDoubleKeyFrame KeyTime=\"0\" Value=\"1\" />\r\n            </DoubleAnimationUsingKeyFrames>\r\n        </Storyboard>";
    private const string SwivelOutStoryboard = "<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Projection).(PlaneProjection.RotationX)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.25\" Value=\"45\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Opacity)\">\r\n                <DiscreteDoubleKeyFrame KeyTime=\"0\" Value=\"1\" />\r\n                <DiscreteDoubleKeyFrame KeyTime=\"0:0:0.267\" Value=\"0\" />\r\n            </DoubleAnimationUsingKeyFrames>\r\n        </Storyboard>";
    private const string FadeInStoryboard = "<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimation \r\n\t\t\t\tDuration=\"0:0:0.267\" \r\n\t\t\t\tStoryboard.TargetProperty=\"(UIElement.Opacity)\" \r\n                To=\"1\"/>\r\n        </Storyboard>";
    private const string FadeOutStoryboard = "<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimation \r\n\t\t\t\tDuration=\"0:0:0.267\"\r\n\t\t\t\tStoryboard.TargetProperty=\"(UIElement.Opacity)\" \r\n                To=\"0\"/>\r\n        </Storyboard>";
    private Frame _rootVisual;
    private PageBase _page;
    private Panel _popupContainer;
    private Grid _childPanel;
    private Grid _overlay;
    private DialogService.AnimationTypes _aType;
    private Brush _backgroundBrush;
    private IApplicationBar _applicationBar;
    private bool _deferredShowToLoaded;
    private UIElement _controlToFadeout;
    private bool _isHiding;

    public bool IsOverlayApplied = true;

    public bool HideOnNavigation = true;

    public FrameworkElement Child { get; set; }

    public DialogService.AnimationTypes AnimationType
    {
      get
      {
        return this._aType;
      }
      set
      {
        this._aType = value;
        this.AnimationTypeChild = value;
      }
    }

    public DialogService.AnimationTypes AnimationTypeChild { get; set; }

    public double VerticalOffset { get; set; }

    public double ControlVerticalOffset { get; set; }

    public bool BackButtonPressed { get; set; }

    public Brush BackgroundBrush
    {
      get
      {
        return this._backgroundBrush;
      }
      set
      {
        this._backgroundBrush = value;
        if (value != null)
        {
          SolidColorBrush solidColorBrush = value as SolidColorBrush;
          Color? nullable = solidColorBrush != null ? new Color?(solidColorBrush.Color) : new Color?();
          Color transparent = Colors.Transparent;
          if ((nullable.HasValue ? (nullable.HasValue ? ((nullable.GetValueOrDefault()== transparent) ? 1 : 0) : 1) : 0) == 0)
            return;
        }
        this._animationTypeOverlay = DialogService.AnimationTypes.None;
      }
    }

    public bool IsOpen { get; private set; }

    public bool IsBackKeyOverride { get; set; }

    public bool HasPopup { get; set; }

    private Frame Frame
    {
      get
      {
        return this._rootVisual ?? (this._rootVisual = (Frame) FramePageUtils.Frame);
      }
    }

    private PageBase CurrentPage
    {
      get
      {
        return this._page ?? (this._page = FramePageUtils.CurrentPage);
      }
    }

    public bool KeepAppBar { get; set; }

    public ApplicationBar AppBar { get; set; }

    public bool SetStatusBarBackground { get; set; }

    private Panel PopupContainer
    {
      get
      {
        if (this._popupContainer == null)
        {
          if (this.ShowOnFrame)
          {
            this._popupContainer = this.GetFramePopupContainer();
          }
          else
          {
            PageBase currentPage = this.CurrentPage;
            this._popupContainer = (currentPage != null ? ((UserControl) currentPage).Content :  null) as Panel;
          }
        }
        return this._popupContainer;
      }
    }

    public bool ShowOnFrame { get; set; }

    public Action<Action> OnClosingAction { get; set; }

    public event EventHandler Closing;

    public event EventHandler Closed;

    public event EventHandler Opened;

    public DialogService()
    {
      this.AnimationType = DialogService.AnimationTypes.None;
      this.AnimationTypeChild = DialogService.AnimationTypes.Slide;
      this.BackButtonPressed = false;
      this.BackgroundBrush = (Brush) new SolidColorBrush(Color.FromArgb((byte) 102, (byte) 0, (byte) 0, (byte) 0));
    }

    private Panel GetFramePopupContainer()
    {
      Panel panel =  null;
      Frame frame = this.Frame;
      List<Grid> m0List;
      if (frame == null)
      {
        m0List =  null;
      }
      else
      {
        int num = 0;
        IEnumerable<Grid> logicalChildrenByType = ((FrameworkElement) frame).GetLogicalChildrenByType<Grid>(num != 0);
        m0List = logicalChildrenByType != null ? Enumerable.ToList<Grid>(logicalChildrenByType) :  null;
      }
      List<Grid> gridList = (List<Grid>) m0List;
      if (gridList != null)
      {
        List<Grid>.Enumerator enumerator = gridList.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            List<Panel> list = (List<Panel>) Enumerable.ToList<Panel>(((FrameworkElement) enumerator.Current).GetLogicalChildrenByType<Panel>(false));
            if (Enumerable.Any<Panel>(list))
            {
              panel = (Panel) Enumerable.First<Panel>(list);
              break;
            }
          }
        }
        finally
        {
          enumerator.Dispose();
        }
      }
      return panel;
    }

    private void InitializePopup()
    {
      this._childPanel = this.CreateGrid();
      if (this.IsOverlayApplied)
      {
        this._overlay = this.CreateGrid();
        ((FrameworkElement) this._overlay).UseOptimizedManipulationRouting = false;
        ((UIElement) this._overlay).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>) ((s, e) => this.Hide()));
        if (this.BackgroundBrush != null)
          ((Panel) this._overlay).Background = this.BackgroundBrush;
      }
      if (this.PopupContainer != null)
      {
        if (this._overlay != null)
          ((PresentationFrameworkCollection<UIElement>) this.PopupContainer.Children).Add((UIElement) this._overlay);
        ((PresentationFrameworkCollection<UIElement>) this.PopupContainer.Children).Add((UIElement) this._childPanel);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this._childPanel).Children).Add((UIElement) this.Child);
      }
      else
      {
        this._deferredShowToLoaded = true;
        // ISSUE: method pointer
        ((FrameworkElement) this.Frame).Loaded+=(new RoutedEventHandler( this.RootVisualDeferredShowLoaded));
      }
    }

    private Grid CreateGrid()
    {
      Grid grid1 = new Grid();
      string str = Guid.NewGuid().ToString();
      ((FrameworkElement) grid1).Name = str;
      Grid grid2 = grid1;
      Grid.SetColumnSpan((FrameworkElement) grid2, int.MaxValue);
      Grid.SetRowSpan((FrameworkElement) grid2, int.MaxValue);
      ((UIElement) grid2).Opacity = 0.0;
      this.CalculateVerticalOffset((Panel) grid2);
      return grid2;
    }

    private void CalculateVerticalOffset(Panel panel)
    {
      if (panel == null)
        return;
      int num = 0;
      if (SystemTray.IsVisible && SystemTray.Opacity < 1.0 && SystemTray.Opacity > 0.0)
        num += 32;
      ((FrameworkElement) panel).Margin=(new Thickness(0.0, this.VerticalOffset + (double) num + this.ControlVerticalOffset, 0.0, 0.0));
    }

    private void RootVisualDeferredShowLoaded(object sender, RoutedEventArgs e)
    {
      // ISSUE: method pointer
      ((FrameworkElement) this.Frame).Loaded-=(new RoutedEventHandler( this.RootVisualDeferredShowLoaded));
      this._deferredShowToLoaded = false;
      this.Show( null);
    }

    public void Show(UIElement controlToFadeout = null)
    {
      this._controlToFadeout = controlToFadeout;
      object lockobj = DialogService.Lockobj;
      bool lockTaken = false;
      try
      {
        Monitor.Enter(lockobj, ref lockTaken);
        if (this.CurrentPage == null)
          return;
        this.IsOpen = true;
        this.InitializePopup();
        if (this._deferredShowToLoaded)
          return;
        if (!this.IsBackKeyOverride)
          this.CurrentPage.BackKeyPress += (new EventHandler<CancelEventArgs>(this.OnBackKeyPress));
        // ISSUE: method pointer
        ((Page) this.CurrentPage).NavigationService.Navigated+=(new NavigatedEventHandler( this.OnNavigated));
        if (this.SetStatusBarBackground)
          SystemTray.BackgroundColor = (((SolidColorBrush) Application.Current.Resources["PhoneChromeBrush"]).Color);
        if (!this.KeepAppBar)
        {
          this._applicationBar = this.CurrentPage.ApplicationBar;
          this.CurrentPage.ApplicationBar = ((IApplicationBar) this.AppBar);
        }
        this.AnimateParentControl(true);
        int completedStoryboards = 0;
        Action completionCallback = (Action) (() =>
        {
          ++completedStoryboards;
          if (completedStoryboards != 3)
            return;
          ((Collection<IFlyout>) this.CurrentPage.Flyouts).Add((IFlyout) this);
          // ISSUE: reference to a compiler-generated field
          EventHandler opened = this.Opened;
          if (opened == null)
            return;
          // ISSUE: variable of the null type
          
          opened(this, null);
        });
        this.RunShowStoryboard((FrameworkElement) this._overlay, this._animationTypeOverlay, completionCallback);
        this.RunShowStoryboard((FrameworkElement) this._childPanel, this.AnimationType, completionCallback);
        this.RunShowStoryboard(this.Child, this.AnimationTypeChild, completionCallback);
      }
      finally
      {
        if (lockTaken)
          Monitor.Exit(lockobj);
      }
    }

    private void AnimateParentControl(bool fadeout)
    {
      if (this._controlToFadeout == null)
        return;
      AnimationUtils.Animate(fadeout ? 0.5 : 1.0, (DependencyObject) this._controlToFadeout, "Opacity", 0.25);
      if (fadeout)
        return;
      this._controlToFadeout.Visibility = Visibility.Visible;
    }

    private void RunShowStoryboard(FrameworkElement element, DialogService.AnimationTypes animation, Action completionCallback)
    {
      if (element == null)
      {
        Action action = completionCallback;
        if (action == null)
          return;
        action();
      }
      else
      {
        Storyboard storyboard =  null;
        switch (animation)
        {
          case DialogService.AnimationTypes.Slide:
            storyboard = XamlReader.Load("\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"-150\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"0\" To=\"1\" Duration=\"0:0:0.350\">\r\n                <DoubleAnimation.EasingFunction>\r\n                    <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                </DoubleAnimation.EasingFunction>\r\n            </DoubleAnimation>\r\n        </Storyboard>") as Storyboard;
            ((UIElement) element).RenderTransform = ((Transform) new TranslateTransform());
            break;
          case DialogService.AnimationTypes.SlideInversed:
            storyboard = XamlReader.Load("\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\">\r\n                <SplineDoubleKeyFrame KeyTime=\"0\" Value=\"800\"/>\r\n                <SplineDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\">\r\n                    <SplineDoubleKeyFrame.KeySpline>\r\n                        <KeySpline>\r\n                            <KeySpline.ControlPoint1>\r\n                                <Point X=\"0.10000000149011612\" Y=\"0.89999997615811421\" />\r\n                            </KeySpline.ControlPoint1>\r\n                            <KeySpline.ControlPoint2>\r\n                                <Point X=\"0.20000000298023224\" Y=\"1\" />\r\n                            </KeySpline.ControlPoint2>\r\n                        </KeySpline>\r\n                    </SplineDoubleKeyFrame.KeySpline>\r\n                </SplineDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"0\" To=\"1\" Duration=\"0\" />\r\n        </Storyboard>") as Storyboard;
            ((UIElement) element).RenderTransform = ((Transform) new TranslateTransform());
            break;
          case DialogService.AnimationTypes.SlideHorizontal:
            storyboard = XamlReader.Load("\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.X)\" >\r\n                    <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"-150\"/>\r\n                    <EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\">\r\n                        <EasingDoubleKeyFrame.EasingFunction>\r\n                            <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                        </EasingDoubleKeyFrame.EasingFunction>\r\n                    </EasingDoubleKeyFrame>\r\n                </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"0\" To=\"1\" Duration=\"0:0:0.350\" >\r\n                <DoubleAnimation.EasingFunction>\r\n                    <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                </DoubleAnimation.EasingFunction>\r\n            </DoubleAnimation>\r\n        </Storyboard>") as Storyboard;
            ((UIElement) element).RenderTransform = ((Transform) new TranslateTransform());
            break;
          case DialogService.AnimationTypes.Swivel:
          case DialogService.AnimationTypes.SwivelHorizontal:
            storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Projection).(PlaneProjection.RotationX)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.0\" Value=\"-45\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Opacity)\">\r\n                <DiscreteDoubleKeyFrame KeyTime=\"0\" Value=\"1\" />\r\n            </DoubleAnimationUsingKeyFrames>\r\n        </Storyboard>") as Storyboard;
            FrameworkElement frameworkElement = element;
            PlaneProjection planeProjection = new PlaneProjection();
            double num1 = -45.0;
            planeProjection.RotationX = num1;
            double num2 = element.ActualHeight / 2.0;
            planeProjection.CenterOfRotationX = num2;
            ((UIElement) frameworkElement).Projection=((Projection) planeProjection);
            break;
          case DialogService.AnimationTypes.Fade:
            storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimation \r\n\t\t\t\tDuration=\"0:0:0.267\" \r\n\t\t\t\tStoryboard.TargetProperty=\"(UIElement.Opacity)\" \r\n                To=\"1\"/>\r\n        </Storyboard>") as Storyboard;
            break;
        }
        if (storyboard != null)
        {
          ((UIElement) element).Opacity = 0.0;
          ((DependencyObject) this.CurrentPage).Dispatcher.BeginInvoke((Action) (() =>
          {
            ((Timeline) storyboard).Completed += ((EventHandler) ((s, e) =>
            {
              Action action = completionCallback;
              if (action == null)
                return;
              action();
            }));
            IEnumerator<Timeline> enumerator = ((PresentationFrameworkCollection<Timeline>) storyboard.Children).GetEnumerator();
            try
            {
              while (((IEnumerator) enumerator).MoveNext())
                Storyboard.SetTarget(enumerator.Current, (DependencyObject) element);
            }
            finally
            {
              if (enumerator != null)
                ((IDisposable) enumerator).Dispose();
            }
            storyboard.Begin();
          }));
        }
        else
        {
          ((UIElement) element).Opacity = 1.0;
          Action action = completionCallback;
          if (action == null)
            return;
          action();
        }
      }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
      if (!e.IsNavigationInitiator || !this.HideOnNavigation)
        return;
      this.Hide();
    }

    public void Hide()
    {
      if (!this.IsOpen || this._isHiding)
        return;
      this._isHiding = true;
      // ISSUE: reference to a compiler-generated field
      EventHandler closing = this.Closing;
      if (closing != null)
      {
        EventArgs empty = EventArgs.Empty;
        closing(this, empty);
      }
      Action hideAction = (Action) (() => this.RunHideStoryboard(this.Child, this.AnimationTypeChild, (Action) (() =>
      {
        this.RunHideStoryboard((FrameworkElement) this._overlay, this._animationTypeOverlay, (Action) (() => {}));
        this.RunHideStoryboard((FrameworkElement) this._childPanel, this.AnimationType, new Action(this.HideStoryboardCompleted));
      })));
      if (this.OnClosingAction != null)
        this.OnClosingAction((Action) (() => hideAction()));
      else
        hideAction();
    }

    private void RunHideStoryboard(FrameworkElement element, DialogService.AnimationTypes animation, Action completionCallback)
    {
      if (element == null)
        return;
      Storyboard storyboard =  null;
      switch (animation)
      {
        case DialogService.AnimationTypes.Slide:
          storyboard = XamlReader.Load("\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.25\" Value=\"-150\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"1\" To=\"0\" Duration=\"0:0:0.25\">\r\n                <DoubleAnimation.EasingFunction>\r\n                    <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                </DoubleAnimation.EasingFunction>\r\n            </DoubleAnimation>\r\n        </Storyboard>") as Storyboard;
          break;
        case DialogService.AnimationTypes.SlideInversed:
          storyboard = XamlReader.Load("\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.Y)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"800\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n        </Storyboard>") as Storyboard;
          break;
        case DialogService.AnimationTypes.SlideHorizontal:
          storyboard = XamlReader.Load("\r\n        <Storyboard  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.RenderTransform).(TranslateTransform.X)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.25\" Value=\"150\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimation Storyboard.TargetProperty=\"(UIElement.Opacity)\" From=\"1\" To=\"0\" Duration=\"0:0:0.25\">\r\n                <DoubleAnimation.EasingFunction>\r\n                    <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                </DoubleAnimation.EasingFunction>\r\n            </DoubleAnimation>\r\n        </Storyboard>") as Storyboard;
          break;
        case DialogService.AnimationTypes.Swivel:
        case DialogService.AnimationTypes.SwivelHorizontal:
          storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Projection).(PlaneProjection.RotationX)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.25\" Value=\"45\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Opacity)\">\r\n                <DiscreteDoubleKeyFrame KeyTime=\"0\" Value=\"1\" />\r\n                <DiscreteDoubleKeyFrame KeyTime=\"0:0:0.267\" Value=\"0\" />\r\n            </DoubleAnimationUsingKeyFrames>\r\n        </Storyboard>") as Storyboard;
          FrameworkElement frameworkElement = element;
          PlaneProjection planeProjection = new PlaneProjection();
          double num1 = 0.0;
          planeProjection.RotationX = num1;
          double num2 = element.ActualHeight / 2.0;
          planeProjection.CenterOfRotationX = num2;
          ((UIElement) frameworkElement).Projection=((Projection) planeProjection);
          break;
        case DialogService.AnimationTypes.Fade:
          storyboard = XamlReader.Load("<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimation \r\n\t\t\t\tDuration=\"0:0:0.267\"\r\n\t\t\t\tStoryboard.TargetProperty=\"(UIElement.Opacity)\" \r\n                To=\"0\"/>\r\n        </Storyboard>") as Storyboard;
          break;
      }
      try
      {
        if (storyboard != null)
        {
          ((Timeline) storyboard).Completed += ((EventHandler) ((s, e) => completionCallback()));
          IEnumerator<Timeline> enumerator = ((PresentationFrameworkCollection<Timeline>) storyboard.Children).GetEnumerator();
          try
          {
            while (((IEnumerator) enumerator).MoveNext())
              Storyboard.SetTarget(enumerator.Current, (DependencyObject) element);
          }
          finally
          {
            if (enumerator != null)
              ((IDisposable) enumerator).Dispose();
          }
          storyboard.Begin();
        }
        else
          completionCallback();
      }
      catch
      {
        completionCallback();
      }
    }

    private void HideStoryboardCompleted()
    {
      this.IsOpen = false;
      try
      {
        if (this.SetStatusBarBackground)
          SystemTray.BackgroundColor = (((SolidColorBrush) Application.Current.Resources["PhoneBackgroundBrush"]).Color);
        if (this.CurrentPage != null)
        {
          if (((Collection<IFlyout>) this.CurrentPage.Flyouts).Contains((IFlyout) this))
            ((Collection<IFlyout>) this.CurrentPage.Flyouts).Remove((IFlyout) this);
          this.CurrentPage.BackKeyPress -= (new EventHandler<CancelEventArgs>(this.OnBackKeyPress));
          // ISSUE: method pointer
          ((Page) this.CurrentPage).NavigationService.Navigated -= (new NavigatedEventHandler( this.OnNavigated));
          if (this._applicationBar != null)
          {
            this.CurrentPage.ApplicationBar = this._applicationBar;
            this._applicationBar =  null;
          }
          this.AnimateParentControl(false);
          this._page =  null;
        }
      }
      catch
      {
      }
      try
      {
        Panel popupContainer = this.PopupContainer;
        if ((popupContainer != null ? popupContainer.Children :  null) != null)
        {
          if (this._overlay != null)
            ((PresentationFrameworkCollection<UIElement>) this.PopupContainer.Children).Remove((UIElement) this._overlay);
          ((PresentationFrameworkCollection<UIElement>) this.PopupContainer.Children).Remove((UIElement) this._childPanel);
        }
      }
      catch
      {
      }
      this._isHiding = false;
      try
      {
        // ISSUE: reference to a compiler-generated field
        EventHandler closed = this.Closed;
        if (closed == null)
          return;
        // ISSUE: variable of the null type

        closed(this, null);
      }
      catch
      {
      }
    }

    public void ChangeChild(FrameworkElement newChild, Action callback = null)
    {
      this.RunHideStoryboard(this.Child, this.AnimationTypeChild, (Action) (() =>
      {
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this._childPanel).Children).Remove((UIElement) this.Child);
        this.Child = newChild;
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this._childPanel).Children).Add((UIElement) this.Child);
        this.RunShowStoryboard(this.Child, this.AnimationTypeChild, callback);
      }));
    }

    private void OnBackKeyPress(object sender, CancelEventArgs e)
    {
      if (!this.CurrentPage.CanNavigateBack)
        return;
      if (this.HasPopup)
      {
        e.Cancel = true;
      }
      else
      {
        if (!this.IsOpen)
          return;
        e.Cancel = true;
        this.BackButtonPressed = true;
        this.Hide();
      }
    }

    public enum AnimationTypes
    {
      Slide,
      SlideInversed,
      SlideHorizontal,
      Swivel,
      SwivelHorizontal,
      Fade,
      None,
    }
  }
}
