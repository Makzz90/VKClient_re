using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;

namespace VKClient.Common.Library
{
  public class AppGlobalStateManager
  {
    private readonly string _appGlobalStateKey = "AppGlobalState";
    private AppGlobalStateData _globalState = new AppGlobalStateData();
    private static AppGlobalStateManager _current;
    private bool _isInitialized;
    private StatsEventsTracker _eventsTracker;

    public static AppGlobalStateManager Current
    {
      get
      {
        if (AppGlobalStateManager._current == null)
          AppGlobalStateManager._current = new AppGlobalStateManager();
        return AppGlobalStateManager._current;
      }
    }

    public long LoggedInUserId
    {
      get
      {
        return this._globalState.LoggedInUserId;
      }
    }

    public AppGlobalStateData GlobalState
    {
      get
      {
        return this._globalState;
      }
    }

    public bool IsInitialized
    {
      get
      {
        return this._isInitialized;
      }
    }

    public void UpdateCachedUserStatus(string updatedStatus)
    {
      if (this.GlobalState.LoggedInUser == null)
        return;
      this.GlobalState.LoggedInUser.activity = updatedStatus ?? "";
    }

    public void Initialize(bool startUpdatesManager = true)
    {
      if (this._isInitialized)
        return;
      CacheManager.EnsureCacheFolderExists();
      if (CacheManager.TryDeserialize((IBinarySerializable) this._globalState, this._appGlobalStateKey, CacheManager.DataType.StateData))
      {
        AutorizationData autorizationData = new AutorizationData();
        autorizationData.access_token = this._globalState.AccessToken ?? "";
        autorizationData.secret = this._globalState.Secret ?? "";
        autorizationData.user_id = this._globalState.LoggedInUserId;
        int num = startUpdatesManager ? 1 : 0;
        VKRequestsDispatcher.SetAuthorizationData(autorizationData, num != 0);
      }
      this._eventsTracker = StatsEventsTracker.Instance;
      this._eventsTracker.PendingEvents = EventsConverter.ConvertToPendingEvents(this.GlobalState.PendingStatisticsEvents);
      this._isInitialized = true;
    }

    public bool IsUserLoginRequired()
    {
      return string.IsNullOrEmpty(this._globalState.AccessToken);
    }

    public void HandleUserLogin(AutorizationData data)
    {
      this.ReceiveNewToken(data);
    }

    public void HandleUserLogoutOrAuthorizationFailure()
    {
      this._globalState.ResetForNewUser();
      this.SaveState();
      this.ReceiveNewToken(new AutorizationData());
      AudioTrackHelper.EnsureStopPlayingOnLogout();
      EventAggregator.Current.Publish(new BaseDataChangedEvent());
      EventAggregator.Current.Publish(new UserIsLoggedOutEvent());
    }

    public void SaveState()
    {
      this.GlobalState.PendingStatisticsEvents = EventsConverter.ConvertFromPendingEvents(StatsEventsTracker.Instance.PendingEvents);
      CacheManager.TrySerialize((IBinarySerializable) this._globalState, this._appGlobalStateKey, false, CacheManager.DataType.StateData);
    }

    private void ReceiveNewToken(AutorizationData autorizationData)
    {
      this._globalState.LoggedInUserId = autorizationData.user_id;
      this._globalState.AccessToken = autorizationData.access_token;
      this._globalState.Secret = autorizationData.secret;
      CacheManager.EnsureCacheFolderExists();
      VKRequestsDispatcher.SetAuthorizationData(autorizationData, true);
      this.SaveState();
    }
  }
}
