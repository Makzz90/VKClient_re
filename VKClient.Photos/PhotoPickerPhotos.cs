using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base.Utils;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Photos.ImageEditor;
using VKClient.Photos.Library;
using VKClient.Photos.Localization;
using VKClient.Photos.UC;
using Windows.Storage;

namespace VKClient.Photos
{
    public class PhotoPickerPhotos : PageBase
    {
        private static readonly double MAX_ALLOWED_VERT_SPEED = 3.0;
        private bool _isInitialized;
        private bool _pickToStorageFile;
        private bool _isConfirmed;
        private FrameworkElement _hoveredOverElement;
        private System.Windows.Point _p;
        private Image _manipImage;
        private CameraCaptureTask _cameraTask = new CameraCaptureTask();
        private bool? _selectMode;
        private bool _readyToToggle;
        private bool _firstManipulationDeltaEvent;
        private ApplicationBar _defaultAppBar;
        private ApplicationBarIconButton _appBarIconButtonConfirm;
        private ApplicationBarIconButton _appBarIconButtonAddPhoto;
        private ApplicationBarIconButton _appBarIconButtonChooseAlbum;
        private System.Windows.Point _previousTranslatedPoint;
        internal Grid LayoutRoot;
        internal Grid ContentPanel;
        internal ExtendedLongListSelector itemsControlPhotos;
        internal PickAlbumUC ucPickAlbum;
        internal GenericHeaderUC ucHeader;
        internal ImageEditorDecorator2UC imageEditor;
        private bool _contentLoaded;

        public PhotoPickerPhotosViewModel VM
        {
            get
            {
                return base.DataContext as PhotoPickerPhotosViewModel;
            }
        }

