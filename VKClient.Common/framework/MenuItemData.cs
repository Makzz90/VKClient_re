using System;

namespace VKClient.Common.Framework
{
  public class MenuItemData
  {
    public string Title { get; set; }

    public string Tag { get; set; }

    public Action OnTap { get; set; }
  }
}
