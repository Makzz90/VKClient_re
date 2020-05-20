namespace VKClient.Audio.Base.Events
{
  public class ViewBlockEvent : StatEventBase
  {
    public int Position { get; set; }

    public string ItemType { get; set; }
  }
}
