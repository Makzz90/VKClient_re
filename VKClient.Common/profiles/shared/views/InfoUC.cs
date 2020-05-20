using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Profiles.Shared.ViewModels;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class InfoUC : UserControl
  {
    private bool _contentLoaded;

    private ProfileInfoViewModelBase ViewModel
    {
      get
      {
        return base.DataContext as ProfileInfoViewModelBase;
      }
    }

    public InfoUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void Item_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      InfoListItem dataContext = ((FrameworkElement) sender).DataContext as InfoListItem;
      if (dataContext == null || dataContext.TapAction == null)
        return;
      dataContext.TapAction();
    }

    private void BorderFullInformation_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ProfileInfoViewModelBase viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      viewModel.ShowFullInfoPopup();
    }

    private void BorderWikiPage_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ProfileInfoViewModelBase viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      viewModel.OpenWikiPage();
    }

    private void BorderLink_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ProfileInfoViewModelBase viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      viewModel.OpenLink();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/InfoUC.xaml", UriKind.Relative));
    }
  }
}
