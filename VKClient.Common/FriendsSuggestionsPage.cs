using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class FriendsSuggestionsPage : PageBase
  {
    private bool _isInitialized;
    internal GenericHeaderUC ucHeader;
    internal FriendsSearchUC ucFriendsSearch;
    private bool _contentLoaded;

    public FriendsSuggestionsPage()
    {
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = CommonResources.PageTitle_FindFriends;
      this.ucHeader.OnHeaderTap = (Action) (() => this.ucFriendsSearch.ScrollToTop());
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      FriendsSuggestionsViewModel suggestionsViewModel = new FriendsSuggestionsViewModel();
      base.DataContext = suggestionsViewModel;
      suggestionsViewModel.FriendsSearchVM.LoadData();
      this._isInitialized = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/FriendsSuggestionsPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucFriendsSearch = (FriendsSearchUC) base.FindName("ucFriendsSearch");
    }
  }
}
