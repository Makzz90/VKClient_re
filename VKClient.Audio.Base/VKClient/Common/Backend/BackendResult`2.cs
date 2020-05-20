using System;
using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class BackendResult<T, Z> where Z : struct, IConvertible
  {
    public T ResultData { get; set; }

    public Z ResultCode { get; set; }

    public VKRequestsDispatcher.Error Error { get; set; }

    public List<ExecuteError> ExecuteErrors { get; set; }

    public BackendResult(Z resultCode, T resultData)
    {
      this.ResultData = resultData;
      this.ResultCode = resultCode;
    }

    public BackendResult(Z resultCode)
    {
      this.ResultCode = resultCode;
    }

    public BackendResult()
    {
    }
  }
}
