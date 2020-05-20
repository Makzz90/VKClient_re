using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework.DatePicker;
using VKClient.Groups.Management.Information.Library;

namespace VKClient.Groups.Management.Information.UC
{
  public class EventDatesUC : UserControl
  {
    internal PostScheduleDatePicker StartDatePicker;
    internal PostScheduleTimePicker StartTimePicker;
    internal PostScheduleDatePicker FinishDatePicker;
    internal PostScheduleTimePicker FinishTimePicker;
    private bool _contentLoaded;

    public EventDatesViewModel ViewModel
    {
      get
      {
        return base.DataContext as EventDatesViewModel;
      }
    }

    public EventDatesUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void StartDatePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", (BindingFlags) 292, Type.DefaultBinder, this.StartDatePicker,  null);
    }

    private void StartTimePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", (BindingFlags) 292, Type.DefaultBinder, this.StartTimePicker,  null);
    }

    private void FinishDatePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", (BindingFlags) 292, Type.DefaultBinder, this.FinishDatePicker,  null);
    }

    private void FinishTimePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", (BindingFlags) 292, Type.DefaultBinder, this.FinishTimePicker,  null);
    }

    private void SetFinishTimeButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      this.ViewModel.FinishFieldsVisibility = Visibility.Visible;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/Information/UC/EventDatesUC.xaml", UriKind.Relative));
      this.StartDatePicker = (PostScheduleDatePicker) base.FindName("StartDatePicker");
      this.StartTimePicker = (PostScheduleTimePicker) base.FindName("StartTimePicker");
      this.FinishDatePicker = (PostScheduleDatePicker) base.FindName("FinishDatePicker");
      this.FinishTimePicker = (PostScheduleTimePicker) base.FindName("FinishTimePicker");
    }
  }
}
