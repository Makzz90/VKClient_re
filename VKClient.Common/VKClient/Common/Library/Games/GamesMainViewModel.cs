using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Games
{
  public class GamesMainViewModel : ViewModelBase, ICollectionDataProvider<GamesDashboardResponse, GamesSectionItem>, IHandle<GameDisconnectedEvent>, IHandle
  {
    private bool _isFirstLoading = true;
    private int _staticCollectionElementsCount;
    private GamesSectionItem _invitesSectionItem;
    private GamesSectionItem _myGamesSectionItem;
    private ObservableCollection<GameHeader> _myGames;
    private readonly List<GameHeader> _games;
    private readonly GenericCollectionViewModel<GamesDashboardResponse, GamesSectionItem> _gamesSectionsVM;

    private ObservableCollection<GameHeader> MyGames
    {
      get
      {
        return this._myGames;
      }
      set
      {
        this._myGames = value;
        AppGlobalStateManager.Current.GlobalState.MyGamesIds = this._myGames == null ? new List<long>() : this._myGames.Select<GameHeader, long>((Func<GameHeader, long>) (header => header.Game.id)).ToList<long>();
      }
    }

    public GenericCollectionViewModel<GamesDashboardResponse, GamesSectionItem> GamesSectionsVM
    {
      get
      {
        return this._gamesSectionsVM;
      }
    }

    public Func<GamesDashboardResponse, ListWithCount<GamesSectionItem>> ConverterFunc
    {
      get
      {
        return (Func<GamesDashboardResponse, ListWithCount<GamesSectionItem>>) (data =>
        {
          ListWithCount<GamesSectionItem> listWithCount = new ListWithCount<GamesSectionItem>()
          {
            TotalCount = data.catalog.count
          };
          if (this._isFirstLoading)
          {
            Dictionary<long, List<GameRequestHeader>> dictionary = new Dictionary<long, List<GameRequestHeader>>();
            if (data.requests != null && !data.requests.items.IsNullOrEmpty())
            {
              List<GameRequest>[] gameRequestListArray = data.requests.items.Split<GameRequest>((Func<GameRequest, bool>) (request => request.type == "invite"));
              List<GameRequest> list = gameRequestListArray[0];
              if (!list.IsNullOrEmpty())
              {
                List<GameRequestHeader> gameRequestHeaderList = new List<GameRequestHeader>();
                foreach (GameRequest gameRequest in list)
                {
                  GameRequest gameInvite = gameRequest;
                  Game game = data.requests.apps.FirstOrDefault<Game>((Func<Game, bool>) (app => app.id == gameInvite.app_id));
                  User user = data.requests.profiles.FirstOrDefault<User>((Func<User, bool>) (u => u.id == gameInvite.from_id));
                  GameRequestHeader gameRequestHeader = new GameRequestHeader(gameInvite, game, user);
                  gameRequestHeaderList.Add(gameRequestHeader);
                }
                this._invitesSectionItem = (GamesSectionItem) new InvitesGamesSectionItem()
                {
                  Data = (object) gameRequestHeaderList
                };
                listWithCount.List.Add(this._invitesSectionItem);
                this._staticCollectionElementsCount = this._staticCollectionElementsCount + 1;
              }
              List<GameRequestHeader> source = new List<GameRequestHeader>();
              foreach (GameRequest gameRequest in gameRequestListArray[1])
              {
                GameRequest request = gameRequest;
                Game game = data.requests.apps.FirstOrDefault<Game>((Func<Game, bool>) (app => app.id == request.app_id));
                User user = data.requests.profiles.FirstOrDefault<User>((Func<User, bool>) (u => u.id == request.from_id));
                GameRequestHeader gameRequestHeader = new GameRequestHeader(request, game, user);
                source.Add(gameRequestHeader);
              }
              foreach (GameRequestHeader gameRequestHeader in source.Where<GameRequestHeader>((Func<GameRequestHeader, bool>) (request => !request.IsRead)))
              {
                long id = gameRequestHeader.Game.id;
                if (dictionary.ContainsKey(id))
                  dictionary[id].Add(gameRequestHeader);
                else
                  dictionary.Add(id, new List<GameRequestHeader>()
                  {
                    gameRequestHeader
                  });
              }
            }
            if (data.myGames != null && !data.myGames.items.IsNullOrEmpty())
            {
              ObservableCollection<GameHeader> source = new ObservableCollection<GameHeader>();
              foreach (Game game in data.myGames.items)
              {
                List<GameRequestHeader> gameRequestHeaderList = (List<GameRequestHeader>) null;
                if (dictionary.ContainsKey(game.id))
                  gameRequestHeaderList = dictionary[game.id];
                GameHeader gameHeader = new GameHeader(game)
                {
                  Requests = gameRequestHeaderList
                };
                source.Add(gameHeader);
              }
              this.MyGames = new ObservableCollection<GameHeader>((IEnumerable<GameHeader>) source.OrderByDescending<GameHeader, long>((Func<GameHeader, long>) (game => game.LastRequestDate)));
              this._myGamesSectionItem = (GamesSectionItem) new MyGamesSectionItem()
              {
                Data = (object) this.MyGames
              };
              listWithCount.List.Add(this._myGamesSectionItem);
              this._staticCollectionElementsCount = this._staticCollectionElementsCount + 1;
            }
            if (data.activity != null && !data.activity.items.IsNullOrEmpty() && (!data.activity.apps.IsNullOrEmpty() && !data.activity.profiles.IsNullOrEmpty()))
            {
              List<GameActivity> items = data.activity.items;
              List<GameActivityHeader> gameActivityHeaderList = new List<GameActivityHeader>();
              foreach (GameActivity gameActivity in items)
              {
                GameActivity activity = gameActivity;
                Game game = data.activity.apps.FirstOrDefault<Game>((Func<Game, bool>) (app => app.id == activity.app_id));
                User user = data.activity.profiles.FirstOrDefault<User>((Func<User, bool>) (u => u.id == activity.user_id));
                GameActivityHeader gameActivityHeader = new GameActivityHeader(activity, game, user);
                gameActivityHeaderList.Add(gameActivityHeader);
              }
              FriendsActivityGamesSectionItem gamesSectionItem = new FriendsActivityGamesSectionItem()
              {
                Data = (object) gameActivityHeaderList
              };
              listWithCount.List.Add((GamesSectionItem) gamesSectionItem);
              this._staticCollectionElementsCount = this._staticCollectionElementsCount + 1;
            }
            if (data.banners != null && !data.banners.items.IsNullOrEmpty())
            {
              List<GameHeader> list1 = data.banners.items.Select<Game, GameHeader>((Func<Game, GameHeader>) (item => new GameHeader(item))).ToList<GameHeader>();
              List<GamesSectionItem> list2 = listWithCount.List;
              list2.Add((GamesSectionItem) new CatalogHeaderGamesSectionItem()
              {
                Data = (object) list1
              });
              this._staticCollectionElementsCount = this._staticCollectionElementsCount + 1;
            }
            else
            {
              listWithCount.List.Add((GamesSectionItem) new CatalogHeaderEmptyGamesSectionItem());
              this._staticCollectionElementsCount = this._staticCollectionElementsCount + 1;
            }
            this._isFirstLoading = false;
          }
          foreach (Game game in data.catalog.items)
          {
            GameHeader gameHeader = new GameHeader(game);
            this._games.Add(gameHeader);
            GamesSectionItem gamesSectionItem = (GamesSectionItem) new CatalogGamesSectionItem()
            {
              Data = (object) gameHeader
            };
            listWithCount.List.Add(gamesSectionItem);
          }
          return listWithCount;
        });
      }
    }

    public GamesMainViewModel()
    {
      GamesVisitSource gamesVisitSource = AppGlobalStateManager.Current.GlobalState.GamesVisitSource;
      EventAggregator.Current.Publish((object) new OpenGamesEvent()
      {
        visit_source = gamesVisitSource
      });
      this._games = new List<GameHeader>();
      this._gamesSectionsVM = new GenericCollectionViewModel<GamesDashboardResponse, GamesSectionItem>((ICollectionDataProvider<GamesDashboardResponse, GamesSectionItem>) this);
      EventAggregator.Current.Subscribe((object) this);
    }

    public void ReloadData()
    {
      this._isFirstLoading = true;
      this._staticCollectionElementsCount = 0;
      this.GamesSectionsVM.LoadData(true, false, (Action<BackendResult<GamesDashboardResponse, ResultCode>>) null, false);
    }

    public void GetData(GenericCollectionViewModel<GamesDashboardResponse, GamesSectionItem> caller, int offset, int count, Action<BackendResult<GamesDashboardResponse, ResultCode>> callback)
    {
      if (offset > this._staticCollectionElementsCount)
        offset -= this._staticCollectionElementsCount;
      AppsService.Instance.GetDashboard(offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<GamesDashboardResponse, GamesSectionItem> caller, int count)
    {
      if (this._staticCollectionElementsCount > 0)
        count -= this._staticCollectionElementsCount;
      if (count <= 0)
        return CommonResources.NoGames;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneGameFrm, CommonResources.TwoFourGamesFrm, CommonResources.FiveGamesFrm, true, null, false);
    }

    public void OpenGame(long gameId)
    {
      if (this._games.IsNullOrEmpty())
        return;
      GameHeader gameHeader = this._games.FirstOrDefault<GameHeader>((Func<GameHeader, bool>) (g =>
      {
        if (g.Game != null)
          return g.Game.id == gameId;
        return false;
      }));
      int selectedIndex = gameHeader == null ? -1 : this._games.IndexOf(gameHeader);
      if (selectedIndex < 0)
        return;
      FramePageUtils.CurrentPage.OpenGamesPopup(new List<object>((IEnumerable<object>) this._games), GamesClickSource.catalog, "", selectedIndex, null);
    }

    public void RemoveInvitesPanel()
    {
      if (this._invitesSectionItem == null)
        return;
      this._gamesSectionsVM.Delete(this._invitesSectionItem);
    }

    public void Handle(GameDisconnectedEvent data)
    {
      if (this.MyGames.IsNullOrEmpty())
        return;
      List<GameHeader> gameHeaderList = new List<GameHeader>((IEnumerable<GameHeader>) this.MyGames);
      using (IEnumerator<GameHeader> enumerator = gameHeaderList.Where<GameHeader>((Func<GameHeader, bool>) (game => game.Game.id == data.GameId)).GetEnumerator())
      {
        if (enumerator.MoveNext())
        {
          GameHeader current = enumerator.Current;
          gameHeaderList.Remove(current);
        }
      }
      if (gameHeaderList.Count == 0)
        this._gamesSectionsVM.Delete(this._myGamesSectionItem);
      this.MyGames = new ObservableCollection<GameHeader>(gameHeaderList);
    }
  }
}
