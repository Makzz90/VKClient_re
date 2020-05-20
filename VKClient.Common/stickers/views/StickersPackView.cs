using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
using VKClient.Common.Gifts.ViewModels;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Stickers.Views
{
  public class StickersPackView : UserControl, IManipulationHandler, IHandle<StickersPackPurchasedEvent>, IHandle, IHandle<StickersPackActivatedDeactivatedEvent>, IHandle<GiftSentEvent>
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
    internal Grid gridSendAsAGift;
    private bool _contentLoaded;

    public bool? Handled { get; set; }

    static StickersPackView()
    {
      CubicEase cubicEase = new CubicEase();
      int num = 0;
      ((EasingFunctionBase) cubicEase).EasingMode = ((EasingMode) num);
      StickersPackView.ANIMATION_EASING = (IEasingFunction) cubicEase;
    }

    public StickersPackView()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((UIElement) this.textBlockDescription).Visibility = Visibility.Collapsed;
      this.textBlockDescription.Text = ("");
      ((UIElement) this.gridSendAsAGift).Visibility = Visibility.Collapsed;
      this.slideView.ParentManipulationHandler = (IManipulationHandler) this;
      EventAggregator.Current.Subscribe(this);
      // ISSUE: method pointer
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
    }
    private void CurrentPage_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      this.UpdateValuesForOrientation();
    }

    private void Init(StockItemHeader stockItemHeader, string referrer)
    {
      this._stockItemHeader = stockItemHeader;
      base.DataContext = this._stockItemHeader;
      if (!string.IsNullOrEmpty(referrer))
        this.ucStickersPackInfo.Referrer = referrer;
      string description = this._stockItemHeader.Description;
      if (!string.IsNullOrEmpty(description))
      {
        ((UIElement) this.textBlockDescription).Visibility = Visibility.Visible;
        this.textBlockDescription.Text = description;
      }
      if (this._stockItemHeader.Price > 0 && (this._stockItemHeader.CanPurchaseFor || this._stockItemHeader.IsChat))
        ((UIElement) this.gridSendAsAGift).Visibility = Visibility.Visible;
      this._currentPage = (PhoneApplicationPage) FramePageUtils.CurrentPage;
      Content content = Application.Current.Host.Content;
      this._pageHeightPortrait = content.ActualHeight;
      this._pageHeightLandscape = content.ActualWidth;
      this._height = 0.0;
      this._height = this._height + ((FrameworkElement) this.gridSlideView).Height;
      double height1 = this._height;
      double height2 = ((FrameworkElement) this.ucStickersPackInfo).Height;
      Thickness margin1 = ((FrameworkElement) this.ucStickersPackInfo).Margin;
      // ISSUE: explicit reference operation
      double top1 = margin1.Top;
      double num1 = height2 + top1;
      Thickness margin2 = ((FrameworkElement) this.ucStickersPackInfo).Margin;
      // ISSUE: explicit reference operation
      double bottom1 = ((Thickness) @margin2).Bottom;
      double num2 = num1 + bottom1;
      this._height = height1 + num2;
      double height3 = this._height;
      double actualHeight = ((FrameworkElement) this.textBlockDescription).ActualHeight;
      Thickness margin3 = ((FrameworkElement) this.textBlockDescription).Margin;
      // ISSUE: explicit reference operation
      double top2 = ((Thickness) @margin3).Top;
      double num3 = actualHeight + top2;
      Thickness margin4 = ((FrameworkElement) this.textBlockDescription).Margin;
      // ISSUE: explicit reference operation
      double bottom2 = ((Thickness) @margin4).Bottom;
      double num4 = num3 + bottom2;
      this._height = height3 + num4;
      if (((UIElement) this.gridSendAsAGift).Visibility == Visibility.Visible)
      {
        double height4 = this._height;
        double height5 = ((FrameworkElement) this.gridSendAsAGift).Height;
        Thickness margin5 = ((FrameworkElement) this.gridSendAsAGift).Margin;
        // ISSUE: explicit reference operation
        double top3 = ((Thickness) @margin5).Top;
        double num5 = height5 + top3;
        Thickness margin6 = ((FrameworkElement) this.gridSendAsAGift).Margin;
        // ISSUE: explicit reference operation
        double bottom3 = ((Thickness) @margin6).Bottom;
        double num6 = num5 + bottom3;
        this._height = height4 + num6;
      }
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
        this.scrollViewerContent.VerticalScrollBarVisibility=((ScrollBarVisibility) 3);
      }
      else
        this.scrollViewerContent.VerticalScrollBarVisibility=((ScrollBarVisibility) 0);
      ((FrameworkElement) this.borderContent).MaxHeight=(this._pageHeight - this._contentMarginTop * 2.0);
      ((FrameworkElement) this.borderContent).Margin=(new Thickness(0.0, this._contentMarginTop, 0.0, 0.0));
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
            easing = StickersPackView.ANIMATION_EASING
          });
          animInfoList.Add(new AnimationInfo()
          {
            target = (DependencyObject) this.rectBackground,
            from = ((UIElement) this.rectBackground).Opacity,
            to = 0.0,
            propertyPath = UIElement.OpacityProperty,
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
            propertyPath = TranslateTransform.YProperty,
            duration = 200,
            easing = StickersPackView.ANIMATION_EASING
          });
          animInfoList.Add(new AnimationInfo()
          {
            target = (DependencyObject) this.rectBackground,
            from = ((UIElement) this.rectBackground).Opacity,
            to = 0.0,
            propertyPath = UIElement.OpacityProperty,
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
        DialogService flyout = StickersPackView._flyout;
        if (flyout == null)
          return;
        // ISSUE: explicit non-virtual call
        flyout.Hide();
      }));
    }

    private void AnimateToInitial()
    {
      this._isAnimating = true;
      ((DependencyObject) this.translateContent).Animate(this.translateContent.Y, 0.0, TranslateTransform.YProperty, 175, new int?(), StickersPackView.ANIMATION_EASING, (Action) (() => this._isAnimating = false));
    }

    private void SlideView_OnSelectionChanged(object sender, int e)
    {
      if (this._stockItemHeader == null)
        return;
      List<string> demoPhotos = this._stockItemHeader.DemoPhotos;
      // ISSUE: explicit non-virtual call
      if ((demoPhotos != null ? (demoPhotos.Count == 0 ? 1 : 0) : 0) != 0)
        return;
      if (((ItemsControl) this.listBoxNavDots).ItemsSource == null)
        ((ItemsControl) this.listBoxNavDots).ItemsSource = ((IEnumerable) this._stockItemHeader.DemoPhotos);
      ((Selector) this.listBoxNavDots).SelectedIndex = e;
    }

    private static void ShowWithLoader(string referrer, Action<Action<BackendResult<StockItem, ResultCode>>, CancellationToken> loadAction, long userOrChatId = 0, bool isChat = false)
    {
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      FullscreenLoader fullscreenLoader = new FullscreenLoader();
      fullscreenLoader.HideOnBackKeyPress = true;
      Action<FullscreenLoaderHiddenEventArgs> action = (Action<FullscreenLoaderHiddenEventArgs>) (args => cancellationTokenSource.Cancel());
      fullscreenLoader.HiddenCallback = action;
      FullscreenLoader loader = fullscreenLoader;
      loader.Show( null, true);
      loadAction((Action<BackendResult<StockItem, ResultCode>>) (result =>
      {
        loader.Hide(false);
        if (result.ResultCode == ResultCode.Succeeded)
          Execute.ExecuteOnUIThread((Action) (() => StickersPackView.Show(result.ResultData, referrer, userOrChatId, isChat)));
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
          StoreProduct storeProduct = resultData != null ? resultData.product :  null;
          if (storeProduct != null && storeProduct.purchased == 1 && storeProduct.active == 1)
            EventAggregator.Current.Publish(new StickersUpdatedEvent(new StockItemHeader(resultData, false, 0, false)));
        }
        Action<BackendResult<StockItem, ResultCode>> action = callback;
        if (action == null)
          return;
        BackendResult<StockItem, ResultCode> backendResult = result;
        action(backendResult);
      }), new CancellationToken?(cancellationToken))), 0, false);
    }

    public static void Show(long stickerId, string referrer)
    {
      StickersPackView.ShowWithLoader(referrer, (Action<Action<BackendResult<StockItem, ResultCode>>, CancellationToken>) ((callback, cancellationToken) => StoreService.Instance.GetStockItemByStickerId(stickerId, callback, new CancellationToken?(cancellationToken))), 0, false);
    }

    public static void Show(string stickersPackName, string referrer)
    {
      StickersPackView.ShowWithLoader(referrer, (Action<Action<BackendResult<StockItem, ResultCode>>, CancellationToken>) ((callback, cancellationToken) => StoreService.Instance.GetStockItemByName(stickersPackName, callback, new CancellationToken?(cancellationToken))), 0, false);
    }

    public static void Show(StockItem stockItem, string referrer, long userOrChatId = 0, bool isChat = false)
    {
      StickersPackView.Show(new StockItemHeader(stockItem, false, userOrChatId, isChat), referrer);
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
      int num1 = 0;
      dialogService.IsOverlayApplied = num1 != 0;
      int num2 = 0;
      dialogService.HideOnNavigation = num2 != 0;
      Action<Action> action = (Action<Action>) (callback => stickersPackView.AnimateHide(false, callback));
      dialogService.OnClosingAction = action;
      StickersPackView._flyout = dialogService;
      StickersPackView._flyout.Opened += (EventHandler) ((sender, args) =>
      {
        stickersPackView.AnimateShow();
        EventAggregator.Current.Publish(new StickersPurchaseFunnelEvent(StickersPurchaseFunnelAction.sticker_page));
      });
      StickersPackView._flyout.Child = (FrameworkElement) stickersPackView;
      StickersPackView._flyout.Show( null);
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
      if (handled.HasValue)
      {
        handled = this.Handled;
        if (handled.Value)
          goto label_5;
      }
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      this.Handled = new bool?(Math.Abs(((Point) @translation).Y) > Math.Abs(((Point) @translation).X));
      handled = this.Handled;
      if (!handled.Value)
        return;
label_5:
      e.Handled = true;
      // ISSUE: explicit reference operation
      this.HandleDragDelta(((Point) @translation).Y);
    }

    private void BorderContent_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
      bool? handled = this.Handled;
      this.Handled = new bool?();
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

    private void SendAsAGift_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._stockItemHeader == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.stickers_present, GiftPurchaseStepsAction.store));
      EventAggregator.Current.Publish(new StickersPurchaseFunnelEvent(StickersPurchaseFunnelAction.present_button_clicked));
      long productId = (long) this._stockItemHeader.ProductId;
      long userOrChatId = this._stockItemHeader.UserOrChatId;
      bool isChat = this._stockItemHeader.IsChat;
      if (productId == 0L || userOrChatId == 0L)
        return;
      CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      FullscreenLoader fullscreenLoader = new FullscreenLoader();
      fullscreenLoader.HideOnBackKeyPress = true;
      Action<FullscreenLoaderHiddenEventArgs> action = (Action<FullscreenLoaderHiddenEventArgs>) (args => cancellationTokenSource.Cancel());
      fullscreenLoader.HiddenCallback = action;
      FullscreenLoader loader = fullscreenLoader;
      loader.Show( null, true);
      GiftsService.Instance.GetGiftInfoFromStore(productId, userOrChatId, isChat, (Action<BackendResult<GiftInfoFromStoreResponse, ResultCode>>) (result =>
      {
        loader.Hide(false);
        if (result.ResultCode == ResultCode.Succeeded)
        {
          GiftInfoFromStoreResponse resultData = result.ResultData;
          List<long> userIds = resultData.userIds;
          GiftsSectionItem giftItem = resultData.giftItem;
          Gift gift = giftItem.gift;
          if (userIds == null || userIds.Count == 0)
          {
            Execute.ExecuteOnUIThread((Action) (() => MessageBox.Show(isChat ? CommonResources.AllChatParticipantsHaveStickerPack : CommonResources.UserAlreadyHasStickerPack, CommonResources.StickerPack, (MessageBoxButton) 0)));
          }
          else
          {
            if (giftItem == null || gift == null)
              return;
            Navigator.Current.NavigateToGiftSend(gift.id, "stickers", giftItem.description, gift.thumb_256, giftItem.price, giftItem.gifts_left, userIds, true);
          }
        }
        else
          GenericInfoUC.ShowBasedOnResult((int) result.ResultCode, "", (VKRequestsDispatcher.Error) null);
      }), new CancellationToken?(cancellationTokenSource.Token));
    }

    private void HideWithDelay()
    {
      new DelayedExecutor(800).AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() => this.Hide(false)))));
    }

    public void Handle(StickersPackPurchasedEvent message)
    {
      int productId = message.StockItemHeader.ProductId;
      StockItemHeader stockItemHeader = this._stockItemHeader;
      int? nullable = stockItemHeader != null ? new int?(stockItemHeader.ProductId) : new int?();
      int valueOrDefault = nullable.GetValueOrDefault();
      if ((productId == valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) == 0)
        return;
      this.HideWithDelay();
    }

    public void Handle(StickersPackActivatedDeactivatedEvent message)
    {
      int productId = message.StockItemHeader.ProductId;
      StockItemHeader stockItemHeader = this._stockItemHeader;
      int? nullable = stockItemHeader != null ? new int?(stockItemHeader.ProductId) : new int?();
      int valueOrDefault = nullable.GetValueOrDefault();
      if ((productId == valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) == 0)
        return;
      this.HideWithDelay();
    }

    public void Handle(GiftSentEvent message)
    {
      StockItemHeader stockItemHeader = this._stockItemHeader;
      int? nullable1 = stockItemHeader != null ? new int?(stockItemHeader.ProductId) : new int?();
      long? nullable2 = nullable1.HasValue ? new long?( nullable1.GetValueOrDefault()) : new long?();
      long num = -message.GiftId;
      if ((nullable2.GetValueOrDefault() == num ? (!nullable2.HasValue ? 1 : 0) : 1) != 0)
        return;
      Execute.ExecuteOnUIThread((Action) (() => this.Hide(false)));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Stickers/Views/StickersPackView.xaml", UriKind.Relative));
      this.ListBoxItemNavDotsStyle = (Style) base.FindName("ListBoxItemNavDotsStyle");
      this.ShowStoryboard = (Storyboard) base.FindName("ShowStoryboard");
      this.splineKeyFrameShowBegin = (SplineDoubleKeyFrame) base.FindName("splineKeyFrameShowBegin");
      this.splineKeyFrameShowEnd = (SplineDoubleKeyFrame) base.FindName("splineKeyFrameShowEnd");
      this.rectBackground = (Rectangle) base.FindName("rectBackground");
      this.translateContent = (TranslateTransform) base.FindName("translateContent");
      this.borderContent = (Border) base.FindName("borderContent");
      this.scrollViewerContent = (ScrollViewer) base.FindName("scrollViewerContent");
      this.gridSlideView = (Grid) base.FindName("gridSlideView");
      this.slideView = (SlideView) base.FindName("slideView");
      this.listBoxNavDots = (ListBox) base.FindName("listBoxNavDots");
      this.ucStickersPackInfo = (StickersPackInfoUC) base.FindName("ucStickersPackInfo");
      this.textBlockDescription = (TextBlock) base.FindName("textBlockDescription");
      this.gridSendAsAGift = (Grid) base.FindName("gridSendAsAGift");
    }
  }
}
