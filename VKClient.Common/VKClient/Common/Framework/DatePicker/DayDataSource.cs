using System;

namespace VKClient.Common.Framework.DatePicker
{
  public class DayDataSource : DataSource
  {
    protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
    {
      int num = DateTime.DaysInMonth(relativeDate.Year, relativeDate.Month);
      int day = (num + relativeDate.Day - 1 + delta) % num + 1;
      return new DateTime?(new DateTime(relativeDate.Year, relativeDate.Month, day, relativeDate.Hour, relativeDate.Minute, relativeDate.Second));
    }
  }
}
