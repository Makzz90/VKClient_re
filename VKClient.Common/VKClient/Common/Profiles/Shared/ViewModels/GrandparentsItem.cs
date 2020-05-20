using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class GrandparentsItem : ProfileInfoItem
  {
    public GrandparentsItem(IEnumerable<Relative> grandparents, List<User> users)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_Grandparents;
      this.Data = (object) grandparents.Select<Relative, string>((Func<Relative, string>) (r => r.GetVKFormatted(users))).ToList<string>().GetCommaSeparated(", ");
    }
  }
}
