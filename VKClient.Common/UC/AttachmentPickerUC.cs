using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using Windows.Storage.Pickers;

namespace VKClient.Common.UC
{
  public class AttachmentPickerUC : UserControl
  {
    private DialogService _ds;
    private readonly CameraCaptureTask _cameraCaptureTask = new CameraCaptureTask();
    private Action _quickPhotoPickCallback;
    private AttachmentPickerViewModel _viewModel;
    private int _adminLevel;
    private ConversationInfo _conversationInfo;
    private string _savedText;
    private bool _albumPhotosSelected;
    private AttachmentSubpickerUC _subPickerUC;
    internal ScrollViewer scrollViewer;
    internal ExtendedLongListSelector listBoxPhotos;
    internal Border borderLoading;
    private bool _contentLoaded;

    public bool IsShown { get; private set; }

    private long OwnerId { get; set; }

    public AttachmentPickerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((ChooserBase<PhotoResult>) this._cameraCaptureTask).Completed += (new EventHandler<PhotoResult>(AttachmentPickerUC.CameraCaptureTask_OnCompleted));
    }

    private static void CameraCaptureTask_OnCompleted(object sender, PhotoResult e)
    {
        if (((TaskEventArgs)e).TaskResult != TaskResult.OK)
        return;
      MemoryStream memoryStream = StreamUtils.ReadFully(e.ChosenPhoto);
      e.ChosenPhoto.Position = 0L;
      memoryStream.Position = 0L;
      ParametersRepository.SetParameterForId("ChoosenPhotos", new List<Stream>()
      {
        e.ChosenPhoto
      });
      ParametersRepository.SetParameterForId("ChoosenPhotosPreviews", new List<Stream>()
      {
        (Stream) memoryStream
      });
      string paramId = "ChoosenPhotosSizes";
      List<Size> sizeList = new List<Size>();
      Size size =  new Size();
      sizeList.Add(size);
      ParametersRepository.SetParameterForId(paramId, sizeList);
    }

    public static AttachmentPickerUC Show(List<NamedAttachmentType> attachmentTypes, int maxCount, Action quickPhotoPickCallback, bool excludeLocation, long ownerId = 0, int adminLevel = 0, ConversationInfo conversationInfo = null)
    {
      AttachmentPickerUC attachmentPickerUc = new AttachmentPickerUC()
      {
        OwnerId = ownerId,
        _adminLevel = adminLevel,
        _conversationInfo = conversationInfo
      };
      if (maxCount > 0)
        attachmentPickerUc.DoShow((IEnumerable<NamedAttachmentType>) attachmentTypes, maxCount, quickPhotoPickCallback, excludeLocation);
      return attachmentPickerUc;
    }

    private void DoShow(IEnumerable<NamedAttachmentType> attachmentTypes, int maxCount, Action quickPhotoPickCallback, bool excludeLocation)
    {
      IEnumerable<NamedAttachmentType> namedAttachmentTypes = attachmentTypes;
      Func<NamedAttachmentType, bool> func1 = (Func<NamedAttachmentType, bool>) (t => t.AttachmentType == AttachmentType.Timer);
      if (Enumerable.All<NamedAttachmentType>(namedAttachmentTypes, (Func<NamedAttachmentType, bool>) func1))
        --maxCount;
      this._viewModel = new AttachmentPickerViewModel(AttachmentPickerUC.Convert(attachmentTypes, excludeLocation), maxCount);
      base.DataContext = this._viewModel;
      this._quickPhotoPickCallback = quickPhotoPickCallback;
      this._ds = new DialogService()
      {
        AnimationType = DialogService.AnimationTypes.None,
        AnimationTypeChild = DialogService.AnimationTypes.Swivel,
        Child = (FrameworkElement) this,
        HideOnNavigation = true
      };
      // ISSUE: method pointer
      base.SizeChanged += (delegate(object sender, SizeChangedEventArgs args)
      {
          Execute.ExecuteOnUIThread(new Action(this.UpdateListBoxPhotosSize));
      });
      PageBase currentPage = FramePageUtils.CurrentPage;
      if (currentPage != null)
      {
        currentPage.OrientationChanged += (new EventHandler<OrientationChangedEventArgs>(this.Page_OnOrientationChanged));
        this.UpdateScrollViewer(currentPage.Orientation);
      }
      this._viewModel.PhotosVM.LoadData(true, (Action) (() => Execute.ExecuteOnUIThread(new Action(this.UpdateListBoxPhotosSize))));
      this._ds.Closed += (EventHandler) ((s, e) =>
      {
        this._viewModel.PhotosVM.CleanupSession();
        this.IsShown = false;
      });
      this._ds.Show( null);
      this.IsShown = true;
    }

