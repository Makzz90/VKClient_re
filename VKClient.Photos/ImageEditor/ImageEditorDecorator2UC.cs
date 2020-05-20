using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.ImageViewer;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using VKClient.Photos.UC;

using ExifLib;

namespace VKClient.Photos.ImageEditor
{
    public class ImageEditorDecorator2UC : UserControl, INotifyPropertyChanged
    {
        private IEasingFunction _easing;
        private bool _inCropMode;
        private bool _filtersPanelShown;
        private IApplicationBar _savedPageAppBar;
        private PhotoPickerPhotosViewModel _pppVM;
        private PhotoPickerPhotos _pickerPage;
        private int _totalCount;
        private bool _isShown;
        private int _indToShow;
        private List<Size> _imageSizes;
        private string _albumId;
        private bool _isInSetResetCrop;
        private ImageEditorViewModel _imageEditorVM;
        private DialogService _de;
        //private BitmapImage _tempBI;
        private bool _showingImageViewer;
        private ScrollViewerOffsetMediator _scrollMediator;
        private bool _inSelectOwnPhotoArea;
        internal Grid LayoutRoot;
        internal VKClient.Common.ImageViewer.ImageViewer imageViewer;
        internal Grid gridDecorator;
        internal Ellipse elliplseSelect;
        internal Image imageSelect;
        internal Grid gridCrop;
        internal Grid gridCropLines;
        internal Grid gridFilters;
        internal ScrollViewer scrollFilters;
        internal Rectangle rectChrome;
        internal StackPanel stackPanelCrop;
        internal StackPanel stackPanelEffects;
        internal Image sendPhotosButton;
        internal Grid gridChooseThumbnail;
        private bool _contentLoaded;

        public ImageEditorViewModel ImageEditor
        {
            get
            {
                return this._imageEditorVM;
            }
        }

        public List<FilterViewModel> Filters
        {
            get
            {
                return AvailableFilters.Filters;
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
                this._isShown = value;
            }
        }

        public bool IsSelected
        {
            get
            {
                return this._pppVM != null && this._pppVM.SelectedPhotos.Any<AlbumPhoto>((Func<AlbumPhoto, bool>)(p =>
                {
                    if (p.AlbumId == this._pppVM.AlbumId)
                        return p.SeqNo == this.CurrentPhotoSeqNo;
                    return false;
                }));
            }
        }

        private AlbumPhoto CurrentPhoto
        {
            get
            {
                if (this._pppVM != null)
                {
                    foreach (AlbumPhotoHeaderFourInARow photo in (Collection<AlbumPhotoHeaderFourInARow>)this._pppVM.Photos)
                    {
                        foreach (AlbumPhoto asAlbumPhoto in photo.GetAsAlbumPhotos())
                        {
                            if (asAlbumPhoto.AlbumId == this._pppVM.AlbumId && asAlbumPhoto.SeqNo == this.CurrentPhotoSeqNo)
                                return asAlbumPhoto;
                        }
                    }
                }
                return null;
            }
        }

        private int CurrentPhotoSeqNo
        {
            get
            {
                return this._totalCount - this.imageViewer.CurrentInd - 1;
            }
        }

        public string SelectUnselectImageUri
        {
            get
            {
                return !this.IsSelected ? "/VKClient.Common;component/Resources/PhotoChooser-Check-WXGA.png" : "/VKClient.Common;component/Resources/PhotoChooser-Checked-WXGA.png";
            }
        }

