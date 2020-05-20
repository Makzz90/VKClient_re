using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Video.Library;
using Windows.Storage;

namespace VKClient.Video
{
    public class AddEditVideoPage : PageBase
    {
        private bool _isInitialized;
        private ApplicationBarIconButton _appBarButtonCommit;
        private ApplicationBarIconButton _appBarButtonCancel;
        private ApplicationBar _appBar;
        //private bool _isSaving;
        internal Grid LayoutRoot;
        internal GenericHeaderUC ucHeader;
        internal Grid ContentPanel;
        internal Image PreviewImage;
        internal ProgressBar ProgressUpload;
        internal TextBox textBoxName;
        internal TextBox textBoxDescription;
        internal PrivacyHeaderUC ucPrivacyHeaderView;
        internal PrivacyHeaderUC ucPrivacyHeaderComment;
        private bool _contentLoaded;

        private AddEditVideoViewModel VM
        {
            get
            {
                return base.DataContext as AddEditVideoViewModel;
            }
        }

        public AddEditVideoPage()
        {
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            Uri uri1 = new Uri("Resources/check.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri1;
            string appBarMenuSave = CommonResources.AppBarMenu_Save;
            applicationBarIconButton1.Text = appBarMenuSave;
            this._appBarButtonCommit = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            Uri uri2 = new Uri("Resources/appbar.cancel.rest.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = uri2;
            string appBarCancel = CommonResources.AppBar_Cancel;
            applicationBarIconButton2.Text = appBarCancel;
            this._appBarButtonCancel = applicationBarIconButton2;
            ApplicationBar applicationBar = new ApplicationBar();
            Color appBarBgColor = VKConstants.AppBarBGColor;
            applicationBar.BackgroundColor = appBarBgColor;
            Color appBarFgColor = VKConstants.AppBarFGColor;
            applicationBar.ForegroundColor = appBarFgColor;
            this._appBar = applicationBar;
            // ISSUE: explicit constructor call
            // base.\u002Ector();
            this.InitializeComponent();
            this.BuildAppBar();
            this.SuppressMenu = true;
            // ISSUE: method pointer
            base.Loaded += (new RoutedEventHandler(this.AddEditVideoPage_Loaded));
            this.ucPrivacyHeaderView.OnTap = new Action(this.PrivacyViewTap);
            this.ucPrivacyHeaderComment.OnTap = new Action(this.PrivacyCommentTap);
            this.ucHeader.HideSandwitchButton = true;
        }

        private void AddEditVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateAppBar();
        }

        private void PrivacyCommentTap()
        {
            Navigator.Current.NavigateToEditPrivacy(new EditPrivacyPageInputData()
            {
                PrivacyForEdit = this.VM.CommentVideoPrivacyVM,
                UpdatePrivacyCallback = (Action<PrivacyInfo>)(pi => this.VM.CommentVideoPrivacyVM = new EditPrivacyViewModel(this.VM.CommentVideoPrivacyVM.PrivacyQuestion, pi, "", (List<string>)null))
            });
        }

        private void PrivacyViewTap()
        {
            Navigator.Current.NavigateToEditPrivacy(new EditPrivacyPageInputData()
            {
                PrivacyForEdit = this.VM.ViewVideoPrivacyVM,
                UpdatePrivacyCallback = (Action<PrivacyInfo>)(pi => this.VM.ViewVideoPrivacyVM = new EditPrivacyViewModel(this.VM.ViewVideoPrivacyVM.PrivacyQuestion, pi, "", (List<string>)null))
            });
        }

        private void BuildAppBar()
        {
            this._appBarButtonCommit.Click += (new EventHandler(this._appBarCommit_Click));
            this._appBarButtonCancel.Click += (new EventHandler(this._appBarButtonCancel_Click));
            this._appBar.Buttons.Add((object)this._appBarButtonCommit);
            this._appBar.Buttons.Add((object)this._appBarButtonCancel);
            this.ApplicationBar = ((IApplicationBar)this._appBar);
        }

        private void _appBarButtonCancel_Click(object sender, EventArgs e)
        {
            if (this.VM.IsSaving)
                this.VM.Cancel();
            else
                ((Page)this).NavigationService.GoBackSafe();
        }

        private void _appBarCommit_Click(object sender, EventArgs e)
        {
            //this._isSaving = true;
            this._appBarButtonCommit.IsEnabled = false;
            this.VM.Description = this.textBoxDescription.Text;
            this.VM.Name = this.textBoxName.Text;
            ((Control)this).Focus();
            this.VM.Save((Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this._appBarButtonCommit.IsEnabled = true;
                //this._isSaving = false;
                this.UpdateAppBar();
                if (res)
                {
                    ((Page)this).NavigationService.GoBackSafe();
                }
                else
                {
                    if (this.VM.C != null && this.VM.C.IsSet)
                        return;
                    new GenericInfoUC().ShowAndHideLater(CommonResources.Error, (FrameworkElement)null);
                }
            }))));
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            if (((Page)this).NavigationContext.QueryString.ContainsKey("VideoToUploadPath"))
            {
                base.DataContext = ((object)AddEditVideoViewModel.CreateForNewVideo(((Page)this).NavigationContext.QueryString["VideoToUploadPath"], long.Parse(((Page)this).NavigationContext.QueryString["OwnerId"])));
            }
            else
            {
                long ownerId = long.Parse(((Page)this).NavigationContext.QueryString["OwnerId"]);
                long num = long.Parse(((Page)this).NavigationContext.QueryString["VideoId"]);
                VKClient.Common.Backend.DataObjects.Video parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("VideoForEdit") as VKClient.Common.Backend.DataObjects.Video;
                long videoId = num;
                VKClient.Common.Backend.DataObjects.Video video = parameterForIdAndReset;
                base.DataContext = ((object)AddEditVideoViewModel.CreateForEditVideo(ownerId, videoId, video));
            }
            this.UpdateAppBar();
            this._isInitialized = true;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            AddEditVideoViewModel.PickedExternalFile = (StorageFile)null;
        }

        private void textBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateAppBar();
        }

        private void UpdateAppBar()
        {
            this._appBarButtonCommit.IsEnabled = (!string.IsNullOrWhiteSpace(this.textBoxName.Text));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Video;component/AddEditVideoPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.ucHeader = (GenericHeaderUC)base.FindName("ucHeader");
            this.ContentPanel = (Grid)base.FindName("ContentPanel");
            this.PreviewImage = (Image)base.FindName("PreviewImage");
            this.ProgressUpload = (ProgressBar)base.FindName("ProgressUpload");
            this.textBoxName = (TextBox)base.FindName("textBoxName");
            this.textBoxDescription = (TextBox)base.FindName("textBoxDescription");
            this.ucPrivacyHeaderView = (PrivacyHeaderUC)base.FindName("ucPrivacyHeaderView");
            this.ucPrivacyHeaderComment = (PrivacyHeaderUC)base.FindName("ucPrivacyHeaderComment");
        }
    }
}
