using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using VKClient.Photos.Localization;

namespace VKClient.Photos
{
  public class PhotoAlbumPage : PageBase
  {
    private bool _isInitialized;
    private bool _pickMode;
    private PhotoAlbumViewModel.PhotoAlbumViewModelInput _inputData;
    private bool _isInEditMode;
    private PhotoChooserTask _photoChooserTask = new PhotoChooserTask();
    private Stream _choosenPhotoPending;
    private ApplicationBar _mainAppBar;
    private ApplicationBar _editAppBar;
    private ApplicationBarIconButton _appBarIconButtonEdit;
    private ApplicationBarIconButton _appBarIconButtonAddPhoto;
    private ApplicationBarIconButton _appBarButtonDelete;
    private ApplicationBarIconButton _appBarButtonMoveToAlbum;
    private ApplicationBarIconButton _appBarButtonCancel;
    internal Grid LayoutRoot;
    internal ExtendedLongListSelector itemsControlPhotos;
    internal Grid ContentPanel;
    internal MyListBox listBoxPhotos;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    private PhotoAlbumViewModel PhotoAlbumVM
    {
      get
      {
        return base.DataContext as PhotoAlbumViewModel;
      }
    }

    private bool IsInEditMode
    {
      get
      {
        return this._isInEditMode;
      }
      set
      {
        if (this._isInEditMode == value)
          return;
        this._isInEditMode = value;
        if (this._isInEditMode)
          this.listBoxPhotos.UnselectAll();
        ((UIElement) this.itemsControlPhotos).Visibility=(this._isInEditMode ? Visibility.Collapsed : Visibility.Visible);
        ((UIElement) this.listBoxPhotos).Visibility=(!this._isInEditMode ? Visibility.Collapsed : Visibility.Visible);
        this.SuppressMenu = this._isInEditMode;
        this.Header.HideSandwitchButton = this._isInEditMode;
        this.UpdateAppBar();
        this.UpdateHeaderOpacity();
      }
    }

    public PhotoAlbumPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("Resources/appbar.manage.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri=(uri1);
      string albumPageAppBarEdit = PhotoResources.PhotoAlbumPage_AppBar_Edit;
      applicationBarIconButton1.Text=(albumPageAppBarEdit);
      this._appBarIconButtonEdit = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("Resources/appbar.feature.camera.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri=(uri2);
      string albumPageAddPhoto = PhotoResources.PhotoAlbumPage_AddPhoto;
      applicationBarIconButton2.Text=(albumPageAddPhoto);
      this._appBarIconButtonAddPhoto = applicationBarIconButton2;
      ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
      string pageAppBarDelete = PhotoResources.EditAlbumPage_AppBar_Delete;
      applicationBarIconButton3.Text=(pageAppBarDelete);
      Uri uri3 = new Uri("Resources/appbar.delete.rest.png", UriKind.Relative);
      applicationBarIconButton3.IconUri=(uri3);
      this._appBarButtonDelete = applicationBarIconButton3;
      ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
      string appBarMoveToAlbum = PhotoResources.EditAlbumPage_AppBar_MoveToAlbum;
      applicationBarIconButton4.Text=(appBarMoveToAlbum);
      Uri uri4 = new Uri("Resources/appbar.movetofolder.rest.png", UriKind.Relative);
      applicationBarIconButton4.IconUri=(uri4);
      this._appBarButtonMoveToAlbum = applicationBarIconButton4;
      ApplicationBarIconButton applicationBarIconButton5 = new ApplicationBarIconButton();
      string pageAppBarCancel = PhotoResources.EditAlbumPage_AppBar_Cancel;
      applicationBarIconButton5.Text=(pageAppBarCancel);
      Uri uri5 = new Uri("Resources/appbar.cancel.rest.png", UriKind.Relative);
      applicationBarIconButton5.IconUri=(uri5);
      this._appBarButtonCancel = applicationBarIconButton5;
      // ISSUE: explicit constructor call
     // base.\u002Ector();
      this.InitializeComponent();
      this._photoChooserTask.ShowCamera=(true);
      ((ChooserBase<PhotoResult>) this._photoChooserTask).Completed+=(new EventHandler<PhotoResult>(this._photoChooserTask_Completed));
      this.BuildAppBar();
      this.itemsControlPhotos.ScrollPositionChanged += new EventHandler(this.itemsControlPhotos_ScrollPositionChanged);
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
      this.Header.HeaderBackgroundBrush = (Brush) (Application.Current.Resources["PhonePhotoHeaderBackgroundBrush"] as SolidColorBrush);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.itemsControlPhotos);
      this.itemsControlPhotos.OnRefresh = new Action(this.HandleOnRefresh);
    }

