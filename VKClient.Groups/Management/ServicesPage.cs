using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
    public class ServicesPage : PageBase
    {
        private bool _isInitialized;
        internal GenericHeaderUC Header;
        internal ScrollViewer Viewer;
        internal StackPanel ViewerContent;
        internal TextBoxPanelControl TextBoxPanel;
        private bool _contentLoaded;

        public ServicesViewModel ViewModel
        {
            get
            {
                return base.DataContext as ServicesViewModel;
            }
        }

        public ServicesPage()
        {
            this.InitializeComponent();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                ServicesViewModel viewModel = new ServicesViewModel(long.Parse(((Page)this).NavigationContext.QueryString["CommunityId"]));
                base.DataContext = viewModel;
                ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
                Uri uri1 = new Uri("/Resources/check.png", UriKind.Relative);
                applicationBarIconButton1.IconUri = uri1;
                string appBarMenuSave = CommonResources.AppBarMenu_Save;
                applicationBarIconButton1.Text = appBarMenuSave;
                ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
                ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
                Uri uri2 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
                applicationBarIconButton3.IconUri = uri2;
                string appBarCancel = CommonResources.AppBar_Cancel;
                applicationBarIconButton3.Text = appBarCancel;
                ApplicationBarIconButton applicationBarIconButton4 = applicationBarIconButton3;
                applicationBarIconButton2.Click += ((EventHandler)((p, f) =>
                {
                    ((Control)this).Focus();
                    viewModel.SaveChanges();
                }));
                applicationBarIconButton4.Click += ((EventHandler)((p, f) => Navigator.Current.GoBack()));
                this.ApplicationBar = ((IApplicationBar)ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
                this.ApplicationBar.Buttons.Add(applicationBarIconButton2);
                this.ApplicationBar.Buttons.Add(applicationBarIconButton4);
                viewModel.Reload(true);
                this._isInitialized = true;
            }
            else
            {
                if (!ParametersRepository.Contains("CommunityManagementService"))
                    return;
                CommunityService parameterForIdAndReset1 = (CommunityService)ParametersRepository.GetParameterForIdAndReset("CommunityManagementService");
                CommunityServiceState parameterForIdAndReset2 = (CommunityServiceState)ParametersRepository.GetParameterForIdAndReset("CommunityManagementServiceNewState");
                switch (parameterForIdAndReset1)
                {
                    case CommunityService.Wall:
                        this.ViewModel.WallOrComments = parameterForIdAndReset2;
                        break;
                    case CommunityService.Photos:
                        this.ViewModel.Photos = parameterForIdAndReset2;
                        break;
                    case CommunityService.Videos:
                        this.ViewModel.Videos = parameterForIdAndReset2;
                        break;
                    case CommunityService.Audios:
                        this.ViewModel.Audios = parameterForIdAndReset2;
                        break;
                    case CommunityService.Documents:
                        this.ViewModel.Documents = parameterForIdAndReset2;
                        break;
                    case CommunityService.Discussions:
                        this.ViewModel.Discussions = parameterForIdAndReset2;
                        break;
                }
            }
        }

        private void WallOption_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Wall, this.ViewModel.WallOrComments);
        }

        private void PhotosOption_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Photos, this.ViewModel.Photos);
        }

        private void VideosOption_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Videos, this.ViewModel.Videos);
        }

        private void AudiosOption_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Audios, this.ViewModel.Audios);
        }

        private void DocumentsOption_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Documents, this.ViewModel.Documents);
        }

        private void DiscussionsOption_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToCommunityManagementServiceSwitch(CommunityService.Discussions, this.ViewModel.Discussions);
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ((FrameworkElement)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            this.TextBoxPanel.IsOpen = true;
            Point relativePosition = ((UIElement)sender).GetRelativePosition(this.ViewerContent);
            // ISSUE: explicit reference operation
            this.Viewer.ScrollToOffsetWithAnimation(relativePosition.Y - 38.0, 0.2, false);
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            this.TextBoxPanel.IsOpen = false;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/ServicesPage.xaml", UriKind.Relative));
            this.Header = (GenericHeaderUC)base.FindName("Header");
            this.Viewer = (ScrollViewer)base.FindName("Viewer");
            this.ViewerContent = (StackPanel)base.FindName("ViewerContent");
            this.TextBoxPanel = (TextBoxPanelControl)base.FindName("TextBoxPanel");
        }
    }
}
