using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileMarketViewModel : ViewModelBase, IMediaHorizontalItemsViewModel, ICollectionDataProvider2<VKList<Product>, MediaListItemViewModelBase>
  {
    private readonly List<string> _cachedSources = new List<string>();
    private const double CONTAINER_WIDTH = 480.0;
    private const double CONTAINER_HEIGHT = 236.0;
    private IProfileData _profileData;
    private bool _isGroup;
    private int _marketAlbumId;
    private int _offset;
    private readonly GenericCollectionViewModel2<VKList<Product>, MediaListItemViewModelBase> _itemsVM;
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

    public Func<VKList<Product>, ListWithCount<MediaListItemViewModelBase>> ConverterFunc
    {
      get
      {
        return (Func<VKList<Product>, ListWithCount<MediaListItemViewModelBase>>) (products =>
        {
          ListWithCount<MediaListItemViewModelBase> listWithCount = new ListWithCount<MediaListItemViewModelBase>()
          {
            TotalCount = products.count
          };
          foreach (Product product in products.items)
          {
            ProductMediaListItemViewModel listItemViewModel = new ProductMediaListItemViewModel(product);
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
        return 236.0;
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
        return CommonResources.Profile_Products;
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
          nullable = counters != null ? new int?(counters.market) : new int?();
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
        return new Action(this.NavigateToProducts);
      }
    }

    public Action<MediaListItemViewModelBase> ItemTapAction
    {
      get
      {
        return (Action<MediaListItemViewModelBase>) (item =>
        {
          ProductMediaListItemViewModel listItemViewModel = item as ProductMediaListItemViewModel;
          if (listItemViewModel == null)
            return;
          CurrentMarketItemSource.Source = MarketItemSource.group_module;
          Navigator.Current.NavigateToProduct(listItemViewModel.Product.owner_id, listItemViewModel.Product.id);
        });
      }
    }

    public ProfileMarketViewModel()
    {
      this._items = new ObservableCollection<MediaListItemViewModelBase>();
      this._itemsVM = new GenericCollectionViewModel2<VKList<Product>, MediaListItemViewModelBase>((ICollectionDataProvider2<VKList<Product>, MediaListItemViewModelBase>) this)
      {
        LoadCount = 28
      };
    }

    public void Init(IProfileData profileData)
    {
      this._profileData = profileData;
      this._isGroup = this._profileData is GroupData;
      List<MediaListItemViewModelBase> items = ProfileMarketViewModel.CreateItems(profileData);
      if (!this._profileData.isMarketMainAlbumEmpty && this._isGroup)
      {
        Group group = ((GroupData) this._profileData).group;
        int? nullable;
        if (group == null)
        {
          nullable = new int?();
        }
        else
        {
            VKClient.Common.Backend.DataObjects.Market market = group.market;
          nullable = market != null ? new int?(market.main_album_id) : new int?();
        }
        this._marketAlbumId = nullable ?? 0;
      }
      if (!ProfileMarketViewModel.AreItemsEqual((IList<MediaListItemViewModelBase>) items, (IList<MediaListItemViewModelBase>) this._itemsVM.Collection))
      {
        this._offset = items.Count;
        this._cachedSources.Clear();
        foreach (ProductMediaListItemViewModel listItemViewModel in items)
          this._cachedSources.Add(listItemViewModel.ImageUri);
        this.Items = new ObservableCollection<MediaListItemViewModelBase>(items);
        this._itemsVM.LoadData(true, true, true, true, (Action<List<MediaListItemViewModelBase>>) null, (Action<BackendResult<VKList<Product>, ResultCode>>) null, false);
      }
      this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.Count));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanDisplay));
    }

    private static List<MediaListItemViewModelBase> CreateItems(IProfileData profileData)
    {
      List<MediaListItemViewModelBase> itemViewModelBaseList = new List<MediaListItemViewModelBase>();
      if ((profileData != null ? profileData.products : (List<Product>) null) == null)
        return itemViewModelBaseList;
      itemViewModelBaseList.AddRange((IEnumerable<MediaListItemViewModelBase>) profileData.products.Select<Product, ProductMediaListItemViewModel>((Func<Product, ProductMediaListItemViewModel>) (item => new ProductMediaListItemViewModel(item))));
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

    public void GetData(GenericCollectionViewModel2<VKList<Product>, MediaListItemViewModelBase> caller, int offset, int count, Action<BackendResult<VKList<Product>, ResultCode>> callback)
    {
      MarketService.Instance.GetProducts(this._isGroup ? -this._profileData.Id : this._profileData.Id, (long) this._marketAlbumId, count, this._offset, (Action<BackendResult<VKList<Product>, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
          this._offset = this._offset + count;
        callback(result);
      }));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel2<VKList<Product>, MediaListItemViewModelBase> caller, int count)
    {
      return "";
    }

    public void Unload()
    {
      foreach (ProductMediaListItemViewModel listItemViewModel in (Collection<MediaListItemViewModelBase>) this._items)
        listItemViewModel.ImageUri = null;
    }

    public void Reload()
    {
      if (this._cachedSources.Count != this._items.Count)
        return;
      for (int index = 0; index < this._cachedSources.Count; ++index)
        ((ProductMediaListItemViewModel) this._items[index]).ImageUri = this._cachedSources[index];
    }

    private void NavigateToProducts()
    {
      Navigator.Current.NavigateToMarket(this._isGroup ? -this._profileData.Id : this._profileData.Id);
    }

    public void LoadMoreItems(object linkedItem)
    {
      this._itemsVM.LoadMoreIfNeeded(linkedItem);
    }
  }
}