    private void Page_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      this.UpdateScrollViewer(e.Orientation);
      this.UpdateListBoxPhotosSize();
    }

    private void UpdateScrollViewer(PageOrientation orientation)
    {
        this.scrollViewer.VerticalScrollBarVisibility = (orientation == PageOrientation.Landscape || orientation == PageOrientation.LandscapeLeft || orientation == PageOrientation.LandscapeRight ? (ScrollBarVisibility)1 : (ScrollBarVisibility)0);
    }

    private void UpdateListBoxPhotosSize()
    {
      double buttonsCurrentSize = FramePageUtils.SoftNavButtonsCurrentSize;
      Content content = Application.Current.Host.Content;
      ((FrameworkElement) this.listBoxPhotos).Height=(!FramePageUtils.IsHorizontal ? content.ActualWidth : content.ActualHeight - buttonsCurrentSize);
      double num = (((FrameworkElement) this.listBoxPhotos).Height - ((FrameworkElement) this.listBoxPhotos).Width) / 2.0;
      ((FrameworkElement) this.listBoxPhotos).Margin=(new Thickness(num, -num, 0.0, -num));
    }

    private static List<AttachmentPickerItemViewModel> Convert(IEnumerable<NamedAttachmentType> attachmentTypes, bool excludeLocation)
    {
      List<AttachmentPickerItemViewModel> pickerItemViewModelList = new List<AttachmentPickerItemViewModel>();
      IEnumerator<NamedAttachmentType> enumerator = attachmentTypes.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          NamedAttachmentType current = enumerator.Current;
          if (!excludeLocation || current.AttachmentType != AttachmentType.Location)
          {
            AttachmentPickerItemViewModel pickerItemViewModel = new AttachmentPickerItemViewModel()
            {
              Title = string.Concat(((string) ((string) current.Name).Substring(0, 1)).ToUpper(), ((string) current.Name).Substring(1)),
              AttachmentType = current
            };
            switch (current.AttachmentType)
            {
              case AttachmentType.Photo:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Types/Photo32px.png";
                pickerItemViewModel.HighlightedIcon = "/Resources/Attach32px.png";
                break;
              case AttachmentType.Video:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Types/Video32px.png";
                break;
              case AttachmentType.Audio:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Types/Audio32px.png";
                break;
              case AttachmentType.Document:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Types/Document32px.png";
                break;
              case AttachmentType.Location:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Types/Location32px.png";
                break;
              case AttachmentType.PhotoFromPhone:
              case AttachmentType.VideoFromPhone:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Locations/Gallery32px.png";
                break;
              case AttachmentType.PhotoMy:
              case AttachmentType.VideoMy:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Locations/User32px.png";
                break;
              case AttachmentType.Poll:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Types/Poll32px.png";
                break;
              case AttachmentType.Timer:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Types/Timer32px.png";
                break;
              case AttachmentType.PhotoCommunity:
              case AttachmentType.VideoCommunity:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Locations/Community32px.png";
                break;
              case AttachmentType.Graffiti:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Types/Graffiti32px.png";
                break;
              case AttachmentType.MoneyTransfer:
                pickerItemViewModel.Icon = "/Resources/AttachmentPicker/Types/Money32px.png";
                break;
              case AttachmentType.Gift:
                pickerItemViewModel.Icon = "/Resources/Gift32px.png";
                break;
            }
            pickerItemViewModelList.Add(pickerItemViewModel);
          }
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
      return pickerItemViewModelList;
    }

    private void Image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null || !(frameworkElement.DataContext is AlbumPhoto))
        return;
      this.ToggleSelection(frameworkElement.DataContext as AlbumPhoto);
      this.UpdateAttachPhotoItem();
    }

    private void ToggleSelection(AlbumPhoto choosenPhoto)
    {
      if (choosenPhoto == null || !choosenPhoto.IsSelected && this._viewModel.PhotosVM.SelectedCount == this._viewModel.MaxCount)
        return;
      choosenPhoto.IsSelected = !choosenPhoto.IsSelected;
    }

    public static void AnimateTransform(double animateToScale, int dur, Transform transform, int center = 20)
    {
      ScaleTransform scaleTransform = transform as ScaleTransform;
      double num1 = (double) center;
      scaleTransform.CenterX = num1;
      double num2 = (double) center;
      scaleTransform.CenterY = num2;
      ((DependencyObject) transform).Animate(1.0, animateToScale, ScaleTransform.ScaleXProperty, dur, new int?(0), (IEasingFunction) new CubicEase(),  null, true);
      ((DependencyObject) transform).Animate(1.0, animateToScale, ScaleTransform.ScaleYProperty, dur, new int?(0), (IEasingFunction) new CubicEase(),  null, true);
    }

    private void UpdateAttachPhotoItem()
    {
        int number = Enumerable.Count<AlbumPhoto>(this._viewModel.PhotosVM.AlbumPhotos, (Func<AlbumPhoto, bool>)(p => p.IsSelected));
      this._albumPhotosSelected = number > 0;
      AttachmentPickerItemViewModel pickerItemViewModel = (AttachmentPickerItemViewModel)Enumerable.FirstOrDefault<AttachmentPickerItemViewModel>(this._viewModel.AttachmentTypes, (Func<AttachmentPickerItemViewModel, bool>)(at => at.AttachmentType.AttachmentType == AttachmentType.Photo));
      if (pickerItemViewModel == null)
        return;
      if (this._albumPhotosSelected)
      {
        if (this._savedText == "")
          this._savedText = pickerItemViewModel.Title;
        pickerItemViewModel.Title = UIStringFormatterHelper.FormatNumberOfSomething(number, CommonResources.AttachOnePhotoFrm, CommonResources.AttachTwoFourPhotosFrm, CommonResources.AttachFivePhotosFrm, true,  null, false);
        pickerItemViewModel.IsHighlighted = true;
      }
      else
      {
        pickerItemViewModel.Title = this._savedText;
        pickerItemViewModel.IsHighlighted = false;
      }
    }

    private void Item_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      AttachmentPickerItemViewModel dataContext = (sender as FrameworkElement).DataContext as AttachmentPickerItemViewModel;
      if (dataContext == null)
        return;
      this.HandleAttachmentSelection(dataContext);
    }

    private void HandleAttachmentSelection(AttachmentPickerItemViewModel item)
    {
      if (item == null)
        return;
      if (this._subPickerUC != null)
        this._subPickerUC.ItemSelected -= new AttachmentSubItemSelectedEventHandler(this.HandleAttachmentSelection);
      AttachmentType attachmentType = item.AttachmentType.AttachmentType;
      switch (attachmentType)
      {
        case AttachmentType.Photo:
          if (!item.IsHighlighted)
          {
            List<NamedAttachmentType> attachmentTypes = new List<NamedAttachmentType>((IEnumerable<NamedAttachmentType>) AttachmentTypes.AttachmentPhotoSubtypes);
            if (this.OwnerId < 0L && this._adminLevel > 1)
              attachmentTypes.Add(AttachmentTypes.PhotoCommunityType);
            this.ShowAttachmentSubPickerFor(attachmentTypes);
            break;
          }
          ((UIElement) this.borderLoading).Visibility = Visibility.Visible;
          ThreadPool.QueueUserWorkItem((WaitCallback) (state =>
          {
            List<Stream> choosedPhotos = new List<Stream>();
            List<Stream> previewsPhotos = new List<Stream>();
            List<Size> sizes = new List<Size>();
            this._viewModel.PhotosVM.SuppressEXIFFetch = true;
            IEnumerator<AlbumPhoto> enumerator4 = ((IEnumerable<AlbumPhoto>)Enumerable.Where<AlbumPhoto>(this._viewModel.PhotosVM.AlbumPhotos, (Func<AlbumPhoto, bool>)(ap => ap.IsSelected))).GetEnumerator();
            try
            {
              while (enumerator4.MoveNext())
              {
                AlbumPhoto current = enumerator4.Current;
                Stream imageStream = current.ImageStream;
                if (imageStream != null)
                {
                  choosedPhotos.Add(imageStream);
                  previewsPhotos.Add(current.ThumbnailStream);
                  Size size =  new Size();
                  sizes.Add(size);
                }
              }
            }
            finally
            {
              if (enumerator4 != null)
                enumerator4.Dispose();
            }
            Execute.ExecuteOnUIThread((Action) (() =>
            {
              ParametersRepository.SetParameterForId("ChoosenPhotos", choosedPhotos);
              ParametersRepository.SetParameterForId("ChoosenPhotosPreviews", previewsPhotos);
              ParametersRepository.SetParameterForId("ChoosenPhotosSizes", sizes);
              this._quickPhotoPickCallback();
              ((UIElement) this.borderLoading).Visibility = Visibility.Collapsed;
              this._ds.Hide();
            }));
          }));
          break;
        case AttachmentType.Video:
          List<NamedAttachmentType> attachmentTypes1 = new List<NamedAttachmentType>((IEnumerable<NamedAttachmentType>) AttachmentTypes.AttachmentVideoSubtypes);
          if (this.OwnerId < 0L && this._adminLevel > 1)
            attachmentTypes1.Add(AttachmentTypes.VideoCommunityType);
          this.ShowAttachmentSubPickerFor(attachmentTypes1);
          break;
        case AttachmentType.Audio:
          Navigator.Current.NavigateToAudio(1, 0, false, 0, 0, "");
          break;
        case AttachmentType.Document:
          Navigator.Current.NavigateToDocumentsPicker(this._viewModel.MaxCount);
          break;
        case AttachmentType.Location:
          Navigator.Current.NavigateToMap(true, 0.0, 0.0);
          break;
        case AttachmentType.PhotoFromPhone:
          Navigator.Current.NavigateToPhotoPickerPhotos(this._viewModel.MaxCount, false, false);
          break;
        case AttachmentType.VideoFromPhone:
          this._ds.Hide();
          FileOpenPicker fileOpenPicker1 = new FileOpenPicker();
          fileOpenPicker1.ContinuationData["FilePickedType"] = (int)attachmentType;
          List<string>.Enumerator enumerator1 = VKConstants.SupportedVideoExtensions.GetEnumerator();
          try
          {
            while (enumerator1.MoveNext())
            {
              string current = enumerator1.Current;
              fileOpenPicker1.FileTypeFilter.Add(current);
            }
          }
          finally
          {
            enumerator1.Dispose();
          }
          fileOpenPicker1.ContinuationData["Operation"] = "VideoFromPhone";
          fileOpenPicker1.PickSingleFileAndContinue();
          break;
        case AttachmentType.DocumentFromPhone:
          this._ds.Hide();
          FileOpenPicker fileOpenPicker2 = new FileOpenPicker();
          ((IDictionary<string, object>) fileOpenPicker2.ContinuationData)["FilePickedType"] = attachmentType;
          List<string>.Enumerator enumerator2 = VKConstants.SupportedDocExtensions.GetEnumerator();
          try
          {
            while (enumerator2.MoveNext())
            {
              string current = enumerator2.Current;
              fileOpenPicker2.FileTypeFilter.Add(current);
            }
          }
          finally
          {
            enumerator2.Dispose();
          }
          ((IDictionary<string, object>) fileOpenPicker2.ContinuationData)["Operation"] = "DocumentFromPhone";
          fileOpenPicker2.PickSingleFileAndContinue();
          break;
        case AttachmentType.PhotoMy:
          Navigator.Current.NavigateToPhotoAlbums(true, 0, false, 0);
          break;
        case AttachmentType.VideoMy:
          Navigator.Current.NavigateToVideo(true, 0, false, false);
          break;
        case AttachmentType.DocumentMy:
          Navigator.Current.NavigateToDocumentsPicker(this._viewModel.MaxCount);
          break;
        case AttachmentType.DocumentPhoto:
          this._ds.Hide();
          FileOpenPicker fileOpenPicker3 = new FileOpenPicker();
          ((IDictionary<string, object>) fileOpenPicker3.ContinuationData)["FilePickedType"] = attachmentType;
          List<string>.Enumerator enumerator3 = ((List<string>) VKConstants.SupportedDocLibraryExtensions).GetEnumerator();
          try
          {
            while (enumerator3.MoveNext())
            {
              string current = enumerator3.Current;
              fileOpenPicker3.FileTypeFilter.Add(current);
            }
          }
          finally
          {
            enumerator3.Dispose();
          }
          ((IDictionary<string, object>) fileOpenPicker3.ContinuationData)["Operation"] = "DocumentLibraryFromPhone";
          fileOpenPicker3.PickSingleFileAndContinue();
          break;
        case AttachmentType.Poll:
          Navigator.Current.NavigateToCreateEditPoll(this.OwnerId, 0,  null);
          break;
        case AttachmentType.Timer:
          Navigator.Current.NavigateToPostSchedule(new DateTime?());
          break;
        case AttachmentType.PhotoCommunity:
          Navigator.Current.NavigateToPhotoAlbums(true, -this.OwnerId, true, this._adminLevel);
          break;
        case AttachmentType.VideoCommunity:
          Navigator.Current.NavigateToVideo(true, -this.OwnerId, true, false);
          break;
        case AttachmentType.Graffiti:
          if (this._conversationInfo == null)
            break;
          this._ds.Hide();
          ParametersRepository.SetParameterForId("ConversationInfo", this._conversationInfo);
          Navigator.Current.NavigateToGraffitiDrawPage(this._conversationInfo.UserOrChatId, this._conversationInfo.IsChat, this._conversationInfo.Title);
          break;
        case AttachmentType.MoneyTransfer:
          Navigator.Current.NavigateToSendMoneyPage(this._conversationInfo.User.id, this._conversationInfo.User, 0, "");
          break;
        case AttachmentType.Gift:
          Navigator.Current.NavigateToGiftsCatalog(this._conversationInfo.UserOrChatId, this._conversationInfo.IsChat);
          break;
      }
    }

    private void ShowAttachmentSubPickerFor(List<NamedAttachmentType> attachmentTypes)
    {
      if (this._subPickerUC != null)
        this._subPickerUC.ItemSelected -= new AttachmentSubItemSelectedEventHandler(this.HandleAttachmentSelection);
      this._subPickerUC = new AttachmentSubpickerUC();
      this._subPickerUC.ItemSelected += new AttachmentSubItemSelectedEventHandler(this.HandleAttachmentSelection);
      this._subPickerUC.itemsControl.ItemsSource = ((IEnumerable) AttachmentPickerUC.Convert((IEnumerable<NamedAttachmentType>) attachmentTypes, true));
      this._ds.ChangeChild((FrameworkElement) this._subPickerUC,  null);
    }

    private void ExtendedLongListSelector_Link(object sender, LinkUnlinkEventArgs e)
    {
      int count = ((Collection<AlbumPhoto>) this._viewModel.PhotosVM.AlbumPhotos).Count;
      object dataContext = ((FrameworkElement) e.ContentPresenter).DataContext;
      if (count >= 20 && (count < 20 || ((Collection<AlbumPhoto>) this._viewModel.PhotosVM.AlbumPhotos)[count - 20] != dataContext))
        return;
      this._viewModel.PhotosVM.CountToLoad = 100;
      this._viewModel.PhotosVM.LoadData(false,  null);
    }

    private void Camera_tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this._ds.Hide();
      ((ChooserBase<PhotoResult>) this._cameraCaptureTask).Show();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/AttachmentPickerUC.xaml", UriKind.Relative));
      this.scrollViewer = (ScrollViewer) base.FindName("scrollViewer");
      this.listBoxPhotos = (ExtendedLongListSelector) base.FindName("listBoxPhotos");
      this.borderLoading = (Border) base.FindName("borderLoading");
    }
  }
}
