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
  internal class GamesInvitesViewModel : ViewModelBase, ICollectionDataProvider<GamesRequestsResponse, GameRequestHeader>
  {
    private readonly GenericCollectionViewModel<GamesRequestsResponse, GameRequestHeader> _gamesInvitesVM;

    public GenericCollectionViewModel<GamesRequestsResponse, GameRequestHeader> GamesInvitesVM
    {
      get
      {
        return this._gamesInvitesVM;
      }
    }

    public Func<GamesRequestsResponse, ListWithCount<GameRequestHeader>> ConverterFunc
    {
      get
      {
        return (Func<GamesRequestsResponse, ListWithCount<GameRequestHeader>>) (data =>
        {
          ListWithCount<GameRequestHeader> listWithCount = new ListWithCount<GameRequestHeader>() { TotalCount = data.count };
          List<GameRequest>.Enumerator enumerator = data.items.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              GameRequest gameRequest = enumerator.Current;
              Game game = (Game)Enumerable.FirstOrDefault<Game>(data.apps, (Func<Game, bool>)(app => app.id == gameRequest.app_id));
              User user = (User)Enumerable.FirstOrDefault<User>(data.profiles, (Func<User, bool>)(p => p.id == gameRequest.from_id));
              listWithCount.List.Add(new GameRequestHeader(gameRequest, game, user));
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

    public GamesInvitesViewModel()
    {
      this._gamesInvitesVM = new GenericCollectionViewModel<GamesRequestsResponse, GameRequestHeader>((ICollectionDataProvider<GamesRequestsResponse, GameRequestHeader>) this);
    }

    public void GetData(GenericCollectionViewModel<GamesRequestsResponse, GameRequestHeader> caller, int offset, int count, Action<BackendResult<GamesRequestsResponse, ResultCode>> callback)
    {
      AppsService.Instance.GetInviteRequests(offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<GamesRequestsResponse, GameRequestHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoGamesInvites;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneGameInviteFrm, CommonResources.TwoFourGameInvitesFrm, CommonResources.FiveGameInvitesFrm, true,  null, false);
    }
  }
}
