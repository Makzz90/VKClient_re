using System;
using System.Collections.Generic;

namespace VKClient.Common.Framework
{
  public class AsyncHelper<A> where A : class
  {
    private List<Action<A>> _callbacks = new List<Action<A>>();
    private object _obj = new object();
    private Action<Action<A>> _action;
    private bool _isInProgress;
    private A _cachedA;

    public AsyncHelper(Action<Action<A>> action)
    {
      this._action = action;
    }

    public void RunAction(Action<A> callback, bool forceFreshData)
    {
      lock (this._obj)
      {
        if (!forceFreshData && this._cachedA != null)
        {
          callback(this._cachedA);
        }
        else
        {
          this._callbacks.Add(callback);
          if (this._isInProgress)
            return;
          this._isInProgress = true;
          this._action((Action<A>) (a =>
          {
            lock (this._obj)
            {
              this._cachedA = a;
              foreach (Action<A> callback1 in this._callbacks)
                callback1(a);
              this._callbacks.Clear();
              this._isInProgress = false;
            }
          }));
        }
      }
    }
  }
}
