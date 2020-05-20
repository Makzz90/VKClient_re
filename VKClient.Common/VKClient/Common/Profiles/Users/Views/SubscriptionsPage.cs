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
      this._viewModel = new SubscriptionsViewModel(long.Parse(this.NavigationContext.QueryString["UserId"]));
      this._viewModel.LoadData(false, (Action<bool>) null);
      this.DataContext = (object) this._viewModel;
      this._isInitialized = true;
    }

    private void ListPages_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      UserGroupHeader userGroupHeader = this.listPages.SelectedItem as UserGroupHeader;
      if (userGroupHeader == null)
        return;
      if (userGroupHeader.UserHeader != null)
        Navigator.Current.NavigateToUserProfile(userGroupHeader.UserHeader.UserId, userGroupHeader.UserHeader.User.Name, "", false);
      else if (userGroupHeader.GroupHeader != null)
        Navigator.Current.NavigateToGroup(userGroupHeader.GroupHeader.Group.id, userGroupHeader.GroupHeader.Group.name, false);
      this.listPages.SelectedItem = null;
    }

    private void ListGroups_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      GroupHeader groupHeader = this.listGroups.SelectedItem as GroupHeader;
      if (groupHeader == null)
        return;
      Navigator.Current.NavigateToGroup(groupHeader.Group.id, groupHeader.Group.name, false);
      this.listGroups.SelectedItem = null;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Users/Views/SubscriptionsPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.pivot = (Pivot) this.FindName("pivot");
      this.listPages = (ExtendedLongListSelector) this.FindName("listPages");
      this.listGroups = (ExtendedLongListSelector) this.FindName("listGroups");
    }
  }
}
