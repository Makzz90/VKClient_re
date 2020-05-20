using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Graffiti;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKMessenger.Backend;
using VKMessenger.Library;
using VKMessenger.Library.Events;
using VKMessenger.Library.VirtItems;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace VKMessenger.Views
{
    public class ConversationPage : PageBase, IScroll, INotifyPropertyChanged, IHandle<MessageActionEvent>, IHandle, IHandle<SpriteElementTapEvent>, IHandle<StickerItemTapEvent>, IHandle<VoiceMessageSentEvent>
    {
        private readonly ApplicationBar _defaultAppBar;
        private readonly ApplicationBar _appBarAttachments;
        private readonly ApplicationBar _appBarSelection;
        private readonly PhotoChooserTask _photoChooserTask;
        private readonly ApplicationBarIconButton _appBarButtonSend;
        private readonly ApplicationBarIconButton _appBarButtonAttachImage;
        private readonly ApplicationBarIconButton _appBarButtonAddAttachment;
        private readonly ApplicationBarIconButton _appBarButtonAttachments;
        private readonly ApplicationBarIconButton _appBarButtonСhoose;
        private readonly ApplicationBarIconButton _appBarButtonCancel;
        private readonly ApplicationBarIconButton _appBarButtonForward;
        private readonly ApplicationBarIconButton _appBarButtonDelete;
        private readonly ApplicationBarIconButton _appBarButtonEmojiToggle;
        private readonly ApplicationBarMenuItem _appBarMenuItemDisableEnableNotifications;
        private readonly ApplicationBarMenuItem _appBarMenuItemAllowDenyMessagesFromGroup;
        private readonly ApplicationBarMenuItem _appbarMenuItemPinToStart;
        private readonly ApplicationBarMenuItem _appbarMenuItemShowMaterials;
        private readonly ApplicationBarMenuItem _appBarMenuItemManageChat;
        private readonly ApplicationBarMenuItem _appBarMenuItemDeleteDialog;
        private readonly ApplicationBarMenuItem _appBarMenuItemRefresh;
        private readonly ApplicationBarMenuItem _appBarMenuItemAddMember;
        //private GeoCoordinate _position;
        private bool _isInitialized;
        //private bool _needScrollBottom;
        private readonly System.DateTime _createdTimestamp;
        private ConversationItems _conversationItems;
        private static int TotalCount;
        private PickerUC _pickerUC;
        private long _userOrChatId;
        private bool _isChat;
        private long _startMessageId;
        //private bool _loadedStartMessageId;
        private bool _isCurrent;
        //private bool _canDettachProductAttachment;
        private IShareContentDataProvider _shareContentDataProvider;
        private bool _needCleanupOnNavigatedFrom;
        private bool _shouldScrollToUnreadItem;
        private long _messageIdToScrollTo;
        internal Grid LayoutRoot;
        internal Grid gridHeader;
        internal StackPanel TitlePanel;
        internal TextBlock textBlockTitle;
        internal TextBlock textBlockSubtitleVertical;
        internal ContextMenu FriendOptionsMenu;
        internal MenuItem menuItemAllowDenyMessagesFromGroup;
        internal MenuItem menuItemRefresh;
        internal MenuItem menuItemPinToStart;
        internal MenuItem menuItemShowMaterials;
        internal MenuItem menuItemDisableEnableNotifications;
        internal MenuItem menuItemAddMember;
        internal MenuItem menuItemDeleteDialog;
        internal Grid ContentPanel;
        internal ViewportControl myScroll;
        internal MyVirtualizingPanel2 myPanel;
        internal NewMessageUC ucNewMessage;
        internal ContentControl MediaControl;
        private bool _contentLoaded;

        private ConversationPage.Mode CurrentMode
        {
            get
            {
                return !this.ConversationVM.IsInSelectionMode ? ConversationPage.Mode.Default : ConversationPage.Mode.Selection;
            }
            set
            {
                this.ConversationVM.IsInSelectionMode = value == ConversationPage.Mode.Selection;
                this.UpdateAppBar();
            }
        }

        public ConversationViewModel ConversationVM
        {
            get
            {
                return base.DataContext as ConversationViewModel;
            }
        }

        public ConversationItems ConversationItems
        {
            get
            {
                return this._conversationItems;
            }
            set
            {
                this._conversationItems = value;
                if (this.PropertyChanged == null)
                    return;
                this.PropertyChanged(this, new PropertyChangedEventArgs("ConversationItems"));
            }
        }

        private NewPostUC ucNewPost
        {
            get
            {
                return this.ucNewMessage.UCNewPost;
            }
        }

        private ScrollViewer scrollNewMessage
        {
            get
            {
                return this.ucNewMessage.ScrollNewMessage;
            }
        }

        private TextBox textBoxNewMessage
        {
            get
            {
                return this.ucNewPost.TextBoxPost;
            }
        }

        public bool IsManipulating
        {
            get
            {
                return this.myScroll.ManipulationState > 0;
            }
        }

        public double VerticalOffset
        {
            get
            {
                return this.myScroll.Viewport.Y;
            }
        }

        public bool IsHorizontalOrientation
        {
            get
            {
                if (this.Orientation != PageOrientation.Landscape && this.Orientation != PageOrientation.LandscapeLeft)
                    return this.Orientation == PageOrientation.LandscapeRight;
                return true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ConversationPage()
        {
            ApplicationBar applicationBar1 = new ApplicationBar();
            Color appBarBgColor1 = VKConstants.AppBarBGColor;
            applicationBar1.BackgroundColor = appBarBgColor1;
            Color appBarFgColor1 = VKConstants.AppBarFGColor;
            applicationBar1.ForegroundColor = appBarFgColor1;
            this._defaultAppBar = applicationBar1;
            ApplicationBar applicationBar2 = new ApplicationBar();
            Color appBarBgColor2 = VKConstants.AppBarBGColor;
            applicationBar2.BackgroundColor = appBarBgColor2;
            Color appBarFgColor2 = VKConstants.AppBarFGColor;
            applicationBar2.ForegroundColor = appBarFgColor2;
            this._appBarAttachments = applicationBar2;
            ApplicationBar applicationBar3 = new ApplicationBar();
            Color appBarBgColor3 = VKConstants.AppBarBGColor;
            applicationBar3.BackgroundColor = appBarBgColor3;
            Color appBarFgColor3 = VKConstants.AppBarFGColor;
            applicationBar3.ForegroundColor = appBarFgColor3;
            this._appBarSelection = applicationBar3;
            PhotoChooserTask photoChooserTask = new PhotoChooserTask();
            int num = 1;
            photoChooserTask.ShowCamera = (num != 0);
            this._photoChooserTask = photoChooserTask;
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            System.Uri uri1 = new System.Uri("./Resources/appbar.send.text.rest.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri1;
            string conversationAppBarSend = CommonResources.Conversation_AppBar_Send;
            applicationBarIconButton1.Text = conversationAppBarSend;
            this._appBarButtonSend = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            System.Uri uri2 = new System.Uri("./Resources/appbar.feature.camera.rest.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = uri2;
            string appBarAttachImage = CommonResources.Conversation_AppBar_AttachImage;
            applicationBarIconButton2.Text = appBarAttachImage;
            this._appBarButtonAttachImage = applicationBarIconButton2;
            ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
            System.Uri uri3 = new System.Uri("./Resources/attach.png", UriKind.Relative);
            applicationBarIconButton3.IconUri = uri3;
            string barAddAttachment = CommonResources.NewPost_AppBar_AddAttachment;
            applicationBarIconButton3.Text = barAddAttachment;
            this._appBarButtonAddAttachment = applicationBarIconButton3;
            ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
            System.Uri uri4 = new System.Uri("./Resources/appbar.attachments-1.rest.png", UriKind.Relative);
            applicationBarIconButton4.IconUri = uri4;
            string appBarAttachments = CommonResources.Conversation_AppBar_Attachments;
            applicationBarIconButton4.Text = appBarAttachments;
            this._appBarButtonAttachments = applicationBarIconButton4;
            ApplicationBarIconButton applicationBarIconButton5 = new ApplicationBarIconButton();
            System.Uri uri5 = new System.Uri("./Resources/appbar.manage.rest.png", UriKind.Relative);
            applicationBarIconButton5.IconUri = uri5;
            string conversationAppBarChoose = CommonResources.Conversation_AppBar_Choose;
            applicationBarIconButton5.Text = conversationAppBarChoose;
            this._appBarButtonСhoose = applicationBarIconButton5;
            ApplicationBarIconButton applicationBarIconButton6 = new ApplicationBarIconButton();
            System.Uri uri6 = new System.Uri("./Resources/appbar.cancel.rest.png", UriKind.Relative);
            applicationBarIconButton6.IconUri = uri6;
            string conversationAppBarCancel = CommonResources.Conversation_AppBar_Cancel;
            applicationBarIconButton6.Text = conversationAppBarCancel;
            this._appBarButtonCancel = applicationBarIconButton6;
            ApplicationBarIconButton applicationBarIconButton7 = new ApplicationBarIconButton();
            System.Uri uri7 = new System.Uri("./Resources/appbar.forward.rest.png", UriKind.Relative);
            applicationBarIconButton7.IconUri = uri7;
            string conversationAppBarForward = CommonResources.Conversation_AppBar_Forward;
            applicationBarIconButton7.Text = conversationAppBarForward;
            this._appBarButtonForward = applicationBarIconButton7;
            ApplicationBarIconButton applicationBarIconButton8 = new ApplicationBarIconButton();
            System.Uri uri8 = new System.Uri("./Resources/appbar.delete.rest.png", UriKind.Relative);
            applicationBarIconButton8.IconUri = uri8;
            string conversationAppBarDelete = CommonResources.Conversation_AppBar_Delete;
            applicationBarIconButton8.Text = conversationAppBarDelete;
            this._appBarButtonDelete = applicationBarIconButton8;
            ApplicationBarIconButton applicationBarIconButton9 = new ApplicationBarIconButton();
            System.Uri uri9 = new System.Uri("./Resources/appbar.smile.png", UriKind.Relative);
            applicationBarIconButton9.IconUri = uri9;
            string str = "emoji";
            applicationBarIconButton9.Text = str;
            this._appBarButtonEmojiToggle = applicationBarIconButton9;
            ApplicationBarMenuItem applicationBarMenuItem1 = new ApplicationBarMenuItem();
            string offNotifications = CommonResources.TurnOffNotifications;
            applicationBarMenuItem1.Text = offNotifications;
            this._appBarMenuItemDisableEnableNotifications = applicationBarMenuItem1;
            ApplicationBarMenuItem applicationBarMenuItem2 = new ApplicationBarMenuItem();
            string messagesFromGroupDeny = CommonResources.MessagesFromGroupDeny;
            applicationBarMenuItem2.Text = messagesFromGroupDeny;
            this._appBarMenuItemAllowDenyMessagesFromGroup = applicationBarMenuItem2;
            ApplicationBarMenuItem applicationBarMenuItem3 = new ApplicationBarMenuItem();
            string pinToStart = CommonResources.PinToStart;
            applicationBarMenuItem3.Text = pinToStart;
            this._appbarMenuItemPinToStart = applicationBarMenuItem3;
            ApplicationBarMenuItem applicationBarMenuItem4 = new ApplicationBarMenuItem();
            string messengerShowMaterials = CommonResources.Messenger_ShowMaterials;
            applicationBarMenuItem4.Text = messengerShowMaterials;
            this._appbarMenuItemShowMaterials = applicationBarMenuItem4;
            ApplicationBarMenuItem applicationBarMenuItem5 = new ApplicationBarMenuItem();
            string appBarManageChat = CommonResources.Conversation_AppBar_ManageChat;
            applicationBarMenuItem5.Text = appBarManageChat;
            this._appBarMenuItemManageChat = applicationBarMenuItem5;
            ApplicationBarMenuItem applicationBarMenuItem6 = new ApplicationBarMenuItem();
            string appBarDeleteDialog = CommonResources.Conversation_AppBar_DeleteDialog;
            applicationBarMenuItem6.Text = appBarDeleteDialog;
            this._appBarMenuItemDeleteDialog = applicationBarMenuItem6;
            ApplicationBarMenuItem applicationBarMenuItem7 = new ApplicationBarMenuItem();
            string conversationAppBarRefresh = CommonResources.Conversation_AppBar_Refresh;
            applicationBarMenuItem7.Text = conversationAppBarRefresh;
            this._appBarMenuItemRefresh = applicationBarMenuItem7;
            ApplicationBarMenuItem applicationBarMenuItem8 = new ApplicationBarMenuItem();
            string conversationAppBarAddMember = CommonResources.Conversation_AppBar_AddMember;
            applicationBarMenuItem8.Text = conversationAppBarAddMember;
            this._appBarMenuItemAddMember = applicationBarMenuItem8;
            // ISSUE: explicit constructor call
            //  base.\u002Ector();
            ++ConversationPage.TotalCount;
            this.InitializeComponent();
            this.myScroll.BindViewportBoundsTo((FrameworkElement)this.myPanel);
            // ISSUE: method pointer
            base.Loaded += (new RoutedEventHandler((this.ConversationPage_Loaded)));
            this.myPanel.Compression += new MyVirtualizingPanel2.OnCompression(this.OnCompression);
            ((ChooserBase<PhotoResult>)this._photoChooserTask).Completed += (new EventHandler<PhotoResult>(this._photoChooserTask_Completed));
            this._appBarButtonSend.Click += (new EventHandler(this._appBarButtonSend_Click));
            this._appBarButtonAttachImage.Click += (new EventHandler(this._appBatButtonAttachImage_Click));
            this._appBarButtonEmojiToggle.Click += (new EventHandler(this._appBarButtonEmojiToggle_Click));
            this._appBarButtonAddAttachment.Click += (new EventHandler(this._appBarButtonAddAttachment_Click));
            this._appBarButtonAttachments.Click += (new EventHandler(this._appBarButtonAttachments_Click));
            this._appBarButtonСhoose.Click += (new EventHandler(this._appBarButtonСhoose_Click));
            this._appBarButtonForward.Click += (new EventHandler(this._appBarButtonForward_Click));
            this._appBarButtonDelete.Click += (new EventHandler(this._appBarButtonDelete_Click));
            this._appBarButtonCancel.Click += (new EventHandler(this._appBarButtonCancel_Click));
            this._appBarMenuItemManageChat.Click += (new EventHandler(this._appBarMenuItemManageChat_Click));
            this._appBarMenuItemDeleteDialog.Click += (new EventHandler(this._appBarMenuItemDeleteDialog_Click));
            this._appBarMenuItemRefresh.Click += (new EventHandler(this._appBarMenuItemRefresh_Click));
            this._appBarMenuItemAddMember.Click += (new EventHandler(this._appBarMenuItemAddMember_Click));
            this._appBarMenuItemDisableEnableNotifications.Click += (new EventHandler(this._appBarMenuItemDisableEnableNotifications_Click));
            this._appBarMenuItemAllowDenyMessagesFromGroup.Click += (new EventHandler(this._appBarMenuItemAllowDenyMessagesFromGroup_Click));
            this._appbarMenuItemPinToStart.Click += (new EventHandler(this._appbarMenuItemPinToStart_Click));
            this._appbarMenuItemShowMaterials.Click += (new EventHandler(this._appbarMenuItemShowMaterials_Click));
            this._createdTimestamp = System.DateTime.Now;
            this.myPanel.InitializeWithScrollViewer((IScrollableArea)new ViewportScrollableAreaAdapter(this.myScroll), true);
            this.OrientationChanged += (new EventHandler<OrientationChangedEventArgs>(this.ConversationPage_OrientationChanged));
            this.myPanel.ScrollPositionChanged += new EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>(this.myPanel_ScrollPositionChanged);
            this.myPanel.ManuallyLoadMore = true;
            this.myPanel.KeepScrollPositionWhenAddingItems = true;
            this.RegisterForCleanup((IMyVirtualizingPanel)this.myPanel);
            // ISSUE: method pointer
            this.ucNewPost.TextBoxPost.TextChanged += (new TextChangedEventHandler((this.textBoxNewMessage_TextChanged)));
            // ISSUE: method pointer
            ((UIElement)this.ucNewPost.TextBoxPost).GotFocus += (new RoutedEventHandler((this.textBoxNewMessage_GotFocus)));
            // ISSUE: method pointer
            ((UIElement)this.ucNewPost.TextBoxPost).LostFocus += (new RoutedEventHandler((this.textBoxNewMessage_LostFocus)));
            this.ucNewPost.TextBlockWatermarkText.Text = CommonResources.Group_SendAMessage;
            this.ucNewMessage.OnAddAttachTap = new Action(this.AddAttachTap);
            this.ucNewMessage.OnSendTap = new Action(this.SendTap);
            this.ucNewMessage.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.PanelOpenClosed);
            Binding binding = new Binding("OutboundMessageVM.Attachments");
            ((FrameworkElement)this.ucNewPost.ItemsControlAttachments).SetBinding((DependencyProperty)ItemsControl.ItemsSourceProperty, binding);
            this.ucNewPost.OnImageDeleteTap = (Action<object>)(sender =>
            {
                FrameworkElement frameworkElement = sender as FrameworkElement;
                if (frameworkElement != null)
                    this.ConversationVM.OutboundMessageVM.RemoveAttachment(frameworkElement.DataContext as IOutboundAttachment);
                this.UpdateAppBar();
            });
            this.SuppressOpenMenuTapArea = true;
            // ISSUE: method pointer
            this.ucNewMessage.TextBoxNewComment.TextChanged += (new TextChangedEventHandler((this.ucNewMessage_OnTextChanged)));
            // ISSUE: method pointer
            this.ucNewMessage.TextBoxNewComment.GotFocus += (async delegate(object o, RoutedEventArgs e)
            {
                await Task.Delay(1);
                this.ucNewMessage_OnTextChanged(this, null);
            });
        }

        ~ConversationPage()
        {
            --ConversationPage.TotalCount;
        }

        private void PanelOpenClosed(object sender, bool e)
        {
            this.UpdateHeaderVisibility();
        }

        private void UpdateHeaderVisibility()
        {
            ((UIElement)this.gridHeader).Visibility = (!FramePageUtils.IsHorizontal || !this.ucNewMessage.PanelControl.IsOpen && !this.ucNewMessage.PanelControl.IsTextBoxTargetFocused ? Visibility.Visible : Visibility.Collapsed);
        }

        private void _appBarButtonEmojiToggle_Click(object sender, EventArgs e)
        {
        }

        protected override void TextBoxPanelIsOpenedChanged(object sender, bool e)
        {
            this.UpdateAppBar();
        }

        private void myPanel_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
        {
            if (e.ScrollHeight != 0.0 && e.ScrollHeight - e.CurrentPosition < VKConstants.LoadMoreNewsThreshold)
            {
                this.ConversationVM.LoadMoreConversations(null);
            }
            else
            {
                if (e.ScrollHeight == 0.0 || e.CurrentPosition >= 100.0)
                    return;
                this.ConversationVM.LoadNewerConversations(null);
            }
        }

        private void _appbarMenuItemPinToStart_Click(object sender, EventArgs e)
        {
            this.ConversationVM.PinToStart();
        }

        private void _appbarMenuItemShowMaterials_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToConversationMaterials(MessagesService.Instance.GetPeerId(this.ConversationVM.UserOrCharId, this.ConversationVM.IsChat));
        }

        private void _appBarMenuItemDisableEnableNotifications_Click(object sender, EventArgs e)
        {
            this.ConversationVM.DisableEnableNotifications((Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!res)
                    new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
                this.UpdateAppBar();
            }))));
        }

        private void _appBarMenuItemAllowDenyMessagesFromGroup_Click(object sender, EventArgs e)
        {
            this.ConversationVM.AllowDenyMessagesFromGroup((Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!res)
                    new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
                this.UpdateAppBar();
            }))));
        }

        private void ConversationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            this.UpdateMargins();
            this.UpdateHeaderVisibility();
            bool p = e.Orientation == PageOrientation.Landscape || e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight;
            SystemTray.IsVisible = (!p);
            foreach (IMyVirtualizingPanel panel in this._panels)
                panel.RespondToOrientationChange(p);
            this.ucNewMessage_OnTextChanged(this, null);
        }

        private void UpdateMargins()
        {
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (e.Cancel || this.CurrentMode != ConversationPage.Mode.Selection)
                return;
            this.CurrentMode = ConversationPage.Mode.Default;
            e.Cancel = true;
        }

        private void _appBarButtonAddAttachment_Click(object sender, EventArgs e)
        {
            this._pickerUC = PickerUC.PickAttachmentTypeAndNavigate(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, null, (() => Navigator.Current.NavigateToPhotoPickerPhotos(10, false, false)));
        }

        private void _appBarMenuItemAddMember_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToPickUser(true, this._userOrChatId, false, 0, PickUserMode.PickForMessage, "", 0, false);
        }

        private void _appBarMenuItemRefresh_Click(object sender, EventArgs e)
        {
            this.ConversationVM.RefreshConversations();
        }

        private void _appBarMenuItemDeleteDialog_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.Conversation_DeleteDialog, (MessageBoxButton)1) != MessageBoxResult.OK)
                return;
            this.ConversationVM.DeleteDialog();
            ObservableCollection<ConversationHeader> conversations = ConversationsViewModel.Instance.Conversations;
            ConversationHeader conversationHeader = conversations.FirstOrDefault<ConversationHeader>((Func<ConversationHeader, bool>)(c =>
            {
                if (c.IsChat == this._isChat)
                    return c.UserOrChatId == this._userOrChatId;
                return false;
            }));
            if (conversationHeader != null)
                conversations.Remove(conversationHeader);
            ((Page)this).NavigationService.GoBackSafe();
        }

        private void _appBarMenuItemManageChat_Click(object sender, EventArgs e)
        {
            this.ManageChatIfApplicable();
        }

        private void ManageChatIfApplicable()
        {
            if (!this.ConversationVM.IsChat || this.ConversationVM.IsKickedFromChat)
                return;
            ((Page)this).NavigationService.Navigate(new System.Uri(string.Format("/VKMessenger;component/Views/ChatEditPage.xaml?ChatId={0}", this.ConversationVM.UserOrCharId), UriKind.Relative));
        }

        private void _photoChooserTask_Completed(object sender, PhotoResult e)
        {
            Logger.Instance.Info("Back from photo chooser");
            if (((TaskEventArgs)e).TaskResult != TaskResult.OK)
                return;
            ParametersRepository.SetParameterForId("ChoosenPhoto", e.ChosenPhoto);
        }

        private void UpdateAppBar()
        {
            if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
                return;
            if (this.CurrentMode == ConversationPage.Mode.Selection)
            {
                this.ApplicationBar = ((IApplicationBar)this._appBarSelection);
                ApplicationBarIconButton appBarButtonDelete = this._appBarButtonDelete;
                ApplicationBarIconButton barButtonForward = this._appBarButtonForward;
                ObservableCollection<MessageViewModel> messages = this.ConversationVM.Messages;
                int num1;
                bool flag = (num1 = messages.Any<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.IsSelected)) ? 1 : 0) != 0;
                barButtonForward.IsEnabled = (num1 != 0);
                int num2 = flag ? 1 : 0;
                appBarButtonDelete.IsEnabled = (num2 != 0);
            }
            else
            {
                int num1 = this.ConversationVM.OutboundMessageVM != null ? this.ConversationVM.OutboundMessageVM.Attachments.Count : 0;
                int num2 = 0;
                if (num1 > 0 || num2 > 0)
                    this._appBarButtonAttachments.IconUri = (new System.Uri(string.Format("./Resources/appbar.attachments-{0}.rest.png", Math.Min(num1 + num2, 10)), UriKind.Relative));
                this.UpdateSendButtonState();
                this.ApplicationBar = (null);
            }
            ((UIElement)this.ucNewMessage).Visibility = (this.CurrentMode == ConversationPage.Mode.Selection ? Visibility.Collapsed : Visibility.Visible);
            this._appbarMenuItemPinToStart.IsEnabled = (!SecondaryTileManager.Instance.TileExistsForConversation(this._userOrChatId, this._isChat));
            this._appBarMenuItemDisableEnableNotifications.Text = (this.ConversationVM.AreNotificationsDisabled ? CommonResources.TurnOnNotifications : CommonResources.TurnOffNotifications);
            this._appBarMenuItemAllowDenyMessagesFromGroup.Text = (this.ConversationVM.IsMessagesFromGroupDenied ? CommonResources.MessagesFromGroupAllow.ToLowerInvariant() : CommonResources.MessagesFromGroupDeny.ToLowerInvariant());
        }

        private void UpdateSendButtonState()
        {
            this._appBarButtonSend.IsEnabled = ((this.ConversationVM.OutboundMessageVM != null ? this.ConversationVM.OutboundMessageVM.Attachments.Count : 0) > 0 || !string.IsNullOrWhiteSpace(this.textBoxNewMessage.Text));
            this.ucNewMessage.UpdateSendButton(this._appBarButtonSend.IsEnabled);
        }

        private void _appBarButtonСhoose_Click(object sender, EventArgs e)
        {
            this.CurrentMode = ConversationPage.Mode.Selection;
        }

        private void _appBarButtonCancel_Click(object sender, EventArgs e)
        {
            this.CurrentMode = ConversationPage.Mode.Default;
        }

        private void _appBarButtonDelete_Click(object sender, EventArgs e)
        {
            List<MessageViewModel> list = this.ConversationVM.Messages.Where<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.IsSelected)).ToList<MessageViewModel>();
            if (MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, list.Count == 1 ? CommonResources.Conversation_DeleteMessage : CommonResources.Conversation_DeleteMessages, (MessageBoxButton)1) != MessageBoxResult.OK)
                return;
            this.ConversationVM.DeleteMessages(list, null);
            this.ConversationVM.IsInSelectionMode = false;
            this.UpdateAppBar();
            ((Control)this).Focus();
        }

        private void _appBarButtonForward_Click(object sender, EventArgs e)
        {
            List<Message> list = this.ConversationVM.Messages.Where<MessageViewModel>((Func<MessageViewModel, bool>)(m => m.IsSelected)).Select<MessageViewModel, Message>((Func<MessageViewModel, Message>)(m => m.Message)).Where<Message>((Func<Message, bool>)(m => (uint)m.mid > 0U)).ToList<Message>();
            ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
            contentDataProvider.ForwardedMessages = list;
            contentDataProvider.StoreDataToRepository();
            ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider)contentDataProvider);
            this.ConversationVM.IsInSelectionMode = false;
            this.UpdateAppBar();
            Navigator.Current.NavigateToPickConversation();
        }

        private void _appBarButtonAttachments_Click(object sender, EventArgs e)
        {
            ((Page)this).NavigationService.Navigate(new System.Uri(string.Format("/VKMessenger;component/Views/ManageAttachmentsPage.xaml?{0}={1}&{2}={3}", NavigationParametersNames.IsChat, this.ConversationVM.IsChat, NavigationParametersNames.UserOrChatId, this.ConversationVM.UserOrCharId), UriKind.Relative));
        }

        private void _appBatButtonAttachImage_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToPhotoPickerPhotos(this.ConversationVM.OutboundMessageVM.NumberOfAttAllowedToAdd, false, false);
        }

        private void ConversationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.Instance.Info("Conversation page loaded in {0} ms. ", (System.DateTime.Now - this._createdTimestamp).TotalMilliseconds);
            Logger.Instance.Info("ConversationPage_Loaded");
            Stopwatch stopwatch = Stopwatch.StartNew();
            if (this.ConversationItems == null)
            {
                this.ConversationItems = new ConversationItems(this.ConversationVM);
                if (this._shouldScrollToUnreadItem)
                {
                    this.ScrollToUnreadItem();
                    this._shouldScrollToUnreadItem = false;
                }
                if (this._messageIdToScrollTo != 0L)
                {
                    this.ScrollToMessageId(this._messageIdToScrollTo);
                    this._messageIdToScrollTo = 0L;
                }
            }
            ((FrameworkElement)this.myPanel).DataContext = this;
            ((FrameworkElement)this.myPanel).SetBinding(MyVirtualizingPanel2.ItemsSourceProperty, new Binding("ConversationItems.Messages"));
            stopwatch.Stop();
            Logger.Instance.Info("MyPanel set context in {0} ms.", stopwatch.ElapsedMilliseconds);
            this.HandleOrientationChange();
        }

        private void BuildAppBar()
        {
            this._defaultAppBar.Opacity = 0.9;
            this._appBarSelection.Opacity = 0.99;
            this._appBarSelection.StateChanged += (new EventHandler<ApplicationBarStateChangedEventArgs>(this._defaultAppBar_StateChanged));
            this._defaultAppBar.StateChanged += (new EventHandler<ApplicationBarStateChangedEventArgs>(this._defaultAppBar_StateChanged));
            this._appBarAttachments.StateChanged += (new EventHandler<ApplicationBarStateChangedEventArgs>(this._defaultAppBar_StateChanged));
            this._appBarAttachments.Opacity = 0.9;
            this._defaultAppBar.Buttons.Add(this._appBarButtonSend);
            this._defaultAppBar.Buttons.Add(this._appBarButtonEmojiToggle);
            this._defaultAppBar.Buttons.Add(this._appBarButtonAddAttachment);
            this._defaultAppBar.Buttons.Add(this._appBarButtonСhoose);
            this._defaultAppBar.MenuItems.Add(this._appbarMenuItemPinToStart);
            this._appBarAttachments.MenuItems.Add(this._appbarMenuItemPinToStart);
            this._defaultAppBar.MenuItems.Add(this._appbarMenuItemShowMaterials);
            this._appBarAttachments.MenuItems.Add(this._appbarMenuItemShowMaterials);
            this._appBarAttachments.Buttons.Add(this._appBarButtonSend);
            this._appBarAttachments.Buttons.Add(this._appBarButtonEmojiToggle);
            this._appBarAttachments.Buttons.Add(this._appBarButtonAttachments);
            this._appBarAttachments.Buttons.Add(this._appBarButtonСhoose);
            this._appBarSelection.Buttons.Add(this._appBarButtonForward);
            this._appBarSelection.Buttons.Add(this._appBarButtonDelete);
            this._appBarSelection.Buttons.Add(this._appBarButtonCancel);
            this._defaultAppBar.MenuItems.Add(this._appBarMenuItemRefresh);
            this._appBarAttachments.MenuItems.Add(this._appBarMenuItemRefresh);
            if (this._isChat)
            {
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemManageChat);
                this._appBarAttachments.MenuItems.Add(this._appBarMenuItemManageChat);
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemDisableEnableNotifications);
                this._appBarAttachments.MenuItems.Add(this._appBarMenuItemDisableEnableNotifications);
            }
            else
            {
                this._defaultAppBar.MenuItems.Add(this._appBarMenuItemAddMember);
                this._appBarAttachments.MenuItems.Add(this._appBarMenuItemAddMember);
                if (this._userOrChatId < 0L && this._userOrChatId > -2000000000L)
                    this._defaultAppBar.MenuItems.Add(this._appBarMenuItemAllowDenyMessagesFromGroup);
            }
            this._defaultAppBar.MenuItems.Add(this._appBarMenuItemDeleteDialog);
            this._appBarAttachments.MenuItems.Add(this._appBarMenuItemDeleteDialog);
        }

        private void _defaultAppBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
        }

        private void _appBarButtonSend_Click(object sender, EventArgs e)
        {
            this.SendMessage();
        }

        private void SendMessage()
        {
            this.ConversationVM.SendMessage(this.textBoxNewMessage.Text);
            this.textBoxNewMessage.Text = string.Empty;
            this.ScrollToBottom(true, false);
            this.UpdateAppBar();
        }

        private void textBoxNewMessage_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void textBoxNewMessage_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            base.HandleOnNavigatedTo(e);
            this._isCurrent = true;
            this.UpdateMargins();
            EventAggregator.Current.Subscribe(this);
            try
            {
                this.MediaControl.Content = MediaPlayerWrapper.Instance.Player;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Cannot assign media player to control", ex);
            }
            if (!this._isInitialized)
            {
                this._userOrChatId = long.Parse(((Page)this).NavigationContext.QueryString[NavigationParametersNames.UserOrChatId]);
                this._isChat = ((Page)this).NavigationContext.QueryString[NavigationParametersNames.IsChat].ToLowerInvariant() == bool.TrueString.ToLowerInvariant();
                this.ucNewMessage.UserOrChatId = this._userOrChatId;
                this.ucNewMessage.IsChat = this._isChat;
                this._startMessageId = !((Page)this).NavigationContext.QueryString.ContainsKey("MessageId") ? 0L : long.Parse(((Page)this).NavigationContext.QueryString["MessageId"]);
                if (this._startMessageId == 0L)
                    this._startMessageId = -1L;
                this._shareContentDataProvider = ShareContentDataProviderManager.RetrieveDataProvider();
                if (this._shareContentDataProvider is ShareExternalContentDataProvider)
                {
                    ((Page)this).NavigationService.ClearBackStack();
                    this.SuppressMenu = true;
                }
                ConversationViewModel vm = ConversationViewModelCache.Current.GetVM(this._userOrChatId, this._isChat, false);
                if (this._startMessageId <= 0L)
                    vm.TrimMessages();
                else
                    vm.Messages.Clear();
                this.textBoxNewMessage.Text = (vm.OutboundMessageVM.MessageText ?? "");
                vm.PropertyChanged += new PropertyChangedEventHandler(this.cvm_PropertyChanged);
                base.DataContext = vm;
                this._isInitialized = true;
                //if (vm.Messages != null && vm.Messages.Count > 0)
                //  this._needScrollBottom = true;
                if (e.IsNavigationInitiator && ((Page)this).NavigationContext.QueryString.ContainsKey("FromLookup") && ((Page)this).NavigationContext.QueryString["FromLookup"] == bool.TrueString)
                    ((Page)this).NavigationService.RemoveBackEntrySafe();
                bool flag = ((Page)this).NavigationContext.QueryString.ContainsKey("IsContactProductSellerMode") && string.Equals(((Page)this).NavigationContext.QueryString["IsContactProductSellerMode"], bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
                this.ConversationVM.CanDettachProductAttachment = !flag;
                OutboundMessageViewModel outboundMessageVm = this.ConversationVM.OutboundMessageVM;
                if (outboundMessageVm != null)
                {
                    ObservableCollection<IOutboundAttachment> attachments = outboundMessageVm.Attachments;
                    if (attachments != null)
                    {
                        foreach (IOutboundAttachment outboundAttachment in (Collection<IOutboundAttachment>)attachments)
                        {
                            OutboundProductAttachment productAttachment = outboundAttachment as OutboundProductAttachment;
                            if (productAttachment != null && !productAttachment.CanDettach)
                            {
                                outboundMessageVm.RemoveAttachment((IOutboundAttachment)productAttachment);
                                if (this.textBoxNewMessage.Text == CommonResources.ContactSellerMessage)
                                {
                                    this.textBoxNewMessage.Text = ("");
                                    break;
                                }
                                break;
                            }
                        }
                        if (flag)
                        {
                            while (attachments.Count > 0)
                                outboundMessageVm.RemoveAttachment(attachments[0]);
                        }
                    }
                }
                List<Message> parameterForIdAndReset = (List<Message>)ParametersRepository.GetParameterForIdAndReset("MessagesToForward");
                if (parameterForIdAndReset != null && parameterForIdAndReset.Count > 0)
                {
                    ((Page)this).NavigationService.RemoveBackEntrySafe();
                    this.ConversationVM.AddForwardedMessagesToOutboundMessage((IList<Message>)parameterForIdAndReset);
                }
                this.BuildAppBar();
            }
            else
            {
                Logger.Instance.Info("FORCE SET VM");
                ConversationViewModelCache.Current.SetVM(this.ConversationVM, false);
            }
            this.ConversationVM.IsOnScreen = true;
            this.ConversationVM.EnsureConversationIsUpToDate(e.NavigationMode == 0, this._startMessageId, null);
            this._startMessageId = -1L;
            this.ConversationVM.LoadHeaderInfoAsync();
            this.ConversationVM.AddAttachmentsFromRepository();
            this.ConversationVM.Scroll = (IScroll)this;
            this.UpdateAppBar();
            if (ParametersRepository.Contains(NavigationParametersNames.NewMessageContents))
            {
                string str = ParametersRepository.GetParameterForIdAndReset(NavigationParametersNames.NewMessageContents).ToString();
                if (!string.IsNullOrWhiteSpace(str))
                {
                    this.textBoxNewMessage.Text = str;
                    this.UpdateAppBar();
                }
            }
            if (ParametersRepository.Contains(NavigationParametersNames.Graffiti))
            {
                GraffitiAttachmentItem parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset(NavigationParametersNames.Graffiti) as GraffitiAttachmentItem;
                if (parameterForIdAndReset != null)
                {
                    this.ConversationVM.SendGraffiti(parameterForIdAndReset);
                    this.ScrollToBottom(true, false);
                }
            }
            DeviceNetworkInformation.NetworkAvailabilityChanged += (new EventHandler<NetworkNotificationEventArgs>(this.DeviceNetworkInformation_NetworkAvailabilityChanged));
            Logger.Instance.Info("ConversationPage, ViewModel hash code={0}", this.ConversationVM.GetHashCode());
            CurrentMediaSource.AudioSource = StatisticsActionSource.messages;
            CurrentMediaSource.VideoSource = StatisticsActionSource.messages;
            CurrentMediaSource.GifPlaySource = StatisticsActionSource.messages;
            CurrentMarketItemSource.Source = MarketItemSource.im;
            stopwatch.Stop();
            if (e.NavigationMode == NavigationMode.Back)
                this.ScrollToBottomIfNeeded();
            InputPane forCurrentView = InputPane.GetForCurrentView();
            forCurrentView.Showing += this.ucNewMessage_OnInputPaneShowing;
        }

        private void ScrollToBottomIfNeeded()
        {
            MessageViewModel messageViewModel = this.ConversationVM.Messages.LastOrDefault<MessageViewModel>();
            if (messageViewModel == null || messageViewModel.Message.@out != 1 || string.IsNullOrEmpty(messageViewModel.Message.action))
                return;
            this.ScrollToBottom(false, false);
        }

        private void cvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;
            if (!(propertyName == "AreNotificationsDisabled") && !(propertyName == "IsMessagesFromGroupDenied"))
                return;
            this.UpdateAppBar();
        }

        private void DeviceNetworkInformation_NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
        {
            if (e.NotificationType != NetworkNotificationType.InterfaceConnected)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this.ConversationVM == null)
                    return;
                this.ConversationVM.EnsureConversationIsUpToDate(false, 0, null);
            }));
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.HandleOnNavigatingFrom(e);
            this.ConversationVM.RemoveUnreadMessagesItem();
            if (e.IsNavigationInitiator)
                this.ucNewMessage.HideAudioRecoringUC();
            else
                this.ucNewMessage.ShowAudioRecordingPreview();
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            this._isCurrent = false;
            EventAggregator.Current.Unsubscribe(this);
            MediaPlayerWrapper.Instance.Stop();
            VoiceMessagePlayer.ResetPlayerData();
            this.MediaControl.Content = null;
            DeviceNetworkInformation.NetworkAvailabilityChanged -= (new EventHandler<NetworkNotificationEventArgs>(this.DeviceNetworkInformation_NetworkAvailabilityChanged));
            this.ConversationVM.Scroll = null;
            this.ConversationVM.IsOnScreen = false;
            this.ConversationVM.OutboundMessageVM.MessageText = this.textBoxNewMessage.Text;
            this.ConversationVM.IsInSelectionMode = false;
            ConversationViewModelCache.Current.SetVM(this.ConversationVM, e.NavigationMode == NavigationMode.Back);
            if (this._needCleanupOnNavigatedFrom)
            {
                this.Cleanup();
                this._needCleanupOnNavigatedFrom = false;
            }
            if (e.NavigationMode == NavigationMode.Back && this._shareContentDataProvider is ShareExternalContentDataProvider)
            {
                ObservableCollection<IOutboundAttachment> attachments = this.ConversationVM.OutboundMessageVM.Attachments;
                for (int index = 0; index < attachments.Count; ++index)
                {
                    if (attachments[index] is OutboundUploadDocumentAttachment)
                    {
                        attachments.RemoveAt(index);
                        --index;
                    }
                }
            }
            InputPane.GetForCurrentView().Showing -= this.ucNewMessage_OnInputPaneShowing;
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            base.OnRemovedFromJournal(e);
            if (!this._isCurrent)
                this.Cleanup();
            else
                this._needCleanupOnNavigatedFrom = true;
        }

        private void Cleanup()
        {
            if (this.ConversationItems != null)
                this.ConversationItems.Cleanup();
            if (this.ConversationVM != null)
                this.ConversationVM.PropertyChanged -= new PropertyChangedEventHandler(this.cvm_PropertyChanged);
            ((DependencyObject)this.ucNewPost.ItemsControlAttachments).ClearValue((DependencyProperty)ItemsControl.ItemsSourceProperty);
            base.DataContext = null;
        }

        private void textBoxNewMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateSendButtonState();
            this.ConversationVM.UserIsTyping();
        }

        public void ScrollToUnreadItem()
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this.ConversationItems != null)
                    this.ScrollToItem(this.ConversationItems.Messages.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(ci => (ci as MessageItem).MVM.Message.action == ConversationViewModel.UNREAD_ITEM_ACTION)));
                else
                    this._shouldScrollToUnreadItem = true;
            }));
        }

        public void ScrollToMessageId(long messageId)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this.ConversationItems != null)
                    this.ScrollToItem(this.ConversationItems.Messages.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>)(ci => (long)(ci as MessageItem).MVM.Message.id == messageId)));
                else
                    this._messageIdToScrollTo = messageId;
            }));
        }

        private void ScrollToItem(IVirtualizable item)
        {
            if (item == null)
                return;
            this.myPanel.ScrollTo(Math.Max(0.0, this.myPanel.GetScrollOffsetForItem(this.ConversationItems.Messages.IndexOf(item)) + item.FixedHeight - (this.Orientation == PageOrientation.Landscape || this.Orientation == PageOrientation.LandscapeLeft || this.Orientation == PageOrientation.LandscapeRight ? 200.0 : 400.0)));
        }

        public void ScrollToBottom(bool animated = true, bool onlyIfInTheBottom = false)
        {
            base.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!(!onlyIfInTheBottom | this.VerticalOffset < 50.0))
                    return;
                if (animated)
                    this.myPanel.ScrollToBottom(false);
                else
                    this.myPanel.ScrollTo(0.0);
            }));
        }

        private void PageBase_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            this.HandleOrientationChange();
        }

        private void HandleOrientationChange()
        {
            bool flag = this.Orientation == PageOrientation.LandscapeRight || this.Orientation == PageOrientation.LandscapeLeft;
            ((FrameworkElement)this.scrollNewMessage).MaxHeight = (flag ? 100.0 : 168.0);
            ((FrameworkElement)this.gridHeader).Height = (flag ? 88.0 : 112.0);
        }

        private void Title_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.ConversationVM.IsChat)
                this.ManageChatIfApplicable();
            else if (this.ConversationVM.UserOrCharId > 0L)
            {
                Navigator.Current.NavigateToUserProfile(this.ConversationVM.UserOrCharId, this.ConversationVM.Title, "", false);
            }
            else
            {
                if (this.ConversationVM.UserOrCharId <= -2000000000L)
                    return;
                Navigator.Current.NavigateToGroup(-this.ConversationVM.UserOrCharId, this.ConversationVM.Title, false);
            }
        }

        public void Handle(MessageActionEvent message)
        {
            if ((long)message.Message.UserOrChatId != this.ConversationVM.UserOrCharId || message.Message.IsChat != this.ConversationVM.IsChat)
                return;
            switch (message.MessageActionType)
            {
                case MessageActionType.Quote:
                    this.ConversationVM.AddForwardedMessagesToOutboundMessage((IList<Message>)new List<Message>()
          {
            message.Message.Message
          });
                    this.UpdateAppBar();
                    break;
                case MessageActionType.Forward:
                    ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
                    contentDataProvider.ForwardedMessages = new List<Message>()
          {
            message.Message.Message
          };
                    contentDataProvider.StoreDataToRepository();
                    ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider)contentDataProvider);
                    Navigator.Current.NavigateToPickConversation();
                    break;
                case MessageActionType.Delete:
                    bool refreshConversations = message.Message == this.ConversationVM.Messages.LastOrDefault<MessageViewModel>();
                    ObservableCollection<AttachmentViewModel> attachments = message.Message.Attachments;
                    DocPreviewVoiceMessage previewVoiceMessage1;
                    if (attachments == null)
                    {
                        previewVoiceMessage1 = null;
                    }
                    else
                    {
                        AttachmentViewModel attachmentViewModel = attachments.FirstOrDefault<AttachmentViewModel>((Func<AttachmentViewModel, bool>)(a =>
                        {
                            Attachment attachment = a.Attachment;
                            bool? nullable;
                            if (attachment == null)
                            {
                                nullable = new bool?();
                            }
                            else
                            {
                                Doc doc = attachment.doc;
                                nullable = doc != null ? new bool?(doc.IsVoiceMessage) : new bool?();
                            }
                            return nullable ?? false;
                        }));
                        if (attachmentViewModel == null)
                        {
                            previewVoiceMessage1 = null;
                        }
                        else
                        {
                            DocPreview preview = attachmentViewModel.Attachment.doc.preview;
                            previewVoiceMessage1 = preview != null ? preview.audio_msg : null;
                        }
                    }
                    DocPreviewVoiceMessage previewVoiceMessage2 = previewVoiceMessage1;
                    if (previewVoiceMessage2 != null)
                    {
                        string currentOriginalSource = MediaPlayerWrapper.Instance.CurrentOriginalSource;
                        if (previewVoiceMessage2.link_ogg == currentOriginalSource)
                            MediaPlayerWrapper.Instance.Stop();
                    }
                    ConversationViewModel conversationVm = this.ConversationVM;
                    List<MessageViewModel> messageViewModels = new List<MessageViewModel>();
                    messageViewModels.Add(message.Message);
                    Action callback = (Action)(() =>
                    {
                        if (!refreshConversations)
                            return;
                        ConversationsViewModel.Instance.RefreshConversations(true);
                    });
                    conversationVm.DeleteMessages(messageViewModels, callback);
                    ((Control)this).Focus();
                    break;
                case MessageActionType.SelectUnselect:
                    message.Message.IsSelected = !message.Message.IsSelected;
                    this.UpdateAppBar();
                    break;
                case MessageActionType.EnterSelectMode:
                    message.Message.IsSelected = true;
                    this._appBarButtonСhoose_Click(null, null);
                    break;
            }
        }

        private void OnCompression(object sender, CompressionEventArgs e)
        {
            if (e.Type == CompressionType.Top)
                this.ConversationVM.LoadNewerConversations(null);
            if (e.Type != CompressionType.Bottom)
                return;
            this.ConversationVM.LoadMoreConversations(null);
        }

        private void ArrowDownTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.ConversationVM.RefreshConversations();
        }

        public void Handle(SpriteElementTapEvent data)
        {
            if (!this._isCurrent)
                return;
            base.Dispatcher.BeginInvoke((Action)(() =>
            {
                int selectionStart = this.textBoxNewMessage.SelectionStart;
                this.textBoxNewMessage.Text = (this.textBoxNewMessage.Text.Insert(selectionStart, data.Data.ElementCode));
                this.textBoxNewMessage.Select(selectionStart + data.Data.ElementCode.Length, 0);
            }));
        }

        public void Handle(StickerItemTapEvent message)
        {
            if (!this._isCurrent)
                return;
            this.ConversationVM.SendSticker(message.StickerItem, message.Referrer);
            this.ScrollToBottom(true, false);
        }

        private void SendTap()
        {
            if (!this._appBarButtonSend.IsEnabled)
                return;
            this.SendMessage();
        }

        private void AddAttachTap()
        {
            ConversationViewModel conversationVm = this.ConversationVM;
            ConversationInfo conversationInfo = new ConversationInfo(conversationVm.IsChat, conversationVm.UserOrCharId, conversationVm.User, conversationVm.Chat);
            AppGlobalStateData globalState = AppGlobalStateManager.Current.GlobalState;
            List<NamedAttachmentType> attachmentTypes = new List<NamedAttachmentType>((IEnumerable<NamedAttachmentType>)AttachmentTypes.AttachmentTypesWithPhotoFromGalleryGraffitiAndLocation);
            if (conversationVm.IsDialogWithOtherUser && globalState.CanSendMoneyTransfers || conversationVm.IsDialogWithCommunity && globalState.CanSendMoneyTransfersToGroups)
                attachmentTypes.Insert(4, AttachmentTypes.MoneyAttachmentType);
            if (conversationVm.IsDialogWithUserOrChat)
                attachmentTypes.Insert(5, AttachmentTypes.GiftAttachmentType);
            AttachmentPickerUC.Show(attachmentTypes, this.ConversationVM.OutboundMessageVM.NumberOfAttAllowedToAdd, (Action)(() =>
            {
                this.ConversationVM.AddAttachmentsFromRepository();
                this.UpdateAppBar();
            }), this.ConversationVM.OutboundMessageVM.HaveGeoAttachment, 0, 0, conversationInfo);
        }

        private void OptionsButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.PrepareMenu();
            this.FriendOptionsMenu.IsOpen = true;
        }

        private void PrepareMenu()
        {
            ((UIElement)this.menuItemAllowDenyMessagesFromGroup).Visibility = (this._defaultAppBar.MenuItems.Contains(this._appBarMenuItemAllowDenyMessagesFromGroup) ? Visibility.Visible : Visibility.Collapsed);
            this.menuItemAllowDenyMessagesFromGroup.Header = this._appBarMenuItemAllowDenyMessagesFromGroup.Text;
            ((UIElement)this.menuItemPinToStart).Visibility = (!this._defaultAppBar.MenuItems.Contains(this._appbarMenuItemPinToStart) || !this._appbarMenuItemPinToStart.IsEnabled ? Visibility.Collapsed : Visibility.Visible);
            ((UIElement)this.menuItemDisableEnableNotifications).Visibility = (this._defaultAppBar.MenuItems.Contains(this._appBarMenuItemDisableEnableNotifications) ? Visibility.Visible : Visibility.Collapsed);
            this.menuItemDisableEnableNotifications.Header = this._appBarMenuItemDisableEnableNotifications.Text;
            if (this.ConversationVM.CanAddMembers && this._defaultAppBar.MenuItems.Contains(this._appBarMenuItemAddMember))
                ((UIElement)this.menuItemAddMember).Visibility = Visibility.Visible;
            else
                ((UIElement)this.menuItemAddMember).Visibility = Visibility.Collapsed;
            ((UIElement)this.menuItemDeleteDialog).Visibility = (this._defaultAppBar.MenuItems.Contains(this._appBarMenuItemDeleteDialog) ? Visibility.Visible : Visibility.Collapsed);
        }

        private void MenuPinToStartClick(object sender, RoutedEventArgs e)
        {
            this._appbarMenuItemPinToStart_Click(null, null);
        }

        private void MenuShowMaterialsClick(object sender, RoutedEventArgs e)
        {
            this._appbarMenuItemShowMaterials_Click(null, null);
        }

        private void MenuDisableEnableNotificationsClick(object sender, RoutedEventArgs e)
        {
            this._appBarMenuItemDisableEnableNotifications_Click(null, null);
        }

        private void MenuItemAllowDenyMessagesFromGroupClick(object sender, RoutedEventArgs e)
        {
            this._appBarMenuItemAllowDenyMessagesFromGroup_Click(null, null);
        }

        private void MenuAddMemberClick(object sender, RoutedEventArgs e)
        {
            this._appBarMenuItemAddMember_Click(null, null);
        }

        private void MenuDeleteDialogClick(object sender, RoutedEventArgs e)
        {
            this._appBarMenuItemDeleteDialog_Click(null, null);
        }

        public void ScrollToOffset(double offset)
        {
            this.myPanel.ScrollTo(offset);
        }

        private void MenuRefreshClick(object sender, RoutedEventArgs e)
        {
            MediaPlayerWrapper.Instance.Stop();
            this.ConversationVM.RefreshConversations();
        }

        private void ucNewMessage_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.UpdateMentionPicker())
                return;
            this.ucNewMessage.MentionPicker.IsVisible = false;
        }

        private bool UpdateMentionPicker()
        {
            if (!this.ConversationVM.IsChat || this.ConversationVM.IsKickedFromChat || !this.GetIsSupportedOrientation())
                return false;
            TextBox textBoxNewComment = this.ucNewMessage.TextBoxNewComment;
            string text = textBoxNewComment.Text;
            if (text.Contains<char>('@') || text.Contains<char>('*'))
            {
                for (int index = text.Length - 1; index >= 0; --index)
                {
                    char mentionStartSymbol = text[index];
                    char ch1 = index != 0 ? text[index - 1] : char.MinValue;
                    char[] chArray = new char[13]
          {
            ' ',
            '.',
            ',',
            ':',
            ';',
            '\'',
            '"',
            '«',
            '»',
            '(',
            ')',
            '<',
            '>'
          };
                    if (((int)mentionStartSymbol == 64 || (int)mentionStartSymbol == 42) && ((int)ch1 == 0 || ((IEnumerable<char>)chArray).Contains<char>(ch1)))
                    {
                        string source = text.Remove(0, index + 1);
                        char ch2 = source.FirstOrDefault<char>((Func<char, bool>)(c => !ConversationPage.GetIsValidDomainSymbol(c)));
                        if ((int)ch2 != 0)
                            source = source.Remove(source.IndexOf(ch2));
                        int selectionStart = textBoxNewComment.SelectionStart;
                        if (textBoxNewComment.SelectionLength > 0 || selectionStart <= index || selectionStart > index + source.Length + 1)
                            return false;
                        List<User> chatMembers = this.ConversationVM.ChatMembers;
                        if (chatMembers == null || !chatMembers.Any<User>())
                            return false;
                        string q = source;
                        List<VKClient.Common.Library.FriendHeader> pickerItems = ConversationPage.GetPickerItems(chatMembers, q);
                        if (!pickerItems.Any<VKClient.Common.Library.FriendHeader>())
                            return false;
                        MentionPickerUC mentionPicker = this.ucNewMessage.MentionPicker;
                        if (!mentionPicker.IsVisible)
                        {
                            this.ShowMentionPicker(pickerItems, mentionStartSymbol.ToString() + q, mentionStartSymbol);
                        }
                        else
                        {
                            if (ConversationPage.GetAreDifferentLists(mentionPicker.ItemsSource, pickerItems))
                                mentionPicker.ItemsSource = pickerItems;
                            mentionPicker.SearchDomain = mentionStartSymbol.ToString() + q;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool GetIsValidDomainSymbol(char c)
        {
            if (((int)c < 97 || (int)c > 122) && ((int)c < 65 || (int)c > 90) && (((int)c < 1072 || (int)c > 1103) && ((int)c < 1040 || (int)c > 1103)) && ((int)c < 48 || (int)c > 57))
                return (int)c == 95;
            return true;
        }

        private static List<VKClient.Common.Library.FriendHeader> GetPickerItems(List<User> users, string q)
        {
            List<VKClient.Common.Library.FriendHeader> friendHeaderList = new List<VKClient.Common.Library.FriendHeader>();
            q = q.ToLower();
            if (q == "")
            {
                List<VKClient.Common.Library.FriendHeader> list = users.Select<User, VKClient.Common.Library.FriendHeader>((Func<User, VKClient.Common.Library.FriendHeader>)(u => new VKClient.Common.Library.FriendHeader(u, false))).ToList<VKClient.Common.Library.FriendHeader>();
                list.Remove(list.First<VKClient.Common.Library.FriendHeader>((Func<VKClient.Common.Library.FriendHeader, bool>)(h => h.UserId == AppGlobalStateManager.Current.LoggedInUserId)));
                return list;
            }
            foreach (User user in users)
            {
                if (user.id != AppGlobalStateManager.Current.LoggedInUserId)
                {
                    bool flag = false;
                    if (user.domain.ToLower().StartsWith(q))
                        flag = true;
                    else if (user.first_name.ToLower().StartsWith(q) || user.last_name.ToLower().StartsWith(q))
                    {
                        flag = true;
                    }
                    else
                    {
                        MatchStrings matchStrings1 = TransliterationHelper.GetMatchStrings(user.first_name.ToLower());
                        MatchStrings matchStrings2 = TransliterationHelper.GetMatchStrings(user.last_name.ToLower());
                        foreach (string latinString in matchStrings1.LatinStrings)
                        {
                            if (latinString.StartsWith(q))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            foreach (string latinString in matchStrings2.LatinStrings)
                            {
                                if (latinString.StartsWith(q))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                foreach (string cyrillicString in matchStrings1.CyrillicStrings)
                                {
                                    if (cyrillicString.StartsWith(q))
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    foreach (string cyrillicString in matchStrings2.CyrillicStrings)
                                    {
                                        if (cyrillicString.StartsWith(q))
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (flag)
                        friendHeaderList.Add(new VKClient.Common.Library.FriendHeader(user, false));
                }
            }
            return friendHeaderList;
        }

        private void ShowMentionPicker(List<VKClient.Common.Library.FriendHeader> items, string searchDomain, char mentionStartSymbol)
        {
            MentionPickerUC expr_0B = this.ucNewMessage.MentionPicker;
            expr_0B.ItemsSource = items;
            expr_0B.SearchDomain = searchDomain;
            expr_0B.Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.mentionPicker_OnTap));
            InputPane forCurrentView = InputPane.GetForCurrentView();
            //WindowsRuntimeMarshal.AddEventHandler<TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>>(new Func<TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>, EventRegistrationToken>(forCurrentView.add_Hiding), new Action<EventRegistrationToken>(forCurrentView.remove_Hiding), new TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>(this.ucNewMessage_OnInputPaneHiding));
            expr_0B.Closed += new EventHandler(this.mentionPicker_OnClosed);
            expr_0B.ItemSelected += new EventHandler<object>(this.mentionPicker_OnItemSelected);
            expr_0B.MentionStartSymbol = mentionStartSymbol;
            expr_0B.IsVisible = true;
        }

        private async void mentionPicker_OnItemSelected(object sender, object e)
        {
            VKClient.Common.Library.FriendHeader friend = e as VKClient.Common.Library.FriendHeader;
            MentionPickerUC picker = this.ucNewMessage.MentionPicker;
            if (friend == null)
                return;
            await Task.Delay(1);
            TextBox textBoxNewComment = this.ucNewMessage.TextBoxNewComment;
            string text = textBoxNewComment.Text;
            int startIndex = text.LastIndexOf(picker.SearchDomain);
            string userMention = ConversationPage.GetUserMention(friend.UserId, friend.FullName, picker.MentionStartSymbol);
            string str = text.Remove(startIndex, Math.Max(picker.SearchDomain.Length, 1)).Insert(startIndex, userMention);
            textBoxNewComment.Text = str;
            int num = startIndex + userMention.Length;
            textBoxNewComment.SelectionStart = num;
            picker.IsVisible = false;
        }

        private void mentionPicker_OnClosed(object sender, EventArgs e)
        {
            //WindowsRuntimeMarshal.RemoveEventHandler<TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>>(new Action<EventRegistrationToken>(InputPane.GetForCurrentView().remove_Hiding), new TypedEventHandler<InputPane, InputPaneVisibilityEventArgs>(this.ucNewMessage_OnInputPaneHiding));
            InputPane.GetForCurrentView().Hiding -= this.ucNewMessage_OnInputPaneHiding;
            this.ucNewMessage.MentionPicker.Tap -= (new EventHandler<System.Windows.Input.GestureEventArgs>(this.mentionPicker_OnTap));
            this.ucNewMessage.MentionPicker.Closed -= new EventHandler(this.mentionPicker_OnClosed);
            this.ucNewMessage.MentionPicker.ItemSelected -= new EventHandler<object>(this.mentionPicker_OnItemSelected);
        }

        private void ucNewMessage_OnInputPaneShowing(InputPane sender, InputPaneVisibilityEventArgs e)
        {
            this.ucNewMessage_OnTextChanged(this, null);
        }

        private void ucNewMessage_OnInputPaneHiding(InputPane sender, InputPaneVisibilityEventArgs e)
        {
            this.ucNewMessage.MentionPicker.IsVisible = false;
        }

        private void mentionPicker_OnTap(object sender, RoutedEventArgs e)
        {
            ((Control)this.ucNewMessage.TextBoxNewComment).Focus();
        }

        private bool GetIsSupportedOrientation()
        {
            if (this.Orientation != PageOrientation.None && this.Orientation != PageOrientation.Portrait && this.Orientation != PageOrientation.PortraitUp)
                return this.Orientation == PageOrientation.PortraitDown;
            return true;
        }

        private static string GetUserMention(long id, string name, char mentionSymbol)
        {
            return string.Format("{0}id{1} ({2})", mentionSymbol, id, name);
        }

        private static bool GetAreDifferentLists(List<VKClient.Common.Library.FriendHeader> firstList, List<VKClient.Common.Library.FriendHeader> secondList)
        {
            if (firstList.Count != secondList.Count)
                return true;
            for (int index = 0; index < firstList.Count; ++index)
            {
                if (firstList[index].UserId != secondList[index].UserId)
                    return true;
            }
            return false;
        }

        public void Handle(VoiceMessageSentEvent message)
        {
            this.ConversationVM.OutboundMessageVM.AddVoiceMessageAttachment(message.File, message.Duration, message.Waveform);
            this.SendMessage();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new System.Uri("/VKMessenger;component/Views/ConversationPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.gridHeader = (Grid)base.FindName("gridHeader");
            this.TitlePanel = (StackPanel)base.FindName("TitlePanel");
            this.textBlockTitle = (TextBlock)base.FindName("textBlockTitle");
            this.textBlockSubtitleVertical = (TextBlock)base.FindName("textBlockSubtitleVertical");
            this.FriendOptionsMenu = (ContextMenu)base.FindName("FriendOptionsMenu");
            this.menuItemAllowDenyMessagesFromGroup = (MenuItem)base.FindName("menuItemAllowDenyMessagesFromGroup");
            this.menuItemRefresh = (MenuItem)base.FindName("menuItemRefresh");
            this.menuItemPinToStart = (MenuItem)base.FindName("menuItemPinToStart");
            this.menuItemShowMaterials = (MenuItem)base.FindName("menuItemShowMaterials");
            this.menuItemDisableEnableNotifications = (MenuItem)base.FindName("menuItemDisableEnableNotifications");
            this.menuItemAddMember = (MenuItem)base.FindName("menuItemAddMember");
            this.menuItemDeleteDialog = (MenuItem)base.FindName("menuItemDeleteDialog");
            this.ContentPanel = (Grid)base.FindName("ContentPanel");
            this.myScroll = (ViewportControl)base.FindName("myScroll");
            this.myPanel = (MyVirtualizingPanel2)base.FindName("myPanel");
            this.ucNewMessage = (NewMessageUC)base.FindName("ucNewMessage");
            this.MediaControl = (ContentControl)base.FindName("MediaControl");
        }

        public enum Mode
        {
            Default,
            Selection,
        }
    }
}
