namespace VKClient.Common.Backend.DataObjects
{
  public class AccountBaseInfoSettingsEntry
  {
    public const string GIF_AUTOPLAY_KEY = "gif_autoplay";
    public const string PAYMENT_TYPE_KEY = "payment_type";
    public const string MONEY_P2P_KEY = "money_p2p";
    public const string MONEY_P2P_GROUPS_KEY = "money_clubs_p2p";

    public string name { get; set; }

    public bool available { get; set; }

    public bool forced { get; set; }

    public string value { get; set; }
  }
}
