using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.Balance.ViewModels
{
  public class BalanceRefillViewModel : ViewModelStatefulBase, IHandle<BalanceUpdatedEvent>, IHandle
  {
    public int Balance { get; private set; }

    public List<VotesPackViewModel> VotesPacks { get; private set; }

    protected BalanceRefillViewModel()
    {
      EventAggregator.Current.Subscribe((object) this);
    }

    public override void Load(Action<bool> callback)
    {
      StoreService.Instance.GetBalanceData((Action<BackendResult<BalanceData, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          this.ProcessData(result.ResultData, (Action) (() =>
          {
            Action<bool> action = callback;
            if (action != null)
            {
              int num = 1;
              action(num != 0);
            }
            this.LoadedCallback();
          }));
        }
        else
        {
          Action<bool> action = callback;
          if (action != null)
          {
            int num = 0;
            action(num != 0);
          }
          this.LoadedCallback();
        }
      }));
    }

    private void ProcessData(BalanceData balanceData, Action callback = null)
    {
      BalanceRefillViewModel.LoadUnfulfilledConsumables((Action<List<InAppUnfulfilledProduct>>) (unfulfilledProducts =>
      {
        BalanceData balanceData1 = balanceData;
        this.Balance = balanceData1 != null ? balanceData1.Balance : 0;
        this.VotesPacks = new List<VotesPackViewModel>();
        BalanceData balanceData2 = balanceData;
        List<VotesPack> votesPackList1;
        if (balanceData2 == null)
        {
          votesPackList1 = (List<VotesPack>) null;
        }
        else
        {
          List<VotesPack> votesPacks = balanceData2.VotesPacks;
          votesPackList1 = votesPacks != null ? votesPacks.OrderBy<VotesPack, int>((Func<VotesPack, int>) (votesPack => votesPack.VotesCount)).ToList<VotesPack>() : (List<VotesPack>) null;
        }
        List<VotesPack> votesPackList2 = votesPackList1;
        if (votesPackList2 != null)
        {
          for (int index = 0; index < votesPackList2.Count; ++index)
          {
            VotesPack votesPack = votesPackList2[index];
            bool flag = unfulfilledProducts.Any<InAppUnfulfilledProduct>((Func<InAppUnfulfilledProduct, bool>) (product => product.ProductId == votesPack.MerchantProductId));
            VotesPackViewModel votesPackViewModel = new VotesPackViewModel(votesPack);
            votesPackViewModel.IconCoinId = index + 1;
            int num = !flag ? 1 : 0;
            votesPackViewModel.CanPurchase = num != 0;
            this.VotesPacks.Add(votesPackViewModel);
          }
        }
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.NotifyPropertyChanged<int>((Expression<Func<int>>) (() => this.Balance));
          this.NotifyPropertyChanged<List<VotesPackViewModel>>((Expression<Func<List<VotesPackViewModel>>>) (() => this.VotesPacks));
        }));
        Action action = callback;
        if (action == null)
          return;
        action();
      }));
    }

    private static void LoadUnfulfilledConsumables(Action<List<InAppUnfulfilledProduct>> callback)
    {
      if (AppGlobalStateManager.Current.GlobalState.PaymentType == AccountPaymentType.inapp)
      {
        InAppPurchaseService.LoadUnfulfilledConsumables(null, (Action<List<InAppUnfulfilledProduct>>) (unfulfilledProducts =>
        {
          Action<List<InAppUnfulfilledProduct>> action = callback;
          if (action == null)
            return;
          List<InAppUnfulfilledProduct> unfulfilledProductList = unfulfilledProducts;
          action(unfulfilledProductList);
        }));
      }
      else
      {
        Action<List<InAppUnfulfilledProduct>> action = callback;
        if (action == null)
          return;
        List<InAppUnfulfilledProduct> unfulfilledProductList = new List<InAppUnfulfilledProduct>();
        action(unfulfilledProductList);
      }
    }

    protected virtual void LoadedCallback()
    {
    }

    public virtual void Handle(BalanceUpdatedEvent message)
    {
      this.ProcessData(message.BalanceData, null);
    }
  }
}
