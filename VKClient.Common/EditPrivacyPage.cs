using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
//
using System.Windows.Media;

namespace VKClient.Common
{
    public class EditPrivacyPage : PageBase
    {
        private bool _isInitialized;
        private EditPrivacyPageInputData _inputData;
        internal Grid LayoutRoot;
        internal GenericHeaderUC Header;
        internal Grid ContentPanel;
        internal ExtendedLongListSelector mainList;
        private bool _contentLoaded;
        //
        //internal RectangleGeometry rectangleGeometry;

        private EditPrivacyViewModel VM
        {
            get
            {
                return base.DataContext as EditPrivacyViewModel;
            }
        }

        public EditPrivacyPage()
        {
            this.InitializeComponent();
            this.Header.TextBlockTitle.Text = (CommonResources.Privacy_Title.ToUpperInvariant());
            this.Header.HideSandwitchButton = true;
            this.SuppressMenu = true;
            //
            //this.rectangleGeometry.RadiusX = this.rectangleGeometry.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry.Rect.Width / 10.0 / 2.0;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                this._inputData = ParametersRepository.GetParameterForIdAndReset("EditPrivacyInputData") as EditPrivacyPageInputData;
                if (this._inputData == null)
                {
                    Navigator.Current.GoBack();
                }
                else
                {
                    base.DataContext = (new EditPrivacyViewModel(this._inputData.PrivacyForEdit, this._inputData.UpdatePrivacyCallback));
                    this._isInitialized = true;
                }
            }
            this.HandleInputParams();
        }

        protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
        {
            base.HandleOnNavigatedFrom(e);
            if (e.NavigationMode != NavigationMode.Back || this._inputData == null || (this.VM == null || !this.VM.IsOKState))
                return;
            this._inputData.UpdatePrivacyCallback(this.VM.GetAsPrivacyInfo());
        }

        private void HandleInputParams()
        {
            List<User> parameterForIdAndReset1 = ParametersRepository.GetParameterForIdAndReset("SelectedUsers") as List<User>;
            List<FriendsList> parameterForIdAndReset2 = ParametersRepository.GetParameterForIdAndReset("SelectedLists") as List<FriendsList>;
            if (this.VM == null || parameterForIdAndReset2 == null && parameterForIdAndReset1 == null)
                return;
            this.VM.HandlePickedUsers(parameterForIdAndReset1, parameterForIdAndReset2);
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
        }

        private void PickMoreUsersTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Group<FriendHeader> dataContext = (((RoutedEventArgs)e).OriginalSource as FrameworkElement).DataContext as Group<FriendHeader>;
            if (dataContext == null || this.VM == null)
                return;
            this.VM.InitiatePickUsersFor(dataContext);
        }

        private void RemoveUserOrListTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FriendHeader dataContext = (((RoutedEventArgs)e).OriginalSource as FrameworkElement).DataContext as FriendHeader;
            if (this.VM == null)
                return;
            this.VM.Remove(dataContext);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/EditPrivacyPage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)base.FindName("LayoutRoot");
            this.Header = (GenericHeaderUC)base.FindName("Header");
            this.ContentPanel = (Grid)base.FindName("ContentPanel");
            this.mainList = (ExtendedLongListSelector)base.FindName("mainList");
            //
            //this.rectangleGeometry = (RectangleGeometry)base.FindName("rectangleGeometry");
        }
    }
}
