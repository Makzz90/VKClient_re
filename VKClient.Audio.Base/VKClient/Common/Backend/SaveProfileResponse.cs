using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class SaveProfileResponse
  {
    public int changed { get; set; }

    public NameChangeRequest name_request { get; set; }
  }
}