    private void HandleOnRefresh()
    {
      if (this.PhotoAlbumVM == null)
        return;
      this.PhotoAlbumVM.RefreshPhotos();
    }

    private void OnHeaderTap()
    {
      if (!this.PhotoAlbumVM.PhotosGenCol.Collection.Any<AlbumPhotoHeaderFourInARow>())
        return;
      this.itemsControlPhotos.ScrollToTop();
    }

    private void itemsControlPhotos_ScrollPositionChanged(object sender, EventArgs e)
    {
      this.UpdateHeaderOpacity();
    }

    private void UpdateHeaderOpacity()
    {
      if (this.IsInEditMode)
      {
        ((UIElement) this.Header).Opacity=(1.0);
      }
      else
      {
        if (this.itemsControlPhotos.LockedBounds)
          return;
        ((UIElement) this.Header).Opacity=(this.CalculateOpacityFadeAwayBasedOnScroll(this.itemsControlPhotos.ScrollPosition + 88.0));
        if (this.PhotoAlbumVM == null)
          return;
        this.PhotoAlbumVM.HeaderOpacity = 1.0 - this.CalculateOpacityFadeAwayBasedOnScroll(this.itemsControlPhotos.ScrollPosition + 88.0 + 44.0);
      }
    }

    private double CalculateOpacityFadeAwayBasedOnScroll(double sp)
    {
      return sp >= 232.0 ? (sp <= 320.0 ? 1.0 / 88.0 * sp - 29.0 / 11.0 : 1.0) : 0.0;
    }

    private void _photoChooserTask_Completed(object sender, PhotoResult e)
    {
        if (((TaskEventArgs)e).TaskResult != TaskResult.OK)
        return;
      this._choosenPhotoPending = e.ChosenPhoto;
    }

    private void _appBarIconButtonAddPhoto_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(20, false, false);
    }

    private void _appBarIconButtonEdit_Click(object sender, EventArgs e)
    {
      this.IsInEditMode = true;
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar1 = new ApplicationBar();
      Color appBarBgColor1 = VKConstants.AppBarBGColor;
      applicationBar1.BackgroundColor=(appBarBgColor1);
      Color appBarFgColor1 = VKConstants.AppBarFGColor;
      applicationBar1.ForegroundColor=(appBarFgColor1);
      this._mainAppBar = applicationBar1;
      this._mainAppBar.Opacity=(0.9);
      this._appBarIconButtonAddPhoto.Click+=(new EventHandler(this._appBarIconButtonAddPhoto_Click));
      this._appBarIconButtonEdit.Click+=(new EventHandler(this._appBarIconButtonEdit_Click));
      ApplicationBar applicationBar2 = new ApplicationBar();
      Color appBarBgColor2 = VKConstants.AppBarBGColor;
      applicationBar2.BackgroundColor=(appBarBgColor2);
      Color appBarFgColor2 = VKConstants.AppBarFGColor;
      applicationBar2.ForegroundColor=(appBarFgColor2);
      this._editAppBar = applicationBar2;
      this._editAppBar.Opacity=(0.9);
      this._appBarButtonCancel.Click+=(new EventHandler(this._appBarButtonCancel_Click));
      this._appBarButtonDelete.Click+=(new EventHandler(this._appBarButtonDelete_Click));
      this._appBarButtonMoveToAlbum.Click+=(new EventHandler(this._appBarButtonMoveToAlbum_Click));
      this._editAppBar.Buttons.Add(this._appBarButtonDelete);
      this._editAppBar.Buttons.Add(this._appBarButtonMoveToAlbum);
      this._editAppBar.Buttons.Add(this._appBarButtonCancel);
    }