        public PhotoPickerPhotos()
        {
            ApplicationBar applicationBar = new ApplicationBar();
            Color appBarBgColor = VKConstants.AppBarBGColor;
            applicationBar.BackgroundColor = appBarBgColor;
            Color appBarFgColor = VKConstants.AppBarFGColor;
            applicationBar.ForegroundColor = appBarFgColor;
            double num = 0.9;
            applicationBar.Opacity = num;
            this._defaultAppBar = applicationBar;
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            System.Uri uri1 = new System.Uri("Resources/send.photo.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri1;
            string barConfirmChoice = CommonResources.AppBarConfirmChoice;
            applicationBarIconButton1.Text = barConfirmChoice;
            this._appBarIconButtonConfirm = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            System.Uri uri2 = new System.Uri("Resources/appbar.feature.camera.rest.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = uri2;
            string albumPageAddPhoto = PhotoResources.PhotoAlbumPage_AddPhoto;
            applicationBarIconButton2.Text = albumPageAddPhoto;
            this._appBarIconButtonAddPhoto = applicationBarIconButton2;
            ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
            System.Uri uri3 = new System.Uri("Resources/outline.squares.png", UriKind.Relative);
            applicationBarIconButton3.IconUri = uri3;
            string appBarChooseAlbum = CommonResources.AppBarChooseAlbum;
            applicationBarIconButton3.Text = appBarChooseAlbum;
            this._appBarIconButtonChooseAlbum = applicationBarIconButton3;
            // ISSUE: explicit constructor call
            //base.\u002Ector();
            this.InitializeComponent();
            this.BuildAppBar();
            this.SuppressMenu = true;
            // ISSUE: method pointer
            this.ucHeader.OnHeaderTap = delegate
                  {
                      this.itemsControlPhotos.ScrollToTop();
                  };
            this.ucHeader.InitializeMenu(this.ucPickAlbum, this.ContentPanel, delegate
                  {
                      this.ucPickAlbum.SelectedAlbumCallback = delegate(string albId)
                      {
                          this.VM.AlbumId = albId;
                          this.ucHeader.ShowHideMenu();
                      };
                  }, new Action(this.ucPickAlbum.Initialize), new Action(this.ucPickAlbum.Cleanup));
            this._cameraTask.Completed += (new EventHandler<PhotoResult>(this._cameraTask_Completed));
        }

        private void _cameraTask_Completed(object sender, PhotoResult e)
        {
            if (((TaskEventArgs)e).TaskResult != TaskResult.OK)
                return;
            ParametersRepository.SetParameterForId("CapturedPhoto", e.ChosenPhoto);
        }

        private void BuildAppBar()
        {
            this._appBarIconButtonAddPhoto.Click += (new EventHandler(this._appBarIconButtonAddPhoto_Click));
            this._appBarIconButtonConfirm.Click += (new EventHandler(this._appBarIconButtonConfirm_Click));
            this._appBarIconButtonChooseAlbum.Click += (new EventHandler(this._appBarIconButtonChooseAlbum_Click));
            this._defaultAppBar.Buttons.Add(this._appBarIconButtonConfirm);
            this._defaultAppBar.Buttons.Add(this._appBarIconButtonAddPhoto);
            this.ApplicationBar = ((IApplicationBar)this._defaultAppBar);
        }

        private void UpdateAppBar()
        {
            this._appBarIconButtonConfirm.IsEnabled = (this.VM.SelectedCount > 0);
            if (this.VM.CanTakePicture && !this._defaultAppBar.Buttons.Contains(this._appBarIconButtonAddPhoto))
            {
                this._defaultAppBar.Buttons.Insert(1, this._appBarIconButtonAddPhoto);
            }
            else
            {
                if (this.VM.CanTakePicture || !this._defaultAppBar.Buttons.Contains(this._appBarIconButtonAddPhoto))
                    return;
                this._defaultAppBar.Buttons.Remove(this._appBarIconButtonAddPhoto);
            }
        }

        private void _appBarIconButtonConfirm_Click(object sender, EventArgs e)
        {
            this.HandleConfirm();
        }

        public async void HandleConfirm()
        {
            if (this._isConfirmed)
                return;
            this._isConfirmed = true;
            List<Stream> streamList1 = new List<Stream>();
            List<Stream> streamList2 = new List<Stream>();
            List<Size> sizeList = new List<Size>();
            this.VM.ImageEditor.SuppressParseEXIF = true;
            List<Stream> photoStreams = new List<Stream>();
            foreach (AlbumPhoto selectedPhoto in this.VM.SelectedPhotos)
            {
                ImageEffectsInfo imageEffectsInfo = this.VM.ImageEditor.GetImageEffectsInfo(selectedPhoto.AlbumId, selectedPhoto.SeqNo);
                if (imageEffectsInfo.AppliedAny && AppGlobalStateManager.Current.GlobalState.SaveEditedPhotos)
                {
                    photoStreams.Add((Stream)StreamUtils.ReadFully(selectedPhoto.ImageStream));
                    selectedPhoto.ImageStream.Position = 0L;
                }
                Stream imageStream = selectedPhoto.ImageStream;
                if (imageStream != null)
                {
                    streamList1.Add(imageStream);
                    streamList2.Add(selectedPhoto.ThumbnailStream);
                    Size size = new Size();
                    if (imageEffectsInfo.CropRect == null)
                    {
                        size.Width = selectedPhoto.Width;
                        size.Height = selectedPhoto.Height;
                    }
                    sizeList.Add(size);
                }
            }
            this.SavePhotosAsync(photoStreams);
            if (!this._pickToStorageFile)
            {
                ParametersRepository.SetParameterForId("ChoosenPhotos", streamList1);
                ParametersRepository.SetParameterForId("ChoosenPhotosPreviews", streamList2);
                ParametersRepository.SetParameterForId("ChoosenPhotosSizes", sizeList);
            }
            else
            {
                List<StorageFile> filesList = new List<StorageFile>();
                List<Stream>.Enumerator enumerator2 = streamList1.GetEnumerator();
                try
                {
                    while (enumerator2.MoveNext())
                    {
                        Stream stream = enumerator2.Current;
                        StorageFile storageFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("VK_Photo_" + DateTime.Now.Ticks + ".jpg", CreationCollisionOption.ReplaceExisting);
                        StorageFile storageFile2 = storageFile;
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        Stream stream2 = await storageFile2.OpenStreamForWriteAsync();
                        try
                        {
                            await stream.CopyToAsync(stream2);
                        }
                        finally
                        {
                            if (/*num < 0 &&*/ stream2 != null)
                            {
                                stream2.Dispose();
                            }
                        }
                        stream2 = null;
                        // long arg_3A1_0 = stopwatch.ElapsedMilliseconds;
                        filesList.Add(storageFile2);
                        storageFile2 = null;
                        stopwatch = null;
                        stream = null;
                    }
                }
                finally
                {
                    //if (num < 0)
                    //{
                    enumerator2.Dispose();
                    //}
                }
                // List<Stream>.Enumerator enumerator = new List<Stream>.Enumerator();
                ParametersRepository.SetParameterForId("PickedPhotoDocuments", filesList);
                ParametersRepository.SetParameterForId("FilePickedType", 10);
                filesList = null;
            }
            this.imageEditor.Hide(true);
            Navigator.Current.GoBack();
        }

        public async Task SavePhotosAsync(List<Stream> photoStreams)
        {
            IReadOnlyList<StorageFolder> readOnlyList = await KnownFolders.PicturesLibrary.GetFoldersAsync();
            IEnumerable<StorageFolder> arg_A6_0 = readOnlyList;
            Func<StorageFolder, bool> arg_A6_1 = new Func<StorageFolder, bool>((x) => { return x.Name == "VK"; });

            StorageFolder storageFolder = Enumerable.FirstOrDefault<StorageFolder>(arg_A6_0, arg_A6_1);
            if (storageFolder == null)
            {
                storageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("VK", CreationCollisionOption.ReplaceExisting);
            }
            int num = 1;
            DateTime now = DateTime.Now;
            List<Stream>.Enumerator enumerator = photoStreams.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Stream input = enumerator.Current;
                    Stream var_6_275 = await (await storageFolder.CreateFileAsync(now.Ticks.ToString() + num.ToString() + ".jpg")).OpenStreamForWriteAsync();
                    StreamUtils.CopyStream(input, var_6_275, null, null, 0L);
                    var_6_275.Close();
                    num++;
                    input = null;
                }
            }
            finally
            {
                //int num2;
                //if (num2 < 0)
                //{
                    enumerator.Dispose();
               // }
            }
            enumerator = default(List<Stream>.Enumerator);
        }


