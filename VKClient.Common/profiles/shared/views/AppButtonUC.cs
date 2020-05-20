using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Profiles.Shared.ViewModels;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class AppButtonUC : UserControl
  {
    private bool _contentLoaded;

    public AppButtonUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void AppButton_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ProfileAppViewModel dataContext = base.DataContext as ProfileAppViewModel;
      if (dataContext == null)
        return;
      dataContext.NavigateToApp();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/AppButtonUC.xaml", UriKind.Relative));
    }
  }
}
