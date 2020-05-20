using System;

namespace VKClient.Common.Backend
{
  public interface IPageDataRequesteeInfo
  {
    Guid Guid { get; }

    RequestExecutionRule ExecutionRule { get; }
  }
}
