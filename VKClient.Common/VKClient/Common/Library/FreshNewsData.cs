using System.Collections.Generic;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class FreshNewsData
  {
      public List<IVirtualizable> Items { get; set; }

    public string NextFrom { get; set; }

    public FreshNewsData(IEnumerable<IVirtualizable> items, string nextFrom)
    {
      this.Items = new List<IVirtualizable>(items);
      this.NextFrom = nextFrom;
    }
  }
}
