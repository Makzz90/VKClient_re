namespace VKClient.Audio.Base.Events
{
  public class PostActionEvent : StatEventBase
  {
    public string PostId { get; set; }

    public PostActionType ActionType { get; set; }
  }
}
