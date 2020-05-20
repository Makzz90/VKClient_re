namespace VKClient.Audio.Base.Events
{
  public class BalanceTopupEvent : StatEventBase
  {
      public BalanceTopupSource Source { get; private set; }

      public BalanceTopupAction Action { get; private set; }

    public BalanceTopupEvent(BalanceTopupSource source, BalanceTopupAction action)
    {
      this.Source = source;
      this.Action = action;
    }
  }
}
