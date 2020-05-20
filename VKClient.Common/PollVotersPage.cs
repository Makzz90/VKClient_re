using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class PollVotersPage : PageBase
  {
    private bool _isInitialized;
    internal ExtendedLongListSelector listBoxVoters;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    private PollVotersViewModel PollVotersVM
    {
      get
      {
        return base.DataContext as PollVotersViewModel;
      }
    }

    public PollVotersPage()
    {
      this.InitializeComponent();
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBoxVoters.ScrollToTop());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxVoters);
      this.listBoxVoters.OnRefresh = (Action) (() => this.PollVotersVM.VotersVM.LoadData(true, false,  null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      long ownerId = long.Parse(((Page) this).NavigationContext.QueryString["OwnerId"]);
      long num1 = long.Parse(((Page) this).NavigationContext.QueryString["PollId"]);
      long num2 = long.Parse(((Page) this).NavigationContext.QueryString["AnswerId"]);
      string str = ((Page) this).NavigationContext.QueryString["AnswerText"];
      long pollId = num1;
      long answerId = num2;
      string answerText = str;
      PollVotersViewModel pollVotersViewModel = new PollVotersViewModel(ownerId, pollId, answerId, answerText);
      pollVotersViewModel.LoadData();
      base.DataContext = pollVotersViewModel;
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      this.PollVotersVM.VotersVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void ExtendedLongListSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FriendHeader selectedItem = this.listBoxVoters.SelectedItem as FriendHeader;
      if (selectedItem == null)
        return;
      Navigator.Current.NavigateToUserProfile(selectedItem.UserId, selectedItem.User.Name, "", false);
      this.listBoxVoters.SelectedItem = null;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/PollVotersPage.xaml", UriKind.Relative));
      this.listBoxVoters = (ExtendedLongListSelector) base.FindName("listBoxVoters");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
    }
  }
}
