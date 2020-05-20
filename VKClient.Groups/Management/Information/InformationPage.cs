using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Information
{
    public class InformationPage : PageBase
    {
        private bool _isInitialized;
        internal GenericHeaderUC Header;
        internal ScrollViewer Viewer;
        internal StackPanel ViewerContent;
        internal TextBoxPanelControl TextBoxPanel;
        private bool _contentLoaded;

        public InformationViewModel ViewModel
        {
            get
            {
                return ((FrameworkElement)this).DataContext as InformationViewModel;
            }
        }

        public InformationPage()
        {
            this.InitializeComponent();
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            InformationViewModel viewModel = new InformationViewModel(long.Parse(((Page)this).NavigationContext.QueryString["CommunityId"]));
            ((FrameworkElement)this).DataContext = viewModel;
            ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
            Uri uri1 = new Uri("/Resources/check.png", UriKind.Relative);
            applicationBarIconButton1.IconUri = uri1;
            string appBarMenuSave = CommonResources.AppBarMenu_Save;
            applicationBarIconButton1.Text = appBarMenuSave;
            int num = 0;
            applicationBarIconButton1.IsEnabled = (num != 0);
            ApplicationBarIconButton appBarButtonSave = applicationBarIconButton1;
            ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
            Uri uri2 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
            applicationBarIconButton2.IconUri = uri2;
            string appBarCancel = CommonResources.AppBar_Cancel;
            applicationBarIconButton2.Text = appBarCancel;
            ApplicationBarIconButton applicationBarIconButton3 = applicationBarIconButton2;
            appBarButtonSave.Click += ((EventHandler)((p, f) =>
            {
                ((Control)this).Focus();
                viewModel.SaveChanges();
            }));
            applicationBarIconButton3.Click += ((EventHandler)((p, f) => Navigator.Current.GoBack()));
            this.ApplicationBar = ((IApplicationBar)ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
            viewModel.PropertyChanged += (PropertyChangedEventHandler)((p, f) => appBarButtonSave.IsEnabled = (viewModel.IsFormEnabled && viewModel.IsFormCompleted));
            this.ApplicationBar.Buttons.Add(appBarButtonSave);
            this.ApplicationBar.Buttons.Add(applicationBarIconButton3);
            InformationViewModel informationViewModel1 = viewModel;




            InformationViewModel expr_148 = viewModel;
            expr_148.OnTextBoxGotFocus = (RoutedEventHandler)Delegate.Combine(expr_148.OnTextBoxGotFocus, new RoutedEventHandler((object o, RoutedEventArgs args) =>
            {
                this.TextBoxPanel.IsOpen = true;
                Point relativePosition = ((FrameworkElement)o).GetRelativePosition(this.ViewerContent);
                this.Viewer.ScrollToOffsetWithAnimation(relativePosition.Y - 38.0, 0.2, false);
            }));
            InformationViewModel expr_16F = viewModel;
            expr_16F.OnTextBoxLostFocus = (RoutedEventHandler)Delegate.Combine(expr_16F.OnTextBoxLostFocus, new RoutedEventHandler((object o, RoutedEventArgs args) =>
            {
                this.TextBoxPanel.IsOpen = false;
            }));
            viewModel.Reload(true);
            this._isInitialized = true;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/Information/InformationPage.xaml", UriKind.Relative));
            this.Header = (GenericHeaderUC)((FrameworkElement)this).FindName("Header");
            this.Viewer = (ScrollViewer)((FrameworkElement)this).FindName("Viewer");
            this.ViewerContent = (StackPanel)((FrameworkElement)this).FindName("ViewerContent");
            this.TextBoxPanel = (TextBoxPanelControl)((FrameworkElement)this).FindName("TextBoxPanel");
        }
    }
}
