using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Users.ViewModels;
using VKClient.Common.UC;
using VKClient.Groups.Library;

namespace VKClient.Common.Profiles.Users.Views
{
  public class SubscriptionsPage : PageBase
  {
    private bool _isInitialized;
    private SubscriptionsViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    internal Pivot pivot;
    internal ExtendedLongListSelector listPages;
    internal ExtendedLongListSelector listGroups;
    private bool _contentLoaded;

    public SubscriptionsPage()
    {
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = CommonResources.SubscriptionsPage_OwnSubscriptionsTitle;
      this.ucHeader.OnHeaderTap = (Action) (() =>
      {
        switch (this.pivot.SelectedIndex)
        {
          case 0:
            this.listPages.ScrollToTop();
            break;
          case 1:
            this.listGroups.ScrollToTop();
            break;
        }
      });
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._viewModel = new SubscriptionsViewModel(long.Parse(((Page) this).NavigationContext.QueryString["UserId"]));
      this._viewModel.LoadData(false,  null);
      base.DataContext = this._viewModel;
      this._isInitialized = true;
    }

    private void ListPages_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      UserGroupHeader selectedItem = this.listPages.SelectedItem as UserGroupHeader;
      if (selectedItem == null)
        return;
      if (selectedItem.UserHeader != null)
        Navigator.Current.NavigateToUserProfile(selectedItem.UserHeader.UserId, selectedItem.UserHeader.User.Name, "", false);
      else if (selectedItem.GroupHeader != null)
        Navigator.Current.NavigateToGroup(selectedItem.GroupHeader.Group.id, selectedItem.GroupHeader.Group.name, false);
      this.listPages.SelectedItem = null;
    }

    private void ListGroups_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      GroupHeader selectedItem = this.listGroups.SelectedItem as GroupHeader;
      if (selectedItem == null)
        return;
      Navigator.Current.NavigateToGroup(selectedItem.Group.id, selectedItem.Group.name, false);
      this.listGroups.SelectedItem = null;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Users/Views/SubscriptionsPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.pivot = (Pivot) base.FindName("pivot");
      this.listPages = (ExtendedLongListSelector) base.FindName("listPages");
      this.listGroups = (ExtendedLongListSelector) base.FindName("listGroups");
    }
  }
}