        public Visibility IsSelectedVisibility
        {
            get
            {
                if (!this.IsSelected)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        private PhoneApplicationPage Page
        {
            get
            {
                return ((ContentControl)(Application.Current.RootVisual as PhoneApplicationFrame)).Content as PhoneApplicationPage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageEditorDecorator2UC()
        {
            CubicEase cubicEase = new CubicEase();
            int num = 0;
            ((EasingFunctionBase)cubicEase).EasingMode = ((EasingMode)num);
            this._easing = (IEasingFunction)cubicEase;
            this._imageSizes = new List<Size>();
            this._scrollMediator = new ScrollViewerOffsetMediator();
            //base.\u002Ector();
            this.InitializeComponent();
            ((FrameworkElement)this).DataContext = ((object)this);
            ((UIElement)this).Visibility = Visibility.Collapsed;
            this.imageViewer.HideCallback = (Action)(() => this.Hide(false));
            this.imageViewer.ChangeIndexBeforeAnimation = true;
            this.imageViewer.MaxScale = 8.0;
            this.imageViewer.CurrentIndexChanged = new Action(this.RespondToCurrentIndexChanged);
            this.imageViewer.IsInVerticalSwipeChanged = new Action(this.RespondToVertSwipeChange);
            this.imageViewer.SupportOrientationChange = false;
            this._scrollMediator.ScrollViewer = this.scrollFilters;
            if (ScaleFactor.GetScaleFactor() != 150)
                return;
            ((FrameworkElement)this.LayoutRoot).Height = (854.0);
            ((FrameworkElement)this.imageViewer).Height = (782.0);
        }

        public void Show(int totalCount, string albumId, int ind, Func<int, Image> getImageFunc, Action<int, bool> showHideOriginalImageCallback, PhotoPickerPhotos pickerPage)
        {
            if (this.IsShown)
                return;
            ((UIElement)this).Visibility = Visibility.Visible;
            this._indToShow = ind;
            if (this._pppVM != null)
                this._pppVM.PropertyChanged -= new PropertyChangedEventHandler(this.PickerVM_PropertyChanged);
            this._pppVM = pickerPage.VM;
            this._pppVM.PropertyChanged += new PropertyChangedEventHandler(this.PickerVM_PropertyChanged);
            this._pickerPage = pickerPage;
            this._totalCount = totalCount;
            this._savedPageAppBar = this.Page.ApplicationBar;
            this._albumId = albumId;
            this._imageEditorVM = this._pickerPage.VM.ImageEditor;
            ((UIElement)this.elliplseSelect).Opacity = (0.0);
            ((UIElement)this.imageSelect).Opacity = (0.0);
            this.OnPropertyChanged("ImageEditor");
            this.InitializeImageSizes();
            PhoneApplicationPage page = this.Page;
            // ISSUE: variable of the null type
            page.ApplicationBar = (null);
            EventHandler<CancelEventArgs> eventHandler = new EventHandler<CancelEventArgs>(this.Page_BackKeyPress);
            page.BackKeyPress += (eventHandler);
            this.UpdateConfirmButtonState();
            this.imageViewer.Initialize(totalCount, (Func<int, ImageInfo>)(i =>
            {
                double num1 = 0.0;
                double num2 = 0.0;
                if (this._imageSizes.Count > i)
                {
                    Size imageSize1 = this._imageSizes[i];
                    // ISSUE: explicit reference operation
                    num1 = ((Size)@imageSize1).Width;
                    Size imageSize2 = this._imageSizes[i];
                    // ISSUE: explicit reference operation
                    num2 = ((Size)@imageSize2).Height;
                }
                ImageEffectsInfo imageEffectsInfo = this._imageEditorVM.GetImageEffectsInfo(this._albumId, this._totalCount - i - 1);
                if (imageEffectsInfo != null && imageEffectsInfo.ParsedExif != null && (imageEffectsInfo.ParsedExif.Width != 0 && imageEffectsInfo.ParsedExif.Height != 0))
                {
                    num1 = (double)imageEffectsInfo.ParsedExif.Width;
                    num2 = (double)imageEffectsInfo.ParsedExif.Height;
                    if (imageEffectsInfo.ParsedExif.Orientation == ExifOrientation.TopRight || imageEffectsInfo.ParsedExif.Orientation == ExifOrientation.BottomLeft)
                    {
                        double num3 = num1;
                        num1 = num2;
                        num2 = num3;
                    }
                }
                if (imageEffectsInfo != null && imageEffectsInfo.CropRect != null && !this._inCropMode)
                {
                    num1 = (double)imageEffectsInfo.CropRect.Width;
                    num2 = (double)imageEffectsInfo.CropRect.Height;
                }
                return new ImageInfo()
                {
                    GetSourceFunc = (Func<bool, BitmapSource>)(allowBackgroundCreation => this._imageEditorVM.GetBitmapSource(this._albumId, this._totalCount - i - 1, allowBackgroundCreation)),
                    Width = num1,
                    Height = num2
                };
            }), getImageFunc, showHideOriginalImageCallback, (FrameworkElement)null, (Action<int>)null);
            this.IsShown = true;
            this.ShowViewer();
        }

        private void ShowViewer()
        {
            if (this._showingImageViewer)
                return;
            this._showingImageViewer = true;
            this.imageViewer.Show(this._indToShow, (Action)(() =>
            {
                this._showingImageViewer = false;
                this.Update();
            }), true, (BitmapImage)null);
        }

        private void PickerVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "SelectedCount"))
                return;
            this.UpdateConfirmButtonState();
        }

        private void UpdateConfirmButtonState()
        {
            ((UIElement)this.sendPhotosButton).Opacity = 1.0;
            ((UIElement)this.sendPhotosButton).IsHitTestVisible = true;
        }

        public void Hide(bool leavingPageImmediately = false)
        {
            if (!this.IsShown)
                return;
            this.Page.BackKeyPress -= (new EventHandler<CancelEventArgs>(this.Page_BackKeyPress));
            this.Page.ApplicationBar = this._savedPageAppBar;
            this.IsShown = false;
            this.Update();
            this.ShowHideStackPanelEffects(false);
            this.imageViewer.Hide((Action)(() => ((UIElement)this).Visibility = Visibility.Collapsed), leavingPageImmediately);
        }

        private void RespondToVertSwipeChange()
        {
            this.Update();
        }

        private void Update()
        {
            if (this._showingImageViewer)
                return;
            Visibility visibility = this.imageViewer.IsInVerticalSwipe ? Visibility.Collapsed : Visibility.Visible;
            if (!this.IsShown)
                visibility = Visibility.Collapsed;
            if (!this._pickerPage.VM.OwnPhotoPick)
                this.UpdateImageAndEllipseSelectOpacity(visibility == Visibility.Visible ? 1 : 0);
            this.ShowHideStackPanelEffects(this.IsShown && !this.imageViewer.IsInVerticalSwipe);
            if (this._filtersPanelShown && (!this.IsShown || this.imageViewer.IsInVerticalSwipe))
                this.ShowHideGridFilters(false);
            this.OnPropertyChanged("SelectUnselectImageUri");
            this.OnPropertyChanged("IsSelectedVisibility");
        }

        private void RespondToCurrentIndexChanged()
        {
            this.ImageEditor.SetCurrentPhoto(this._albumId, this.CurrentPhotoSeqNo);
            this.UpdateFiltersState("", null);
            this.Update();
        }

        private void InitializeImageSizes()
        {
            Stopwatch.StartNew();
            this._imageSizes.Clear();
            using (MediaLibrary mediaLibrary = new MediaLibrary())
            {
                // ISSUE: method pointer
                PictureAlbum pictureAlbum = ((IEnumerable<PictureAlbum>)mediaLibrary.RootPictureAlbum.Albums).FirstOrDefault<PictureAlbum>((Func<PictureAlbum, bool>)(a => a.Name == this._albumId));
                if ((pictureAlbum != null))
                {
                    using (IEnumerator<Picture> enumerator = pictureAlbum.Pictures.GetEnumerator())
                    {
                        while (((IEnumerator)enumerator).MoveNext())
                        {
                            Picture current = enumerator.Current;
                            List<Size> imageSizes = this._imageSizes;
                            Size size1 = new Size();
                            // ISSUE: explicit reference operation
                            size1.Width = ((double)current.Width);
                            // ISSUE: explicit reference operation
                            size1.Height = ((double)current.Height);
                            Size size2 = size1;
                            imageSizes.Add(size2);
                            current.Dispose();
                        }
                    }
                    pictureAlbum.Dispose();
                }
                this._imageSizes.Reverse();
            }
        }

        private void Page_BackKeyPress(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            if (this._de != null && this._de.IsOpen)
                this._de.Hide();
            else if (this._inCropMode)
                this.ToggleCropMode();
            else
                this.Hide(false);
        }

        private void TextEffectTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this._de = new DialogService();
            EditPhotoTextUC uc = new EditPhotoTextUC();
            ImageEffectsInfo imageEffectsInfo = this._imageEditorVM.GetImageEffectsInfo(this._albumId, this.CurrentPhotoSeqNo);
            uc.TextBoxText.Text = (imageEffectsInfo.Text ?? "");
            uc.ButtonSave.Click += (delegate(object s, RoutedEventArgs ev)
            {
                string text = uc.TextBoxText.Text;
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    this.ImageEditor.SetResetText(text, new Action<BitmapSource>((b) => { this.HandleEffectApplied(b); }));
                });
                this._de.Hide();
            });
            this._de.Child = uc;
            this._de.HideOnNavigation = false;
            this._de.Show(this.gridDecorator);
        }

        private void HandleEffectApplied(BitmapSource b)
        {
            this._pickerPage.VM.HandleEffectUpdate(this._albumId, this.CurrentPhotoSeqNo);
            this.imageViewer.CurrentImage.Source = ((ImageSource)b);
        }

        private void CropEffectTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() => this.ToggleCropMode()));
        }

        private void FixEffectTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() => this.ImageEditor.SetResetContrast(!this.ImageEditor.ContrastApplied, new Action<BitmapSource>(this.HandleEffectApplied))));
        }

        private void FilterEffectTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.ShowHideGridFilters(!this._filtersPanelShown);
        }

        private void SelectUnselectTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.ToggleSelection((FrameworkElement)(sender as Image), this.CurrentPhoto);
            this.Update();
        }

        private void ToggleSelection(FrameworkElement iconImage, AlbumPhoto choosenPhoto)
        {
            if (choosenPhoto == null)
                return;
            double animateToScale = choosenPhoto.IsSelected ? 0.8 : 1.2;
            int dur = 100;
            Ellipse ellipse = ((IEnumerable<UIElement>)((Panel)(iconImage.Parent as Grid)).Children).FirstOrDefault<UIElement>((Func<UIElement, bool>)(c => c is Ellipse)) as Ellipse;
            if (!(((UIElement)ellipse).RenderTransform is ScaleTransform))
                ((UIElement)ellipse).RenderTransform = ((Transform)new ScaleTransform());
            if (!(((UIElement)iconImage).RenderTransform is ScaleTransform))
                ((UIElement)iconImage).RenderTransform = ((Transform)new ScaleTransform());
            if (!choosenPhoto.IsSelected && this._pppVM.SelectedCount == this._pppVM.MaxAllowedToSelect)
                return;
            choosenPhoto.IsSelected = !choosenPhoto.IsSelected;
            PhotoPickerPhotos.AnimateTransform(animateToScale, dur, ((UIElement)iconImage).RenderTransform, 25);
            PhotoPickerPhotos.AnimateTransform(animateToScale, dur, ((UIElement)ellipse).RenderTransform, 25);
        }

        private void SendPhotoTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this._pickerPage.VM.OwnPhotoPick && this._imageEditorVM.GetImageEffectsInfo(this._albumId, this.CurrentPhotoSeqNo).CropRect == null)
            {
                this._inSelectOwnPhotoArea = true;
                ((UIElement)this.gridChooseThumbnail).Visibility = Visibility.Visible;
                this.ToggleCropMode();
            }
            else
                this.EnsureSelectCurrentAndConfirm();
        }

        private void OnPropertyChanged(string propertyName)
        {
            // ISSUE: reference to a compiler-generated field
            if (this.PropertyChanged == null)
                return;
            // ISSUE: reference to a compiler-generated field
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ShowHideStackPanelEffects(bool show)
        {
            TranslateTransform renderTransform1 = ((UIElement)this.stackPanelEffects).RenderTransform as TranslateTransform;
            int num = show ? 0 : 221;
            ((DependencyObject)renderTransform1).Animate(renderTransform1.Y, (double)num, TranslateTransform.YProperty, 250, new int?(0), this._easing, null, false);
            TranslateTransform renderTransform2 = ((UIElement)this.rectChrome).RenderTransform as TranslateTransform;
            ((DependencyObject)renderTransform2).Animate(renderTransform2.Y, (double)num, TranslateTransform.YProperty, 250, new int?(0), this._easing, null, false);
        }

        private void ShowHideGridFilters(bool show)
        {
            TranslateTransform renderTransform = ((UIElement)this.gridFilters).RenderTransform as TranslateTransform;
            int num = show ? 0 : 221;
            this._filtersPanelShown = show;
            ((DependencyObject)renderTransform).Animate(renderTransform.Y, (double)num, TranslateTransform.YProperty, 250, new int?(0), this._easing, null, false);
        }

        private void FilterTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FilterViewModel fvm = (sender as FrameworkElement).DataContext as FilterViewModel;
            this.UpdateFiltersState(fvm.FilterName, (Action)(() =>
            {
                if (fvm == null)
                    return;
                ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() => this.ImageEditor.SetResetFilter(fvm.FilterName, new Action<BitmapSource>(this.HandleEffectApplied))));
            }));
        }

        private void UpdateFiltersState(string forceFilterName = "", Action callback = null)
        {
            ImageEffectsInfo imageEffectsInfo = this.ImageEditor.GetImageEffectsInfo(this._albumId, this.CurrentPhotoSeqNo);
            string str = string.IsNullOrEmpty(forceFilterName) ? imageEffectsInfo.Filter : forceFilterName;
            foreach (FilterViewModel filter in this.Filters)
            {
                bool flag = str == filter.FilterName;
                filter.IsSelectedVisibility = flag ? Visibility.Visible : Visibility.Collapsed;
                if (flag)
                    this.ScrollToIndex(this.Filters.IndexOf(filter), callback);
            }
        }

        private void ScrollToIndex(int p, Action callback)
        {
            double horizontalOffset;
            double num1 = (horizontalOffset = this.scrollFilters.HorizontalOffset) + 472.0;
            double to = (double)(p * 110);
            double num2 = to + 110.0;
            if (to >= horizontalOffset && num2 <= num1)
            {
                if (callback == null)
                    return;
                callback.Invoke();
            }
            else if (to < horizontalOffset && num2 >= horizontalOffset)
                this.ScrollToOffset(to, callback);
            else if (num2 > num1 && to <= num1)
                this.ScrollToOffset(horizontalOffset + (num2 - num1), callback);
            else
                this.ScrollToOffset(to - 181.0, callback);
        }

        private void ScrollToOffset(double to, Action callback)
        {
            ScrollViewerOffsetMediator scrollMediator = this._scrollMediator;
            double horizontalOffset = this.scrollFilters.HorizontalOffset;
            double to1 = to;
            DependencyProperty horizontalOffsetProperty = ScrollViewerOffsetMediator.HorizontalOffsetProperty;
            int duration = 350;
            int? startTime = new int?(0);
            CubicEase cubicEase = new CubicEase();
            int num1 = 2;
            ((EasingFunctionBase)cubicEase).EasingMode = ((EasingMode)num1);
            Action completed = callback;
            int num2 = 0;
            ((DependencyObject)scrollMediator).Animate(horizontalOffset, to1, horizontalOffsetProperty, duration, startTime, (IEasingFunction)cubicEase, completed, num2 != 0);
        }

        private void SetCrop(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this._inSelectOwnPhotoArea)
            {
                ParametersRepository.SetParameterForId("UserPicSquare", (object)this.imageViewer.RectangleFillRelative);
                this._inSelectOwnPhotoArea = false;
                ((UIElement)this.gridChooseThumbnail).Visibility = Visibility.Collapsed;
                this.EnsureSelectCurrentAndConfirm();
            }
            else
            {
                if (this._isInSetResetCrop)
                    return;
                this._isInSetResetCrop = true;
                ParametersRepository.SetParameterForId("UserPicSquare", (object)new Rect(0.0, 0.0, 1.0, 1.0));
                Rect rectToCrop = this.imageViewer.RectangleFillInCurrentImageCoordinates;
                ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() =>
                {
                    ImageEditorViewModel imageEditorVm = this._imageEditorVM;
                    double rotate = 0.0;
                    CropRegion rect = new CropRegion();
                    // ISSUE: explicit reference operation
                    rect.X = (int)((Rect)@rectToCrop).X;
                    // ISSUE: explicit reference operation
                    rect.Y = (int)((Rect)@rectToCrop).Y;
                    // ISSUE: explicit reference operation
                    rect.Width = (int)((Rect)@rectToCrop).Width;
                    // ISSUE: explicit reference operation
                    rect.Height = (int)((Rect)@rectToCrop).Height;
                    WriteableBitmap source = this.imageViewer.CurrentImage.Source as WriteableBitmap;
                    Action<BitmapSource> callback = (Action<BitmapSource>)(result =>
                    {
                        this._isInSetResetCrop = false;
                        this.HandleEffectApplied(result);
                        this.ToggleCropMode();
                        ((UIElement)this.imageViewer.CurrentImage).RenderTransform = ((Transform)RectangleUtils.TransformRect(this.imageViewer.CurrentImageFitRectOriginal, this.imageViewer.RectangleFill, false));
                        this.imageViewer.AnimateImage(1.0, 1.0, 0.0, 0.0, (Action)null);
                    });
                    imageEditorVm.SetCrop(rotate, rect, source, callback);
                }));
            }
        }

        private void EnsureSelectCurrentAndConfirm()
        {
            if (this._pickerPage.VM.SelectedCount == 0)
                this.CurrentPhoto.IsSelected = true;
            this._pickerPage.HandleConfirm();
        }

        private void ResetCrop(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this._isInSetResetCrop)
                return;
            this._isInSetResetCrop = true;
            ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() => this._imageEditorVM.ResetCrop((Action<BitmapSource>)(result =>
            {
                this._isInSetResetCrop = false;
                this.HandleEffectApplied(result);
                this.imageViewer.AnimateImage(1.0, 1.0, 0.0, 0.0, (Action)null);
                this.ToggleCropMode();
            }))));
        }

        private void ToggleCropMode()
        {
            if (this._isInSetResetCrop)
                return;
            if (this._inCropMode)
            {
                this._inCropMode = false;
                this._inSelectOwnPhotoArea = false;
                ((UIElement)this.gridChooseThumbnail).Visibility = Visibility.Collapsed;
                ((UIElement)this.gridCropLines).Visibility = Visibility.Collapsed;
                ((UIElement)this.gridCrop).Visibility = Visibility.Collapsed;
                this.imageViewer.Mode = ImageViewerMode.Normal;
                ((UIElement)this.stackPanelEffects).Visibility = Visibility.Visible;
                ((UIElement)this.stackPanelCrop).Visibility = Visibility.Collapsed;
                this.UpdateImageAndEllipseSelectOpacity(1);
            }
            else
            {
                this._inCropMode = true;
                Picture galleryImage = this._imageEditorVM.GetGalleryImage(this._albumId, this.CurrentPhotoSeqNo);
                bool rotated90 = false;
                Size correctImageSize = this._imageEditorVM.GetCorrectImageSize(galleryImage, this._albumId, this.CurrentPhotoSeqNo, out rotated90);
                BitmapImage bitmapImage = new BitmapImage();
                Point point = new Point();
                Size viewportSize = this._imageEditorVM.ViewportSize;
                // ISSUE: explicit reference operation
                double num1 = ((Size)@viewportSize).Width * 2.0;
                viewportSize = this._imageEditorVM.ViewportSize;
                // ISSUE: explicit reference operation
                double num2 = ((Size)@viewportSize).Height * 2.0;
                Size size = new Size(num1, num2);
                Rect fit1 = RectangleUtils.ResizeToFit(new Rect(point, size), correctImageSize);
                // ISSUE: explicit reference operation
                if (((Rect)@fit1).Height < (double)galleryImage.Height)
                {
                    // ISSUE: explicit reference operation
                    // ISSUE: explicit reference operation
                    bitmapImage.DecodePixelHeight = (rotated90 ? (int)((Rect)@fit1).Height : (int)((Rect)@fit1).Width);
                }
                ((BitmapSource)bitmapImage).SetSource(galleryImage.GetImage());
                this.imageViewer.CurrentImage.Source = ((ImageSource)this._imageEditorVM.RotateIfNeeded(this._albumId, this.CurrentPhotoSeqNo, new WriteableBitmap((BitmapSource)bitmapImage)));
                this.imageViewer.RectangleFill = ScaleFactor.GetScaleFactor() != 150 ? new Rect(12.0, 136.0, 456.0, 456.0) : new Rect(12.0, 163.0, 456.0, 456.0);
                this.imageViewer.Mode = ImageViewerMode.RectangleFill;
                ImageEffectsInfo imageEffectsInfo = this._imageEditorVM.GetImageEffectsInfo(this._albumId, this.CurrentPhotoSeqNo);
                if (imageEffectsInfo.CropRect != null)
                {
                    Rect rect1 = new Rect();
                    // ISSUE: explicit reference operation
                    rect1.X = ((double)imageEffectsInfo.CropRect.X);
                    // ISSUE: explicit reference operation
                    rect1.Y = ((double)imageEffectsInfo.CropRect.Y);
                    // ISSUE: explicit reference operation
                    rect1.Width = ((double)imageEffectsInfo.CropRect.Width);
                    // ISSUE: explicit reference operation
                    rect1.Height = ((double)imageEffectsInfo.CropRect.Height);
                    Rect rect2 = rect1;
                    Rect fit2 = RectangleUtils.ResizeToFit(new Rect(new Point(), new Size(((FrameworkElement)this.imageViewer).Width, ((FrameworkElement)this.imageViewer).Height)), correctImageSize);
                    ((UIElement)this.imageViewer.CurrentImage).RenderTransform = ((Transform)RectangleUtils.TransformRect(((GeneralTransform)RectangleUtils.TransformRect(new Rect(new Point(), correctImageSize), fit2, false)).TransformBounds(rect2), this.imageViewer.RectangleFill, false));
                }
                else
                    this.imageViewer.AnimateToRectangleFill();
                galleryImage.Dispose();
                this.ShowHideGridFilters(false);
                ((UIElement)this.gridCropLines).Visibility = Visibility.Visible;
                ((UIElement)this.gridCrop).Visibility = Visibility.Visible;
                ((UIElement)this.stackPanelEffects).Visibility = Visibility.Collapsed;
                ((UIElement)this.stackPanelCrop).Visibility = Visibility.Visible;
                this.UpdateImageAndEllipseSelectOpacity(0);
            }
        }

        private void UpdateImageAndEllipseSelectOpacity(int op)
        {
            if (this._pickerPage.VM.OwnPhotoPick)
                return;
            ((DependencyObject)this.elliplseSelect).Animate(((UIElement)this.elliplseSelect).Opacity, (double)op, UIElement.OpacityProperty, 150, new int?(0), null, null);
            ((DependencyObject)this.imageSelect).Animate(((UIElement)this.imageSelect).Opacity, (double)op, UIElement.OpacityProperty, 150, new int?(0), null, null);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Photos;component/ImageEditor/ImageEditorDecorator2UC.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.imageViewer = (VKClient.Common.ImageViewer.ImageViewer)base.FindName("imageViewer");
            this.gridDecorator = (Grid)base.FindName("gridDecorator");
            this.elliplseSelect = (Ellipse)base.FindName("elliplseSelect");
            this.imageSelect = (Image)base.FindName("imageSelect");
            this.gridCrop = (Grid)base.FindName("gridCrop");
            this.gridCropLines = (Grid)base.FindName("gridCropLines");
            this.gridFilters = (Grid)base.FindName("gridFilters");
            this.scrollFilters = (ScrollViewer)base.FindName("scrollFilters");
            this.rectChrome = (Rectangle)base.FindName("rectChrome");
            this.stackPanelCrop = (StackPanel)base.FindName("stackPanelCrop");
            this.stackPanelEffects = (StackPanel)base.FindName("stackPanelEffects");
            this.sendPhotosButton = (Image)base.FindName("sendPhotosButton");
            this.gridChooseThumbnail = (Grid)base.FindName("gridChooseThumbnail");
        }
    }
}
