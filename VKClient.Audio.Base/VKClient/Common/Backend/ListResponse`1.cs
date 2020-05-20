using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class ListResponse<T>
  {
    public int TotalCount { get; set; }

    public List<T> Data { get; set; }
  }
}
