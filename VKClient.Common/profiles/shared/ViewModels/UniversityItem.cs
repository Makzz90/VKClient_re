using System;
using System.Collections;
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
      this.Data = description;
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
      IEnumerator<University> enumerator = ((IEnumerable<University>)Enumerable.OrderByDescending<University, int>(universities, (Func<University, int>)(u => u.graduation))).GetEnumerator();
      try
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          University current = enumerator.Current;
          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.Append(current.graduation > 0 ? string.Format("{0} '{1:00}", current.name, (current.graduation % 100)) : current.name);
          if (!string.IsNullOrEmpty(current.faculty_name))
            stringBuilder.Append("\n").Append(current.faculty_name);
          if (!string.IsNullOrEmpty(current.chair_name))
            stringBuilder.Append("\n").Append(current.chair_name);
          if (!string.IsNullOrEmpty(current.education_form))
            stringBuilder.Append("\n").Append(current.education_form);
          if (!string.IsNullOrEmpty(current.education_status))
            stringBuilder.Append("\n").Append(current.education_status);
          Group group1 =  null;
          if (groups != null)
          {
            long groupId = current.university_group_id;
            if (groupId > 0L)
                group1 = (Group)Enumerable.FirstOrDefault<Group>(groups, (Func<Group, bool>)(group => group.id == groupId));
          }
          universityItemList.Add(new UniversityItem(stringBuilder.ToString(), group1));
        }
      }
      finally
      {
        if (enumerator != null)
          ((IDisposable) enumerator).Dispose();
      }
      return (IEnumerable<UniversityItem>) universityItemList;
    }

    public string GetData()
    {
      return (this.Data ?? "").ToString();
    }
  }
}
