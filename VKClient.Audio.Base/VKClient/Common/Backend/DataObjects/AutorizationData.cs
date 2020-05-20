namespace VKClient.Common.Backend.DataObjects
{
  public class AutorizationData
  {
    public string access_token { get; set; }

    public int expires_in { get; set; }

    public long user_id { get; set; }

    public string secret { get; set; }
  }
}
