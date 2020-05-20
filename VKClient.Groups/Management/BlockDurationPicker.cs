using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
  public class BlockDurationPicker : PageBase
  {
    internal GenericHeaderUC Header;
    private bool _contentLoaded;

    public BlockDurationPickerViewModel ViewModel
    {
      get
      {
        return base.DataContext as BlockDurationPickerViewModel;
      }
    }

    public BlockDurationPicker()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      base.DataContext = (new BlockDurationPickerViewModel(int.Parse(((Page) this).NavigationContext.QueryString["DurationUnixTime"])));
    }

    private void CurrentDuration_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void Forever_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ParametersRepository.SetParameterForId("BlockDurationUnixTime", 0);
      Navigator.Current.GoBack();
    }

    private void ForYear_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
        ParametersRepository.SetParameterForId("BlockDurationUnixTime", Extensions.DateTimeToUnixTimestamp(this.ViewModel.TimeNow.AddYears(1).ToUniversalTime(), true));
        Navigator.Current.GoBack();
    }


    private void ForMonth_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
        ParametersRepository.SetParameterForId("BlockDurationUnixTime", Extensions.DateTimeToUnixTimestamp(this.ViewModel.TimeNow.AddMonths(1).ToUniversalTime(), true));
        Navigator.Current.GoBack();
    }


    private void ForWeek_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
        ParametersRepository.SetParameterForId("BlockDurationUnixTime", Extensions.DateTimeToUnixTimestamp(this.ViewModel.TimeNow.AddDays(7.0).ToUniversalTime(), true));
        Navigator.Current.GoBack();
    }


    private void ForDay_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
        ParametersRepository.SetParameterForId("BlockDurationUnixTime", Extensions.DateTimeToUnixTimestamp(this.ViewModel.TimeNow.AddDays(1.0).ToUniversalTime(), true));
        Navigator.Current.GoBack();
    }


    private void ForHour_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
        ParametersRepository.SetParameterForId("BlockDurationUnixTime", Extensions.DateTimeToUnixTimestamp(this.ViewModel.TimeNow.AddHours(1.0).ToUniversalTime(), true));
        Navigator.Current.GoBack();
    }


    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/BlockDurationPicker.xaml", UriKind.Relative));
      this.Header = (GenericHeaderUC) base.FindName("Header");
    }
  }
}
