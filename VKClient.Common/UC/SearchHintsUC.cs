using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class SearchHintsUC : UserControl
  {
    private static GenericSearchViewModel<SearchHint, SearchHintHeader> _viewModel;
    private static bool _isReloadScheduled;
    private static DialogService _flyout;
    internal Grid gridRoot;
    internal TextBox searchTextBox;
    internal TextBlock textBlockWatermarkText;
    internal ExtendedLongListSelector searchResultsListBox;
    private bool _contentLoaded;

    public SearchHintsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    public static void ShowPopup()
    {
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      dialogService.AnimationTypeChild = DialogService.AnimationTypes.SlideInversed;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      int num = 1;
      dialogService.ShowOnFrame = num != 0;
      SearchHintsUC._flyout = dialogService;
      if (SearchHintsUC._viewModel == null || SearchHintsUC._isReloadScheduled)
      {
        SearchHintsUC._isReloadScheduled = false;
        SearchHintsSearchDataProvider.Reset();
        SearchHintsUC._viewModel = new GenericSearchViewModel<SearchHint, SearchHintHeader>((ISearchDataProvider<SearchHint, SearchHintHeader>) new SearchHintsSearchDataProvider());
        SearchHintsUC._viewModel.LoadData(true, false, true, false, true);
      }
      SearchHintsUC searchHintsUc = new SearchHintsUC();
      GenericCollectionViewModel2<VKList<SearchHint>, SearchHintHeader> searchVm = SearchHintsUC._viewModel.SearchVM;
      ((FrameworkElement) searchHintsUc).DataContext = searchVm;
      searchHintsUc.searchTextBox.Text = (SearchHintsUC._viewModel.SearchString ?? "");
      SearchHintsUC uc = searchHintsUc;
      ((UIElement) uc.textBlockWatermarkText).Opacity = (string.IsNullOrEmpty(uc.searchTextBox.Text) ? 1.0 : 0.0);
      SearchHintsUC._flyout.Child = (FrameworkElement) uc;
      SearchHintsUC._flyout.Opened += (EventHandler) ((sender, args) => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (!string.IsNullOrWhiteSpace(uc.searchTextBox.Text))
          return;
        ((Control) uc.searchTextBox).Focus();
      })));
      SearchHintsUC._flyout.Closed += (EventHandler) ((sender, args) => ((FrameworkElement) uc).DataContext = null);
      SearchHintsUC._flyout.Show( null);
    }

    public static void Reset()
    {
      SearchHintsUC._isReloadScheduled = true;
    }

    private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((UIElement) this.textBlockWatermarkText).Opacity = (string.IsNullOrEmpty(this.searchTextBox.Text) ? 1.0 : 0.0);
      SearchHintsUC._viewModel.SearchString = this.searchTextBox.Text;
    }

    private void SearchBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      ((Control) this.searchTextBox).Foreground = ((Brush) Application.Current.Resources["PhoneTextBoxSearchMenuForegroundFocusedBrush"]);
      this.searchTextBox.SelectAll();
    }

    private void SearchBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      ((Control) this.searchTextBox).Foreground = ((Brush) Application.Current.Resources["PhoneTextBoxSearchMenuForegroundBrush"]);
    }

    private void SearchResultsListBox_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      ((Control) this.searchResultsListBox).Focus();
    }

    private void SearchResultsListBox_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      SearchHintsUC._viewModel.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void SearchHint_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      SearchHintHeader searchHint = ((FrameworkElement) sender).DataContext as SearchHintHeader;
      if (searchHint == null)
        return;
      FramePageUtils.CurrentPage.OpenCloseMenu(false, (Action) (() =>
      {
        base.UpdateLayout();
        if (searchHint.IsCommunityType)
          Navigator.Current.NavigateToGroup(searchHint.Id, searchHint.Title, false);
        else if (searchHint.IsUserType)
          Navigator.Current.NavigateToUserProfile(searchHint.Id, searchHint.Title, "", false);
        else if (searchHint.IsExtendedSearchType)
        {
          Navigator.Current.NavigateToUsersSearch(SearchHintsUC._viewModel.SearchString);
        }
        else
        {
          if (!searchHint.IsInternalLinkType)
            return;
          Navigator.Current.NavigateToWebUri(searchHint.Subtitle, false, false);
        }
      }), true);
      if (SearchHintsUC._flyout == null)
        return;
      SearchHintsUC._flyout.Hide();
    }

    private void SearchBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      ((Control) this.searchResultsListBox).Focus();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/SearchHintsUC.xaml", UriKind.Relative));
      this.gridRoot = (Grid) base.FindName("gridRoot");
      this.searchTextBox = (TextBox) base.FindName("searchTextBox");
      this.textBlockWatermarkText = (TextBlock) base.FindName("textBlockWatermarkText");
      this.searchResultsListBox = (ExtendedLongListSelector) base.FindName("searchResultsListBox");
    }
  }
}
