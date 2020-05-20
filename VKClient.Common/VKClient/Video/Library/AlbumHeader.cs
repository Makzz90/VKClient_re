using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Video.Library
{
  public class AlbumHeader : ViewModelBase, IHandle<VideoAlbumEditedEvent>, IHandle
  {
    private VideoAlbum _va;
    private bool _isSelected;
    private bool _pickMode;
    private bool _forceAllowEditDelete;

    public string Title
    {
      get
      {
        return this._va.title;
      }
    }

    public Visibility IsLiveVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public Visibility AlreadyViewedVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public Visibility ShowPlaySmallIconVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public Visibility ShowDurationVisibility
    {
      get
      {
        if (string.IsNullOrWhiteSpace(this.UIDuration))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public bool IsSelected
    {
      get
      {
        return this._isSelected;
      }
      set
      {
        if (this._isSelected == value)
          return;
        this._isSelected = value;
        base.NotifyPropertyChanged<bool>(() => this.IsSelected);
      }
    }

    public string Subtitle1
    {
      get
      {
        if (this._va.count <= 0)
          return CommonResources.VideoCatalog_Album_NoVideos;
        return UIStringFormatterHelper.FormatNumberOfSomething(this._va.count, CommonResources.OneVideoFrm, CommonResources.TwoFourVideosFrm, CommonResources.FiveVideosFrm, true,  null, false);
      }
    }

    public string Subtitle2
    {
      get
      {
        DateTimeDiff dateTimeDiff = BaseFormatterHelper.GetDateTimeDiff((double) this._va.updated_time);
        string str = "";
        switch (dateTimeDiff.DiffType)
        {
          case DateTimeDiffType.JustNow:
            str = CommonResources.VideoCatalog_AlbumUpdated_AMomentAgo;
            break;
          case DateTimeDiffType.Seconds:
            str = UIStringFormatterHelper.FormatNumberOfSomething(dateTimeDiff.Value, CommonResources.VideoCatalog_AlbumUpdated_OneSecondAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_TwoFourSecondsAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_FiveSecondsAgoFrm, true,  null, false);
            break;
          case DateTimeDiffType.Minutes:
            str = UIStringFormatterHelper.FormatNumberOfSomething(dateTimeDiff.Value, CommonResources.VideoCatalog_AlbumUpdated_OneMinuteAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_TwoFourMinutesAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_FiveMinutesAgoFrm, true,  null, false);
            break;
          case DateTimeDiffType.Hours:
            str = UIStringFormatterHelper.FormatNumberOfSomething(dateTimeDiff.Value, CommonResources.VideoCatalog_AlbumUpdated_OneHourAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_TwoFourHoursAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_FiveHoursAgoFrm, true,  null, false);
            break;
          case DateTimeDiffType.Days:
            str = UIStringFormatterHelper.FormatNumberOfSomething(dateTimeDiff.Value, CommonResources.VideoCatalog_AlbumUpdated_OneDayAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_TwoFourDaysAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_FiveDaysAgoFrm, true,  null, false);
            break;
          case DateTimeDiffType.Months:
            str = UIStringFormatterHelper.FormatNumberOfSomething(dateTimeDiff.Value, CommonResources.VideoCatalog_AlbumUpdated_OneMonthAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_TwoFourMonthsAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_FiveMonthsAgoFrm, true,  null, false);
            break;
          case DateTimeDiffType.Years:
            str = UIStringFormatterHelper.FormatNumberOfSomething(dateTimeDiff.Value, CommonResources.VideoCatalog_AlbumUpdated_OneYearAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_TwoFourYearsAgoFrm, CommonResources.VideoCatalog_AlbumUpdated_FiveYearsAgoFrm, true,  null, false);
            break;
        }
        return str;
      }
    }

    public Visibility AllowEditVisibility
    {
      get
      {
        if (!this.CanEditOrDelete)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility AllowDeleteVisibility
    {
      get
      {
        if (!this.CanEditOrDelete)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility AllowEditOrDeleteVisibility
    {
      get
      {
        if (!this.CanEditOrDelete)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public bool CanEditOrDelete
    {
      get
      {
        if (this._va.owner_id != AppGlobalStateManager.Current.LoggedInUserId)
          return this._forceAllowEditDelete;
        return true;
      }
    }

    public Visibility IsVideoVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public Visibility IsAlbumVisibility
    {
      get
      {
        return Visibility.Visible;
      }
    }

    public string CountStr
    {
      get
      {
        return UIStringFormatterHelper.FormatForUIShort((long) this._va.count);
      }
    }

    public Visibility NoVideosVisibility
    {
      get
      {
        if (this._va.count > 0)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public string UIDuration
    {
      get
      {
        return "";
      }
    }

    public string ImageUri
    {
      get
      {
        return this._va.photo_320;
      }
    }

    public VideoAlbum VideoAlbum
    {
      get
      {
        return this._va;
      }
    }

    public AlbumHeader(VideoAlbum va, bool pickMode = false, bool forceAllowEditDelete = false)
    {
      this._va = va;
      this._pickMode = pickMode;
      this._forceAllowEditDelete = forceAllowEditDelete;
      EventAggregator.Current.Subscribe(this);
    }

    public void HandleTap()
    {
      Navigator.Current.NavigateToVideoAlbum(this._va.album_id, this._va.title, this._pickMode, Math.Abs(this._va.owner_id), this._va.owner_id < 0L);
    }

    internal void HandleEdit()
    {
      Navigator.Current.NavigateToCreateEditVideoAlbum(this._va.id, this._va.owner_id < 0L ? -this._va.owner_id : 0, this._va.title, this._va.PrivacyInfo);
    }

    internal void HandleDelete()
    {
        if (MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.VideoCatalog_DeleteAlbum, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      VideoService.Instance.DeleteAlbum(this._va.album_id, (Action<BackendResult<object, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          EventAggregator current = EventAggregator.Current;
          VideoAlbumAddedDeletedEvent addedDeletedEvent = new VideoAlbumAddedDeletedEvent();
          addedDeletedEvent.AlbumId = this._va.album_id;
          addedDeletedEvent.OwnerId = this._va.owner_id;
          int num = 0;
          addedDeletedEvent.IsAdded = num != 0;
          current.Publish(addedDeletedEvent);
        }
        else
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", null);
      }), new long?(this._va.owner_id < 0L ? -this._va.owner_id : 0L));
    }

    public void Handle(VideoAlbumEditedEvent message)
    {
      if (message.OwnerId != this._va.owner_id || message.AlbumId != this._va.album_id)
        return;
      this.VideoAlbum.privacy=(message.Privacy.ToStringList());
      this.VideoAlbum.title = message.Name;
      // ISSUE: method reference
      base.NotifyPropertyChanged<string>(() => this.Title);
    }
  }
}
