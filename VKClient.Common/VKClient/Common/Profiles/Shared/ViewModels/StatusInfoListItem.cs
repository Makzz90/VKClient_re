using System;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class StatusInfoListItem : InfoListItem, IHandle<ProfileStatusChangedEvent>, IHandle
  {
    private bool _isStatusSet;
    private readonly IProfileData _profileData;

    public StatusInfoListItem(IProfileData profileData)
    {
      this._profileData = profileData;
      this._isStatusSet = !string.IsNullOrEmpty(this._profileData.Activity);
      this.Text = this._isStatusSet ? this._profileData.Activity : CommonResources.ChangeStatusText;
      if (this._profileData.AdminLevel > 1)
      {
        this.IsTiltEnabled = true;
        this.TapAction = (Action) (() =>
        {
          GroupData groupData = this._profileData as GroupData;
          Utility.OpenSetStatusPopup(this._profileData.Activity, groupData == null ? 0L : groupData.Id, new Action<string>(this.UpdateStatus));
        });
      }
      EventAggregator.Current.Subscribe((object) this);
    }

    private void UpdateStatus(string status)
    {
      this.Text = !string.IsNullOrEmpty(status) ? status : CommonResources.ChangeStatusText;
      this._profileData.Activity = status;
      this._isStatusSet = !string.IsNullOrEmpty(status);
    }

    public void Handle(ProfileStatusChangedEvent message)
    {
      if (message.IsGroup)
      {
        if (!(this._profileData is UserData) || this._profileData.Id != message.Id)
          return;
        this._profileData.Activity = message.Status;
      }
      else
      {
        if (!(this._profileData is GroupData) || this._profileData.Id != message.Id)
          return;
        this._profileData.Activity = message.Status;
      }
    }
  }
}
