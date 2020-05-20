using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class SuggestedPostponedPostsViewModel : ViewModelBase, ICollectionDataProvider<WallData, IVirtualizable>, IHandle<WallPostPublished>, IHandle, IHandle<WallPostDeleted>, IHandle<WallPostAddedOrEdited>
  {
    private long _userOrGroupId;
    private bool _isGroup;
    private SuggestedPostponedMode _mode;
    private GenericCollectionViewModel<WallData, IVirtualizable> _wallVM;

    public GenericCollectionViewModel<WallData, IVirtualizable> WallVM
    {
      get
      {
        return this._wallVM;
      }
    }

    public long OwnerId
    {
      get
      {
        if (!this._isGroup)
          return this._userOrGroupId;
        return -this._userOrGroupId;
      }
    }

    public string Title
    {
      get
      {
        switch (this._mode)
        {
          case SuggestedPostponedMode.Suggested:
            return CommonResources.SuggestedNews_Title.ToUpperInvariant();
          case SuggestedPostponedMode.Postponed:
            return CommonResources.PostponedNews_Title.ToUpperInvariant();
          default:
            return "";
        }
      }
    }

    private string ModeFilter
    {
      get
      {
        switch (this._mode)
        {
          case SuggestedPostponedMode.Suggested:
            return "suggests";
          case SuggestedPostponedMode.Postponed:
            return "postponed";
          default:
            return "";
        }
      }
    }

    Func<WallData, ListWithCount<IVirtualizable>> ICollectionDataProvider<WallData, IVirtualizable>.ConverterFunc
    {
      get
      {
        return (Func<WallData, ListWithCount<IVirtualizable>>) (wallData =>
        {
          ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>();
          List<IVirtualizable> virtualizableList = WallPostItemsGenerator.Generate(wallData.wall, wallData.profiles, wallData.groups, new Action<WallPostItem>(this.DeletedCallback), 0.0);
          int totalCount = wallData.TotalCount;
          listWithCount.TotalCount = totalCount;
          listWithCount.List.AddRange((IEnumerable<IVirtualizable>) virtualizableList);
          return listWithCount;
        });
      }
    }

    public SuggestedPostponedPostsViewModel(long userOrGroupId, bool isGroup, SuggestedPostponedMode mode)
    {
      this._userOrGroupId = userOrGroupId;
      this._isGroup = isGroup;
      this._mode = mode;
      this._wallVM = new GenericCollectionViewModel<WallData, IVirtualizable>((ICollectionDataProvider<WallData, IVirtualizable>) this);
      EventAggregator.Current.Subscribe(this);
    }

    public void GetData(GenericCollectionViewModel<WallData, IVirtualizable> caller, int offset, int count, Action<BackendResult<WallData, ResultCode>> callback)
    {
      WallService.Current.GetWall(this.OwnerId, offset, count, callback, this.ModeFilter);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<WallData, IVirtualizable> caller, int count)
    {
      switch (this._mode)
      {
        case SuggestedPostponedMode.Suggested:
          if (count <= 0)
            return CommonResources.SuggestedPosts_NoPosts;
          return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.SuggestedNews_OneSuggestedPostFrm, CommonResources.SuggestedNews_TwoFourSuggestedPostsFrm, CommonResources.SuggestedNews_FiveSuggestedPostsFrm, true,  null, false);
        case SuggestedPostponedMode.Postponed:
          if (count <= 0)
            return CommonResources.PostponedNews_NoPosts;
          return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.PostponedNews_OnePostponedPostFrm, CommonResources.PostponedNews_TwoFourPostponedPostsFrm, CommonResources.PostponedNews_FivePostponedPostsFrm, true,  null, false);
        default:
          return "";
      }
    }

    private void DeletedCallback(WallPostItem obj)
    {
      this.WallVM.Delete((IVirtualizable) obj);
    }

    public void Handle(WallPostDeleted message)
    {
      this.RemoveIfApplicable(message.WallPost);
    }

    private void RemoveIfApplicable(WallPost wallPost)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
          IVirtualizable virtualizable = (IVirtualizable)Enumerable.FirstOrDefault<IVirtualizable>(this._wallVM.Collection, (Func<IVirtualizable, bool>)(w =>
        {
          if ((w as WallPostItem).WallPost.id == wallPost.id)
            return (w as WallPostItem).WallPost.to_id == wallPost.to_id;
          return false;
        }));
        if (virtualizable == null)
          return;
        this._wallVM.Delete(virtualizable);
      }));
    }

    public void Handle(WallPostPublished message)
    {
      this.RemoveIfApplicable(message.WallPost);
    }

    public void Handle(WallPostAddedOrEdited message)
    {
      if (!message.Edited)
        return;
      WallPostItem itemForWallPost = this.GetItemForWallPost(message.NewlyAddedWallPost.id, message.NewlyAddedWallPost.to_id);
      if (itemForWallPost == null)
        return;
      List<WallPost> wallPosts = new List<WallPost>();
      wallPosts.Add(message.NewlyAddedWallPost);
      List<User> users = message.Users;
      List<Group> groups = message.Groups;
      Action<WallPostItem> deletedCallback = new Action<WallPostItem>(this.DeletedCallback);
      double itemsWidth = 0.0;
      IVirtualizable virtualizable = (IVirtualizable) Enumerable.First<IVirtualizable>(WallPostItemsGenerator.Generate(wallPosts, users, groups, deletedCallback, itemsWidth));
      int ind = ((Collection<IVirtualizable>) this._wallVM.Collection).IndexOf((IVirtualizable) itemForWallPost);
      this._wallVM.Delete((IVirtualizable) itemForWallPost);
      this._wallVM.Insert(virtualizable, ind);
    }

    public WallPostItem GetItemForWallPost(long id, long to_id)
    {
        return Enumerable.FirstOrDefault<IVirtualizable>(this._wallVM.Collection, (Func<IVirtualizable, bool>)(w =>
      {
        if ((w as WallPostItem).WallPost.id == id)
          return (w as WallPostItem).WallPost.to_id == to_id;
        return false;
      })) as WallPostItem;
    }
  }
}
