using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Profiles.Shared.ViewModels;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class SuggestedPostponedPostsUC : UserControl
  {
    private bool _contentLoaded;

    private SuggestedPostponedPostsViewModel ViewModel
    {
      get
      {
        return this.DataContext as SuggestedPostponedPostsViewModel;
      }
    }

    public SuggestedPostponedPostsUC()
    {
      this.InitializeComponent();
    }

    private void BorderSuggested_OnTap(object sender, GestureEventArgs e)
    {
      if (this.ViewModel == null)
        return;
      this.ViewModel.OpenSuggestedPostsPage();
    }

    private void BorderPostponed_OnTap(object sender, GestureEventArgs e)
    {
      if (this.ViewModel == null)
        return;
      this.ViewModel.OpenPostponedPostsPage();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/SuggestedPostponedPostsUC.xaml", UriKind.Relative));
    }
  }
}
