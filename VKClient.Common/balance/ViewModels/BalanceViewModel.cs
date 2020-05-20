using System;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Balance.ViewModels
{
  public class BalanceViewModel : BalanceRefillViewModel
  {
    public string BalanceStr
    {
      get
      {
        int balance = this.Balance;
        if (balance == 0)
          return CommonResources.NoVotes;
        return UIStringFormatterHelper.FormatNumberOfSomething(balance, CommonResources.OneVoteFrm, CommonResources.TwoFourVotesFrm, CommonResources.FiveVotesFrm, true,  null, false);
      }
    }

    private void NotifyProperties()
    {
        Execute.ExecuteOnUIThread(delegate
        {
            base.NotifyPropertyChanged<string>(() => this.BalanceStr);
        });
    }

    protected override void LoadedCallback()
    {
      this.NotifyProperties();
    }

    public override void Handle(BalanceUpdatedEvent message)
    {
      base.Handle(message);
      this.NotifyProperties();
    }
  }
}
