using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class PhoneItem : ProfileContactInfoItem, IProfileInfoSupportCopyToClipboard
  {
    private PhoneItem(string title, string phoneNumber)
      : base("/Resources/Profile/Contacts/ProfilePhone.png", phoneNumber)
    {
      this.Title = title;
      this.Data = phoneNumber;
      this.NavigationAction = (Action) (() =>
      {
        string input = this.Data.ToString();
        if (!Regex.IsMatch(input, "\\d"))
          return;
        PhoneCallTask phoneCallTask = new PhoneCallTask();
        string str = input;
        phoneCallTask.PhoneNumber = str;
        phoneCallTask.Show();
      });
    }

    public static IEnumerable<PhoneItem> GetPhones(UserData profileData)
    {
      List<PhoneItem> phoneItemList = new List<PhoneItem>();
      User user = profileData.user;
      if (!string.IsNullOrEmpty(user.mobile_phone))
      {
        List<string> phoneNumbers = BaseFormatterHelper.ParsePhoneNumbers(user.mobile_phone);
        phoneItemList.AddRange((IEnumerable<PhoneItem>)Enumerable.Select<string, PhoneItem>(phoneNumbers, (Func<string, PhoneItem>)(phoneNumber => new PhoneItem(CommonResources.ProfilePage_Info_MobilePhone, phoneNumber))));
      }
      if (!string.IsNullOrEmpty(user.home_phone))
      {
        List<string> phoneNumbers = BaseFormatterHelper.ParsePhoneNumbers(user.home_phone);
        phoneItemList.AddRange((IEnumerable<PhoneItem>)Enumerable.Select<string, PhoneItem>(phoneNumbers, (Func<string, PhoneItem>)(phoneNumber => new PhoneItem(CommonResources.ProfilePage_Info_AlternativePhone, phoneNumber))));
      }
      return (IEnumerable<PhoneItem>) phoneItemList;
    }

    public string GetData()
    {
      return (this.Data ?? "").ToString();
    }
  }
}
