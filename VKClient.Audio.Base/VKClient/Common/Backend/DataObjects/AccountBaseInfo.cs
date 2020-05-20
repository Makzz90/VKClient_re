using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public sealed class AccountBaseInfo
  {
    public string support_url { get; set; }

    public MoneyTransfersSettings money_p2p_params { get; set; }

    public List<AccountBaseInfoSettingsEntry> settings { get; set; }
  }
}
