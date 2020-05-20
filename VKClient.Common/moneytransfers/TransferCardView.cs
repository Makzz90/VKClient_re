using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.ImageViewer;
using VKClient.Common.MoneyTransfers.Library;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.MoneyTransfers
{
  public class TransferCardView : UserControl, IHandle<MoneyTransferAcceptedEvent>, IHandle, IHandle<MoneyTransferSentEvent>
  {
    private double _height;
    private bool? _handled;
    private PhoneApplicationPage _currentPage;
    private static DialogService _flyout;
    private double _pageHeight;
    private double _pageHeightPortrait;
    private double _pageHeightLandscape;
    private double _contentMarginTop;
    private double _contentMarginTopPortrait;
    private double _contentMarginTopLandscape;
    private static readonly IEasingFunction ANIMATION_EASING;
    private bool _isAnimating;
    private bool _isHidden;
    private const int HIDE_VELOCITY_THRESHOLD = 500;
    private const int HIDE_TRANSLATION_THRESHOLD = 100;
    internal Rectangle rectBackground;
    internal TranslateTransform translateContent;
    internal Border contentBorder;
    internal ScrollViewer scrollViewerContent;
    internal Storyboard ShowStoryboard;
    internal SplineDoubleKeyFrame splineKeyFrameShowBegin;
    internal SplineDoubleKeyFrame splineKeyFrameShowEnd;
    private bool _contentLoaded;

    public MoneyTransferViewModel ViewModel
    {
      get
      {
        return base.DataContext as MoneyTransferViewModel;
      }
    }

    static TransferCardView()
    {
      CubicEase cubicEase = new CubicEase();
      int num = 0;
      ((EasingFunctionBase) cubicEase).EasingMode = ((EasingMode) num);
      TransferCardView.ANIMATION_EASING = (IEasingFunction) cubicEase;
    }

    public TransferCardView()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      base.Loaded+=(delegate(object sender, RoutedEventArgs args)
      {
          if (this._currentPage == null)
          {
              return;
          }
          this._currentPage.OrientationChanged+=(new EventHandler<OrientationChangedEventArgs>(this.CurrentPage_OnOrientationChanged));
      });
      base.Unloaded+=(delegate(object sender, RoutedEventArgs args)
      {
          if (this._currentPage == null)
          {
              return;
          }
          this._currentPage.OrientationChanged-=(new EventHandler<OrientationChangedEventArgs>(this.CurrentPage_OnOrientationChanged));
      });
      EventAggregator.Current.Subscribe(this);
    }

    private void CurrentPage_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      this.UpdateValuesForOrientation();
    }

    private void Initialize(MoneyTransferViewModel transfer)
    {
      base.DataContext = transfer;
      this._currentPage = (PhoneApplicationPage) FramePageUtils.CurrentPage;
      Content content = Application.Current.Host.Content;
      this._pageHeightPortrait = content.ActualHeight;
      this._pageHeightLandscape = content.ActualWidth;
      this._height = 274.0;
      if (transfer.CommentVisibility == Visibility.Visible)
      {
        this._height = this._height + 8.0;
        this._height = this._height + this.MeasureCommentHeight();
      }
      if (transfer.CardButtonsVisibility == Visibility.Visible)
        this._height = this._height + 56.0;
      if (transfer.ActionButtonVisibility == Visibility.Visible)
        this._height = this._height + 56.0;
      this._height = Math.Round(this._height);
      this._contentMarginTopPortrait = Math.Round((this._pageHeightPortrait - this._height) / 2.0);
      this._contentMarginTopLandscape = Math.Round((this._pageHeightLandscape - this._height) / 2.0);
      this.UpdateValuesForOrientation();
      this.PrepareAnimations();
    }

    private double MeasureCommentHeight()
    {
      TextBlock textBlock = new TextBlock();
      double num1 = 22.777;
      textBlock.FontSize = num1;
      string comment = this.ViewModel.Comment;
      textBlock.Text = comment;
      int num2 = 2;
      textBlock.TextWrapping=((TextWrapping) num2);
      int num3 = 0;
      textBlock.TextAlignment=((TextAlignment) num3);
      double num4 = 32.0;
      textBlock.LineHeight = num4;
      double num5 = 400.0;
      ((FrameworkElement) textBlock).Width = num5;
      return ((FrameworkElement) textBlock).ActualHeight;
    }

    private void UpdateValuesForOrientation()
    {
        if ((this._currentPage == null ? 0 : (this._currentPage.Orientation == PageOrientation.Landscape || this._currentPage.Orientation == PageOrientation.LandscapeLeft ? 1 : (this._currentPage.Orientation == PageOrientation.LandscapeRight ? 1 : 0))) != 0)
      {
        this._contentMarginTop = this._contentMarginTopLandscape;
        this._pageHeight = this._pageHeightLandscape;
      }
      else
      {
        this._contentMarginTop = this._contentMarginTopPortrait;
        this._pageHeight = this._pageHeightPortrait;
      }
      if (this._contentMarginTop < 32.0)
      {
        this._contentMarginTop = 32.0;
        this.scrollViewerContent.VerticalScrollBarVisibility=((ScrollBarVisibility) 3);
      }
      else
        this.scrollViewerContent.VerticalScrollBarVisibility=((ScrollBarVisibility) 0);
      ((FrameworkElement) this.contentBorder).MaxHeight=(this._pageHeight - this._contentMarginTop * 2.0);
      ((FrameworkElement) this.contentBorder).Margin=(new Thickness(0.0, this._contentMarginTop, 0.0, 0.0));
    }

    private void PrepareAnimations()
    {
      ((UIElement) this.rectBackground).Opacity = 0.0;
      this.translateContent.Y = 96.0;
      ((DoubleKeyFrame) this.splineKeyFrameShowBegin).Value = 96.0;
      ((DoubleKeyFrame) this.splineKeyFrameShowEnd).Value = 0.0;
    }

    private void ShowStoryboard_OnCompleted(object sender, EventArgs e)
    {
      this._isAnimating = false;
    }

    private void AnimateShow()
    {
      this._isAnimating = true;
      this.ShowStoryboard.Begin();
    }

    private void AnimateHide(bool up, Action callback)
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
        List<AnimationInfo> animInfoList = new List<AnimationInfo>();
        animInfoList.Add(new AnimationInfo()
        {
          target = (DependencyObject) this.translateContent,
          from = this.translateContent.Y,
          to = up ? -(this._height + this._contentMarginTop) : this._pageHeight - this._contentMarginTop,
          propertyPath = TranslateTransform.YProperty,
          duration = 200,
          easing = TransferCardView.ANIMATION_EASING
        });
        animInfoList.Add(new AnimationInfo()
        {
          target = (DependencyObject) this.rectBackground,
          from = ((UIElement) this.rectBackground).Opacity,
          to = 0.0,
          propertyPath = UIElement.OpacityProperty,
          duration = 200,
          easing = TransferCardView.ANIMATION_EASING
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

    private void Hide(bool up = false)
    {
      this.AnimateHide(up, (Action) (() =>
      {
        DialogService flyout = TransferCardView._flyout;
        if (flyout == null)
          return;
        // ISSUE: explicit non-virtual call
        flyout.Hide();
      }));
    }

    private void AnimateToInitial()
    {
      this._isAnimating = true;
      ((DependencyObject) this.translateContent).Animate(this.translateContent.Y, 0.0, TranslateTransform.YProperty, 175, new int?(), TransferCardView.ANIMATION_EASING, (Action) (() => this._isAnimating = false));
    }

    private static void Show(MoneyTransferViewModel viewModel)
    {
      DialogService flyout = TransferCardView._flyout;
      if ((flyout != null ? (flyout.IsOpen ? 1 : 0) : 0) != 0)
        return;
      TransferCardView view = new TransferCardView();
      view.Initialize(viewModel);
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      dialogService.AnimationTypeChild = DialogService.AnimationTypes.None;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      int num1 = 0;
      dialogService.HideOnNavigation = num1 != 0;
      int num2 = 0;
      dialogService.IsOverlayApplied = num2 != 0;
      Action<Action> action = (Action<Action>) (callback => view.AnimateHide(false, callback));
      dialogService.OnClosingAction = action;
      TransferCardView._flyout = dialogService;
      TransferCardView._flyout.Opened += (EventHandler) ((sender, args) => view.AnimateShow());
      TransferCardView._flyout.Child = (FrameworkElement) view;
      TransferCardView._flyout.Show( null);
    }

    public static void Show(long id, long fromId, long toId)
    {
      TransferCardView.ShowWithLoader((Action<Action<BackendResult<MoneyTransferResponse, ResultCode>>, CancellationToken>) ((callback, cancellationToken) => MoneyTransfersService.GetTransfer(id, fromId, toId, callback, new CancellationToken?(cancellationToken))));
    }

    private static void ShowWithLoader(Action<Action<BackendResult<MoneyTransferResponse, ResultCode>>, CancellationToken> loadAction)
    {
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      FullscreenLoader fullscreenLoader = new FullscreenLoader();
      fullscreenLoader.HideOnBackKeyPress = true;
      Action<FullscreenLoaderHiddenEventArgs> action = (Action<FullscreenLoaderHiddenEventArgs>) (args => cancellationTokenSource.Cancel());
      fullscreenLoader.HiddenCallback = action;
      FullscreenLoader loader = fullscreenLoader;
      loader.Show( null, true);
      loadAction((Action<BackendResult<MoneyTransferResponse, ResultCode>>) (result =>
      {
        loader.Hide(false);
        ResultCode resultCode = result.ResultCode;
        if (resultCode == ResultCode.Succeeded)
          Execute.ExecuteOnUIThread((Action) (() =>
          {
            MoneyTransferResponse resultData = result.ResultData;
            if (resultData == null)
              return;
            MoneyTransfer transfer = resultData.transfer;
            User user = resultData.user;
            if (transfer == null || user == null)
              return;
            TransferCardView.Show(new MoneyTransferViewModel(transfer, user));
          }));
        else
          GenericInfoUC.ShowBasedOnResult(resultCode, "", null);
      }), cancellationTokenSource.Token);
    }

    private void Background_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.Hide(false);
    }

    private void ContentBorder_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
    }

    private void ContentBorder_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
    }

    private void ContentBorder_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      if (e.PinchManipulation != null)
        return;
      Point translation = e.DeltaManipulation.Translation;
      if (!this._handled.HasValue || !this._handled.Value)
      {
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        this._handled = new bool?(Math.Abs(((Point) @translation).Y) > Math.Abs(((Point) @translation).X));
        if (!this._handled.Value)
          return;
      }
      e.Handled = true;
      // ISSUE: explicit reference operation
      this.HandleDragDelta(((Point) @translation).Y);
    }

    private void ContentBorder_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
      bool? handled = this._handled;
      this._handled = new bool?();
      if (!handled.HasValue || !handled.Value)
        return;
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

    private void Title_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.NavigateToUserProfile(this.ViewModel.UserId, "", "", false);
    }

    private void Button_OnClicked(object sender, RoutedEventArgs e)
    {
      if (int.Parse(((FrameworkElement) sender).Tag.ToString()) == 1)
        this.ViewModel.AcceptTransfer();
      else
        this.ViewModel.DeclineTransfer((Action) (() => Execute.ExecuteOnUIThread((Action) (() => this.Hide(false)))));
    }

    private void ActionButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.ViewModel.Transfer.IsAwaits)
        this.ViewModel.CancelTransfer((Action) (() => Execute.ExecuteOnUIThread((Action) (() => this.Hide(false)))));
      else
        this.ViewModel.RetryTransfer();
    }

    public void Handle(MoneyTransferAcceptedEvent message)
    {
      this.Hide(false);
    }

    public void Handle(MoneyTransferSentEvent message)
    {
      this.Hide(false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/MoneyTransfers/TransferCardView.xaml", UriKind.Relative));
      this.rectBackground = (Rectangle) base.FindName("rectBackground");
      this.translateContent = (TranslateTransform) base.FindName("translateContent");
      this.contentBorder = (Border) base.FindName("contentBorder");
      this.scrollViewerContent = (ScrollViewer) base.FindName("scrollViewerContent");
      this.ShowStoryboard = (Storyboard) base.FindName("ShowStoryboard");
      this.splineKeyFrameShowBegin = (SplineDoubleKeyFrame) base.FindName("splineKeyFrameShowBegin");
      this.splineKeyFrameShowEnd = (SplineDoubleKeyFrame) base.FindName("splineKeyFrameShowEnd");
    }
  }
}
