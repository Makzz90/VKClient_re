using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.VideoCatalog
{
  public class CatalogItemViewModel : ViewModelBase
  {
    private VideoCatalogItem _item;
    private List<User> _knownUsers;
    private List<Group> _knownGroups;
    private bool _isOwnVideos;

    public string Title
    {
      get
      {
        return this._item.title;
      }
    }

    public Visibility AllowEditVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public Visibility AllowDeleteVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public Visibility AllowEditOrDeleteVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public Visibility IsLiveVisibility
    {
      get
      {
        return this._item.live != 1 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility AlreadyViewedVisibility
    {
      get
      {
        return this._item.watched != 1 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string Subtitle1
    {
      get
      {
        if (this._isOwnVideos)
          return "";
        if (this.IsVideo)
        {
          if (this._item.owner_id < 0L)
          {
            Group group = this._knownGroups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this._item.owner_id));
            if (group != null)
              return group.name;
          }
          else
          {
            User user = this._knownUsers.FirstOrDefault<User>((Func<User, bool>) (u => u.id == this._item.owner_id));
            if (user != null)
              return user.Name;
          }
        }
        else
        {
          Group group = this._knownGroups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this._item.owner_id));
          if (group != null)
          {
            int membersCount = group.members_count;
            if (membersCount <= 0)
              return "";
            return UIStringFormatterHelper.FormatNumberOfSomething(membersCount, CommonResources.OneFollowerFrm, CommonResources.TwoFourFollowersFrm, CommonResources.FiveFollowersFrm, true, null, false);
          }
        }
        return "";
      }
    }

    public string Subtitle2
    {
      get
      {
        if (this.IsVideo)
        {
          int views = this._item.views;
          if (views <= 0)
            return "";
          return UIStringFormatterHelper.FormatNumberOfSomething(views, CommonResources.OneViewFrm, CommonResources.TwoFourViewsFrm, CommonResources.FiveViewsFrm, true, null, false);
        }
        int count = this._item.count;
        if (count <= 0)
          return CommonResources.VideoCatalog_Album_NoVideos;
        return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneVideoFrm, CommonResources.TwoFourVideosFrm, CommonResources.FiveVideosFrm, true, null, false);
      }
    }

    public Visibility NoVideosVisibility
    {
      get
      {
        return !this.IsVideo && this._item.count <= 0 ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public bool IsSelected
    {
      get
      {
        return false;
      }
    }

    public string CountStr
    {
      get
      {
        return UIStringFormatterHelper.FormatForUIShort((long) this._item.count);
      }
    }

    public Visibility ShowPlaySmallIconVisibility
    {
      get
      {
        return this.ShowDurationVisibility != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility ShowDurationVisibility
    {
      get
      {
        return string.IsNullOrWhiteSpace(this.UIDuration) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string UIDuration
    {
      get
      {
        if (this._item.live == 1)
          return CommonResources.VideoCatalog_LIVE;
        if (this._item.duration <= 0)
          return "";
        return UIStringFormatterHelper.FormatDuration(this._item.duration);
      }
    }

    public string ImageUri
    {
      get
      {
        return this._item.photo_320;
      }
    }

    public bool IsVideo
    {
      get
      {
        return this._item.type == "video";
      }
    }

    public Visibility IsVideoVisibility
    {
      get
      {
        return !this.IsVideo ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility IsAlbumVisibility
    {
      get
      {
        return !this.IsVideo ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public StatisticsActionSource? ActionSource { get; set; }

    public string VideoContext { get; set; }

    public CatalogItemViewModel(VideoCatalogItem item, List<User> knownUsers, List<Group> knownGroups, bool isOwnVideos = false)
    {
      this._item = item;
      this._knownUsers = knownUsers;
      this._knownGroups = knownGroups;
      this._isOwnVideos = isOwnVideos;
    }

    public void HandleTap()
    {
      if (this._item.type == "video")
      {
        if (this.ActionSource.HasValue)
          CurrentMediaSource.VideoSource = this.ActionSource.Value;
        if (!string.IsNullOrEmpty(this.VideoContext))
          CurrentMediaSource.VideoContext = this.VideoContext;
        Navigator.Current.NavigateToVideoWithComments( null, this._item.owner_id, this._item.id, "");
      }
      else
      {
        if (!(this._item.type == "album"))
          return;
        long ownerId = this._item.owner_id;
        Navigator.Current.NavigateToVideo(false, Math.Abs(ownerId), ownerId < 0L, false);
      }
    }
  }
}
