using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class SettingsChangeShortNamePage : PageBase
  {
    private bool _isInitialized;
    private ApplicationBarIconButton _appBarButtonCheck;
    private ApplicationBarIconButton _appBarButtonCancel;
    private ApplicationBar _appBar;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal Grid ContentPanel;
    internal TextBox textBoxName;
    internal ContextMenu AtNameMenu;
    internal ContextMenu LinkMenu;
    private bool _contentLoaded;

    private SettingsChangeShortNameViewModel VM
    {
      get
      {
        return base.DataContext as SettingsChangeShortNameViewModel;
      }
    }

    public SettingsChangeShortNamePage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string chatEditAppBarSave = CommonResources.ChatEdit_AppBar_Save;
      applicationBarIconButton1.Text = chatEditAppBarSave;
      this._appBarButtonCheck = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string appBarCancel = CommonResources.AppBar_Cancel;
      applicationBarIconButton2.Text = appBarCancel;
      this._appBarButtonCancel = applicationBarIconButton2;
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar.Opacity = num;
      this._appBar = applicationBar;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.BuildAppBar();
      this.ucHeader.TextBlockTitle.Text = (CommonResources.Settings_ShortName.ToUpperInvariant());
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.SettingsChangeShortNamePage_Loaded));
      this.ucHeader.HideSandwitchButton = true;
      this.SuppressMenu = true;
    }

    private void SettingsChangeShortNamePage_Loaded(object sender, RoutedEventArgs e)
    {
      ((Control) this.textBoxName).Focus();
      this.textBoxName.Select(this.textBoxName.Text.Length, 0);
      // ISSUE: method pointer
      base.Loaded-=(new RoutedEventHandler( this.SettingsChangeShortNamePage_Loaded));
    }

    private void BuildAppBar()
    {
      this._appBarButtonCheck.Click+=(new EventHandler(this._appBarButtonCheck_Click));
      this._appBarButtonCancel.Click+=(new EventHandler(this._appBarButtonCancel_Click));
      this._appBar.Buttons.Add(this._appBarButtonCheck);
      this._appBar.Buttons.Add(this._appBarButtonCancel);
      this.ApplicationBar = ((IApplicationBar) this._appBar);
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void _appBarButtonCheck_Click(object sender, EventArgs e)
    {
      if (!this.VM.CanSave)
        return;
      this.VM.SaveShortName((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() => this.UpdateAppBar()))));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        SettingsChangeShortNameViewModel shortNameViewModel = new SettingsChangeShortNameViewModel(((Page) this).NavigationContext.QueryString["CurrentShortName"]);
        base.DataContext = shortNameViewModel;
        this._isInitialized = true;
        shortNameViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
      }
      this.UpdateAppBar();
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "CanSave"))
        return;
      this.UpdateAppBar();
    }

    private void UpdateAppBar()
    {
      this._appBarButtonCheck.IsEnabled = this.VM.CanSave;
    }

    private void textBoxName_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
    }

    private void UpdateSource(TextBox textBox)
    {
      ((FrameworkElement) textBox).GetBindingExpression((DependencyProperty) TextBox.TextProperty).UpdateSource();
    }

    private void CopyAtName(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(this.VM.AtShortName);
    }

    private void AtNameTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.AtNameMenu.IsOpen = true;
    }

    private void CopyLink(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(this.VM.YourLink);
    }

    private void CopyLinkTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.LinkMenu.IsOpen = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/SettingsChangeShortNamePage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
      this.textBoxName = (TextBox) base.FindName("textBoxName");
      this.AtNameMenu = (ContextMenu) base.FindName("AtNameMenu");
      this.LinkMenu = (ContextMenu) base.FindName("LinkMenu");
    }
  }
}
