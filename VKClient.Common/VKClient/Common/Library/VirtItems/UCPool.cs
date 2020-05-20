using System;
using System.Collections.Generic;
using System.Linq;

namespace VKClient.Common.Library.VirtItems
{
  public class UCPool
  {
    private List<UserControlVirtualizable> _ucList = new List<UserControlVirtualizable>();

    public T GetFromPool<T>() where T : UserControlVirtualizable, new()
    {
      T obj = this._ucList.FirstOrDefault<UserControlVirtualizable>((Func<UserControlVirtualizable, bool>) (u => u is T)) as T;
      if ((object) obj != null)
        this._ucList.Remove((UserControlVirtualizable) obj);
      else
        obj = Activator.CreateInstance<T>();
      return obj;
    }

    public void AddBackToPool(UserControlVirtualizable uc)
    {
      this._ucList.Add(uc);
    }
  }
}
