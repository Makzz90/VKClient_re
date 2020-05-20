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
      this.Data = (object) description;
    }

    public static IEnumerable<MilitaryItem> GetMilitaryItems(List<Military> military, List<Country> countries)
    {
      List<MilitaryItem> militaryItemList = new List<MilitaryItem>();
      if (military.IsNullOrEmpty())
        return (IEnumerable<MilitaryItem>) militaryItemList;
      foreach (Military military1 in military)
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(military1.unit);
        bool flag = false;
        long countryId = military1.country_id;
        if (countryId > 0L)
        {
          Country country = countries == null ? (Country) null : countries.FirstOrDefault<Country>((Func<Country, bool>) (c => c.id == countryId));
          if (country != null)
          {
            stringBuilder.Append("\n").Append(country.name);
            flag = true;
          }
        }
        if (military1.from > 0)
        {
          stringBuilder.Append(flag ? ", " : "\n");
          if (military1.until == military1.from)
            stringBuilder.Append(string.Format("{0} {1} {2}", (object) CommonResources.In, (object) military1.from, (object) CommonResources.YearsShort));
          else if (military1.until == 0)
            stringBuilder.Append(string.Format("{0} {1} {2}", (object) CommonResources.Since, (object) military1.from, (object) CommonResources.YearsShort));
          else
            stringBuilder.Append(military1.from);
        }
        if (military1.until > 0 && military1.until != military1.from)
        {
          if (military1.from > 0)
            stringBuilder.Append(string.Format("–{0}", (object) military1.until));
          else
            stringBuilder.Append(string.Format("{0}{1} {2} {3}", (object) (flag ? ", " : "\n"), (object) CommonResources.Until, (object) military1.until, (object) CommonResources.YearsShort));
        }
        militaryItemList.Add(new MilitaryItem(stringBuilder.ToString()));
      }
      return (IEnumerable<MilitaryItem>) militaryItemList;
    }

    public string GetData()
    {
      return (this.Data ?? (object) "").ToString();
    }
  }
}
