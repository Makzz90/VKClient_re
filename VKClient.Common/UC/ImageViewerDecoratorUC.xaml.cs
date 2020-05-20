using Microsoft.Phone.Applications.Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.Library;

namespace VKClient.Common.UC
{
    public partial class ImageViewerDecoratorUC : UserControl, INotifyPropertyChanged, IHandle<PhotoIsRepostedInGroup>, IHandle
    {
        private DispatcherTimer _timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(200.0)
        };
        private bool _cancelBackKeyPress = true;
        private DialogService _ds = new DialogService();
        private ApplicationBarMenuItem _appBarItemShare = new ApplicationBarMenuItem()
        {
            Text = CommonResources.AppBarMenu_Share
        };
        private ApplicationBarMenuItem _appBarItemSave = new ApplicationBarMenuItem()
        {
            Text = CommonResources.AppBarMenu_Save
        };
        private ApplicationBarMenuItem _appBarItemSaveInAlbum = new ApplicationBarMenuItem()
        {
            Text = CommonResources.AppBarMenu_SaveInAlbum
        };
        private bool _fromDialog;
        private bool _friendsOnly;
        private SharePostUC _sharePostUC;
        private ImageViewerViewModel _imageViewerVM;
        private bool _isShown;
        private IApplicationBar _savedPageAppBar;
        private ApplicationBar _defaultAppBar;
        private bool _userHideDecorator;
        private System.Windows.Controls.Page _initialPage;
        private Func<int, Image> _getImageByIdFunc;
        private FrameworkElement _currentViewControl;
        private Action<int> _setContextOnViewControl;
        private Action<int, bool> _showHideOverlay;
        private bool _hideActions;
        private PhoneApplicationPage _page;

