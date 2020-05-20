using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class SaveProfileInfoResult
  {
    public int chaned { get; set; }

    public NameChangeRequest name_request { get; set; }
  }
}
