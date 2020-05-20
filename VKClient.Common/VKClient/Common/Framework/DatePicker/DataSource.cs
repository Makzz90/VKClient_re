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
        return (object) this._selectedItem;
      }
      set
      {
        if (value == this._selectedItem)
          return;
        DateTimeWrapper dateTimeWrapper = (DateTimeWrapper) value;
        if (dateTimeWrapper != null && this._selectedItem != null && !(dateTimeWrapper.DateTime != this._selectedItem.DateTime))
          return;
        object obj = (object) this._selectedItem;
        this._selectedItem = dateTimeWrapper;
        EventHandler<SelectionChangedEventArgs> eventHandler = this.SelectionChanged;
        if (eventHandler == null)
          return;
        eventHandler((object) this, new SelectionChangedEventArgs((IList) new object[1]{ obj }, (IList) new object[1]
        {
          (object) this._selectedItem
        }));
      }
    }

    public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

    public object GetNext(object relativeTo)
    {
      DateTime? relativeTo1 = this.GetRelativeTo(((DateTimeWrapper) relativeTo).DateTime, 1);
      if (!relativeTo1.HasValue)
        return null;
      return (object) new DateTimeWrapper(relativeTo1.Value);
    }

    public object GetPrevious(object relativeTo)
    {
      DateTime? relativeTo1 = this.GetRelativeTo(((DateTimeWrapper) relativeTo).DateTime, -1);
      if (!relativeTo1.HasValue)
        return null;
      return (object) new DateTimeWrapper(relativeTo1.Value);
    }

    protected abstract DateTime? GetRelativeTo(DateTime relativeDate, int delta);
  }
}
