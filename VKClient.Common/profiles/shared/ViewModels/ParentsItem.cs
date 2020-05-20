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
      List<string> list = (List<string>)Enumerable.ToList<string>(Enumerable.Select<Relative, string>(parents, (Func<Relative, string>)(r => r.GetVKFormatted(users))));
      if (list.Count <= 0)
        return;
      this.Data = list.Count == 1 ? list[0] : string.Format("{0} {1} {2}", list[0], CommonResources.And, list[1]);
    }
  }
}
