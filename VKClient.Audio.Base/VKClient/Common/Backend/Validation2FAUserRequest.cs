namespace VKClient.Common.Backend
{
  public class Validation2FAUserRequest
  {
    public string username { get; set; }

    public string password { get; set; }

    public string validationType { get; set; }

    public string validationSid { get; set; }

    public string phoneMask { get; set; }
  }
}
