using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Framework.DatePicker;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;

namespace VKClient.Common
{
  public class PostSchedulePage : PageBase
  {
    private bool _isInitialized;
    private PostScheduleViewModel _viewModel;
    private readonly ApplicationBarIconButton _appBarButtonCheck;
    private readonly ApplicationBarIconButton _appBarButtonCancel;
    internal PostScheduleDatePicker datePicker;
    internal PostScheduleTimePicker timePicker;
    private bool _contentLoaded;

    public PostSchedulePage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string chatEditAppBarSave = CommonResources.ChatEdit_AppBar_Save;
      applicationBarIconButton1.Text = chatEditAppBarSave;
      this._appBarButtonCheck = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string appBarCancel = CommonResources.AppBar_Cancel;
      applicationBarIconButton2.Text = appBarCancel;
      this._appBarButtonCancel = applicationBarIconButton2;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar1 = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar1.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar1.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar1.Opacity = num;
      ApplicationBar applicationBar2 = applicationBar1;
      applicationBar2.Buttons.Add(this._appBarButtonCheck);
      applicationBar2.Buttons.Add(this._appBarButtonCancel);
      this._appBarButtonCheck.Click+=(new EventHandler(this.AppBarButtonCheck_Click));
      this._appBarButtonCancel.Click+=(new EventHandler(this.AppBarButtonCancel_Click));
      this.ApplicationBar = ((IApplicationBar) applicationBar2);
    }

    private void AppBarButtonCheck_Click(object sender, EventArgs e)
    {
      TimerAttachment timerAttachment = new TimerAttachment()
      {
        ScheduledPublishDateTime = this._viewModel.GetScheduledDateTime()
      };
      if (timerAttachment.ScheduledPublishDateTime > DateTime.Now.AddYears(1))
      {
        MessageBox.Show(CommonResources.PostSchedule_InvalidPublishDate, CommonResources.Error, (MessageBoxButton) 0);
      }
      else
      {
        ParametersRepository.SetParameterForId("PickedTimer", timerAttachment);
        Navigator.Current.GoBack();
      }
    }

    private void AppBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      long ticks = long.Parse(((Page) this).NavigationContext.QueryString["PublishDateTime"]);
      this._viewModel = ticks > 0L ? new PostScheduleViewModel(new DateTime?(new DateTime(ticks))) : new PostScheduleViewModel(new DateTime?());
      base.DataContext = this._viewModel;
      this._isInitialized = true;
    }

    private void DatePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, Type.DefaultBinder, this.datePicker,  null);
    }

    private void TimePicker_OnClicked(object sender, RoutedEventArgs e)
    {
      typeof (Microsoft.Phone.Controls.DateTimePickerBase).InvokeMember("OpenPickerPage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, Type.DefaultBinder, this.timePicker,  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/PostSchedulePage.xaml", UriKind.Relative));
      this.datePicker = (PostScheduleDatePicker) base.FindName("datePicker");
      this.timePicker = (PostScheduleTimePicker) base.FindName("timePicker");
    }
  }
}
