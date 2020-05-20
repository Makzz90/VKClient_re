using System;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Balance.ViewModels
{
  public class BalanceRefillPopupViewModel : BalanceRefillViewModel
  {
    public string BalanceDesc
    {
      get
      {
        return UIStringFormatterHelper.FormatNumberOfSomething(this.Balance, CommonResources.OneVoteFrm, CommonResources.TwoFourVotesFrm, CommonResources.FiveVotesFrm, false,  null, true);
      }
    }

    private void NotifyProperties()
    {
        Execute.ExecuteOnUIThread(delegate
        {
            base.NotifyPropertyChanged<string>(() => this.BalanceDesc);
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
