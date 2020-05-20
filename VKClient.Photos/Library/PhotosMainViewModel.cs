using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.Localization;

namespace VKClient.Photos.Library
{
  public class PhotosMainViewModel : ViewModelBase, IHandle<PhotoDeletedFromAlbum>, IHandle, IHandle<PhotoUploadedToAlbum>, IHandle<PhotosMovedToAlbum>, IHandle<PhotoSetAsAlbumCover>, IHandle<PhotoAlbumCreated>, IHandle<PhotoAlbumDeleted>, ICollectionDataProvider<AlbumsData, Group<AlbumHeader>>, ICollectionDataProvider<AlbumsData, AlbumHeader>, ISupportReorder<AlbumHeader>
  {
    private readonly ThemeHelper _themeHelper = new ThemeHelper();
    private readonly bool _selectForMove;
    private readonly string _excludeAlbumId;
    private AlbumsData _albumsData;
    private bool _inAlbumCreatedHandler;

    public Visibility PhotoFeedMoveNotificationVisibility = Visibility.Collapsed;

    public long UserOrGroupId { get; private set; }

    public bool IsGroup { get; private set; }

    public GenericCollectionViewModel<AlbumsData, Group<AlbumHeader>> AlbumsVM { get; private set; }

    public GenericCollectionViewModel<AlbumsData, AlbumHeader> EditAlbumsVM { get; private set; }

    public string Title
    {
      get
      {
        if (!this._selectForMove)
          return this.PhotoPageTitle2;
        return PhotoResources.PhotosMainPage_ChooseAlbum;
      }
    }

    public string PhotoPageTitle2
    {
      get
      {
        if (this.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId && !this.IsGroup || (this._albumsData == null || this.IsGroup))
          return PhotoResources.PhotosMainPage_MyPhotosTitle;
        return string.Format(PhotoResources.PhotosMainPage_TitleFrm, this._albumsData.userGen.first_name).ToUpperInvariant();
      }
    }

    public string Title2
    {
      get
      {
        if (this.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId && !this.IsGroup)
          return CommonResources.PhotoPage_My;
        return PhotoResources.PhotosMainPage_Albums;
      }
    }

    private long OwnerId
    {
      get
      {
        if (this.UserOrGroupId == 0L)
          return AppGlobalStateManager.Current.LoggedInUserId;
        if (!this.IsGroup)
          return this.UserOrGroupId;
        return -this.UserOrGroupId;
      }
    }

    public string PhotoFeedMoveNotificationIconSource
    {
      get
      {
          return this._themeHelper.PhoneLightThemeVisibility == Visibility.Visible ? "/Resources/PhotosMovedNotification/PhotosMovedNotificationLight.png" : "/Resources/PhotosMovedNotification/PhotosMovedNotificationDark.png";
      }
    }

