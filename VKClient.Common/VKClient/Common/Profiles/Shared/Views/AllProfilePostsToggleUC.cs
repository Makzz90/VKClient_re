using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Profiles.Shared.ViewModels;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class AllProfilePostsToggleUC : UserControl
  {
    private bool _contentLoaded;

    private AllProfilePostsToggleViewModel ViewModel
    {
      get
      {
        return this.DataContext as AllProfilePostsToggleViewModel;
      }
    }

    public AllProfilePostsToggleUC()
    {
      this.InitializeComponent();
    }

    private void AllPosts_OnTap(object sender, GestureEventArgs e)
    {
      if (this.ViewModel == null || this.ViewModel.IsLocked)
        return;
      this.ViewModel.IsAllPosts = true;
    }

    private void ProfilePosts_OnTap(object sender, GestureEventArgs e)
    {
      if (this.ViewModel == null || this.ViewModel.IsLocked)
        return;
      this.ViewModel.IsAllPosts = false;
    }

    private void Search_OnTap(object sender, GestureEventArgs e)
    {
      AllProfilePostsToggleViewModel viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      viewModel.NavigateToSearch();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/AllProfilePostsToggleUC.xaml", UriKind.Relative));
    }
  }
}
