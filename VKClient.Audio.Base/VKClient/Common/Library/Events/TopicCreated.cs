namespace VKClient.Common.Library.Events
{
  public class TopicCreated
  {
    public long tid { get; set; }

    public long gid { get; set; }

    public string title { get; set; }

    public string text { get; set; }
  }
}
