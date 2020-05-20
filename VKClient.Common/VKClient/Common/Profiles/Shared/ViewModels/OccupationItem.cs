using System;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class OccupationItem : ProfileInfoItem
  {
    public string GroupImage { get; private set; }

    private OccupationItem(string title, string description, Group group = null)
      : base(ProfileInfoItemType.Full)
    {
      this.Title = title;
      this.Data = (object) description;
      if (group == null)
        return;
      this.GroupImage = group.photo_200;
      this.NavigationAction = (Action) (() => Navigator.Current.NavigateToGroup(group.id, group.name, false));
    }

    public static OccupationItem GetOccupationItem(Occupation occupation, Group group)
    {
      string title = "";
      string name = occupation.name;
      Group group1 = (Group) null;
      if (occupation.type == OccupationType.work && group != null && group.id == occupation.id)
        group1 = group;
      switch (occupation.type)
      {
        case OccupationType.work:
          title = CommonResources.OccupationType_Work.ToLowerInvariant();
          break;
        case OccupationType.school:
        case OccupationType.university:
          title = CommonResources.OccupationType_SchoolUniversity.ToLowerInvariant();
          break;
      }
      return new OccupationItem(title, name, group1);
    }
  }
}
