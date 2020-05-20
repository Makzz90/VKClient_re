using System.Collections.Generic;

namespace VKClient.Common.Framework
{
  public interface ILinqTree<T>
  {
    T Parent { get; }

    IEnumerable<T> Children();
  }
}
