using System;
using System.Globalization;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.BLExtensions
{
  public static class BirthDateExtensions
  {
    public static string GetDateString(this BirthDate birthDate)
    {
      int? day = birthDate.Day;
      int? month = birthDate.Month;
      int? year = birthDate.Year;
      if (!day.HasValue || !month.HasValue)
        return "";
      CultureInfo cultureInfo = new CultureInfo("ru-RU");
      string str = string.Format("{0}.{1}.", day, month);
      DateTime result;
      if (DateTime.TryParse(!year.HasValue ? str + "1970" : str + year.ToString(), (IFormatProvider) cultureInfo, DateTimeStyles.None, out result))
        return result.ToString(year.HasValue ? "d MMMM yyyy" : "M");
      return "";
    }
  }
}
