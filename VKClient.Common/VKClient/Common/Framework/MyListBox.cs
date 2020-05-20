using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Framework
{
  public class MyListBox : ReorderListBox
  {
    public static readonly DependencyProperty SupportsFooterProperty = DependencyProperty.Register("SupportsFooter", typeof (bool), typeof (MyListBox), new PropertyMetadata((object) true));
    public static readonly DependencyProperty MyItemsSourceProperty = DependencyProperty.Register("MyItemsSource", typeof (IEnumerable), typeof (MyListBox), new PropertyMetadata(null, new PropertyChangedCallback(MyListBox.OnMyItemsSourceChanged)));
    public static readonly DependencyProperty IsInSelectionModeProperty = DependencyProperty.Register("IsSelectionEnabled", typeof (bool), typeof (MyListBox), new PropertyMetadata((object) false, new PropertyChangedCallback(MyListBox.OnIsSelectionEnabledPropertyChanged)));
    private bool _changingMyItems;
    private bool _changingItems;

    public bool SupportsFooter
    {
      get
      {
        return (bool) this.GetValue(MyListBox.SupportsFooterProperty);
      }
      set
      {
        this.SetValue(MyListBox.SupportsFooterProperty, (object) value);
      }
    }

    public IEnumerable MyItemsSource
    {
      get
      {
        return (IEnumerable) this.GetValue(MyListBox.MyItemsSourceProperty);
      }
      set
      {
        this.SetValue(MyListBox.MyItemsSourceProperty, (object) value);
      }
    }

    public new IList SelectedItems { get; private set; }

    public bool IsSelectionEnabled
    {
      get
      {
        return (bool) this.GetValue(MyListBox.IsInSelectionModeProperty);
      }
      set
      {
        this.SetValue(MyListBox.IsInSelectionModeProperty, (object) value);
      }
    }

    public event SelectionChangedEventHandler MultiSelectionChanged;

    public MyListBox()
    {
      this.DefaultStyleKey = (object) typeof (MyListBox);
      this.SelectedItems = (IList) new List<object>();
    }

    private static void OnMyItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      MyListBox myListBox = d as MyListBox;
      if (e.NewValue is INotifyCollectionChanged)
        (e.NewValue as INotifyCollectionChanged).CollectionChanged += new NotifyCollectionChangedEventHandler(myListBox.MyListBox_CollectionChanged);
      ObservableCollection<object> observableCollection = new ObservableCollection<object>();
      foreach (object obj in e.NewValue as IEnumerable)
        observableCollection.Add(obj);
      if (myListBox.SupportsFooter)
        observableCollection.Add(myListBox.DataContext);
      observableCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(myListBox.itemsSource_CollectionChanged);
      myListBox.ItemsSource = (IEnumerable) observableCollection;
    }

    private void itemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (this._changingItems)
        return;
      this._changingMyItems = true;
      IList list = this.MyItemsSource as IList;
      if (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex <= list.Count)
        list.Insert(e.NewStartingIndex, e.NewItems[0]);
      if (e.Action == NotifyCollectionChangedAction.Remove)
        list.RemoveAt(e.OldStartingIndex);
      this._changingMyItems = false;
    }

    private void MyListBox_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (this._changingMyItems)
        return;
      this._changingItems = true;
      ObservableCollection<object> observableCollection = this.ItemsSource as ObservableCollection<object>;
      if (e.Action == NotifyCollectionChangedAction.Add)
        observableCollection.Insert(e.NewStartingIndex, e.NewItems[0]);
      if (e.Action == NotifyCollectionChangedAction.Remove)
        observableCollection.RemoveAt(e.OldStartingIndex);
      this._changingItems = false;
    }

    private static void OnIsSelectionEnabledPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      MyListBox myListBox = (MyListBox) obj;
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
      for (int index = 0; index < this.Items.Count && this.SelectedItems.Count > 0; ++index)
      {
        MyListBoxItem myListBoxItem = (MyListBoxItem) this.ItemContainerGenerator.ContainerFromIndex(index);
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
      this.Dispatcher.BeginInvoke((Action) (() =>
      {
        for (int index = 0; index < this.Items.Count; ++index)
        {
          MyListBoxItem myListBoxItem = (MyListBoxItem) this.ItemContainerGenerator.ContainerFromIndex(index);
          if (myListBoxItem != null && myListBoxItem.Content != this.DataContext)
          {
            myListBoxItem.State = SelectionEnabledState.Opened;
            myListBoxItem.UpdateVisualState(true);
          }
        }
      }));
    }

    private void CloseSelection()
    {
      this.Dispatcher.BeginInvoke((Action) (() =>
      {
        for (int index = 0; index < this.Items.Count; ++index)
        {
          MyListBoxItem myListBoxItem = (MyListBoxItem) this.ItemContainerGenerator.ContainerFromIndex(index);
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
      SelectionChangedEventHandler changedEventHandler = this.MultiSelectionChanged;
      if (changedEventHandler == null)
        return;
      changedEventHandler((object) this, new SelectionChangedEventArgs(removedItems, addedItems));
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
      if (item != this.DataContext)
      {
        base.PrepareContainerForItemOverride(element, item);
        MyListBoxItem myListBoxItem = (MyListBoxItem) element;
        Style itemContainerStyle = this.ItemContainerStyle;
        myListBoxItem.Style = itemContainerStyle;
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
        myListBoxItem.Content = obj;
        DataTemplate dataTemplate = Application.Current.Resources[this.SupportsFooter ? (object) "FooterTemplate" : (object) "ReorderListBoxFooterTemplate"] as DataTemplate;
        myListBoxItem.ContentTemplate = dataTemplate;
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
        object obj = this.SelectedItems[index];
        if (!this.Items.Contains(obj))
        {
          this.SelectedItems.Remove(obj);
          removedItems.Add(obj);
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
        if ((object) obj != null)
          objList.Add(obj);
      }
      return objList;
    }
  }
}
