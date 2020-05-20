using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;

using VKClient.Audio.Base.Extensions;

namespace VKClient.Common.Library.VirtItems
{
  public class NewsFeedAdsItem : VirtualizableItemBase, ISupportChildHeightChange, IHaveUniqueKey, IHaveNewsfeedItemId
  {
    private readonly List<User> _knownUsers;
    private readonly List<Group> _knownGroups;
    private UCItem _headerItem;

    public WallPostItem WallPostItem { get; private set; }

    public NewsItem NewsItem { get; set; }

    public override double FixedHeight
    {
      get
      {
        if (this.WallPostItem == null)
          return 0.0;
        return 66.0 + this.WallPostItem.FixedHeight;
      }
    }

    public string NewsfeedItemId
    {
      get
      {
        return "ads";
      }
    }

    public NewsFeedAdsItem(double width, Thickness margin, NewsItem newsItem, List<User> knownUsers, List<Group> knownGroups)
      : base(width, margin, new Thickness())
    {
      this.NewsItem = newsItem;
      this._knownUsers = knownUsers;
      this._knownGroups = knownGroups;
      this.GenerateLayout();
      this.PostPixelEvent("load");
    }

    private void GenerateLayout()
    {
      if (this.NewsItem.ads == null || this.NewsItem.ads.Count <= 0 || !(this.NewsItem.ads[0].type == "post"))
        return;
      NewsItemDataWithUsersAndGroupsInfo wallPostWithInfo = new NewsItemDataWithUsersAndGroupsInfo()
      {
        WallPost = this.NewsItem.ads[0].post,
        Groups = this._knownGroups,
        Profiles = this._knownUsers
      };
      if (this.WallPostItem == null)
      {
        this.WallPostItem = new WallPostItem(this.Width, new Thickness(0.0, 66.0, 0.0, 0.0), true, wallPostWithInfo, (Action<WallPostItem>) null, false, (Action<long, User, Group>) null, false, false, true, true, this, (Func<List<MenuItem>>) null);
        this.VirtualizableChildren.Add((IVirtualizable) this.WallPostItem);
      }
      if (this._headerItem != null)
        return;
      this._headerItem = new UCItem(this.Width, new Thickness(), (Func<UserControlVirtualizable>) (() => (UserControlVirtualizable) new AdHeaderUC()
      {
        textBlock1 = {
          Text = this.NewsItem.ads_title.Capitalize()
        },
        textBlock2 = {
          Text = this.NewsItem.ads[0].age_restriction
        }
      }), (Func<double>) (() => 66.0), (Action<UserControlVirtualizable>) null, 0.0, false);
      this.VirtualizableChildren.Add((IVirtualizable) this._headerItem);
    }

    private void PostPixelEvent(string eventName)
    {
      AdPixelEvent adPixelEvent = new AdPixelEvent();
      bool flag = false;
      if (this.NewsItem.ads_statistics != null)
      {
        foreach (AdStatistics adsStatistic in this.NewsItem.ads_statistics)
        {
          if (adsStatistic.type == eventName)
          {
            flag = true;
            adPixelEvent.UrlToLoad.Add(adsStatistic.url);
          }
        }
      }
      foreach (Ad ad in this.NewsItem.ads)
      {
        if (ad.statistics != null)
        {
          foreach (AdStatistics statistic in ad.statistics)
          {
            if (statistic.type == eventName)
            {
              flag = true;
              adPixelEvent.UrlToLoad.Add(statistic.url);
            }
          }
        }
      }
      if (!flag)
        return;
      EventAggregator.Current.Publish((object) adPixelEvent);
    }

    protected override void NotifyAboutImpression()
    {
      base.NotifyAboutImpression();
      if (this.NewsItem.ads == null || this.NewsItem.ads.Count <= 0 || !(this.NewsItem.ads[0].type == "post"))
        return;
      EventAggregator.Current.Publish((object) new AdImpressionEvent()
      {
        AdDataImpression = this.NewsItem.ads[0].ad_data_impression
      });
      this.PostPixelEvent("impression");
      ViewPostEvent viewPostEvent = new ViewPostEvent();
      viewPostEvent.PostId = this.WallPostItem.WallPost.PostId;
      viewPostEvent.CopyPostIds = this.WallPostItem.WallPost.CopyPostIds;
      viewPostEvent.ItemType = NewsFeedItemType.WallPost;
      viewPostEvent.Position = this.Parent.VirtualizableItems.IndexOf((IVirtualizable) this);
      int num = 1;
      viewPostEvent.ShouldSendImmediately = num != 0;
      EventAggregator.Current.Publish((object) viewPostEvent);
    }

    public void RespondToChildHeightChange(IVirtualizable child)
    {
      this.GenerateLayout();
      this.RegenerateChildren();
      this.NotifyHeightChanged();
    }

    public string GetKey()
    {
      string str = "ads";
      WallPostItem wallPostItem = this.WallPostItem;
      if ((wallPostItem != null ? wallPostItem.WallPost : (WallPost) null) != null)
        str += string.Format("{0}_{1}", (object) this.WallPostItem.WallPost.owner_id, (object) this.WallPostItem.WallPost.id);
      return str;
    }
  }
}
