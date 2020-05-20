using System;

namespace VKClient.Common.Framework.DatePicker
{
  public class TwelveHourDataSource : DataSource
  {
    protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
    {
      int num = 12;
      int hour = (num + relativeDate.Hour + delta) % num + (num <= relativeDate.Hour ? num : 0);
      return new DateTime?(new DateTime(relativeDate.Year, relativeDate.Month, relativeDate.Day, hour, relativeDate.Minute, 0));
    }
  }
}
