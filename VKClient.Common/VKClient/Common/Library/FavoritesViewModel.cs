using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using VKClient.Video.Library;

namespace VKClient.Common.Library
{
  public class FavoritesViewModel : ViewModelBase, ICollectionDataProvider<PhotosListWithCount, AlbumPhotoHeaderFourInARow>, ICollectionDataProvider<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>, ICollectionDataProvider<WallData, IVirtualizable>, ICollectionDataProvider<UsersListWithCount, FriendHeader>, ICollectionDataProvider<List<Link>, Link>, ICollectionDataProvider<VKList<Product>, TwoInARowItemViewModel<Product>>, IHandle<UserIsFavedOrUnfavedEvent>, IHandle, IHandle<GroupFavedUnfavedEvent>, IHandle<ProductFavedUnfavedEvent>, IHandle<WallPostDeleted>, IHandle<WallPostAddedOrEdited>
  {
    private GenericCollectionViewModel<PhotosListWithCount, AlbumPhotoHeaderFourInARow> _photosVM;
    private GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> _videosVM;
    private GenericCollectionViewModel<WallData, IVirtualizable> _postsVM;
    private GenericCollectionViewModel<UsersListWithCount, FriendHeader> _usersVM;
    private GenericCollectionViewModel<List<Link>, Link> _linksVM;
    private GenericCollectionViewModel<VKList<Product>, TwoInARowItemViewModel<Product>> _productsVM;
    private int _productsCount;

    public GenericCollectionViewModel<PhotosListWithCount, AlbumPhotoHeaderFourInARow> PhotosVM
    {
      get
      {
        return this._photosVM;
      }
    }

    public GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> VideosVM
    {
      get
      {
        return this._videosVM;
      }
    }

    public GenericCollectionViewModel<WallData, IVirtualizable> PostsVM
    {
      get
      {
        return this._postsVM;
      }
    }

    public GenericCollectionViewModel<UsersListWithCount, FriendHeader> UsersVM
    {
      get
      {
        return this._usersVM;
      }
    }

    public GenericCollectionViewModel<List<Link>, Link> LinksVM
    {
      get
      {
        return this._linksVM;
      }
    }

    public GenericCollectionViewModel<VKList<Product>, TwoInARowItemViewModel<Product>> ProductsVM
    {
      get
      {
        return this._productsVM;
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.FavoritesPage_Title;
      }
    }

    Func<PhotosListWithCount, ListWithCount<AlbumPhotoHeaderFourInARow>> ICollectionDataProvider<PhotosListWithCount, AlbumPhotoHeaderFourInARow>.ConverterFunc
    {
      get
      {
        return (Func<PhotosListWithCount, ListWithCount<AlbumPhotoHeaderFourInARow>>) (plwc =>
        {
          ListWithCount<AlbumPhotoHeaderFourInARow> listWithCount = new ListWithCount<AlbumPhotoHeaderFourInARow>();
          listWithCount.TotalCount = plwc.photosCount;
          foreach (IEnumerable<Photo> photos in plwc.response.Partition<Photo>(4))
          {
            AlbumPhotoHeaderFourInARow headerFourInArow = new AlbumPhotoHeaderFourInARow(photos);
            listWithCount.List.Add(headerFourInArow);
          }
          return listWithCount;
        });
      }
    }

    Func<VKList<VKClient.Common.Backend.DataObjects.Video>, ListWithCount<VideoHeader>> ICollectionDataProvider<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>.ConverterFunc
    {
      get
      {
        return (Func<VKList<VKClient.Common.Backend.DataObjects.Video>, ListWithCount<VideoHeader>>) (vlwc =>
        {
          ListWithCount<VideoHeader> listWithCount = new ListWithCount<VideoHeader>();
          listWithCount.TotalCount = vlwc.count;
          listWithCount.List.AddRange(vlwc.items.Select<VKClient.Common.Backend.DataObjects.Video, VideoHeader>((Func<VKClient.Common.Backend.DataObjects.Video, VideoHeader>) (v => new VideoHeader(v, (List<MenuItemData>) null, vlwc.profiles, vlwc.groups, StatisticsActionSource.favorites, "", false, 0L))));
          return listWithCount;
        });
      }
    }

    Func<WallData, ListWithCount<IVirtualizable>> ICollectionDataProvider<WallData, IVirtualizable>.ConverterFunc
    {
      get
      {
        return (Func<WallData, ListWithCount<IVirtualizable>>) (wd =>
        {
          ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>();
          listWithCount.TotalCount = wd.TotalCount;
          listWithCount.List.AddRange((IEnumerable<IVirtualizable>) WallPostItemsGenerator.Generate(wd.wall, wd.profiles, wd.groups, (Action<WallPostItem>) null, 0.0));
          return listWithCount;
        });
      }
    }

