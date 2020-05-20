using System;
using System.Collections;
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
                AppGlobalStateManager.Current.GlobalState.MyGamesIds = (this._myGames == null ? new List<long>() : (List<long>)Enumerable.ToList<long>(Enumerable.Select<GameHeader, long>(this._myGames, (Func<GameHeader, long>)(header => header.Game.id))));
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
                return (Func<GamesDashboardResponse, ListWithCount<GamesSectionItem>>)(data =>
                {
                    ListWithCount<GamesSectionItem> listWithCount = new ListWithCount<GamesSectionItem>() { TotalCount = data.catalog.count };
                    if (this._isFirstLoading)
                    {
                        Dictionary<long, List<GameRequestHeader>> dictionary = new Dictionary<long, List<GameRequestHeader>>();
                        if (data.requests != null && !((IList)data.requests.items).IsNullOrEmpty())
                        {
                            List<GameRequest>[] gameRequestListArray = (List<GameRequest>[])ListExtensions.Split<GameRequest>(data.requests.items, (Func<GameRequest, bool>)(request => request.type == "invite"));
                            List<GameRequest> gameRequestList = gameRequestListArray[0];
                            if (!((IList)gameRequestList).IsNullOrEmpty())
                            {
                                List<GameRequestHeader> gameRequestHeaderList = new List<GameRequestHeader>();
                                List<GameRequest>.Enumerator enumerator = gameRequestList.GetEnumerator();
                                try
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        GameRequest gameInvite = enumerator.Current;
                                        Game game = (Game)Enumerable.FirstOrDefault<Game>(data.requests.apps, (Func<Game, bool>)(app => app.id == gameInvite.app_id));
                                        User user = (User)Enumerable.FirstOrDefault<User>(data.requests.profiles, (Func<User, bool>)(u => u.id == gameInvite.from_id));
                                        GameRequestHeader gameRequestHeader = new GameRequestHeader(gameInvite, game, user);
                                        gameRequestHeaderList.Add(gameRequestHeader);
                                    }
                                }
                                finally
                                {
                                    enumerator.Dispose();
                                }
                                this._invitesSectionItem = (GamesSectionItem)new InvitesGamesSectionItem()
                                {
                                    Data = gameRequestHeaderList
                                };
                                listWithCount.List.Add(this._invitesSectionItem);
                                this._staticCollectionElementsCount = this._staticCollectionElementsCount + 1;
                            }
                            List<GameRequestHeader> gameRequestHeaderList1 = new List<GameRequestHeader>();
                            List<GameRequest>.Enumerator enumerator1 = gameRequestListArray[1].GetEnumerator();
                            try
                            {
                                while (enumerator1.MoveNext())
                                {
                                    GameRequest request = enumerator1.Current;
                                    Game game = (Game)Enumerable.FirstOrDefault<Game>(data.requests.apps, (Func<Game, bool>)(app => app.id == request.app_id));
                                    User user = (User)Enumerable.FirstOrDefault<User>(data.requests.profiles, (Func<User, bool>)(u => u.id == request.from_id));
                                    GameRequestHeader gameRequestHeader = new GameRequestHeader(request, game, user);
                                    gameRequestHeaderList1.Add(gameRequestHeader);
                                }
                            }
                            finally
                            {
                                enumerator1.Dispose();
                            }
                            IEnumerator<GameRequestHeader> enumerator2 = ((IEnumerable<GameRequestHeader>)Enumerable.Where<GameRequestHeader>(gameRequestHeaderList1, (Func<GameRequestHeader, bool>)(request => !request.IsRead))).GetEnumerator();
                            try
                            {
                                while (enumerator2.MoveNext())
                                {
                                    GameRequestHeader current = enumerator2.Current;
                                    long id = current.Game.id;
                                    if (dictionary.ContainsKey(id))
                                        dictionary[id].Add(current);
                                    else
                                        dictionary.Add(id, new List<GameRequestHeader>()
                    {
                      current
                    });
                                }
                            }
                            finally
                            {
                                if (enumerator2 != null)
                                    enumerator2.Dispose();
                            }
                        }
                        if (data.myGames != null && !((IList)data.myGames.items).IsNullOrEmpty())
                        {
                            ObservableCollection<GameHeader> observableCollection = new ObservableCollection<GameHeader>();
                            List<Game>.Enumerator enumerator = data.myGames.items.GetEnumerator();
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    Game current = enumerator.Current;
                                    List<GameRequestHeader> gameRequestHeaderList = null;
                                    if (dictionary.ContainsKey(current.id))
                                        gameRequestHeaderList = dictionary[current.id];
                                    GameHeader gameHeader = new GameHeader(current) { Requests = gameRequestHeaderList };
                                    ((Collection<GameHeader>)observableCollection).Add(gameHeader);
                                }
                            }
                            finally
                            {
                                enumerator.Dispose();
                            }
                            this.MyGames = new ObservableCollection<GameHeader>((IEnumerable<GameHeader>)Enumerable.OrderByDescending<GameHeader, long>(observableCollection, (Func<GameHeader, long>)(game => game.LastRequestDate)));
                            this._myGamesSectionItem = (GamesSectionItem)new MyGamesSectionItem()
                            {
                                Data = this.MyGames
                            };
                            listWithCount.List.Add(this._myGamesSectionItem);
                            this._staticCollectionElementsCount = this._staticCollectionElementsCount + 1;
                        }
                        if (data.activity != null && !((IList)data.activity.items).IsNullOrEmpty() && (!((IList)data.activity.apps).IsNullOrEmpty() && !((IList)data.activity.profiles).IsNullOrEmpty()))
                        {
                            List<GameActivity> items = data.activity.items;
                            List<GameActivityHeader> gameActivityHeaderList = new List<GameActivityHeader>();
                            List<GameActivity>.Enumerator enumerator = items.GetEnumerator();
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    GameActivity activity = enumerator.Current;
                                    Game game = (Game)Enumerable.FirstOrDefault<Game>(data.activity.apps, (Func<Game, bool>)(app => app.id == activity.app_id));
                                    User user = (User)Enumerable.FirstOrDefault<User>(data.activity.profiles, (Func<User, bool>)(u => u.id == activity.user_id));
                                    GameActivityHeader gameActivityHeader = new GameActivityHeader(activity, game, user);
                                    gameActivityHeaderList.Add(gameActivityHeader);
                                }
                            }
                            finally
                            {
                                enumerator.Dispose();
                            }
                            FriendsActivityGamesSectionItem gamesSectionItem = new FriendsActivityGamesSectionItem() { Data = gameActivityHeaderList };
                            listWithCount.List.Add((GamesSectionItem)gamesSectionItem);
                            this._staticCollectionElementsCount = this._staticCollectionElementsCount + 1;
                        }
                        if (data.banners != null && !((IList)data.banners.items).IsNullOrEmpty())
                        {
                            List<GameHeader> list1 = (List<GameHeader>)Enumerable.ToList<GameHeader>(Enumerable.Select<Game, GameHeader>(data.banners.items, (Func<Game, GameHeader>)(item => new GameHeader(item))));
                            List<GamesSectionItem> list2 = listWithCount.List;
                            list2.Add((GamesSectionItem)new CatalogHeaderGamesSectionItem()
                            {
                                Data = list1
                            });
                            this._staticCollectionElementsCount = this._staticCollectionElementsCount + 1;
                        }
                        else
                        {
                            listWithCount.List.Add((GamesSectionItem)new CatalogHeaderEmptyGamesSectionItem());
                            this._staticCollectionElementsCount = this._staticCollectionElementsCount + 1;
                        }
                        this._isFirstLoading = false;
                    }
                    List<Game>.Enumerator enumerator3 = data.catalog.items.GetEnumerator();
                    try
                    {
                        while (enumerator3.MoveNext())
                        {
                            GameHeader gameHeader = new GameHeader(enumerator3.Current);
                            this._games.Add(gameHeader);
                            GamesSectionItem gamesSectionItem = (GamesSectionItem)new CatalogGamesSectionItem() { Data = gameHeader };
                            listWithCount.List.Add(gamesSectionItem);
                        }
                    }
                    finally
                    {
                        enumerator3.Dispose();
                    }
                    return listWithCount;
                });
            }
        }

        public GamesMainViewModel()
        {
            GamesVisitSource gamesVisitSource = AppGlobalStateManager.Current.GlobalState.GamesVisitSource;
            EventAggregator.Current.Publish(new OpenGamesEvent()
            {
                visit_source = gamesVisitSource
            });
            this._games = new List<GameHeader>();
            this._gamesSectionsVM = new GenericCollectionViewModel<GamesDashboardResponse, GamesSectionItem>((ICollectionDataProvider<GamesDashboardResponse, GamesSectionItem>)this);
            EventAggregator.Current.Subscribe(this);
        }

        public void ReloadData()
        {
            this._isFirstLoading = true;
            this._staticCollectionElementsCount = 0;
            this.GamesSectionsVM.LoadData(true, false, null, false);
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
            if (((IList)this._games).IsNullOrEmpty())
                return;
            GameHeader gameHeader = (GameHeader)Enumerable.FirstOrDefault<GameHeader>(this._games, (Func<GameHeader, bool>)(g =>
            {
                if (g.Game != null)
                    return g.Game.id == gameId;
                return false;
            }));
            int selectedIndex = gameHeader == null ? -1 : this._games.IndexOf(gameHeader);
            if (selectedIndex < 0)
                return;
            FramePageUtils.CurrentPage.OpenGamesPopup(new List<object>((IEnumerable<object>)this._games), GamesClickSource.catalog, "", selectedIndex, null);
        }

        public void RemoveInvitesPanel()
        {
            if (this._invitesSectionItem == null)
                return;
            this._gamesSectionsVM.Delete(this._invitesSectionItem);
        }

        public void Handle(GameDisconnectedEvent data)
        {
            if (((IList)this.MyGames).IsNullOrEmpty())
                return;
            List<GameHeader> list = new List<GameHeader>((IEnumerable<GameHeader>)this.MyGames);
            IEnumerator<GameHeader> enumerator = ((IEnumerable<GameHeader>)Enumerable.Where<GameHeader>(list, (Func<GameHeader, bool>)(game => game.Game.id == data.GameId))).GetEnumerator();
            try
            {
                if (enumerator.MoveNext())
                {
                    GameHeader current = enumerator.Current;
                    list.Remove(current);
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
            if (list.Count == 0)
                this._gamesSectionsVM.Delete(this._myGamesSectionItem);
            this.MyGames = new ObservableCollection<GameHeader>(list);
        }
    }
}
