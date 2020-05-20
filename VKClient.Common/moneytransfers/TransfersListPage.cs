using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.MoneyTransfers.Library;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.MoneyTransfers
{
  public class TransfersListPage : PageBase
  {
    private TransfersListViewModel _viewModel;
    private bool _isInitialized;
    internal GenericHeaderUC header;
    internal Pivot pivot;
    internal ExtendedLongListSelector inboxList;
    internal ExtendedLongListSelector outboxList;
    internal PullToRefreshUC pullToRefresh;
    private bool _contentLoaded;

    public TransfersListPage()
    {
      this.InitializeComponent();
      if (AppGlobalStateManager.Current.GlobalState.CanSendMoneyTransfers)
      {
        this.ApplicationBar = ((IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
        ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
        Uri uri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
        applicationBarIconButton1.IconUri = uri;
        string conversationAppBarSend = CommonResources.Conversation_AppBar_Send;
        applicationBarIconButton1.Text = conversationAppBarSend;
        ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
        applicationBarIconButton2.Click+=(new EventHandler(TransfersListPage.AppBarAddButton_OnClicked));
        this.ApplicationBar.Buttons.Add(applicationBarIconButton2);
      }
      this.header.OnHeaderTap += (Action) (() =>
      {
        if (this.pivot.SelectedIndex == 0)
          this.inboxList.ScrollToTop();
        else
          this.outboxList.ScrollToTop();
      });
      this.inboxList.OnRefresh = (Action) (() => this._viewModel.Inbox.LoadData(true, false,  null, false));
      this.outboxList.OnRefresh = (Action) (() => this._viewModel.Outbox.LoadData(true, false,  null, false));
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.inboxList);
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.outboxList);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._viewModel = new TransfersListViewModel();
        base.DataContext = this._viewModel;
        this._viewModel.Inbox.LoadData(false, false,  null, false);
        this._isInitialized = true;
      }
      else
      {
        if (!ParametersRepository.Contains("SelectedUsers"))
          return;
        User targetUser = Enumerable.First<User>((List<User>)ParametersRepository.GetParameterForIdAndReset("SelectedUsers"));
        this.SkipNextNavigationParametersRepositoryClearing = true;
        Navigator.Current.NavigateToSendMoneyPage(targetUser.id, targetUser, 0, "");
      }
    }

    private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      if (this.pivot.SelectedIndex == 0)
        this._viewModel.Inbox.LoadMoreIfNeeded(e.ContentPresenter.Content);
      else
        this._viewModel.Outbox.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.pivot.SelectedIndex != 1)
        return;
      this._viewModel.Outbox.LoadData(false, false,  null, false);
    }

    private void Transfer_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      MoneyTransferViewModel dataContext = ((FrameworkElement) ((RoutedEventArgs) e).OriginalSource).DataContext as MoneyTransferViewModel;
      MoneyTransfer moneyTransfer = dataContext != null ? dataContext.Transfer :  null;
      if (moneyTransfer == null)
        return;
      TransferCardView.Show(moneyTransfer.id, moneyTransfer.from_id, moneyTransfer.to_id);
    }

    private void Photo_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      MoneyTransferViewModel dataContext = ((FrameworkElement) ((RoutedEventArgs) e).OriginalSource).DataContext as MoneyTransferViewModel;
      if (dataContext == null)
        return;
      if (dataContext.UserId > 0L)
        Navigator.Current.NavigateToUserProfile(dataContext.UserId, "", "", false);
      else
        Navigator.Current.NavigateToGroup(-dataContext.UserId, "", false);
    }

    private static void AppBarAddButton_OnClicked(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToPickUser(false, 0, true, 0, PickUserMode.PickWithSearch, CommonResources.MoneyTransfers_ChooseRecipient, 0, false);
    }

    private void Button_OnClicked(object sender, RoutedEventArgs e)
    {
      Button button = (Button) sender;
      int num = int.Parse(((FrameworkElement) button).Tag.ToString());
      MoneyTransferViewModel dataContext = ((FrameworkElement) button).DataContext as MoneyTransferViewModel;
      if (dataContext == null)
        return;
      if (num == 1)
        dataContext.AcceptTransfer();
      else if (!dataContext.IsOutbox)
        dataContext.DeclineTransfer( null);
      else
        dataContext.CancelTransfer( null);
    }

    private void Button_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/MoneyTransfers/TransfersListPage.xaml", UriKind.Relative));
      this.header = (GenericHeaderUC) base.FindName("header");
      this.pivot = (Pivot) base.FindName("pivot");
      this.inboxList = (ExtendedLongListSelector) base.FindName("inboxList");
      this.outboxList = (ExtendedLongListSelector) base.FindName("outboxList");
      this.pullToRefresh = (PullToRefreshUC) base.FindName("pullToRefresh");
    }
  }
}
