using System.Collections.Generic;

namespace VKClient.Common.Framework
{
  public interface IMyVirtualizingPanel
  {
    object DataContext { get; }

    List<IVirtualizable> VirtualizableItems { get; }

    void Substitute(IVirtualizable item, IVirtualizable updatedItem);

    void Cleanup();

    void RespondToOrientationChange(bool p);

    void RearrangeAllItems();
  }
}
