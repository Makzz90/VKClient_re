using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Market.ViewModels;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Common.Market.Views
{
    public class ProductPage : PageBase, IHandle<SpriteElementTapEvent>, IHandle, IHandle<StickerItemTapEvent>, ISupportShare
    {
        private readonly PhotoChooserTask _photoChooserTask = new PhotoChooserTask()
        {
            ShowCamera = true
        };
        private DialogService _ds = new DialogService();
        private const int COMMENTS_PRELOAD_COUNT = 7;
        private readonly ViewportScrollableAreaAdapter _adapter;
        private bool _isInitialized;
        private long _ownerId;
        private long _productId;
        private ProductViewModel _viewModel;
        private WallPostViewModel _commentVM;
        private double _floatActionsLockThreshold;
        private SharePostUC _sharePostUC;
        private const string COMMENT_TEXT_STATE_KEY = "CommentText";
        internal Style ListBoxItemNavDotsStyle;
        internal RowDefinition rowDefinitionContent;
        internal GenericHeaderUC ucHeader;
        internal MoreActionsUC ucMoreActions;
        internal ViewportControl viewportControl;
        internal StackPanel stackPanelContent;
        internal Canvas canvasBackground;
        internal SlideView slideView;
        internal ListBox listBoxNavDots;
        internal TextBlock textBlockMetaData;
        internal Grid gridAction;
        internal CommentsGenericUC ucCommentGeneric;
        internal NewMessageUC ucNewMessage;
        internal FloatActionsUC ucFloatActions;
        private bool _contentLoaded;

        public ProductPage()
        {
            this.InitializeComponent();
            this.ucHeader.TextBlockTitle.Text = CommonResources.Product.ToUpperInvariant();
            this.ucHeader.OnHeaderTap = new Action(this.Header_OnTap);
            this.ucMoreActions.SetBlue();
            this.ucMoreActions.TapCallback = new Action(this.ShowContextMenu);
            this._adapter = new ViewportScrollableAreaAdapter(this.viewportControl);
            this.ucCommentGeneric.InitializeWithScrollViewer((IScrollableArea)this._adapter);
            this.ucCommentGeneric.UCNewComment = this.ucNewMessage;
            this.ucNewMessage.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.Panel_OnIsOpenedChanged);
            this.ucNewMessage.OnAddAttachTap = new Action(this.AddAttach);
            this.ucNewMessage.OnSendTap = new Action(this.Send);
            this.ucNewMessage.UCNewPost.OnImageDeleteTap = new Action<object>(this.DeleteImage);
            this.ucNewMessage.UCNewPost.TextBlockWatermarkText.Text = CommonResources.Comment;
            Binding binding = new Binding("OutboundAttachments");
            this.ucNewMessage.UCNewPost.ItemsControlAttachments.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            this.viewportControl.BindViewportBoundsTo((FrameworkElement)this.stackPanelContent);
            this.viewportControl.ViewportChanged += new EventHandler<ViewportChangedEventArgs>(this.ViewportControl_OnViewportChanged);
            this.RegisterForCleanup((IMyVirtualizingPanel)this.ucCommentGeneric.Panel);
            this._photoChooserTask.Completed += new EventHandler<PhotoResult>(ProductPage.PhotoChooserTask_OnCompleted);
            this.ucCommentGeneric.UCNewComment.TextBoxNewComment.TextChanged += new TextChangedEventHandler(this.TextBoxNewComment_OnTextChanged);
            EventAggregator.Current.Subscribe((object)this);
        }

        private void ViewportControl_OnViewportChanged(object sender, ViewportChangedEventArgs args)
        {
            if (this.ucNewMessage.PanelControl.IsOpen || this.ucNewMessage.PanelControl.IsTextBoxTargetFocused)
                return;
            this.UpdateFloatControlsVisibility();
        }

        private void UpdateFloatControlsVisibility()
        {
            this.ucFloatActions.Visibility = this.viewportControl.Viewport.Y <= this._floatActionsLockThreshold || this._floatActionsLockThreshold <= 0.0 ? Visibility.Visible : Visibility.Collapsed;
            double num = this._floatActionsLockThreshold + this.ucNewMessage.ActualHeight;
            this.ucNewMessage.Visibility = this.viewportControl.Viewport.Y <= num || num <= 0.0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Header_OnTap()
        {
            this.ucCommentGeneric.Panel.ScrollToBottom(false);
        }

        private void ShowContextMenu()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            MenuItem menuItem1 = new MenuItem();
            string copyLink = CommonResources.CopyLink;
            menuItem1.Header = (object)copyLink;
            MenuItem menuItem2 = menuItem1;
            menuItem2.Click += new RoutedEventHandler(this.MenuItemCopyLink_OnClicked);
            menuItems.Add(menuItem2);
            this.ucMoreActions.SetMenu(menuItems);
            this.ucMoreActions.ShowMenu();
        }

        private void MenuItemCopyLink_OnClicked(object sender, RoutedEventArgs e)
        {
            this._viewModel.CopyLink();
        }

        private void Panel_OnIsOpenedChanged(object sender, bool e)
        {
            if (this.ucNewMessage.PanelControl.IsOpen || this.ucNewMessage.PanelControl.IsTextBoxTargetFocused)
            {
                this.ucCommentGeneric.Panel.ScrollTo(this._adapter.VerticalOffset + this.ucNewMessage.PanelControl.PortraitOrientationHeight);
            }
            else
            {
                this.ucCommentGeneric.Panel.ScrollTo(this._adapter.VerticalOffset - this.ucNewMessage.PanelControl.PortraitOrientationHeight);
                this.UpdateFloatControlsVisibility();
            }
        }

        private void AddAttach()
        {
            AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action)(() => PostCommentsPage.HandleInputParams(this._commentVM)), true, 0L, 0, null);
        }

        private void Send()
        {
            this.ucCommentGeneric.AddComment(this._commentVM.OutboundAttachments.ToList<IOutboundAttachment>(), (Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!res)
                    return;
                this.InitializeCommentVM();
            }))), null, "");
        }

        private void DeleteImage(object sender)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;
            this._commentVM.OutboundAttachments.Remove(frameworkElement.DataContext as IOutboundAttachment);
        }

        private static void PhotoChooserTask_OnCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult != TaskResult.OK)
                return;
            ParametersRepository.SetParameterForId("ChoosenPhoto", (object)e.ChosenPhoto);
        }

        private void TextBoxNewComment_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            this.ucNewMessage.UpdateSendButton(this._viewModel.CanComment && this.IsReadyToSend());
        }

        private bool IsReadyToSend()
        {
            string text = this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text;
            ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
            if (!string.IsNullOrWhiteSpace(text) && outboundAttachments.Count == 0)
                return true;
            if (outboundAttachments.Count > 0)
                return outboundAttachments.All<IOutboundAttachment>((Func<IOutboundAttachment, bool>)(a => a.UploadState == OutboundAttachmentUploadState.Completed));
            return false;
        }

        public void InitiateShare()
        {
            this.OpenSharePopup();
        }

        private void OpenSharePopup()
        {
            this._ds = new DialogService()
            {
                SetStatusBarBackground = false,
                HideOnNavigation = false
            };
            this._sharePostUC = new SharePostUC();
            this._sharePostUC.SendTap += new EventHandler(this.ButtonSendWithMessage_Click);
            this._sharePostUC.ShareTap += (EventHandler)((sender, args) => this.Share(0L, ""));
            this._ds.Child = (FrameworkElement)this._sharePostUC;
            this._ds.AnimationType = DialogService.AnimationTypes.None;
            this._ds.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
            this._ds.Show(null);
        }

        private void Share(long groupId = 0, string groupName = "")
        {
            this._ds.Hide();
            this._viewModel.ShareToGroup(UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text), groupId, groupName);
        }

        private void ButtonSendWithMessage_Click(object sender, EventArgs eventArgs)
        {
            this._ds.Hide();
            this._viewModel.Share(this._sharePostUC.Text);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            bool flag = true;
            if (!this._isInitialized)
            {
                IDictionary<string, string> queryString = this.NavigationContext.QueryString;
                this._ownerId = long.Parse(queryString["OwnerId"]);
                this._productId = long.Parse(queryString["ProductId"]);
                Product product = ParametersRepository.GetParameterForIdAndReset("Product") as Product;
                this.InitializeCommentVM();
                this._viewModel = product != null ? new ProductViewModel(product) : new ProductViewModel(this._ownerId, this._productId);
                this._viewModel.PageLoadInfoViewModel.LoadingStateChangedCallback = new Action(this.OnLoadedStateChanged);
                this.DataContext = (object)this._viewModel;
                this._viewModel.Reload();
                this.RestoreUnboundState();
                this._isInitialized = true;
                flag = false;
            }
            if (!flag && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New))
                WallPostVMCacheManager.TryDeserializeInstance(this._commentVM);
            this.ProcessInputData();
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            if (e.NavigationMode != NavigationMode.Back)
                WallPostVMCacheManager.RegisterForDelayedSerialization(this._commentVM);
            if (e.NavigationMode == NavigationMode.Back)
                WallPostVMCacheManager.ResetInstance();
            this.SaveUnboundState();
        }

        private void SaveUnboundState()
        {
            this.State["CommentText"] = (object)this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text;
        }

        private void RestoreUnboundState()
        {
            if (!this.State.ContainsKey("CommentText"))
                return;
            this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text = this.State["CommentText"].ToString();
        }

        private void InitializeCommentVM()
        {
            this._commentVM = WallPostViewModel.CreateNewProductCommentVM(this._ownerId, this._productId);
            this._commentVM.PropertyChanged += new PropertyChangedEventHandler(this.CommentsVM_OnPropertyChanged);
            this.ucNewMessage.DataContext = (object)this._commentVM;
        }

        private void CommentsVM_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender != this._commentVM || !(e.PropertyName == "CanPublish"))
                return;
            ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
            Func<IOutboundAttachment, bool> predicate = (Func<IOutboundAttachment, bool>)(a => a.UploadState != OutboundAttachmentUploadState.Uploading);
            //Func<IOutboundAttachment, bool> predicate = null;
            if (!outboundAttachments.All<IOutboundAttachment>(predicate))
                return;
            this._viewModel.SetInProgress(false, "");
        }

        private void ProcessInputData()
        {
            Group group = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
            if (group != null)
                this.Share(group.id, group.name);
            Photo photo = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
            if (photo != null)
                this._commentVM.AddAttachment((IOutboundAttachment)OutboundPhotoAttachment.CreateForChoosingExistingPhoto(photo, 0L, false, PostType.WallPost));
            VKClient.Common.Backend.DataObjects.Video video = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
            if (video != null)
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundVideoAttachment(video));
            AudioObj audio = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
            if (audio != null)
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundAudioAttachment(audio));
            Doc pickedDocument = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
            if (pickedDocument != null)
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundDocumentAttachment(pickedDocument));
            List<Stream> streamList1 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            List<Stream> streamList2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
            if (streamList1 != null && streamList2 != null)
            {
                for (int index = 0; index < streamList1.Count; ++index)
                {
                    Stream stream1 = streamList1[index];
                    Stream stream2 = streamList2[index];
                    long userOrGroupId = 0;
                    int num1 = 0;
                    Stream previewStream = stream2;
                    int num2 = 0;
                    this._commentVM.AddAttachment((IOutboundAttachment)OutboundPhotoAttachment.CreateForUploadNewPhoto(stream1, userOrGroupId, num1 != 0, previewStream, (PostType)num2));
                }
                this._viewModel.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
                this._commentVM.UploadAttachments();
            }
            FileOpenPickerContinuationEventArgs continuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
            if ((continuationEventArgs == null || !(continuationEventArgs.Files).Any<StorageFile>()) && !ParametersRepository.Contains("PickedPhotoDocument"))
                return;
            object parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
            StorageFile file = continuationEventArgs != null ? (continuationEventArgs.Files).First<StorageFile>() : (StorageFile)ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocument");
            AttachmentType result;
            if (parameterForIdAndReset == null || !Enum.TryParse<AttachmentType>(parameterForIdAndReset.ToString(), out result))
                return;
            if (result != AttachmentType.VideoFromPhone)
            {
                if (result != AttachmentType.DocumentFromPhone && result != AttachmentType.DocumentPhoto)
                    return;
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundUploadDocumentAttachment(file));
                this._commentVM.UploadAttachments();
            }
            else
            {
                this._commentVM.AddAttachment((IOutboundAttachment)new OutboundUploadVideoAttachment(file, true, 0L));
                this._commentVM.UploadAttachments();
            }
        }

        private void OnLoadedStateChanged()
        {
            if (this._viewModel.PageLoadInfoViewModel.LoadingState != PageLoadingState.Loaded)
                return;
            this.UpdateMetaData();
            this._viewModel.LoadMoreComments(7, new Action<bool>(this.CommentsLoadedCallback));
        }

        private void UpdateMetaData()
        {
            this.textBlockMetaData.Text = this._viewModel.MetaData;
        }

        private void CommentsLoadedCallback(bool success)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this._viewModel.PageLoadInfoViewModel.LoadingState == PageLoadingState.LoadingFailed)
                    return;
                this.ucCommentGeneric.ProcessLoadedComments(true);
                if (this._ownerId < 0L)
                {
                    Group cachedGroup = GroupsService.Current.GetCachedGroup(-this._ownerId);
                    if (cachedGroup != null)
                        this.ucNewMessage.SetAdminLevel(cachedGroup.admin_level);
                }
                this.ucNewMessage.Visibility = Visibility.Visible;
                this.UpdateLayout();
                this.ucNewMessage.Visibility = Visibility.Collapsed;
                this.UpdateLayout();
                this.UpdateFloatControlsLockThreshold();
            }));
        }

        public void Handle(SpriteElementTapEvent message)
        {
            if (!this._isCurrentPage)
                return;
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                TextBox textBoxNewComment = this.ucCommentGeneric.UCNewComment.TextBoxNewComment;
                int selectionStart = textBoxNewComment.SelectionStart;
                string str = textBoxNewComment.Text.Insert(selectionStart, message.Data.ElementCode);
                textBoxNewComment.Text = str;
                int start = selectionStart + message.Data.ElementCode.Length;
                int length = 0;
                textBoxNewComment.Select(start, length);
            }));
        }

        public void Handle(StickerItemTapEvent message)
        {
            if (!this._isCurrentPage)
                return;
            this.ucCommentGeneric.AddComment(new List<IOutboundAttachment>(), (Action<bool>)(res => { }), message.StickerItem, message.Referrer);
        }

        private void GridContent_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = e.NewSize.Height;
            if (double.IsInfinity(height) || double.IsNaN(height))
                return;
            this.canvasBackground.Height = height;
            this.canvasBackground.Children.Clear();
            Rectangle rect = new Rectangle();
            double num = height;
            rect.Height = num;
            Thickness thickness = new Thickness(0.0);
            rect.Margin = thickness;
            double width = e.NewSize.Width;
            rect.Width = width;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneNewsBackgroundBrush"];
            rect.Fill = (Brush)solidColorBrush;
            foreach (UIElement coverByRectangle in RectangleHelper.CoverByRectangles(rect))
                this.canvasBackground.Children.Add(coverByRectangle);
        }

        private void StackPanelProductInfo_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = e.NewSize.Height;
            if (double.IsInfinity(height) || double.IsNaN(height))
                return;
            this.ucCommentGeneric.Panel.DeltaOffset = -height;
        }

        private void UpdateFloatControlsLockThreshold()
        {
            try
            {
                this._floatActionsLockThreshold = this.gridAction.TransformToVisual((UIElement)this.stackPanelContent).Transform(new Point(0.0, 0.0)).Y - this.rowDefinitionContent.ActualHeight + this.ucFloatActions.ActualHeight;
                if (this.ucNewMessage.Visibility == Visibility.Visible)
                    this._floatActionsLockThreshold = this._floatActionsLockThreshold - this.ucNewMessage.ActualHeight;
                this.UpdateFloatControlsVisibility();
            }
            catch
            {
            }
        }

        private void Description_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this._viewModel.ExpandDescription();
            this.UpdateLayout();
            this.UpdateFloatControlsLockThreshold();
        }

        private void MetaData_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this._viewModel.NavigateToGroup();
        }

        private void BorderWikiPage_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this._viewModel.NavigateToMarketWiki();
        }

        private void SlideView_OnSelectionChanged(object sender, int e)
        {
            if (this.listBoxNavDots.ItemsSource == null)
                this.listBoxNavDots.ItemsSource = (IEnumerable)this._viewModel.Photos;
            if (this._viewModel.Photos.Count == 0)
                return;
            this.listBoxNavDots.SelectedIndex = e;
        }

        private void ContactSellerButton_OnClick(object sender, RoutedEventArgs e)
        {
            this._viewModel.ContactSeller();
        }

        private void SlideView_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this._viewModel.OpenPhotoViewer(this.slideView.SelectedIndex);
        }

        private void TextBlockMetaData_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProductViewModel productViewModel = this._viewModel;
            if (string.IsNullOrEmpty(productViewModel != null ? productViewModel.MetaData : null) || string.IsNullOrEmpty(this.textBlockMetaData.Text) || e.NewSize.Height <= this.textBlockMetaData.LineHeight)
                return;
            this.textBlockMetaData.Text = this._viewModel.MetaData.Replace(" · ", "\n");
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/Market/Views/ProductPage.xaml", UriKind.Relative));
            this.ListBoxItemNavDotsStyle = (Style)this.FindName("ListBoxItemNavDotsStyle");
            this.rowDefinitionContent = (RowDefinition)this.FindName("rowDefinitionContent");
            this.ucHeader = (GenericHeaderUC)this.FindName("ucHeader");
            this.ucMoreActions = (MoreActionsUC)this.FindName("ucMoreActions");
            this.viewportControl = (ViewportControl)this.FindName("viewportControl");
            this.stackPanelContent = (StackPanel)this.FindName("stackPanelContent");
            this.canvasBackground = (Canvas)this.FindName("canvasBackground");
            this.slideView = (SlideView)this.FindName("slideView");
            this.listBoxNavDots = (ListBox)this.FindName("listBoxNavDots");
            this.textBlockMetaData = (TextBlock)this.FindName("textBlockMetaData");
            this.gridAction = (Grid)this.FindName("gridAction");
            this.ucCommentGeneric = (CommentsGenericUC)this.FindName("ucCommentGeneric");
            this.ucNewMessage = (NewMessageUC)this.FindName("ucNewMessage");
            this.ucFloatActions = (FloatActionsUC)this.FindName("ucFloatActions");
        }
    }
}
