using System;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class StatusItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard, IHandle<ProfileStatusChangedEvent>, IHandle
  {
    private bool _isStatusSet;
    private readonly IProfileData _profileData;

    public StatusItem(IProfileData profileData)
      : base(ProfileInfoItemType.RichText)
    {
      this._profileData = profileData;
      this._isStatusSet = !string.IsNullOrEmpty(this._profileData.Activity);
      this.Title = CommonResources.StatusText;
      this.Data = this._isStatusSet ? this._profileData.Activity : CommonResources.ChangeStatusText;
      if (this._profileData.AdminLevel > 1)
        this.NavigationAction = (Action) (() =>
        {
          GroupData profileData1 = this._profileData as GroupData;
          Utility.OpenSetStatusPopup(this._profileData.Activity, profileData1 == null ? 0L : profileData1.Id, new Action<string>(this.UpdateData));
        });
      EventAggregator.Current.Subscribe(this);
    }

    private void UpdateData(string status)
    {
      this.Data = !string.IsNullOrEmpty(status) ? status : CommonResources.ChangeStatusText;
      this._profileData.Activity = status;
      this._isStatusSet = !string.IsNullOrEmpty(status);
    }

    public string GetData()
    {
      if (!this._isStatusSet)
        return "";
      return (this.Data ?? "").ToString();
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
