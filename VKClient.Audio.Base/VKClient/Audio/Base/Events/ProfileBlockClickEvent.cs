namespace VKClient.Audio.Base.Events
{
  public class ProfileBlockClickEvent : StatEventBase
  {
    public long UserId { get; set; }

    public ProfileBlockType BlockType { get; set; }
  }
}
