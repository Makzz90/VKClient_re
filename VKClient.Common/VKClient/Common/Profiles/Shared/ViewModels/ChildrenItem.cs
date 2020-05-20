using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ChildrenItem : ProfileInfoItem
  {
    public ChildrenItem(IEnumerable<Relative> children, List<User> users)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_Children;
      this.Data = (object) children.Select<Relative, string>((Func<Relative, string>) (r => r.GetVKFormatted(users))).ToList<string>().GetCommaSeparated(", ");
    }
  }
}
