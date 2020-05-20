using System;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.Games
{
  public class GameSettingsViewModel : ViewModelBase
  {
    private bool _pushNotificationsEnabled = true;
    private readonly long _gameId;
    private GameHeader _gameHeader;
    private bool _isInProgress;

    public GameHeader GameHeader
    {
      get
      {
        return this._gameHeader;
      }
      set
      {
        this._gameHeader = value;
        this.NotifyPropertyChanged("GameHeader");
      }
    }

    public new bool IsInProgress
    {
      get
      {
        return this._isInProgress;
      }
      set
      {
        this._isInProgress = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsInProgress));
        this.SetInProgress(this._isInProgress, "");
      }
    }

    public bool PushNotificationsEnabled
    {
      get
      {
        return this._pushNotificationsEnabled;
      }
      set
      {
        if (this._isInProgress || this.PushNotificationsEnabled == value)
          return;
        this._pushNotificationsEnabled = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.PushNotificationsEnabled));
        this.IsInProgress = true;
        AppsService.Instance.ToggleRequests(this._gameId, value, (Action<BackendResult<int, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.IsInProgress = false;
          if (res.ResultCode == ResultCode.Succeeded)
            return;
          this._pushNotificationsEnabled = !value;
          this.NotifyPropertyChanged("PushNotificationsEnabled");
        }))));
      }
    }

    public GameSettingsViewModel(long gameId)
    {
      this._gameId = gameId;
    }

    public void LoadGameInfo()
    {
      this.IsInProgress = true;
      AppsService.Instance.GetApp(this._gameId, (Action<BackendResult<VKList<Game>, ResultCode>>) (result =>
      {
        this.IsInProgress = false;
        if (result.ResultCode != ResultCode.Succeeded)
          return;
        Game game = result.ResultData.items[0];
        this.GameHeader = new GameHeader(game);
        this._pushNotificationsEnabled = game.push_enabled == 1;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.PushNotificationsEnabled));
      }));
    }

    public void DisconnectGame()
    {
      if (this.IsInProgress || MessageBox.Show(CommonResources.Games_Disconnect_Message, CommonResources.Games_Disconnect_Title, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.IsInProgress = true;
      AppsService.Instance.Remove(this._gameId, (Action<BackendResult<OwnCounters, ResultCode>>) (result =>
      {
        if (result.ResultCode != ResultCode.Succeeded)
          return;
        CountersManager.Current.Counters = result.ResultData;
        this.IsInProgress = false;
        EventAggregator.Current.Publish((object) new GameDisconnectedEvent(this._gameId));
      }));
    }
  }
}
