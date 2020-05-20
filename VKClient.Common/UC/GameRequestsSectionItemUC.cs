using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.BackendServices;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GameRequestsSectionItemUC : UserControl, INotifyPropertyChanged
  {
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (List<GameRequestHeader>), typeof (GameRequestsSectionItemUC), new PropertyMetadata(new PropertyChangedCallback(GameRequestsSectionItemUC.OnItemsSourceChanged)));
    public static readonly DependencyProperty MaxDisplayedItemsCountProperty = DependencyProperty.Register("MaxDisplayedItemsCount", typeof (int), typeof (GameRequestsSectionItemUC), new PropertyMetadata(2));
    private List<GameRequestHeader> _gameRequests;
    private ObservableCollection<GameRequestHeaderUC> _actualItemsSource;
    internal GroupFooterUC ucFooter;
    private bool _contentLoaded;

    public List<GameRequestHeader> ItemsSource
    {
      get
      {
        return (List<GameRequestHeader>) base.GetValue(GameRequestsSectionItemUC.ItemsSourceProperty);
      }
      set
      {
        this.SetDPValue(GameRequestsSectionItemUC.ItemsSourceProperty, value, "ItemsSource");
      }
    }

    public int MaxDisplayedItemsCount
    {
      get
      {
        return (int) base.GetValue(GameRequestsSectionItemUC.MaxDisplayedItemsCountProperty);
      }
      set
      {
        this.SetDPValue(GameRequestsSectionItemUC.MaxDisplayedItemsCountProperty, value, "MaxDisplayedItemsCount");
      }
    }

    public ObservableCollection<GameRequestHeaderUC> ActualItemsSource
    {
      get
      {
        return this._actualItemsSource;
      }
      set
      {
        this._actualItemsSource = value;
        this.OnPropertyChanged("ActualItemsSource");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GameRequestsSectionItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = this;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameRequestsSectionItemUC requestsSectionItemUc = d as GameRequestsSectionItemUC;
      // ISSUE: explicit reference operation
      if (requestsSectionItemUc == null || !(e.NewValue is List<GameRequestHeader>))
        return;
      requestsSectionItemUc.UpdateData();
    }

    public void MarkAllAsRead()
    {
      if (this.ItemsSource == null)
        return;
      List<long> longList = new List<long>();
      IEnumerator<GameRequestHeaderUC> enumerator = ((IEnumerable<GameRequestHeaderUC>) Enumerable.Where<GameRequestHeaderUC>(this.ActualItemsSource, (Func<GameRequestHeaderUC, bool>) (item =>
      {
        if (item.DataProvider != null)
          return !item.DataProvider.IsRead;
        return false;
      }))).GetEnumerator();
      try
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          GameRequestHeaderUC current = enumerator.Current;
          longList.Add(current.DataProvider.GameRequest.id);
          current.MarkAsRead();
          EventAggregator.Current.Publish(new GameRequestReadEvent(current.DataProvider));
        }
      }
      finally
      {
        if (enumerator != null)
          ((IDisposable) enumerator).Dispose();
      }
      if (longList.Count <= 0)
        return;
      AppsService.Instance.MarkRequestAsRead((IEnumerable<long>) longList, (Action<BackendResult<OwnCounters, ResultCode>>) (result =>
      {
        if (result.ResultCode != ResultCode.Succeeded)
          return;
        CountersManager.Current.Counters = result.ResultData;
      }));
    }

    private void UpdateData()
    {
      this.ActualItemsSource =  null;
      if (this.ItemsSource == null)
        return;
      this._gameRequests = (List<GameRequestHeader>) Enumerable.ToList<GameRequestHeader>(Enumerable.Where<GameRequestHeader>(this.ItemsSource, (Func<GameRequestHeader, bool>) (item =>
      {
        if (item != null)
          return !item.IsInvite;
        return false;
      })));
      List<GameRequestHeader> gameRequestHeaderList = new List<GameRequestHeader>((IEnumerable<GameRequestHeader>) Enumerable.Where<GameRequestHeader>(this._gameRequests, (Func<GameRequestHeader, bool>) (item => !item.IsRead)));
      if (gameRequestHeaderList.Count < this.MaxDisplayedItemsCount && gameRequestHeaderList.Count < this._gameRequests.Count)
      {
        int num = Math.Min(this.MaxDisplayedItemsCount, this._gameRequests.Count) - gameRequestHeaderList.Count;
        gameRequestHeaderList.AddRange((IEnumerable<GameRequestHeader>) Enumerable.Take<GameRequestHeader>(Enumerable.Skip<GameRequestHeader>(this._gameRequests, gameRequestHeaderList.Count), num));
      }
      this.ActualItemsSource = new ObservableCollection<GameRequestHeaderUC>();
      List<GameRequestHeader>.Enumerator enumerator = gameRequestHeaderList.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          GameRequestHeader current = enumerator.Current;
          ObservableCollection<GameRequestHeaderUC> actualItemsSource = this.ActualItemsSource;
          GameRequestHeaderUC gameRequestHeaderUc = new GameRequestHeaderUC();
          gameRequestHeaderUc.DataProvider = current;
          // ISSUE: method pointer
          Action<GameRequestHeader, Action> action = new Action<GameRequestHeader, Action>( this.DeleteRequestAction);
          gameRequestHeaderUc.DeleteRequestAction = action;
          ((Collection<GameRequestHeaderUC>) actualItemsSource).Add(gameRequestHeaderUc);
        }
      }
      finally
      {
        enumerator.Dispose();
      }
      int number = this._gameRequests.Count - ((Collection<GameRequestHeaderUC>) this.ActualItemsSource).Count;
      if (number == 1)
      {
        // ISSUE: method pointer
        ((Collection<GameRequestHeaderUC>) this.ActualItemsSource).Add(new GameRequestHeaderUC()
        {
          DataProvider = (GameRequestHeader) Enumerable.Last<GameRequestHeader>(this._gameRequests),
          DeleteRequestAction = new Action<GameRequestHeader, Action>( this.DeleteRequestAction)
        });
        --number;
      }
      this.UpdateFooterVisibility();
      this.ucFooter.FooterText = UIStringFormatterHelper.FormatNumberOfSomething(number, CommonResources.Games_ShowMoreRequestsOneFrm, CommonResources.Games_ShowMoreRequestsTwoFourFrm, CommonResources.Games_ShowMoreRequestsFiveFrm, true,  null, false);
      if (((UIElement) this.ucFooter).Visibility != Visibility.Collapsed || ((Collection<GameRequestHeaderUC>) this.ActualItemsSource).Count <= 0)
        return;
      ((GameRequestHeaderUC) Enumerable.Last<GameRequestHeaderUC>(this.ActualItemsSource)).IsSeparatorVisible = false;
    }

    private void DeleteRequestAction(GameRequestHeader gameRequestHeader, Action callback)
    {
        GameRequestHeaderUC uc = this.ActualItemsSource.FirstOrDefault<GameRequestHeaderUC>((Func<GameRequestHeaderUC, bool>)(item => item.DataProvider == gameRequestHeader));
        if (uc == null || uc.DataProvider == null || uc.DataProvider.GameRequest == null)
            return;
        AppsService.Instance.DeleteRequest(uc.DataProvider.GameRequest.id, (Action<BackendResult<OwnCounters, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
        {
            if (result.ResultCode == ResultCode.Succeeded)
                CountersManager.Current.Counters = result.ResultData;
            this.ItemsSource.Remove(this.ItemsSource.FirstOrDefault<GameRequestHeader>(/*func ?? (func = (*/new Func<GameRequestHeader, bool>(header => header.GameRequest.id == uc.DataProvider.GameRequest.id)));//omg_re
            this.UpdateData();
            if (callback == null)
                return;
            callback();
        }))));
    }

    private void UpdateFooterVisibility()
    {
      ((UIElement) this.ucFooter).Visibility = (((Collection<GameRequestHeaderUC>) this.ActualItemsSource).Count < this._gameRequests.Count ? Visibility.Visible : Visibility.Collapsed);
    }

    private void Footer_OnMoreTapped(object sender, EventArgs e)
    {
      for (int count = ((Collection<GameRequestHeaderUC>) this.ActualItemsSource).Count; count < this._gameRequests.Count; ++count)
      {
        // ISSUE: method pointer
        ((Collection<GameRequestHeaderUC>) this.ActualItemsSource).Add(new GameRequestHeaderUC()
        {
          DataProvider = this._gameRequests[count],
          DeleteRequestAction = new Action<GameRequestHeader, Action>( this.DeleteRequestAction)
        });
      }
      this.UpdateFooterVisibility();
      if (((UIElement)this.ucFooter).Visibility != Visibility.Collapsed || ((Collection<GameRequestHeaderUC>)this.ActualItemsSource).Count <= 0)
        return;
      ((GameRequestHeaderUC) Enumerable.Last<GameRequestHeaderUC>(this.ActualItemsSource)).IsSeparatorVisible = false;
    }

    private void SetDPValue(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
    {
      base.SetValue(property, value);
      // ISSUE: reference to a compiler-generated field
      if (this.PropertyChanged == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      // ISSUE: reference to a compiler-generated field
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GameRequestsSectionItemUC.xaml", UriKind.Relative));
      this.ucFooter = (GroupFooterUC) base.FindName("ucFooter");
    }
  }
}
