using System.Collections;
using System.Collections.Specialized;

namespace VKClient.Common.Framework
{
  public class CollectionsSynchronizer
  {
    private IList _target;
    private int _startingIndexTarget;
    private INotifyCollectionChanged _source;
    private int _sourceCount;

    public int StartingIndexTarget
    {
      get
      {
        return this._startingIndexTarget;
      }
      set
      {
        this._startingIndexTarget = value;
      }
    }

    public INotifyCollectionChanged Source
    {
      get
      {
        return this._source;
      }
    }

    public CollectionsSynchronizer(INotifyCollectionChanged source, IList target, int startingIndexTarget = 0)
    {
      this._startingIndexTarget = startingIndexTarget;
      this._target = target;
      this._source = source;
      source.CollectionChanged += new NotifyCollectionChangedEventHandler(this.source_CollectionChanged);
      this._sourceCount = (source as IList).Count;
    }

    public void UnhookEvents()
    {
      this.Source.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.source_CollectionChanged);
    }

    private void source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Reset)
      {
        for (int index = this._startingIndexTarget; index < this._startingIndexTarget + this._sourceCount; ++index)
          this._target.RemoveAt(this._startingIndexTarget);
        this._sourceCount = 0;
      }
      if (e.Action == NotifyCollectionChangedAction.Remove)
      {
        this._sourceCount = this._sourceCount - 1;
        this._target.RemoveAt(e.OldStartingIndex + this._startingIndexTarget);
      }
      if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems.Count <= 0)
        return;
      this._sourceCount = this._sourceCount + 1;
      this._target.Insert(e.NewStartingIndex + this._startingIndexTarget, e.NewItems[0]);
    }
  }
}
