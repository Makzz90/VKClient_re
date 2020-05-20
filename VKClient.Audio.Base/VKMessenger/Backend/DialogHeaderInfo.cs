namespace VKMessenger.Backend
{
  public class DialogHeaderInfo
  {
    public int unread { get; set; }

    public Message message { get; set; }

    public DialogHeaderInfo()
    {
      this.message = new Message();
    }
  }
}
