using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Library;

namespace VKClient.Common.Framework
{
  public class MyListBoxItem : ReorderListBoxItem
  {
      public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(MyListBoxItem), new PropertyMetadata(false, new PropertyChangedCallback(MyListBoxItem.OnIsSelectedPropertyChanged)));
      internal static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(SelectionEnabledState), typeof(MyListBoxItem), new PropertyMetadata(SelectionEnabledState.Closed, new PropertyChangedCallback(MyListBoxItem.ItemStateChanged)));
    internal bool _canTriggerSelectionChanged = true;
    private MyListBox _parent;
    internal bool _isBeingVirtualized;
    private const string Closed = "Closed";
    private const string Opened = "Opened";

    public bool IsChecked
    {
      get
      {
        return (bool) base.GetValue(MyListBoxItem.IsCheckedProperty);
      }
      set
      {
        base.SetValue(MyListBoxItem.IsCheckedProperty, value);
      }
    }

    internal SelectionEnabledState State
    {
      get
      {
        return (SelectionEnabledState) base.GetValue(MyListBoxItem.StateProperty);
      }
      set
      {
        base.SetValue(MyListBoxItem.StateProperty, value);
      }
    }

    public MyListBoxItem()
    {
      base.DefaultStyleKey = (typeof (MyListBoxItem));
    }

    private static void OnIsSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      MyListBoxItem myListBoxItem = (MyListBoxItem) obj;
      RoutedEventArgs e1 = new RoutedEventArgs();
      // ISSUE: explicit reference operation
      bool newValue = (bool) e.NewValue;
      if (newValue)
        myListBoxItem.OnSelected(e1);
      else
        myListBoxItem.OnUnselected(e1);
      if (myListBoxItem._parent == null || myListBoxItem._isBeingVirtualized)
        return;
      if (newValue)
      {
        myListBoxItem._parent.SelectedItems.Add(((ContentControl) myListBoxItem).Content);
        if (!myListBoxItem._canTriggerSelectionChanged)
          return;
        myListBoxItem._parent.OnSelectionChanged((IList) new object[0], (IList) new object[1]
        {
          ((ContentControl) myListBoxItem).Content
        });
      }
      else
      {
        myListBoxItem._parent.SelectedItems.Remove(((ContentControl) myListBoxItem).Content);
        if (!myListBoxItem._canTriggerSelectionChanged)
          return;
        myListBoxItem._parent.OnSelectionChanged((IList) new object[1]
        {
          ((ContentControl) myListBoxItem).Content
        }, (IList) new object[0]);
      }
    }

    private static void ItemStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      MyListBoxItem myListBoxItem = d as MyListBoxItem;
      if (!(((FrameworkElement) myListBoxItem).DataContext is IMarker))
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
      string str;
      switch (this.State)
      {
        case SelectionEnabledState.Closed:
          str = "Closed";
          break;
        case SelectionEnabledState.Opened:
          str = "Opened";
          break;
        default:
          str = "Closed";
          break;
      }
      VisualStateManager.GoToState((Control) this, str, useTransitions);
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
