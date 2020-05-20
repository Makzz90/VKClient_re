using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
    public class GenericCollectionViewModel<B, T> : ViewModelBase, IMarker, ISupportCollectionEdit, ISupportLoadMore, ISupportReload
        where B : class
        where T : class
    {
        public readonly int DEFAULT_LOAD_COUNT = 30;
        public readonly int DEFAULT_RELOAD_COUNT = 40;
        public readonly int DEFAULT_OFFSET_KNOB = 15;
        private ObservableCollection<T> _collection = new ObservableCollection<T>();
        private Dictionary<string, string> _collectionItemsDict = new Dictionary<string, string>();
        private int _totalCount = -1;
        private MergedCollection _mergedCollection = new MergedCollection();
        private string _noContentText = "";
        private bool _canShowProgress = true;
        private Visibility _noContentNewsButtonsVisibility = Visibility.Collapsed;
        private GenericCollectionViewModel<B, T>.Status _status;
        private bool _noDataOnLastFetch;
        private bool _isLoaded;
        private bool _isLoading;
        private ICollectionDataProvider<B, T> _parent;
        private ICommand _tryAgainCmd;
        private ICommand _settingsCmd;
        private bool _refresh;
        private string _noContentImage;
        public bool _updatingCollection;

        public MergedCollection MergedCollection
        {
            get
            {
                return this._mergedCollection;
            }
            set
            {
                this._mergedCollection = value;
            }
        }

        public bool Refresh
        {
            get
            {
                return this._refresh;
            }
        }

        public ICommand TryAgainCmd
        {
            get
            {
                return this._tryAgainCmd;
            }
        }

        public ICommand SettingsCmd
        {
            get
            {
                return this._settingsCmd;
            }
        }

        public ObservableCollection<T> Collection
        {
            get
            {
                return this._collection;
            }
            private set
            {
                this._collection = value;
                this.NotifyPropertyChanged("Collection");
            }
        }

        public int LoadCount { get; set; }

        public int ReloadCount { get; set; }

        public int OffsetKnob { get; set; }

        public bool IsLoaded
        {
            get
            {
                return this._isLoaded;
            }
        }

        public int TotalCount
        {
            get
            {
                return this._totalCount;
            }
            set
            {
                this._totalCount = value;
            }
        }

        public string NoItemsDescription { get; set; }

        public Visibility IsLoadedVisibility
        {
            get
            {
                if (!this._isLoaded)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool IsFullyLoaded
        {
            get
            {
                if (!this.IsLoaded || !this._noDataOnLastFetch && (this._totalCount < 0 || this.GetCollectionCount() != this._totalCount))
                    return this._totalCount == int.MinValue;
                return true;
            }
        }

        public bool NeedCollectionCountBeforeFullyLoading { get; set; }

        public string FooterText
        {
            get
            {
                if (this.StatusText != "" || !this.IsFullyLoaded && (!this.NeedCollectionCountBeforeFullyLoading || !this.IsLoaded))
                    return "";
                int count = this.GetCollectionCount();
                if (this.NeedCollectionCountBeforeFullyLoading && this._totalCount > 0)
                    count = this._totalCount == -1 ? 0 : this._totalCount;
                if (count == 0 && !string.IsNullOrEmpty(this.NoItemsDescription) || count == 0 && (!string.IsNullOrEmpty(this.NoContentImage) || !string.IsNullOrEmpty(this.NoContentText)))
                    return "";
                return this._parent.GetFooterTextForCount(this, count);
            }
        }

        public string NoContentImage
        {
            get
            {
                return this._noContentImage;
            }
            set
            {
                if (this._noContentImage == value)
                    return;
                this._noContentImage = value;
                this.NotifyPropertyChanged<string>((() => this.NoContentImage));
                this.NotifyPropertyChanged<Visibility>((() => this.NoContentBlockVisibility));
                this.NotifyPropertyChanged<string>((() => this.FooterText));
            }
        }

        public string NoContentText
        {
            get
            {
                return this._noContentText;
            }
            set
            {
                if (this._noContentText == value)
                    return;
                this._noContentText = value;
                this.NotifyPropertyChanged<string>((() => this.NoContentText));
                this.NotifyPropertyChanged<Visibility>((() => this.NoContentBlockVisibility));
                this.NotifyPropertyChanged<string>((() => this.FooterText));
            }
        }

        public bool CanShowProgress
        {
            get
            {
                return this._canShowProgress;
            }
            set
            {
                if (this._canShowProgress == value)
                    return;
                this._canShowProgress = value;
            }
        }

        public Visibility NoContentBlockVisibility
        {
            get
            {
                if (string.IsNullOrEmpty(this.NoContentImage) && string.IsNullOrEmpty(this.NoContentText))
                    return Visibility.Collapsed;
                if (this.StatusText != "" || !this.IsFullyLoaded)
                    return Visibility.Collapsed;
                if (this.GetCollectionCount() != 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility NoContentNewsButtonsVisibility
        {
            get
            {
                return this._noContentNewsButtonsVisibility;
            }
            set
            {
                if (this._noContentNewsButtonsVisibility == value)
                    return;
                this._noContentNewsButtonsVisibility = value;
            }
        }

        public Visibility NoItemsDescriptionVisibility
        {
            get
            {
                if (string.IsNullOrEmpty(this.NoItemsDescription) || !this.IsFullyLoaded || (!(this.StatusText == "") || this.GetCollectionCount() != 0))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility FooterTextVisibility
        {
            get
            {
                if (!(this.FooterText != "") || !(this.StatusText == ""))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public string StatusText
        {
            get
            {
                if (this._status.IsLoading)
                    return CommonResources.Loading;
                if (!this._status.ResultCode.HasValue)
                    return "";
                switch (this._status.ResultCode.Value)
                {
                    case ResultCode.DeletedOrBanned:
                        return CommonResources.UserDeletedOrBanned;
                    case ResultCode.AccessDeniedExtended:
                    case ResultCode.AccessDenied:
                        return CommonResources.Error_AccessDenied;
                    case ResultCode.CommunicationFailed:
                        return CommonResources.FailedToConnectError.Replace("\\r\\n", Environment.NewLine);
                    default:
                        return CommonResources.Error_Generic;
                }
            }
        }

        public Visibility StatusTextVisibility
        {
            get
            {
                if (string.IsNullOrEmpty(this.StatusText) || !this.CanShowProgress)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool IsLoading
        {
            get
            {
                return this._status.IsLoading;
            }
        }

        public Visibility IsLoadingVisibility
        {
            get
            {
                if (!this.IsLoading || !this.CanShowProgress)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility TryAgainVisibility
        {
            get
            {
                if (!this._status.ResultCode.HasValue)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility SettingsVisibility
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        private ViewModelBase MainVM
        {
            get
            {
                if (this.Page != null)
                    return ((FrameworkElement)this.Page).DataContext as ViewModelBase;
                return null;
            }
        }

        private PhoneApplicationPage Page
        {
            get
            {
                return VKClient.Common.Framework.CodeForFun.TemplatedVisualTreeExtensions.GetFirstLogicalChildByType<PhoneApplicationPage>( this.RootVisual, false);
                //return this.RootVisual.GetFirstLogicalChildByType(false);
            }
        }

        private Frame RootVisual
        {
            get
            {
                return Application.Current.RootVisual as Frame;
            }
        }

        public bool Refreshing { get; set; }

        public bool RecreateCollectionOnRefresh { get; set; }

        public ICollectionDataProvider<B, T> ParentViewModel
        {
            get
            {
                return this._parent;
            }
        }

        public event EventHandler StartedEdit;

        public event EventHandler EndedEdit;

        public GenericCollectionViewModel(ICollectionDataProvider<B, T> parent)
        {
            this._parent = parent;
            this.Initialize();
            this._collection.CollectionChanged += new NotifyCollectionChangedEventHandler(this.CollectionChanged);
        }

        private int GetCollectionCount()
        {
            if (!typeof(IItemsCount).IsAssignableFrom(typeof(T)))
                return this.Collection.Count;
            int num = 0;
            foreach (T obj in (System.Collections.ObjectModel.Collection<T>)this.Collection)
            {
                IItemsCount itemsCount = (object)obj as IItemsCount;
                if (itemsCount != null)
                    num += itemsCount.GetItemsCount();
            }
            return num;
        }

        private int GetElementsCountForItem(T item)
        {
            if (item is IItemsCount)
                return (item as IItemsCount).GetItemsCount();
            return 1;
        }

        private void Initialize()
        {
            this.LoadCount = this.DEFAULT_LOAD_COUNT;
            this.ReloadCount = this.DEFAULT_RELOAD_COUNT;
            this.OffsetKnob = this.DEFAULT_OFFSET_KNOB;
            this._status = new GenericCollectionViewModel<B, T>.Status();
            this._tryAgainCmd = (ICommand)new DelegateCommand((Action<object>)(o => this.Reload()));
            this._settingsCmd = (ICommand)new DelegateCommand((Action<object>)(type => this.NavigateToCommunicationSettings((ConnectionSettingsType)Enum.Parse(typeof(ConnectionSettingsType), type.ToString(), false))));
        }

        public void Reload()
        {
            this.LoadData(this._refresh, false, (Action<BackendResult<B, ResultCode>>)null, false);
        }

        public void LoadMoreIfNeeded(object linkedItem)
        {
            int count1 = this.Collection.Count;
            if (this.MergedCollection != null && this.MergedCollection.Count > 0)
            {
                int count2 = this.MergedCollection.Count;
                if (count2 >= this.OffsetKnob && (count2 < this.OffsetKnob || this.MergedCollection[count2 - this.OffsetKnob] != linkedItem))
                    return;
                this.LoadData(false, false, (Action<BackendResult<B, ResultCode>>)null, false);
            }
            else
            {
                if ((!(linkedItem is T) || count1 >= this.OffsetKnob) && (count1 < this.OffsetKnob || (object)this.Collection[count1 - this.OffsetKnob] != linkedItem))
                    return;
                this.LoadData(false, false, (Action<BackendResult<B, ResultCode>>)null, false);
            }
        }

        public void LoadData(bool refresh = false, bool suppressLoadingMessage = false, Action<BackendResult<B, ResultCode>> callback = null, bool clearCollectionOnRefresh = false)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (this._isLoading || this.IsFullyLoaded && !refresh)
                    return;
                this._isLoading = true;
                this._refresh = refresh;
                this.UpdateStatus(!suppressLoadingMessage, new ResultCode?());
                ViewModelBase _mainVM = this.MainVM;
                if (refresh && _mainVM != null && !suppressLoadingMessage)
                    _mainVM.SetInProgress(true, CommonResources.Refreshing);
                if (clearCollectionOnRefresh & refresh)
                    this.Collection = new ObservableCollection<T>();
                this._parent.GetData(this, refresh ? 0 : this.GetCollectionCount(), refresh ? this.LoadCount : this.ReloadCount, (Action<BackendResult<B, ResultCode>>)(res =>
                {
                    if (_mainVM != null)
                        _mainVM.SetInProgress(false, "");
                    this.UpdateStatus(false, new ResultCode?());
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        this._isLoaded = true;
                        this.ReadData(refresh, res.ResultData, (Action)(() =>
                        {
                            if (callback == null)
                                return;
                            callback(res);
                        }));
                    }
                    else
                    {
                        this.UpdateStatus(false, new ResultCode?(res.ResultCode));
                        this._isLoading = false;
                        if (callback == null)
                            return;
                        callback(res);
                    }
                }));
            }));
        }

        private void UpdateStatus(bool isLoading, ResultCode? resultCode)
        {
            this._status.IsLoading = isLoading;
            this._status.ResultCode = resultCode;
            this.NotifyChanged();
        }

        public void NotifyChanged()
        {
            this.NotifyPropertyChanged<Visibility>((() => this.StatusTextVisibility));
            this.NotifyPropertyChanged<bool>((() => this.IsLoading));
            this.NotifyPropertyChanged<Visibility>((() => this.IsLoadingVisibility));
            this.NotifyPropertyChanged<string>((() => this.StatusText));
            this.NotifyPropertyChanged<string>((() => this.FooterText));
            this.NotifyPropertyChanged<Visibility>((() => this.FooterTextVisibility));
            this.NotifyPropertyChanged<string>((() => this.NoContentImage));
            this.NotifyPropertyChanged<string>((() => this.NoContentText));
            this.NotifyPropertyChanged<Visibility>((() => this.NoContentBlockVisibility));
            this.NotifyPropertyChanged<Visibility>((() => this.NoContentNewsButtonsVisibility));
            this.NotifyPropertyChanged<Visibility>((() => this.NoItemsDescriptionVisibility));
            this.NotifyPropertyChanged<Visibility>((() => this.TryAgainVisibility));
            this.NotifyPropertyChanged<Visibility>((() => this.IsLoadedVisibility));
        }

        public void ReadData(ListWithCount<T> listWithCount)
        {
            this._updatingCollection = true;
            this._totalCount = listWithCount.TotalCount;
            bool receivedNewData = false;
            this.RaiseOnStartedEdit();
            listWithCount.List.ForEach((Action<T>)(t =>
            {
                IList list1 = t as IList;
                bool flag1 = false;
                if (list1 != null)
                {
                    int index = listWithCount.List.IndexOf(t);
                    if (index >= 0 && index < this._collection.Count)
                    {
                        IList list2 = this._collection[index] as IList;
                        foreach (object obj in list1)
                            list2.Add(obj);
                        receivedNewData = true;
                        flag1 = true;
                    }
                }
                if (flag1)
                    return;
                bool flag2 = true;
                IHaveUniqueKey haveUniqueKey = t as IHaveUniqueKey;
                if (haveUniqueKey != null && this._collectionItemsDict.ContainsKey(haveUniqueKey.GetKey()))
                    flag2 = false;
                if (!flag2)
                    return;
                this._collection.Add(t);
                this.AddRemoveKeyToDict(t, true);
                receivedNewData = true;
            }));
            this.RaiseOnEndedEdit();
            this._noDataOnLastFetch = listWithCount.List.Count == 0 || !receivedNewData;
            this._updatingCollection = false;
        }

        private void ReadData(bool refresh, B backendData, Action callback)
        {
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (refresh)
                {
                    this.Refreshing = true;
                    if (this.RecreateCollectionOnRefresh)
                        this.Collection = new ObservableCollection<T>();
                    else
                        this._collection.Clear();
                    this._collectionItemsDict.Clear();
                }
                this.ReadData(this._parent.ConverterFunc(backendData));
                this.Refreshing = false;
                this._isLoading = false;
                this.NotifyChanged();
                callback();
            }));
        }

        private void AddRemoveKeyToDict(T t, bool add)
        {
            IHaveUniqueKey haveUniqueKey = (object)t as IHaveUniqueKey;
            if (haveUniqueKey == null)
                return;
            string key = haveUniqueKey.GetKey();
            if (string.IsNullOrEmpty(key))
                return;
            if (add)
                this._collectionItemsDict[key] = key;
            else
                this._collectionItemsDict.Remove(key);
        }

        private void NavigateToCommunicationSettings(ConnectionSettingsType type)
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            ConnectionSettingsType connectionSettingsType = type;
            connectionSettingsTask.ConnectionSettingsType=(connectionSettingsType);
            connectionSettingsTask.Show();
        }

        public void Delete(T item)
        {
            if (item == null)
                return;
            this._updatingCollection = true;
            if (this._collection.Remove(item))
            {
                this.AddRemoveKeyToDict(item, false);
                this._totalCount = this._totalCount - this.GetElementsCountForItem(item);
                this.NotifyChanged();
            }
            this._updatingCollection = false;
        }

        public void DeleteAt(int ind)
        {
            if (ind < 0 || ind > this._collection.Count - 1)
                return;
            this.Delete(this._collection[ind]);
        }

        public void Insert(T item, int ind)
        {
            this._updatingCollection = true;
            this._collection.Insert(ind, item);
            this.AddRemoveKeyToDict(item, true);
            this._updatingCollection = false;
            this._totalCount = this._totalCount + this.GetElementsCountForItem(item);
            this.NotifyChanged();
        }

        public void InsertRange(IEnumerable<T> items, int ind)
        {
            this.RaiseOnStartedEdit();
            foreach (T obj in items)
                this.Insert(obj, ind++);
            this.RaiseOnEndedEdit();
        }

        public bool ItemExists(T item)
        {
            IHaveUniqueKey haveUniqueKey = item as IHaveUniqueKey;
            if (haveUniqueKey != null)
                return this._collectionItemsDict.ContainsKey(haveUniqueKey.GetKey());
            return false;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this._updatingCollection || !(this._parent is ISupportReorder<T>) || e.Action != NotifyCollectionChangedAction.Add)
                return;
            T before = default(T);
            T after = default(T);
            T obj = this._collection[e.NewStartingIndex];
            if (e.NewStartingIndex > 0)
                after = this._collection[e.NewStartingIndex - 1];
            if (e.NewStartingIndex < this._collection.Count - 1)
                before = this._collection[e.NewStartingIndex + 1];
            (this._parent as ISupportReorder<T>).Reordered(obj, before, after);
        }

        private void RaiseOnStartedEdit()
        {
            if (this.StartedEdit != null)
                this.StartedEdit(this, EventArgs.Empty);
        }

        private void RaiseOnEndedEdit()
        {
            if (this.EndedEdit != null)
                this.EndedEdit(this, EventArgs.Empty);
        }

        public void LoadMore()
        {
            if (this._status.ResultCode.HasValue)
                return;
            this.LoadData(false, true, null, false);
        }

        private class Status
        {
            public bool IsLoading { get; set; }

            public ResultCode? ResultCode { get; set; }
        }
    }
}
