using System;

namespace VKClient.Common.Framework.DatePicker
{
  public class MyDatePicker : Microsoft.Phone.Controls.DatePicker
  {
    public MyDatePicker()
    {
      this.PickerPageUri = new Uri("/VKClient.Common;component/Framework/DatePicker/MyDatePickerPage.xaml", UriKind.Relative);
    }
  }
}
