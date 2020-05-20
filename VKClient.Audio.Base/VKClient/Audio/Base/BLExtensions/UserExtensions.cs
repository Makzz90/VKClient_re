using System;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.BLExtensions
{
  public static class UserExtensions
  {
    public static bool IsBirthdayToday(this User user)
    {
      User user1 = user;
      DateTime now = DateTime.Now;
      int day = now.Day;
      now = DateTime.Now;
      int month = now.Month;
      return UserExtensions.IsBirthdayOnDate(user1, day, month);
    }

    public static bool IsBirthdayTomorrow(this User user)
    {
      DateTime dateTime = DateTime.Now.AddDays(1.0);
      return UserExtensions.IsBirthdayOnDate(user, dateTime.Day, dateTime.Month);
    }

    private static bool IsBirthdayOnDate(User user, int day, int month)
    {
      if (user == null || user.bdate == null)
        return false;
      string[] strArray = user.bdate.Split('.');
      int result1 = 0;
      int result2 = 0;
      if (strArray.Length >= 2 && int.TryParse(strArray[0], out result1) && (int.TryParse(strArray[1], out result2) && day == result1))
        return month == result2;
      return false;
    }

    public static int GetBDateMonth(this User user)
    {
      if (user == null || user.bdate == null)
        return 0;
      string[] strArray = user.bdate.Split('.');
      int result = 0;
      if (strArray.Length >= 2 && int.TryParse(strArray[1], out result))
        return result;
      return 0;
    }

    public static int GetBDateDay(this User user)
    {
      if (user == null || user.bdate == null)
        return 0;
      string[] strArray = user.bdate.Split('.');
      int result = 0;
      if (strArray.Length >= 1 && int.TryParse(strArray[0], out result))
        return result;
      return 0;
    }

    public static BirthDate GetBirthDate(this User user)
    {
      return new BirthDate(user);
    }

    public static void InitFromGroup(this User user, Group group)
    {
      user.id = -group.id;
      user.first_name = group.name;
      user.photo_max = group.photo_200;
      user.is_messages_blocked = group.is_messages_blocked;
    }
  }
}
