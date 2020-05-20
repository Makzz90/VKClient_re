using System;

namespace VKClient.Common.Framework.DatePicker
{
  public class YearDataSource : DataSource
  {
    protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
    {
      if (1601 == relativeDate.Year || 3000 == relativeDate.Year)
        return new DateTime?();
      int year = relativeDate.Year + delta;
      int day = Math.Min(relativeDate.Day, DateTime.DaysInMonth(year, relativeDate.Month));
      return new DateTime?(new DateTime(year, relativeDate.Month, day, relativeDate.Hour, relativeDate.Minute, relativeDate.Second));
    }
  }
}
