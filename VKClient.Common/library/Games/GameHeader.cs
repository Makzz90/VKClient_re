using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Games
{
  public class GameHeader : ViewModelBase, IHandle<GameDisconnectedEvent>, IHandle
  {
    private List<GameRequestHeader> _requests;
    private readonly Game _game;

    public Game Game
    {
      get
      {
        return this._game;
      }
    }

    public string Title
    {
      get
      {
        return this._game.title;
      }
    }

    public string Description
    {
      get
      {
        return this._game.description;
      }
    }

    public string Genre
    {
      get
      {
        return this._game.genre;
      }
    }

    public Visibility IsNewVisibility
    {
      get
      {
        if (this._game.is_new != 1)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public string Icon
    {
      get
      {
        return this._game.icon_278;
      }
    }

    public string PlayersStr
    {
      get
      {
        return UIStringFormatterHelper.FormatNumberOfSomething(this._game.members_count, CommonResources.OneMemberFrm, CommonResources.TwoFourMembersFrm, CommonResources.FiveMembersFrm, true,  null, false);
      }
    }

    public bool IsPushNotificationsEnabled
    {
      get
      {
        return this._game.push_enabled == 1;
      }
    }

    public bool IsInstalled
    {
      get
      {
        return this._game.installed;
      }
    }

    public List<GameRequestHeader> Requests
    {
      get
      {
        return this._requests ?? (this._requests = new List<GameRequestHeader>());
      }
      set
      {
        this._requests = value;
        this.NotifyPropertyChanged("Requests");
      }
    }

    public long LastRequestDate
    {
      get
      {
        if (!this.Requests.IsNullOrEmpty())
            return ((GameRequestHeader)Enumerable.Last<GameRequestHeader>(Enumerable.OrderByDescending<GameRequestHeader, long>(this.Requests, (Func<GameRequestHeader, long>)(req => req.GameRequest.date)))).GameRequest.date;
        return 0;
      }
    }

    public GameHeader(Game game)
    {
      this._game = game;
      EventAggregator.Current.Subscribe(this);
    }

    public void Handle(GameDisconnectedEvent message)
    {
      if (this._game == null || message.GameId != this._game.id)
        return;
      this._game.installed = false;
    }
  }
}
