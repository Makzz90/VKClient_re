using Microsoft.Phone.Applications.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.ImageViewer;
using VKClient.Common.Localization;

namespace VKClient.Common.Graffiti.Views
{
  public class GraffitiPreviewUC : UserControl
  {
    private const int ANIMATION_DURATION = 200;
    private const int HIDE_VELOCITY_THRESHOLD = 500;
    private const int HIDE_TRANSLATION_THRESHOLD = 100;
    private static readonly IEasingFunction ANIMATION_EASING;
    private WriteableBitmap _bitmap;
    private double _height;
    private double _pageHeight;
    private readonly double _pageHeightPortrait;
    private readonly double _pageHeightLandscape;
    private double _contentMarginTop;
    private double _contentMarginTopPortrait;
    private double _contentMarginTopLandscape;
    private bool _isPortrait;
    private bool _isAnimating;
    private bool _isHidden;
    private static DialogService _flyout;
    internal Storyboard ShowStoryboard;
    internal SplineDoubleKeyFrame splineKeyFrameShowBegin;
    internal SplineDoubleKeyFrame splineKeyFrameShowEnd;
    internal Border borderRoot;
    internal RotateTransform rotateRoot;
    internal Rectangle rectBackground;
    internal TranslateTransform translateContent;
    internal Grid gridContent;
    internal Image image;
    internal RotateTransform rotateImage;
    internal TextBlock textBlockSend;
    private bool _contentLoaded;

    public Action SendButtonClickAction { get; set; }

    public Action HideCallback { get; set; }

    static GraffitiPreviewUC()
    {
      CubicEase cubicEase = new CubicEase();
      int num = 0;
      ((EasingFunctionBase) cubicEase).EasingMode = ((EasingMode) num);
      GraffitiPreviewUC.ANIMATION_EASING = (IEasingFunction) cubicEase;
    }

    public GraffitiPreviewUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.textBlockSend.Text = (CommonResources.Captcha_Send.Capitalize());
      Content content = Application.Current.Host.Content;
      this._pageHeightPortrait = content.ActualHeight;
      this._pageHeightLandscape = content.ActualWidth;
    }

    private void Init(WriteableBitmap bitmap)
    {
      this._bitmap = bitmap;
      this.image.Source = ((ImageSource) this._bitmap);
      this.PrepareAnimations();
    }

    public void SetOrientation(DeviceOrientation orientation)
    {
      this._isPortrait = orientation != DeviceOrientation.LandscapeLeft && orientation != DeviceOrientation.LandscapeRight;
      if (this._isPortrait)
      {
        this._height = 496.0;
        this._contentMarginTopPortrait = Math.Round((this._pageHeightPortrait - this._height) / 2.0);
        this._contentMarginTopLandscape = Math.Round((this._pageHeightLandscape - this._height) / 2.0);
        this._contentMarginTop = this._contentMarginTopPortrait;
        this._pageHeight = this._pageHeightPortrait;
        ((FrameworkElement) this.borderRoot).Width = this._pageHeightLandscape;
        this.rotateRoot.CenterX=(this._pageHeightLandscape / 2.0);
        this.rotateRoot.CenterY=(this._pageHeightPortrait / 2.0);
        this.rotateRoot.Angle = 0.0;
      }
      else
      {
        this._height = 432.0;
        this._contentMarginTopPortrait = Math.Round((this._pageHeightPortrait - this._height) / 2.0);
        this._contentMarginTopLandscape = Math.Round((this._pageHeightLandscape - this._height) / 2.0);
        this._contentMarginTop = this._contentMarginTopLandscape;
        this._pageHeight = this._pageHeightLandscape;
        ((FrameworkElement) this.borderRoot).Width = this._pageHeightPortrait;
        if (orientation == DeviceOrientation.LandscapeLeft)
        {
          this.rotateRoot.CenterX=(this._pageHeightLandscape / 2.0);
          this.rotateRoot.CenterY=(this._pageHeightLandscape / 2.0);
          this.rotateRoot.Angle = 90.0;
        }
        else
        {
          this.rotateRoot.CenterX=(this._pageHeightPortrait / 2.0);
          this.rotateRoot.CenterY=(this._pageHeightPortrait / 2.0);
          this.rotateRoot.Angle=(-90.0);
        }
      }
      ((FrameworkElement) this.gridContent).Height = this._height;
      ((FrameworkElement) this.gridContent).Margin=(new Thickness(0.0, this._contentMarginTop, 0.0, 0.0));
      this.UpdateImageSize();
    }