    Func<UsersListWithCount, ListWithCount<FriendHeader>> ICollectionDataProvider<UsersListWithCount, FriendHeader>.ConverterFunc
    {
      get
      {
        return (Func<UsersListWithCount, ListWithCount<FriendHeader>>) (ulwc =>
        {
          ListWithCount<FriendHeader> listWithCount = new ListWithCount<FriendHeader>();
          listWithCount.TotalCount = ulwc.count;
          listWithCount.List.AddRange((IEnumerable<FriendHeader>) ulwc.users.Select<User, FriendHeader>((Func<User, FriendHeader>) (u => new FriendHeader(u, false))).ToList<FriendHeader>());
          return listWithCount;
        });
      }
    }

    Func<List<Link>, ListWithCount<Link>> ICollectionDataProvider<List<Link>, Link>.ConverterFunc
    {
      get
      {
        return (Func<List<Link>, ListWithCount<Link>>) (list => new ListWithCount<Link>()
        {
          List = list
        });
      }
    }

    public Func<VKList<Product>, ListWithCount<TwoInARowItemViewModel<Product>>> ConverterFunc
    {
      get
      {
        return (Func<VKList<Product>, ListWithCount<TwoInARowItemViewModel<Product>>>) (data =>
        {
          ListWithCount<TwoInARowItemViewModel<Product>> listWithCount = new ListWithCount<TwoInARowItemViewModel<Product>>()
          {
            TotalCount = data.count
          };
          foreach (IEnumerable<Product> source in data.items.Partition<Product>(2))
          {
            List<Product> list = source.ToList<Product>();
            TwoInARowItemViewModel<Product> arowItemViewModel = new TwoInARowItemViewModel<Product>()
            {
              Item1 = list[0]
            };
            this._productsCount = this._productsCount + 1;
            if (list.Count > 1)
            {
              arowItemViewModel.Item2 = list[1];
              this._productsCount = this._productsCount + 1;
            }
            listWithCount.List.Add(arowItemViewModel);
          }
          return listWithCount;
        });
      }
    }

    public FavoritesViewModel()
    {
      this._photosVM = new GenericCollectionViewModel<PhotosListWithCount, AlbumPhotoHeaderFourInARow>((ICollectionDataProvider<PhotosListWithCount, AlbumPhotoHeaderFourInARow>) this)
      {
        NoContentText = CommonResources.NoContent_Favorites_Photos,
        NoContentImage = "../Resources/NoContentImages/Favorites.png"
      };
      this._videosVM = new GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>((ICollectionDataProvider<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader>) this)
      {
        NoContentText = CommonResources.NoContent_Favorites_Videos,
        NoContentImage = "../Resources/NoContentImages/Favorites.png"
      };
      this._postsVM = new GenericCollectionViewModel<WallData, IVirtualizable>((ICollectionDataProvider<WallData, IVirtualizable>) this)
      {
        NoContentText = CommonResources.NoContent_Favorites_Posts,
        NoContentImage = "../Resources/NoContentImages/Favorites.png"
      };
      this._usersVM = new GenericCollectionViewModel<UsersListWithCount, FriendHeader>((ICollectionDataProvider<UsersListWithCount, FriendHeader>) this)
      {
        NoContentText = CommonResources.NoContent_Favorites_Users,
        NoContentImage = "../Resources/NoContentImages/Favorites.png"
      };
      this._linksVM = new GenericCollectionViewModel<List<Link>, Link>((ICollectionDataProvider<List<Link>, Link>) this)
      {
        NoContentText = CommonResources.NoContent_Favorites_Links,
        NoContentImage = "../Resources/NoContentImages/Favorites.png"
      };
      this._productsVM = new GenericCollectionViewModel<VKList<Product>, TwoInARowItemViewModel<Product>>((ICollectionDataProvider<VKList<Product>, TwoInARowItemViewModel<Product>>) this)
      {
        NoContentText = CommonResources.NoContent_Favorites_Products,
        NoContentImage = "../Resources/NoContentImages/Favorites.png"
      };
      this._photosVM.LoadCount = 40;
      this._photosVM.ReloadCount = 80;
      EventAggregator.Current.Subscribe((object) this);
    }

