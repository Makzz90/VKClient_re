using System;

namespace VKClient.Common.Framework.DatePicker
{
  public class MinuteDataSource : DataSource
  {
    protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
    {
      int num = 60;
      int minute = (num + relativeDate.Minute + delta) % num;
      return new DateTime?(new DateTime(relativeDate.Year, relativeDate.Month, relativeDate.Day, relativeDate.Hour, minute, 0));
    }
  }
}
