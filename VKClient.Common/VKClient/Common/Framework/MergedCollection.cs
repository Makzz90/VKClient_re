using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VKClient.Common.Framework
{
  public class MergedCollection : ObservableCollection<object>
  {
    private List<CollectionsSynchronizer> _synchronizers = new List<CollectionsSynchronizer>();
    private List<NotifyCollectionChangedEventHandler> _eventHandlers = new List<NotifyCollectionChangedEventHandler>();

    public void Reset()
    {
      if (this._synchronizers.Count <= 0)
        return;
      for (int index = 0; index < this._eventHandlers.Count; ++index)
      {
        this._synchronizers[index].Source.CollectionChanged -= this._eventHandlers[index];
        this._synchronizers[index].UnhookEvents();
      }
      this._synchronizers.Clear();
      this._eventHandlers.Clear();
      this.Clear();
    }

    public void Merge(INotifyCollectionChanged coll)
    {
      this._synchronizers.Add(new CollectionsSynchronizer(coll, (IList) this, this.Count));
      if (coll is ICollection)
      {
        foreach (object obj in (IEnumerable) (coll as ICollection))
          this.Add(obj);
      }
      int indOfCurrent = this._synchronizers.Count - 1;
      NotifyCollectionChangedEventHandler changedEventHandler = (NotifyCollectionChangedEventHandler) ((s, e) => this.coll_CollectionChanged(s, e, indOfCurrent));
      coll.CollectionChanged += changedEventHandler;
      this._eventHandlers.Add(changedEventHandler);
    }

    private void coll_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e, int indOfCurrent)
    {
      int num = 0;
      if (e.Action == NotifyCollectionChangedAction.Reset && indOfCurrent + 1 < this._synchronizers.Count)
        num = this._synchronizers[indOfCurrent + 1].StartingIndexTarget - this._synchronizers[indOfCurrent].StartingIndexTarget;
      for (int index = indOfCurrent + 1; index < this._synchronizers.Count; ++index)
      {
        if (e.Action == NotifyCollectionChangedAction.Reset)
          this._synchronizers[index].StartingIndexTarget -= num;
        if (e.Action == NotifyCollectionChangedAction.Remove)
          --this._synchronizers[index].StartingIndexTarget;
        if (e.Action == NotifyCollectionChangedAction.Add)
          ++this._synchronizers[index].StartingIndexTarget;
      }
    }
  }
}
