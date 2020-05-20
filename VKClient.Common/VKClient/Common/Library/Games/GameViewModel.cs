using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Games
{
  public class GameViewModel : ViewModelBase, ICollectionDataProvider<WallData, IVirtualizable>
  {
    private readonly GameHeader _gameHeader;
    private readonly double _newsItemsWidth;
    private bool? _isSubscribedToGameNews;
    private readonly GenericCollectionViewModel<WallData, IVirtualizable> _wallVM;
    private List<GameRequestHeader> _gameRequests;
    private List<GameLeaderboardItemHeader> _gameLeaderboard;
    private List<GameActivityHeader> _gameActivity;
    private int _leaderboardType;

    public GameHeader GameHeader
    {
      get
      {
        return this._gameHeader;
      }
    }

    public bool? IsSubscribedToGameNews
    {
      get
      {
        return this._isSubscribedToGameNews;
      }
      private set
      {
        this._isSubscribedToGameNews = value;
        this.NotifyPropertyChanged("IsSubscribedToGameNews");
      }
    }

    public int LeaderboardType
    {
      get
      {
        return this._leaderboardType;
      }
      set
      {
        this._leaderboardType = value;
        this.NotifyPropertyChanged("LeaderboardType");
      }
    }

    public Visibility InstallGameButtonVisibility
    {
      get
      {
        return !string.IsNullOrEmpty(this._gameHeader.Game.platform_id) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public GenericCollectionViewModel<WallData, IVirtualizable> WallVM
    {
      get
      {
        return this._wallVM;
      }
    }

    public List<GameRequestHeader> GameRequests
    {
      get
      {
        return this._gameRequests;
      }
      set
      {
        this._gameRequests = value;
        this.NotifyPropertyChanged("GameRequests");
      }
    }

    public List<GameLeaderboardItemHeader> GameLeaderboard
    {
      get
      {
        return this._gameLeaderboard;
      }
      set
      {
        this._gameLeaderboard = value;
        this.NotifyPropertyChanged("GameLeaderboard");
      }
    }

    public List<GameActivityHeader> GameActivity
    {
      get
      {
        return this._gameActivity;
      }
      set
      {
        this._gameActivity = value;
        this.NotifyPropertyChanged("GameActivity");
      }
    }

    public Func<WallData, ListWithCount<IVirtualizable>> ConverterFunc
    {
      get
      {
        return (Func<WallData, ListWithCount<IVirtualizable>>) (wallData =>
        {
          ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>();
          List<IVirtualizable> virtualizableList = WallPostItemsGenerator.Generate(wallData.wall, wallData.profiles, wallData.groups, (Action<WallPostItem>) null, this._newsItemsWidth);
          int totalCount = wallData.TotalCount;
          listWithCount.TotalCount = totalCount;
          listWithCount.List.AddRange((IEnumerable<IVirtualizable>) virtualizableList);
          return listWithCount;
        });
      }
    }

    public GameViewModel(GameHeader gameHeader, double newsItemsWidth)
    {
      this._gameHeader = gameHeader;
      this._newsItemsWidth = newsItemsWidth;
      this._gameRequests = new List<GameRequestHeader>();
      this._gameLeaderboard = new List<GameLeaderboardItemHeader>();
      this._gameActivity = new List<GameActivityHeader>();
      this._wallVM = new GenericCollectionViewModel<WallData, IVirtualizable>((ICollectionDataProvider<WallData, IVirtualizable>) this)
      {
        LoadCount = 10,
        ReloadCount = 20
      };
    }

    public void LoadGameInfo(Action callback = null)
    {
      AppsService.Instance.GetGameDashboard(this._gameHeader.Game.id, (Action<BackendResult<GameDashboardResponse, ResultCode>>) (res =>
      {
        if (callback != null)
          Execute.ExecuteOnUIThread(callback);
        GameDashboardResponse resultData = res.ResultData;
        int leaderboardType = this._gameHeader.Game.leaderboard_type;
        if (leaderboardType > 0)
        {
          List<GameLeaderboardItem> items = resultData.leaderboard.items;
          List<GameLeaderboardItemHeader> source = new List<GameLeaderboardItemHeader>();
          for (int index = 0; index < items.Count; ++index)
          {
            GameLeaderboardItem leaderboardItem = items[index];
            User user = resultData.leaderboard.profiles.FirstOrDefault<User>((Func<User, bool>) (u => u.id == leaderboardItem.user_id));
            GameLeaderboardItemHeader leaderboardItemHeader = new GameLeaderboardItemHeader(leaderboardItem, user, leaderboardType, index + 1);
            source.Add(leaderboardItemHeader);
          }
          this.LeaderboardType = leaderboardType;
          this.GameLeaderboard = new List<GameLeaderboardItemHeader>(source.Take<GameLeaderboardItemHeader>(10));
        }
        else if (resultData.activity != null && !resultData.activity.items.IsNullOrEmpty())
        {
          List<VKClient.Audio.Base.DataObjects.GameActivity> items = resultData.activity.items;
          List<GameActivityHeader> gameActivityHeaderList = new List<GameActivityHeader>();
          foreach (VKClient.Audio.Base.DataObjects.GameActivity gameActivity in items)
          {
            VKClient.Audio.Base.DataObjects.GameActivity activity = gameActivity;
            Game game = resultData.activity.apps.FirstOrDefault<Game>((Func<Game, bool>) (app => app.id == activity.app_id));
            User user = resultData.activity.profiles.FirstOrDefault<User>((Func<User, bool>) (u => u.id == activity.user_id));
            GameActivityHeader gameActivityHeader = new GameActivityHeader(activity, game, user);
            gameActivityHeaderList.Add(gameActivityHeader);
          }
          this.GameActivity = new List<GameActivityHeader>((IEnumerable<GameActivityHeader>) gameActivityHeaderList);
        }
        if (resultData.requests == null || resultData.requests.items.IsNullOrEmpty())
          return;
        List<GameRequest> items1 = resultData.requests.items;
        List<GameRequestHeader> gameRequestHeaderList = new List<GameRequestHeader>();
        foreach (GameRequest gameRequest in items1.Where<GameRequest>((Func<GameRequest, bool>) (request => request.type != "invite")))
        {
          GameRequest request = gameRequest;
          Game game = resultData.requests.apps.FirstOrDefault<Game>((Func<Game, bool>) (app => app.id == request.app_id));
          User user = resultData.requests.profiles.FirstOrDefault<User>((Func<User, bool>) (u => u.id == request.from_id));
          GameRequestHeader gameRequestHeader = new GameRequestHeader(request, game, user);
          gameRequestHeaderList.Add(gameRequestHeader);
        }
        this.GameRequests = new List<GameRequestHeader>((IEnumerable<GameRequestHeader>) gameRequestHeaderList);
      }));
    }

    public void LoadGameGroupInfo(Action callback = null)
    {
      long authorGroup = this._gameHeader.Game.author_group;
      if (authorGroup == 0L)
        return;
      GroupsService.Current.GetGroupInfo(authorGroup, (Action<BackendResult<GroupData, ResultCode>>) (result =>
      {
        this.IsSubscribedToGameNews = new bool?(result.ResultData.group.IsMember);
        if (callback == null)
          return;
        callback();
      }));
    }

    public void GetData(GenericCollectionViewModel<WallData, IVirtualizable> caller, int offset, int count, Action<BackendResult<WallData, ResultCode>> callback)
    {
      WallService.Current.GetWall(-this._gameHeader.Game.author_group, offset, count, callback, "all");
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<WallData, IVirtualizable> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoWallPosts;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneWallPostFrm, CommonResources.TwoWallPostsFrm, CommonResources.FiveWallPostsFrm, true, null, false);
    }
  }
}