    private void _appBarButtonMoveToAlbum_Click(object sender, EventArgs e)
    {
      Navigator.Current.PickAlbumToMovePhotos(this.PhotoAlbumVM.InputData.UserOrGroupId, this.PhotoAlbumVM.InputData.IsGroup, this.PhotoAlbumVM.AlbumId, this.GetSelected().Select<AlbumPhoto, long>((Func<AlbumPhoto, long>) (a => a.Photo.pid)).ToList<long>(), this.PhotoAlbumVM.CanEditAlbum ? 3 : 0);
    }

    private void _appBarButtonDelete_Click(object sender, EventArgs e)
    {
      if (!this.AskDeletePhotoConfirmation(this.GetSelected().Count))
        return;
      this.PhotoAlbumVM.DeletePhotos(this.GetSelected());
      if (this.PhotoAlbumVM.PhotosCount != 0)
        return;
      this.IsInEditMode = false;
    }

    private bool AskDeletePhotoConfirmation(int count)
    {
      return MessageBox.Show(PhotoResources.DeletePhotoConfirmation, UIStringFormatterHelper.FormatNumberOfSomething(count, PhotoResources.DeleteOnePhoto, PhotoResources.DeletePhotosFrm, PhotoResources.DeletePhotosFrm, true,  null, false), (MessageBoxButton) 1) == MessageBoxResult.OK;
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      this.IsInEditMode = false;
    }

