using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common.Stickers.ViewModels
{
  public class StickersManageViewModel : ViewModelStatefulBase, IHandle<StickersPackActivatedDeactivatedEvent>, IHandle
  {
    private StickersManageViewModel.CurrentSource _stickersListSource;
    private bool _updatingCollection;

    public ObservableCollection<StockItemHeader> ActiveStickers { get; private set; }

    public ObservableCollection<StockItemHeader> HiddenStickers { get; private set; }

    public StickersManageViewModel.CurrentSource StickersListSource
    {
      get
      {
        return this._stickersListSource;
      }
      set
      {
        if (this._stickersListSource == value)
          return;
        this._stickersListSource = value;
        this.NotifyPropertyChanged<StickersManageViewModel.CurrentSource>((System.Linq.Expressions.Expression<Func<StickersManageViewModel.CurrentSource>>) (() => this.StickersListSource));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.ActiveTabForeground));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.HiddenTabForeground));
        this.NotifyProperties();
      }
    }

    public SolidColorBrush ActiveTabForeground
    {
      get
      {
        return this.GetForeground(StickersManageViewModel.CurrentSource.Active);
      }
    }

    public SolidColorBrush HiddenTabForeground
    {
      get
      {
        return this.GetForeground(StickersManageViewModel.CurrentSource.Hidden);
      }
    }

    public Visibility ActiveTabVisibility
    {
      get
      {
        return this.GetVisibility(StickersManageViewModel.CurrentSource.Active);
      }
    }

    public Visibility HiddenTabVisibility
    {
      get
      {
        return this.GetVisibility(StickersManageViewModel.CurrentSource.Hidden);
      }
    }

    public Visibility ActiveStickersVisibility
    {
      get
      {
        return (((Collection<StockItemHeader>) this.ActiveStickers).Count > 0).ToVisiblity();
      }
    }

    public Visibility HiddenStickersVisibility
    {
      get
      {
        return (((Collection<StockItemHeader>) this.HiddenStickers).Count > 0).ToVisiblity();
      }
    }

    public Visibility OverlayVisibility
    {
      get
      {
        return this.IsInProgress.ToVisiblity();
      }
    }

    public bool StickersAutoSuggestEnabled
    {
      get
      {
        return AppGlobalStateManager.Current.GlobalState.StickersAutoSuggestEnabled;
      }
      set
      {
        if (value == AppGlobalStateManager.Current.GlobalState.StickersAutoSuggestEnabled)
          return;
        AppGlobalStateManager.Current.GlobalState.StickersAutoSuggestEnabled = value;
        EventAggregator.Current.Publish(new AutoSuggestEnabledChangedEvent());
      }
    }

    public bool IsStickersReorderingEnabled
    {
      get
      {
        return ((Collection<StockItemHeader>) this.ActiveStickers).Count > 1;
      }
    }

    public Action<bool> StickersPackActivationHandler { get; set; }

    public StickersManageViewModel()
    {
      this.ActiveStickers.CollectionChanged += new NotifyCollectionChangedEventHandler(this.ActiveStickers_OnCollectionChanged);
      EventAggregator.Current.Subscribe(this);

        this.ActiveStickers = new ObservableCollection<StockItemHeader>();

    this.HiddenStickers = new ObservableCollection<StockItemHeader>();
    }

    private SolidColorBrush GetForeground(StickersManageViewModel.CurrentSource currentSource)
    {
      if (this._stickersListSource == currentSource)
        return (SolidColorBrush) Application.Current.Resources["PhoneBlue300_GrayBlue100Brush"];
      return (SolidColorBrush) Application.Current.Resources["PhoneGray400_Gray500Brush"];
    }

    private Visibility GetVisibility(StickersManageViewModel.CurrentSource currentSource)
    {
      return (this._stickersListSource == currentSource).ToVisiblity();
    }

    private void NotifyProperties()
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ActiveTabVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.HiddenTabVisibility));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsStickersReorderingEnabled));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ActiveStickersVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.HiddenStickersVisibility));
      }));
    }

    private void ActiveStickers_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (this._updatingCollection || e.Action != NotifyCollectionChangedAction.Add)
        return;
      int num1 = 0;
      int num2 = 0;
      int index = e.NewStartingIndex;
      int id = ((Collection<StockItemHeader>) this.ActiveStickers)[index].Product.id;
      if (index > 0)
        num2 = ((Collection<StockItemHeader>) this.ActiveStickers)[index - 1].Product.id;
      if (index < ((Collection<StockItemHeader>) this.ActiveStickers).Count - 1)
        num1 = ((Collection<StockItemHeader>) this.ActiveStickers)[index + 1].Product.id;
      this.SetProgress(true);
      StoreService.Instance.ReorderProducts(id, num2, num1, (Action<BackendResult<bool, ResultCode>>) (result =>
      {
        this.SetProgress(false);
        if (result.ResultCode != ResultCode.Succeeded)
          GenericInfoUC.ShowBasedOnResult((int) result.ResultCode, "", (VKRequestsDispatcher.Error) null);
        else
          EventAggregator.Current.Publish(new StickersPacksReorderedEvent(((Collection<StockItemHeader>) this.ActiveStickers)[index].StockItem, index));
      }));
    }

    private void AddActiveStickers(StockItemHeader stockItemHeader)
    {
      this._updatingCollection = true;
      ((Collection<StockItemHeader>) this.ActiveStickers).Add(stockItemHeader);
      this._updatingCollection = false;
    }

    public override void Load(Action<ResultCode> callback)
    {
        StoreService instance = StoreService.Instance;
        List<StoreProductFilter> productFilters = new List<StoreProductFilter>();
        productFilters.Add(StoreProductFilter.Purchased);
        Action<BackendResult<List<StockItem>, ResultCode>> callback1 = (Action<BackendResult<List<StockItem>, ResultCode>>)(result =>
        {
            ResultCode resultCode = result.ResultCode;
            if (resultCode == ResultCode.Succeeded)
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    List<StockItem> resultData = result.ResultData;
                    this.ClearItems();
                    foreach (StockItem stockItem in resultData.Where<StockItem>((Func<StockItem, bool>)(product => product != null)))
                    {
                        StockItemHeader stockItemHeader = new StockItemHeader(stockItem, false, 0, false);
                        if (stockItemHeader.IsActive)
                            this.AddActiveStickers(stockItemHeader);
                        else
                            this.HiddenStickers.Add(stockItemHeader);
                    }
                    this.NotifyProperties();
                }));
            callback(resultCode);
        });
        instance.GetStockItems(productFilters, callback1);
    }

    private void ClearItems()
    {
      StickersManageViewModel.ClearCollection((IList) this.ActiveStickers);
      StickersManageViewModel.ClearCollection((IList) this.HiddenStickers);
    }

    private static void ClearCollection(IList collection)
    {
      while (collection.Count > 0)
        collection.RemoveAt(collection.Count - 1);
    }

    public void Activate(StockItemHeader stockItemHeader)
    {
      this.SetProgress(true);
      StorePurchaseManager.ActivateStickersPack(stockItemHeader, (Action<bool>) (activated => this.SetProgress(false)));
    }

    public void Deactivate(StockItemHeader stockItemHeader)
    {
      this.SetProgress(true);
      StorePurchaseManager.DeactivateStickersPack(stockItemHeader, (Action<bool>) (deactivated => this.SetProgress(false)));
    }

    private void SetProgress(bool isInProgress)
    {
        Execute.ExecuteOnUIThread((Action)(() =>
        {
            Action<bool> activationHandler = this.StickersPackActivationHandler;
            if (activationHandler != null)
            {
                int num = isInProgress ? 1 : 0;
                activationHandler(num != 0);
            }
            this.SetInProgress(isInProgress, "");
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.OverlayVisibility));
        }));
    }

    private void ActivateDeactivate(long productId, bool activate)
    {
        Execute.ExecuteOnUIThread((Action)(() =>
        {
            StockItemHeader stockItemHeader = activate ? this.HiddenStickers.FirstOrDefault<StockItemHeader>((Func<StockItemHeader, bool>)(item => (long)item.ProductId == productId)) : this.ActiveStickers.FirstOrDefault<StockItemHeader>((Func<StockItemHeader, bool>)(item => (long)item.ProductId == productId));
            if (stockItemHeader == null)
                return;
            if (activate)
            {
                this.HiddenStickers.Remove(stockItemHeader);
                this.AddActiveStickers(stockItemHeader);
            }
            else
            {
                this.ActiveStickers.Remove(stockItemHeader);
                this.HiddenStickers.Insert(0, stockItemHeader);
            }
            this.NotifyProperties();
        }));
    }

    public void Handle(StickersPackActivatedDeactivatedEvent message)
    {
      this.ActivateDeactivate((long) message.StockItemHeader.ProductId, message.IsActive);
    }

    public enum CurrentSource
    {
      Active,
      Hidden,
    }
  }
}
