using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
using VKClient.Common.Library;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using VKClient.Photos.Localization;

using System.Linq;
using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Photos
{
  public class PhotosMainPage : PageBase
  {
    private bool _isInitialized;
    private int _adminLevel;
    private List<long> _selectedPhotos;
    private bool _selectForMove;
    private bool _isInEditMode;
    private readonly ApplicationBar _albumsAppBar;
    private readonly ApplicationBar _editAppBar;
    private readonly ApplicationBarIconButton _appBarButtonEdit;
    private readonly ApplicationBarIconButton _appBarButtonDelete;
    private readonly ApplicationBarIconButton _appBarButtonCancel;
    private readonly ApplicationBarIconButton _appBarMenuItemAddAlbum;
    private bool _showingDialog;
    internal GenericHeaderUC Header;
    internal ExtendedLongListSelector itemsControlAlbums;
    internal MyListBox listBoxAlbums;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    public bool IsInEditMode
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
          this.listBoxAlbums.UnselectAll();
        ((UIElement) this.itemsControlAlbums).Visibility = (this._isInEditMode ? Visibility.Collapsed : Visibility.Visible);
        ((UIElement) this.listBoxAlbums).Visibility = (!this._isInEditMode ? Visibility.Collapsed : Visibility.Visible);
        this.UpdateAppBar();
      }
    }

    public PhotosMainViewModel PhotosMainVM
    {
      get
      {
        return base.DataContext as PhotosMainViewModel;
      }
    }

    private bool OwnPhotos
    {
      get
      {
        if (!this.CommonParameters.IsGroup)
          return this.CommonParameters.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId;
        return false;
      }
    }

    private bool EditableGroupPhotos
    {
      get
      {
        if (this.CommonParameters.IsGroup)
          return this._adminLevel > 1;
        return false;
      }
    }

    public PhotosMainViewModel PhotoMainVM
    {
      get
      {
        return base.DataContext as PhotosMainViewModel;
      }
    }

    public PhotosMainPage()
    {
      ApplicationBar applicationBar1 = new ApplicationBar();
      Color appBarBgColor1 = VKConstants.AppBarBGColor;
      applicationBar1.BackgroundColor = appBarBgColor1;
      Color appBarFgColor1 = VKConstants.AppBarFGColor;
      applicationBar1.ForegroundColor = appBarFgColor1;
      this._albumsAppBar = applicationBar1;
      ApplicationBar applicationBar2 = new ApplicationBar();
      Color appBarBgColor2 = VKConstants.AppBarBGColor;
      applicationBar2.BackgroundColor = appBarBgColor2;
      Color appBarFgColor2 = VKConstants.AppBarFGColor;
      applicationBar2.ForegroundColor = appBarFgColor2;
      this._editAppBar = applicationBar2;
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("Resources/appbar.manage.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string albumPageAppBarEdit = PhotoResources.PhotoAlbumPage_AppBar_Edit;
      applicationBarIconButton1.Text = albumPageAppBarEdit;
      this._appBarButtonEdit = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("Resources/appbar.delete.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string pageAppBarDelete = PhotoResources.EditAlbumPage_AppBar_Delete;
      applicationBarIconButton2.Text = pageAppBarDelete;
      this._appBarButtonDelete = applicationBarIconButton2;
      ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
      Uri uri3 = new Uri("Resources/appbar.cancel.rest.png", UriKind.Relative);
      applicationBarIconButton3.IconUri = uri3;
      string pageAppBarCancel = PhotoResources.EditAlbumPage_AppBar_Cancel;
      applicationBarIconButton3.Text = pageAppBarCancel;
      this._appBarButtonCancel = applicationBarIconButton3;
      ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
      Uri uri4 = new Uri("Resources/appbar.add.rest.png", UriKind.Relative);
      applicationBarIconButton4.IconUri = uri4;
      string mainPageAlbumsAdd = PhotoResources.PhotosMainPage_Albums_Add;
      applicationBarIconButton4.Text = mainPageAlbumsAdd;
      this._appBarMenuItemAddAlbum = applicationBarIconButton4;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.BuildAppBar();
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.itemsControlAlbums);
      this.itemsControlAlbums.OnRefresh = (Action)(() => this.PhotoMainVM.AlbumsVM.LoadData(true, false, (Action<BackendResult<AlbumsData, ResultCode>>)null, false));
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
    }

    private void OnHeaderTap()
    {
      if (this._isInEditMode)
        return;
      this.itemsControlAlbums.ScrollToTop();
    }

    private void BuildAppBar()
    {
      this._albumsAppBar.Buttons.Add(this._appBarMenuItemAddAlbum);
      this._albumsAppBar.Opacity = 0.9;
      this._editAppBar.Buttons.Add(this._appBarMenuItemAddAlbum);
      this._editAppBar.Buttons.Add(this._appBarButtonDelete);
      this._editAppBar.Buttons.Add(this._appBarButtonCancel);
      this._editAppBar.Opacity = 0.9;
      this._appBarMenuItemAddAlbum.Click+=(new EventHandler(this._appBarMenuItemAddAlbum_Click));
      this._appBarButtonEdit.Click+=(new EventHandler(this._appBarButtonEdit_Click));
      this._appBarButtonDelete.Click+=(new EventHandler(this._appBarButtonDelete_Click));
      this._appBarButtonCancel.Click+=(new EventHandler(this._appBarButtonCancel_Click));
    }

    private void _appBarMenuItemAddAlbum_Click(object sender, EventArgs e)
    {
      this.ShowEditAlbum(new Album());
    }

    private void _appBarButtonEdit_Click(object sender, EventArgs e)
    {
      this.IsInEditMode = true;
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      this.IsInEditMode = false;
    }

    private void _appBarButtonDelete_Click(object sender, EventArgs e)
    {
      if (!PhotosMainPage.AskDeleteAlbum(this.listBoxAlbums.GetSelected<AlbumHeader>().Count))
        return;
      this.PhotoMainVM.DeleteAlbums(this.listBoxAlbums.GetSelected<AlbumHeader>());
      if (((Collection<AlbumHeader>) this.PhotoMainVM.EditAlbumsVM.Collection).Count != 0)
        return;
      this.IsInEditMode = false;
    }

    private void UpdateAppBar()
    {
      if (this.OwnPhotos || this.EditableGroupPhotos)
      {
        if (this.CommonParameters.PickMode)
          this.ApplicationBar = ( null);
        else
          this.ApplicationBar = (!this._isInEditMode ? (IApplicationBar) this._albumsAppBar : (IApplicationBar) this._editAppBar);
      }
      else
      {
        this.ApplicationBar = ( null);
        ExtendedLongListSelector itemsControlAlbums = this.itemsControlAlbums;
        Thickness margin = ((FrameworkElement) this.itemsControlAlbums).Margin;
        // ISSUE: explicit reference operation
        double left = ((Thickness) @margin).Left;
        margin = ((FrameworkElement) this.itemsControlAlbums).Margin;
        // ISSUE: explicit reference operation
        double top = ((Thickness) @margin).Top;
        margin = ((FrameworkElement) this.itemsControlAlbums).Margin;
        // ISSUE: explicit reference operation
        double right = ((Thickness) @margin).Right;
        double num = -72.0;
        Thickness thickness = new Thickness(left, top, right, num);
        ((FrameworkElement) itemsControlAlbums).Margin = thickness;
      }
      this._appBarButtonDelete.IsEnabled = (this.listBoxAlbums.SelectedItems.Count > 0);
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      base.OnBackKeyPress(e);
      if (this._showingDialog)
      {
        this._showingDialog = false;
      }
      else
      {
        if (!this.IsInEditMode)
          return;
        e.Cancel = true;
        this.IsInEditMode = false;
      }
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
        base.HandleOnNavigatedTo(e);
        if (this._isInitialized)
            return;
        this._selectForMove = ((Page)this).NavigationContext.QueryString.ContainsKey("SelectForMove");
        string excludeAlbumId = ((Page)this).NavigationContext.QueryString.ContainsKey("ExcludeId") ? ((Page)this).NavigationContext.QueryString["ExcludeId"] : "";
        this._adminLevel = int.Parse(((Page)this).NavigationContext.QueryString["AdminLevel"]);
        if (((Page)this).NavigationContext.QueryString.ContainsKey("SelectedPhotos"))
            this._selectedPhotos = ((Page)this).NavigationContext.QueryString["SelectedPhotos"].ParseCommaSeparated();
        PhotosMainViewModel photosMainVM = new PhotosMainViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, this._selectForMove, excludeAlbumId);
        photosMainVM.AlbumsVM.Collection.CollectionChanged += (NotifyCollectionChangedEventHandler)((p, f) =>
        {
            ObservableCollection<Group<AlbumHeader>> collection = photosMainVM.AlbumsVM.Collection;
            Func<Group<AlbumHeader>, bool> func = (Func<Group<AlbumHeader>, bool>)(group => group.Any<AlbumHeader>((Func<AlbumHeader, bool>)(album => album.AlbumType == AlbumType.NormalAlbum)));
            if (collection.Any<Group<AlbumHeader>>(func))
            {
                if (this._albumsAppBar.Buttons.Contains((object)this._appBarButtonEdit))
                    return;
                this._albumsAppBar.Buttons.Add((object)this._appBarButtonEdit);
            }
            else
            {
                if (!this._albumsAppBar.Buttons.Contains((object)this._appBarButtonEdit))
                    return;
                this._albumsAppBar.Buttons.Remove((object)this._appBarButtonEdit);
            }
        });
        ((FrameworkElement)this).DataContext=((object)photosMainVM);
        photosMainVM.LoadAlbums();
        this.UpdateAppBar();
        this._isInitialized = true;
    }

    private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      AlbumHeader albumHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as AlbumHeader;
      if (albumHeader == null)
        return;
      if (!this._selectForMove)
      {
        INavigator current = Navigator.Current;
        long userOrGroupId = this.PhotoMainVM.UserOrGroupId;
        int num1 = this.PhotoMainVM.IsGroup ? 1 : 0;
        string type = albumHeader.AlbumType.ToString();
        string albumId = albumHeader.AlbumId;
        string albumName = albumHeader.AlbumName;
        int photosCount = albumHeader.PhotosCount;
        string title = this.PhotoMainVM.Title;
        string description = albumHeader.Album == null ? "" : albumHeader.Album.description ?? "";
        int num2 = this.CommonParameters.PickMode ? 1 : 0;
        int adminLevel = this._adminLevel;
        Album album = albumHeader.Album;
        int num3 = album != null ? (album.can_upload == 1 ? 1 : 0) : 0;
        current.NavigateToPhotoAlbum(userOrGroupId, num1 != 0, type, albumId, albumName, photosCount, title, description, num2 != 0, adminLevel, num3 != 0);
      }
      else
      {
        PhotosToMoveInfo photosToMoveInfo = new PhotosToMoveInfo();
        photosToMoveInfo.albumId = albumHeader.AlbumId;
        photosToMoveInfo.albumName = albumHeader.AlbumName;
        photosToMoveInfo.photos = this._selectedPhotos;
        PhotoAlbumViewModel.PhotoAlbumViewModelInput albumViewModelInput = new PhotoAlbumViewModel.PhotoAlbumViewModelInput();
        albumViewModelInput.AlbumDescription = albumHeader.Album == null ? "" : albumHeader.Album.description ?? "";
        albumViewModelInput.AlbumId = albumHeader.AlbumId;
        albumViewModelInput.AlbumName = albumHeader.AlbumName;
        albumViewModelInput.AlbumType = albumHeader.AlbumType;
        int num = this.PhotoMainVM.IsGroup ? 1 : 0;
        albumViewModelInput.IsGroup = num != 0;
        string photoPageTitle2 = this.PhotoMainVM.PhotoPageTitle2;
        albumViewModelInput.PageTitle = photoPageTitle2;
        int photosCount = albumHeader.PhotosCount;
        albumViewModelInput.PhotosCount = photosCount;
        long userOrGroupId = this.PhotoMainVM.UserOrGroupId;
        albumViewModelInput.UserOrGroupId = userOrGroupId;
        photosToMoveInfo.TargetAlbumInputData = albumViewModelInput;
        ParametersRepository.SetParameterForId("PhotosToMove", photosToMoveInfo);
        ((Page) this).NavigationService.GoBackSafe();
      }
    }

    private void ShowEditAlbum(Album album)
    {
        Execute.ExecuteOnUIThread((Action)(() =>
        {
            DialogService dc = new DialogService()
            {
                SetStatusBarBackground = true,
                HideOnNavigation = false
            };
            VKClient.Photos.UC.CreateAlbumUC createAlbumUc = new VKClient.Photos.UC.CreateAlbumUC();
            CreateEditAlbumViewModel editAlbumViewModel = new CreateEditAlbumViewModel(album, (Action<Album>)(a => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.PhotoMainVM.AddOrUpdateAlbum(a);
                dc.Hide();
                this._showingDialog = false;
            }))), this.PhotoMainVM.IsGroup ? this.PhotoMainVM.UserOrGroupId : 0L);
            ((FrameworkElement)createAlbumUc).DataContext=((object)editAlbumViewModel);
            ((UIElement)createAlbumUc).Visibility=Visibility.Visible;
            dc.Child = (FrameworkElement)createAlbumUc;
            this._showingDialog = true;
            dc.Show((UIElement)null);
        }));
    }

    private static bool AskDeleteAlbum(int count)
    {
        return MessageBox.Show(PhotoResources.GenericConfirmation, UIStringFormatterHelper.FormatNumberOfSomething(count, PhotoResources.DeleteOneAlbumFrm, PhotoResources.DeleteAlbumsFrm, PhotoResources.DeleteAlbumsFrm, true, (string)null, false), (MessageBoxButton)1) == MessageBoxResult.OK;
    }

    private void listBoxAlbums_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void listBoxAlbumsSelection(object sender, SelectionChangedEventArgs e)
    {
      ((Selector) this.listBoxAlbums).SelectedItem = null;
    }

    private void EditHeader_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      AlbumHeader albumHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as AlbumHeader;
      if (albumHeader == null)
        return;
      this.ShowEditAlbum(albumHeader.Album);
    }

    private void listBoxAlbums_Link(object sender, MyLinkUnlinkEventArgs e)
    {
      this.PhotosMainVM.EditAlbumsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void itemsControlAlbums_Link(object sender, LinkUnlinkEventArgs e)
    {
      AlbumHeader content = e.ContentPresenter.Content as AlbumHeader;
      if (content == null)
        return;
      foreach (Group<AlbumHeader> group in (Collection<Group<AlbumHeader>>) this.PhotosMainVM.AlbumsVM.Collection)
      {
        int count = ((Collection<AlbumHeader>) group).Count;
        if (count > 20 && ((Collection<AlbumHeader>) group)[count - 20] == content)
          this.PhotosMainVM.AlbumsVM.LoadData(false, false,  null, false);
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Photos;component/PhotosMainPage.xaml", UriKind.Relative));
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.itemsControlAlbums = (ExtendedLongListSelector) base.FindName("itemsControlAlbums");
      this.listBoxAlbums = (MyListBox) base.FindName("listBoxAlbums");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
    }
  }
}