        private void _appBarIconButtonAddPhoto_Click(object sender, EventArgs e)
        {
            ((ChooserBase<PhotoResult>)this._cameraTask).Show();
        }

        private void _appBarIconButtonChooseAlbum_Click(object sender, EventArgs e)
        {
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            // ISSUE: object of a compiler-generated type is created
            // ISSUE: variable of a compiler-generated type
            //   PhotoPickerPhotos.<>c__DisplayClass25_0 cDisplayClass250 = new PhotoPickerPhotos.<>c__DisplayClass25_0();
            // ISSUE: reference to a compiler-generated field
            //   cDisplayClass250.<>4__this = this;
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                PhotoPickerPhotosViewModel pickerPhotosViewModel = new PhotoPickerPhotosViewModel(int.Parse(((Page)this).NavigationContext.QueryString["MaxAllowedToSelect"]), bool.Parse(((Page)this).NavigationContext.QueryString["OwnPhotoPick"]));
                pickerPhotosViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
                base.DataContext = pickerPhotosViewModel;
                this._pickToStorageFile = bool.Parse(((Page)this).NavigationContext.QueryString["PickToStorageFile"]);
                this._isInitialized = true;
            }
            object parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("CapturedPhoto");
            // ISSUE: reference to a compiler-generated field
            bool haveCapturedPhoto = parameterForIdAndReset != null;
            // ISSUE: method pointer
            this.VM.LoadData(true, delegate
          {
              if (haveCapturedPhoto)
              {
                  this.SelectCapturedPhoto();
              }
          });
            this.UpdateAppBar();
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            if (e.NavigationMode != NavigationMode.Back)
                return;
            this.VM.ImageEditor.CleanupSession();
        }

        private void SelectCapturedPhoto()
        {
            if (this.VM.PhotosCount <= 0 || this.VM.RecentlyAddedImageInd < 0)
                return;
            List<AlbumPhoto> list = (this.VM.Photos)[this.VM.RecentlyAddedImageInd / 4].GetAsAlbumPhotos().ToList<AlbumPhoto>();
            int index = this.VM.RecentlyAddedImageInd % 4;
            if (index >= list.Count)
                return;
            AlbumPhoto albumPhoto = list[index];
            albumPhoto.IsSelected = true;
            this.ShowPhotoEditor(albumPhoto.SeqNo);
        }