    Func<AlbumsData, ListWithCount<Group<AlbumHeader>>> ICollectionDataProvider<AlbumsData, Group<AlbumHeader>>.ConverterFunc
    {
        get
        {
            return delegate(AlbumsData data)
            {
                List<AlbumHeader> list;
                List<AlbumHeader> list2;
                this.ConvertToAlbumHeaders(data, out list, out list2);
                if (this._selectForMove)
                {
                    list.Clear();
                }
                ListWithCount<Group<AlbumHeader>> listWithCount = new ListWithCount<Group<AlbumHeader>>
                {
                    TotalCount = list.Count + data.Albums.count
                };
                Group<AlbumHeader> group = new Group<AlbumHeader>("", false);
                using (List<AlbumHeader>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        AlbumHeader current = enumerator.Current;
                        group.Add(current);
                    }
                }
                Group<AlbumHeader> group2 = new Group<AlbumHeader>(PhotosMainViewModel.FormatAlbumsCount(data.Albums.count), !Enumerable.Any<AlbumHeader>(group));
                using (List<AlbumHeader>.Enumerator enumerator = list2.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        AlbumHeader current2 = enumerator.Current;
                        if (!this._selectForMove || current2.AlbumId != "-6")
                        {
                            group2.Add(current2);
                        }
                    }
                }
                listWithCount.List.Add(group);
                if (Enumerable.Any<AlbumHeader>(group2))
                {
                    listWithCount.List.Add(group2);
                }
                return listWithCount;
            };
        }
    }

    Func<AlbumsData, ListWithCount<AlbumHeader>> ICollectionDataProvider<AlbumsData, AlbumHeader>.ConverterFunc
    {
        get
        {
            return delegate(AlbumsData data)
            {
                List<AlbumHeader> list;
                List<AlbumHeader> list2;
                this.ConvertToAlbumHeaders(data, out list, out list2);
                return new ListWithCount<AlbumHeader>
                {
                    TotalCount = list2.Count,
                    List = list2
                };
            };
        }
    }

    public PhotosMainViewModel(long userOrGroupId, bool isGroup, bool selectForMove = false, string excludeAlbumId = "")
    {
      this.UserOrGroupId = userOrGroupId;
      this.IsGroup = isGroup;
      this._selectForMove = selectForMove;
      this._excludeAlbumId = excludeAlbumId;
      EventAggregator.Current.Subscribe(this);
      this.AlbumsVM = new GenericCollectionViewModel<AlbumsData, Group<AlbumHeader>>((ICollectionDataProvider<AlbumsData, Group<AlbumHeader>>) this)
      {
        NoContentText = CommonResources.NoContent_Photos_Albums,
        NoContentImage = "/Resources/NoContentImages/Photos.png"
      };
      this.EditAlbumsVM = new GenericCollectionViewModel<AlbumsData, AlbumHeader>((ICollectionDataProvider<AlbumsData, AlbumHeader>) this)
      {
        CanShowProgress = false
      };
    }

    public void HidePhotoFeedMoveNotification()
    {
      AppGlobalStateManager.Current.GlobalState.PhotoFeedMoveHintShown = true;
      this.PhotoFeedMoveNotificationVisibility = Visibility.Collapsed;
      this.NotifyPropertyChanged<Visibility>((() => this.PhotoFeedMoveNotificationVisibility));
    }

    public void Reordered(AlbumHeader item, AlbumHeader before, AlbumHeader after)
    {
      PhotosService.Current.ReorderAlbums(item.AlbumId, before != null ? before.AlbumId : "", after != null ? after.AlbumId : "", this.OwnerId, (Action<BackendResult<ResponseWithId, ResultCode>>) (res => {}));
      this.UpdateAlbums();
    }

    private void UpdateAlbums()
    {
      if (!((IEnumerable<AlbumHeader>) this.EditAlbumsVM.Collection).Any<AlbumHeader>())
      {
        ((Collection<Group<AlbumHeader>>) this.AlbumsVM.Collection).RemoveAt(1);
        this.AlbumsVM.NotifyChanged();
      }
      else
      {
        ((Collection<AlbumHeader>) ((Collection<Group<AlbumHeader>>) this.AlbumsVM.Collection)[1]).Clear();
        foreach (AlbumHeader albumHeader in (Collection<AlbumHeader>) this.EditAlbumsVM.Collection)
          ((Collection<AlbumHeader>) ((Collection<Group<AlbumHeader>>) this.AlbumsVM.Collection)[1]).Add(albumHeader);
        this.UpdateAlbumsCount();
        this.AlbumsVM.NotifyChanged();
      }
    }

    public void LoadAlbums()
    {
      this.AlbumsVM.LoadData(false, false,  null, false);
      this.EditAlbumsVM.LoadData(false, false,  null, false);
    }

    internal void AddOrUpdateAlbum(Album createdOrUpdatedAlbum)
		{
			Func<AlbumHeader, bool> _9__1=null;
			Execute.ExecuteOnUIThread(delegate
			{
				AlbumHeader albumHeader = this.FindByAlbumId(createdOrUpdatedAlbum.aid);
				if (albumHeader != null)
				{
					albumHeader.Album = createdOrUpdatedAlbum;
					albumHeader.ReadDataFromAlbumField();
					IEnumerable<AlbumHeader> arg_5B_0 = this.EditAlbumsVM.Collection;
					Func<AlbumHeader, bool> arg_5B_1;
					if ((arg_5B_1 = _9__1) == null)
					{
						arg_5B_1 = (_9__1 = ((AlbumHeader ah) => ah.AlbumId == createdOrUpdatedAlbum.aid));
					}
					AlbumHeader albumHeader2 = Enumerable.FirstOrDefault<AlbumHeader>(arg_5B_0, arg_5B_1);
					if (albumHeader2 != null)
					{
						albumHeader2.Album = createdOrUpdatedAlbum;
						albumHeader2.ReadDataFromAlbumField();
						return;
					}
				}
				else
				{
					AlbumHeader albumHeader3 = new AlbumHeader
					{
						AlbumType = AlbumType.NormalAlbum,
						Album = createdOrUpdatedAlbum
					};
					albumHeader3.ImageUri = (albumHeader3.ImageUriSmall = "https://vk.com/images/m_noalbum.png");
					albumHeader3.ReadDataFromAlbumField();
					this.EditAlbumsVM.Insert(albumHeader3, 0);
					if (this.AlbumsVM.Collection.Count > 1)
					{
						this.AlbumsVM.Collection[1].Insert(0, albumHeader3);
					}
					else
					{
						GenericCollectionViewModel<AlbumsData, Group<AlbumHeader>> arg_130_0 = this.AlbumsVM;
						string arg_116_0 = PhotosMainViewModel.FormatAlbumsCount(1);
						List<AlbumHeader> expr_10E = new List<AlbumHeader>();
						expr_10E.Add(albumHeader3);
						arg_130_0.Insert(new Group<AlbumHeader>(arg_116_0, expr_10E, false), this.AlbumsVM.Collection.Count);
					}
					this.UpdateAlbums();
					this.UpdateAlbumsCount();
					if (!this._inAlbumCreatedHandler)
					{
						EventAggregator.Current.Publish(new PhotoAlbumCreated
						{
							Album = createdOrUpdatedAlbum,
							EventSource = this.GetHashCode()
						});
					}
				}
			});
		}

    private void UpdateAlbumsCount()
    {
      if (((Collection<Group<AlbumHeader>>) this.AlbumsVM.Collection).Count < 2)
        return;
      ((Collection<Group<AlbumHeader>>) this.AlbumsVM.Collection)[1].Title = PhotosMainViewModel.FormatAlbumsCount(((Collection<AlbumHeader>) ((Collection<Group<AlbumHeader>>) this.AlbumsVM.Collection)[1]).Count);
    }

    internal void DeleteAlbums(List<AlbumHeader> list)
		{
			PhotosService arg_41_0 = PhotosService.Current;
			Func<AlbumHeader, string> arg_25_1 = new Func<AlbumHeader, string>((ah)=>{return ah.AlbumId;});
			
			arg_41_0.DeleteAlbums(Enumerable.ToList<string>(Enumerable.Select<AlbumHeader, string>(list, arg_25_1)), this.IsGroup ? this.UserOrGroupId : 0L);
			using (List<AlbumHeader>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					AlbumHeader current = enumerator.Current;
					this.EditAlbumsVM.Delete(current);
					EventAggregator.Current.Publish(new PhotoAlbumDeleted
					{
						aid = current.AlbumId,
						EventSource = this.GetHashCode()
					});
				}
			}
			this.UpdateAlbums();
		}

    private AlbumHeader FindByAlbumId(string aid)
    {
      foreach (Collection<AlbumHeader> collection in (Collection<Group<AlbumHeader>>) this.AlbumsVM.Collection)
      {
        foreach (AlbumHeader albumHeader in collection)
        {
          if (albumHeader.AlbumId == aid)
            return albumHeader;
        }
      }
      return  null;
    }

    private void ApplyAlbumAction(string aid, Action<AlbumHeader> action)
    {
        AlbumHeader albumHeader = Enumerable.FirstOrDefault<AlbumHeader>(this.EditAlbumsVM.Collection, (AlbumHeader ah) => ah.AlbumId == aid);
        if (albumHeader != null)
        {
            action.Invoke(albumHeader);
        }
        AlbumHeader albumHeader2 = this.FindByAlbumId(aid);
        if (albumHeader2 != null)
        {
            if (albumHeader == null || albumHeader != albumHeader2)
            {
                action.Invoke(albumHeader2);
            }
            albumHeader2.ReadDataFromAlbumField();
            return;
        }
        this.AlbumsVM.LoadData(true, false, null, false);
    }

    public void Handle(PhotoSetAsAlbumCover message)
    {
      this.ApplyAlbumAction(message.aid, (Action<AlbumHeader>) (a =>
      {
        AlbumHeader albumHeader1 = a;
        Photo photo1 = message.Photo;
        string str1 = photo1 != null ? photo1.src_big :  null;
        albumHeader1.ImageUri = str1;
        AlbumHeader albumHeader2 = a;
        Photo photo2 = message.Photo;
        string str2 = photo2 != null ? photo2.src :  null;
        albumHeader2.ImageUriSmall = str2;
      }));
    }

    public void Handle(PhotoUploadedToAlbum message)
    {
      this.ApplyAlbumAction(message.aid, (Action<AlbumHeader>) (a =>
      {
        ++a.Album.size;
        a.ReadDataFromAlbumField();
      }));
    }

    public void Handle(PhotoDeletedFromAlbum message)
    {
      this.ApplyAlbumAction(message.AlbumId, (Action<AlbumHeader>) (a =>
      {
        --a.Album.size;
        a.ReadDataFromAlbumField();
      }));
    }

    public void Handle(PhotosMovedToAlbum message)
    {
      int count = message.photos.Count;
      this.ApplyAlbumAction(message.fromAlbumId, (Action<AlbumHeader>) (a =>
      {
        a.Album.size -= count;
        a.ReadDataFromAlbumField();
      }));
      this.ApplyAlbumAction(message.toAlbumId, (Action<AlbumHeader>) (a =>
      {
        a.Album.size += count;
        a.ReadDataFromAlbumField();
      }));
    }

    public void Handle(PhotoAlbumCreated message)
    {
      if (message.EventSource == this.GetHashCode())
        return;
      this._inAlbumCreatedHandler = true;
      this.AddOrUpdateAlbum(message.Album);
      this._inAlbumCreatedHandler = false;
    }

    public void Handle(PhotoAlbumDeleted message)
    {
        if (message.EventSource != this.GetHashCode())
        {
            AlbumHeader albumHeader = Enumerable.FirstOrDefault<AlbumHeader>(this.EditAlbumsVM.Collection, (AlbumHeader a) => a.AlbumId == message.aid);
            if (albumHeader != null)
            {
                this.EditAlbumsVM.Delete(albumHeader);
                this.UpdateAlbums();
            }
        }
    }

    public void GetData(GenericCollectionViewModel<AlbumsData, Group<AlbumHeader>> caller, int offset, int count, Action<BackendResult<AlbumsData, ResultCode>> callback)
    {
      if (((Collection<Group<AlbumHeader>>) this.AlbumsVM.Collection).Count > 0)
      {
        Group<AlbumHeader> group = ((IEnumerable<Group<AlbumHeader>>) this.AlbumsVM.Collection).First<Group<AlbumHeader>>();
        if (offset != 0)
          offset -= ((Collection<AlbumHeader>) group).Count;
      }
      PhotosService.Current.GetUsersAlbums(this.UserOrGroupId, this.IsGroup, offset, count, (Action<BackendResult<AlbumsData, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          this._albumsData = res.ResultData;
          this.NotifyPropertyChanged<string>((() => this.Title));
          this.PhotoFeedMoveNotificationVisibility = (!AppGlobalStateManager.Current.GlobalState.PhotoFeedMoveHintShown).ToVisiblity();
          this.NotifyPropertyChanged<Visibility>((() => this.PhotoFeedMoveNotificationVisibility));
        }
        callback(res);
      }), true);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<AlbumsData, Group<AlbumHeader>> caller, int count)
    {
      return PhotosMainViewModel.GetAlbumsTextForCount(count);
    }

    public void GetData(GenericCollectionViewModel<AlbumsData, AlbumHeader> caller, int offset, int count, Action<BackendResult<AlbumsData, ResultCode>> callback)
    {
      PhotosService.Current.GetUsersAlbums(this.UserOrGroupId, this.IsGroup, offset, count, (Action<BackendResult<AlbumsData, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
        {
          this._albumsData = res.ResultData;
          this.NotifyPropertyChanged<string>((() => this.Title));
        }
        callback(res);
      }), false);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<AlbumsData, AlbumHeader> caller, int count)
    {
      return PhotosMainViewModel.GetAlbumsTextForCount(count);
    }

    public static string GetAlbumsTextForCount(int count)
    {
      if (count > 0)
        return "";
      return PhotoResources.NoAlbums;
    }

    private static string FormatAlbumsCount(int count)
    {
      return UIStringFormatterHelper.FormatNumberOfSomething(count, PhotoResources.OneAlbumFrm, PhotoResources.TwoFourAlbumsFrm, PhotoResources.FiveAlbumsFrm, true,  null, false);
    }

    private void ConvertToAlbumHeaders(AlbumsData data, out List<AlbumHeader> nonEditAlbums, out List<AlbumHeader> editAlbums)
    {
      nonEditAlbums = new List<AlbumHeader>();
      editAlbums = new List<AlbumHeader>();
      if (data.allPhotos.NotNullAndHasAtLeastOneNonNullElement())
      {
        List<AlbumHeader> albumHeaderList = nonEditAlbums;
        AlbumHeader albumHeader = new AlbumHeader();
        albumHeader.AlbumName = PhotoResources.PhotosMainPage_AllPhotos;
        string srcBig = data.allPhotos[0].src_big;
        albumHeader.ImageUri = srcBig;
        string src = data.allPhotos[0].src;
        albumHeader.ImageUriSmall = src;
        string str1 = data.allPhotos.Count >= 2 ? data.allPhotos[1].src_big : "";
        albumHeader.ImageUri2 = str1;
        string str2 = data.allPhotos.Count >= 3 ? data.allPhotos[2].src_big : "";
        albumHeader.ImageUri3 = str2;
        int allPhotosCount = data.allPhotosCount;
        albumHeader.PhotosCount = allPhotosCount;
        int num = 0;
        albumHeader.AlbumType = (AlbumType) num;
        albumHeaderList.Add(albumHeader);
      }
      if (data.profilePhotos.NotNullAndHasAtLeastOneNonNullElement())
      {
        List<AlbumHeader> albumHeaderList = nonEditAlbums;
        AlbumHeader albumHeader = new AlbumHeader();
        albumHeader.AlbumName = PhotoResources.PhotosMainPage_ProfilePhotos;
        string srcBig = data.profilePhotos[0].src_big;
        albumHeader.ImageUri = srcBig;
        string src = data.profilePhotos[0].src;
        albumHeader.ImageUriSmall = src;
        string str1 = data.profilePhotos.Count >= 2 ? data.profilePhotos[1].src_big : "";
        albumHeader.ImageUri2 = str1;
        string str2 = data.profilePhotos.Count >= 3 ? data.profilePhotos[2].src_big : "";
        albumHeader.ImageUri3 = str2;
        int profilePhotosCount = data.profilePhotosCount;
        albumHeader.PhotosCount = profilePhotosCount;
        int num = 1;
        albumHeader.AlbumType = (AlbumType) num;
        albumHeaderList.Add(albumHeader);
      }
      if (data.userPhotos.NotNullAndHasAtLeastOneNonNullElement())
        nonEditAlbums.Add(new AlbumHeader()
        {
          AlbumName = string.Format(PhotoResources.PhotosMainPage_PhotosWithFormat, data.userIns.first_name),
          ImageUri = data.userPhotos[0].src_big,
          ImageUriSmall = data.userPhotos[0].src,
          ImageUri2 = data.userPhotos.Count >= 2 ? data.userPhotos[1].src_big : "",
          ImageUri3 = data.userPhotos.Count >= 3 ? data.userPhotos[2].src_big : "",
          PhotosCount = data.userPhotosCount,
          AlbumType = AlbumType.PhotosWithUser
        });
      if (data.wallPhotos.NotNullAndHasAtLeastOneNonNullElement())
      {
        List<AlbumHeader> albumHeaderList = nonEditAlbums;
        AlbumHeader albumHeader = new AlbumHeader();
        albumHeader.AlbumName = PhotoResources.PhotosMainPage_WallPhotos;
        string srcBig = data.wallPhotos[0].src_big;
        albumHeader.ImageUri = srcBig;
        string src = data.wallPhotos[0].src;
        albumHeader.ImageUriSmall = src;
        string str1 = data.wallPhotos.Count >= 2 ? data.wallPhotos[1].src_big : "";
        albumHeader.ImageUri2 = str1;
        string str2 = data.wallPhotos.Count >= 3 ? data.wallPhotos[2].src_big : "";
        albumHeader.ImageUri3 = str2;
        int wallPhotosCount = data.wallPhotosCount;
        albumHeader.PhotosCount = wallPhotosCount;
        int num = 3;
        albumHeader.AlbumType = (AlbumType) num;
        albumHeaderList.Add(albumHeader);
      }
      if (data.savedPhotos.NotNullAndHasAtLeastOneNonNullElement())
      {
        List<AlbumHeader> albumHeaderList = nonEditAlbums;
        AlbumHeader albumHeader = new AlbumHeader();
        albumHeader.AlbumName = PhotoResources.PhotosMainPage_SavedPhotos;
        string srcBig = data.savedPhotos[0].src_big;
        albumHeader.ImageUri = srcBig;
        string src = data.savedPhotos[0].src;
        albumHeader.ImageUriSmall = src;
        string str1 = data.savedPhotos.Count >= 2 ? data.savedPhotos[1].src_big : "";
        albumHeader.ImageUri2 = str1;
        string str2 = data.savedPhotos.Count >= 3 ? data.savedPhotos[2].src_big : "";
        albumHeader.ImageUri3 = str2;
        int savedPhotosCount = data.savedPhotosCount;
        albumHeader.PhotosCount = savedPhotosCount;
        int num = 4;
        albumHeader.AlbumType = (AlbumType) num;
        albumHeaderList.Add(albumHeader);
      }
      foreach (Album album in data.albums)
      {
        if (album.aid != this._excludeAlbumId)
          editAlbums.Add(new AlbumHeader()
          {
            AlbumName = album.title,
            PhotosCount = album.size,
            ImageUri = album.thumb_src,
            ImageUriSmall = album.thumb_src_small,
            AlbumId = album.aid,
            AlbumType = AlbumType.NormalAlbum,
            Album = album
          });
      }
    }
  }
}
