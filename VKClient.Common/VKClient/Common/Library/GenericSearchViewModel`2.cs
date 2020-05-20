using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
    public class GenericSearchViewModel<B, T> : GenericSearchViewModelBase, IHandle<UserIsLoggedOutEvent>, IHandle, ICollectionDataProvider2<VKList<B>, T>, ILocalCollectionDataProvider<T>
        where B : class
        where T : class, ISearchableItemHeader<B>
    {
        private readonly ISearchDataProvider<B, T> _searchDataProvider;
        private string _searchString;

        public override Dictionary<string, string> Parameters { get; set; }

        public override string SearchString
        {
            get
            {
                return this._searchString;
            }
            set
            {
                if (!(this._searchString != value))
                    return;
                this._searchString = value;
                this.NotifyPropertyChanged("SearchString");
                this.SearchVM.LoadData(true, false, true, false, (Action<List<T>>)null, (Action<BackendResult<VKList<B>, ResultCode>>)null, false);
            }
        }

        public GenericCollectionViewModel2<VKList<B>, T> SearchVM { get; set; }

        public Func<VKList<B>, ListWithCount<T>> ConverterFunc
        {
            get
            {
                return this._searchDataProvider.ConverterFunc;
            }
        }

        public string LocalGroupName
        {
            get
            {
                return this._searchDataProvider.LocalGroupName;
            }
        }

        public string GlobalGroupName
        {
            get
            {
                return this._searchDataProvider.GlobalGroupName;
            }
        }

        public Func<T, bool> GetIsLocalItem
        {
            get
            {
                return (Func<T, bool>)(item => item.IsLocalItem);
            }
        }

        public GenericSearchViewModel(ISearchDataProvider<B, T> searchDataProvider)
        {
            this.Parameters = new Dictionary<string, string>();

            this._searchDataProvider = searchDataProvider;
            this.SearchVM = new GenericCollectionViewModel2<VKList<B>, T>((ICollectionDataProvider2<VKList<B>, T>)this);
            EventAggregator.Current.Subscribe((object)this);
        }

        public void GetData(GenericCollectionViewModel2<VKList<B>, T> caller, int offset, int count, Action<BackendResult<VKList<B>, ResultCode>> callback)
        {
            this._searchDataProvider.GetData(this.SearchString, this.Parameters, offset, count, callback);
        }

        public string GetFooterTextForCount(GenericCollectionViewModel2<VKList<B>, T> caller, int count)
        {
            return this._searchDataProvider.GetFooterTextForCount(count);
        }

        public void GetLocalData(Action<List<T>> callback)
        {
            if (this._searchDataProvider.LocalItems == null || !this._searchDataProvider.LocalItems.Any<T>())
            {
                callback(new List<T>());
            }
            else
            {
                string str = this._searchString;
                string searchString = (str != null ? str.ToLowerInvariant().Trim() : null) ?? "";
                if (string.IsNullOrWhiteSpace(searchString))
                    callback(new List<T>());
                else
                    callback(new List<T>(this._searchDataProvider.LocalItems.Where<T>((Func<T, bool>)(item => item.Matches(searchString)))));
            }
        }

        public override void LoadData(bool refresh = false, bool suppressLoadingMessage = false, bool suppressSystemTrayProgress = false, bool clearCollectionOnRefresh = false, bool instantLoad = false)
        {
            this.SearchVM.LoadData(refresh, suppressLoadingMessage, suppressSystemTrayProgress, clearCollectionOnRefresh, (Action<List<T>>)null, (Action<BackendResult<VKList<B>, ResultCode>>)null, instantLoad);
        }

        public override void LoadMoreIfNeeded(object linkedItem)
        {
            this.SearchVM.LoadMoreIfNeeded(linkedItem);
        }

        public void Handle(UserIsLoggedOutEvent message)
        {
            this.SearchString = "";
        }
    }
}