        public bool ShareButtonOnly
        {
            set
            {
                this.AddButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool CancelBackKeyPress
        {
            get
            {
                return this._cancelBackKeyPress;
            }
            set
            {
                this._cancelBackKeyPress = value;
            }
        }

        public FrameworkElement GridBottom
        {
            get
            {
                return (FrameworkElement)this.gridBottom;
            }
        }

        public bool IsShown
        {
            get
            {
                return this._isShown;
            }
            private set
            {
                if (this._isShown == value)
                    return;
                if (this.IsShownChanging != null)
                    this.IsShownChanging((object)this, value);
                this._isShown = value;
            }
        }

        private ImageViewerViewModel ImageViewerVM
        {
            get
            {
                return this._imageViewerVM;
            }
            set
            {
                if (this._imageViewerVM == value)
                    return;
                this._imageViewerVM = value;
            }
        }

        public PhotoViewModel CurrentPhotoVM
        {
            get
            {
                return this.GetVMByIndex(this.imageViewer.CurrentInd);
            }
        }

        public PhotoViewModel NextPhotoVM
        {
            get
            {
                return this.GetVMByIndex(this.imageViewer.CurrentInd + 1);
            }
        }

        public PhotoViewModel PreviousPhotoVM
        {
            get
            {
                return this.GetVMByIndex(this.imageViewer.CurrentInd - 1);
            }
        }

        private PhoneApplicationPage Page
        {
            get
            {
                if (this._page == null)
                    this._page = (Application.Current.RootVisual as PhoneApplicationFrame).Content as PhoneApplicationPage;
                return this._page;
            }
        }

        public event EventHandler<bool> IsShownChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageViewerDecoratorUC()
        {
            this.InitializeComponent();
            this.Visibility = Visibility.Collapsed;
            this.imageViewer.SupportOrientationChange = true;
            this.imageViewer.HideCallback = (Action)(() => this.Hide(false));
            this.imageViewer.TapCallback = new Action(this.RespondToTap);
            this.imageViewer.DoubleTapCallback = new Action(this.RespondToDoubleTap);
            this.imageViewer.CurrentIndexChanged = new Action(this.RespondToCurrentIndexChanged);
            this.imageViewer.ManuallyAppliedOrientationChanged = new Action(this.RespondToManuallyAppliedOrientationChanged);
            this.imageViewer.IsInVerticalSwipeChanged = new Action(this.RespondToVertSwipeChange);
            this.imageViewer.Height = this.imageViewer.HARDCODED_HEIGHT;
            EventAggregator.Current.Subscribe((object)this);
            this.DataContext = (object)this;
            this.BuildAppBar();
        }

        private PhotoViewModel GetVMByIndex(int currentInd)
        {
            if (currentInd >= 0 && this.ImageViewerVM != null && currentInd < this.ImageViewerVM.PhotosCollection.Count)
                return this.ImageViewerVM.PhotosCollection[currentInd];
            return (PhotoViewModel)null;
        }

        private void BuildAppBar()
        {
            this._appBarItemShare.Click += new EventHandler(this._appBarItemShare_Click);
            this._appBarItemSave.Click += new EventHandler(this._appBarItemSave_Click);
            this._appBarItemSaveInAlbum.Click += new EventHandler(this._appBarItemSaveInAlbum_Click);
            this._defaultAppBar = new ApplicationBar()
            {
                BackgroundColor = Colors.Black,
                ForegroundColor = Colors.White,
                Opacity = 0.0,
                Mode = ApplicationBarMode.Minimized
            };
            this._defaultAppBar.StateChanged += this._defaultAppBar_StateChanged;
            this._defaultAppBar.MenuItems.Add((object)this._appBarItemShare);
            this._defaultAppBar.MenuItems.Add((object)this._appBarItemSave);
            this._defaultAppBar.MenuItems.Add((object)this._appBarItemSaveInAlbum);
        }

        private void _defaultAppBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            this._defaultAppBar.Opacity = e.IsMenuVisible ? 0.99 : 0.0;
        }

        private void _appBarItemSaveInAlbum_Click(object sender, EventArgs e)
        {
            if (this.CurrentPhotoVM == null)
                return;
            this.CurrentPhotoVM.SaveInSavedPhotosAlbum();
        }

        private void _appBarItemSave_Click(object sender, EventArgs e)
        {
            BitmapImage bitmap = this.imageViewer.CurrentImage.Source as BitmapImage;
            if (bitmap == null || bitmap.PixelWidth == 0)
                return;
            this.SaveImage(bitmap);
        }

        private void _appBarItemShare_Click(object sender, EventArgs e)
        {
            this.DoShare();
        }

        private void DoShare()
        {
            if (this.CurrentPhotoVM == null)
                return;
            this._ds = new DialogService()
            {
                SetStatusBarBackground = false,
                HideOnNavigation = false
            };
            this._sharePostUC = new SharePostUC();
            this._sharePostUC.ShareCommunityTap += new EventHandler(this.ButtonShareWithCommunity_Click);
            this._sharePostUC.SendTap += new EventHandler(this.ButtonSend_Click);
            this._sharePostUC.ShareTap += new EventHandler(this.ButtonShare_Click);
            if (this._fromDialog || this._friendsOnly)
            {
                this._sharePostUC.SetShareEnabled(false);
                this._sharePostUC.SetShareCommunityEnabled(false);
            }
            this._ds.Child = (FrameworkElement)this._sharePostUC;
            this._ds.AnimationType = DialogService.AnimationTypes.None;
            this._ds.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
            this._ds.Show(null);
        }

        private void ButtonShareWithCommunity_Click(object sender, EventArgs eventArgs)
        {
            if (this.CurrentPhotoVM == null)
                return;
            Navigator.Current.NavigateToGroups(AppGlobalStateManager.Current.LoggedInUserId, "", true, this.CurrentPhotoVM.Document == null ? this.CurrentPhotoVM.OwnerId : this.CurrentPhotoVM.Document.owner_id, this.CurrentPhotoVM.Document == null ? this.CurrentPhotoVM.Pid : this.CurrentPhotoVM.Document.id, UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text), this.CurrentPhotoVM.IsGif, this.CurrentPhotoVM.Document == null ? "" : this.CurrentPhotoVM.Document.access_key);
        }

        private void ButtonShare_Click(object sender, EventArgs eventArgs)
        {
            this.Share(0L, "");
        }

