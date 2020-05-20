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
    public class GamesMyViewModel : ViewModelBase, ICollectionDataProvider<VKList<Game>, GameHeader>, IHandle<GameDisconnectedEvent>, IHandle
    {
        private readonly GenericCollectionViewModel<VKList<Game>, GameHeader> _myGamesVM;

        public GenericCollectionViewModel<VKList<Game>, GameHeader> MyGamesVM
        {
            get
            {
                return this._myGamesVM;
            }
        }

        public Func<VKList<Game>, ListWithCount<GameHeader>> ConverterFunc
        {
            get
            {
                return (Func<VKList<Game>, ListWithCount<GameHeader>>)(data =>
                {
                    ListWithCount<GameHeader> listWithCount = new ListWithCount<GameHeader>()
                    {
                        TotalCount = data.count
                    };
                    foreach (Game game in data.items)
                        listWithCount.List.Add(new GameHeader(game));
                    return listWithCount;
                });
            }
        }

        public event EventHandler ItemsCleared;

        public GamesMyViewModel()
        {
            this._myGamesVM = new GenericCollectionViewModel<VKList<Game>, GameHeader>((ICollectionDataProvider<VKList<Game>, GameHeader>)this);
            EventAggregator.Current.Subscribe((object)this);
        }

        public void GetData(GenericCollectionViewModel<VKList<Game>, GameHeader> caller, int offset, int count, Action<BackendResult<VKList<Game>, ResultCode>> callback)
        {
            AppsService.Instance.GetMyGames(offset, count, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<VKList<Game>, GameHeader> caller, int count)
        {
            if (count <= 0)
                return CommonResources.NoGames;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneGameFrm, CommonResources.TwoFourGamesFrm, CommonResources.FiveGamesFrm, true, null, false);
        }

        public void Handle(GameDisconnectedEvent data)
        {
            this._myGamesVM.Delete(this._myGamesVM.Collection.FirstOrDefault<GameHeader>((Func<GameHeader, bool>)(game => game.Game.id == data.GameId)));
            if (this._myGamesVM.Collection.Count != 0 || this.ItemsCleared == null)
                return;
            this.ItemsCleared((object)this, EventArgs.Empty);
        }
    }
}
