using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections;
using System.Windows.Controls;

namespace VKClient.Common.Framework.DatePicker
{
  public abstract class DataSource : ILoopingSelectorDataSource
  {
    private DateTimeWrapper _selectedItem;

    public object SelectedItem
    {
      get
      {
        return this._selectedItem;
      }
      set
      {
        if (value == this._selectedItem)
          return;
        DateTimeWrapper dateTimeWrapper = (DateTimeWrapper) value;
        if (dateTimeWrapper != null && this._selectedItem != null && !(dateTimeWrapper.DateTime != this._selectedItem.DateTime))
          return;
        object selectedItem = this._selectedItem;
        this._selectedItem = dateTimeWrapper;
        EventHandler<SelectionChangedEventArgs> selectionChanged = this.SelectionChanged;
        if (selectionChanged == null)
          return;
        selectionChanged(this, new SelectionChangedEventArgs((IList) new object[1]
        {
          selectedItem
        }, (IList) new object[1]
        {
          this._selectedItem
        }));
      }
    }

    public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

    public object GetNext(object relativeTo)
    {
      DateTime? relativeTo1 = this.GetRelativeTo(((DateTimeWrapper) relativeTo).DateTime, 1);
      if (!relativeTo1.HasValue)
        return null;
      return new DateTimeWrapper(relativeTo1.Value);
    }

    public object GetPrevious(object relativeTo)
    {
      DateTime? relativeTo1 = this.GetRelativeTo(((DateTimeWrapper) relativeTo).DateTime, -1);
      if (!relativeTo1.HasValue)
        return null;
      return new DateTimeWrapper(relativeTo1.Value);
    }

    protected abstract DateTime? GetRelativeTo(DateTime relativeDate, int delta);
  }
}
