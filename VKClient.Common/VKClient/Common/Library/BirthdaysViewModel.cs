using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VKClient.Audio.Base.BLExtensions;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class BirthdaysViewModel : ViewModelBase, ICollectionDataProvider<List<User>, BirthdayInfo>
  {
    private GenericCollectionViewModel<List<User>, BirthdayInfo> _bithdaysGroupsViewModel;

    public GenericCollectionViewModel<List<User>, BirthdayInfo> BithdaysGroupsViewModel
    {
      get
      {
        return this._bithdaysGroupsViewModel;
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.Birthdays_Title;
      }
    }

    public Func<List<User>, ListWithCount<BirthdayInfo>> ConverterFunc
    {
      get
      {
        return (Func<List<User>, ListWithCount<BirthdayInfo>>) (list =>
        {
          DateTime now = DateTime.Now;
          int currentMonth = now.Month;
          int currentDay = now.Day;
          IEnumerable<User> source = list.OrderBy<User, int>((Func<User, int>) (u => this.GetDiffFromDate(currentMonth, currentDay, u.GetBDateMonth(), u.GetBDateDay()))).Where<User>((Func<User, bool>) (u => !string.IsNullOrWhiteSpace(u.bdate)));
          ListWithCount<BirthdayInfo> listWithCount = new ListWithCount<BirthdayInfo>();
          CultureInfo culture = new CultureInfo("ru-RU");
          List<BirthdayInfo> list1 = source.Select<User, BirthdayInfo>((Func<User, BirthdayInfo>) (u =>
          {
            string str = "";
            if (!string.IsNullOrWhiteSpace(u.bdate))
            {
              string bdate = u.bdate;
              if (u.bdate.LastIndexOf('.') == u.bdate.IndexOf('.'))
                bdate += ".1970";
              DateTime result;
              if (DateTime.TryParse(bdate, (IFormatProvider) culture, DateTimeStyles.None, out result))
                str = result.ToString("M");
            }
            string subtitle = str;
            if (u.IsBirthdayToday())
              subtitle = CommonResources.HasABirthdayToday;
            else if (u.IsBirthdayTomorrow())
              subtitle = CommonResources.HasABirthdayTomorrow;
            return new BirthdayInfo(u, subtitle);
          })).ToList<BirthdayInfo>();
          listWithCount.List = list1;
          return listWithCount;
        });
      }
    }

    public BirthdaysViewModel()
    {
      this._bithdaysGroupsViewModel = new GenericCollectionViewModel<List<User>, BirthdayInfo>((ICollectionDataProvider<List<User>, BirthdayInfo>) this)
      {
        NeedCollectionCountBeforeFullyLoading = true
      };
    }

    public async void GetData(GenericCollectionViewModel<List<User>, BirthdayInfo> caller, int offset, int count, Action<BackendResult<List<User>, ResultCode>> callback)
    {
      if (offset != 0)
        return;
      SavedContacts friends = await FriendsCache.Instance.GetFriends();
      callback(new BackendResult<List<User>, ResultCode>(ResultCode.Succeeded, friends.SavedUsers));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<List<User>, BirthdayInfo> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoFriends;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneFriendFrm, CommonResources.TwoFourFriendsFrm, CommonResources.FiveFriendsFrm, true, null, false);
    }

    private int GetDiffFromDate(int baseMonth, int baseDay, int month, int day)
    {
      int num1 = baseMonth * 100 + baseDay;
      int num2 = month * 100 + day;
      if (num2 >= num1)
        return num2 - num1;
      return num2 - num1 + 100000;
    }
  }
}
