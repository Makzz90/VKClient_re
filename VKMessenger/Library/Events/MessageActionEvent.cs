namespace VKMessenger.Library.Events
{
  public class MessageActionEvent
  {
    public MessageActionType MessageActionType { get; set; }

    public MessageViewModel Message { get; set; }
  }
}
