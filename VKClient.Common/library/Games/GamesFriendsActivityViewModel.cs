using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Games
{
  public class GamesFriendsActivityViewModel : ViewModelBase, ICollectionDataProvider<GamesFriendsActivityResponse, GameActivityHeader>
  {
    private readonly long _gameId;
    private readonly GenericCollectionViewModel<GamesFriendsActivityResponse, GameActivityHeader> _friendsActivityVM;
    private string _start_from;

    public GenericCollectionViewModel<GamesFriendsActivityResponse, GameActivityHeader> FriendsActivityVM
    {
      get
      {
        return this._friendsActivityVM;
      }
    }

    public Func<GamesFriendsActivityResponse, ListWithCount<GameActivityHeader>> ConverterFunc
    {
      get
      {
        return (Func<GamesFriendsActivityResponse, ListWithCount<GameActivityHeader>>) (data =>
        {
          this._start_from = data.next_from;
          ListWithCount<GameActivityHeader> listWithCount = new ListWithCount<GameActivityHeader>() { TotalCount = data.count };
          List<GameActivity>.Enumerator enumerator = data.items.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              GameActivity gameActivity = enumerator.Current;
              Game game = (Game)Enumerable.FirstOrDefault<Game>(data.apps, (Func<Game, bool>)(app => app.id == gameActivity.app_id));
              User user = (User)Enumerable.FirstOrDefault<User>(data.profiles, (Func<User, bool>)(p => p.id == gameActivity.user_id));
              listWithCount.List.Add(new GameActivityHeader(gameActivity, game, user));
            }
          }
          finally
          {
            enumerator.Dispose();
          }
          return listWithCount;
        });
      }
    }

    public GamesFriendsActivityViewModel(long gameId = 0)
    {
      this._gameId = gameId;
      this._friendsActivityVM = new GenericCollectionViewModel<GamesFriendsActivityResponse, GameActivityHeader>((ICollectionDataProvider<GamesFriendsActivityResponse, GameActivityHeader>) this);
    }

    public void GetData(GenericCollectionViewModel<GamesFriendsActivityResponse, GameActivityHeader> caller, int offset, int count, Action<BackendResult<GamesFriendsActivityResponse, ResultCode>> callback)
    {
      string start_from = offset == 0 ? "" : this._start_from;
      if (offset > 0 && string.IsNullOrEmpty(start_from))
        callback(new BackendResult<GamesFriendsActivityResponse, ResultCode>(ResultCode.Succeeded, new GamesFriendsActivityResponse()
        {
          apps = new List<Game>(),
          items = new List<GameActivity>(),
          profiles = new List<User>(),
          count = 0
        }));
      else
        AppsService.Instance.GetActivity(this._gameId, count, start_from, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<GamesFriendsActivityResponse, GameActivityHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoGamesFriendActivities;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneGameFriendActivity, CommonResources.TwoFourGameFriendActivities, CommonResources.FiveFriendGameActivity, true,  null, false);
    }
  }
}
