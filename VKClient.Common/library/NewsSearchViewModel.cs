using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
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

        public TrendingTopicsViewModel TrendsViewModel { get; private set; }

        public GenericCollectionViewModel2<NewsFeedData, IVirtualizable> SearchVM { get; private set; }

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
                    this.SearchVM.LoadData(true, false, true, true, null, null, false);
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
                base.NotifyPropertyChanged<Visibility>(() => this.NewsVisibility);
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
                return delegate(NewsFeedData newsFeedData)
                {
                    this._from = newsFeedData.next_from;
                    ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>
                    {
                        TotalCount = newsFeedData.TotalCount
                    };
                    using (List<NewsItem>.Enumerator enumerator = newsFeedData.items.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            NewsItem current = enumerator.Current;
                            listWithCount.List.Add(new WallPostItem(480.0, default(Thickness), true, new NewsItemDataWithUsersAndGroupsInfo
                            {
                                NewsItem = current,
                                Profiles = newsFeedData.profiles,
                                Groups = newsFeedData.groups
                            }, new Action<WallPostItem>(this.ItemDeletedCallback), false, null, false, false, true, true, null, null));
                        }
                    }
                    return listWithCount;
                };
            }
        }

        public NewsSearchViewModel()
        {
            this.TrendsViewModel = new TrendingTopicsViewModel();

            this.SearchVM = new GenericCollectionViewModel2<NewsFeedData, IVirtualizable>(this);
            this.TrendsViewModel.Load();
        }

        public void Search(string query)
        {
            this.Query = query;
        }

        public void Refresh()
        {
            this.SearchVM.LoadData(true, false, false, false, null, null, false);
        }

        public void GetData(GenericCollectionViewModel2<NewsFeedData, IVirtualizable> caller, int offset, int count, Action<BackendResult<NewsFeedData, ResultCode>> callback)
        {
            if (offset > 0 && string.IsNullOrWhiteSpace(this._from))
            {
                callback(new BackendResult<NewsFeedData, ResultCode>(ResultCode.Succeeded, new NewsFeedData()));
            }
            else
            {
                string str = offset == 0 ? "" : this._from;
                if (offset == 0)
                {
                    this._startTime = VKClient.Common.Utils.Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow - TimeSpan.FromDays(365.0), true);
                    this._endTime = VKClient.Common.Utils.Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow, true);
                }
                NewsFeedService.Current.Search(this._query, 20, this._startTime, this._endTime, str, callback);
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
            this.SearchVM.Delete(obj);
        }
    }
}
