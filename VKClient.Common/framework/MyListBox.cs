using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Framework
{
  public class MyListBox : ReorderListBox
  {
    public static readonly DependencyProperty SupportsFooterProperty = DependencyProperty.Register("SupportsFooter", typeof (bool), typeof (MyListBox), new PropertyMetadata(true));
    public static readonly DependencyProperty MyItemsSourceProperty = DependencyProperty.Register("MyItemsSource", typeof(IEnumerable), typeof(MyListBox), new PropertyMetadata(null, new PropertyChangedCallback(MyListBox.OnMyItemsSourceChanged)));
    public static readonly DependencyProperty IsInSelectionModeProperty = DependencyProperty.Register("IsSelectionEnabled", typeof(bool), typeof(MyListBox), new PropertyMetadata(false, new PropertyChangedCallback(MyListBox.OnIsSelectionEnabledPropertyChanged)));
    private bool _changingMyItems;
    private bool _changingItems;

    public bool SupportsFooter
    {
      get
      {
        return (bool) base.GetValue(MyListBox.SupportsFooterProperty);
      }
      set
      {
        base.SetValue(MyListBox.SupportsFooterProperty, value);
      }
    }

    public IEnumerable MyItemsSource
    {
      get
      {
        return (IEnumerable) base.GetValue(MyListBox.MyItemsSourceProperty);
      }
      set
      {
        base.SetValue(MyListBox.MyItemsSourceProperty, value);
      }
    }

    public IList SelectedItems { get; private set; }

    public bool IsSelectionEnabled
    {
      get
      {
        return (bool) base.GetValue(MyListBox.IsInSelectionModeProperty);
      }
      set
      {
        base.SetValue(MyListBox.IsInSelectionModeProperty, value);
      }
    }

    public event SelectionChangedEventHandler MultiSelectionChanged;/*
    {
      add
      {
        SelectionChangedEventHandler changedEventHandler = this.MultiSelectionChanged;
        SelectionChangedEventHandler comparand;
        do
        {
          comparand = changedEventHandler;
          changedEventHandler = Interlocked.CompareExchange<SelectionChangedEventHandler>(ref this.MultiSelectionChanged, (SelectionChangedEventHandler) Delegate.Combine((Delegate) comparand, (Delegate) value), comparand);
        }
        while (changedEventHandler != comparand);
      }
      remove
      {
        SelectionChangedEventHandler changedEventHandler = this.MultiSelectionChanged;
        SelectionChangedEventHandler comparand;
        do
        {
          comparand = changedEventHandler;
          changedEventHandler = Interlocked.CompareExchange<SelectionChangedEventHandler>(ref this.MultiSelectionChanged, (SelectionChangedEventHandler) Delegate.Remove((Delegate) comparand, (Delegate) value), comparand);
        }
        while (changedEventHandler != comparand);
      }
    }*/

    public MyListBox()
    {
      base.DefaultStyleKey = (typeof (MyListBox));
      this.SelectedItems = (IList) new List<object>();
    }

    private static void OnMyItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      MyListBox myListBox = d as MyListBox;
      // ISSUE: explicit reference operation
      if (e.NewValue is INotifyCollectionChanged)
      {
        // ISSUE: explicit reference operation
        (e.NewValue as INotifyCollectionChanged).CollectionChanged += new NotifyCollectionChangedEventHandler(myListBox.MyListBox_CollectionChanged);
      }
      ObservableCollection<object> observableCollection = new ObservableCollection<object>();
      // ISSUE: explicit reference operation
      foreach (object obj in e.NewValue as IEnumerable)
        observableCollection.Add(obj);
      if (myListBox.SupportsFooter)
        observableCollection.Add(((FrameworkElement) myListBox).DataContext);
      observableCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(myListBox.itemsSource_CollectionChanged);
      ((ItemsControl) myListBox).ItemsSource = ((IEnumerable) observableCollection);
    }

    private void itemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (this._changingItems)
        return;
      this._changingMyItems = true;
      IList myItemsSource = this.MyItemsSource as IList;
      if (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex <= myItemsSource.Count)
        myItemsSource.Insert(e.NewStartingIndex, e.NewItems[0]);
      if (e.Action == NotifyCollectionChangedAction.Remove)
        myItemsSource.RemoveAt(e.OldStartingIndex);
      this._changingMyItems = false;
    }

    private void MyListBox_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (this._changingMyItems)
        return;
      this._changingItems = true;
      ObservableCollection<object> itemsSource = ((ItemsControl) this).ItemsSource as ObservableCollection<object>;
      if (e.Action == NotifyCollectionChangedAction.Add)
        itemsSource.Insert(e.NewStartingIndex, e.NewItems[0]);
      if (e.Action == NotifyCollectionChangedAction.Remove)
        itemsSource.RemoveAt(e.OldStartingIndex);
      this._changingItems = false;
    }

    private static void OnIsSelectionEnabledPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      MyListBox myListBox = (MyListBox) obj;
      // ISSUE: explicit reference operation
      if ((bool) e.NewValue)
      {
        myListBox.OpenSelection();
      }
      else
      {
        myListBox.UnselectAll();
        myListBox.CloseSelection();
      }
    }

    public void UnselectAll()
    {
      if (this.SelectedItems.Count <= 0)
        return;
      IList removedItems = (IList) new List<object>();
      foreach (object selectedItem in (IEnumerable) this.SelectedItems)
        removedItems.Add(selectedItem);
      for (int index = 0; index < ((PresentationFrameworkCollection<object>) ((ItemsControl) this).Items).Count && this.SelectedItems.Count > 0; ++index)
      {
        MyListBoxItem myListBoxItem = (MyListBoxItem) ((ItemsControl) this).ItemContainerGenerator.ContainerFromIndex(index);
        if (myListBoxItem != null && myListBoxItem.IsChecked)
        {
          myListBoxItem._canTriggerSelectionChanged = false;
          myListBoxItem.IsChecked = false;
          myListBoxItem._canTriggerSelectionChanged = true;
        }
      }
      this.SelectedItems.Clear();
      this.OnSelectionChanged(removedItems, (IList) new object[0]);
    }

    private void OpenSelection()
    {
      base.Dispatcher.BeginInvoke((Action) (() =>
      {
        for (int index = 0; index < ((PresentationFrameworkCollection<object>) ((ItemsControl) this).Items).Count; ++index)
        {
          MyListBoxItem myListBoxItem = (MyListBoxItem) ((ItemsControl) this).ItemContainerGenerator.ContainerFromIndex(index);
          if (myListBoxItem != null && ((ContentControl) myListBoxItem).Content != base.DataContext)
          {
            myListBoxItem.State = SelectionEnabledState.Opened;
            myListBoxItem.UpdateVisualState(true);
          }
        }
      }));
    }

    private void CloseSelection()
    {
      base.Dispatcher.BeginInvoke((Action) (() =>
      {
        for (int index = 0; index < ((PresentationFrameworkCollection<object>) ((ItemsControl) this).Items).Count; ++index)
        {
          MyListBoxItem myListBoxItem = (MyListBoxItem) ((ItemsControl) this).ItemContainerGenerator.ContainerFromIndex(index);
          if (myListBoxItem != null)
          {
            myListBoxItem.State = SelectionEnabledState.Closed;
            myListBoxItem.UpdateVisualState(true);
          }
        }
      }));
    }

    internal void OnSelectionChanged(IList removedItems, IList addedItems)
    {
      // ISSUE: reference to a compiler-generated field
      SelectionChangedEventHandler selectionChanged = this.MultiSelectionChanged;
      if (selectionChanged == null)
        return;
      selectionChanged.Invoke(this, new SelectionChangedEventArgs(removedItems, addedItems));
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
      return (DependencyObject) new MyListBoxItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
      return item is MyListBoxItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
      if (item != base.DataContext)
      {
        base.PrepareContainerForItemOverride(element, item);
        MyListBoxItem myListBoxItem = (MyListBoxItem) element;
        Style itemContainerStyle = this.ItemContainerStyle;
        ((FrameworkElement) myListBoxItem).Style = itemContainerStyle;
        int num1 = 1;
        myListBoxItem._isBeingVirtualized = num1 != 0;
        int num2 = this.SelectedItems.Contains(item) ? 1 : 0;
        myListBoxItem.IsChecked = num2 != 0;
        int num3 = this.IsSelectionEnabled ? 2 : 0;
        myListBoxItem.State = (SelectionEnabledState) num3;
        int num4 = 0;
        myListBoxItem.UpdateVisualState(num4 != 0);
        int num5 = 0;
        myListBoxItem._isBeingVirtualized = num5 != 0;
      }
      else
      {
        MyListBoxItem myListBoxItem = (MyListBoxItem) element;
        int num1 = 0;
        myListBoxItem.IsReorderEnabled = num1 != 0;
        int num2 = 0;
        myListBoxItem.State = (SelectionEnabledState) num2;
        int num3 = 0;
        myListBoxItem.UpdateVisualState(num3 != 0);
        object obj = item;
        ((ContentControl) myListBoxItem).Content = obj;
        DataTemplate dataTemplate = Application.Current.Resources[this.SupportsFooter ? "FooterTemplate" : "ReorderListBoxFooterTemplate"] as DataTemplate;
        ((ContentControl) myListBoxItem).ContentTemplate = dataTemplate;
      }
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
      base.OnItemsChanged(e);
      if (this.SelectedItems.Count <= 0)
        return;
      IList removedItems = (IList) new List<object>();
      for (int index = 0; index < this.SelectedItems.Count; ++index)
      {
        object selectedItem = this.SelectedItems[index];
        if (!((PresentationFrameworkCollection<object>) ((ItemsControl) this).Items).Contains(selectedItem))
        {
          this.SelectedItems.Remove(selectedItem);
          removedItems.Add(selectedItem);
          --index;
        }
      }
      this.OnSelectionChanged(removedItems, (IList) new object[0]);
    }

    public List<T> GetSelected<T>() where T : class
    {
      List<T> objList = new List<T>();
      foreach (object selectedItem in (IEnumerable) this.SelectedItems)
      {
        T obj = selectedItem as T;
        if (obj != null)
          objList.Add(obj);
      }
      return objList;
    }
  }
}
