using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class GenericRootList<T> where T : class
  {
    public List<T> response { get; set; }
  }
}
