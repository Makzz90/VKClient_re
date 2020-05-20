using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class SchoolItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public string GroupImage { get; private set; }

    public double TitleMaxWidth { get; private set; }

    private SchoolItem(string title, string description, Group group = null)
      : base(ProfileInfoItemType.Full)
    {
      this.Title = string.IsNullOrEmpty(title) ? CommonResources.ProfilePage_Info_School : title.ToLowerInvariant();
      this.Data = (object) description;
      if (group != null)
      {
        this.GroupImage = group.photo_200;
        this.NavigationAction = (Action) (() => Navigator.Current.NavigateToGroup(group.id, group.name, false));
      }
      if (!string.IsNullOrEmpty(this.GroupImage))
        this.TitleMaxWidth = 370.0;
      else
        this.TitleMaxWidth = 448.0;
    }

    public static IEnumerable<SchoolItem> GetSchools(List<School> schools, List<Group> groups)
    {
      List<SchoolItem> schoolItemList = new List<SchoolItem>();
      if (schools.IsNullOrEmpty())
        return (IEnumerable<SchoolItem>) schoolItemList;
      foreach (School school in (IEnumerable<School>) schools.OrderByDescending<School, int>((Func<School, int>) (s => s.year_graduated)))
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(school.year_graduated > 0 ? string.Format("{0} '{1:00}", (object) school.name, (object) (school.year_graduated % 100)) : school.name);
        if (!string.IsNullOrEmpty(school.speciality))
          stringBuilder.Append("\n").Append(school.speciality);
        Group group1 = (Group) null;
        if (groups != null)
        {
          long groupId = school.school_group_id;
          if (groupId > 0L)
            group1 = groups.FirstOrDefault<Group>((Func<Group, bool>) (group => group.id == groupId));
        }
        schoolItemList.Add(new SchoolItem(school.type_str, stringBuilder.ToString(), group1));
      }
      return (IEnumerable<SchoolItem>) schoolItemList;
    }

    public string GetData()
    {
      return (this.Data ?? (object) "").ToString();
    }
  }
}
