using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Video.VideoCatalog
{
    public partial class AddToAlbumPage : PageBase
  {
    private ApplicationBarIconButton _appBarButtonCommit = new ApplicationBarIconButton()
    {
      IconUri = new Uri("Resources/check.png", UriKind.Relative),
      Text = CommonResources.AppBarMenu_Save
    };
    private readonly ApplicationBarIconButton _addAlbumButton = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Add
    };
    private ApplicationBar _appBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor,
      Opacity = 0.9
    };
    private bool _isInitialized;
    private long _videoId;

    private AddToAlbumViewModel VM
    {
      get
      {
        return this.DataContext as AddToAlbumViewModel;
      }
    }

    public AddToAlbumPage()
    {
      this.InitializeComponent();
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      this._appBarButtonCommit.Click += new EventHandler(this._appBarButtonCommit_Click);
      this._addAlbumButton.Click += new EventHandler(this._addAlbumButton_Click);
      this._appBar.Buttons.Add((object) this._addAlbumButton);
      this._appBar.Buttons.Add((object) this._appBarButtonCommit);
      this.ApplicationBar = (IApplicationBar) this._appBar;
    }

    private void _addAlbumButton_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToCreateEditVideoAlbum(0L, this.VM.TargetId < 0L ? -this.VM.TargetId : 0L, "", (PrivacyInfo) null);
    }

    private void _appBarButtonCommit_Click(object sender, EventArgs e)
    {
      this.VM.Save((Action<bool>) (res =>
      {
        if (!res)
          return;
        Execute.ExecuteOnUIThread((Action) (() => Navigator.Current.GoBack()));
      }));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._videoId = long.Parse(this.NavigationContext.QueryString["VideoId"]);
      AddToAlbumViewModel toAlbumViewModel = new AddToAlbumViewModel(this.CommonParameters.OwnerId, this._videoId);
      this.DataContext = (object) toAlbumViewModel;
      toAlbumViewModel.AlbumsVM.LoadData(false, false, (Action<BackendResult<VKList<VideoAlbum>, ResultCode>>) null, false);
      this._isInitialized = true;
    }

  }
}
