namespace VKClient.Audio.Base.Events
{
  public class DiscoverActionEvent : StatEventBase
  {
    public DiscoverActionType ActionType { get; set; }

    public string ActionParam { get; set; }
  }
}
