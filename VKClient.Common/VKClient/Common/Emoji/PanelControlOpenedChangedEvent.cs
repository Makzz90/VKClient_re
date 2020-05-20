namespace VKClient.Common.Emoji
{
  public class PanelControlOpenedChangedEvent
  {
      public bool IsOpened { get; set; }

    public PanelControlOpenedChangedEvent(bool isOpened)
    {
      this.IsOpened = isOpened;
    }
  }
}
