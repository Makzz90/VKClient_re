using System;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Threading.Tasks;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class UsersSearchNearbyViewModel : ViewModelBase
  {
    private readonly GeoCoordinateWatcher _coordinateWatcher;
    private ObservableCollection<SubscriptionItemHeader> _users;
    private Action<GeoPositionStatus> _statusChangedCallback;
    private bool _isEnabled;

    public ObservableCollection<SubscriptionItemHeader> Users
    {
      get
      {
        return this._users;
      }
      private set
      {
        this._users = value;
        this.NotifyPropertyChanged("Users");
      }
    }

    public GeoPositionStatus PositionStatus { get; private set; }

    public UsersSearchNearbyViewModel()
    {
      this._users = new ObservableCollection<SubscriptionItemHeader>();
      this._coordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High)
      {
        MovementThreshold = 5.0
      };
    }

    public void LoadGeoposition(Action<GeoPositionStatus> statusChangedCallback)
    {
      this._coordinateWatcher.Stop();
      this._coordinateWatcher.StatusChanged -= new EventHandler<GeoPositionStatusChangedEventArgs>(this.CoordinateWatcher_OnStatusChanged);
      this._statusChangedCallback = statusChangedCallback;
      this._coordinateWatcher.Start();
      this._coordinateWatcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(this.CoordinateWatcher_OnStatusChanged);
    }

    private void CoordinateWatcher_OnStatusChanged(object sender, GeoPositionStatusChangedEventArgs args)
    {
      if (this._statusChangedCallback == null)
        return;
      this.PositionStatus = args.Status;
      this._statusChangedCallback(this.PositionStatus);
    }

    public void StartLoading()
    {
      if (this._isEnabled)
        return;
      this._isEnabled = true;
      this.StartLoadingNearbyUsers();
    }

    private async void StartLoadingNearbyUsers()
    {
      if (!this._isEnabled)
        return;
      if (this._coordinateWatcher.Position == null)
      {
        await Task.Delay(2000);
        this.StartLoadingNearbyUsers();
      }
      else
        UsersService.Instance.GetNearby(this._coordinateWatcher.Position.Location.Latitude, this._coordinateWatcher.Position.Location.Longitude, new uint?(50U), (Action<BackendResult<VKList<User>, ResultCode>>) (async result =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
          {
            this.Users = new ObservableCollection<SubscriptionItemHeader>();
            foreach (User user in result.ResultData.users)
              this.Users.Add(new SubscriptionItemHeader(user, true));
          }
          if (!this._isEnabled)
            return;
          await Task.Delay(2000);
          this.StartLoadingNearbyUsers();
        }));
    }

    public void StopLoading()
    {
      this._isEnabled = false;
    }
  }
}
