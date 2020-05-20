using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class AppNotificationUC : UserControl
  {
    private static readonly double VERTICAL_HEIGHT = 112.0;
    private static readonly double HORIZONTAL_HEIGHT = 88.0;
    private static AppNotificationUC _instance;
    private DelayedExecutor _de;
    private bool _subscribedOrientationChange;
    private bool _isShown;
    private Action _tapCallback;
    private bool _isAnimating;
    internal Grid LayoutRoot;
    internal Grid push_notification;
    internal Canvas close;
    internal Path Ellipse_8_copy_2;
    internal Canvas Group_9;
    internal Path Rounded_Rectangle_11;
    internal Path Rounded_Rectangle_1;
    private bool _contentLoaded;

    public static AppNotificationUC Instance
    {
      get
      {
        return AppNotificationUC._instance;
      }
    }

    public AppNotificationUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      base.DataContext = AppNotificationViewModel.Instance;
      AppNotificationUC._instance = this;
      this._de = new DelayedExecutor(4000);
    }

    public void ShowAndHideLater(string imageSrc, string title, string subtitle, Action tapCallback, Action autoHideCallback = null)
    {
      if (FramePageUtils.Frame != null && !this._subscribedOrientationChange)
      {
        FramePageUtils.Frame.OrientationChanged += (new EventHandler<OrientationChangedEventArgs>(this.Frame_OrientationChanged));
        this._subscribedOrientationChange = true;
      }
      this._tapCallback = tapCallback;
      this.UpdateHeight();
      AppNotificationViewModel.Instance.SetData(imageSrc, title, subtitle);
      this.Show();
      this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() => this.Hide(true, autoHideCallback)))));
    }

    private void Frame_OrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      this.UpdateHeight();
    }

    private void UpdateHeight()
    {
      ((FrameworkElement) this.LayoutRoot).Height=(FramePageUtils.IsHorizontal ? AppNotificationUC.HORIZONTAL_HEIGHT : AppNotificationUC.VERTICAL_HEIGHT);
    }

    private void Hide(bool animate = true, Action autohideCallback = null)
    {
      if (animate)
      {
        this.AnimateShowHide(false, autohideCallback);
      }
      else
      {
        if (this._isAnimating)
          return;
        base.Visibility = Visibility.Collapsed;
        this._isShown = false;
        if (autohideCallback == null)
          return;
        autohideCallback();
      }
    }

    private void Show()
    {
      if (this._isShown)
        return;
      this.AnimateShowHide(true,  null);
    }

    private void AnimateShowHide(bool show, Action callback = null)
    {
      if (this._isAnimating)
        return;
      this._isAnimating = true;
      TranslateTransform renderTransform = ((UIElement) this.LayoutRoot).RenderTransform as TranslateTransform;
      if (show)
      {
        base.Visibility = Visibility.Visible;
        TranslateTransform translateTransform = renderTransform;
        double from = -AppNotificationUC.VERTICAL_HEIGHT;
        double to = 0.0;
        // ISSUE: variable of the null type
        int duration = 250;
        int? startTime = new int?(0);
        ExponentialEase exponentialEase = new ExponentialEase();
        int num1 = 1;
        ((EasingFunctionBase) exponentialEase).EasingMode = ((EasingMode) num1);
        double num2 = 6.0;
        exponentialEase.Exponent = num2;
        Action completed = (Action) (() =>
        {
          this._isShown = true;
          this._isAnimating = false;
          if (callback == null)
            return;
          callback();
        });
        int num3 = 0;
        ((DependencyObject)translateTransform).Animate(from, to, TranslateTransform.YProperty, duration, startTime, (IEasingFunction)exponentialEase, completed, num3 != 0);
      }
      else
      {
        TranslateTransform translateTransform = renderTransform;
        double from = 0.0;
        double to = -AppNotificationUC.VERTICAL_HEIGHT;
        // ISSUE: variable of the null type
        int duration = 250;
        int? startTime = new int?(0);
        ExponentialEase exponentialEase = new ExponentialEase();
        int num1 = 0;
        ((EasingFunctionBase) exponentialEase).EasingMode = ((EasingMode) num1);
        double num2 = 6.0;
        exponentialEase.Exponent = num2;
        Action completed = (Action) (() =>
        {
          this._isShown = false;
          this._isAnimating = false;
          base.Visibility = Visibility.Collapsed;
          if (callback == null)
            return;
          callback();
        });
        int num3 = 0;
        ((DependencyObject)translateTransform).Animate(from, to, TranslateTransform.YProperty, duration, startTime, (IEasingFunction)exponentialEase, completed, num3 != 0);
      }
    }

    private void GridTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.Hide(false,  null);
      PageBase currentPage = FramePageUtils.CurrentPage;
      if (currentPage != null && currentPage.IsMenuOpen)
        currentPage.OpenCloseMenu(false, this._tapCallback, false);
      else
        this._tapCallback();
    }

    private void CloseTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.Hide(true,  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/AppNotificationUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.push_notification = (Grid) base.FindName("push_notification");
      this.close = (Canvas) base.FindName("close");
      this.Ellipse_8_copy_2 = (Path) base.FindName("Ellipse_8_copy_2");
      this.Group_9 = (Canvas) base.FindName("Group_9");
      this.Rounded_Rectangle_11 = (Path) base.FindName("Rounded_Rectangle_11");
      this.Rounded_Rectangle_1 = (Path) base.FindName("Rounded_Rectangle_1");
    }
  }
}
