using System;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;
using VKClient.Groups.Localization;
using System.Linq;
using System.Collections.Generic;

namespace VKClient.Groups.Library
{
  public class GroupDiscussionsViewModel : ViewModelBase, IHandle<TopicCommentAddedDeletedOrEdited>, IHandle, IHandle<TopicCreated>, ICollectionDataProvider<TopicsInfo, ThemeHeader>
  {
    private readonly long _gid;
    private readonly int _adminLevel;
    private readonly bool _isPublicPage;
    private readonly bool _canCreateTopic;
    private readonly GenericCollectionViewModel<TopicsInfo, ThemeHeader> _discussionsVM;

    public bool CanCreateDiscussion
    {
      get
      {
        return this._canCreateTopic;
      }
    }

    public GenericCollectionViewModel<TopicsInfo, ThemeHeader> DiscussionsVM
    {
      get
      {
        return this._discussionsVM;
      }
    }

    public int AdminLevel
    {
      get
      {
        return this._adminLevel;
      }
    }

    public bool IsPublicPage
    {
      get
      {
        return this._isPublicPage;
      }
    }

    public long GroupId
    {
      get
      {
        return this._gid;
      }
    }

    public string Title
    {
      get
      {
        return GroupResources.themes.ToUpperInvariant();
      }
    }

    public Func<TopicsInfo, ListWithCount<ThemeHeader>> ConverterFunc
    {
        get
        {
            return (Func<TopicsInfo, ListWithCount<ThemeHeader>>)(ti => new ListWithCount<ThemeHeader>()
            {
                TotalCount = ti.TotalCount,
                List = new List<ThemeHeader>(ti.topics.Select<Topic, ThemeHeader>((Func<Topic, ThemeHeader>)(t => new ThemeHeader(t, ti.profiles.FirstOrDefault<User>((Func<User, bool>)(u => u.uid == t.updated_by)), ti.groups.FirstOrDefault<Group>((Func<Group, bool>)(g => g.id == -t.updated_by))))))
            });
        }
    }

    public GroupDiscussionsViewModel(long gid, int adminLevel, bool isPublicPage, bool canCreateTopic)
    {
      this._gid = gid;
      this._adminLevel = adminLevel;
      this._isPublicPage = isPublicPage;
      this._canCreateTopic = canCreateTopic;
      EventAggregator.Current.Subscribe(this);
      this._discussionsVM = new GenericCollectionViewModel<TopicsInfo, ThemeHeader>((ICollectionDataProvider<TopicsInfo, ThemeHeader>) this);
    }

    public void LoadData(bool reload = false, bool suppressLoading = false)
    {
      this._discussionsVM.LoadData(reload, suppressLoading,  null, false);
    }

    internal void NavigateToDiscusson(bool loadFromEnd, ThemeHeader header)
    {
      Navigator.Current.NavigateToGroupDiscussion(this._gid, header.Topic.tid, header.Topic.title, header.Topic.comments, loadFromEnd, header.Topic.is_closed == 0);
    }

    public void Handle(TopicCommentAddedDeletedOrEdited message)
    {
        if (message.gid != this._gid)
            return;
        Execute.ExecuteOnUIThread((Action)(() => this.LoadData(true, true)));
    }

    public void Handle(TopicCreated message)
    {
        if (message.gid != this._gid)
            return;
        Execute.ExecuteOnUIThread((Action)(() => this.LoadData(true, true)));
    }

    public void GetData(GenericCollectionViewModel<TopicsInfo, ThemeHeader> caller, int offset, int count, Action<BackendResult<TopicsInfo, ResultCode>> callback)
    {
      GroupsService.Current.GetTopics(this._gid, offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<TopicsInfo, ThemeHeader> caller, int count)
    {
      if (count <= 0)
        return GroupResources.NoTopics;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneTopicFrm, GroupResources.TwoFourTopicsFrm, GroupResources.FiveTopicsFrm, true,  null, false);
    }
  }
}
