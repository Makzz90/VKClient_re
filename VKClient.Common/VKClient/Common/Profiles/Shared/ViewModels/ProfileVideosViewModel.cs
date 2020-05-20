using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
    public class ProfileVideosViewModel : ViewModelBase, IMediaHorizontalItemsViewModel, ICollectionDataProvider2<VKList<VKClient.Common.Backend.DataObjects.Video>, MediaListItemViewModelBase>
  {
    private readonly List<string> _cachedSources = new List<string>();
    private const double CONTAINER_WIDTH = 480.0;
    private const double CONTAINER_HEIGHT = 200.0;
    private IProfileData _profileData;
    private bool _isGroup;
    private int _offset;
    private readonly GenericCollectionViewModel2<VKList<VKClient.Common.Backend.DataObjects.Video>, MediaListItemViewModelBase> _itemsVM;
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

    public Func<VKList<VKClient.Common.Backend.DataObjects.Video>, ListWithCount<MediaListItemViewModelBase>> ConverterFunc
    {
      get
      {
          return (Func<VKList<VKClient.Common.Backend.DataObjects.Video>, ListWithCount<MediaListItemViewModelBase>>)(videos =>
        {
          ListWithCount<MediaListItemViewModelBase> listWithCount = new ListWithCount<MediaListItemViewModelBase>()
          {
            TotalCount = videos.count
          };
          foreach (VKClient.Common.Backend.DataObjects.Video video in videos.items)
          {
            VideoMediaListItemViewModel listItemViewModel = new VideoMediaListItemViewModel(video);
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
        return 200.0;
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
        return CommonResources.Profile_Videos;
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
          Counters counters = profileData.counters;
          nullable = counters != null ? new int?(counters.videos) : new int?();
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
        return (Action) (() => Navigator.Current.NavigateToVideo(false, this._profileData.Id, this._isGroup, this._profileData.AdminLevel > 1));
      }
    }

    public Action<MediaListItemViewModelBase> ItemTapAction
    {
      get
      {
        return (Action<MediaListItemViewModelBase>) (item =>
        {
          VideoMediaListItemViewModel listItemViewModel = item as VideoMediaListItemViewModel;
          if (listItemViewModel == null)
            return;
          Navigator.Current.NavigateToVideoWithComments(listItemViewModel.Video, listItemViewModel.Video.owner_id, listItemViewModel.Video.id, "");
        });
      }
    }

    public ProfileVideosViewModel()
    {
      this._items = new ObservableCollection<MediaListItemViewModelBase>();
      this._itemsVM = new GenericCollectionViewModel2<VKList<VKClient.Common.Backend.DataObjects.Video>, MediaListItemViewModelBase>((ICollectionDataProvider2<VKList<VKClient.Common.Backend.DataObjects.Video>, MediaListItemViewModelBase>)this)
      {
        LoadCount = 28
      };
    }

    public void Init(IProfileData profileData)
    {
      this._profileData = profileData;
      this._isGroup = this._profileData is GroupData;
      List<MediaListItemViewModelBase> items = ProfileVideosViewModel.CreateItems(profileData);
      if (!ProfileVideosViewModel.AreItemsEqual((IList<MediaListItemViewModelBase>) items, (IList<MediaListItemViewModelBase>) this._itemsVM.Collection))
      {
        this._offset = items.Count;
        this._cachedSources.Clear();
        foreach (VideoMediaListItemViewModel listItemViewModel in items)
          this._cachedSources.Add(listItemViewModel.ImageUri);
        this.Items = new ObservableCollection<MediaListItemViewModelBase>(items);
        this._itemsVM.LoadData(true, true, true, true, null, null, false);
      }
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.Count));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanDisplay));
    }

    private static List<MediaListItemViewModelBase> CreateItems(IProfileData profileData)
    {
      List<MediaListItemViewModelBase> itemViewModelBaseList = new List<MediaListItemViewModelBase>();
      if (profileData == null || profileData.videos == null)
        return itemViewModelBaseList;
      itemViewModelBaseList.AddRange((IEnumerable<MediaListItemViewModelBase>)profileData.videos.Select<VKClient.Common.Backend.DataObjects.Video, VideoMediaListItemViewModel>((Func<VKClient.Common.Backend.DataObjects.Video, VideoMediaListItemViewModel>)(item => new VideoMediaListItemViewModel(item))));
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

    public void GetData(GenericCollectionViewModel2<VKList<VKClient.Common.Backend.DataObjects.Video>, MediaListItemViewModelBase> caller, int offset, int count, Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>> callback)
    {
        VideoService.Instance.GetVideos(this._profileData.Id, this._isGroup, this._offset, count, (Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>>)(result =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
          this._offset = this._offset + count;
        callback(result);
      }), 0L, true);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel2<VKList<VKClient.Common.Backend.DataObjects.Video>, MediaListItemViewModelBase> caller, int count)
    {
      return "";
    }

    public void Unload()
    {
      foreach (VideoMediaListItemViewModel listItemViewModel in (Collection<MediaListItemViewModelBase>) this._items)
        listItemViewModel.ImageUri = null;
    }

    public void Reload()
    {
      if (this._cachedSources.Count != this._items.Count)
        return;
      for (int index = 0; index < this._cachedSources.Count; ++index)
        ((VideoMediaListItemViewModel) this._items[index]).ImageUri = this._cachedSources[index];
    }

    public void LoadMoreItems(object linkedItem)
    {
      this._itemsVM.LoadMoreIfNeeded(linkedItem);
    }
  }
}
