using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class GrandchildrenItem : ProfileInfoItem
  {
    public GrandchildrenItem(IEnumerable<Relative> grandchildren, List<User> users)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.ProfilePage_Info_Grandchildren;
      this.Data = ListExtensions.GetCommaSeparated((List<string>)Enumerable.ToList<string>(Enumerable.Select<Relative, string>(grandchildren, (Func<Relative, string>)(r => r.GetVKFormatted(users)))), ", ");
    }
  }
}
