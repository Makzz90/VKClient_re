using System;

namespace VKClient.Common.Framework.DatePicker
{
  public class PostScheduleDatePicker : Microsoft.Phone.Controls.DatePicker
  {
    public PostScheduleDatePicker()
    {
      this.PickerPageUri = new Uri("/VKClient.Common;component/Framework/DatePicker/PostScheduleDatePickerPage.xaml", UriKind.Relative);
    }
  }
}
