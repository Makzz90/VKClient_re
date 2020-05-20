using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend
{
  public class CheckNameResponse
  {
    public int status { get; set; }

    public string reason { get; set; }

    public VKList<string> suggestions { get; set; }
  }
}
