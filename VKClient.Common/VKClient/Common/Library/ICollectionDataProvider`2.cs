using System;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public interface ICollectionDataProvider<B, T> where B : class where T : class
  {
    Func<B, ListWithCount<T>> ConverterFunc { get; }

    void GetData(GenericCollectionViewModel<B, T> caller, int offset, int count, Action<BackendResult<B, ResultCode>> callback);

    string GetFooterTextForCount(GenericCollectionViewModel<B, T> caller, int count);
  }
}
