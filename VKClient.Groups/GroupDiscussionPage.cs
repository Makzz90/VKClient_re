using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
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
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Groups.Library;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Groups
{
  public class GroupDiscussionPage : PageBase, IHandle<SpriteElementTapEvent>, IHandle, IHandle<StickerItemTapEvent>
  {
    private bool _isInitialized;
    private PhotoChooserTask _photoChooserTask;
    private WallPostViewModel _commentVM;
    private ApplicationBarIconButton _appBarButtonAddComment;
    private ApplicationBarIconButton _appBarButtonEmojiToggle;
    private ApplicationBarIconButton _appBarButtonAttachments;
    private ApplicationBarIconButton _appBarButtonRefresh;
    private ApplicationBar _defaultAppBar;
    private ViewportScrollableAreaAdapter _adapter;
    private bool _isAddingComment;
    internal Grid LayoutRoot;
    internal ViewportControl scroll;
    internal StackPanel scrollablePanel;
    internal MyVirtualizingPanel2 panel;
    internal NewMessageUC newCommentUC;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    private GroupDiscussionViewModel GroupDiscussionVM
    {
      get
      {
        return base.DataContext as GroupDiscussionViewModel;
      }
    }

    public bool ReadyToSend
    {
      get
      {
        string text = this.newCommentUC.TextBoxNewComment.Text;
        ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
        if (!string.IsNullOrWhiteSpace(text) && outboundAttachments.Count == 0)
          return true;
        if (outboundAttachments.Count > 0)
          return outboundAttachments.All<IOutboundAttachment>((Func<IOutboundAttachment, bool>) (a => a.UploadState == OutboundAttachmentUploadState.Completed));
        return false;
      }
    }

    public GroupDiscussionPage()
    {
      PhotoChooserTask photoChooserTask = new PhotoChooserTask();
      int num = 1;
      photoChooserTask.ShowCamera=(num != 0);
      this._photoChooserTask = photoChooserTask;
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("Resources/appbar.send.text.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri=(uri1);
      string commentsPageAppBarSend = CommonResources.PostCommentsPage_AppBar_Send;
      applicationBarIconButton1.Text=(commentsPageAppBarSend);
      this._appBarButtonAddComment = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("Resources/appbar.smile.png", UriKind.Relative);
      applicationBarIconButton2.IconUri=(uri2);
      string str = "emoji";
      applicationBarIconButton2.Text=(str);
      this._appBarButtonEmojiToggle = applicationBarIconButton2;
      ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
      Uri uri3 = new Uri("Resources/attach.png", UriKind.Relative);
      applicationBarIconButton3.IconUri=(uri3);
      string barAddAttachment = CommonResources.NewPost_AppBar_AddAttachment;
      applicationBarIconButton3.Text=(barAddAttachment);
      this._appBarButtonAttachments = applicationBarIconButton3;
      ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
      Uri uri4 = new Uri("Resources/appbar.refresh.rest.png", UriKind.Relative);
      applicationBarIconButton4.IconUri=(uri4);
      string appBarRefresh = CommonResources.AppBar_Refresh;
      applicationBarIconButton4.Text=(appBarRefresh);
      this._appBarButtonRefresh = applicationBarIconButton4;
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor=(appBarBgColor);
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor=(appBarFgColor);
      this._defaultAppBar = applicationBar;
      // ISSUE: explicit constructor call
  //    base.\u002Ector();
      this.InitializeComponent();
      this._adapter = new ViewportScrollableAreaAdapter(this.scroll);
      this.panel.InitializeWithScrollViewer((IScrollableArea) this._adapter, false);
      this.newCommentUC.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.PanelIsOpenedChanged);
      this.scroll.BindViewportBoundsTo((FrameworkElement) this.scrollablePanel);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panel);
      this.BuildAppBar();
      // ISSUE: method pointer
      ((UIElement) this.newCommentUC.TextBoxNewComment).GotFocus+=(new RoutedEventHandler(this.textBoxGotFocus));
      // ISSUE: method pointer
      ((UIElement) this.newCommentUC.TextBoxNewComment).LostFocus+=(new RoutedEventHandler(this.textBoxLostFocus));
      // ISSUE: method pointer
      this.newCommentUC.TextBoxNewComment.TextChanged+=(new TextChangedEventHandler(this.TextBoxNewComment_TextChanged));
      this.panel.ScrollPositionChanged += new EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>(this.panel_ScrollPositionChanged);
      this.panel.ManuallyLoadMore = true;
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler(this.GroupDiscussionPage_Loaded));
      ((ChooserBase<PhotoResult>) this._photoChooserTask).Completed+=(new EventHandler<PhotoResult>(this._photoChooserTask_Completed));
      EventAggregator.Current.Subscribe((object) this);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.panel);
      this.panel.OnRefresh = (Action) (() => this.GroupDiscussionVM.LoadData(true, (Action<bool>) null));
      this.newCommentUC.UCNewPost.TextBlockWatermarkText.Text=(CommonResources.Comment);
      this.newCommentUC.OnAddAttachTap = (Action) (() => this.AddAttachTap());
      this.newCommentUC.OnSendTap = (Action) (() => this._appBarButtonAddComment_Click((object) null, (EventArgs) null));
      this.newCommentUC.UCNewPost.OnImageDeleteTap = (Action<object>) (sender =>
      {
        FrameworkElement frameworkElement = sender as FrameworkElement;
        if (frameworkElement != null)
          this._commentVM.OutboundAttachments.Remove(frameworkElement.DataContext as IOutboundAttachment);
        this.UpdateAppBar();
      });
      Binding binding = new Binding("OutboundAttachments");
      ((FrameworkElement) this.newCommentUC.UCNewPost.ItemsControlAttachments).SetBinding((DependencyProperty) ItemsControl.ItemsSourceProperty, binding);
    }

    private void PanelIsOpenedChanged(object sender, bool e)
    {
      if (this.newCommentUC.PanelControl.IsOpen || this.newCommentUC.PanelControl.IsTextBoxTargetFocused)
        this.panel.ScrollTo(this._adapter.VerticalOffset + this.newCommentUC.PanelControl.PortraitOrientationHeight);
      else
        this.panel.ScrollTo(this._adapter.VerticalOffset - this.newCommentUC.PanelControl.PortraitOrientationHeight);
    }

    private void AddAttachTap()
    {
      AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action) (() =>
      {
        PostCommentsPage.HandleInputParams(this._commentVM);
        this.UpdateAppBar();
      }), true, 0, 0, (ConversationInfo) null);
    }

    private void TextBoxNewComment_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void _photoChooserTask_Completed(object sender, PhotoResult e)
    {
        if (((TaskEventArgs)e).TaskResult != TaskResult.OK)
        return;
      ParametersRepository.SetParameterForId("ChoosenPhoto", (object) e.ChosenPhoto);
    }

    private void GroupDiscussionPage_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void panel_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
    {
      if (this.GroupDiscussionVM.LoadFromEnd)
      {
        if (e.ScrollHeight == 0.0 || e.CurrentPosition >= 100.0)
          return;
        this.GroupDiscussionVM.LoadData(false, (Action<bool>) null);
      }
      else
      {
        if (e.ScrollHeight == 0.0 || e.ScrollHeight - e.CurrentPosition >= VKConstants.LoadMoreNewsThreshold)
          return;
        this.GroupDiscussionVM.LoadData(false, (Action<bool>) null);
      }
    }

    private void BuildAppBar()
    {
      this._appBarButtonEmojiToggle.Click+=(new EventHandler(this._appBarButtonEmojiToggle_Click));
      this._appBarButtonAddComment.Click+=(new EventHandler(this._appBarButtonAddComment_Click));
      this._appBarButtonAttachments.Click+=(new EventHandler(this._appBarButtonAttachments_Click));
      this._appBarButtonRefresh.Click+=(new EventHandler(this._appBarButtonRefresh_Click));
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonRefresh);
      this._defaultAppBar.Opacity=(0.9);
      this._defaultAppBar.StateChanged+=(new EventHandler<ApplicationBarStateChangedEventArgs>(this._defaultAppBar_StateChanged));
    }

    private void _appBarButtonEmojiToggle_Click(object sender, EventArgs e)
    {
    }

    private void _defaultAppBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
    {
    }

    private void _appBarButtonAttachments_Click(object sender, EventArgs e)
    {
    }

    private void _appBarButtonRefresh_Click(object sender, EventArgs e)
    {
      this.GroupDiscussionVM.LoadData(true, (Action<bool>) null);
    }

    private void _appBarButtonAddComment_Click(object sender, EventArgs e)
    {
      if (this._isAddingComment)
        return;
      string text = this.newCommentUC.TextBoxNewComment.Text;
      if (text.Length < 2 && this._commentVM.OutboundAttachments.Count <= 0)
        return;
      this._isAddingComment = true;
      this.GroupDiscussionVM.AddComment(text.Replace("\r\n", "\r").Replace("\r", "\r\n"), this._commentVM.OutboundAttachments.ToList<IOutboundAttachment>(), (Action<bool>) (res =>
      {
        this._isAddingComment = false;
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (res)
          {
            this.newCommentUC.TextBoxNewComment.Text=(string.Empty);
            this.InitializeCommentVM();
            this.UpdateAppBar();
          }
          else
            ExtendedMessageBox.ShowSafe(CommonResources.Error);
        }));
      }), (StickerItemData) null, this.newCommentUC.FromGroupChecked, "");
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      bool flag1 = true;
      if (!this._isInitialized)
      {
        long gid = long.Parse(((Page) this).NavigationContext.QueryString["GroupId"]);
        long num1 = long.Parse(((Page) this).NavigationContext.QueryString["TopicId"]);
        string str = ((Page) this).NavigationContext.QueryString["TopicName"];
        int num2 = int.Parse(((Page) this).NavigationContext.QueryString["KnownCommentsCount"]);
        bool flag2 = ((Page) this).NavigationContext.QueryString["LoadFromTheEnd"] == bool.TrueString;
        bool flag3 = ((Page) this).NavigationContext.QueryString["CanComment"] == bool.TrueString;
        long tid = num1;
        string topicName = str;
        int knownCommentsCount = num2;
        int num3 = flag3 ? 1 : 0;
        MyVirtualizingPanel2 panel = this.panel;
        int num4 = flag2 ? 1 : 0;
        Action<CommentItem> replyCallback = new Action<CommentItem>(this.replyCallback);
        GroupDiscussionViewModel discussionViewModel = new GroupDiscussionViewModel(gid, tid, topicName, knownCommentsCount, num3 != 0, panel, num4 != 0, replyCallback);
        this.InitializeCommentVM();
        base.DataContext=((object) discussionViewModel);
        discussionViewModel.LoadData(false, new Action<bool>(this.LoadedCallback));
        this.UpdateAppBar();
        this.RestoreUnboundState();
        this._isInitialized = true;
        flag1 = false;
      }
      if (!flag1 && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New))
        WallPostVMCacheManager.TryDeserializeInstance(this._commentVM);
      this.ProcessInputData();
      this.UpdateAppBar();
    }

    private void LoadedCallback(bool success)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        Group cachedGroup = GroupsService.Current.GetCachedGroup(this.GroupDiscussionVM.GroupId);
        if (cachedGroup == null)
          return;
        this.newCommentUC.SetAdminLevel(cachedGroup.admin_level);
      }));
    }

    private void replyCallback(CommentItem obj)
    {
      TextBox textBoxNewComment = this.newCommentUC.TextBoxNewComment;
      string str = textBoxNewComment.Text + string.Format("[post{0}|{1}], ", (object) obj.Comment.cid, (object) obj.NameWithoutLastName);
      textBoxNewComment.Text=(str);
      ((Control) this.newCommentUC.TextBoxNewComment).Focus();
      this.newCommentUC.TextBoxNewComment.Select(this.newCommentUC.TextBoxNewComment.Text.Length, 0);
    }

    private void ProcessInputData()
    {
      Photo parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
      if (parameterForIdAndReset1 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) OutboundPhotoAttachment.CreateForChoosingExistingPhoto(parameterForIdAndReset1, 0, false, PostType.WallPost));
      VKClient.Common.Backend.DataObjects.Video parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
      if (parameterForIdAndReset2 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundVideoAttachment(parameterForIdAndReset2));
      AudioObj parameterForIdAndReset3 = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
      if (parameterForIdAndReset3 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundAudioAttachment(parameterForIdAndReset3));
      Doc parameterForIdAndReset4 = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
      if (parameterForIdAndReset4 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundDocumentAttachment(parameterForIdAndReset4));
      List<Stream> parameterForIdAndReset5 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
      List<Stream> parameterForIdAndReset6 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
      if (parameterForIdAndReset5 != null)
      {
        for (int index = 0; index < parameterForIdAndReset5.Count; ++index)
        {
          Stream stream1 = parameterForIdAndReset5[index];
          Stream stream2 = parameterForIdAndReset6[index];
          long userOrGroupId = 0;
          int num1 = 0;
          Stream previewStream = stream2;
          int num2 = 0;
          this._commentVM.AddAttachment((IOutboundAttachment) OutboundPhotoAttachment.CreateForUploadNewPhoto(stream1, userOrGroupId, num1 != 0, previewStream, (PostType) num2));
        }
        this.GroupDiscussionVM.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
        this._commentVM.UploadAttachments();
      }
      FileOpenPickerContinuationEventArgs parameterForIdAndReset7 = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
      if ((parameterForIdAndReset7 == null || !parameterForIdAndReset7.Files.Any<StorageFile>()) && !ParametersRepository.Contains("PickedPhotoDocuments"))
        return;
      object parameterForIdAndReset8 = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
      IReadOnlyList<StorageFile> storageFileList = parameterForIdAndReset7 != null ? parameterForIdAndReset7.Files : (IReadOnlyList<StorageFile>) ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocuments");
      AttachmentType result;
      if (parameterForIdAndReset8 == null || !Enum.TryParse<AttachmentType>(parameterForIdAndReset8.ToString(), out result))
        return;
      foreach (StorageFile file in (IEnumerable<StorageFile>) storageFileList)
      {
        if (result != AttachmentType.VideoFromPhone)
        {
          if (result == AttachmentType.DocumentFromPhone || result == AttachmentType.DocumentPhoto)
          {
            this._commentVM.AddAttachment((IOutboundAttachment) new OutboundUploadDocumentAttachment(file));
            this._commentVM.UploadAttachments();
          }
        }
        else
        {
          this._commentVM.AddAttachment((IOutboundAttachment) new OutboundUploadVideoAttachment(file, true, 0L));
          this._commentVM.UploadAttachments();
        }
      }
    }

    private void InitializeCommentVM()
    {
      this._commentVM = WallPostViewModel.CreateNewDiscussionCommentVM();
      this._commentVM.PropertyChanged += new PropertyChangedEventHandler(this._commentVM_PropertyChanged);
      ((FrameworkElement) this.newCommentUC).DataContext=((object) this._commentVM);
    }

    private void _commentVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender != this._commentVM || !(e.PropertyName == "CanPublish"))
        return;
      this.UpdateAppBar();
      ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
      Func<IOutboundAttachment, bool> func = (Func<IOutboundAttachment, bool>) (a => a.UploadState == OutboundAttachmentUploadState.Uploading);
      if (outboundAttachments.Any<IOutboundAttachment>(func))
        return;
      this.GroupDiscussionVM.SetInProgress(false, "");
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
      this.State["CommentText"] = (object) this.newCommentUC.TextBoxNewComment.Text;
    }

    private void RestoreUnboundState()
    {
      if (!this.State.ContainsKey("CommentText"))
        return;
      this.newCommentUC.TextBoxNewComment.Text=(this.State["CommentText"].ToString());
    }

    private void UpdateAppBar()
    {
      if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
        return;
      if (this.GroupDiscussionVM.CanComment && !this._defaultAppBar.Buttons.Contains((object) this._appBarButtonAddComment))
      {
        this._defaultAppBar.Buttons.Insert(0, (object) this._appBarButtonAddComment);
        if (!this._defaultAppBar.Buttons.Contains((object) this._appBarButtonEmojiToggle))
          this._defaultAppBar.Buttons.Insert(1, (object) this._appBarButtonEmojiToggle);
      }
      if (this.GroupDiscussionVM.CanComment && !this._defaultAppBar.Buttons.Contains((object) this._appBarButtonAttachments))
        this._defaultAppBar.Buttons.Insert(2, (object) this._appBarButtonAttachments);
      this._appBarButtonAddComment.IsEnabled=(this.ReadyToSend);
      this.newCommentUC.UpdateSendButton(this.ReadyToSend && this.GroupDiscussionVM.CanComment);
      int count = this._commentVM.OutboundAttachments.Count;
      if (count > 0)
        this._appBarButtonAttachments.IconUri=(new Uri(string.Format("Resources/appbar.attachments-{0}.rest.png", (object) Math.Min(count, 10)), UriKind.Relative));
      else
        this._appBarButtonAttachments.IconUri=(new Uri("Resources/attach.png", UriKind.Relative));
    }

    private void textBoxGotFocus(object sender, RoutedEventArgs e)
    {
      this.GroupDiscussionVM.EnsureLoadFromEnd();
    }

    private void textBoxLostFocus(object sender, RoutedEventArgs e)
    {
    }

    public void Handle(StickerItemTapEvent message)
    {
      if (!this._isCurrentPage || this._isAddingComment)
        return;
      this._isAddingComment = true;
      this.GroupDiscussionVM.AddComment("", this._commentVM.OutboundAttachments.ToList<IOutboundAttachment>(), (Action<bool>) (res =>
      {
        this._isAddingComment = false;
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (res)
            return;
          ExtendedMessageBox.ShowSafe(CommonResources.Error);
        }));
      }), message.StickerItem, this.newCommentUC.FromGroupChecked, message.Referrer);
    }

    public void Handle(SpriteElementTapEvent data)
    {
      if (!this._isCurrentPage)
        return;
      base.Dispatcher.BeginInvoke((Action) (() =>
      {
        TextBox textBoxNewComment = this.newCommentUC.TextBoxNewComment;
        int selectionStart = textBoxNewComment.SelectionStart;
        string str = textBoxNewComment.Text.Insert(selectionStart, data.Data.ElementCode);
        textBoxNewComment.Text=(str);
        int num1 = selectionStart + data.Data.ElementCode.Length;
        int num2 = 0;
        textBoxNewComment.Select(num1, num2);
      }));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Groups;component/GroupDiscussionPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.scroll = (ViewportControl) base.FindName("scroll");
      this.scrollablePanel = (StackPanel) base.FindName("scrollablePanel");
      this.panel = (MyVirtualizingPanel2) base.FindName("panel");
      this.newCommentUC = (NewMessageUC) base.FindName("newCommentUC");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
    }
  }
}
