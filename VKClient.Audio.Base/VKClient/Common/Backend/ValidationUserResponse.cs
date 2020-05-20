namespace VKClient.Common.Backend
{
  public class ValidationUserResponse
  {
    public bool IsSucceeded { get; set; }

    public long user_id { get; set; }

    public string access_token { get; set; }

    public string phone { get; set; }

    public string phone_status { get; set; }

    public string email { get; set; }

    public string email_status { get; set; }
  }
}
