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

    public static IEnumerable<CareerItem> GetCareerItems(List<Career> career, List<City> cities, List<Group> groups)
    {
      List<CareerItem> careerItemList = new List<CareerItem>();
      if (career.IsNullOrEmpty())
        return (IEnumerable<CareerItem>) careerItemList;
      career.Reverse();
      foreach (Career career1 in career)
      {
        StringBuilder stringBuilder = new StringBuilder();
        bool flag = false;
        long cityId = career1.city_id;
        if (cityId > 0L)
        {
          City city = cities == null ? (City) null : cities.FirstOrDefault<City>((Func<City, bool>) (c => c.id == cityId));
          if (city != null)
          {
            stringBuilder.Append(city.name);
            flag = true;
          }
        }
        if (career1.from > 0L)
        {
          if (flag)
            stringBuilder.Append(", ");
          if (career1.until == career1.from)
            stringBuilder.Append(string.Format("{0} {1} {2}", (object) CommonResources.In, (object) career1.from, (object) CommonResources.YearsShort));
          else if (career1.until == 0L)
            stringBuilder.Append(string.Format("{0} {1} {2}", (object) CommonResources.Since, (object) career1.from, (object) CommonResources.YearsShort));
          else
            stringBuilder.Append(career1.from);
        }
        if (career1.until > 0L && career1.until != career1.from)
        {
          if (career1.from > 0L)
            stringBuilder.Append(string.Format("–{0}", (object) career1.until));
          else
            stringBuilder.Append(string.Format("{0}{1} {2} {3}", (object) (flag ? ", " : ""), (object) CommonResources.Until, (object) career1.until, (object) CommonResources.YearsShort));
        }
        if (!string.IsNullOrEmpty(career1.position))
          stringBuilder.Append("\n").Append(career1.position);
        Group group = (Group) null;
        long careerGroupId = career1.group_id;
        string title;
        if (!groups.IsNullOrEmpty() && careerGroupId != 0L)
        {
          group = groups.First<Group>((Func<Group, bool>) (g => g.id == careerGroupId));
          title = group.name;
        }
        else
          title = career1.company;
        careerItemList.Add(new CareerItem(title, stringBuilder.ToString(), group));
      }
      return (IEnumerable<CareerItem>) careerItemList;
    }
  }
}