    private void UpdateImageSize()
    {
      if (this._bitmap == null)
        return;
      double val1_1 = (double) ((BitmapSource) this._bitmap).PixelWidth / 2.0;
      double val1_2 = (double) ((BitmapSource) this._bitmap).PixelHeight / 2.0;
      double num = val1_1 / val1_2;
      if (this._isPortrait)
      {
        ((FrameworkElement) this.image).Width=(Math.Min(val1_1, 384.0));
        ((FrameworkElement) this.image).Height=(((FrameworkElement) this.image).Width / num);
      }
      else
      {
        ((FrameworkElement) this.image).Height=(Math.Min(val1_2, 320.0));
        ((FrameworkElement) this.image).Width=(((FrameworkElement) this.image).Height * num);
      }
    }

    private void PrepareAnimations()
    {
      ((UIElement) this.rectBackground).Opacity = 0.0;
      this.translateContent.Y = 96.0;
      ((DoubleKeyFrame) this.splineKeyFrameShowBegin).Value = 96.0;
      ((DoubleKeyFrame) this.splineKeyFrameShowEnd).Value = 0.0;
    }

    private void AnimateShow()
    {
      this._isAnimating = true;
      this.ShowStoryboard.Begin();
    }

    private void ShowStoryboard_OnCompleted(object sender, EventArgs e)
    {
      this._isAnimating = false;
    }

    private void AnimateHide(bool up, Action callback = null)
    {
      if (this._isHidden)
      {
        Action action = callback;
        if (action == null)
          return;
        action();
      }
      else
      {
        this._isAnimating = true;
        if (up)
        {
          List<AnimationInfo> animInfoList = new List<AnimationInfo>();
          animInfoList.Add(new AnimationInfo()
          {
            target = (DependencyObject) this.translateContent,
            from = this.translateContent.Y,
            to = -(this._height + this._contentMarginTop),
            propertyPath = TranslateTransform.YProperty,
            duration = 200,
            easing = GraffitiPreviewUC.ANIMATION_EASING
          });
          animInfoList.Add(new AnimationInfo()
          {
            target = (DependencyObject) this.rectBackground,
            from = ((UIElement) this.rectBackground).Opacity,
            to = 0.0,
            propertyPath = UIElement.OpacityProperty,
            duration = 200,
            easing = GraffitiPreviewUC.ANIMATION_EASING
          });
          int? startTime = new int?();
          Action completed = (Action) (() =>
          {
            this._isAnimating = false;
            this._isHidden = true;
            Action action = callback;
            if (action == null)
              return;
            action();
          });
          AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        }
        else
        {
          List<AnimationInfo> animInfoList = new List<AnimationInfo>();
          animInfoList.Add(new AnimationInfo()
          {
            target = (DependencyObject) this.translateContent,
            from = this.translateContent.Y,
            to = this._pageHeight - this._contentMarginTop,
            propertyPath = TranslateTransform.YProperty,
            duration = 200,
            easing = GraffitiPreviewUC.ANIMATION_EASING
          });
          animInfoList.Add(new AnimationInfo()
          {
            target = (DependencyObject) this.rectBackground,
            from = ((UIElement) this.rectBackground).Opacity,
            to = 0.0,
            propertyPath = UIElement.OpacityProperty,
            duration = 200,
            easing = GraffitiPreviewUC.ANIMATION_EASING
          });
          int? startTime = new int?();
          Action completed = (Action) (() =>
          {
            this._isAnimating = false;
            this._isHidden = true;
            Action action = callback;
            if (action == null)
              return;
            action();
          });
          AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        }
      }
    }

    private void Hide(bool up = false)
    {
      this.AnimateHide(up, (Action) (() =>
      {
        DialogService flyout = GraffitiPreviewUC._flyout;
        if (flyout == null)
          return;
        // ISSUE: explicit non-virtual call
        flyout.Hide();
      }));
    }

