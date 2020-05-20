using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Video.VideoCatalog
{
    public partial class CreateEditVideoAlbumPage : PageBase
  {
    private ApplicationBarIconButton _appBarButtonCommit = new ApplicationBarIconButton()
    {
      IconUri = new Uri("Resources/check.png", UriKind.Relative),
      Text = CommonResources.AppBarMenu_Save
    };
    private ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton()
    {
      IconUri = new Uri("Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Cancel
    };
    private readonly ApplicationBar _appBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor,
      Opacity = 0.9
    };
    private bool _isInitialized;
    private long _albumId;
    private long _groupId;
    private string _name;
    private PrivacyInfo _pi;
    private bool _loaded;

    private CreateEditVideoAlbumViewModel VM
    {
      get
      {
        return this.DataContext as CreateEditVideoAlbumViewModel;
      }
    }

    public CreateEditVideoAlbumPage()
    {
      this.InitializeComponent();
      this.BuildAppBar();
      this.Loaded += new RoutedEventHandler(this.CreateEditVideoAlbumPage_Loaded);
    }

    private void CreateEditVideoAlbumPage_Loaded(object sender, RoutedEventArgs e)
    {
      if (this._loaded)
        return;
      this.ucCreateEditVideoAlbum.textBoxName.Focus();
      this.ucCreateEditVideoAlbum.textBoxName.SelectAll();
      this._loaded = true;
    }

    private void BuildAppBar()
    {
      this._appBarButtonCommit.Click += new EventHandler(this._appBarButtonCommit_Click);
      this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
      this._appBar.Buttons.Add((object) this._appBarButtonCommit);
      this._appBar.Buttons.Add((object) this._appBarButtonCancel);
      this.ApplicationBar = (IApplicationBar) this._appBar;
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void _appBarButtonCommit_Click(object sender, EventArgs e)
    {
      this.VM.Save((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
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
        this._albumId = long.Parse(this.NavigationContext.QueryString["AlbumId"]);
        this._groupId = long.Parse(this.NavigationContext.QueryString["GroupId"]);
        this._name = this.NavigationContext.QueryString["Name"];
        this._groupId = long.Parse(this.NavigationContext.QueryString["GroupId"]);
        this._pi = ParametersRepository.GetParameterForIdAndReset("AlbumPrivacyInfo") as PrivacyInfo;
        CreateEditVideoAlbumViewModel videoAlbumViewModel = new CreateEditVideoAlbumViewModel(this._albumId, this._groupId, this._name, this._pi);
        videoAlbumViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
        this.DataContext = (object) videoAlbumViewModel;
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
  }
}
