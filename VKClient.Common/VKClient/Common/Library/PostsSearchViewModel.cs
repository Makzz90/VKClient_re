using System;
using System.Collections.Generic;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class PostsSearchViewModel : ViewModelBase, ICollectionDataProvider2<WallData, IVirtualizable>, ISearchWallPostsViewModel
  {
    private readonly long _ownerId;
    private readonly string _domain;
    private string _query;

    public GenericCollectionViewModel2<WallData, IVirtualizable> SearchVM { get; set; }

    public string Query
    {
      get
      {
        return this._query;
      }
      private set
      {
        this._query = value;
        if (!string.IsNullOrEmpty(this._query))
        {
          this.NewsVisibility = Visibility.Visible;
          this.SearchVM.LoadData(true, false, true, true, (Action<List<IVirtualizable>>) null, (Action<BackendResult<WallData, ResultCode>>) null, false);
        }
        else
          this.NewsVisibility = Visibility.Collapsed;
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.NewsVisibility));
      }
    }

    public Visibility NewsVisibility { get; private set; }

    public Visibility TrendsVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public int ItemsCount
    {
      get
      {
        return this.SearchVM.Collection.Count;
      }
    }

    public Func<WallData, ListWithCount<IVirtualizable>> ConverterFunc
    {
      get
      {
        return (Func<WallData, ListWithCount<IVirtualizable>>) (wallData =>
        {
          ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>();
          List<IVirtualizable> virtualizableList = WallPostItemsGenerator.Generate(wallData.items, wallData.profiles, wallData.groups, new Action<WallPostItem>(this.ItemDeletedCallback), 0.0);
          listWithCount.List.AddRange((IEnumerable<IVirtualizable>) virtualizableList);
          int totalCount = wallData.TotalCount;
          listWithCount.TotalCount = totalCount;
          return listWithCount;
        });
      }
    }

    public PostsSearchViewModel(long ownerId)
      : this()
    {
      this._ownerId = ownerId;
    }

    public PostsSearchViewModel(string domain)
      : this()
    {
      this._domain = domain;
    }

    private PostsSearchViewModel()
    {
      this.SearchVM = new GenericCollectionViewModel2<WallData, IVirtualizable>((ICollectionDataProvider2<WallData, IVirtualizable>) this);
    }

    public void Search(string query)
    {
      this.Query = query;
    }

    public void Refresh()
    {
      this.SearchVM.LoadData(true, false, false, false, (Action<List<IVirtualizable>>) null, (Action<BackendResult<WallData, ResultCode>>) null, false);
    }

    public void GetData(GenericCollectionViewModel2<WallData, IVirtualizable> caller, int offset, int count, Action<BackendResult<WallData, ResultCode>> callback)
    {
      WallService.Current.Search(this._ownerId, this._domain, this._query, count, offset, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel2<WallData, IVirtualizable> caller, int count)
    {
      if (count == 0)
        return CommonResources.NoNews;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneNewsFrm, CommonResources.TwoFourNewsFrm, CommonResources.FiveNewsFrm, true, null, false);
    }

    private void ItemDeletedCallback(WallPostItem obj)
    {
      this.SearchVM.Delete((IVirtualizable) obj);
    }
  }
}
