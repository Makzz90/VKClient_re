using System;
using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public interface ISearchDataProvider<B, T> where B : class where T : class, ISearchableItemHeader<B>
  {
    string LocalGroupName { get; }

    string GlobalGroupName { get; }

    IEnumerable<T> LocalItems { get; }

    Func<VKList<B>, ListWithCount<T>> ConverterFunc { get; }

    string GetFooterTextForCount(int count);

    void GetData(string searchString, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<B>, ResultCode>> callback);
  }
}
