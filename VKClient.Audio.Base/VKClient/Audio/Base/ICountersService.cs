using System;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base
{
  public interface ICountersService
  {
    void GetCounters(Action<BackendResult<OwnCounters, ResultCode>> callback);
  }
}
