using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Groups.Management.Information.Library;

namespace VKClient.Groups.Management.Information.UC
{
  public class CommunityPlacementUC : UserControl
  {
    private bool _contentLoaded;

    public CommunityPlacementViewModel ViewModel
    {
      get
      {
        return base.DataContext as CommunityPlacementViewModel;
      }
    }

    public CommunityPlacementUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
        if (this.ViewModel.EditButtonVisibility != Visibility.Collapsed)
        return;
      this.EditButton_OnClicked(sender, e);
    }

    private void EditButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      e.Handled = true;
      this.ViewModel.NavigateToPlacementSelection();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/Information/UC/CommunityPlacementUC.xaml", UriKind.Relative));
    }
  }
}
