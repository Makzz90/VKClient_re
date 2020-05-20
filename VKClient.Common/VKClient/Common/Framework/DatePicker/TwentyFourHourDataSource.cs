using System;

namespace VKClient.Common.Framework.DatePicker
{
  public class TwentyFourHourDataSource : DataSource
  {
    protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
    {
      int num = 24;
      int hour = (num + relativeDate.Hour + delta) % num;
      return new DateTime?(new DateTime(relativeDate.Year, relativeDate.Month, relativeDate.Day, hour, relativeDate.Minute, 0));
    }
  }
}
