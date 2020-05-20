using Microsoft.Phone.Tasks;
using System;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class BirthdayItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    public BirthdayItem(UserData profileData)
      : base(ProfileInfoItemType.RichText)
    {
      BirthdayItem birthdayItem = this;
      this.Title = CommonResources.ProfilePage_Info_BirthDate;
      string[] strArray = profileData.user.bdate.Split('.');
      if (strArray.Length < 2)
        return;
      int num1 = int.Parse(strArray[0]);
      int month = int.Parse(strArray[1]);
      int num2 = 0;
      if (strArray.Length > 2)
        num2 = int.Parse(strArray[2]);
      string str1 = string.Format("{0} {1}", num1, BirthdayItem.GetOfMonthStr(month));
      if (num2 != 0)
        str1 = str1 + " " + num2;
      this.Data = str1;
      this.NavigationAction = (Action) (() =>
      {
        SaveAppointmentTask saveAppointmentTask = new SaveAppointmentTask();
        DateTime? nullable = new DateTime?(birthdayItem.GetBirthday(profileData.user));
        saveAppointmentTask.StartTime = nullable;
        string str2 = string.Format("{0}: {1}", CommonResources.ProfilePage_Info_Birthday, profileData.user.Name);
        saveAppointmentTask.Subject = str2;
        int num3 = 0;
        saveAppointmentTask.IsAllDayEvent = (num3 != 0);
        saveAppointmentTask.Show();
      });
    }

    private DateTime GetBirthday(User user)
    {
      string[] strArray = user.bdate.Split('.');
      if (strArray.Length < 2)
        return DateTime.MinValue;
      int day = int.Parse(strArray[0]);
      DateTime dateTime = new DateTime(DateTime.Now.Year, int.Parse(strArray[1]), day);
      if (DateTime.Now > dateTime)
        dateTime = dateTime.AddYears(1);
      return dateTime;
    }

    private static string GetOfMonthStr(int month)
    {
      switch (month)
      {
        case 1:
          return CommonResources.OfJanuary;
        case 2:
          return CommonResources.OfFebruary;
        case 3:
          return CommonResources.OfMarch;
        case 4:
          return CommonResources.OfApril;
        case 5:
          return CommonResources.OfMay;
        case 6:
          return CommonResources.OfJune;
        case 7:
          return CommonResources.OfJuly;
        case 8:
          return CommonResources.OfAugust;
        case 9:
          return CommonResources.OfSeptember;
        case 10:
          return CommonResources.OfOctober;
        case 11:
          return CommonResources.OfNovember;
        case 12:
          return CommonResources.OfDecember;
        default:
          return "";
      }
    }

    public string GetData()
    {
      return (this.Data ?? "").ToString();
    }
  }
}
