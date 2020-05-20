using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfilePhotosViewModel : ViewModelBase, IMediaHorizontalItemsViewModel, ICollectionDataProvider2<VKList<Photo>, MediaListItemViewModelBase>, IHandle<PhotoUploadedToAlbum>, IHandle
  {
    private readonly List<string> _cachedSources = new List<string>();
    private const double CONTAINER_WIDTH = 480.0;
    private const double CONTAINER_HEIGHT = 160.0;
    private IProfileData _profileData;
    private GroupData _groupData;
    private bool _isGroup;
    private int _offset;
    private readonly GenericCollectionViewModel2<VKList<Photo>, MediaListItemViewModelBase> _itemsVM;
    private ObservableCollection<MediaListItemViewModelBase> _items;

    public ObservableCollection<MediaListItemViewModelBase> Items
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

    public Func<VKList<Photo>, ListWithCount<MediaListItemViewModelBase>> ConverterFunc
    {
      get
      {
        return (Func<VKList<Photo>, ListWithCount<MediaListItemViewModelBase>>) (photos =>
        {
          ListWithCount<MediaListItemViewModelBase> listWithCount = new ListWithCount<MediaListItemViewModelBase>() { TotalCount = photos.count };
          foreach (Photo photo in photos.items)
          {
            PhotoMediaListItemViewModel listItemViewModel = new PhotoMediaListItemViewModel(photo);
            listWithCount.List.Add((MediaListItemViewModelBase) listItemViewModel);
            this.Items.Add((MediaListItemViewModelBase) listItemViewModel);
            this._cachedSources.Add(listItemViewModel.ImageUri);
          }
          return listWithCount;
        });
      }
    }

    public double ContainerWidth
    {
      get
      {
        return 480.0;
      }
    }

    public double ContainerHeight
    {
      get
      {
        return 160.0;
      }
    }

    public Thickness ContainerMargin
    {
      get
      {
        double num = (this.ContainerWidth - this.ContainerHeight) / 2.0;
        return new Thickness(0.0, -num, 0.0, -num);
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.Profile_Photos;
      }
    }

    private int TotalCount
    {
      get
      {
        IProfileData profileData = this._profileData;
        int? nullable;
        if (profileData == null)
        {
          nullable = new int?();
        }
        else
        {
          VKList<Photo> photos = profileData.photos;
          nullable = photos != null ? new int?(photos.count) : new int?();
        }
        return nullable ?? 0;
      }
    }

    public int Count
    {
      get
      {
        return this.TotalCount;
      }
    }

    public bool CanDisplay
    {
      get
      {
        if (this.TotalCount > 0)
          return this._items.Count > 0;
        return false;
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
        return (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.photos);
          Navigator.Current.NavigateToPhotoAlbums(false, this._profileData.Id, this._isGroup, this._profileData.AdminLevel);
        });
      }
    }

    public Action<MediaListItemViewModelBase> ItemTapAction
    {
      get
      {
        return (Action<MediaListItemViewModelBase>) (vm =>
        {
            List<Photo> list = (List<Photo>)Enumerable.ToList<Photo>(Enumerable.Select<MediaListItemViewModelBase, Photo>(this._items, (Func<MediaListItemViewModelBase, Photo>)(item => ((PhotoMediaListItemViewModel)item).Photo)));
          int val1 = this._items.IndexOf(vm);
          this.PublishProfileBlockClickEvent(ProfileBlockType.photos);
          ImageViewerDecoratorUC.ShowPhotosFromProfile(this._profileData.Id, this._isGroup, Math.Max(val1, 0), list, true);
        });
      }
    }

    public ProfilePhotosViewModel()
    {
      this._items = new ObservableCollection<MediaListItemViewModelBase>();
      this._itemsVM = new GenericCollectionViewModel2<VKList<Photo>, MediaListItemViewModelBase>( this)
      {
        LoadCount = 28
      };
      EventAggregator.Current.Subscribe(this);
    }

    public void Init(IProfileData profileData)
    {
      this._profileData = profileData;
      this._groupData = this._profileData as GroupData;
      this._isGroup = this._groupData != null;
      List<MediaListItemViewModelBase> items = ProfilePhotosViewModel.CreateItems(profileData);
      if (!ProfilePhotosViewModel.AreItemsEqual((IList<MediaListItemViewModelBase>) items, (IList<MediaListItemViewModelBase>) this._itemsVM.Collection))
      {
        this._offset = this._isGroup || items.Count <= 0 ? items.Count : ((PhotoMediaListItemViewModel) Enumerable.Last<MediaListItemViewModelBase>(items)).Photo.real_offset + 1;
        this._cachedSources.Clear();
        foreach (PhotoMediaListItemViewModel listItemViewModel in items)
          this._cachedSources.Add(listItemViewModel.ImageUri);
        this.Items = new ObservableCollection<MediaListItemViewModelBase>(items);
        this._itemsVM.LoadData(true, true, true, true,  null,  null, false);
      }
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<int>(() => this.Count);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<bool>(() => this.CanDisplay);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.Title);
    }

    private static List<MediaListItemViewModelBase> CreateItems(IProfileData profileData)
    {
      List<MediaListItemViewModelBase> itemViewModelBaseList = new List<MediaListItemViewModelBase>();
      if (profileData == null || profileData.photos == null)
        return itemViewModelBaseList;
      itemViewModelBaseList.AddRange((IEnumerable<MediaListItemViewModelBase>)Enumerable.Select<Photo, PhotoMediaListItemViewModel>(profileData.photos.items, (Func<Photo, PhotoMediaListItemViewModel>)(photo => new PhotoMediaListItemViewModel(photo))));
      return itemViewModelBaseList;
    }

    private static bool AreItemsEqual(IList<MediaListItemViewModelBase> items1, IList<MediaListItemViewModelBase> items2)
    {
      if (items1.Count != items2.Count)
        return false;
      for (int index = 0; index < items1.Count; ++index)
      {
        if (items1[index].Id != items2[index].Id)
          return false;
      }
      return true;
    }

    public void GetData(GenericCollectionViewModel2<VKList<Photo>, MediaListItemViewModelBase> caller, int offset, int count, Action<BackendResult<VKList<Photo>, ResultCode>> callback)
    {
      if (this._isGroup)
        ProfilesService.Instance.GetPhotos(-this._profileData.Id, this._offset, this._groupData.group.main_album_id, (Action<BackendResult<VKList<Photo>, ResultCode>>) (result =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
            this._offset = this._offset + count;
          callback(result);
        }));
      else
        ProfilesService.Instance.GetAllPhotos(this._profileData.Id, this._offset, (Action<BackendResult<VKList<Photo>, ResultCode>>) (result =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
          {
            Photo photo = (Photo) Enumerable.LastOrDefault<Photo>(result.ResultData.items);
            this._offset = photo == null ? this._offset + count : photo.real_offset + 1;
          }
          callback(result);
        }));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel2<VKList<Photo>, MediaListItemViewModelBase> caller, int count)
    {
      return "";
    }

    public void Unload()
    {
      using (IEnumerator<MediaListItemViewModelBase> enumerator = this._items.GetEnumerator())
      {
        while (enumerator.MoveNext())
          ((PhotoMediaListItemViewModel) enumerator.Current).ImageUri =  null;
      }
    }

    public void Reload()
    {
      if (this._cachedSources.Count != this._items.Count)
        return;
      for (int index = 0; index < this._cachedSources.Count; ++index)
        ((PhotoMediaListItemViewModel) this._items[index]).ImageUri = this._cachedSources[index];
    }

    public void LoadMoreItems(object linkedItem)
    {
      this._itemsVM.LoadMoreIfNeeded(linkedItem);
    }

    private void PublishProfileBlockClickEvent(ProfileBlockType blockType)
    {
      if (this._isGroup)
        return;
      EventAggregator.Current.Publish(new ProfileBlockClickEvent()
      {
        UserId = this._profileData.Id,
        BlockType = blockType
      });
    }

    public void HandlePhotoDeleted(PhotoDeletedFromAlbum message)
    {
      IProfileData profileData = this._profileData;
      long num = profileData != null ? profileData.Id : 0L;
      if (num == 0L)
        return;
      if (this._isGroup)
        num *= -1L;
      if (message.OwnerId != num || this.Items == null)
        return;
      using (IEnumerator<MediaListItemViewModelBase> enumerator = this.Items.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          MediaListItemViewModelBase current = enumerator.Current;
          PhotoMediaListItemViewModel listItemViewModel = current as PhotoMediaListItemViewModel;
          Photo photo = listItemViewModel != null ? listItemViewModel.Photo :  null;
          if (photo != null && photo.id == message.PhotoId)
          {
            this._cachedSources.RemoveAt(this.Items.IndexOf(current));
            this._profileData.photos.items.Remove(photo);
            this.Items.Remove(current);
            --this._profileData.photos.count;
            // ISSUE: type reference
            // ISSUE: method reference
            this.NotifyPropertyChanged<int>(() => this.Count);
            break;
          }
        }
      }
    }

    public void Handle(PhotoUploadedToAlbum message)
    {
      PhotoMediaListItemViewModel listItemViewModel = new PhotoMediaListItemViewModel(message.photo);
      this._cachedSources.Insert(0, listItemViewModel.ImageUri);
      this._profileData.photos.items.Insert(0, message.photo);
      this.Items.Insert(0, (MediaListItemViewModelBase) listItemViewModel);
      ++this._profileData.photos.count;
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<int>(() => this.Count);
    }
  }
}
