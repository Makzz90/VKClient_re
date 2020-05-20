using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Common.Library
{
  public class GenericCollectionViewModel2<B, T> : ViewModelBase, IMarker, ISupportCollectionEdit, ISupportLoadMore, ISupportReload where B : class where T : class
  {
    private ObservableCollection<T> _collection = new ObservableCollection<T>();
    private readonly Dictionary<string, string> _collectionItemsDict = new Dictionary<string, string>();
    private int _totalCount = -1;
    private string _noContentText = "";
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private const int DELAY_GLOBAL_SEARCH = 300;
    private const int DEFAULT_LOAD_COUNT = 30;
    private const int DEFAULT_RELOAD_COUNT = 40;
    private const int DEFAULT_OFFSET_KNOB = 15;
    private GenericCollectionViewModel2<B, T>.Status _status;
    private ObservableCollection<Group<T>> _groupedCollection;
    private int _noDataOnLastFetch;
    private bool _isLoaded;
    private bool _isLoading;
    private Group<T> _localGroup;
    private Group<T> _globalGroup;
    private int _preloadedItemsCount;
    private readonly ICollectionDataProvider2<B, T> _dataProvider;
    private readonly ILocalCollectionDataProvider<T> _localDataProvider;
    private ICommand _tryAgainCmd;
    private ICommand _settingsCmd;
    private bool _refresh;
    private string _noContentImage;
    private readonly DelayedExecutor _delayedExecutor;
    private bool _updatingCollection;

    private bool IsGroupedMode
    {
      get
      {
        return this._localDataProvider != null;
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
      set
      {
        this._collection = value;
        this.NotifyPropertyChanged("Collection");
      }
    }

    public ObservableCollection<Group<T>> GroupedCollection
    {
      get
      {
        return this._groupedCollection;
      }
      set
      {
        this._groupedCollection = value;
        this.NotifyPropertyChanged("GroupedCollection");
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
        return !this._isLoaded ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public bool IsFullyLoaded
    {
      get
      {
        int collectionCount = this.GetCollectionCount();
        if (this.IsGroupedMode)
          collectionCount -= this._preloadedItemsCount;
        if (!this.IsLoaded)
          return false;
        if (this._noDataOnLastFetch > 1)
          return true;
        if (this._totalCount > 0)
          return collectionCount == this._totalCount;
        return false;
      }
    }

    public string FooterText
    {
      get
      {
        if (this.StatusText != "" || !this.IsFullyLoaded || this.GetCollectionCount() == 0 && !string.IsNullOrEmpty(this.NoItemsDescription))
          return "";
        return this._dataProvider.GetFooterTextForCount(this, this.GetCollectionCount());
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
      }
    }

    public Visibility NoContentBlockVisibility
    {
      get
      {
        return string.IsNullOrEmpty(this.NoContentImage) && string.IsNullOrEmpty(this.NoContentText) || (this.StatusText != "" || !this.IsFullyLoaded) || this.GetCollectionCount() != 0 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility NoItemsDescriptionVisibility
    {
      get
      {
        return string.IsNullOrEmpty(this.NoItemsDescription) || !this.IsFullyLoaded || (!(this.StatusText == "") || this.GetCollectionCount() != 0) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility FooterTextVisibility
    {
      get
      {
        return !(this.FooterText != "") || !(this.StatusText == "") ? Visibility.Collapsed : Visibility.Visible;
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
        return string.IsNullOrEmpty(this.StatusText) ? Visibility.Collapsed : Visibility.Visible;
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
        return !this.IsLoading ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility TryAgainVisibility
    {
      get
      {
        return !this._status.ResultCode.HasValue ? Visibility.Collapsed : Visibility.Visible;
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
          return this.Page.DataContext as ViewModelBase;
        return (ViewModelBase) null;
      }
    }

    private PhoneApplicationPage Page
    {
      get
      {
        return this.RootVisual.GetFirstLogicalChildByType<PhoneApplicationPage>(false);
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

    public event EventHandler StartedEdit;

    public event EventHandler EndedEdit;

    public GenericCollectionViewModel2(ICollectionDataProvider2<B, T> dataProvider)
    {
      this._dataProvider = dataProvider;
      this._localDataProvider = this._dataProvider as ILocalCollectionDataProvider<T>;
      this._delayedExecutor = new DelayedExecutor(300);
      this.InitializeGroupedCollection();
      this.Initialize();
      this._collection.CollectionChanged += new NotifyCollectionChangedEventHandler(this.CollectionChanged);
    }

    private int GetCollectionCount()
    {
      if (this.IsGroupedMode)
        return this._globalGroup.Count + this._localGroup.Count;
      if (!typeof (IItemsCount).IsAssignableFrom(typeof (T)))
        return this._collection.Count;
      int num = 0;
      foreach (T obj in (System.Collections.ObjectModel.Collection<T>) this.Collection)
      {
        IItemsCount itemsCount = (object) obj as IItemsCount;
        if (itemsCount != null)
          num += itemsCount.GetItemsCount();
      }
      return num;
    }

    private static int GetElementsCountForItem(T item)
    {
      if ((object) item is IItemsCount)
        return ((object) item as IItemsCount).GetItemsCount();
      return 1;
    }

    private void InitializeGroupedCollection()
    {
      if (!this.IsGroupedMode)
        return;
      this._localGroup = new Group<T>(this._localDataProvider.LocalGroupName, false);
      this._globalGroup = new Group<T>(this._localDataProvider.GlobalGroupName, false);
      ObservableCollection<Group<T>> observableCollection = new ObservableCollection<Group<T>>();
      Group<T> group1 = this._localGroup;
      observableCollection.Add(group1);
      Group<T> group2 = this._globalGroup;
      observableCollection.Add(group2);
      this.GroupedCollection = observableCollection;
      this._preloadedItemsCount = 0;
    }

    private void Initialize()
    {
      this.LoadCount = 30;
      this.ReloadCount = 40;
      this.OffsetKnob = 15;
      this._status = new GenericCollectionViewModel2<B, T>.Status();
      this._tryAgainCmd = (ICommand) new DelegateCommand((Action<object>) (o => this.LoadData(this._refresh, false, false, false, (Action<List<T>>) null, (Action<BackendResult<B, ResultCode>>) null, false)));
      this._settingsCmd = (ICommand) new DelegateCommand((Action<object>) (type => this.NavigateToCommunicationSettings((ConnectionSettingsType) Enum.Parse(typeof (ConnectionSettingsType), type.ToString(), false))));
    }

    public void Reload()
    {
      this.LoadData(this._refresh, false, false, false, (Action<List<T>>) null, (Action<BackendResult<B, ResultCode>>) null, false);
    }

    public void LoadMoreIfNeeded(object linkedItem)
    {
      IList list = this.IsGroupedMode ? (IList) this._globalGroup : (IList) this.Collection;
      if (list == null)
        return;
      int count = list.Count;
      if ((!(linkedItem is T) || count >= this.OffsetKnob) && (count < this.OffsetKnob || list[count - this.OffsetKnob] != linkedItem) || this._isLoading)
        return;
      this.LoadData(false, false, false, false, (Action<List<T>>) null, (Action<BackendResult<B, ResultCode>>) null, false);
    }

    public void LoadData(bool refresh = false, bool suppressLoadingMessage = false, bool suppressSystemTrayProgress = false, bool clearCollectionOnRefresh = false, Action<List<T>> callbackLocal = null, Action<BackendResult<B, ResultCode>> callback = null, bool instantLoad = false)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        this._cancellationTokenSource.Cancel();
        this._cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = this._cancellationTokenSource.Token;
        if (this.IsFullyLoaded && !refresh)
          return;
        this._isLoading = true;
        this._refresh = refresh;
        this.UpdateStatus(!suppressLoadingMessage, new ResultCode?());
        ViewModelBase mainVM = this.MainVM;
        if (refresh && mainVM != null && (!suppressLoadingMessage && !suppressSystemTrayProgress))
          this.MainVM.SetInProgress(true, CommonResources.Refreshing);
        int offset = refresh ? 0 : this.GetCollectionCount();
        if (this.IsGroupedMode && !refresh)
          offset -= this._preloadedItemsCount;
        int count = refresh ? this.LoadCount : this.ReloadCount;
        if (clearCollectionOnRefresh & refresh)
          this.Collection = new ObservableCollection<T>();
        if (refresh)
          this._collectionItemsDict.Clear();
        if (this.IsGroupedMode & refresh)
        {
          if (instantLoad)
            this.DoLocalGlobalLoad(cancellationToken, mainVM, offset, count, refresh, suppressLoadingMessage, suppressSystemTrayProgress, clearCollectionOnRefresh, callbackLocal, callback);
          else
            this._delayedExecutor.AddToDelayedExecution((Action) (() => this.DoLocalGlobalLoad(cancellationToken, mainVM, offset, count, refresh, suppressLoadingMessage, suppressSystemTrayProgress, clearCollectionOnRefresh, callbackLocal, callback)));
        }
        else if (instantLoad)
          this.DoGlobalLoad(cancellationToken, mainVM, offset, count, refresh, suppressLoadingMessage, suppressSystemTrayProgress, clearCollectionOnRefresh, callbackLocal, callback);
        else
          this._delayedExecutor.AddToDelayedExecution((Action) (() => this.DoGlobalLoad(cancellationToken, mainVM, offset, count, refresh, suppressLoadingMessage, suppressSystemTrayProgress, clearCollectionOnRefresh, callbackLocal, callback)));
      }));
    }

    private void DoLocalGlobalLoad(CancellationToken cancellationToken, ViewModelBase mainVM, int offset, int count, bool refresh, bool suppressLoadingMessage, bool suppressSystemTrayProgress, bool clearCollectionOnRefresh, Action<List<T>> callbackLocal, Action<BackendResult<B, ResultCode>> callback = null)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        ViewModelBase viewModelBase = mainVM;
        if (viewModelBase != null)
        {
          int num = 0;
          string inProgressText = "";
          viewModelBase.SetInProgress(num != 0, inProgressText);
        }
        this.UpdateStatus(false, new ResultCode?());
        this._isLoading = false;
      }
      else
        this._localDataProvider.GetLocalData((Action<List<T>>) (res =>
        {
          this.InitializeGroupedCollection();
          this.ReadLocalData(res, (Action) (() =>
          {
            Action<List<T>> action = callbackLocal;
            if (action != null)
            {
              List<T> objList = res;
              action(objList);
            }
            this.DoGlobalLoad(cancellationToken, mainVM, offset, count, refresh, suppressLoadingMessage, suppressSystemTrayProgress, clearCollectionOnRefresh, callbackLocal, callback);
          }));
        }));
    }

    private void DoGlobalLoad(CancellationToken cancellationToken, ViewModelBase mainVM, int offset, int count, bool refresh, bool suppressLoadingMessage, bool suppressSystemTrayProgress, bool clearCollectionOnRefresh, Action<List<T>> callbackLocal, Action<BackendResult<B, ResultCode>> callback = null)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        ViewModelBase viewModelBase = mainVM;
        if (viewModelBase != null)
        {
          int num = 0;
          string inProgressText = "";
          viewModelBase.SetInProgress(num != 0, inProgressText);
        }
        this.UpdateStatus(false, new ResultCode?());
        this._isLoading = false;
      }
      else
        this._dataProvider.GetData(this, offset, count, (Action<BackendResult<B, ResultCode>>) (res =>
        {
          ViewModelBase viewModelBase = mainVM;
          if (viewModelBase != null)
          {
            int num = 0;
            string inProgressText = "";
            viewModelBase.SetInProgress(num != 0, inProgressText);
          }
          this.UpdateStatus(false, new ResultCode?());
          if (cancellationToken.IsCancellationRequested)
            this._isLoading = false;
          else if (res.ResultCode == ResultCode.Succeeded)
          {
            this._isLoaded = true;
            if (refresh)
              this.Collection = new ObservableCollection<T>();
            this.ReadData(refresh, res.ResultData, (Action<bool>) (needAnotherLoad =>
            {
              if (needAnotherLoad)
              {
                this.LoadData(refresh, suppressLoadingMessage, suppressSystemTrayProgress, clearCollectionOnRefresh, callbackLocal, callback, false);
              }
              else
              {
                Action<BackendResult<B, ResultCode>> action = callback;
                if (action == null)
                  return;
                BackendResult<B, ResultCode> backendResult = res;
                action(backendResult);
              }
            }));
          }
          else
          {
            this.UpdateStatus(false, new ResultCode?(res.ResultCode));
            this._isLoading = false;
            Action<BackendResult<B, ResultCode>> action = callback;
            if (action == null)
              return;
            BackendResult<B, ResultCode> backendResult = res;
            action(backendResult);
          }
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
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.StatusTextVisibility));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsLoading));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsLoadingVisibility));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.StatusText));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.FooterText));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.FooterTextVisibility));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.NoContentImage));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.NoContentText));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.NoContentBlockVisibility));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.NoItemsDescriptionVisibility));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.TryAgainVisibility));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsLoadedVisibility));
    }

    public bool ReadData(ListWithCount<T> listWithCount)
    {
      this._updatingCollection = true;
      bool flag1 = false;
      this._totalCount = listWithCount.TotalCount;
      IList collection = this.IsGroupedMode ? (IList) this._globalGroup : (IList) this._collection;
      bool receivedNewData = false;
      this.RaiseOnStartedEdit();
      listWithCount.List.ForEach((Action<T>) (listItem =>
      {
        IList list1 = (object) listItem as IList;
        if (list1 != null)
        {
          IList list2 = null;
          int index = listWithCount.List.IndexOf(listItem);
          if (index > -1 && index < collection.Count)
            list2 = collection[index] as IList;
          if (list2 == null)
          {
            if (list1.Count <= 0)
              return;
            bool flag2 = false;
            foreach (object t in (IEnumerable) list1)
            {
              IHaveUniqueKey haveUniqueKey = t as IHaveUniqueKey;
              if (haveUniqueKey != null && !this._collectionItemsDict.ContainsKey(haveUniqueKey.GetKey()))
              {
                this.AddRemoveKeyToDict(t, true);
                flag2 = true;
              }
            }
            if (!flag2)
              return;
            collection.Add((object) listItem);
            receivedNewData = true;
          }
          else
          {
            foreach (object t in (IEnumerable) list2)
            {
              bool flag2 = true;
              IHaveUniqueKey haveUniqueKey = t as IHaveUniqueKey;
              if (haveUniqueKey != null && this._collectionItemsDict.ContainsKey(haveUniqueKey.GetKey()))
                flag2 = false;
              if (flag2)
              {
                list2.Add(t);
                this.AddRemoveKeyToDict(t, true);
                receivedNewData = true;
              }
            }
          }
        }
        else
        {
          bool flag2 = true;
          IHaveUniqueKey haveUniqueKey = (object) listItem as IHaveUniqueKey;
          if (haveUniqueKey != null && this._collectionItemsDict.ContainsKey(haveUniqueKey.GetKey()))
            flag2 = false;
          if (!flag2)
            return;
          if (this.IsGroupedMode)
          {
            if (this._localDataProvider.GetIsLocalItem(listItem))
              this._localGroup.Add(listItem);
            else
              this._globalGroup.Add(listItem);
          }
          else
            collection.Add((object) listItem);
          this.AddRemoveKeyToDict((object) listItem, true);
          receivedNewData = true;
        }
      }));
      this.RaiseOnEndedEdit();
      if (listWithCount.List.Count == 0 || !receivedNewData)
      {
        this._noDataOnLastFetch = this._noDataOnLastFetch + 1;
        if (this._noDataOnLastFetch < 2)
          flag1 = true;
      }
      else
        this._noDataOnLastFetch = 0;
      this._updatingCollection = false;
      return flag1;
    }

    private void ReadLocalData(List<T> data, Action callback)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        foreach (T obj in data)
        {
          IHaveUniqueKey haveUniqueKey = (object) obj as IHaveUniqueKey;
          if (haveUniqueKey == null || !this._collectionItemsDict.ContainsKey(haveUniqueKey.GetKey()))
          {
            this._localGroup.Add(obj);
            this.AddRemoveKeyToDict((object) obj, true);
            this._preloadedItemsCount = this._preloadedItemsCount + 1;
          }
        }
        Action action = callback;
        if (action == null)
          return;
        action();
      }));
    }

    private void ReadData(bool refresh, B backendData, Action<bool> callback)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (refresh)
        {
          this.Refreshing = true;
          this._collection.Clear();
        }
        bool flag = this.ReadData(this._dataProvider.ConverterFunc(backendData));
        this.Refreshing = false;
        this._isLoading = false;
        this.NotifyChanged();
        callback(flag);
      }));
    }

    private void AddRemoveKeyToDict(object t, bool add)
    {
      IHaveUniqueKey haveUniqueKey = t as IHaveUniqueKey;
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
      new ConnectionSettingsTask()
      {
        ConnectionSettingsType = type
      }.Show();
    }

    public void Delete(T item)
    {
      this._updatingCollection = true;
      if (this._collection.Remove(item))
      {
        this.AddRemoveKeyToDict((object) item, false);
        this._totalCount = this._totalCount - GenericCollectionViewModel2<B, T>.GetElementsCountForItem(item);
        this.NotifyChanged();
      }
      this._updatingCollection = false;
    }

    public void DeleteGrouped(T item)
    {
      this._updatingCollection = true;
      if (this._collection.Remove(item))
      {
        this.AddRemoveKeyToDict((object) item, false);
        this._totalCount = this._totalCount - GenericCollectionViewModel2<B, T>.GetElementsCountForItem(item);
        this.NotifyChanged();
      }
      foreach (Group<T> grouped in (System.Collections.ObjectModel.Collection<Group<T>>) this.GroupedCollection)
      {
        if (grouped.Contains(item))
        {
          grouped.Remove(item);
          this._totalCount = this._totalCount - GenericCollectionViewModel2<B, T>.GetElementsCountForItem(item);
          this.NotifyChanged();
        }
      }
      this._updatingCollection = false;
    }

    public void Insert(T item, int ind)
    {
      this._updatingCollection = true;
      this._collection.Insert(ind, item);
      this.AddRemoveKeyToDict((object) item, true);
      this._updatingCollection = false;
      this._totalCount = this._totalCount + GenericCollectionViewModel2<B, T>.GetElementsCountForItem(item);
      this.NotifyChanged();
    }

    private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (this._updatingCollection || !(this._dataProvider is ISupportReorder<T>) || e.Action != NotifyCollectionChangedAction.Add)
        return;
      T before = default (T);
      T after = default (T);
      T obj = this._collection[e.NewStartingIndex];
      if (e.NewStartingIndex > 0)
        after = this._collection[e.NewStartingIndex - 1];
      if (e.NewStartingIndex < this._collection.Count - 1)
        before = this._collection[e.NewStartingIndex + 1];
      (this._dataProvider as ISupportReorder<T>).Reordered(obj, before, after);
    }

    private void RaiseOnStartedEdit()
    {
      if (this.StartedEdit == null)
        return;
      this.StartedEdit((object) this, EventArgs.Empty);
    }

    private void RaiseOnEndedEdit()
    {
      if (this.EndedEdit == null)
        return;
      this.EndedEdit((object) this, EventArgs.Empty);
    }

    public void LoadMore()
    {
      this.LoadData(false, true, false, false, (Action<List<T>>) null, (Action<BackendResult<B, ResultCode>>) null, false);
    }

    private class Status
    {
      public bool IsLoading { get; set; }

      public ResultCode? ResultCode { get; set; }
    }
  }
}
