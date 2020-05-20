namespace VKClient.Common.Library.FriendsImport.Gmail
{
  public class GmailAccessTokenResponse
  {
    public string access_token { get; set; }

    public string id_token { get; set; }

    public long expires_in { get; set; }

    public string token_type { get; set; }

    public string refresh_token { get; set; }
  }
}
