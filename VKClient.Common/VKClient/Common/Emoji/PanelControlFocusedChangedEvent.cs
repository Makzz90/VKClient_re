namespace VKClient.Common.Emoji
{
  public class PanelControlFocusedChangedEvent
  {
      public bool IsFocused { get; set; }

    public PanelControlFocusedChangedEvent(bool isFocused)
    {
      this.IsFocused = isFocused;
    }
  }
}
