using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Video.VideoCatalog
{
  public class CreateEditVideoAlbumPage : PageBase
  {
    private bool _isInitialized;
    private long _albumId;
    private long _groupId;
    private string _name;
    private PrivacyInfo _pi;
    private ApplicationBarIconButton _appBarButtonCommit;
    private ApplicationBarIconButton _appBarButtonCancel;
    private readonly ApplicationBar _appBar;
    private bool _loaded;
    internal Grid LayoutRoot;
    internal CreateEditVideoAlbumUC ucCreateEditVideoAlbum;
    private bool _contentLoaded;

    private CreateEditVideoAlbumViewModel VM
    {
      get
      {
        return base.DataContext as CreateEditVideoAlbumViewModel;
      }
    }

    public CreateEditVideoAlbumPage()
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
      double num = 0.9;
      applicationBar.Opacity = num;
      this._appBar = applicationBar;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.BuildAppBar();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.CreateEditVideoAlbumPage_Loaded));
    }

    private void CreateEditVideoAlbumPage_Loaded(object sender, RoutedEventArgs e)
    {
      if (this._loaded)
        return;
      ((Control) this.ucCreateEditVideoAlbum.textBoxName).Focus();
      this.ucCreateEditVideoAlbum.textBoxName.SelectAll();
      this._loaded = true;
    }

    private void BuildAppBar()
    {
      this._appBarButtonCommit.Click+=(new EventHandler(this._appBarButtonCommit_Click));
      this._appBarButtonCancel.Click+=(new EventHandler(this._appBarButtonCancel_Click));
      this._appBar.Buttons.Add(this._appBarButtonCommit);
      this._appBar.Buttons.Add(this._appBarButtonCancel);
      this.ApplicationBar = ((IApplicationBar) this._appBar);
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void _appBarButtonCommit_Click(object sender, EventArgs e)
    {
        this.VM.Save((Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
        {
            if (!res)
                return;
            Navigator.Current.GoBack();
        }))));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._albumId = long.Parse(((Page) this).NavigationContext.QueryString["AlbumId"]);
        this._groupId = long.Parse(((Page) this).NavigationContext.QueryString["GroupId"]);
        this._name = ((Page) this).NavigationContext.QueryString["Name"];
        this._groupId = long.Parse(((Page) this).NavigationContext.QueryString["GroupId"]);
        this._pi = ParametersRepository.GetParameterForIdAndReset("AlbumPrivacyInfo") as PrivacyInfo;
        CreateEditVideoAlbumViewModel videoAlbumViewModel = new CreateEditVideoAlbumViewModel(this._albumId, this._groupId, this._name, this._pi);
        videoAlbumViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
        base.DataContext = videoAlbumViewModel;
        this._isInitialized = true;
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
      this._appBarButtonCommit.IsEnabled = this.VM.CanSave;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/CreateEditVideoAlbumPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucCreateEditVideoAlbum = (CreateEditVideoAlbumUC) base.FindName("ucCreateEditVideoAlbum");
    }
  }
}
