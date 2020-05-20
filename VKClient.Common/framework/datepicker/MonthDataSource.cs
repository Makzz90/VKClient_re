using System;

namespace VKClient.Common.Framework.DatePicker
{
  public class MonthDataSource : DataSource
  {
    protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
    {
      int num = 12;
      int month = (num + relativeDate.Month - 1 + delta) % num + 1;
      int day = Math.Min(relativeDate.Day, DateTime.DaysInMonth(relativeDate.Year, month));
      return new DateTime?(new DateTime(relativeDate.Year, month, day, relativeDate.Hour, relativeDate.Minute, relativeDate.Second));
    }
  }
}
