using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;
using VKClient.Common.Shared.ImagePreview;
using VKClient.Common.Stickers.AutoSuggest;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.Stickers.Views;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewMessageUC : UserControl, IHandle<StickersAutoSuggestDictionary.AutoSuggestDictionaryUpdatedEvent>, IHandle, IHandle<PreviewCompletedEvent>, IHandle<HasStickersUpdatesChangedEvent>, IHandle<StickersSettings.StickersListUpdatedEvent>, IHandle<StickersSettings.StickersKeyboardOpenRequestEvent>, IHandle<StickersSettings.StickersItemTapEvent>
  {
      public static readonly DependencyProperty IsVoiceMessageButtonEnabledProperty = DependencyProperty.Register("IsVoiceMessageButtonEnabled", typeof(bool), typeof(NewMessageUC), new PropertyMetadata(new PropertyChangedCallback(NewMessageUC.IsVoiceMessageButtonEnabled_OnChanged)));
    private int _adminLevel;
    private PhoneApplicationPage _parentPage;
    private bool _canRecordVoiceMessage;
    private string _lastKeyForAutoSuggest;
    private bool _lastAutoSuggestStickersEnabled;
    private string _replyAutoForm;
    private bool _isEnabled;
    private ImageBrush _keyboardBrush;
    private ImageBrush _emojiBrush;
    private DispatcherTimer _auioRecordHoldTimer;
    private Point _translationOrigin;
    private bool _isAnimatingHoldToRecord;
    private bool _panelInitialized;
    private SwipeThroughControl _stickersSlideView;
    internal MentionPickerUC mentionPicker;
    internal StackPanel panelReply;
    internal CheckBox checkBoxAsCommunity;
    internal TextBlock textBlockReply;
    internal ReplyUserUC ucReplyUser;
    internal ScrollViewer scrollNewMessage;
    internal NewPostUC ucNewPost;
    internal Border borderEmoji;
    internal Ellipse ellipseHasStickersUpdates;
    internal Border borderHoldToRecord;
    internal Border borderSend;
    internal Border borderVoice;
    internal AudioRecorderUC ucAudioRecorder;
    internal StickersAutoSuggestUC ucStickersAutoSuggest;
    internal TextBoxPanelControl panelControl;
    private bool _contentLoaded;

    private bool HaveRightsToPostOnBehalfOfCommunity
    {
      get
      {
        return this._adminLevel > 1;
      }
    }

    public long UserOrChatId { get; set; }

    public bool IsChat { get; set; }

    public ScrollViewer ScrollNewMessage
    {
      get
      {
        return this.scrollNewMessage;
      }
    }

    public NewPostUC UCNewPost
    {
      get
      {
        return this.ucNewPost;
      }
    }

    public Action OnAddAttachTap { get; set; }

    public Action OnSendTap { get; set; }

    public TextBox TextBoxNewComment
    {
      get
      {
        return this.UCNewPost.textBoxPost;
      }
    }

    public ReplyUserUC ReplyUserUC
    {
      get
      {
        return this.ucReplyUser;
      }
    }

    public TextBoxPanelControl PanelControl
    {
      get
      {
        return this.panelControl;
      }
    }

    public bool FromGroupChecked
    {
      get
      {
        if (((ToggleButton) this.checkBoxAsCommunity).IsChecked.HasValue)
          return ((ToggleButton) this.checkBoxAsCommunity).IsChecked.Value;
        return false;
      }
    }

    public bool IsVoiceMessageButtonEnabled
    {
      get
      {
        return (bool) base.GetValue(NewMessageUC.IsVoiceMessageButtonEnabledProperty);
      }
      set
      {
        base.SetValue(NewMessageUC.IsVoiceMessageButtonEnabledProperty, value);
      }
    }

    public MentionPickerUC MentionPicker
    {
      get
      {
        return this.mentionPicker;
      }
    }

    public NewMessageUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.SetAdminLevel(0);
      this.panelControl.BindTextBox(this.TextBoxNewComment);
      this.panelControl.IsFocusedChanged += new EventHandler<bool>(this.IsFocusedChanged);
      this.panelControl.IsEmojiOpenedChanged += new EventHandler<bool>(this.IsEmojiOpenedChanged);
      // ISSUE: method pointer
      this.TextBoxNewComment.TextChanged += new TextChangedEventHandler(this.TextBoxNewComment_TextChanged);
            this.UpdateSendButton(false);
      this.UpdateAutoSuggestVisibility();
      this.ucStickersAutoSuggest.AutoSuggestStickerSendingCallback = new Action(this.OnAutoSuggestStickerSending);
      this.ucStickersAutoSuggest.AutoSuggestStickerSentCallback = new Action(this.OnAutoSuggestStickerSent);
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.NewMessageUC_Loaded));
      this.UpdateHasStickersUpdatesState();
      this._lastAutoSuggestStickersEnabled = AppGlobalStateManager.Current.GlobalState.StickersAutoSuggestEnabled;
      this.UpdateVoiceMessageAvailability();//4.12
      EventAggregator.Current.Subscribe((object)this);
      base.SizeChanged += (delegate(object sender, SizeChangedEventArgs args)//4.12
      {
          double width = args.NewSize.Width;
          if (double.IsNaN(width) || double.IsInfinity(width))
          {
              return;
          }
          this.ucAudioRecorder.Width = (width);
      });
    }

    public void SetAdminLevel(int adminLevel)
    {
      this._adminLevel = adminLevel;
      if (this.HaveRightsToPostOnBehalfOfCommunity)
      {
        ((UIElement) this.panelReply).Visibility = Visibility.Visible;
        ((UIElement) this.checkBoxAsCommunity).Visibility = Visibility.Visible;
        ((UIElement) this.textBlockReply).Visibility = Visibility.Collapsed;
      }
      else
      {
        ((UIElement) this.checkBoxAsCommunity).Visibility = Visibility.Collapsed;
        ((UIElement) this.textBlockReply).Visibility = Visibility.Visible;
        if (!string.IsNullOrEmpty(this.ucReplyUser.Title))
          return;
        ((UIElement) this.panelReply).Visibility = Visibility.Collapsed;
        ((UIElement) this.ucReplyUser).Visibility = Visibility.Collapsed;
      }
    }

    private static void IsVoiceMessageButtonEnabled_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((NewMessageUC) d).UpdateVoiceMessageAvailability();
    }

    public void HideAudioRecoringUC()
    {
      this.ucAudioRecorder.IsOpened = false;
    }

    public void ShowAudioRecordingPreview()
    {
      this.ucAudioRecorder.StopRecordingAndShowPreview();
    }

    private void UpdateVoiceMessageAvailability()
    {
      this._canRecordVoiceMessage = AudioRecorderHelper.CanRecord && this.IsVoiceMessageButtonEnabled;
      if (this._canRecordVoiceMessage)
      {
        ((UIElement) this.borderSend).Opacity = 0.0;
        ((UIElement) this.borderVoice).Visibility = Visibility.Visible;
      }
      else
      {
        ((UIElement) this.borderSend).Opacity = 1.0;
        ((UIElement) this.borderVoice).Visibility = Visibility.Collapsed;
      }
    }

    private void NewMessageUC_Loaded(object sender, RoutedEventArgs e)
    {
      if (this._parentPage != null)
        return;
      this._parentPage = (PhoneApplicationPage) FramePageUtils.CurrentPage;
      if (this._parentPage == null)
        return;
      this._parentPage.OrientationChanged += (new EventHandler<OrientationChangedEventArgs>(this._parentPage_OrientationChanged));
    }

    private void _parentPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      this.UpdateAutoSuggestVisibility();
    }

    private void OnAutoSuggestStickerSending()
    {
      this.ucNewPost.ForceFocusIfNeeded();
    }

    private void OnAutoSuggestStickerSent()
    {
      this.TextBoxNewComment.Text = ("");
      this._replyAutoForm = "";
    }

    private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (this.ucAudioRecorder.IsOpened)
        this.TextBoxNewComment.Text = ("");
      this.UpdateAutoSuggest(false);
    }

    private void UpdateAutoSuggest(bool force = false)
    {
      string text = this.TextBoxNewComment.Text;
      if (!string.IsNullOrEmpty(this._replyAutoForm) && text.StartsWith(this._replyAutoForm))
        text = text.Substring(this._replyAutoForm.Length);
      string str = StickersAutoSuggestDictionary.Instance.PrepareTextForLookup(text);
      if (((str != this._lastKeyForAutoSuggest ? 1 : (this._lastAutoSuggestStickersEnabled != AppGlobalStateManager.Current.GlobalState.StickersAutoSuggestEnabled ? 1 : 0)) | (force ? 1 : 0)) != 0)
      {
        this.ucStickersAutoSuggest.SetData(StickersAutoSuggestDictionary.Instance.GetAutoSuggestItemsFor(str), str);
        this._lastKeyForAutoSuggest = str;
        this._lastAutoSuggestStickersEnabled = AppGlobalStateManager.Current.GlobalState.StickersAutoSuggestEnabled;
      }
      this.UpdateAutoSuggestVisibility();
    }

    private void UpdateAutoSuggestVisibility()
    {
        this.ucStickersAutoSuggest.ShowHide((this.ucNewPost.IsFocused || this.panelControl.IsOpen) && (this._parentPage == null || this._parentPage.Orientation != PageOrientation.Landscape && this._parentPage.Orientation != PageOrientation.LandscapeLeft && this._parentPage.Orientation != PageOrientation.LandscapeRight) && this.ucStickersAutoSuggest.HasItemsToShow);
    }

    private void UpdateAudioMessage()
    {
      if (this._isEnabled)
      {
        ((UIElement) this.borderVoice).Opacity = 0.0;
        ((UIElement) this.borderSend).Opacity = 1.0;
        ((UIElement) this.borderVoice).IsHitTestVisible = false;
        ((UIElement) this.borderSend).IsHitTestVisible = true;
      }
      else
      {
        ((UIElement) this.borderVoice).Opacity = 1.0;
        ((UIElement) this.borderSend).Opacity = 0.0;
        ((UIElement) this.borderVoice).IsHitTestVisible = true;
        ((UIElement) this.borderSend).IsHitTestVisible = false;
      }
    }

    public void SetReplyAutoForm(string replyAutoForm)
    {
      this._replyAutoForm = replyAutoForm;
    }

    public void UpdateSendButton(bool isEnabled)
    {
      this._isEnabled = isEnabled;
      this.scrollNewMessage.VerticalScrollBarVisibility=(this._isEnabled ? (ScrollBarVisibility) 1 : (ScrollBarVisibility) 0);
      if (!this._canRecordVoiceMessage)
        return;
      this.UpdateAudioMessage();
    }

    private void IsEmojiOpenedChanged(object sender, bool e)
    {
      ImageBrush imageBrush;
      if (this.panelControl.IsOpen)
      {
        if (this._keyboardBrush == null)
        {
          this._keyboardBrush = new ImageBrush();
          ImageLoader.SetImageBrushMultiResSource(this._keyboardBrush, "/Resources/Keyboard32px.png");
        }
        imageBrush = this._keyboardBrush;
      }
      else
      {
        if (this._emojiBrush == null)
        {
          this._emojiBrush = new ImageBrush();
          ImageLoader.SetImageBrushMultiResSource(this._emojiBrush, "/Resources/Smile32px.png");
        }
        imageBrush = this._emojiBrush;
      }
      ((UIElement) this.borderEmoji).OpacityMask=((Brush) imageBrush);
      this.UpdateAutoSuggest(false);
    }

    private void IsFocusedChanged(object sender, bool e)
    {
      this.ucNewPost.IsFocused = this.panelControl.IsTextBoxTargetFocused;
      this.UpdateAutoSuggest(false);
    }

    private void AddAttachTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Action onAddAttachTap = this.OnAddAttachTap;
      if (onAddAttachTap == null)
        return;
      onAddAttachTap();
    }

    private void SendTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnSendTap == null || !this._isEnabled)
        return;
      this.ucNewPost.ForceFocusIfNeeded();
      this.OnSendTap();
    }

    private void UcAudioRecorder_OnOpened(object sender, EventArgs e)
    {
      this.panelControl.ShowOverlay();
    }

    private void UcAudioRecorder_OnClosed(object sender, EventArgs e)
    {
      this.panelControl.HideOverlay();
      this.ucNewPost.ForceFocusIfNeeded();
    }

    private void BorderVoice_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
      this._translationOrigin = e.ManipulationOrigin;
      this.TextBoxNewComment.Text = ("");
      if (this._auioRecordHoldTimer == null)
      {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(200.0);
        dispatcherTimer.Interval = timeSpan;
        this._auioRecordHoldTimer = dispatcherTimer;
      }
      this._auioRecordHoldTimer.Tick-=(new EventHandler(this.AuioRecordHoldTimer_OnTick));
      this._auioRecordHoldTimer.Tick+=(new EventHandler(this.AuioRecordHoldTimer_OnTick));
      this._auioRecordHoldTimer.Start();
    }

    private void AuioRecordHoldTimer_OnTick(object sender, EventArgs eventArgs)
    {
      this._auioRecordHoldTimer.Tick-=(new EventHandler(this.AuioRecordHoldTimer_OnTick));
      this._auioRecordHoldTimer.Stop();
      ((UIElement) this.borderHoldToRecord).Visibility = Visibility.Collapsed;
      ((UIElement) this.borderHoldToRecord).Opacity = 0.0;
      this._isAnimatingHoldToRecord = false;
      this.ucAudioRecorder.IsOpened = true;
      this.ucAudioRecorder.HandleManipulationStarted(this._translationOrigin);
    }

    private void BorderVoice_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
      if (this.ucAudioRecorder.IsOpened)
        this.ucAudioRecorder.HandleManipulationDelta(e);
      else
        this._translationOrigin = e.CumulativeManipulation.Translation;
    }

    private void BorderVoice_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
      ((Control) this).Focus();
      this.ucNewPost.ForceFocusIfNeeded();
      if (this._auioRecordHoldTimer.IsEnabled)
      {
        this._auioRecordHoldTimer.Tick-=(new EventHandler(this.AuioRecordHoldTimer_OnTick));
        this._auioRecordHoldTimer.Stop();
        this.ShowHoldToRecord();
      }
      else
        this.ucAudioRecorder.HandleManipulationCompleted(e);
    }

    private void ShowHoldToRecord()
    {
      if (this._isAnimatingHoldToRecord)
        return;
      this._isAnimatingHoldToRecord = true;
      ((UIElement) this.borderHoldToRecord).Visibility = Visibility.Visible;
      ((DependencyObject) this.borderHoldToRecord).Animate(((UIElement) this.borderHoldToRecord).Opacity, 1.0, UIElement.OpacityProperty, 100, new int?(),  null, (Action) (() => ((DependencyObject) this.borderHoldToRecord).Animate(((UIElement) this.borderHoldToRecord).Opacity, 0.0, UIElement.OpacityProperty, 100, new int?(1000),  null, (Action) (() =>
      {
        ((UIElement) this.borderHoldToRecord).Visibility = Visibility.Collapsed;
        this._isAnimatingHoldToRecord = false;
      }))));
    }

    private void Smiles_OnMouseEnter(object sender, MouseEventArgs e)
    {
      this.InitPanel();
      if (!this.panelControl.IsOpen)
        this.OpenPanel();
      else
        ((Control) this.ucNewPost.TextBoxPost).Focus();
    }

    private void InitPanel()
    {
      if (this._panelInitialized)
        return;
      SwipeThroughControl swipeThroughControl = new SwipeThroughControl();
      swipeThroughControl.BackspaceTapCallback = new Action(this.HandleBackspaceTap);
      long userOrChatId = this.UserOrChatId;
      swipeThroughControl.UserOrChatId = userOrChatId;
      int num = this.IsChat ? 1 : 0;
      swipeThroughControl.IsChat = num != 0;
      this._stickersSlideView = swipeThroughControl;
      this.panelControl.InitializeWithChildControl((FrameworkElement) this._stickersSlideView);
      this._stickersSlideView.CreateSingleElement = (Func<Control>) (() =>
      {
        SpriteListControl spriteListControl = new SpriteListControl();
        FramePageUtils.CurrentPage.RegisterForCleanup((IMyVirtualizingPanel) spriteListControl.MyPanel);
        spriteListControl.MyPanel.LoadedHeightDownwards = spriteListControl.MyPanel.LoadedHeightDownwardsNotScrolling = 600.0;
        spriteListControl.MyPanel.LoadedHeightUpwards = spriteListControl.MyPanel.LoadedHeightUpwardsNotScrolling = 300.0;
        spriteListControl.MyPanel.LoadUnloadThreshold = 100.0;
        return (Control) spriteListControl;
      });
      this._panelInitialized = true;
      this.ReloadStickersItems(true, false);
    }

    private void HandleBackspaceTap()
    {
      TextBox textBoxNewComment = this.TextBoxNewComment;
      string s = textBoxNewComment != null ? textBoxNewComment.Text :  null;
      if (string.IsNullOrEmpty(s) || s.Length <= 0)
        return;
      int num = 1;
      if (s.Length > 1 && char.IsSurrogatePair(s, s.Length - 2))
        num = 2;
      this.TextBoxNewComment.Text = (s.Substring(0, s.Length - num));
    }

    private void ReloadStickersItems(bool reloadSystemItems = false, bool keepPosition = true)
    {
      if (!this._panelInitialized)
        return;
      StoreProduct storeProduct =  null;
      if (keepPosition)
        storeProduct = this.GetCurrentSelectedProduct();
      this._stickersSlideView.Items = new ObservableCollection<object>((IEnumerable<object>) StickersSettings.Instance.CreateSpriteListItemData());
      if (reloadSystemItems)
      {
        this._stickersSlideView.HeaderItems = new List<object>((IEnumerable<object>) StickersSettings.Instance.CreateStoreSpriteListItem());
        this._stickersSlideView.FooterItems = new List<object>((IEnumerable<object>) StickersSettings.Instance.CreateSettingsSpriteListItem());
      }
      if (!keepPosition || storeProduct == null)
        return;
      this.TrySlideToStickersPack((long) storeProduct.id);
    }

    private void OpenPanel()
    {
      this.panelControl.IsOpen = true;
      ((Control) this).Focus();
      this.MarkUpdatesAsViewed(false);
    }

    private void ReplyPanel_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ucNewPost.ForceFocusIfNeeded();
    }

    private void UcReplyUser_OnTitleChanged(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(this.ucReplyUser.Title))
      {
        ((UIElement) this.ucReplyUser).Visibility = Visibility.Visible;
        ((UIElement) this.panelReply).Visibility = Visibility.Visible;
        ((UIElement) this.textBlockReply).Visibility = (this.HaveRightsToPostOnBehalfOfCommunity ? Visibility.Collapsed : Visibility.Visible);
      }
      else
      {
        ((UIElement) this.ucReplyUser).Visibility = Visibility.Collapsed;
        if (!this.HaveRightsToPostOnBehalfOfCommunity)
        {
          ((UIElement) this.panelReply).Visibility = Visibility.Collapsed;
          ((UIElement) this.textBlockReply).Visibility = Visibility.Visible;
        }
        else
          ((UIElement) this.textBlockReply).Visibility = Visibility.Collapsed;
      }
    }

    private bool TrySlideToStickersPack(long productId)
    {
      this.InitPanel();
      int num = -1;
      ObservableCollection<object> items = this._stickersSlideView.Items;
      for (int index = 0; index < items.Count; ++index)
      {
        StoreProduct stickerProduct = ((SpriteListItemData) items[index]).StickerProduct;
        if (stickerProduct != null && (long) stickerProduct.id == productId)
        {
          num = index;
          break;
        }
      }
      if (num < 0)
        return false;
      if (this._stickersSlideView.SelectedIndex != num)
        this._stickersSlideView.SelectedIndex = num;
      return true;
    }

    private StoreProduct GetCurrentSelectedProduct()
    {
      SwipeThroughControl stickersSlideView = this._stickersSlideView;
      if ((stickersSlideView != null ? stickersSlideView.Items :  null) == null || this._stickersSlideView.Items.Count == 0)
        return  null;
      return ((SpriteListItemData) this._stickersSlideView.Items[this._stickersSlideView.SelectedIndex]).StickerProduct;
    }

    private void TryOpenStickersPackKeyboard(long productId, int delay = 0)
    {
      if (this._parentPage != FramePageUtils.CurrentPage || !this.TrySlideToStickersPack(productId))
        return;
      Action openPanel = (Action) (() =>
      {
        if (this.panelControl.IsOpen)
          return;
        this.OpenPanel();
      });
      if (delay <= 0)
        openPanel();
      else
        new DelayedExecutor(delay).AddToDelayedExecution((Action) (() => openPanel()));
    }

    private void OpenKeyboardOrPopup(StockItemHeader stockItemHeader)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this._parentPage != FramePageUtils.CurrentPage)
          return;
        if (base.IsHitTestVisible)
        {
          this.TryOpenStickersPackKeyboard((long) stockItemHeader.ProductId, 0);
        }
        else
        {
          CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.message;
          StickersPackView.Show(stockItemHeader, "message");
        }
      }));
    }

    private void MarkUpdatesAsViewed(bool force = false)
    {
      if (!force && ((UIElement) this.ellipseHasStickersUpdates).Visibility != Visibility.Visible)
        return;
      AppGlobalStateManager.Current.GlobalState.HasStickersUpdates = false;
      this.UpdateHasStickersUpdatesState();
      StoreService.Instance.MarkUpdatesAsViewed();
    }

    private void UpdateHasStickersUpdatesState()
    {
      Execute.ExecuteOnUIThread((Action) (() => ((UIElement) this.ellipseHasStickersUpdates).Visibility = (AppGlobalStateManager.Current.GlobalState.HasStickersUpdates ? Visibility.Visible : Visibility.Collapsed)));
    }

    public void Handle(StickersAutoSuggestDictionary.AutoSuggestDictionaryUpdatedEvent message)
    {
      this.UpdateAutoSuggest(true);
    }

    public void Handle(PreviewCompletedEvent message)
    {
      ((Control) this).Focus();
      this.ucNewPost.ForceFocusIfNeeded();
    }

    public void Handle(HasStickersUpdatesChangedEvent message)
    {
      if (this.panelControl.IsOpen)
        this.MarkUpdatesAsViewed(true);
      else
        this.UpdateHasStickersUpdatesState();
    }

    public void Handle(StickersSettings.StickersListUpdatedEvent message)
    {
      this.ReloadStickersItems(false, true);
    }

    public void Handle(StickersSettings.StickersKeyboardOpenRequestEvent message)
    {
      if (this._parentPage != FramePageUtils.CurrentPage)
        return;
      new DelayedExecutor(1000).AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() => this.OpenKeyboardOrPopup(message.StockItemHeader)))));
    }

    public void Handle(StickersSettings.StickersItemTapEvent message)
    {
      this.OpenKeyboardOrPopup(message.StockItemHeader);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewMessageUC.xaml", UriKind.Relative));
      this.mentionPicker = (MentionPickerUC) base.FindName("mentionPicker");
      this.panelReply = (StackPanel) base.FindName("panelReply");
      this.checkBoxAsCommunity = (CheckBox) base.FindName("checkBoxAsCommunity");
      this.textBlockReply = (TextBlock) base.FindName("textBlockReply");
      this.ucReplyUser = (ReplyUserUC) base.FindName("ucReplyUser");
      this.scrollNewMessage = (ScrollViewer) base.FindName("scrollNewMessage");
      this.ucNewPost = (NewPostUC) base.FindName("ucNewPost");
      this.borderEmoji = (Border) base.FindName("borderEmoji");
      this.ellipseHasStickersUpdates = (Ellipse) base.FindName("ellipseHasStickersUpdates");
      this.borderHoldToRecord = (Border) base.FindName("borderHoldToRecord");
      this.borderSend = (Border) base.FindName("borderSend");
      this.borderVoice = (Border) base.FindName("borderVoice");
      this.ucAudioRecorder = (AudioRecorderUC) base.FindName("ucAudioRecorder");
      this.ucStickersAutoSuggest = (StickersAutoSuggestUC) base.FindName("ucStickersAutoSuggest");
      this.panelControl = (TextBoxPanelControl) base.FindName("panelControl");
    }
  }
}
