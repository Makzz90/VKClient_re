namespace VKClient.Common.Backend.DataObjects
{
  public class CaptchaError
  {
    public string error { get; set; }

    public string captcha_sid { get; set; }

    public string captcha_img { get; set; }
  }
}