        private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "SelectedCount") && !(e.PropertyName == "CanTakePicture"))
                return;
            this.UpdateAppBar();
        }

        private void Image_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            this.ShowPhotoEditor((frameworkElement.DataContext as AlbumPhotoHeaderFourInARow).GetPhotoByTag(frameworkElement.Tag.ToString()).SeqNo);
        }

        private void ShowPhotoEditor(int photoSeqNo)
        {
            this.imageEditor.Show(this.VM.TotalCount, this.VM.AlbumId, this.VM.TotalCount - photoSeqNo - 1, (int ind) => Enumerable.FirstOrDefault<FrameworkElement>(this.GetPhotoById(ind)) as Image, delegate(int ind, bool show)
            {
                List<FrameworkElement> photoById = this.GetPhotoById(ind);
                FrameworkElement frameworkElement = Enumerable.FirstOrDefault<FrameworkElement>(photoById);
                if (frameworkElement != null)
                {
                    frameworkElement.Opacity = ((double)(show ? 1 : 0));
                }
                if (photoById.Count > 1)
                {
                    using (IEnumerator<FrameworkElement> enumerator = Enumerable.Skip<FrameworkElement>(photoById, 1).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            FrameworkElement current = enumerator.Current;
                            current.Animate(current.Opacity, (double)(show ? 1 : 0), UIElement.OpacityProperty, 150, new int?(0), null, null);
                        }
                    }
                }
            }, this);
        }

        private List<FrameworkElement> GetPhotoById(int arg)
        {
            List<FrameworkElement> frameworkElementList = new List<FrameworkElement>();
            int num1 = arg / 4;
            int num2 = arg % 4;
            return frameworkElementList;
        }

        private void SelectUnselectTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.VM.OwnPhotoPick)
                return;
            FrameworkElement element = sender as FrameworkElement;
            this.ToggleSelection(element, (element.DataContext as AlbumPhotoHeaderFourInARow).GetPhotoByTag(element.Tag.ToString()));
        }

        private void ToggleSelection(FrameworkElement element, AlbumPhoto choosenPhoto)
        {
            if (choosenPhoto == null)
                return;
            double animateToScale = choosenPhoto.IsSelected ? 0.8 : 1.2;
            int dur = 100;
            Ellipse ellipse = ((PresentationFrameworkCollection<UIElement>)(element.Parent as Panel).Children)[2] as Ellipse;
            Image image = ((PresentationFrameworkCollection<UIElement>)(element.Parent as Panel).Children)[3] as Image;
            if (!(((UIElement)ellipse).RenderTransform is ScaleTransform))
                ((UIElement)ellipse).RenderTransform = ((Transform)new ScaleTransform());
            if (!choosenPhoto.IsSelected && this.VM.SelectedCount == this.VM.MaxAllowedToSelect)
                return;
            choosenPhoto.IsSelected = !choosenPhoto.IsSelected;
            PhotoPickerPhotos.AnimateTransform(animateToScale, dur, ((UIElement)image).RenderTransform, 20);
            PhotoPickerPhotos.AnimateTransform(animateToScale, dur, ((UIElement)ellipse).RenderTransform, 20);
        }

        public static void AnimateTransform(double animateToScale, int dur, Transform transform, int center = 20)
        {
            ScaleTransform scaleTransform = transform as ScaleTransform;
            double num1 = (double)center;
            scaleTransform.CenterX = num1;
            double num2 = (double)center;
            scaleTransform.CenterY = num2;
            ((DependencyObject)transform).Animate(1.0, animateToScale, ScaleTransform.ScaleXProperty, dur, new int?(0), (IEasingFunction)new CubicEase(), null, true);
            ((DependencyObject)transform).Animate(1.0, animateToScale, ScaleTransform.ScaleYProperty, dur, new int?(0), (IEasingFunction)new CubicEase(), null, true);
        }

        private void Image_ManipulationStarted_1(object sender, ManipulationStartedEventArgs e)
        {
            this._p = e.ManipulationOrigin;
            this._manipImage = ((RoutedEventArgs)e).OriginalSource as Image;
            this._selectMode = new bool?();
            this._readyToToggle = false;
            this._firstManipulationDeltaEvent = true;
            this._previousTranslatedPoint = e.ManipulationOrigin;
        }

        private void Image_ManipulationDelta_1(object sender, ManipulationDeltaEventArgs e)
        {
            Point point1;
            // ISSUE: explicit reference operation
            // ISSUE: variable of a reference type
            // ISSUE: explicit reference operation
            double x1 = (this._p).X;
            Point translation1 = e.CumulativeManipulation.Translation;
            // ISSUE: explicit reference operation
            double x2 = ((Point)@translation1).X;
            double num1 = x1 + x2;
            // ISSUE: explicit reference operation
            double y1 = (this._p).Y;
            Point translation2 = e.CumulativeManipulation.Translation;
            // ISSUE: explicit reference operation
            double y2 = ((Point)@translation2).Y;
            double num2 = y1 + y2;
            point1 = new Point(num1, num2);
            List<Point> pointList = new List<Point>();
            Point point2 = ((UIElement)this._manipImage).TransformToVisual(Application.Current.RootVisual).Transform(point1);
            pointList.Add(point2);
            Point previousTranslatedPoint = this._previousTranslatedPoint;
            int num3 = 5;
            Point point3;
            for (int index = 1; index < num3; ++index)
            {
                point3 = new Point();
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                point3.X = (((double)index * ((Point)@point1).X + (double)(num3 - index) * (this._previousTranslatedPoint).X) / (double)num3);
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                point3.Y = (((double)index * ((Point)@point1).Y + (double)(num3 - index) * (this._previousTranslatedPoint).Y) / (double)num3);
                Point point4 = ((UIElement)this._manipImage).TransformToVisual(Application.Current.RootVisual).Transform(point3);
                pointList.Add(point4);
            }
            this._previousTranslatedPoint = point1;
            point3 = e.DeltaManipulation.Translation;
            // ISSUE: explicit reference operation
            //  ((Point) @point3).X;
            point3 = e.DeltaManipulation.Translation;
            // ISSUE: explicit reference operation
            double y3 = ((Point)@point3).Y;
            if (!this._readyToToggle)
                this._readyToToggle = Math.Abs(y3) < PhotoPickerPhotos.MAX_ALLOWED_VERT_SPEED;
            if (!this._readyToToggle)
                return;
            if (this._firstManipulationDeltaEvent)
            {
                this.HandleHoverOverUpdate((FrameworkElement)this._manipImage);
                this._firstManipulationDeltaEvent = false;
            }
            using (List<Point>.Enumerator enumerator = pointList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    this.HandleHoverOverUpdate(VisualTreeHelper.FindElementsInHostCoordinates(enumerator.Current, Application.Current.RootVisual).FirstOrDefault<UIElement>() as FrameworkElement);
            }
        }

        private void HandleHoverOverUpdate(FrameworkElement control)
        {
            if (control == this._hoveredOverElement || !(control is Image))
                return;
            Image image1 = control as Image;
            if (((FrameworkElement)image1).Tag == null || !(((FrameworkElement)image1).DataContext is AlbumPhotoHeaderFourInARow))
                return;
            AlbumPhoto photoByTag = (control.DataContext as AlbumPhotoHeaderFourInARow).GetPhotoByTag(control.Tag.ToString());
            if (photoByTag == null)
                return;
            Image image2 = ((PresentationFrameworkCollection<UIElement>)(((FrameworkElement)image1).Parent as Panel).Children)[3] as Image;
            if (!this._selectMode.HasValue)
                this._selectMode = new bool?(!photoByTag.IsSelected);
            if (photoByTag.IsSelected != this._selectMode.Value)
                this.ToggleSelection((FrameworkElement)image2, photoByTag);
            this._hoveredOverElement = control;
        }

        private void Image_ManipulationCompleted_1(object sender, ManipulationCompletedEventArgs e)
        {
            this._hoveredOverElement = null;
        }

        private void photosLink(object sender, LinkUnlinkEventArgs e)
        {
            int count = (this.VM.Photos).Count;
            object content = e.ContentPresenter.Content;
            int num = 10;
            if ((!(content is AlbumHeaderTwoInARow) || count >= num) && (count < num || (this.VM.Photos)[count - num] != content))
                return;
            this.VM.LoadData(false, null);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new System.Uri("/VKClient.Photos;component/PhotoPickerPhotos.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.ContentPanel = (Grid)base.FindName("ContentPanel");
            this.itemsControlPhotos = (ExtendedLongListSelector)base.FindName("itemsControlPhotos");
            this.ucPickAlbum = (PickAlbumUC)base.FindName("ucPickAlbum");
            this.ucHeader = (GenericHeaderUC)base.FindName("ucHeader");
            this.imageEditor = (ImageEditorDecorator2UC)base.FindName("imageEditor");
        }
    }
}
