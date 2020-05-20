using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
      EventAggregator.Current.Subscribe(this);
    }

    public override void Load(Action<ResultCode> callback)
    {
      StoreService.Instance.GetBalanceData((Action<BackendResult<BalanceData, ResultCode>>) (result =>
      {
        ResultCode resultCode = result.ResultCode;
        if (resultCode == ResultCode.Succeeded)
        {
          this.ProcessData(result.ResultData, (Action) (() =>
          {
            Action<ResultCode> action = callback;
            if (action != null)
            {
              int num = (int) resultCode;
              action((ResultCode) num);
            }
            this.LoadedCallback();
          }));
        }
        else
        {
          Action<ResultCode> action = callback;
          if (action != null)
          {
            int num = (int) resultCode;
            action((ResultCode) num);
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
        List<VotesPack> m0List;
        if (balanceData2 == null)
        {
          m0List =  null;
        }
        else
        {
          List<VotesPack> votesPacks = balanceData2.VotesPacks;
          m0List = votesPacks != null ? Enumerable.ToList<VotesPack>(Enumerable.OrderBy<VotesPack, int>(votesPacks, (Func<VotesPack, int>)(votesPack => votesPack.VotesCount))) : null;
        }
        List<VotesPack> votesPackList = m0List;
        if (votesPackList != null)
        {
          for (int index = 0; index < votesPackList.Count; ++index)
          {
            VotesPack votesPack = votesPackList[index];
            bool flag = Enumerable.Any<InAppUnfulfilledProduct>(unfulfilledProducts, (Func<InAppUnfulfilledProduct, bool>)(product => product.ProductId == votesPack.MerchantProductId));
            VotesPackViewModel votesPackViewModel = new VotesPackViewModel(votesPack);
            votesPackViewModel.IconCoinId = index + 1;
            int num = !flag ? 1 : 0;
            votesPackViewModel.CanPurchase = num != 0;
            this.VotesPacks.Add(votesPackViewModel);
          }
        }
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          // ISSUE: type reference
          // ISSUE: method reference
          this.NotifyPropertyChanged<int>(() => this.Balance);
          // ISSUE: type reference
          // ISSUE: method reference
          this.NotifyPropertyChanged<List<VotesPackViewModel>>(()=>this.VotesPacks);
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
        InAppPurchaseService.LoadUnfulfilledConsumables( null, (Action<List<InAppUnfulfilledProduct>>) (unfulfilledProducts =>
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
      this.ProcessData(message.BalanceData,  null);
    }
  }
}
