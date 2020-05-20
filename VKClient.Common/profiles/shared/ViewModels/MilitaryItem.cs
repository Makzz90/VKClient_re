using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class MilitaryItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    private MilitaryItem(string description)
      : base(ProfileInfoItemType.RichText)
    {
      this.Title = CommonResources.MilitaryService_BranchUnit;
      this.Data = description;
    }

    public static IEnumerable<MilitaryItem> GetMilitaryItems(List<Military> military, List<Country> countries)
    {
      List<MilitaryItem> militaryItemList = new List<MilitaryItem>();
      if (military.IsNullOrEmpty())
        return (IEnumerable<MilitaryItem>) militaryItemList;
      List<Military>.Enumerator enumerator = military.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Military current = enumerator.Current;
          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.Append(current.unit);
          bool flag = false;
          long countryId = current.country_id;
          if (countryId > 0L)
          {
              Country country = countries == null ? null : Enumerable.FirstOrDefault<Country>(countries, (Func<Country, bool>)(c => c.id == countryId));
            if (country != null)
            {
              stringBuilder.Append("\n").Append(country.name);
              flag = true;
            }
          }
          if (current.from > 0)
          {
            stringBuilder.Append(flag ? ", " : "\n");
            if (current.until == current.from)
              stringBuilder.Append(string.Format("{0} {1} {2}", CommonResources.In, current.from, CommonResources.YearsShort));
            else if (current.until == 0)
              stringBuilder.Append(string.Format("{0} {1} {2}", CommonResources.Since, current.from, CommonResources.YearsShort));
            else
              stringBuilder.Append(current.from);
          }
          if (current.until > 0 && current.until != current.from)
          {
            if (current.from > 0)
              stringBuilder.Append(string.Format("–{0}", current.until));
            else
              stringBuilder.Append(string.Format("{0}{1} {2} {3}", (flag ? ", " : "\n"), CommonResources.Until, current.until, CommonResources.YearsShort));
          }
          militaryItemList.Add(new MilitaryItem(stringBuilder.ToString()));
        }
      }
      finally
      {
        enumerator.Dispose();
      }
      return (IEnumerable<MilitaryItem>) militaryItemList;
    }

    public string GetData()
    {
      return (this.Data ?? "").ToString();
    }
  }
}
