using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Common
{
  public class NewPost : PageBase
  {
    private bool _isInitialized;
    private PhotoChooserTask _photoChooserTask;
    private AttachmentPickerUC _pickerUC;
    private Point _textBoxTapPoint;
    private ApplicationBarIconButton _appBarButtonSend;
    private ApplicationBarIconButton _appBarButtonAttachImage;
    private ApplicationBarIconButton _appBarButtonAttachLocation;
    private ApplicationBarIconButton _appBarButtonAddAttachment;
    private bool _excludeLocation;
    private bool _isPublishing;
    private bool _published;
    private bool _fromPhotoPicker;
    private bool _isForwardNav;
    private bool _isFromWallPostPage;
    private int _adminLevel;
    private IShareContentDataProvider _shareContentDataProvider;
    private DelayedExecutor _de;
    //private double savedHeight;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal Grid ContentPanel;
    internal ScrollViewer scroll;
    internal StackPanel stackPanel;
    internal TextBox textBoxTopicTitle;
    internal TextBlock textBlockWatermarkTitle;
    internal NewPostUC ucNewPost;
    internal Border wallRepostContainer;
    internal CheckBox checkBoxFriendsOnly;
    internal TextBoxPanelControl textBoxPanel;
    private bool _contentLoaded;

    protected WallPostViewModel WallPostVM
    {
      get
      {
        return base.DataContext as WallPostViewModel;
      }
    }

    private TextBox textBoxPost
    {
      get
      {
        return this.ucNewPost.TextBoxPost;
      }
    }

    private TextBlock textBlockWatermarkText
    {
      get
      {
        return this.ucNewPost.TextBlockWatermarkText;
      }
    }

    public NewPost()
    {
      PhotoChooserTask photoChooserTask = new PhotoChooserTask();
      int num = 1;
      photoChooserTask.ShowCamera = (num != 0);
      this._photoChooserTask = photoChooserTask;
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("Resources/appbar.send.text.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string newPostSend = CommonResources.NewPost_Send;
      applicationBarIconButton1.Text = newPostSend;
      this._appBarButtonSend = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("Resources/appbar.feature.camera.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string postAppBarAddPhoto = CommonResources.NewPost_AppBar_AddPhoto;
      applicationBarIconButton2.Text = postAppBarAddPhoto;
      this._appBarButtonAttachImage = applicationBarIconButton2;
      ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
      Uri uri3 = new Uri("Resources/appbar.checkin.rest.png", UriKind.Relative);
      applicationBarIconButton3.IconUri = uri3;
      string appBarAddLocation = CommonResources.NewPost_AppBar_AddLocation;
      applicationBarIconButton3.Text = appBarAddLocation;
      this._appBarButtonAttachLocation = applicationBarIconButton3;
      ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
      Uri uri4 = new Uri("Resources/attach.png", UriKind.Relative);
      applicationBarIconButton4.IconUri = uri4;
      string barAddAttachment = CommonResources.NewPost_AppBar_AddAttachment;
      applicationBarIconButton4.Text = barAddAttachment;
      this._appBarButtonAddAttachment = applicationBarIconButton4;
      this._de = new DelayedExecutor(100);
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      ((ChooserBase<PhotoResult>) this._photoChooserTask).Completed += (new EventHandler<PhotoResult>(this._photoChooserTask_Completed));
      this.BuildAppBar();
      // ISSUE: method pointer
      this.ucNewPost.textBoxPost.TextChanged += (new TextChangedEventHandler( this.textBoxPost_TextChanged_1));
      // ISSUE: method pointer
      ((UIElement) this.ucNewPost.textBoxPost).GotFocus += (new RoutedEventHandler( this.TextBox_OnGotFocus));
      // ISSUE: method pointer
      ((UIElement) this.ucNewPost.textBoxPost).LostFocus += (new RoutedEventHandler( this.TextBox_OnLostFocus));
      ((UIElement) this.ucNewPost.textBoxPost).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.TextBoxPost_OnTap));
      this.ucNewPost.OnImageDeleteTap = (Action<object>) (sender => this.Image_Delete_Tap(sender,  null));
      this.ucNewPost.OnAddAttachmentTap = (Action) (() => this.AddAttachmentTap(null,  null));
    }

    private void TextBoxPost_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this._textBoxTapPoint = e.GetPosition((UIElement) this.stackPanel);
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.textBoxPanel.IsOpen = true;
      FrameworkElement frameworkElement = (FrameworkElement) sender;
      if (frameworkElement.Name == ((FrameworkElement) this.textBoxTopicTitle).Name)
      {
        this.scroll.ScrollToVerticalOffset(0.0);
        base.UpdateLayout();
        Point relativePosition = ((UIElement) frameworkElement).GetRelativePosition((UIElement) this.stackPanel);
        // ISSUE: explicit reference operation
        this.scroll.ScrollToOffsetWithAnimation(((Point) @relativePosition).Y, 0.2, false);
      }
      else
      {
        this.scroll.ScrollToVerticalOffset(0.0);
        base.UpdateLayout();
        // ISSUE: explicit reference operation
        this.scroll.ScrollToOffsetWithAnimation(this._textBoxTapPoint.Y - 40.0, 0.2, false);
      }
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.textBoxPanel.IsOpen = false;
    }

    private void _photoChooserTask_Completed(object sender, PhotoResult e)
    {
      Logger.Instance.Info("Back from photo chooser");
      if (((TaskEventArgs)e).TaskResult != TaskResult.OK)
        return;
      ParametersRepository.SetParameterForId("ChoosenPhoto", e.ChosenPhoto);
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar1 = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar1.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar1.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar1.Opacity = num;
      ApplicationBar applicationBar2 = applicationBar1;
      applicationBar2.Buttons.Add(this._appBarButtonSend);
      this._appBarButtonSend.Click+=(new EventHandler(this._appBarButtonSend_Click));
      this.ApplicationBar = ((IApplicationBar) applicationBar2);
    }

    private void _appBarButtonAddAttachment_Click(object sender, EventArgs e)
    {
      ((Control) this).Focus();
      this._pickerUC = AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, 10, (Action) (() => this.HandleInputParams( null)), this._excludeLocation, this.CommonParameters.IsGroup ? -this.CommonParameters.UserOrGroupId : this.CommonParameters.UserOrGroupId, 0,  null);
    }

    private void _appBarButtonAttachLocation_Click(object sender, EventArgs e)
    {
      this.WallPostVM.GoDirectlyToPhotoChooser = false;
      Navigator.Current.NavigateToMap(true, 0.0, 0.0);
    }

    private void _appBarButtonAttachImage_Click(object sender, EventArgs e)
    {
      this.ShowPhotoChooser(false);
    }

    private void ShowPhotoChooser(bool goDirectly = false)
    {
      this.WallPostVM.GoDirectlyToPhotoChooser = goDirectly;
      Navigator.Current.NavigateToPhotoPickerPhotos(this.WallPostVM.NumberOfAttAllowedToAdd, false, false);
    }

    private void _appBarButtonSend_Click(object sender, EventArgs e)
		{
			this._isPublishing = true;
			this.UpdateViewState();
			this.WallPostVM.Publish(delegate(ResultCode res)
			{
				Execute.ExecuteOnUIThread(delegate
				{
					this._isPublishing = false;
					if (res == ResultCode.Succeeded)
					{
						this._published = true;
						WallPostVMCacheManager.ResetVM(this.WallPostVM);
						if (this._shareContentDataProvider is ShareExternalContentDataProvider)
						{
							((ShareExternalContentDataProvider)this._shareContentDataProvider).ShareOperation.ReportCompleted();
							return;
						}
						if (this.WallPostVM.WMMode == WallPostViewModel.Mode.PublishWallPost && this._isFromWallPostPage && Enumerable.Any<JournalEntry>(this.NavigationService.BackStack))
						{
							this.NavigationService.RemoveBackEntrySafe();
						}
						Navigator.Current.GoBack();
						return;
					}
					else
					{
						if (res != ResultCode.PostsLimitOrAlreadyScheduled)
						{
							this.UpdateViewState();
							new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
							return;
						}
						IEnumerable<IOutboundAttachment> arg_EF_0 = this.WallPostVM.OutboundAttachments;
						Func<IOutboundAttachment, bool> arg_EF_1= new Func<IOutboundAttachment, bool>((a)=>{return a.AttachmentId == "timestamp";});
						
						if (Enumerable.FirstOrDefault<IOutboundAttachment>(arg_EF_0, arg_EF_1) != null)
						{
							this.UpdateViewState();
							new GenericInfoUC(2000).ShowAndHideLater(CommonResources.ScheduledForExistingTime, null);
							return;
						}
						this.UpdateViewState();
						new GenericInfoUC(2000).ShowAndHideLater(CommonResources.PostsLimitReached, null);
						return;
					}
				});
			});
		}

    private void UpdateAppBar()
		{
			this._appBarButtonSend.IsEnabled=(this.WallPostVM.CanPublish && !this._isPublishing);
			this._appBarButtonAttachImage.IsEnabled=(!this._isPublishing);
			bool arg_80_1;
			if (!this._isPublishing)
			{
				IEnumerable<IOutboundAttachment> arg_6B_0 = this.WallPostVM.OutboundAttachments;
                Func<IOutboundAttachment, bool> arg_6B_1 = new Func<IOutboundAttachment, bool>((a) => { return a.IsGeo; });
				
				if (!Enumerable.Any<IOutboundAttachment>(arg_6B_0, arg_6B_1))
				{
					arg_80_1 = this.WallPostVM.EditWallRepost;
					goto IL_80;
				}
			}
			arg_80_1 = true;
			IL_80:
			this._excludeLocation = arg_80_1;
			this._appBarButtonAddAttachment.IsEnabled=(!this._isPublishing);
			switch (this.WallPostVM.WMMode)
			{
			case WallPostViewModel.Mode.NewWallComment:
			case WallPostViewModel.Mode.NewPhotoComment:
			case WallPostViewModel.Mode.NewVideoComment:
			case WallPostViewModel.Mode.NewDiscussionComment:
			case WallPostViewModel.Mode.NewProductComment:
				this._excludeLocation = true;
				base.ApplicationBar.Buttons.Remove(this._appBarButtonSend);
				break;
			case WallPostViewModel.Mode.EditWallComment:
			case WallPostViewModel.Mode.EditPhotoComment:
			case WallPostViewModel.Mode.EditVideoComment:
			case WallPostViewModel.Mode.EditDiscussionComment:
			case WallPostViewModel.Mode.NewTopic:
			case WallPostViewModel.Mode.EditProductComment:
				this._excludeLocation = true;
				break;
			}
			this._appBarButtonAttachImage.IsEnabled=(this.WallPostVM.CanAddMoreAttachments);
			this._appBarButtonAddAttachment.IsEnabled=(this.WallPostVM.CanAddMoreAttachments);
		}

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      this._fromPhotoPicker = false;
      this._isForwardNav = e.NavigationMode == 0;
      if (ParametersRepository.Contains("FromPhotoPicker"))
        this._fromPhotoPicker = (bool) ParametersRepository.GetParameterForIdAndReset("FromPhotoPicker");
      if (!this._isInitialized)
      {
        this._adminLevel = int.Parse(((Page) this).NavigationContext.QueryString["AdminLevel"]);
        bool isPublicPage = ((Page) this).NavigationContext.QueryString["IsPublicPage"] == bool.TrueString;
        bool isNewTopicMode = ((Page) this).NavigationContext.QueryString["IsNewTopicMode"] == bool.TrueString;
        this._isFromWallPostPage = ((Page) this).NavigationContext.QueryString["FromWallPostPage"] == bool.TrueString;
        WallPostViewModel.Mode mode;
        // ISSUE: explicit reference operation
        // ISSUE: cast to a reference type
        Enum.TryParse<WallPostViewModel.Mode>(((Page) this).NavigationContext.QueryString["Mode"], out mode);
        WallPost parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("PublishWallPost") as WallPost;
        WallPost parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("EditWallPost") as WallPost;
        Comment parameterForIdAndReset3 = ParametersRepository.GetParameterForIdAndReset("EditWallComment") as Comment;
        Comment parameterForIdAndReset4 = ParametersRepository.GetParameterForIdAndReset("EditPhotoComment") as Comment;
        Comment parameterForIdAndReset5 = ParametersRepository.GetParameterForIdAndReset("EditVideoComment") as Comment;
        Comment parameterForIdAndReset6 = ParametersRepository.GetParameterForIdAndReset("EditProductComment") as Comment;
        Comment parameterForIdAndReset7 = ParametersRepository.GetParameterForIdAndReset("EditDiscussionComment") as Comment;
        Dictionary<long, long> parameterForIdAndReset8 = ParametersRepository.GetParameterForIdAndReset("CidToAuthorIdDict") as Dictionary<long, long>;
        WallRepostInfo parameterForIdAndReset9 = ParametersRepository.GetParameterForIdAndReset("WallRepostInfo") as WallRepostInfo;
        WallPostViewModel parameterForIdAndReset10 = ParametersRepository.GetParameterForIdAndReset("NewCommentVM") as WallPostViewModel;
        this._shareContentDataProvider = ShareContentDataProviderManager.RetrieveDataProvider();
        if (this._shareContentDataProvider is ShareExternalContentDataProvider)
        {
          ((Page) this).NavigationService.ClearBackStack();
          this.ucHeader.HideSandwitchButton = true;
          this.SuppressMenu = true;
        }
        WallPostViewModel vm;
        if (parameterForIdAndReset1 != null)
          vm = new WallPostViewModel(parameterForIdAndReset1, this._adminLevel,  null)
          {
            WMMode = WallPostViewModel.Mode.PublishWallPost
          };
        else if (parameterForIdAndReset2 != null)
        {
          vm = new WallPostViewModel(parameterForIdAndReset2, this._adminLevel, parameterForIdAndReset9)
          {
            WMMode = WallPostViewModel.Mode.EditWallPost
          };
          if (vm.WallRepostInfo != null)
          {
            RepostHeaderUC repostHeaderUc1 = new RepostHeaderUC();
            Thickness thickness = new Thickness(0.0, 14.0, 0.0, 14.0);
            ((FrameworkElement) repostHeaderUc1).Margin = thickness;
            RepostHeaderUC repostHeaderUc2 = repostHeaderUc1;
            repostHeaderUc2.Configure(vm.WallRepostInfo,  null);
            this.wallRepostContainer.Child = ((UIElement) repostHeaderUc2);
          }
        }
        else
          vm = parameterForIdAndReset3 == null ? (parameterForIdAndReset4 == null ? (parameterForIdAndReset5 == null ? (parameterForIdAndReset6 == null ? (parameterForIdAndReset7 == null ? (parameterForIdAndReset10 == null ? new WallPostViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, this._adminLevel, isPublicPage, isNewTopicMode) : parameterForIdAndReset10) : WallPostViewModel.CreateEditDiscussionCommentVM(parameterForIdAndReset7, parameterForIdAndReset8)) : WallPostViewModel.CreateEditProductCommentVM(parameterForIdAndReset6)) : WallPostViewModel.CreateEditVideoCommentVM(parameterForIdAndReset5)) : WallPostViewModel.CreateEditPhotoCommentVM(parameterForIdAndReset4)) : WallPostViewModel.CreateEditWallCommentVM(parameterForIdAndReset3);
        vm.IsOnPostPage = true;
        vm.WMMode = mode;
        if (!this._fromPhotoPicker && (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New || (mode == WallPostViewModel.Mode.NewTopic || mode == WallPostViewModel.Mode.NewWallPost)))
          WallPostVMCacheManager.TryDeserializeVM(vm);
        vm.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
        base.DataContext = vm;
        this._isInitialized = true;
      }
      if (this.HandleInputParams(e))
        return;
      this.UpdateViewState();
      ListExtensions.ForEach<IOutboundAttachment>(this.WallPostVM.OutboundAttachments, (Action<IOutboundAttachment>)(a => a.SetRetryFlag()));
      if (!e.IsNavigationInitiator || e.NavigationMode != NavigationMode.New)
        return;
      if (this.WallPostVM.IsInNewWallPostMode || this.WallPostVM.EditWallRepost)
      {
        this.FocusTextBox();
      }
      else
      {
        if (!this.WallPostVM.IsInNewTopicMode)
          return;
        this.FocusTitleTextBox();
      }
    }

    private void FocusTextBox()
    {
      this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
      {
        ((Control) this.textBoxPost).Focus();
        this.textBoxPost.Select(this.textBoxPost.Text.Length, 0);
      }))));
    }

    private void FocusTitleTextBox()
    {
      this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() => ((Control) this.textBoxTopicTitle).Focus()))));
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      if (this._published)
        return;
      WallPostVMCacheManager.TrySerializeVM(this.WallPostVM);
    }

    private void UpdateViewState()
    {
      if (this.WallPostVM.IsNewCommentMode)
        ((UIElement) this.textBoxPost).Visibility = Visibility.Collapsed;
      this.textBoxPost.Text = (this.WallPostVM.Text ?? "");
      this.textBoxTopicTitle.Text = (this.WallPostVM.TopicTitle ?? "");
      ((UIElement) this.textBlockWatermarkText).Opacity = (this.textBoxPost.Text == "" ? 1.0 : 0.0);
      ((UIElement) this.textBlockWatermarkTitle).Opacity = (this.textBoxTopicTitle.Text == "" ? 1.0 : 0.0);
      this.UpdateAppBar();
    }

    private bool HandleInputParams(NavigationEventArgs e = null)
		{
			if (ParametersRepository.GetParameterForIdAndReset("GoPickImage") != null)
			{
				this.ShowPhotoChooser(true);
				return true;
			}
			string text = ParametersRepository.GetParameterForIdAndReset("NewMessageContents") as string;
			if (!string.IsNullOrEmpty(text))
			{
				this.WallPostVM.Text = text;
			}
			GeoCoordinate geoCoordinate = ParametersRepository.GetParameterForIdAndReset("NewPositionToBeAttached") as GeoCoordinate;
			if (geoCoordinate != null)
			{
				OutboundGeoAttachment attachment = new OutboundGeoAttachment(geoCoordinate.Latitude, geoCoordinate.Longitude);
				this.WallPostVM.AddAttachment(attachment);
			}
			Poll poll = ParametersRepository.GetParameterForIdAndReset("UpdatedPoll") as Poll;
			if (poll != null)
			{
				IEnumerable<IOutboundAttachment> arg_B2_0 = this.WallPostVM.Attachments;
				Func<IOutboundAttachment, bool> arg_B2_1 = new Func<IOutboundAttachment, bool>((a)=>{return a is OutboundPollAttachment;});
				
				OutboundPollAttachment outboundPollAttachment = Enumerable.FirstOrDefault<IOutboundAttachment>(arg_B2_0, arg_B2_1) as OutboundPollAttachment;
				if (outboundPollAttachment != null)
				{
					outboundPollAttachment.Poll = poll;
				}
				else
				{
					OutboundPollAttachment attachment2 = new OutboundPollAttachment(poll);
					this.WallPostVM.AddAttachment(attachment2);
				}
			}
			Stream stream = ParametersRepository.GetParameterForIdAndReset("ChoosenPhoto") as Stream;
			List<Stream> list = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
			List<Stream> list2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
			if (stream != null)
			{
				if (!this._fromPhotoPicker || this._isForwardNav)
				{
					OutboundPhotoAttachment attachment3 = OutboundPhotoAttachment.CreateForUploadNewPhoto(stream, this.WallPostVM.UserOrGroupId, this.WallPostVM.IsGroup, null, PostType.WallPost);
					this.WallPostVM.AddAttachment(attachment3);
					this.WallPostVM.UploadAttachments();
				}
			}
			else if (list != null)
			{
				if (!this._fromPhotoPicker || this._isForwardNav)
				{
					for (int i = 0; i < list.Count; i++)
					{
						Stream arg_1C4_0 = list[i];
						Stream previewStream = null;
						if (list2 != null && list2.Count > i)
						{
							previewStream = list2[i];
						}
						OutboundPhotoAttachment attachment4 = OutboundPhotoAttachment.CreateForUploadNewPhoto(arg_1C4_0, this.WallPostVM.UserOrGroupId, this.WallPostVM.IsGroup, previewStream, PostType.WallPost);
						this.WallPostVM.AddAttachment(attachment4);
					}
					this.WallPostVM.UploadAttachments();
				}
			}
			else if (this.WallPostVM.GoDirectlyToPhotoChooser && e != null && e.IsNavigationInitiator)
			{
				Navigator.Current.GoBack();
			}
			Photo photo = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
			if (photo != null)
			{
				OutboundPhotoAttachment attachment5 = OutboundPhotoAttachment.CreateForChoosingExistingPhoto(photo, this.WallPostVM.UserOrGroupId, this.WallPostVM.IsGroup, PostType.WallPost);
				this.WallPostVM.AddAttachment(attachment5);
			}
            VKClient.Common.Backend.DataObjects.Video video = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
			if (video != null)
			{
				OutboundVideoAttachment attachment6 = new OutboundVideoAttachment(video);
				this.WallPostVM.AddAttachment(attachment6);
			}
			AudioObj audioObj = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
			if (audioObj != null)
			{
				OutboundAudioAttachment attachment7 = new OutboundAudioAttachment(audioObj);
				this.WallPostVM.AddAttachment(attachment7);
			}
			Doc doc = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
			if (doc != null)
			{
				OutboundDocumentAttachment attachment8 = new OutboundDocumentAttachment(doc);
				this.WallPostVM.AddAttachment(attachment8);
			}
			TimerAttachment timerAttachment = ParametersRepository.GetParameterForIdAndReset("PickedTimer") as TimerAttachment;
			if (timerAttachment != null)
			{
				OutboundTimerAttachment attachment9 = new OutboundTimerAttachment(timerAttachment);
				IEnumerable<IOutboundAttachment> arg_326_0 = this.WallPostVM.Attachments;
                Func<IOutboundAttachment, bool> arg_326_1 = new Func<IOutboundAttachment, bool>((a) => { return a.AttachmentId == "timestamp"; });
				
				IOutboundAttachment outboundAttachment = Enumerable.FirstOrDefault<IOutboundAttachment>(arg_326_0, arg_326_1);
				if (outboundAttachment != null)
				{
					int index = this.WallPostVM.Attachments.IndexOf(outboundAttachment);
					this.WallPostVM.RemoveAttachment(outboundAttachment);
					this.WallPostVM.InsertAttachment(index, attachment9);
				}
				else
				{
					this.WallPostVM.AddAttachment(attachment9);
				}
				this.WallPostVM.FromGroup = true;
			}
			FileOpenPickerContinuationEventArgs fileOpenPickerContinuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
			if ((fileOpenPickerContinuationEventArgs != null && Enumerable.Any<StorageFile>(fileOpenPickerContinuationEventArgs.Files)) || ParametersRepository.Contains("PickedPhotoDocuments"))
			{
				object parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
				IReadOnlyList<StorageFile> arg_3DA_0;
				if (fileOpenPickerContinuationEventArgs == null)
				{
					IReadOnlyList<StorageFile> readOnlyList = (List<StorageFile>)ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocuments");
					arg_3DA_0 = readOnlyList;
				}
				else
				{
					arg_3DA_0 = fileOpenPickerContinuationEventArgs.Files;
				}
				IReadOnlyList<StorageFile> readOnlyList2 = arg_3DA_0;
				AttachmentType attachmentType;
				if (parameterForIdAndReset != null && Enum.TryParse<AttachmentType>(parameterForIdAndReset.ToString(), out attachmentType))
				{
					using (IEnumerator<StorageFile> enumerator = readOnlyList2.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							StorageFile current = enumerator.Current;
							if (attachmentType != AttachmentType.VideoFromPhone)
							{
								if (attachmentType == AttachmentType.DocumentFromPhone || attachmentType == AttachmentType.DocumentPhoto)
								{
									OutboundUploadDocumentAttachment attachment10 = new OutboundUploadDocumentAttachment(current);
									this.WallPostVM.AddAttachment(attachment10);
									this.WallPostVM.UploadAttachments();
								}
							}
							else
							{
								long groupId = this.WallPostVM.FromGroup ? this.WallPostVM.UserOrGroupId : 0L;
								OutboundUploadVideoAttachment attachment11 = new OutboundUploadVideoAttachment(current, true, groupId);
								this.WallPostVM.AddAttachment(attachment11);
								this.WallPostVM.UploadAttachments();
							}
						}
					}
				}
			}
			List<StorageFile> list3 = ParametersRepository.GetParameterForIdAndReset("ChosenDocuments") as List<StorageFile>;
			if (list3 != null)
			{
				IEnumerable<StorageFile> arg_4D4_0 = list3;
				Func<StorageFile, OutboundUploadDocumentAttachment> arg_4D4_1 = new Func<StorageFile, OutboundUploadDocumentAttachment>( (chosenDocument)=>new OutboundUploadDocumentAttachment(chosenDocument));
				using (IEnumerator<OutboundUploadDocumentAttachment> enumerator2 = Enumerable.Select<StorageFile, OutboundUploadDocumentAttachment>(arg_4D4_0, arg_4D4_1).GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						OutboundUploadDocumentAttachment current2 = enumerator2.Current;
						this.WallPostVM.AddAttachment(current2);
					}
				}
				this.WallPostVM.UploadAttachments();
			}
			List<StorageFile> list4 = ParametersRepository.GetParameterForIdAndReset("ChosenVideos") as List<StorageFile>;
			if (list4 != null)
			{
				IEnumerable<StorageFile> arg_550_0 = list4;
				Func<StorageFile, OutboundUploadVideoAttachment> arg_550_1 = new Func<StorageFile, OutboundUploadVideoAttachment>( (chosenDocument)=> new OutboundUploadVideoAttachment(chosenDocument, true, 0L));
				using (IEnumerator<OutboundUploadVideoAttachment> enumerator3 = Enumerable.Select<StorageFile, OutboundUploadVideoAttachment>(arg_550_0, arg_550_1).GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						OutboundUploadVideoAttachment current3 = enumerator3.Current;
						this.WallPostVM.AddAttachment(current3);
					}
				}
				this.WallPostVM.UploadAttachments();
			}
			return false;
		}

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "CanPublish") && !(e.PropertyName == "CanAddMoreAttachments"))
        return;
      this.UpdateAppBar();
    }

    private void Image_Delete_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      this.WallPostVM.RemoveAttachment(frameworkElement.DataContext as IOutboundAttachment);
      if (!this.WallPostVM.IsNewCommentMode || ((Collection<IOutboundAttachment>) this.WallPostVM.OutboundAttachments).Count != 0)
        return;
      Navigator.Current.GoBack();
    }

    private void textBoxPost_TextChanged_1(object sender, TextChangedEventArgs e)
    {
      this.WallPostVM.Text = this.textBoxPost.Text;
    }

    private void textBoxTitle_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.WallPostVM.TopicTitle = this.textBoxTopicTitle.Text;
      ((UIElement) this.textBlockWatermarkTitle).Opacity = (this.textBoxTopicTitle.Text == "" ? 1.0 : 0.0);
    }

    private void textBoxTitle_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter || string.IsNullOrEmpty(this.textBoxTopicTitle.Text))
        return;
      ((Control) this.textBoxPost).Focus();
    }

    private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      OutboundPhotoAttachment dataContext = frameworkElement.DataContext as OutboundPhotoAttachment;
      if (dataContext == null || dataContext.UploadState != OutboundAttachmentUploadState.NotStarted && dataContext.UploadState != OutboundAttachmentUploadState.Failed)
        return;
      this.WallPostVM.UploadAttachment((IOutboundAttachment) dataContext,  null);
    }

    private void AddAttachmentTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ((Control) this).Focus();
      List<NamedAttachmentType> attachmentTypes;
      int maxCount;
      if (this.WallPostVM.CanAddMoreAttachments)
      {
        attachmentTypes = new List<NamedAttachmentType>((IEnumerable<NamedAttachmentType>) AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation);
        if (this.WallPostVM.CanAddPollAttachment)
          attachmentTypes.Add(AttachmentTypes.PollAttachmentType);
        if (this.WallPostVM.CannAddTimerAttachment)
          attachmentTypes.Add(AttachmentTypes.TimerAttachmentType);
        maxCount = this.WallPostVM.NumberOfAttAllowedToAdd;
      }
      else
      {
        if (!this.WallPostVM.CannAddTimerAttachment)
          return;
        attachmentTypes = new List<NamedAttachmentType>()
        {
          AttachmentTypes.TimerAttachmentType
        };
        maxCount = 1;
      }
      this._pickerUC = AttachmentPickerUC.Show(attachmentTypes, maxCount, (Action) (() => this.HandleInputParams( null)), this._excludeLocation, this.CommonParameters.IsGroup ? -this.CommonParameters.UserOrGroupId : this.CommonParameters.UserOrGroupId, this._adminLevel,  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/NewPost.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
      this.scroll = (ScrollViewer) base.FindName("scroll");
      this.stackPanel = (StackPanel) base.FindName("stackPanel");
      this.textBoxTopicTitle = (TextBox) base.FindName("textBoxTopicTitle");
      this.textBlockWatermarkTitle = (TextBlock) base.FindName("textBlockWatermarkTitle");
      this.ucNewPost = (NewPostUC) base.FindName("ucNewPost");
      this.wallRepostContainer = (Border) base.FindName("wallRepostContainer");
      this.checkBoxFriendsOnly = (CheckBox) base.FindName("checkBoxFriendsOnly");
      this.textBoxPanel = (TextBoxPanelControl) base.FindName("textBoxPanel");
    }
  }
}
