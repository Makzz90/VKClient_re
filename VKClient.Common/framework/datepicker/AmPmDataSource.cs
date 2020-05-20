using System;

namespace VKClient.Common.Framework.DatePicker
{
  public class AmPmDataSource : DataSource
  {
    protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
    {
      int num = 24;
      int hour = relativeDate.Hour + delta * (num / 2);
      if (hour < 0 || num <= hour)
        return new DateTime?();
      return new DateTime?(new DateTime(relativeDate.Year, relativeDate.Month, relativeDate.Day, hour, relativeDate.Minute, 0));
    }
  }
}
