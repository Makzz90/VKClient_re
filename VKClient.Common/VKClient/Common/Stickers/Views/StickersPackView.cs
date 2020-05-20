using Microsoft.Phone.Controls;
using System;
using System.Collections;
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
using VKClient.Common.Library;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Stickers.Views
{
  public class StickersPackView : UserControl, IManipulationHandler, IHandle<StickersPackPurchasedEvent>, IHandle, IHandle<StickersPackActivatedDeactivatedEvent>
  {
    private const int MARGIN_TOP = 32;
    private StockItemHeader _stockItemHeader;
    private double _height;
    private PhoneApplicationPage _currentPage;
    private double _pageHeight;
    private double _pageHeightPortrait;
    private double _pageHeightLandscape;
    private double _contentMarginTop;
    private double _contentMarginTopPortrait;
    private double _contentMarginTopLandscape;
    private bool _isAnimating;
    private bool _isHidden;
    private static DialogService _flyout;
    private const int HIDE_VELOCITY_THRESHOLD = 500;
    private const int HIDE_TRANSLATION_THRESHOLD = 100;
    private static readonly IEasingFunction ANIMATION_EASING;
    internal Style ListBoxItemNavDotsStyle;
    internal Storyboard ShowStoryboard;
    internal SplineDoubleKeyFrame splineKeyFrameShowBegin;
    internal SplineDoubleKeyFrame splineKeyFrameShowEnd;
    internal Rectangle rectBackground;
    internal TranslateTransform translateContent;
    internal Border borderContent;
    internal ScrollViewer scrollViewerContent;
    internal Grid gridSlideView;
    internal SlideView slideView;
    internal ListBox listBoxNavDots;
    internal StickersPackInfoUC ucStickersPackInfo;
    internal TextBlock textBlockDescription;
    private bool _contentLoaded;

    public bool? Handled { get; set; }

    static StickersPackView()
    {
      CubicEase cubicEase = new CubicEase();
      int num = 0;
      cubicEase.EasingMode = (EasingMode) num;
      StickersPackView.ANIMATION_EASING = (IEasingFunction) cubicEase;
    }

    public StickersPackView()
    {
      this.InitializeComponent();
      this.textBlockDescription.Visibility = Visibility.Collapsed;
      this.textBlockDescription.Text = "";
      this.slideView.ParentManipulationHandler = (IManipulationHandler) this;
      EventAggregator.Current.Subscribe((object) this);
      this.Loaded += (RoutedEventHandler) ((sender, args) =>
      {
        if (this._currentPage == null)
          return;
        this._currentPage.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(this.CurrentPage_OnOrientationChanged);
      });
      this.Unloaded += (RoutedEventHandler) ((sender, args) =>
      {
        if (this._currentPage == null)
          return;
        this._currentPage.OrientationChanged -= new EventHandler<OrientationChangedEventArgs>(this.CurrentPage_OnOrientationChanged);
      });
    }

    private void CurrentPage_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      this.UpdateValuesForOrientation();
    }

    private void Init(StockItemHeader stockItemHeader, string referrer)
    {
      this._stockItemHeader = stockItemHeader;
      this.DataContext = (object) this._stockItemHeader;
      if (!string.IsNullOrEmpty(referrer))
        this.ucStickersPackInfo.Referrer = referrer;
      string description = this._stockItemHeader.Description;
      if (!string.IsNullOrEmpty(description))
      {
        this.textBlockDescription.Visibility = Visibility.Visible;
        this.textBlockDescription.Text = description;
      }
      this._currentPage = (PhoneApplicationPage) FramePageUtils.CurrentPage;
      Content content = Application.Current.Host.Content;
      this._pageHeightPortrait = content.ActualHeight;
      this._pageHeightLandscape = content.ActualWidth;
      this._height = 0.0;
      this._height = this._height + this.gridSlideView.Height;
      double num1 = this._height;
      double num2 = this.ucStickersPackInfo.Height + this.ucStickersPackInfo.Margin.Top;
      Thickness margin = this.ucStickersPackInfo.Margin;
      double bottom1 = margin.Bottom;
      double num3 = num2 + bottom1;
      this._height = num1 + num3;
      double num4 = this._height;
      double actualHeight = this.textBlockDescription.ActualHeight;
      margin = this.textBlockDescription.Margin;
      double top = margin.Top;
      double num5 = actualHeight + top;
      margin = this.textBlockDescription.Margin;
      double bottom2 = margin.Bottom;
      double num6 = num5 + bottom2;
      this._height = num4 + num6;
      this._height = Math.Round(this._height);
      this._contentMarginTopPortrait = Math.Round((this._pageHeightPortrait - this._height) / 2.0);
      this._contentMarginTopLandscape = Math.Round((this._pageHeightLandscape - this._height) / 2.0);
      this.UpdateValuesForOrientation();
      this.PrepareAnimations();
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
        this.scrollViewerContent.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
      }
      else
        this.scrollViewerContent.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
      this.borderContent.MaxHeight = this._pageHeight - this._contentMarginTop * 2.0;
      this.borderContent.Margin = new Thickness(0.0, this._contentMarginTop, 0.0, 0.0);
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
            easing = StickersPackView.ANIMATION_EASING
          });
          animInfoList.Add(new AnimationInfo()
          {
            target = (DependencyObject) this.rectBackground,
            from = this.rectBackground.Opacity,
            to = 0.0,
            propertyPath = (object) UIElement.OpacityProperty,
            duration = 200,
            easing = StickersPackView.ANIMATION_EASING
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
            easing = StickersPackView.ANIMATION_EASING
          });
          animInfoList.Add(new AnimationInfo()
          {
            target = (DependencyObject) this.rectBackground,
            from = this.rectBackground.Opacity,
            to = 0.0,
            propertyPath = (object) UIElement.OpacityProperty,
            duration = 200,
            easing = StickersPackView.ANIMATION_EASING
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
        DialogService dialogService = StickersPackView._flyout;
        if (dialogService == null)
          return;
         dialogService.Hide();
      }));
    }

    private void AnimateToInitial()
    {
      this._isAnimating = true;
      this.translateContent.Animate(this.translateContent.Y, 0.0, (object) TranslateTransform.YProperty, 175, new int?(), StickersPackView.ANIMATION_EASING, (Action) (() => this._isAnimating = false));
    }

    private void SlideView_OnSelectionChanged(object sender, int e)
    {
      if (this._stockItemHeader == null)
        return;
      List<string> demoPhotos = this._stockItemHeader.DemoPhotos;
      if ((demoPhotos != null ? ( (demoPhotos.Count) == 0 ? 1 : 0) : 0) != 0)
        return;
      if (this.listBoxNavDots.ItemsSource == null)
        this.listBoxNavDots.ItemsSource = (IEnumerable) this._stockItemHeader.DemoPhotos;
      this.listBoxNavDots.SelectedIndex = e;
    }

    private static void ShowWithLoader(string referrer, Action<Action<BackendResult<StockItem, ResultCode>>, CancellationToken> loadAction)
    {
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      FullscreenLoader fullscreenLoader = new FullscreenLoader();
      fullscreenLoader.HideOnBackKeyPress = true;
      Action<FullscreenLoaderHiddenEventArgs> action = (Action<FullscreenLoaderHiddenEventArgs>) (args => cancellationTokenSource.Cancel());
      fullscreenLoader.HiddenCallback = action;
      FullscreenLoader loader = fullscreenLoader;
      loader.Show(null, true);
      loadAction((Action<BackendResult<StockItem, ResultCode>>) (result =>
      {
        loader.Hide(false);
        if (result.ResultCode == ResultCode.Succeeded)
          Execute.ExecuteOnUIThread((Action) (() => StickersPackView.Show(result.ResultData, referrer)));
        else
          GenericInfoUC.ShowBasedOnResult((int) result.ResultCode, "", (VKRequestsDispatcher.Error) null);
      }), cancellationTokenSource.Token);
    }

    public static void ShowAndReloadStickers(long stickerId, string referrer)
    {
      StickersPackView.ShowWithLoader(referrer, (Action<Action<BackendResult<StockItem, ResultCode>>, CancellationToken>) ((callback, cancellationToken) => StoreService.Instance.GetStockItemByStickerId(stickerId, (Action<BackendResult<StockItem, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          StockItem resultData = result.ResultData;
          StoreProduct storeProduct = resultData != null ? resultData.product : (StoreProduct) null;
          if (storeProduct != null && storeProduct.purchased == 1 && storeProduct.active == 1)
            EventAggregator.Current.Publish((object) new StickersUpdatedEvent(new StockItemHeader(resultData, false)));
        }
        Action<BackendResult<StockItem, ResultCode>> action = callback;
        if (action == null)
          return;
        BackendResult<StockItem, ResultCode> backendResult = result;
        action(backendResult);
      }), new CancellationToken?(cancellationToken))));
    }

    public static void Show(long stickerId, string referrer)
    {
      StickersPackView.ShowWithLoader(referrer, (Action<Action<BackendResult<StockItem, ResultCode>>, CancellationToken>) ((callback, cancellationToken) => StoreService.Instance.GetStockItemByStickerId(stickerId, callback, new CancellationToken?(cancellationToken))));
    }

    public static void Show(string stickersPackName, string referrer)
    {
      StickersPackView.ShowWithLoader(referrer, (Action<Action<BackendResult<StockItem, ResultCode>>, CancellationToken>) ((callback, cancellationToken) => StoreService.Instance.GetStockItemByName(stickersPackName, callback, new CancellationToken?(cancellationToken))));
    }

    public static void Show(StockItem stockItem, string referrer)
    {
      StickersPackView.Show(new StockItemHeader(stockItem, false), referrer);
    }

    public static void Show(StockItemHeader stockItemHeader, string referrer)
    {
      StickersPackView stickersPackView = new StickersPackView();
      stickersPackView.Init(stockItemHeader, referrer);
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      dialogService.AnimationTypeChild = DialogService.AnimationTypes.None;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      int num = 0;
      dialogService.IsOverlayApplied = num != 0;
      Action<Action> action = (Action<Action>) (callback => stickersPackView.AnimateHide(false, callback));
      dialogService.OnClosingAction = action;
      StickersPackView._flyout = dialogService;
      StickersPackView._flyout.Opened += (EventHandler) ((sender, args) =>
      {
        stickersPackView.AnimateShow();
        EventAggregator.Current.Publish((object) new StickersPurchaseFunnelEvent(StickersPurchaseFunnelAction.sticker_page));
      });
      StickersPackView._flyout.Child = (FrameworkElement) stickersPackView;
      StickersPackView._flyout.Show(null);
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
      Point translation = e.DeltaManipulation.Translation;
      bool? handled = this.Handled;
      if (!handled.HasValue)
      {
        this.Handled = new bool?(Math.Abs(translation.Y) > Math.Abs(translation.X));
        handled = this.Handled;
        if (!handled.Value)
          return;
      }
      e.Handled = true;
      this.HandleDragDelta(translation.Y);
    }

    private void BorderContent_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
      bool? handled = this.Handled;
      this.Handled = new bool?();
      if (!handled.HasValue || !handled.Value)
        return;
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

    public void Handle(StickersPackPurchasedEvent message)
    {
      int productId = message.StockItemHeader.ProductId;
      StockItemHeader stockItemHeader = this._stockItemHeader;
      int? nullable = stockItemHeader != null ? new int?(stockItemHeader.ProductId) : new int?();
      int valueOrDefault = nullable.GetValueOrDefault();
      if ((productId == valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) == 0)
        return;
      new DelayedExecutor(800).AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() => this.Hide(false)))));
    }

    public void Handle(StickersPackActivatedDeactivatedEvent message)
    {
      int productId = message.StockItemHeader.ProductId;
      StockItemHeader stockItemHeader = this._stockItemHeader;
      int? nullable = stockItemHeader != null ? new int?(stockItemHeader.ProductId) : new int?();
      int valueOrDefault = nullable.GetValueOrDefault();
      if ((productId == valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) == 0)
        return;
      new DelayedExecutor(800).AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() => this.Hide(false)))));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Stickers/Views/StickersPackView.xaml", UriKind.Relative));
      this.ListBoxItemNavDotsStyle = (Style) this.FindName("ListBoxItemNavDotsStyle");
      this.ShowStoryboard = (Storyboard) this.FindName("ShowStoryboard");
      this.splineKeyFrameShowBegin = (SplineDoubleKeyFrame) this.FindName("splineKeyFrameShowBegin");
      this.splineKeyFrameShowEnd = (SplineDoubleKeyFrame) this.FindName("splineKeyFrameShowEnd");
      this.rectBackground = (Rectangle) this.FindName("rectBackground");
      this.translateContent = (TranslateTransform) this.FindName("translateContent");
      this.borderContent = (Border) this.FindName("borderContent");
      this.scrollViewerContent = (ScrollViewer) this.FindName("scrollViewerContent");
      this.gridSlideView = (Grid) this.FindName("gridSlideView");
      this.slideView = (SlideView) this.FindName("slideView");
      this.listBoxNavDots = (ListBox) this.FindName("listBoxNavDots");
      this.ucStickersPackInfo = (StickersPackInfoUC) this.FindName("ucStickersPackInfo");
      this.textBlockDescription = (TextBlock) this.FindName("textBlockDescription");
    }
  }
}
