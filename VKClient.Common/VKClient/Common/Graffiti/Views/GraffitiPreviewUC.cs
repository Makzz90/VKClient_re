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
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.ImageViewer;
using VKClient.Common.Localization;

using VKClient.Audio.Base.Extensions;

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
      cubicEase.EasingMode = (EasingMode) num;
      GraffitiPreviewUC.ANIMATION_EASING = (IEasingFunction) cubicEase;
    }

    public GraffitiPreviewUC()
    {
      this.InitializeComponent();
      this.textBlockSend.Text = CommonResources.Captcha_Send.Capitalize();
      Content content = Application.Current.Host.Content;
      this._pageHeightPortrait = content.ActualHeight;
      this._pageHeightLandscape = content.ActualWidth;
    }

    private void Init(WriteableBitmap bitmap)
    {
      this._bitmap = bitmap;
      this.image.Source = (ImageSource) this._bitmap;
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
        this.borderRoot.Width = this._pageHeightLandscape;
        this.rotateRoot.CenterX = this._pageHeightLandscape / 2.0;
        this.rotateRoot.CenterY = this._pageHeightPortrait / 2.0;
        this.rotateRoot.Angle = 0.0;
      }
      else
      {
        this._height = 432.0;
        this._contentMarginTopPortrait = Math.Round((this._pageHeightPortrait - this._height) / 2.0);
        this._contentMarginTopLandscape = Math.Round((this._pageHeightLandscape - this._height) / 2.0);
        this._contentMarginTop = this._contentMarginTopLandscape;
        this._pageHeight = this._pageHeightLandscape;
        this.borderRoot.Width = this._pageHeightPortrait;
        if (orientation == DeviceOrientation.LandscapeLeft)
        {
          this.rotateRoot.CenterX = this._pageHeightLandscape / 2.0;
          this.rotateRoot.CenterY = this._pageHeightLandscape / 2.0;
          this.rotateRoot.Angle = 90.0;
        }
        else
        {
          this.rotateRoot.CenterX = this._pageHeightPortrait / 2.0;
          this.rotateRoot.CenterY = this._pageHeightPortrait / 2.0;
          this.rotateRoot.Angle = -90.0;
        }
      }
      this.gridContent.Height = this._height;
      this.gridContent.Margin = new Thickness(0.0, this._contentMarginTop, 0.0, 0.0);
      this.UpdateImageSize();
    }

    private void UpdateImageSize()
    {
      if (this._bitmap == null)
        return;
      double val1_1 = (double) this._bitmap.PixelWidth / 2.0;
      double val1_2 = (double) this._bitmap.PixelHeight / 2.0;
      double num = val1_1 / val1_2;
      if (this._isPortrait)
      {
        this.image.Width = Math.Min(val1_1, 384.0);
        this.image.Height = this.image.Width / num;
      }
      else
      {
        this.image.Height = Math.Min(val1_2, 320.0);
        this.image.Width = this.image.Height * num;
      }
    }

    private void PrepareAnimations()
    {
      this.rectBackground.Opacity = 0.0;
      this.translateContent.Y = 96.0;
      this.splineKeyFrameShowBegin.Value = 96.0;
      this.splineKeyFrameShowEnd.Value = 0.0;
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
            propertyPath = (object) TranslateTransform.YProperty,
            duration = 200,
            easing = GraffitiPreviewUC.ANIMATION_EASING
          });
          animInfoList.Add(new AnimationInfo()
          {
            target = (DependencyObject) this.rectBackground,
            from = this.rectBackground.Opacity,
            to = 0.0,
            propertyPath = (object) UIElement.OpacityProperty,
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
            propertyPath = (object) TranslateTransform.YProperty,
            duration = 200,
            easing = GraffitiPreviewUC.ANIMATION_EASING
          });
          animInfoList.Add(new AnimationInfo()
          {
            target = (DependencyObject) this.rectBackground,
            from = this.rectBackground.Opacity,
            to = 0.0,
            propertyPath = (object) UIElement.OpacityProperty,
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
        DialogService dialogService = GraffitiPreviewUC._flyout;
        if (dialogService == null)
          return;
        dialogService.Hide();
      }));
    }

    private void AnimateToInitial()
    {
      this._isAnimating = true;
      this.translateContent.Animate(this.translateContent.Y, 0.0, (object) TranslateTransform.YProperty, 175, new int?(), GraffitiPreviewUC.ANIMATION_EASING, (Action) (() => this._isAnimating = false));
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
      GraffitiPreviewUC._flyout.Show(null);
    }

    private void BorderContent_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
    }

    private void Background_OnTap(object sender, GestureEventArgs e)
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
      this.HandleDragDelta(e.DeltaManipulation.Translation.Y);
    }

    private void BorderContent_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
      this.HandleDragCompleted(e.FinalVelocities.LinearVelocity.Y);
    }

    private void HandleDragDelta(double delta)
    {
      this.translateContent.Y += delta;
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

    private void SentButton_OnTap(object sender, GestureEventArgs e)
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
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Graffiti/Views/GraffitiPreviewUC.xaml", UriKind.Relative));
      this.ShowStoryboard = (Storyboard) this.FindName("ShowStoryboard");
      this.splineKeyFrameShowBegin = (SplineDoubleKeyFrame) this.FindName("splineKeyFrameShowBegin");
      this.splineKeyFrameShowEnd = (SplineDoubleKeyFrame) this.FindName("splineKeyFrameShowEnd");
      this.borderRoot = (Border) this.FindName("borderRoot");
      this.rotateRoot = (RotateTransform) this.FindName("rotateRoot");
      this.rectBackground = (Rectangle) this.FindName("rectBackground");
      this.translateContent = (TranslateTransform) this.FindName("translateContent");
      this.gridContent = (Grid) this.FindName("gridContent");
      this.image = (Image) this.FindName("image");
      this.rotateImage = (RotateTransform) this.FindName("rotateImage");
      this.textBlockSend = (TextBlock) this.FindName("textBlockSend");
    }
  }
}