    private void AnimateToInitial()
    {
      this._isAnimating = true;
      ((DependencyObject) this.translateContent).Animate(this.translateContent.Y, 0.0, TranslateTransform.YProperty, 175, new int?(), GraffitiPreviewUC.ANIMATION_EASING, (Action) (() => this._isAnimating = false));
    }

    public void Show(WriteableBitmap bitmap, DeviceOrientation orientation)
    {
      this.Init(bitmap);
      this.SetOrientation(orientation);
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      dialogService.AnimationTypeChild = DialogService.AnimationTypes.None;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      int num = 0;
      dialogService.IsOverlayApplied = num != 0;
      Action<Action> action = (Action<Action>) (callback => this.AnimateHide(false, callback));
      dialogService.OnClosingAction = action;
      GraffitiPreviewUC._flyout = dialogService;
      GraffitiPreviewUC._flyout.Closed += (EventHandler) ((sender, args) =>
      {
        Action hideCallback = this.HideCallback;
        if (hideCallback == null)
          return;
        hideCallback();
      });
      GraffitiPreviewUC._flyout.Opened += (EventHandler) ((sender, args) => this.AnimateShow());
      GraffitiPreviewUC._flyout.Child = (FrameworkElement) this;
      GraffitiPreviewUC._flyout.Show( null);
    }

    private void BorderContent_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
    }

    private void Background_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.Hide(false);
    }

    private void BorderContent_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
    }

    private void BorderContent_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      if (e.PinchManipulation != null)
        return;
      e.Handled = true;
      Point translation = e.DeltaManipulation.Translation;
      // ISSUE: explicit reference operation
      this.HandleDragDelta(((Point) @translation).Y);
    }

    private void BorderContent_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
      Point linearVelocity = e.FinalVelocities.LinearVelocity;
      // ISSUE: explicit reference operation
      this.HandleDragCompleted(((Point) @linearVelocity).Y);
    }

    private void HandleDragDelta(double delta)
    {
      TranslateTransform translateContent = this.translateContent;
      double num = translateContent.Y + delta;
      translateContent.Y = num;
    }

    private void HandleDragCompleted(double velocityY)
    {
      if (this._isAnimating)
        return;
      double y = this.translateContent.Y;
      bool? nullable1 = new bool?();
      if (velocityY <= -500.0)
        nullable1 = new bool?(true);
      else if (velocityY >= 500.0)
        nullable1 = new bool?(false);
      if (!nullable1.HasValue)
      {
        if (y <= -100.0)
          nullable1 = new bool?(true);
        else if (y >= 100.0)
          nullable1 = new bool?(false);
      }
      double num = 0.0;
      bool? nullable2 = nullable1;
      bool flag1 = true;
      if ((nullable2.GetValueOrDefault() == flag1 ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
      {
        num = -(this._height + this._contentMarginTop);
      }
      else
      {
        nullable2 = nullable1;
        bool flag2 = false;
        if ((nullable2.GetValueOrDefault() == flag2 ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
          num = this._pageHeight - this._contentMarginTop;
      }
      if (nullable1.HasValue)
      {
        this.Hide(nullable1.Value);
      }
      else
      {
        if (num - this.translateContent.Y == 0.0)
          return;
        this.AnimateToInitial();
      }
    }

    private void SentButton_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Action buttonClickAction = this.SendButtonClickAction;
      if (buttonClickAction == null)
        return;
      buttonClickAction();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Graffiti/Views/GraffitiPreviewUC.xaml", UriKind.Relative));
      this.ShowStoryboard = (Storyboard) base.FindName("ShowStoryboard");
      this.splineKeyFrameShowBegin = (SplineDoubleKeyFrame) base.FindName("splineKeyFrameShowBegin");
      this.splineKeyFrameShowEnd = (SplineDoubleKeyFrame) base.FindName("splineKeyFrameShowEnd");
      this.borderRoot = (Border) base.FindName("borderRoot");
      this.rotateRoot = (RotateTransform) base.FindName("rotateRoot");
      this.rectBackground = (Rectangle) base.FindName("rectBackground");
      this.translateContent = (TranslateTransform) base.FindName("translateContent");
      this.gridContent = (Grid) base.FindName("gridContent");
      this.image = (Image) base.FindName("image");
      this.rotateImage = (RotateTransform) base.FindName("rotateImage");
      this.textBlockSend = (TextBlock) base.FindName("textBlockSend");
    }
  }
}
