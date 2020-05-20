using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
  public class ProfilePhotosViewModel : ViewModelBase, IMediaHorizontalItemsViewModel, ICollectionDataProvider2<VKList<Photo>, MediaListItemViewModelBase>
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
          ListWithCount<MediaListItemViewModelBase> listWithCount = new ListWithCount<MediaListItemViewModelBase>()
          {
            TotalCount = photos.count
          };
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
          List<Photo> list = this._items.Select<MediaListItemViewModelBase, Photo>((Func<MediaListItemViewModelBase, Photo>) (item => ((PhotoMediaListItemViewModel) item).Photo)).ToList<Photo>();
          int val1 = this._items.IndexOf(vm);
          this.PublishProfileBlockClickEvent(ProfileBlockType.photos);
          ImageViewerDecoratorUC.ShowPhotosFromProfile(this._profileData.Id, this._isGroup, Math.Max(val1, 0), list, true);
        });
      }
    }

    public ProfilePhotosViewModel()
    {
      this._items = new ObservableCollection<MediaListItemViewModelBase>();
      this._itemsVM = new GenericCollectionViewModel2<VKList<Photo>, MediaListItemViewModelBase>((ICollectionDataProvider2<VKList<Photo>, MediaListItemViewModelBase>) this)
      {
        LoadCount = 28
      };
    }

    public void Init(IProfileData profileData)
    {
      this._profileData = profileData;
      this._groupData = this._profileData as GroupData;
      this._isGroup = this._groupData != null;
      List<MediaListItemViewModelBase> items = ProfilePhotosViewModel.CreateItems(profileData);
      if (!ProfilePhotosViewModel.AreItemsEqual((IList<MediaListItemViewModelBase>) items, (IList<MediaListItemViewModelBase>) this._itemsVM.Collection))
      {
        this._offset = this._isGroup || items.Count <= 0 ? items.Count : ((PhotoMediaListItemViewModel) items.Last<MediaListItemViewModelBase>()).Photo.real_offset + 1;
        this._cachedSources.Clear();
        foreach (PhotoMediaListItemViewModel listItemViewModel in items)
          this._cachedSources.Add(listItemViewModel.ImageUri);
        this.Items = new ObservableCollection<MediaListItemViewModelBase>(items);
        this._itemsVM.LoadData(true, true, true, true, (Action<List<MediaListItemViewModelBase>>) null, (Action<BackendResult<VKList<Photo>, ResultCode>>) null, false);
      }
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.Count));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanDisplay));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Title));
    }

    private static List<MediaListItemViewModelBase> CreateItems(IProfileData profileData)
    {
      List<MediaListItemViewModelBase> itemViewModelBaseList = new List<MediaListItemViewModelBase>();
      if (profileData == null || profileData.photos == null)
        return itemViewModelBaseList;
      itemViewModelBaseList.AddRange((IEnumerable<MediaListItemViewModelBase>) profileData.photos.items.Select<Photo, PhotoMediaListItemViewModel>((Func<Photo, PhotoMediaListItemViewModel>) (photo => new PhotoMediaListItemViewModel(photo))));
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
            Photo photo = result.ResultData.items.LastOrDefault<Photo>();
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
      foreach (PhotoMediaListItemViewModel listItemViewModel in (Collection<MediaListItemViewModelBase>) this._items)
        listItemViewModel.ImageUri = null;
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
      EventAggregator.Current.Publish((object) new ProfileBlockClickEvent()
      {
        UserId = this._profileData.Id,
        BlockType = blockType
      });
    }
  }
}
