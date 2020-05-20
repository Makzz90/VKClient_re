using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class PollVotersResponse
  {
    public long answer_id { get; set; }

    public VKList<User> users { get; set; }
  }
}
