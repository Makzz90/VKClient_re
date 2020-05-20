namespace VKClient.Common.Emoji
{
  public class PanelControlOpenedChangedEvent
  {
      public bool IsOpened { get; private set; }

    public PanelControlOpenedChangedEvent(bool isOpened)
    {
      this.IsOpened = isOpened;
    }
  }
}
