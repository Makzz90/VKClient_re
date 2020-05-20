using System;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public interface ICollectionDataProvider2<B, T> where B : class where T : class
  {
    Func<B, ListWithCount<T>> ConverterFunc { get; }

    void GetData(GenericCollectionViewModel2<B, T> caller, int offset, int count, Action<BackendResult<B, ResultCode>> callback);

    string GetFooterTextForCount(GenericCollectionViewModel2<B, T> caller, int count);
  }
}
