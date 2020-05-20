using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class GamesMainPage : PageBase
  {
    private bool _isInitialized;
    private static int _instancesCount;
    internal GenericHeaderUC HeaderUC;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBoxGames;
    private bool _contentLoaded;

    private GamesMainViewModel VM
    {
      get
      {
        return base.DataContext as GamesMainViewModel;
      }
    }

    public GamesMainPage()
    {
      this.InitializeComponent();
      this.HeaderUC.textBlockTitle.Text = CommonResources.PageTitle_Games_Main;
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxGames);
      this.listBoxGames.OnRefresh = (Action) (() => this.VM.ReloadData());
      ++GamesMainPage._instancesCount;
    }

    ~GamesMainPage()
    {
      --GamesMainPage._instancesCount;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
      if (queryString.ContainsKey("FromPush"))
      {
        bool result;
        bool.TryParse(queryString["FromPush"], out result);
        AppGlobalStateManager.Current.GlobalState.GamesVisitSource = result ? GamesVisitSource.push : GamesVisitSource.direct;
      }
      else
        AppGlobalStateManager.Current.GlobalState.GamesVisitSource = GamesVisitSource.direct;
      long gameId = 0;
      if (queryString.ContainsKey("GameId"))
        long.TryParse(queryString["GameId"], out gameId);
      GamesMainViewModel vm = new GamesMainViewModel();
      vm.GamesSectionsVM.LoadData(false, false, (Action<BackendResult<GamesDashboardResponse, ResultCode>>) (res =>
      {
        if (gameId <= 0L)
          return;
        vm.OpenGame(gameId);
      }), false);
      base.DataContext = vm;
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.GamesSectionsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void GamesInvites_OnCleared(object sender, EventArgs e)
    {
      this.VM.RemoveInvitesPanel();
    }

    private void Game_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      GameHeader dataContext = ((FrameworkElement) (sender as GamesCatalogHeaderUC)).DataContext as GameHeader;
      if (dataContext == null || dataContext.Game == null)
        return;
      this.VM.OpenGame(dataContext.Game.id);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/GamesMainPage.xaml", UriKind.Relative));
      this.HeaderUC = (GenericHeaderUC) base.FindName("HeaderUC");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.listBoxGames = (ExtendedLongListSelector) base.FindName("listBoxGames");
    }
  }
}
