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
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class GamesMyPage : PageBase
  {
    private bool _isInitialized;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBoxMyGames;
    private bool _contentLoaded;

    private GamesMyViewModel VM
    {
      get
      {
        return base.DataContext as GamesMyViewModel;
      }
    }

    public GamesMyPage()
    {
      this.InitializeComponent();
      this.ucHeader.textBlockTitle.Text = (CommonResources.PageTitle_Games_MyGames.ToUpperInvariant());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxMyGames);
      this.listBoxMyGames.OnRefresh = (Action) (() => this.VM.MyGamesVM.LoadData(true, false,  null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      GamesMyViewModel gamesMyViewModel = new GamesMyViewModel();
      gamesMyViewModel.MyGamesVM.LoadData(false, false,  null, false);
      gamesMyViewModel.ItemsCleared += new EventHandler(this.ViewModel_OnItemsCleared);
      base.DataContext = gamesMyViewModel;
      this._isInitialized = true;
    }

    private void ViewModel_OnItemsCleared(object sender, EventArgs eventArgs)
    {
      ((Page) this).NavigationService.RemoveBackEntrySafe();
    }

    private void ExtendedLongListSelector_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.MyGamesVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Game_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      GameHeader selectedItem = this.listBoxMyGames.SelectedItem as GameHeader;
      if (selectedItem == null)
        return;
      FramePageUtils.CurrentPage.OpenGamesPopup(new List<object>((IEnumerable<object>) this.VM.MyGamesVM.Collection), GamesClickSource.catalog, "", this.VM.MyGamesVM.Collection.IndexOf(selectedItem),  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/GamesMyPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.listBoxMyGames = (ExtendedLongListSelector) base.FindName("listBoxMyGames");
    }
  }
}
