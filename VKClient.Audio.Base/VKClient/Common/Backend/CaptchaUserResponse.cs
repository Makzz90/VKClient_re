namespace VKClient.Common.Backend
{
  public class CaptchaUserResponse
  {
    public CaptchaUserRequest Request { get; set; }

    public bool IsCancelled { get; set; }

    public string EnteredString { get; set; }
  }
}
