using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class UsersSearchResultsPage : PageBase
  {
    private bool _isInitialized;
    private UsersSearchResultsViewModel _viewModel;
    internal TextBox textBoxSearch;
    internal TextBlock textBlockWatermarkText;
    internal ExtendedLongListSelector listBoxUsers;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    public UsersSearchResultsPage()
    {
      this.InitializeComponent();
      this.SuppressOpenMenuTapArea = true;
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxUsers);
      this.listBoxUsers.OnRefresh = (Action) (() => this._viewModel.ReloadData());
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      string query = ((Page) this).NavigationContext.QueryString["Query"];
      this._viewModel = new UsersSearchResultsViewModel(query);
      this._viewModel.ReloadData();
      this.textBoxSearch.Text = query;
      ((UIElement) this.textBlockWatermarkText).Opacity = (string.IsNullOrEmpty(this.textBoxSearch.Text) ? 1.0 : 0.0);
      base.DataContext = this._viewModel;
      this._isInitialized = true;
    }

    private void TextBoxSearch_OnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      ((Control) this).Focus();
    }

    private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((UIElement) this.textBlockWatermarkText).Opacity = (string.IsNullOrEmpty(this.textBoxSearch.Text) ? 1.0 : 0.0);
      this._viewModel.SearchName = this.textBoxSearch.Text;
    }

    private void TextBoxSearch_OnGotFocus(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(this.textBoxSearch.Text))
        return;
      this.textBoxSearch.SelectAll();
    }

    private void ListBoxUsers_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      this._viewModel.SearchVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void ListBoxUsers_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      ((Control) this.listBoxUsers).Focus();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UsersSearchResultsPage.xaml", UriKind.Relative));
      this.textBoxSearch = (TextBox) base.FindName("textBoxSearch");
      this.textBlockWatermarkText = (TextBlock) base.FindName("textBlockWatermarkText");
      this.listBoxUsers = (ExtendedLongListSelector) base.FindName("listBoxUsers");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
    }
  }
}
