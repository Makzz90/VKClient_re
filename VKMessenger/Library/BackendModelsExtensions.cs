using System;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKMessenger.Library
{
  public static class BackendModelsExtensions
  {
    public static string GetUserStatusString(this UserStatus userStatus, bool isMale)
    {
      if (userStatus == null || userStatus.time == 0L)
        return "";
      if (userStatus.online == 1L)
        return CommonResources.Conversation_Online;
      DateTime dateTime = Extensions.UnixTimeStampToDateTime((double) userStatus.time, true);
      string empty = string.Empty;
      DateTime now = DateTime.Now;
      int int32 = Convert.ToInt32(Math.Floor((now - dateTime).TotalMinutes));
      string str;
      if (int32 > 0 && int32 < 60)
      {
        if (int32 < 2)
        {
          str = !isMale ? CommonResources.Conversation_LastSeenAMomentAgoFemale : CommonResources.Conversation_LastSeenAMomentAgoMale;
        }
        else
        {
          int num = int32 % 10;
          str = !isMale ? (num != 1 || int32 >= 10 && int32 <= 20 ? (num >= 5 || num == 0 || int32 >= 10 && int32 <= 20 ? string.Format(CommonResources.Conversation_LastSeenXFiveMinutesAgoFemaleFrm, int32) : string.Format(CommonResources.Conversation_LastSeenXTwoFourMinutesAgoFemaleFrm, int32)) : string.Format(CommonResources.Conversation_LastSeenX1MinutesAgoFemaleFrm, int32)) : (num != 1 || int32 >= 10 && int32 <= 20 ? (num >= 5 || num == 0 || int32 >= 10 && int32 <= 20 ? string.Format(CommonResources.Conversation_LastSeenXFiveMinutesAgoMaleFrm, int32) : string.Format(CommonResources.Conversation_LastSeenXTwoFourMinutesAgoMaleFrm, int32)) : string.Format(CommonResources.Conversation_LastSeenX1MinutesAgoMaleFrm, int32));
        }
      }
      else
        str = !(now.Date == dateTime.Date) ? (!(now.AddDays(-1.0).Date == dateTime.Date) ? (now.Year != dateTime.Year ? (!isMale ? string.Format(CommonResources.Conversation_LastSeenOnFemaleFrm, dateTime.ToString("dd.MM.yyyy"), dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenOnMaleFrm, dateTime.ToString("dd.MM.yyyy"), dateTime.ToString("HH:mm"))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenOnFemaleFrm, dateTime.ToString("dd.MM"), dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenOnMaleFrm, dateTime.ToString("dd.MM"), dateTime.ToString("HH:mm")))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenYesterdayFemaleFrm, dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenYesterdayMaleFrm, dateTime.ToString("HH:mm")))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenTodayFemaleFrm, dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenTodayMaleFrm, dateTime.ToString("HH:mm")));
      return str;
    }
  }
}