    public void GetData(GenericCollectionViewModel<PhotosListWithCount, AlbumPhotoHeaderFourInARow> caller, int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      FavoritesService.Instance.GetFavePhotos(offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<PhotosListWithCount, AlbumPhotoHeaderFourInARow> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoPhotos;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePhotoFrm, CommonResources.TwoFourPhotosFrm, CommonResources.FivePhotosFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> caller, int offset, int count, Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>> callback)
    {
      FavoritesService.Instance.GetFaveVideos(offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<VKClient.Common.Backend.DataObjects.Video>, VideoHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoVideos;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneVideoFrm, CommonResources.TwoFourVideosFrm, CommonResources.FiveVideosFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<WallData, IVirtualizable> caller, int offset, int count, Action<BackendResult<WallData, ResultCode>> callback)
    {
      FavoritesService.Instance.GetFavePosts(offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<WallData, IVirtualizable> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoWallPosts;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneWallPostFrm, CommonResources.TwoWallPostsFrm, CommonResources.FiveWallPostsFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<UsersListWithCount, FriendHeader> caller, int offset, int count, Action<BackendResult<UsersListWithCount, ResultCode>> callback)
    {
      FavoritesService.Instance.GetFaveUsers(offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<UsersListWithCount, FriendHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoPersons;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<List<Link>, Link> caller, int offset, int count, Action<BackendResult<List<Link>, ResultCode>> callback)
    {
      if (offset > 0)
        callback(new BackendResult<List<Link>, ResultCode>()
        {
          ResultCode = ResultCode.Succeeded,
          ResultData = new List<Link>()
        });
      else
        FavoritesService.Instance.GetFaveLinks(callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<List<Link>, Link> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoLinks;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneLinkFrm, CommonResources.TwoFourLinksFrm, CommonResources.FiveLinksFrm, true, null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<Product>, TwoInARowItemViewModel<Product>> caller, int offset, int count, Action<BackendResult<VKList<Product>, ResultCode>> callback)
    {
      FavoritesService.Instance.GetFaveProducts(this._productsCount, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<Product>, TwoInARowItemViewModel<Product>> caller, int count)
    {
      if (this._productsCount <= 0)
        return CommonResources.NoProducts;
      return UIStringFormatterHelper.FormatNumberOfSomething(this._productsCount, CommonResources.OneProductFrm, CommonResources.TwoFourProductsFrm, CommonResources.FiveProductsFrm, true, null, false);
    }

    public void Handle(UserIsFavedOrUnfavedEvent message)
    {
      if (message.IsFaved)
      {
        this._usersVM.Insert(new FriendHeader(message.user, false), 0);
      }
      else
      {
        FriendHeader friendHeader = this._usersVM.Collection.FirstOrDefault<FriendHeader>((Func<FriendHeader, bool>) (u => u.UserId == message.user.id));
        if (friendHeader == null)
          return;
        this._usersVM.Delete(friendHeader);
      }
    }

    public void Handle(GroupFavedUnfavedEvent message)
    {
      this._linksVM.LoadData(true, true, (Action<BackendResult<List<Link>, ResultCode>>) null, false);
    }

    public void Handle(ProductFavedUnfavedEvent message)
    {
      this.ReloadProducts(true);
    }

    public void ReloadProducts(bool suppressLoadingMessage = false)
    {
      this._productsCount = 0;
      this._productsVM.LoadData(true, suppressLoadingMessage, (Action<BackendResult<VKList<Product>, ResultCode>>) null, false);
    }

    public void Handle(WallPostDeleted message)
    {
      foreach (IVirtualizable virtualizable in (Collection<IVirtualizable>) this.PostsVM.Collection)
      {
        WallPostItem wallPostItem = virtualizable as WallPostItem;
        if (wallPostItem != null && wallPostItem.WallPost.to_id == message.WallPost.to_id && wallPostItem.WallPost.id == message.WallPost.id)
        {
          this.PostsVM.Delete((IVirtualizable) wallPostItem);
          break;
        }
      }
    }

    public void Handle(WallPostAddedOrEdited message)
    {
      if (!message.Edited)
        return;
      foreach (IVirtualizable virtualizable in (Collection<IVirtualizable>) this.PostsVM.Collection)
      {
        WallPostItem wallPostItem = virtualizable as WallPostItem;
        if (wallPostItem != null && wallPostItem.WallPost.to_id == message.NewlyAddedWallPost.to_id && wallPostItem.WallPost.id == message.NewlyAddedWallPost.id)
        {
          List<WallPost> wallPosts = new List<WallPost>();
          wallPosts.Add(message.NewlyAddedWallPost);
          List<User> users = message.Users;
          List<Group> groups = message.Groups;
          object local = null;
          double itemsWidth = 0.0;
          this.PostsVM.Insert(WallPostItemsGenerator.Generate(wallPosts, users, groups, (Action<WallPostItem>) local, itemsWidth).First<IVirtualizable>(), this.PostsVM.Collection.IndexOf(virtualizable));
          this.PostsVM.Delete(virtualizable);
          break;
        }
      }
    }
  }
}
