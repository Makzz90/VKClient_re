using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class NewsSearchViewModel : ViewModelBase, ICollectionDataProvider2<NewsFeedData, IVirtualizable>, ISearchWallPostsViewModel
  {
    private int _startTime;
    private int _endTime;
    private string _from;
    private string _query;
    private Visibility _trendsVisibility;

    public TrendingTopicsViewModel TrendsViewModel { get { return new TrendingTopicsViewModel(); } }

    public GenericCollectionViewModel2<NewsFeedData, IVirtualizable> SearchVM { get; set; }

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
          this.TrendsVisibility = Visibility.Collapsed;
          this.SearchVM.LoadData(true, false, true, true, (Action<List<IVirtualizable>>) null, (Action<BackendResult<NewsFeedData, ResultCode>>) null, false);
        }
        else
          this.TrendsVisibility = Visibility.Visible;
      }
    }

    public Visibility TrendsVisibility
    {
      get
      {
        return this._trendsVisibility;
      }
      set
      {
        this._trendsVisibility = value;
        this.NotifyPropertyChanged("TrendsVisibility");
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.NewsVisibility));
      }
    }

    public Visibility NewsVisibility
    {
      get
      {
        return (this._trendsVisibility == Visibility.Collapsed).ToVisiblity();
      }
    }

    public int ItemsCount
    {
      get
      {
        return this.SearchVM.Collection.Count;
      }
    }

    public Func<NewsFeedData, ListWithCount<IVirtualizable>> ConverterFunc
    {
      get
      {
        return (Func<NewsFeedData, ListWithCount<IVirtualizable>>) (newsFeedData =>
        {
          this._from = newsFeedData.next_from;
          ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>()
          {
            TotalCount = newsFeedData.TotalCount
          };
          foreach (NewsItem newsItem in newsFeedData.items)
          {
            List<IVirtualizable> list = listWithCount.List;
            double width = 480.0;
            Thickness margin = new Thickness();
            int num1 = 1;
            NewsItemDataWithUsersAndGroupsInfo wallPostWithInfo = new NewsItemDataWithUsersAndGroupsInfo();
            wallPostWithInfo.NewsItem = newsItem;
            List<User> profiles = newsFeedData.profiles;
            wallPostWithInfo.Profiles = profiles;
            List<Group> groups = newsFeedData.groups;
            wallPostWithInfo.Groups = groups;
            Action<WallPostItem> deletedItemCallback = new Action<WallPostItem>(this.ItemDeletedCallback);
            int num2 = 0;
            object local1 = null;
            int num3 = 0;
            int num4 = 0;
            int num5 = 1;
            int num6 = 1;
            object local2 = null;
            object local3 = null;
            WallPostItem wallPostItem = new WallPostItem(width, margin, num1 != 0, wallPostWithInfo, deletedItemCallback, num2 != 0, (Action<long, User, Group>) local1, num3 != 0, num4 != 0, num5 != 0, num6 != 0, (NewsFeedAdsItem) local2, (Func<List<MenuItem>>) local3);
            list.Add((IVirtualizable) wallPostItem);
          }
          return listWithCount;
        });
      }
    }

    public NewsSearchViewModel()
    {
      this.SearchVM = new GenericCollectionViewModel2<NewsFeedData, IVirtualizable>((ICollectionDataProvider2<NewsFeedData, IVirtualizable>) this);
      this.TrendsViewModel.Load();
    }

    public void Search(string query)
    {
      this.Query = query;
    }

    public void Refresh()
    {
      this.SearchVM.LoadData(true, false, false, false, (Action<List<IVirtualizable>>) null, (Action<BackendResult<NewsFeedData, ResultCode>>) null, false);
    }

    public void GetData(GenericCollectionViewModel2<NewsFeedData, IVirtualizable> caller, int offset, int count, Action<BackendResult<NewsFeedData, ResultCode>> callback)
    {
      if (offset > 0 && string.IsNullOrWhiteSpace(this._from))
      {
        callback(new BackendResult<NewsFeedData, ResultCode>(ResultCode.Succeeded, new NewsFeedData()));
      }
      else
      {
        string startFrom = offset == 0 ? "" : this._from;
        if (offset == 0)
        {
          this._startTime = VKClient.Common.Utils.Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow - TimeSpan.FromDays(365.0), true);
          this._endTime = VKClient.Common.Utils.Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true);
        }
        NewsFeedService.Current.Search(this._query, 20, this._startTime, this._endTime, startFrom, callback);
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel2<NewsFeedData, IVirtualizable> caller, int count)
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
