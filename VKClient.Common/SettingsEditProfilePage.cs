using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Framework.DatePicker;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
    public class SettingsEditProfilePage : PageBase
    {
        private bool _isInitialized;
        private readonly ApplicationBarIconButton _appBarButtonCheck;
        private readonly ApplicationBarIconButton _appBarButtonCancel;
        private ApplicationBar _mainAppBar;
        internal GenericHeaderUC ucHeader;
        internal ScrollViewer scrollViewer;
        internal StackPanel stackPanel;
        internal ContextMenu PhotoMenu;
        internal MyDatePicker datePicker;
        internal TextBoxPanelControl textBoxPanel;
        private bool _contentLoaded;
        //
        internal RectangleGeometry rectangleGeometry;
        internal RectangleGeometry rectangleGeometry2;
        internal RectangleGeometry rectangleGeometry3;

        private SettingsEditProfileViewModel VM
        {
            get
            {
                return base.DataContext as SettingsEditProfileViewModel;
            }
        }

        public SettingsEditProfilePage()
        {
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            Uri uri1 = new Uri("/Resources/check.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri1;
            string appBarMenuSave = CommonResources.AppBarMenu_Save;
            applicationBarIconButton1.Text = appBarMenuSave;
            this._appBarButtonCheck = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            Uri uri2 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = uri2;
            string appBarCancel = CommonResources.AppBar_Cancel;
            applicationBarIconButton2.Text = appBarCancel;
            this._appBarButtonCancel = applicationBarIconButton2;
            // ISSUE: explicit constructor call
            //base.\u002Ector();
            this.InitializeComponent();
            this.ucHeader.TextBlockTitle.Text = (CommonResources.Settings_EditProfile_Title.ToUpperInvariant());
            this.BuildAppBar();
            //
            this.rectangleGeometry.RadiusX = this.rectangleGeometry.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry2.RadiusX = this.rectangleGeometry2.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry2.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry3.RadiusX = this.rectangleGeometry3.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry3.Rect.Width / 10.0 / 2.0;
        }

        private void BuildAppBar()
        {
            this._appBarButtonCheck.Click += (new EventHandler(this._appBarButtonCheck_Click));
            this._appBarButtonCancel.Click += (new EventHandler(this._appBarButtonCancel_Click));
            this._mainAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
            this._mainAppBar.Buttons.Add(this._appBarButtonCheck);
            this._mainAppBar.Buttons.Add(this._appBarButtonCancel);
            this.ApplicationBar = ((IApplicationBar)this._mainAppBar);
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            this.textBoxPanel.IsOpen = true;
            Point relativePosition = ((UIElement)sender).GetRelativePosition((UIElement)this.stackPanel);
            this.scrollViewer.ScrollToOffsetWithAnimation(relativePosition.Y - 38.0, 0.2, false);
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            this.textBoxPanel.IsOpen = false;
        }

        private void _appBarButtonCheck_Click(object sender, EventArgs e)
        {
            this.VM.Save();
        }

        private void _appBarButtonCancel_Click(object sender, EventArgs e)
        {
            Navigator.Current.GoBack();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                SettingsEditProfileViewModel profileViewModel = new SettingsEditProfileViewModel();
                profileViewModel.Reload(true);
                profileViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
                base.DataContext = profileViewModel;
                this.UpdateAppBar();
                this._isInitialized = true;
            }
            this.HandleInputParams();
        }

        private void HandleInputParams()
        {
            List<User> parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("SelectedUsers") as List<User>;
            if (parameterForIdAndReset1 != null && parameterForIdAndReset1.Count >= 1)
                this.VM.Partner = parameterForIdAndReset1[0];
            List<Stream> parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
            Rect rect = new Rect();
            if (ParametersRepository.Contains("UserPicSquare"))
                rect = (Rect)ParametersRepository.GetParameterForIdAndReset("UserPicSquare");
            if (parameterForIdAndReset2 == null || parameterForIdAndReset2.Count <= 0)
                return;
            this.VM.UploadUserPhoto(parameterForIdAndReset2[0], rect);
        }

        private void UpdateAppBar()
        {
            this._appBarButtonCheck.IsEnabled = this.VM.CanSave;
        }

        private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "CanSave"))
                return;
            this.UpdateAppBar();
        }

        private void textBoxFirstNameChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateSource(sender as TextBox);
        }

        private void textBoxLastNameChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateSource(sender as TextBox);
        }

        private void UpdateSource(TextBox textBox)
        {
            ((FrameworkElement)textBox).GetBindingExpression((DependencyProperty)TextBox.TextProperty).UpdateSource();
        }

        private void BirthdayTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            typeof(Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, Type.DefaultBinder, this.datePicker, null);
        }

        private void RemovePartnerTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.Partner = null;
        }

        private void ChoosePartnerTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            int sexFilter = 0;
            if (this.VM.RelationshipType.id == 3 || this.VM.RelationshipType.id == 4)
                sexFilter = this.VM.IsMale ? 1 : 2;
            Navigator.Current.NavigateToPickUser(false, 0, true, 0, PickUserMode.PickForPartner, CommonResources.Settings_EditProfile_ChooseAPartner2, sexFilter, false);
        }

        private void CancelNameRequestButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.VM.CancelNameRequest();
        }

        private void ChoosePhotoTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.DoChoosePhoto();
        }

        private void ChosePhotoMenuClick(object sender, RoutedEventArgs e)
        {
            this.DoChoosePhoto();
        }

        private void DoChoosePhoto()
        {
            Navigator.Current.NavigateToPhotoPickerPhotos(1, true, false);
        }

        private void DeletePhotoMenuClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(CommonResources.DeleteConfirmation, CommonResources.DeleteOnePhoto, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;
            this.VM.DeletePhoto();
        }

        private void GridPhotoTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            this.PhotoMenu.IsOpen = true;
        }

        private void CountryPicker_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            CountryPickerUC.Show(this.VM.Country, true, (Action<Country>)(country => this.VM.Country = country), null);
        }

        private void CityPicker_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.VM.Country == null || this.VM.Country.id < 1L)
                return;
            CityPickerUC.Show(this.VM.Country.id, this.VM.City, true, (Action<City>)(city => this.VM.City = city), null);
        }

        private void PartnerTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.VM.Partner == null)
                return;
            Navigator.Current.NavigateToUserProfile(this.VM.Partner.id, this.VM.Partner.Name, "", false);
        }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Key.IsDigit())
                return;
            e.Handled = true;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/SettingsEditProfilePage.xaml", UriKind.Relative));
            this.ucHeader = (GenericHeaderUC)base.FindName("ucHeader");
            this.scrollViewer = (ScrollViewer)base.FindName("scrollViewer");
            this.stackPanel = (StackPanel)base.FindName("stackPanel");
            this.PhotoMenu = (ContextMenu)base.FindName("PhotoMenu");
            this.datePicker = (MyDatePicker)base.FindName("datePicker");
            this.textBoxPanel = (TextBoxPanelControl)base.FindName("textBoxPanel");
            //
            this.rectangleGeometry = (RectangleGeometry)base.FindName("rectangleGeometry");
            this.rectangleGeometry2 = (RectangleGeometry)base.FindName("rectangleGeometry2");
            this.rectangleGeometry3 = (RectangleGeometry)base.FindName("rectangleGeometry3");
        }
    }
}
