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
  public class UniversityItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public string GroupImage { get; private set; }

    public double TitleMaxWidth { get; private set; }

    private UniversityItem(string description, Group group = null)
      : base(ProfileInfoItemType.Full)
    {
      this.Title = CommonResources.ProfilePage_Info_CollegeOrUniversity;
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

    public static IEnumerable<UniversityItem> GetUniversities(List<University> universities, List<Group> groups)
    {
      List<UniversityItem> universityItemList = new List<UniversityItem>();
      if (universities.IsNullOrEmpty())
        return (IEnumerable<UniversityItem>) universityItemList;
      foreach (University university in (IEnumerable<University>) universities.OrderByDescending<University, int>((Func<University, int>) (u => u.graduation)))
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(university.graduation > 0 ? string.Format("{0} '{1:00}", (object) university.name, (object) (university.graduation % 100)) : university.name);
        if (!string.IsNullOrEmpty(university.faculty_name))
          stringBuilder.Append("\n").Append(university.faculty_name);
        if (!string.IsNullOrEmpty(university.chair_name))
          stringBuilder.Append("\n").Append(university.chair_name);
        if (!string.IsNullOrEmpty(university.education_form))
          stringBuilder.Append("\n").Append(university.education_form);
        if (!string.IsNullOrEmpty(university.education_status))
          stringBuilder.Append("\n").Append(university.education_status);
        Group group1 = (Group) null;
        if (groups != null)
        {
          long groupId = university.university_group_id;
          if (groupId > 0L)
            group1 = groups.FirstOrDefault<Group>((Func<Group, bool>) (group => group.id == groupId));
        }
        universityItemList.Add(new UniversityItem(stringBuilder.ToString(), group1));
      }
      return (IEnumerable<UniversityItem>) universityItemList;
    }

    public string GetData()
    {
      return (this.Data ?? (object) "").ToString();
    }
  }
}
