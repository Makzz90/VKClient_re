using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace VKClient.Common.Framework
{
  public class CollectionObject<T> : ViewModelBase
  {
    private ObservableCollection<object> _coll = new ObservableCollection<object>();

    public ObservableCollection<object> Coll
    {
      get
      {
        return this._coll;
      }
    }

    public T Data
    {
      get
      {
        if (this.Coll.Count > 0)
          return (T) this.Coll[0];
        return default (T);
      }
      set
      {
        if (value != null)
        {
          if (this.Coll.Count == 0)
            this.Coll.Add(value);
          else
            this.Coll[0] = value;
        }
        else if (this.Coll.Count > 0)
          this.Coll.RemoveAt(0);
        base.NotifyPropertyChanged<T>(() => this.Data);
      }
    }

    public static CollectionObject<T> CreateCollectionObject<T>(T t)
    {
      return new CollectionObject<T>() { Data = t };
    }
  }
}
