using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileDiscussionsViewModel : ViewModelBase, IMediaVerticalItemsViewModel
  {
    private IProfileData _profileData;
    private GroupData groupData;
    private List<MediaListItemViewModelBase> _items;

    public string Title
    {
      get
      {
        return CommonResources.Profile_Discussions;
      }
    }

    public int Count
    {
      get
      {
        if (this._profileData == null || this._profileData.counters == null)
          return 0;
        return this._profileData.counters.topics;
      }
    }

    public bool CanDisplay
    {
      get
      {
        return this.Count > 0;
      }
    }

    public List<MediaListItemViewModelBase> Items
    {
      get
      {
        return this._items;
      }
      private set
      {
        this._items = value;
        this.NotifyPropertyChanged("Items");
      }
    }

    public bool IsAllItemsVisible
    {
      get
      {
        return true;
      }
    }

    public Action HeaderTapAction
    {
      get
      {
        return (Action) (() => Navigator.Current.NavigateToGroupDiscussions(this._profileData.Id, this._profileData.Name, this._profileData.AdminLevel, this.groupData.group.GroupType == GroupType.PublicPage, this.groupData.group.CanCreateTopic));
      }
    }

    public Action<MediaListItemViewModelBase> ItemTapAction
    {
      get
      {
        return (Action<MediaListItemViewModelBase>) (item =>
        {
          Topic topic = ((DiscussionMediaListItemViewModel) item).Topic;
          Navigator.Current.NavigateToGroupDiscussion(this._profileData.Id, topic.id, topic.title, topic.comments, true, topic.is_closed == 0);
        });
      }
    }

    public void Init(IProfileData profileData)
    {
      this._profileData = profileData;
      this.groupData = profileData as GroupData;
      List<MediaListItemViewModelBase> itemViewModelBaseList = new List<MediaListItemViewModelBase>();
      if (this._profileData.topics != null && this._profileData.topics.Count > 0)
        itemViewModelBaseList.AddRange((IEnumerable<MediaListItemViewModelBase>) this._profileData.topics.Select<Topic, DiscussionMediaListItemViewModel>((Func<Topic, DiscussionMediaListItemViewModel>) (item => new DiscussionMediaListItemViewModel(item))));
      this.Items = new List<MediaListItemViewModelBase>((IEnumerable<MediaListItemViewModelBase>) itemViewModelBaseList);
      this.NotifyPropertyChanged<int>((Expression<Func<int>>) (() => this.Count));
    }
  }
}
