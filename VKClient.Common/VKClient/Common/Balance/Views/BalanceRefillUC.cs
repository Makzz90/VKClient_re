using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Events;
using VKClient.Common.Balance.ViewModels;
using VKClient.Common.Library;
using VKClient.Common.Localization;

namespace VKClient.Common.Balance.Views
{
  public class BalanceRefillUC : UserControl
  {
    public static readonly DependencyProperty BalanceTopupProperty = DependencyProperty.Register("BalanceTopup", typeof (BalanceTopupSource), typeof (BalanceRefillUC), new PropertyMetadata((object) BalanceTopupSource.settings));
    private bool _contentLoaded;

    public BalanceTopupSource BalanceTopup
    {
      get
      {
        return (BalanceTopupSource) this.GetValue(BalanceRefillUC.BalanceTopupProperty);
      }
      set
      {
        this.SetValue(BalanceRefillUC.BalanceTopupProperty, (object) value);
      }
    }

    public BalanceRefillUC()
    {
      this.InitializeComponent();
    }

    private void ButtonBuy_OnTap(object sender, GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      VotesPackViewModel viewModel = (frameworkElement != null ? frameworkElement.DataContext : null) as VotesPackViewModel;
      if (viewModel == null)
        return;
      StorePurchaseManager.BuyVotesPack(viewModel.VotesPack, this.BalanceTopup, null, (Action) (() => viewModel.CanPurchase = false));
    }

    private void Error_OnTap(object sender, GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      VotesPackViewModel votesPackViewModel = (frameworkElement != null ? frameworkElement.DataContext : null) as VotesPackViewModel;
      if (votesPackViewModel == null || MessageBox.Show(CommonResources.PurchaseIncompleteMessage, CommonResources.PurchaseIncompleteTitle, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      StorePurchaseManager.RestorePurchases(votesPackViewModel.VotesPack.MerchantProductId);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Balance/Views/BalanceRefillUC.xaml", UriKind.Relative));
    }
  }
}
