using Newtonsoft.Json;

namespace VKClient.Audio.Base.Events
{
  public class StatEventBase
  {
    public bool ShouldSendImmediately { get; set; }

    public override string ToString()
    {
      return JsonConvert.SerializeObject(this);
    }
  }
}
