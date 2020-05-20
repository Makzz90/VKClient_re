using System;
using System.Collections.Generic;

namespace VKClient.Common.Library
{
  public interface ILocalCollectionDataProvider<T> where T : class
  {
    string LocalGroupName { get; }

    string GlobalGroupName { get; }

    Func<T, bool> GetIsLocalItem { get; }

    void GetLocalData(Action<List<T>> callback);
  }
}
