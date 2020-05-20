using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Groups.Management.Information.Library;

namespace VKClient.Groups.Management.Information.UC
{
  public class AgeLimitsUC : UserControl
  {
    private bool _contentLoaded;

    public AgeLimitsViewModel ViewModel
    {
      get
      {
        return base.DataContext as AgeLimitsViewModel;
      }
    }

    public AgeLimitsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void SetAgeLimitsButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      this.ViewModel.FullFormVisibility = Visibility.Visible;
    }

    private void MoreInformation_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      string uri = "https://m.vk.com/agelimits?api_view=1";
      string lang = LangHelper.GetLang();
      if (!string.IsNullOrEmpty(lang))
        uri += string.Format("&lang={0}", lang);
      Navigator.Current.NavigateToWebUri(uri, true, false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/Information/UC/AgeLimitsUC.xaml", UriKind.Relative));
    }
  }
}
