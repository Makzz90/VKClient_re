using System.Collections.Generic;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class FreshNewsData
  {
      public List<IVirtualizable> Items { get; private set; }

      public string NextFrom { get; private set; }

    public FreshNewsData(IEnumerable<IVirtualizable> items, string nextFrom)
    {
      this.Items = new List<IVirtualizable>(items);
      this.NextFrom = nextFrom;
    }
  }
}
