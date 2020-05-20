using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ParentsItem : ProfileInfoItem
  {
    public ParentsItem(IEnumerable<Relative> parents, List<User> users)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_Parents;
      List<string> list = parents.Select<Relative, string>((Func<Relative, string>) (r => r.GetVKFormatted(users))).ToList<string>();
      if (list.Count <= 0)
        return;
      this.Data = list.Count == 1 ? (object) list[0] : (object) string.Format("{0} {1} {2}", (object) list[0], (object) CommonResources.And, (object) list[1]);
    }
  }
}