        private void Share(long gid = 0, string groupName = "")
        {
            if (this.CurrentPhotoVM == null)
                return;
            this._ds.Hide();
            this.CurrentPhotoVM.Share(UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text), gid, groupName);
        }

        private void ButtonSend_Click(object sender, EventArgs eventArgs)
        {
            if ((this.CurrentPhotoVM == null || this.CurrentPhotoVM.Photo == null) && this.CurrentPhotoVM.Document == null)
                return;
            this._ds.Hide();
            ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
            contentDataProvider.Message = this._sharePostUC.Text;
            contentDataProvider.Photo = this.CurrentPhotoVM.Document == null ? this.CurrentPhotoVM.Photo : null;
            contentDataProvider.Document = this.CurrentPhotoVM.Document;
            contentDataProvider.StoreDataToRepository();
            ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider)contentDataProvider);
            Navigator.Current.NavigateToPickConversation();
        }

        private void RespondToTap()
        {
            this._timer.Tick += new EventHandler(this._timer_Tick);
            this._timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            this._timer.Stop();
            this._timer.Tick -= this._timer_Tick;
            this._userHideDecorator = !this._userHideDecorator;
            this.Update();
        }

        private void RespondToDoubleTap()
        {
            this._timer.Stop();
            this._timer.Tick -= new EventHandler(this._timer_Tick);
        }

        private void RespondToVertSwipeChange()
        {
            this.Update();
        }

        private void RespondToManuallyAppliedOrientationChanged()
        {
            if (this.Page.SupportedOrientations == SupportedPageOrientation.PortraitOrLandscape)
                return;
            if (this.imageViewer.ManuallyAppliedOrientation == DeviceOrientation.LandscapeLeft)
            {
                this.gridTop.RenderTransform = (Transform)new CompositeTransform()
                {
                    Rotation = 90.0,
                    TranslateX = this.ActualWidth
                };
                CompositeTransform compositeTransform = new CompositeTransform();
                compositeTransform.Rotation = 90.0;
                compositeTransform.TranslateX = this.rectBlackBottom.ActualHeight - 18.0;
                compositeTransform.TranslateY = -this.ActualHeight + this.rectBlackBottom.ActualHeight;
                this.gridBottom.Margin = new Thickness();
                this.gridBottom.RenderTransform = (Transform)compositeTransform;
            }
            else if (this.imageViewer.ManuallyAppliedOrientation == DeviceOrientation.LandscapeRight)
            {
                this.gridTop.RenderTransform = (Transform)new CompositeTransform()
                {
                    Rotation = -90.0,
                    TranslateY = this.ActualHeight
                };
                CompositeTransform compositeTransform = new CompositeTransform();
                compositeTransform.Rotation = -90.0;
                compositeTransform.TranslateX = this.ActualWidth - this.rectBlackBottom.ActualHeight + 18.0;
                compositeTransform.TranslateY = this.rectBlackBottom.ActualHeight;
                this.gridBottom.Margin = new Thickness();
                this.gridBottom.RenderTransform = (Transform)compositeTransform;
            }
            else
            {
                this.gridBottom.RenderTransform = (Transform)new CompositeTransform();
                double num = 0.0;
                if (this.Page.Content is FrameworkElement)
                    num = (this.Page.Content as FrameworkElement).Margin.Bottom;
                this.gridBottom.Margin = new Thickness(0.0, 0.0, 0.0, -num);
                this.gridTop.RenderTransform = (Transform)new CompositeTransform();
            }
            this.Update();
        }

        private void RespondToCurrentIndexChanged()
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged((object)this, new PropertyChangedEventArgs("CurrentPhotoVM"));
            }
            this.UpdateAppBar();
            if (this.CurrentPhotoVM != null && !this.CurrentPhotoVM.IsLoadedFullInfo)
                this.CurrentPhotoVM.LoadInfoWithComments((Action<bool, int>)((res, r) => Execute.ExecuteOnUIThread((Action)(() =>
                {
                    if (this.NextPhotoVM == null || this.NextPhotoVM.IsLoadedFullInfo)
                        return;
                    this.NextPhotoVM.LoadInfoWithComments((Action<bool, int>)((resNext, ress) => Execute.ExecuteOnUIThread((Action)(() =>
                    {
                        if (this.PreviousPhotoVM == null || this.PreviousPhotoVM.IsLoadedFullInfo)
                            return;
                        this.PreviousPhotoVM.LoadInfoWithComments((Action<bool, int>)((resPrevious, re) => { }));
                    }))));
                }))));
            this.imageViewer.ForbidResizeInNormalMode = this.CurrentPhotoVM != null && this.CurrentPhotoVM.IsGif;
            if (this._imageViewerVM.PhotosCollection.Count - 1 - this.imageViewer.CurrentInd < 3)
                this.ImageViewerVM.LoadMorePhotos((Action<bool>)(res =>
                {
                    if (!res)
                        return;
                    this.HandlePhotoUpdate(this.ImageViewerVM);
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged((object)this, new PropertyChangedEventArgs("CurrentPhotoVM"));
                    }
                    if (this.CurrentPhotoVM != null && !this.CurrentPhotoVM.IsLoadedFullInfo)
                        this.CurrentPhotoVM.LoadInfoWithComments((Action<bool, int>)((r, r1) => { }));
                    this.Update();
                }));
            this.Update();
        }

        private void UpdateAppBar()
        {
            if ((this.CurrentPhotoVM == null ? 0 : (this.CurrentPhotoVM.OwnerId == AppGlobalStateManager.Current.LoggedInUserId ? 1 : 0)) != 0)
            {
                this._defaultAppBar.MenuItems.Remove((object)this._appBarItemSaveInAlbum);
            }
            else
            {
                if (this._defaultAppBar.MenuItems.Contains((object)this._appBarItemSaveInAlbum))
                    return;
                this._defaultAppBar.MenuItems.Add((object)this._appBarItemSaveInAlbum);
            }
        }

        private void Page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            this.Update();
        }

        public void Show(int ind)
        {
            this.Visibility = Visibility.Visible;
            this._savedPageAppBar = this.Page.ApplicationBar;
            this._userHideDecorator = false;
            PhoneApplicationPage page = this.Page;
            this._initialPage = (System.Windows.Controls.Page)this.Page;
            ApplicationBar applicationBar = !this._hideActions ? this._defaultAppBar : (ApplicationBar)null;
            page.ApplicationBar = (IApplicationBar)applicationBar;
            EventHandler<OrientationChangedEventArgs> eventHandler1 = new EventHandler<OrientationChangedEventArgs>(this.Page_OrientationChanged);
            page.OrientationChanged += eventHandler1;
            EventHandler<CancelEventArgs> eventHandler2 = new EventHandler<CancelEventArgs>(this.Page_BackKeyPress);
            page.BackKeyPress += eventHandler2;
            this.IsShown = true;
            this.RespondToManuallyAppliedOrientationChanged();
            this.imageViewer.Show(ind, (Action)(() => this.Update()), false, null);
        }

        public void Hide(bool leavingPageImmediately = false)
        {
            if (!leavingPageImmediately)
            {
                this.Page.OrientationChanged -= new EventHandler<OrientationChangedEventArgs>(this.Page_OrientationChanged);
                this.Page.BackKeyPress -= new EventHandler<CancelEventArgs>(this.Page_BackKeyPress);
                this.Page.ApplicationBar = this._savedPageAppBar;
            }
            this.IsShown = false;
            if (!leavingPageImmediately)
                this.Update();
            this.imageViewer.Hide((Action)(() => this.Visibility = Visibility.Collapsed), leavingPageImmediately);
        }

        private void Page_BackKeyPress(object sender, CancelEventArgs e)
        {
            if (this._ds.IsOpen)
                return;
            if (this._cancelBackKeyPress)
                e.Cancel = true;
            this.Hide(!this._cancelBackKeyPress);
        }

        public void Update()
        {
            if (this.Page != this._initialPage)
                return;
            if (!this.imageViewer.IsInVerticalSwipe && this.IsShown && !this._userHideDecorator)
            {
                this.textBlockCounter.Visibility = Visibility.Visible;
                if (this.CurrentPhotoVM != null && this.CurrentPhotoVM.IsGif)
                {
                    this.stackPanelLikesComments.Visibility = Visibility.Collapsed;
                    this.stackPanelGifs.Visibility = Visibility.Visible;
                }
                else
                {
                    this.stackPanelLikesComments.Visibility = Visibility.Visible;
                    this.stackPanelGifs.Visibility = Visibility.Collapsed;
                }
                this.Page.ApplicationBar = this.imageViewer.ManuallyAppliedOrientation == DeviceOrientation.LandscapeRight || this.imageViewer.ManuallyAppliedOrientation == DeviceOrientation.LandscapeLeft || (this.Page.Orientation == PageOrientation.LandscapeLeft || this.Page.Orientation == PageOrientation.LandscapeRight) || (this.CurrentPhotoVM == null || this.CurrentPhotoVM.IsGif) ? (IApplicationBar)null : (!this._hideActions ? (IApplicationBar)this._defaultAppBar : (IApplicationBar)null);
                this.rectBlackBottom.Visibility = Visibility.Visible;
                this.rectBlackTop.Visibility = Visibility.Visible;
                this.textBoxText.Visibility = Visibility.Visible;
            }
            else
            {
                if (this.IsShown)
                    this.Page.ApplicationBar = (IApplicationBar)null;
                this.rectBlackBottom.Visibility = Visibility.Collapsed;
                this.rectBlackTop.Visibility = Visibility.Collapsed;
                this.textBlockCounter.Visibility = Visibility.Collapsed;
                this.stackPanelLikesComments.Visibility = Visibility.Collapsed;
                this.stackPanelGifs.Visibility = Visibility.Collapsed;
                this.textBoxText.Visibility = Visibility.Collapsed;
            }
            if (this.ImageViewerVM.PhotosCount > 0)
                this.textBlockCounter.Text = string.Format(CommonResources.ImageViewer_PhotoCounterFrm, (object)(this.ImageViewerVM.PhotosCollection.IndexOf(this.CurrentPhotoVM) + 1 + this.ImageViewerVM.InitialOffset), (object)this.ImageViewerVM.PhotosCount);
            else
                this.textBlockCounter.Text = "";
        }

        private void InitWith(ImageViewerViewModel ivvm, Func<int, Image> getImageByIdFunc, FrameworkElement currentViewControl = null, Action<int> setContextOnViewControl = null, Action<int, bool> showHideOverlay = null)
        {
            this._imageViewerVM = ivvm;
            this._getImageByIdFunc = getImageByIdFunc;
            this._currentViewControl = currentViewControl;
            this._setContextOnViewControl = setContextOnViewControl;
            this._showHideOverlay = showHideOverlay;
            this.HandlePhotoUpdate(ivvm);
        }

        private void HandlePhotoUpdate(ImageViewerViewModel ivvm)
        {
            if (this._imageViewerVM != ivvm)
                return;
            this.imageViewer.Initialize(ivvm.PhotosCollection.Count, (Func<int, ImageInfo>)(ind =>
            {
                Func<int, Image> func = this._getImageByIdFunc;
                Image image1;
                if (func == null)
                {
                    image1 = null;
                }
                else
                {
                    int num = ind;
                    image1 = func(num);
                }
                Image image2 = image1;
                ImageInfo imageInfo = new ImageInfo();
                if (ind == this.imageViewer.CurrentInd && this.imageViewer.CurrentImage.Source is BitmapImage)
                {
                    BitmapImage bitmapImage = this.imageViewer.CurrentImage.Source as BitmapImage;
                    imageInfo.Width = (double)bitmapImage.PixelWidth;
                    imageInfo.Height = (double)bitmapImage.PixelHeight;
                }
                if ((image2 != null ? image2.Source : null) is BitmapImage)
                {
                    BitmapImage bitmapImage = (BitmapImage)image2.Source;
                    imageInfo.Width = (double)bitmapImage.PixelWidth;
                    imageInfo.Height = (double)bitmapImage.PixelHeight;
                }
                if (ind < 0 || ind >= ivvm.PhotosCollection.Count)
                    return (ImageInfo)null;
                PhotoViewModel photoViewModel = ivvm.PhotosCollection[ind];
                if (imageInfo.Width == 0.0)
                    imageInfo.Width = (double)photoViewModel.Photo.width;
                if (imageInfo.Height == 0.0)
                    imageInfo.Height = (double)photoViewModel.Photo.height;
                imageInfo.Uri = photoViewModel.ImageSrc;
                return imageInfo;
            }), this._getImageByIdFunc, (Action<int, bool>)((ind, show) =>
            {
                Func<int, Image> func = this._getImageByIdFunc;
                Image image1;
                if (func == null)
                {
                    image1 = null;
                }
                else
                {
                    int num = ind;
                    image1 = func(num);
                }
                Image image2 = image1;
                if (image2 != null)
                    image2.Opacity = show ? 1.0 : 0.0;
                Action<int, bool> action = this._showHideOverlay;
                if (action == null)
                    return;
                int num1 = ind;
                int num2 = show ? 1 : 0;
                action(num1, num2 != 0);
            }), this._currentViewControl, this._setContextOnViewControl);
        }

        public static void ShowPhotosFromFeed(long userOrGroupId, bool isGroup, string aid, int photosCount, int selectedPhotoIndex, int date, List<Photo> photos, string mode, Func<int, Image> getImageByIdFunc)
        {
            ViewerMode mode1 = (ViewerMode)Enum.Parse(typeof(ViewerMode), mode);
            ImageViewerViewModel ivvm = new ImageViewerViewModel(userOrGroupId, isGroup, aid, photosCount, date, photos, mode1);
            ImageViewerDecoratorUC dec = ImageViewerDecoratorUC.GetDecoratorForCurrentPage(null);
            dec.InitWith(ivvm, getImageByIdFunc, null, null, null);
            ivvm.LoadPhotosFromFeed((Action<bool>)(res =>
            {
                if (!res)
                    return;
                Execute.ExecuteOnUIThread((Action)(() => dec.HandlePhotoUpdate(ivvm)));
            }));
            dec.Show(selectedPhotoIndex);
        }

        public static void ShowPhotosOrGifsById(int selectedPhotoIndex, List<PhotoOrDocument> photoOrDocList, bool fromDialog = false, bool friendsOnly = false, Func<int, Image> getImageByIdFunc = null, PageBase page = null, bool hideActions = false, FrameworkElement currentViewControl = null, Action<int> setContextOnCurrentViewControl = null, Action<int, bool> showHideOverlay = null, bool shareButtonOnly = false)
        {
            ImageViewerViewModel ivvm = new ImageViewerViewModel(photoOrDocList);
            ImageViewerDecoratorUC decoratorForCurrentPage = ImageViewerDecoratorUC.GetDecoratorForCurrentPage(page);
            decoratorForCurrentPage.InitWith(ivvm, getImageByIdFunc, currentViewControl, setContextOnCurrentViewControl, showHideOverlay);
            decoratorForCurrentPage._fromDialog = fromDialog;
            decoratorForCurrentPage._friendsOnly = friendsOnly;
            decoratorForCurrentPage._hideActions = hideActions;
            decoratorForCurrentPage.ShareButtonOnly = shareButtonOnly;
            if (hideActions)
                decoratorForCurrentPage.gridBottom.Visibility = Visibility.Collapsed;
            decoratorForCurrentPage.Show(selectedPhotoIndex);
        }

        public static void ShowPhotosById(int photosCount, int initialOffset, int selectedPhotoIndex, List<long> photoIds, List<long> ownerIds, List<string> accessKeys, List<Photo> photos, bool fromDialog = false, bool friendsOnly = false, Func<int, Image> getImageByIdFunc = null, PageBase page = null, bool hideActions = false)
        {
            ViewerMode mode = ViewerMode.PhotosByIds;
            List<Photo> photos1 = photos;
            ImageViewerViewModel ivvm = new ImageViewerViewModel(photosCount, initialOffset, photoIds, ownerIds, accessKeys, photos1, mode);
            ImageViewerDecoratorUC decoratorForCurrentPage = ImageViewerDecoratorUC.GetDecoratorForCurrentPage(page);
            decoratorForCurrentPage.InitWith(ivvm, getImageByIdFunc, null, null, null);
            decoratorForCurrentPage._fromDialog = fromDialog;
            decoratorForCurrentPage._friendsOnly = friendsOnly;
            decoratorForCurrentPage._hideActions = hideActions;
            if (hideActions)
                decoratorForCurrentPage.gridBottom.Visibility = Visibility.Collapsed;
            decoratorForCurrentPage.Show(selectedPhotoIndex);
        }

        public static void ShowPhotosFromAlbum(string aid, int albumType, long userOrGroupId, bool isGroup, int photosCount, int selectedPhotoIndex, List<Photo> photos, Func<int, Image> getImageByIdFunc)
        {
            ImageViewerViewModel imageViewerViewModel = new ImageViewerViewModel(aid, (AlbumType)albumType, userOrGroupId, isGroup, photosCount, photos);
            ImageViewerDecoratorUC decoratorForCurrentPage = ImageViewerDecoratorUC.GetDecoratorForCurrentPage(null);
            ImageViewerViewModel ivvm = imageViewerViewModel;
            Func<int, Image> getImageByIdFunc1 = getImageByIdFunc;
            decoratorForCurrentPage.InitWith(ivvm, getImageByIdFunc1, null, null, null);
            int ind = selectedPhotoIndex;
            decoratorForCurrentPage.Show(ind);
        }

        public static void ShowPhotosFromProfile(long userOrGroupId, bool isGroup, int selectedPhotoIndex, List<Photo> photos, bool canLoadMoreProfileListPhotos)
        {
            ImageViewerViewModel imageViewerViewModel = new ImageViewerViewModel(userOrGroupId, isGroup, photos, canLoadMoreProfileListPhotos, 0L);
            ImageViewerDecoratorUC decoratorForCurrentPage = ImageViewerDecoratorUC.GetDecoratorForCurrentPage(null);
            ImageViewerViewModel ivvm = imageViewerViewModel;
            decoratorForCurrentPage.InitWith(ivvm, (Func<int, Image>)(i => null), null, null, null);
            int ind = selectedPhotoIndex;
            decoratorForCurrentPage.Show(ind);
        }

        private static ImageViewerDecoratorUC GetDecoratorForCurrentPage(PageBase page = null)
        {
            if (page == null)
                page = (Application.Current.RootVisual as PhoneApplicationFrame).Content as PageBase;
            return page.ImageViewerDecorator;
        }

        public void SetPage(PhoneApplicationPage page)
        {
            this._page = page;
            this.imageViewer.SetPage(this._page);
        }

        private void SaveImage(BitmapImage bitmap)
        {
            ImageHelper.SaveImage(bitmap);
        }

        private void ShowConfirmation()
        {
            new GenericInfoUC().ShowAndHideLater(CommonResources.PhotoIsSaved, null);
        }

        private void LikeUnlikeTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.CurrentPhotoVM == null)
                return;
            this.CurrentPhotoVM.LikeUnlike();
        }

        private void CommentTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.NavigateToComments();
        }

        private void UserTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.NavigateToComments();
        }

        private void NavigateToComments()
        {
            if (this.CurrentPhotoVM == null)
                return;
            Navigator.Current.NavigateToPhotoWithComments(this.CurrentPhotoVM.Photo, this.CurrentPhotoVM.PhotoWithInfo, this.CurrentPhotoVM.Photo.owner_id, this.CurrentPhotoVM.Photo.pid, this.CurrentPhotoVM.AccessKey, this._fromDialog, this._friendsOnly);
        }

        private void textBoxTextTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.NavigateToComments();
        }

        public void Handle(PhotoIsRepostedInGroup message)
        {
            if (!this._ds.IsOpen)
                return;
            this._ds.Hide();
        }

        private void LayoutRoot_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;
        }

        private void LayoutRoot_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;
        }

        private void LayoutRoot_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
        }

        private void AddTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.CurrentPhotoVM == null)
                return;
            this.CurrentPhotoVM.AddDocument();
        }

        private void ShareTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.DoShare();
        }
    }
}
