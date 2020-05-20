using System;

namespace VKClient.Common.Library
{
  public interface ISupportSearchParams
  {
    string ParametersSummaryStr { get; }

    Action OpenParametersPage { get; }

    Action ClearParameters { get; }
  }
}
