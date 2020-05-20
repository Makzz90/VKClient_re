using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Library
{
  public sealed class BlockEditingViewModel : ViewModelBase
  {
    private bool _isFormEnabled = true;
    private string _comment = "";
    private bool? _isCommentVisible = new bool?(false);
    private string _duration = "";
    private readonly long _communityId;
    private readonly bool _isEditing;
    private readonly bool _isOpenedWithoutPicker;
    private readonly User _user;
    private readonly User _manager;
    public int DurationUnixTime;
    private CustomListPickerItem _reason;
    private Visibility _addedByVisibility;
    private string _pageTitle;

    public List<CustomListPickerItem> AvailableReasons = new List<CustomListPickerItem>()
    {
      new CustomListPickerItem() { Name = CommonResources.Other },
      new CustomListPickerItem()
      {
        Name = CommonResources.Group_BanReason_Spam
      },
      new CustomListPickerItem()
      {
        Name = CommonResources.Group_BanReason_VerbalAbuse
      },
      new CustomListPickerItem()
      {
        Name = CommonResources.Group_BanReason_StrongLanguage
      },
      new CustomListPickerItem()
      {
        Name = CommonResources.Group_BanReason_IrrelevantMessages
      }
    };

    public bool IsFormEnabled
    {
      get
      {
        return this._isFormEnabled;
      }
      set
      {
        this._isFormEnabled = value;
        this.NotifyPropertyChanged<bool>((() => this.IsFormEnabled));
      }
    }

    public string UserPhoto
    {
      get
      {
        return this._user.photo_max;
      }
    }

    public string UserName
    {
      get
      {
        return this._user.Name;
      }
    }

    public string UserMembership
    {
      get
      {
        if (this._manager == null)
          return CommonResources.IsCommunityMemberOrFollower;
        if (this._user.sex == 1)
          return CommonResources.BlockedInCommunityFemale;
        return CommonResources.BlockedInCommunityMale;
      }
    }

    public string AddByForm
    {
      get
      {
        if (this._manager == null)
          return "";
        if (this._manager.sex == 1)
          return CommonResources.AddedFemale;
        return CommonResources.AddedMale;
      }
    }

    public string ManagerName
    {
      get
      {
        User manager = this._manager;
        return (manager != null ? manager.Name :  null) ?? "";
      }
    }

    public string BlockStartDate
    {
      get
      {
        return UIStringFormatterHelper.FormateDateForEventUI(Extensions.UnixTimeStampToDateTime((double) this._user.ban_info.date, true));
      }
    }

    public double CommentPlaceholderOpacity
    {
      get
      {
        return this.Comment != "" ? 0.0 : 1.0;
      }
    }

    public string Comment
    {
      get
      {
        return this._comment;
      }
      set
      {
        this._comment = value;
        this.NotifyPropertyChanged<string>((() => this.Comment));
        this.NotifyPropertyChanged<double>((() => this.CommentPlaceholderOpacity));
      }
    }

    public bool? IsCommentVisible
    {
      get
      {
        return this._isCommentVisible;
      }
      set
      {
        this._isCommentVisible = value;
        this.NotifyPropertyChanged<bool?>((() => this.IsCommentVisible));
      }
    }

    public CustomListPickerItem Reason
    {
      get
      {
        return this._reason;
      }
      set
      {
        this._reason = value;
        this.NotifyPropertyChanged<CustomListPickerItem>((() => this.Reason));
      }
    }

    public string Duration
    {
      get
      {
        return this._duration;
      }
      set
      {
        this._duration = value;
        this.NotifyPropertyChanged<string>((() => this.Duration));
      }
    }

    public Visibility AddedByVisibility
    {
      get
      {
        return this._addedByVisibility;
      }
      set
      {
        this._addedByVisibility = value;
        this.NotifyPropertyChanged<Visibility>((() => this.AddedByVisibility));
      }
    }

    public string PageTitle
    {
      get
      {
        return this._pageTitle;
      }
      set
      {
        this._pageTitle = value;
        this.NotifyPropertyChanged<string>((() => this.PageTitle));
      }
    }

    public BlockEditingViewModel(long communityId, User user, User manager, bool isEditing, bool isOpenedWithoutPicker = false)
    {
      this._communityId = communityId;
      this._isEditing = isEditing;
      this._user = user;
      this._manager = manager;
      this._isOpenedWithoutPicker = isOpenedWithoutPicker;
      if (manager != null)
      {
        this.Comment = Extensions.ForUI(user.ban_info.comment);
        this.IsCommentVisible = new bool?(user.ban_info.comment_visible == 1);
        this.Reason = this.AvailableReasons[user.ban_info.reason];
        this.DurationUnixTime = user.ban_info.end_date;
        this.PageTitle = CommonResources.BlockEditing.ToUpper();
        this.AddedByVisibility = Visibility.Visible;
      }
      else
      {
        user.ban_info = new BlockInformation() { date = 0 };
        this.Reason = this.AvailableReasons.First<CustomListPickerItem>();
        this.DurationUnixTime = 0;
        this.PageTitle = CommonResources.Block.ToUpper();
        this.AddedByVisibility = Visibility.Collapsed;
      }
      this.Duration = this.DurationUnixTime == 0 ? CommonResources.Forever : string.Format("{0} {1}", CommonResources.Unblocking, UIStringFormatterHelper.FormateDateForEventUI(Extensions.UnixTimeStampToDateTime((double) this.DurationUnixTime, true)));
    }

    public void GoToManagerProfile()
    {
      Navigator.Current.NavigateToUserProfile(this._manager.id, "", "", false);
    }

    public void UpdateDuration(int durationUnixTime)
    {
      this.DurationUnixTime = durationUnixTime;
      this.Duration = this.DurationUnixTime == 0 ? CommonResources.Forever : string.Format("{0} {1}", CommonResources.Unblocking, UIStringFormatterHelper.FormateDateForEventUI(Extensions.UnixTimeStampToDateTime((double) this.DurationUnixTime, true)));
    }

    public void SaveChanges(NavigationService navigationService)
    {
        int reason = 0;
        if (this.Reason.Name == CommonResources.Group_BanReason_Spam)
            reason = 1;
        else if (this.Reason.Name == CommonResources.Group_BanReason_VerbalAbuse)
            reason = 2;
        else if (this.Reason.Name == CommonResources.Group_BanReason_StrongLanguage)
            reason = 3;
        else if (this.Reason.Name == CommonResources.Group_BanReason_IrrelevantMessages)
            reason = 4;
        this.SetInProgress(true, "");
        this.IsFormEnabled = false;
        GroupsService current1 = GroupsService.Current;
        long communityId = this._communityId;
        long id = this._user.id;
        int durationUnixTime = this.DurationUnixTime;
        int reason1 = reason;
        string comment = this.Comment;
        bool? isCommentVisible1 = this.IsCommentVisible;
        bool flag1 = true;
        int num1 = isCommentVisible1.GetValueOrDefault() == flag1 ? (isCommentVisible1.HasValue ? 1 : 0) : 0;
        Action<BackendResult<int, ResultCode>> callback = (Action<BackendResult<int, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
        {
            if (result.ResultCode == ResultCode.Succeeded)
            {
                this._user.ban_info.date = Extensions.DateTimeToUnixTimestamp(DateTime.Now.ToUniversalTime(), true);
                this._user.ban_info.end_date = this.DurationUnixTime;
                this._user.ban_info.reason = reason;
                this._user.ban_info.comment = this.Comment;
                BlockInformation banInfo = this._user.ban_info;
                bool? isCommentVisible2 = this.IsCommentVisible;
                bool flag2 = true;
                int num2 = (isCommentVisible2.GetValueOrDefault() == flag2 ? (isCommentVisible2.HasValue ? 1 : 0) : 0) != 0 ? 1 : 0;
                banInfo.comment_visible = num2;
                this._user.ban_info.admin_id = AppGlobalStateManager.Current.LoggedInUserId;
                this._user.ban_info.manager = AppGlobalStateManager.Current.GlobalState.LoggedInUser;
                EventAggregator current2 = EventAggregator.Current;
                CommunityBlockChanged communityBlockChanged = new CommunityBlockChanged();
                communityBlockChanged.CommunityId = this._communityId;
                communityBlockChanged.User = this._user;
                int num3 = this._manager != null ? 1 : 0;
                communityBlockChanged.IsEditing = num3 != 0;
                current2.Publish((object)communityBlockChanged);
                if (!this._isEditing && !this._isOpenedWithoutPicker)
                    navigationService.RemoveBackEntry();
                Navigator.Current.GoBack();
            }
            else
            {
                this.SetInProgress(false, "");
                this.IsFormEnabled = true;
                GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            }
        })));
        current1.BlockUser(communityId, id, durationUnixTime, reason1, comment, num1 != 0, callback);
    }
  }
}
