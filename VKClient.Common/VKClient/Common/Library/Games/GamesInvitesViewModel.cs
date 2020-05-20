using System;
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
                return (Func<GamesRequestsResponse, ListWithCount<GameRequestHeader>>)(data =>
                {
                    ListWithCount<GameRequestHeader> listWithCount = new ListWithCount<GameRequestHeader>()
                    {
                        TotalCount = data.count
                    };
                    foreach (GameRequest gameRequest1 in data.items)
                    {
                        GameRequest gameRequest = gameRequest1;
                        Game game = data.apps.FirstOrDefault<Game>((Func<Game, bool>)(app => app.id == gameRequest.app_id));
                        User user = data.profiles.FirstOrDefault<User>((Func<User, bool>)(p => p.id == gameRequest.from_id));
                        listWithCount.List.Add(new GameRequestHeader(gameRequest, game, user));
                    }
                    return listWithCount;
                });
            }
        }

        public GamesInvitesViewModel()
        {
            this._gamesInvitesVM = new GenericCollectionViewModel<GamesRequestsResponse, GameRequestHeader>((ICollectionDataProvider<GamesRequestsResponse, GameRequestHeader>)this);
        }

        public /*async*/ void GetData(GenericCollectionViewModel<GamesRequestsResponse, GameRequestHeader> caller, int offset, int count, Action<BackendResult<GamesRequestsResponse, ResultCode>> callback)
        {
            AppsService.Instance.GetInviteRequests(offset, count, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<GamesRequestsResponse, GameRequestHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoGamesInvites;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneGameInviteFrm, CommonResources.TwoFourGameInvitesFrm, CommonResources.FiveGameInvitesFrm, true, null, false);
        }
    }
}