    private void UpdateAppBar()
    {
      if (this.ImageViewerDecorator != null && this.ImageViewerDecorator.IsShown || this.IsMenuOpen)
        return;
      if (!this._isInEditMode)
      {
        if (this.PhotoAlbumVM.CanAddPhotos || this.PhotoAlbumVM.CanEditAlbum)
        {
          this.ApplicationBar=((IApplicationBar) this._mainAppBar);
        }
        else
        {
          this.ApplicationBar=( null);
          ExtendedLongListSelector itemsControlPhotos = this.itemsControlPhotos;
          Thickness margin = ((FrameworkElement) this.itemsControlPhotos).Margin;
          // ISSUE: explicit reference operation
          double left = ((Thickness) @margin).Left;
          margin = ((FrameworkElement) this.itemsControlPhotos).Margin;
          // ISSUE: explicit reference operation
          double top = ((Thickness) @margin).Top;
          margin = ((FrameworkElement) this.itemsControlPhotos).Margin;
          // ISSUE: explicit reference operation
          double right = ((Thickness) @margin).Right;
          double num = -72.0;
          Thickness thickness = new Thickness(left, top, right, num);
          ((FrameworkElement) itemsControlPhotos).Margin=(thickness);
        }
      }
      else
        this.ApplicationBar=((IApplicationBar) this._editAppBar);
      ApplicationBarIconButton appBarButtonDelete = this._appBarButtonDelete;
      bool flag;
      this._appBarButtonMoveToAlbum.IsEnabled=(flag = this.listBoxPhotos.SelectedItems.Count > 0);
      int num1 = flag ? 1 : 0;
      appBarButtonDelete.IsEnabled=(num1 != 0);
      this._appBarIconButtonEdit.IsEnabled=(this.PhotoAlbumVM.PhotosCount > 0);
      if (this.PhotoAlbumVM.CanAddPhotos)
      {
        if (!this._mainAppBar.Buttons.Contains(this._appBarIconButtonAddPhoto))
          this._mainAppBar.Buttons.Add(this._appBarIconButtonAddPhoto);
      }
      else if (this._mainAppBar.Buttons.Contains(this._appBarIconButtonAddPhoto))
        this._mainAppBar.Buttons.Remove(this._appBarIconButtonAddPhoto);
      if (this.PhotoAlbumVM.CanEditAlbum)
      {
        if (this._mainAppBar.Buttons.Contains(this._appBarIconButtonEdit))
          return;
        this._mainAppBar.Buttons.Add(this._appBarIconButtonEdit);
      }
      else
      {
        if (!this._mainAppBar.Buttons.Contains(this._appBarIconButtonEdit))
          return;
        this._mainAppBar.Buttons.Remove(this._appBarIconButtonEdit);
      }
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      base.OnBackKeyPress(e);
      if (!this.IsInEditMode)
        return;
      e.Cancel = true;
      this.IsInEditMode = false;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      PhotosToMoveInfo photosToMove = ParametersRepository.GetParameterForIdAndReset("PhotosToMove") as PhotosToMoveInfo;
      bool needRefreshAfterMove = false;
      if (!this._isInitialized)
      {
        PhotoAlbumViewModel.PhotoAlbumViewModelInput inputData = new PhotoAlbumViewModel.PhotoAlbumViewModelInput();
        inputData.AlbumId = base.NavigationContext.QueryString["albumId"];
        inputData.UserOrGroupId = (long) int.Parse(base.NavigationContext.QueryString["userOrGroupId"]);
        inputData.IsGroup = bool.Parse(base.NavigationContext.QueryString["isGroup"]);
        if (base.NavigationContext.QueryString.ContainsKey("albumName"))
        {
          inputData.AlbumName = base.NavigationContext.QueryString["albumName"];
          inputData.AlbumType = (AlbumType) Enum.Parse(typeof (AlbumType), base.NavigationContext.QueryString["albumType"], true);
          inputData.PageTitle = base.NavigationContext.QueryString["pageTitle"];
          inputData.AlbumDescription = base.NavigationContext.QueryString["albumDesc"];
          inputData.PhotosCount = int.Parse(base.NavigationContext.QueryString["photosCount"]);
          this._pickMode = bool.Parse(base.NavigationContext.QueryString["PickMode"]);
          inputData.AdminLevel = int.Parse(base.NavigationContext.QueryString["AdminLevel"]);
          inputData.ForceCanUpload = bool.Parse(base.NavigationContext.QueryString["ForceCanUpload"]);
        }
        PhotoAlbumViewModel photoAlbumViewModel = new PhotoAlbumViewModel(inputData);
        photoAlbumViewModel.PropertyChanged += (PropertyChangedEventHandler) ((sender, args) =>
        {
          bool flag = this.PhotoAlbumVM.PhotosCount > 0;
          if (this._appBarIconButtonEdit.IsEnabled == flag)
            return;
          this._appBarIconButtonEdit.IsEnabled=(flag);
        });
        this.UpdateHeaderOpacity();
        base.DataContext=(photoAlbumViewModel);
        if (photosToMove == null)
          photoAlbumViewModel.RefreshPhotos();
        else
          needRefreshAfterMove = true;
        this.UpdateAppBar();
        this._inputData = inputData;
        this._isInitialized = true;
      }
      if (photosToMove != null)
        this.PhotoAlbumVM.MovePhotos(photosToMove.albumId, photosToMove.photos, (Action<bool>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (needRefreshAfterMove)
            this.PhotoAlbumVM.RefreshPhotos();
          if (this.PhotoAlbumVM.PhotosCount == 0)
            this.IsInEditMode = false;
          if (!result)
            ExtendedMessageBox.ShowSafe(CommonResources.GenericErrorText);
          else if (MessageBox.Show(UIStringFormatterHelper.FormatNumberOfSomething(photosToMove.photos.Count, PhotoResources.PhotoAlbumPageOnePhotoMovedFrm, PhotoResources.PhotoAlbumPageTwoFourPhotosMovedFrm, PhotoResources.PhotoAlbumPageFivePhotosMovedFrm, true, photosToMove.albumName, false), PhotoResources.PhotoAlbumPage_PhotoMove, (MessageBoxButton)1) == MessageBoxResult.OK)
            Navigator.Current.NavigateToPhotoAlbum(photosToMove.TargetAlbumInputData.UserOrGroupId, photosToMove.TargetAlbumInputData.IsGroup, photosToMove.TargetAlbumInputData.AlbumType.ToString(), photosToMove.TargetAlbumInputData.AlbumId, photosToMove.TargetAlbumInputData.AlbumName, photosToMove.TargetAlbumInputData.PhotosCount + photosToMove.photos.Count, photosToMove.TargetAlbumInputData.PageTitle, photosToMove.TargetAlbumInputData.AlbumDescription, false, 0, false);
          this.PhotoAlbumVM.UpdateThumbAfterPhotosMoving();
        }))));
      if (this._choosenPhotoPending != null)
      {
        this.PhotoAlbumVM.UploadPhoto(this._choosenPhotoPending, (Action<BackendResult<Photo, ResultCode>>) (res => {}));
        this._choosenPhotoPending =  null;
      }
      this.HandleInputParameters();
    }

    private void HandleInputParameters()
    {
      List<Stream> parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
      if (parameterForIdAndReset == null || parameterForIdAndReset.Count <= 0)
        return;
      this.UploadPhotos(parameterForIdAndReset, 0);
    }

    private void UploadPhotos(List<Stream> choosenPhotos, int ind)
    {
      if (ind < choosenPhotos.Count)
        Execute.ExecuteOnUIThread((Action) (() => this.PhotoAlbumVM.UploadPhoto(choosenPhotos[ind], (Action<BackendResult<Photo, ResultCode>>) (res =>
        {
          if (res.ResultCode != ResultCode.Succeeded)
            return;
          ++ind;
          this.UploadPhotos(choosenPhotos, ind);
        }))));
      else
        Execute.ExecuteOnUIThread(new Action(this.UpdateAppBar));
    }

    private List<AlbumPhoto> GetSelected()
    {
      return this.listBoxPhotos.GetSelected<AlbumPhoto>();
    }

    private void MakeCover_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = sender as MenuItem;
      AlbumPhoto albumPhoto =  null;
      AlbumPhotoHeaderFourInARow dataContext = ((FrameworkElement) menuItem).DataContext as AlbumPhotoHeaderFourInARow;
      if (dataContext != null)
      {
        string str = ((FrameworkElement) (((FrameworkElement) menuItem).Parent as ContextMenu)).Tag.ToString();
        if (!(str == "1"))
        {
          if (!(str == "2"))
          {
            if (!(str == "3"))
            {
              if (str == "4")
                albumPhoto = dataContext.Photo4;
            }
            else
              albumPhoto = dataContext.Photo3;
          }
          else
            albumPhoto = dataContext.Photo2;
        }
        else
          albumPhoto = dataContext.Photo1;
      }
      else
        albumPhoto = ((FrameworkElement) menuItem).DataContext as AlbumPhoto;
      if (albumPhoto == null)
        return;
      this.PhotoAlbumVM.MakeCover(albumPhoto.Photo);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menuItem = sender as MenuItem;
      AlbumPhoto albumPhoto =  null;
      AlbumPhotoHeaderFourInARow dataContext = ((FrameworkElement) menuItem).DataContext as AlbumPhotoHeaderFourInARow;
      if (dataContext != null)
      {
        string str = ((FrameworkElement) (((FrameworkElement) menuItem).Parent as ContextMenu)).Tag.ToString();
        if (!(str == "1"))
        {
          if (!(str == "2"))
          {
            if (!(str == "3"))
            {
              if (str == "4")
                albumPhoto = dataContext.Photo4;
            }
            else
              albumPhoto = dataContext.Photo3;
          }
          else
            albumPhoto = dataContext.Photo2;
        }
        else
          albumPhoto = dataContext.Photo1;
      }
      else
        albumPhoto = ((FrameworkElement) menuItem).DataContext as AlbumPhoto;
      if (albumPhoto == null || !this.AskDeletePhotoConfirmation(1))
        return;
      this.PhotoAlbumVM.DeletePhoto(albumPhoto.Photo);
    }

    private void Image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      AlbumPhoto albumPhoto =  null;
      AlbumPhotoHeaderFourInARow dataContext = frameworkElement.DataContext as AlbumPhotoHeaderFourInARow;
      if (dataContext != null)
      {
        string str = frameworkElement.Tag.ToString();
        if (!(str == "1"))
        {
          if (!(str == "2"))
          {
            if (!(str == "3"))
            {
              if (str == "4")
                albumPhoto = dataContext.Photo4;
            }
            else
              albumPhoto = dataContext.Photo3;
          }
          else
            albumPhoto = dataContext.Photo2;
        }
        else
          albumPhoto = dataContext.Photo1;
      }
      else
        albumPhoto = frameworkElement.DataContext as AlbumPhoto;
      if (albumPhoto == null)
        return;
      if (this._pickMode)
      {
        ParametersRepository.SetParameterForId("PickedPhoto", albumPhoto.Photo);
        base.NavigationService.RemoveBackEntrySafe();
        base.NavigationService.GoBackSafe();
      }
      else
      {
        List<Photo> list = this.PhotoAlbumVM.AlbumPhotos.Select<AlbumPhoto, Photo>((Func<AlbumPhoto, Photo>) (ap => ap.Photo)).ToList<Photo>();
        Navigator.Current.NavigateToImageViewer(this.PhotoAlbumVM.AlbumId, (int) this.PhotoAlbumVM.AType, this._inputData.UserOrGroupId, this._inputData.IsGroup, this.PhotoAlbumVM.PhotosCount, list.IndexOf(albumPhoto.Photo), list, new Func<int, Image>(this.GetPhotoById));
      }
    }

    private Image GetPhotoById(int arg)
    {
      int num1 = arg / 4;
      int num2 = arg % 4;
      return  null;
    }

    public T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
    {
      if (obj is T)
        return obj as T;
      int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
      if (childrenCount < 1)
        return default (T);
      for (int index = 0; index < childrenCount; ++index)
      {
        DependencyObject child = VisualTreeHelper.GetChild(obj, index);
        if (child is T)
          return child as T;
      }
      for (int index = 0; index < childrenCount; ++index)
      {
        DependencyObject descendant = (DependencyObject) this.FindDescendant<T>(VisualTreeHelper.GetChild(obj, index));
        if (descendant != null && descendant is T)
          return descendant as T;
      }
      return default (T);
    }

    private void itemsControlPhotos_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      int count = this.PhotoAlbumVM.PhotosGenCol.Collection.Count;
      AlbumPhotoHeaderFourInARow content = e.ContentPresenter.Content as AlbumPhotoHeaderFourInARow;
      if (count < 10 || content == null || this.PhotoAlbumVM.PhotosGenCol.Collection[count - 10] != content)
        return;
      this.PhotoAlbumVM.LoadMorePhotos();
    }

    private void listBoxPhotos_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      ((Selector) this.listBoxPhotos).SelectedItem=(null);
    }

    private void listBoxPhotos_Link_1(object sender, MyLinkUnlinkEventArgs e)
    {
      int count = this.PhotoAlbumVM.AlbumPhotos.Count;
      AlbumPhoto content = e.ContentPresenter.Content as AlbumPhoto;
      if (count < 20 || content == null || this.PhotoAlbumVM.AlbumPhotos[count - 20] != content)
        return;
      this.PhotoAlbumVM.LoadMorePhotos();
    }

    private void listBoxPhotos_MultiSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void Header_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Photos;component/PhotoAlbumPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.itemsControlPhotos = (ExtendedLongListSelector) base.FindName("itemsControlPhotos");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
      this.listBoxPhotos = (MyListBox) base.FindName("listBoxPhotos");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
    }
  }
}
