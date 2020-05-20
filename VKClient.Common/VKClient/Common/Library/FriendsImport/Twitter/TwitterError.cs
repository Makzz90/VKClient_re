namespace VKClient.Common.Library.FriendsImport.Twitter
{
  public class TwitterError
  {
    public int code { get; set; }

    public string message { get; set; }

    public bool IsRateLimitExceeded()
    {
      return this.code == 88;
    }
  }
}
