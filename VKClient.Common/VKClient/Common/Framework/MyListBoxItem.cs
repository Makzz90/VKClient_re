using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Library;

namespace VKClient.Common.Framework
{
  public class MyListBoxItem : ReorderListBoxItem
  {
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof (bool), typeof (MyListBoxItem), new PropertyMetadata((object) false, new PropertyChangedCallback(MyListBoxItem.OnIsSelectedPropertyChanged)));
    internal static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof (SelectionEnabledState), typeof (MyListBoxItem), new PropertyMetadata((object) SelectionEnabledState.Closed, new PropertyChangedCallback(MyListBoxItem.ItemStateChanged)));
    internal bool _canTriggerSelectionChanged = true;
    private MyListBox _parent;
    internal bool _isBeingVirtualized;
    private const string Closed = "Closed";
    private const string Opened = "Opened";

    public bool IsChecked
    {
      get
      {
        return (bool) this.GetValue(MyListBoxItem.IsCheckedProperty);
      }
      set
      {
        this.SetValue(MyListBoxItem.IsCheckedProperty, (object) value);
      }
    }

    internal SelectionEnabledState State
    {
      get
      {
        return (SelectionEnabledState) this.GetValue(MyListBoxItem.StateProperty);
      }
      set
      {
        this.SetValue(MyListBoxItem.StateProperty, (object) value);
      }
    }

    public MyListBoxItem()
    {
      this.DefaultStyleKey = (object) typeof (MyListBoxItem);
    }

    private static void OnIsSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      MyListBoxItem myListBoxItem = (MyListBoxItem) obj;
      RoutedEventArgs e1 = new RoutedEventArgs();
      bool flag = (bool) e.NewValue;
      if (flag)
        myListBoxItem.OnSelected(e1);
      else
        myListBoxItem.OnUnselected(e1);
      if (myListBoxItem._parent == null || myListBoxItem._isBeingVirtualized)
        return;
      if (flag)
      {
        myListBoxItem._parent.SelectedItems.Add(myListBoxItem.Content);
        if (!myListBoxItem._canTriggerSelectionChanged)
          return;
        myListBoxItem._parent.OnSelectionChanged((IList) new object[0], (IList) new object[1]
        {
          myListBoxItem.Content
        });
      }
      else
      {
        myListBoxItem._parent.SelectedItems.Remove(myListBoxItem.Content);
        if (!myListBoxItem._canTriggerSelectionChanged)
          return;
        myListBoxItem._parent.OnSelectionChanged((IList) new object[1]
        {
          myListBoxItem.Content
        }, (IList) new object[0]);
      }
    }

    private static void ItemStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      MyListBoxItem myListBoxItem = d as MyListBoxItem;
      if (!(myListBoxItem.DataContext is IMarker))
        return;
      myListBoxItem.State = SelectionEnabledState.Closed;
    }

    protected virtual void OnSelected(RoutedEventArgs e)
    {
      if (this._parent != null)
        return;
      this.State = SelectionEnabledState.Opened;
      this.UpdateVisualState(true);
    }

    protected virtual void OnUnselected(RoutedEventArgs e)
    {
      if (this._parent != null)
        return;
      this.State = SelectionEnabledState.Closed;
      this.UpdateVisualState(true);
    }

    internal void UpdateVisualState(bool useTransitions)
    {
      string stateName;
      switch (this.State)
      {
        case SelectionEnabledState.Closed:
          stateName = "Closed";
          break;
        case SelectionEnabledState.Opened:
          stateName = "Opened";
          break;
        default:
          stateName = "Closed";
          break;
      }
      VisualStateManager.GoToState((Control) this, stateName, useTransitions);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this._parent = this.FindParent();
      this.UpdateVisualState(false);
    }

    private MyListBox FindParent()
    {
      DependencyObject parent = VisualTreeHelper.GetParent((DependencyObject) this);
      while (!(parent is MyListBox) && parent != null)
        parent = VisualTreeHelper.GetParent(parent);
      return (MyListBox) parent;
    }
  }
}
