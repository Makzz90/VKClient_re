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
  public class CareerItem : ProfileInfoItem
  {
    public string GroupImage { get; private set; }

    public double TitleMaxWidth { get; private set; }

    private CareerItem(string title, string description, Group group = null)
      : base(ProfileInfoItemType.Full)
    {
      this.Title = title;
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

    public static IEnumerable<CareerItem> GetCareerItems(List<Career> career, List<City> cities, List<Group> groups)
    {
      List<CareerItem> careerItemList = new List<CareerItem>();
      if (career.IsNullOrEmpty())
        return (IEnumerable<CareerItem>) careerItemList;
      career.Reverse();
      List<Career>.Enumerator enumerator = career.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Career current = enumerator.Current;
          StringBuilder stringBuilder = new StringBuilder();
          bool flag = false;
          long cityId = current.city_id;
          if (cityId > 0L)
          {
              City city = cities == null ? null : Enumerable.FirstOrDefault<City>(cities, (Func<City, bool>)(c => c.id == cityId));
            if (city != null)
            {
              stringBuilder.Append(city.name);
              flag = true;
            }
          }
          if (current.from > 0L)
          {
            if (flag)
              stringBuilder.Append(", ");
            if (current.until == current.from)
              stringBuilder.Append(string.Format("{0} {1} {2}", CommonResources.In, current.from, CommonResources.YearsShort));
            else if (current.until == 0L)
              stringBuilder.Append(string.Format("{0} {1} {2}", CommonResources.Since, current.from, CommonResources.YearsShort));
            else
              stringBuilder.Append(current.from);
          }
          if (current.until > 0L && current.until != current.from)
          {
            if (current.from > 0L)
              stringBuilder.Append(string.Format("–{0}", current.until));
            else
              stringBuilder.Append(string.Format("{0}{1} {2} {3}", (flag ? ", " : ""), CommonResources.Until, current.until, CommonResources.YearsShort));
          }
          if (!string.IsNullOrEmpty(current.position))
            stringBuilder.Append("\n").Append(current.position);
          Group group =  null;
          long careerGroupId = current.group_id;
          string title;
          if (!groups.IsNullOrEmpty() && careerGroupId != 0L)
          {
              group = (Group)Enumerable.First<Group>(groups, (Func<Group, bool>)(g => g.id == careerGroupId));
            title = group.name;
          }
          else
            title = current.company;
          careerItemList.Add(new CareerItem(title, stringBuilder.ToString(), group));
        }
      }
      finally
      {
        enumerator.Dispose();
      }
      return (IEnumerable<CareerItem>) careerItemList;
    }
  }
}
