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
using System.Windows.Input;
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
    private const int COMMENTS_PRELOAD_COUNT = 7;
    private readonly ViewportScrollableAreaAdapter _adapter;
    private readonly PhotoChooserTask _photoChooserTask;
    private bool _isInitialized;
    private long _ownerId;
    private long _productId;
    private ProductViewModel _viewModel;
    private WallPostViewModel _commentVM;
    private double _floatActionsLockThreshold;
    private DialogService _ds;
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
      PhotoChooserTask photoChooserTask = new PhotoChooserTask();
      int num = 1;
      photoChooserTask.ShowCamera = (num != 0);
      this._photoChooserTask = photoChooserTask;
      this._ds = new DialogService();
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = (CommonResources.Product.ToUpperInvariant());
      this.ucHeader.OnHeaderTap = new Action(this.Header_OnTap);
      this.ucMoreActions.SetBlue();
      this.ucMoreActions.TapCallback = new Action(this.ShowContextMenu);
      this._adapter = new ViewportScrollableAreaAdapter(this.viewportControl);
      this.ucCommentGeneric.InitializeWithScrollViewer((IScrollableArea) this._adapter);
      this.ucCommentGeneric.UCNewComment = this.ucNewMessage;
      this.ucNewMessage.PanelControl.IsOpenedChanged += new EventHandler<bool>(this.Panel_OnIsOpenedChanged);
      this.ucNewMessage.OnAddAttachTap = new Action(this.AddAttach);
      this.ucNewMessage.OnSendTap = new Action(this.Send);
      this.ucNewMessage.UCNewPost.OnImageDeleteTap = new Action<object>(this.DeleteImage);
      this.ucNewMessage.UCNewPost.TextBlockWatermarkText.Text = CommonResources.Comment;
      Binding binding = new Binding("OutboundAttachments");
      ((FrameworkElement) this.ucNewMessage.UCNewPost.ItemsControlAttachments).SetBinding((DependencyProperty) ItemsControl.ItemsSourceProperty, binding);
      this.viewportControl.BindViewportBoundsTo((FrameworkElement) this.stackPanelContent);
      this.viewportControl.ViewportChanged += (new EventHandler<ViewportChangedEventArgs>(this.ViewportControl_OnViewportChanged));
      this.RegisterForCleanup((IMyVirtualizingPanel) this.ucCommentGeneric.Panel);
      ((ChooserBase<PhotoResult>) this._photoChooserTask).Completed += (new EventHandler<PhotoResult>(ProductPage.PhotoChooserTask_OnCompleted));
      // ISSUE: method pointer
      this.ucCommentGeneric.UCNewComment.TextBoxNewComment.TextChanged += (new TextChangedEventHandler( this.TextBoxNewComment_OnTextChanged));
      EventAggregator.Current.Subscribe(this);
    }

    private void ViewportControl_OnViewportChanged(object sender, ViewportChangedEventArgs args)
    {
      if (this.ucNewMessage.PanelControl.IsOpen || this.ucNewMessage.PanelControl.IsTextBoxTargetFocused)
        return;
      this.UpdateFloatControlsVisibility();
    }

    private void UpdateFloatControlsVisibility()
    {
      FloatActionsUC ucFloatActions = this.ucFloatActions;
      Rect viewport1 = this.viewportControl.Viewport;
      // ISSUE: explicit reference operation
      int num1 = ((Rect) @viewport1).Y <= this._floatActionsLockThreshold || this._floatActionsLockThreshold <= 0.0 ? 0 : 1;
      ((UIElement) ucFloatActions).Visibility = ((Visibility) num1);
      double num2 = this._floatActionsLockThreshold + ((FrameworkElement) this.ucNewMessage).ActualHeight;
      NewMessageUC ucNewMessage = this.ucNewMessage;
      Rect viewport2 = this.viewportControl.Viewport;
      // ISSUE: explicit reference operation
      int num3 = ((Rect) @viewport2).Y <= num2 || num2 <= 0.0 ? 1 : 0;
      ((UIElement) ucNewMessage).Visibility = ((Visibility) num3);
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
      menuItem1.Header = copyLink;
      MenuItem menuItem2 = menuItem1;
      // ISSUE: method pointer
      menuItem2.Click += new RoutedEventHandler( this.MenuItemCopyLink_OnClicked);
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
      AttachmentPickerUC.Show(AttachmentTypes.AttachmentTypesWithPhotoFromGalleryAndLocation, this._commentVM.NumberOfAttAllowedToAdd, (Action) (() => PostCommentsPage.HandleInputParams(this._commentVM)), true, 0, 0,  null);
    }

    private void Send()
    {
      this.ucCommentGeneric.AddComment((List<IOutboundAttachment>) Enumerable.ToList<IOutboundAttachment>(this._commentVM.OutboundAttachments), (Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (!res)
          return;
        this.InitializeCommentVM();
      }))),  null, "");
    }

    private void DeleteImage(object sender)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      ((Collection<IOutboundAttachment>) this._commentVM.OutboundAttachments).Remove(frameworkElement.DataContext as IOutboundAttachment);
    }

    private static void PhotoChooserTask_OnCompleted(object sender, PhotoResult e)
    {
        if (((TaskEventArgs)e).TaskResult != TaskResult.OK)
        return;
      ParametersRepository.SetParameterForId("ChoosenPhoto", e.ChosenPhoto);
    }

    private void TextBoxNewComment_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this.ucNewMessage.UpdateSendButton(this._viewModel.CanComment && this.IsReadyToSend());
    }

    private bool IsReadyToSend()
    {
      string text = this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text;
      ObservableCollection<IOutboundAttachment> outboundAttachments = this._commentVM.OutboundAttachments;
      if (!string.IsNullOrWhiteSpace(text) && ((Collection<IOutboundAttachment>) outboundAttachments).Count == 0)
        return true;
      if (((Collection<IOutboundAttachment>) outboundAttachments).Count > 0)
      {
        IEnumerable<IOutboundAttachment> arg_59_0 = outboundAttachments;
        Func<IOutboundAttachment, bool> arg_59_1 = new Func<IOutboundAttachment, bool>((a) => { return a.UploadState == OutboundAttachmentUploadState.Completed; });
		
		return Enumerable.All<IOutboundAttachment>(arg_59_0, arg_59_1);
      }
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
      this._sharePostUC = new SharePostUC(0L);
      this._sharePostUC.SendTap += new EventHandler(this.ButtonSendWithMessage_Click);
      this._sharePostUC.ShareTap += (EventHandler) ((sender, args) => this.Share(0, ""));
      this._ds.Child = (FrameworkElement) this._sharePostUC;
      this._ds.AnimationType = DialogService.AnimationTypes.None;
      this._ds.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
      this._ds.Show( null);
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
        IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
        this._ownerId = long.Parse(queryString["OwnerId"]);
        this._productId = long.Parse(queryString["ProductId"]);
        Product parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("Product") as Product;
        this.InitializeCommentVM();
        this._viewModel = parameterForIdAndReset != null ? new ProductViewModel(parameterForIdAndReset) : new ProductViewModel(this._ownerId, this._productId);
        this._viewModel.PageLoadInfoViewModel.LoadingStateChangedCallback = new Action(this.OnLoadedStateChanged);
        base.DataContext = this._viewModel;
        this._viewModel.Reload(true);
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
      this.State["CommentText"] = this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text;
    }

    private void RestoreUnboundState()
    {
      if (!this.State.ContainsKey("CommentText"))
        return;
      this.ucCommentGeneric.UCNewComment.TextBoxNewComment.Text = (this.State["CommentText"].ToString());
    }

    private void InitializeCommentVM()
    {
      this._commentVM = WallPostViewModel.CreateNewProductCommentVM(this._ownerId, this._productId);
      this._commentVM.PropertyChanged += new PropertyChangedEventHandler(this.CommentsVM_OnPropertyChanged);
      ((FrameworkElement) this.ucNewMessage).DataContext = this._commentVM;
    }

    private void CommentsVM_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
{
	if (sender == this._commentVM && e.PropertyName == "CanPublish")
	{
		IEnumerable<IOutboundAttachment> arg_45_0 = this._commentVM.OutboundAttachments;
        Func<IOutboundAttachment, bool> arg_45_1 = new Func<IOutboundAttachment, bool>((a) => { return a.UploadState != OutboundAttachmentUploadState.Uploading; });
		
		if (Enumerable.All<IOutboundAttachment>(arg_45_0, arg_45_1))
		{
			this._viewModel.SetInProgress(false, "");
		}
	}
}

    private void ProcessInputData()
    {
      Group parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("PickedGroupForRepost") as Group;
      if (parameterForIdAndReset1 != null)
        this.Share(parameterForIdAndReset1.id, parameterForIdAndReset1.name);
      Photo parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("PickedPhoto") as Photo;
      if (parameterForIdAndReset2 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) OutboundPhotoAttachment.CreateForChoosingExistingPhoto(parameterForIdAndReset2, 0, false, PostType.WallPost));
      VKClient.Common.Backend.DataObjects.Video parameterForIdAndReset3 = ParametersRepository.GetParameterForIdAndReset("PickedVideo") as VKClient.Common.Backend.DataObjects.Video;
      if (parameterForIdAndReset3 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundVideoAttachment(parameterForIdAndReset3));
      AudioObj parameterForIdAndReset4 = ParametersRepository.GetParameterForIdAndReset("PickedAudio") as AudioObj;
      if (parameterForIdAndReset4 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundAudioAttachment(parameterForIdAndReset4));
      Doc parameterForIdAndReset5 = ParametersRepository.GetParameterForIdAndReset("PickedDocument") as Doc;
      if (parameterForIdAndReset5 != null)
        this._commentVM.AddAttachment((IOutboundAttachment) new OutboundDocumentAttachment(parameterForIdAndReset5));
      List<Stream> parameterForIdAndReset6 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
      List<Stream> parameterForIdAndReset7 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotosPreviews") as List<Stream>;
      if (parameterForIdAndReset6 != null && parameterForIdAndReset7 != null)
      {
        for (int index = 0; index < parameterForIdAndReset6.Count; ++index)
        {
          Stream stream1 = parameterForIdAndReset6[index];
          Stream stream2 = parameterForIdAndReset7[index];
          long userOrGroupId = 0;
          int num1 = 0;
          Stream previewStream = stream2;
          int num2 = 0;
          this._commentVM.AddAttachment((IOutboundAttachment) OutboundPhotoAttachment.CreateForUploadNewPhoto(stream1, userOrGroupId, num1 != 0, previewStream, (PostType) num2));
        }
        this._viewModel.SetInProgress(true, CommonResources.WallPost_UploadingAttachments);
        this._commentVM.UploadAttachments();
      }
      FileOpenPickerContinuationEventArgs parameterForIdAndReset8 = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
      if ((parameterForIdAndReset8 == null || !Enumerable.Any<StorageFile>(parameterForIdAndReset8.Files)) && !ParametersRepository.Contains("PickedPhotoDocuments"))
        return;
      object parameterForIdAndReset9 = ParametersRepository.GetParameterForIdAndReset("FilePickedType");
      IReadOnlyList<StorageFile> storageFileList = parameterForIdAndReset8 != null ? parameterForIdAndReset8.Files : (IReadOnlyList<StorageFile>) ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocuments");
      AttachmentType attachmentType;
      // ISSUE: explicit reference operation
      // ISSUE: cast to a reference type
      if (parameterForIdAndReset9 == null || !Enum.TryParse<AttachmentType>(parameterForIdAndReset9.ToString(),out attachmentType))
        return;
      IEnumerator<StorageFile> enumerator = ((IEnumerable<StorageFile>) storageFileList).GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          StorageFile current = enumerator.Current;
          if (attachmentType != AttachmentType.VideoFromPhone)
          {
            if (attachmentType == AttachmentType.DocumentFromPhone || attachmentType == AttachmentType.DocumentPhoto)
            {
              this._commentVM.AddAttachment((IOutboundAttachment) new OutboundUploadDocumentAttachment(current));
              this._commentVM.UploadAttachments();
            }
          }
          else
          {
            this._commentVM.AddAttachment((IOutboundAttachment) new OutboundUploadVideoAttachment(current, true, 0L));
            this._commentVM.UploadAttachments();
          }
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
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
      Execute.ExecuteOnUIThread((Action) (() =>
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
        ((UIElement) this.ucNewMessage).Visibility = Visibility.Visible;
        base.UpdateLayout();
        ((UIElement) this.ucNewMessage).Visibility = Visibility.Collapsed;
        base.UpdateLayout();
        this.UpdateFloatControlsLockThreshold();
      }));
    }

    public void Handle(SpriteElementTapEvent message)
    {
      if (!this._isCurrentPage)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        TextBox textBoxNewComment = this.ucCommentGeneric.UCNewComment.TextBoxNewComment;
        int selectionStart = textBoxNewComment.SelectionStart;
        string str = textBoxNewComment.Text.Insert(selectionStart, message.Data.ElementCode);
        textBoxNewComment.Text = str;
        int num1 = selectionStart + message.Data.ElementCode.Length;
        int num2 = 0;
        textBoxNewComment.Select(num1, num2);
      }));
    }

    public void Handle(StickerItemTapEvent message)
    {
      if (!this._isCurrentPage)
        return;
      this.ucCommentGeneric.AddComment(new List<IOutboundAttachment>(), (Action<bool>) (res => {}), message.StickerItem, message.Referrer);
    }

    private void GridContent_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size newSize1 = e.NewSize;
      // ISSUE: explicit reference operation
      double height = ((Size) @newSize1).Height;
      if (double.IsInfinity(height) || double.IsNaN(height))
        return;
      ((FrameworkElement) this.canvasBackground).Height = height;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this.canvasBackground).Children).Clear();
      Rectangle rect = new Rectangle();
      double num = height;
      ((FrameworkElement) rect).Height = num;
      Thickness thickness = new Thickness(0.0);
      ((FrameworkElement) rect).Margin = thickness;
      Size newSize2 = e.NewSize;
      // ISSUE: explicit reference operation
      double width = ((Size) @newSize2).Width;
      ((FrameworkElement) rect).Width = width;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneNewsBackgroundBrush"];
      ((Shape) rect).Fill = ((Brush) solidColorBrush);
      List<Rectangle>.Enumerator enumerator = RectangleHelper.CoverByRectangles(rect).GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this.canvasBackground).Children).Add((UIElement) enumerator.Current);
      }
      finally
      {
        enumerator.Dispose();
      }
    }

    private void StackPanelProductInfo_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size newSize = e.NewSize;
      // ISSUE: explicit reference operation
      double height = ((Size) @newSize).Height;
      if (double.IsInfinity(height) || double.IsNaN(height))
        return;
      this.ucCommentGeneric.Panel.DeltaOffset = -height;
    }

    private void UpdateFloatControlsLockThreshold()
    {
      try
      {
        Point point = ((UIElement) this.gridAction).TransformToVisual((UIElement) this.stackPanelContent).Transform(new Point(0.0, 0.0));
        // ISSUE: explicit reference operation
        this._floatActionsLockThreshold = ((Point) @point).Y - this.rowDefinitionContent.ActualHeight + ((FrameworkElement) this.ucFloatActions).ActualHeight;
        if (((UIElement) this.ucNewMessage).Visibility == Visibility.Visible)
          this._floatActionsLockThreshold = this._floatActionsLockThreshold - ((FrameworkElement) this.ucNewMessage).ActualHeight;
        this.UpdateFloatControlsVisibility();
      }
      catch
      {
      }
    }

    private void Description_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this._viewModel.ExpandDescription();
      base.UpdateLayout();
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
      if (((ItemsControl) this.listBoxNavDots).ItemsSource == null)
        ((ItemsControl) this.listBoxNavDots).ItemsSource = ((IEnumerable) this._viewModel.Photos);
      if (this._viewModel.Photos.Count == 0)
        return;
      ((Selector) this.listBoxNavDots).SelectedIndex = e;
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
      ProductViewModel viewModel = this._viewModel;
      if (string.IsNullOrEmpty(viewModel != null ? viewModel.MetaData :  null) || string.IsNullOrEmpty(this.textBlockMetaData.Text))
        return;
      Size newSize = e.NewSize;
      // ISSUE: explicit reference operation
      if (((Size) @newSize).Height <= this.textBlockMetaData.LineHeight)
        return;
      this.textBlockMetaData.Text = (this._viewModel.MetaData.Replace(" · ", "\n"));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Market/Views/ProductPage.xaml", UriKind.Relative));
      this.ListBoxItemNavDotsStyle = (Style) base.FindName("ListBoxItemNavDotsStyle");
      this.rowDefinitionContent = (RowDefinition) base.FindName("rowDefinitionContent");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucMoreActions = (MoreActionsUC) base.FindName("ucMoreActions");
      this.viewportControl = (ViewportControl) base.FindName("viewportControl");
      this.stackPanelContent = (StackPanel) base.FindName("stackPanelContent");
      this.canvasBackground = (Canvas) base.FindName("canvasBackground");
      this.slideView = (SlideView) base.FindName("slideView");
      this.listBoxNavDots = (ListBox) base.FindName("listBoxNavDots");
      this.textBlockMetaData = (TextBlock) base.FindName("textBlockMetaData");
      this.gridAction = (Grid) base.FindName("gridAction");
      this.ucCommentGeneric = (CommentsGenericUC) base.FindName("ucCommentGeneric");
      this.ucNewMessage = (NewMessageUC) base.FindName("ucNewMessage");
      this.ucFloatActions = (FloatActionsUC) base.FindName("ucFloatActions");
    }
  }
}
