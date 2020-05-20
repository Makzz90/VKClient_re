using System;
using System.Windows;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Library
{
  public sealed class BlockDurationPickerViewModel : ViewModelBase
  {
    public readonly DateTime TimeNow = DateTime.Now;
    private readonly int _currentDuration;

    public string CurrentDuration
    {
      get
      {
        string str = UIStringFormatterHelper.FormateDateForEventUI(Extensions.UnixTimeStampToDateTime((double) this._currentDuration, true));
        if (this._currentDuration != 0)
          return string.Format("{0} {1}", CommonResources.Unblocking, str);
        return CommonResources.UnblockByManager;
      }
    }

    public Visibility ForeverVisibility
    {
      get
      {
        return ((uint) this._currentDuration > 0U).ToVisiblity();
      }
    }

    public string YearDuration
    {
      get
      {
        return string.Format("{0} {1}", CommonResources.Unblocking, UIStringFormatterHelper.FormateDateForEventUI(this.TimeNow.AddYears(1)));
      }
    }

    public string MonthDuration
    {
      get
      {
        return string.Format("{0} {1}", CommonResources.Unblocking, UIStringFormatterHelper.FormateDateForEventUI(this.TimeNow.AddMonths(1)));
      }
    }

    public string WeekDuration
    {
      get
      {
        return string.Format("{0} {1}", CommonResources.Unblocking, UIStringFormatterHelper.FormateDateForEventUI(this.TimeNow.AddDays(7.0)));
      }
    }

    public string DayDuration
    {
      get
      {
        return string.Format("{0} {1}", CommonResources.Unblocking, UIStringFormatterHelper.FormateDateForEventUI(this.TimeNow.AddDays(1.0)));
      }
    }

    public string HourDuration
    {
      get
      {
        return string.Format("{0} {1}", CommonResources.Unblocking, UIStringFormatterHelper.FormateDateForEventUI(this.TimeNow.AddHours(1.0)));
      }
    }

    public BlockDurationPickerViewModel(int currentDuration)
    {
      this._currentDuration = currentDuration;
    }
  }
}
