using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
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
  public class BlockEditingPage : PageBase
  {
    private bool _isInitialized;
    internal GenericHeaderUC Header;
    internal ScrollViewer Viewer;
    internal StackPanel ViewerContent;
    internal TextBoxPanelControl TextBoxPanel;
    private bool _contentLoaded;

    public BlockEditingViewModel ViewModel
    {
      get
      {
        return base.DataContext as BlockEditingViewModel;
      }
    }

    public BlockEditingPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        BlockEditingViewModel viewModel = new BlockEditingViewModel(long.Parse(((Page) this).NavigationContext.QueryString["CommunityId"]), (User) ParametersRepository.GetParameterForIdAndReset("CommunityManagementBlockEditingUser"), (User) ParametersRepository.GetParameterForIdAndReset("CommunityManagementBlockEditingManager"), bool.Parse(((Page) this).NavigationContext.QueryString["IsEditing"]), bool.Parse(((Page) this).NavigationContext.QueryString["IsOpenedWithoutPicker"]));
        base.DataContext = viewModel;
        ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
        Uri uri1 = new Uri("/Resources/check.png", UriKind.Relative);
        applicationBarIconButton1.IconUri = uri1;
        string appBarMenuSave = CommonResources.AppBarMenu_Save;
        applicationBarIconButton1.Text = appBarMenuSave;
        ApplicationBarIconButton appBarButtonSave = applicationBarIconButton1;
        ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
        Uri uri2 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
        applicationBarIconButton2.IconUri = uri2;
        string appBarCancel = CommonResources.AppBar_Cancel;
        applicationBarIconButton2.Text = appBarCancel;
        ApplicationBarIconButton applicationBarIconButton3 = applicationBarIconButton2;
        appBarButtonSave.Click+=((EventHandler) ((p, f) =>
        {
          ((Control) this).Focus();
          viewModel.SaveChanges(((Page) this).NavigationService);
        }));
        applicationBarIconButton3.Click+=((EventHandler) ((p, f) => Navigator.Current.GoBack()));
        this.ApplicationBar = ((IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
        viewModel.PropertyChanged += (PropertyChangedEventHandler) ((p, f) => appBarButtonSave.IsEnabled = viewModel.IsFormEnabled);
        this.ApplicationBar.Buttons.Add(appBarButtonSave);
        this.ApplicationBar.Buttons.Add(applicationBarIconButton3);
        this._isInitialized = true;
      }
      else
      {
        if (!ParametersRepository.Contains("BlockDurationUnixTime"))
          return;
        this.ViewModel.UpdateDuration((int) ParametersRepository.GetParameterForIdAndReset("BlockDurationUnixTime"));
      }
    }

    private void ManagerName_OnClicked(object sender, RoutedEventArgs e)
    {
      this.ViewModel.GoToManagerProfile();
    }

    private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      ((Control) this).Focus();
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((FrameworkElement) sender).GetBindingExpression((DependencyProperty) TextBox.TextProperty).UpdateSource();
    }

    private void BlockDurationPicker_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      Navigator.Current.NavigateToCommunityManagementBlockDurationPicker(this.ViewModel.DurationUnixTime);
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = true;
      Point relativePosition = ((UIElement) sender).GetRelativePosition((UIElement) this.ViewerContent);
      // ISSUE: explicit reference operation
      this.Viewer.ScrollToOffsetWithAnimation(((Point) @relativePosition).Y - 38.0, 0.2, false);
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
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/BlockEditingPage.xaml", UriKind.Relative));
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.Viewer = (ScrollViewer) base.FindName("Viewer");
      this.ViewerContent = (StackPanel) base.FindName("ViewerContent");
      this.TextBoxPanel = (TextBoxPanelControl) base.FindName("TextBoxPanel");
    }
  }
}
