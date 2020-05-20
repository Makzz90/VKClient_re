using Microsoft.Phone.Controls;
using System;

namespace VKClient.Common.Framework.DatePicker
{
  public class PostScheduleTimePicker : TimePicker
  {
    public PostScheduleTimePicker()
    {
      this.PickerPageUri = new Uri("/VKClient.Common;component/Framework/DatePicker/PostScheduleTimePickerPage.xaml", UriKind.Relative);
    }
  }
}
