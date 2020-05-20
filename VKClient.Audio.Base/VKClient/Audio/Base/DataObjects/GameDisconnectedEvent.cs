namespace VKClient.Audio.Base.DataObjects
{
  public class GameDisconnectedEvent
  {
    public long GameId { get; private set; }

    public GameDisconnectedEvent(long gameId)
    {
      this.GameId = gameId;
    }
  }
}
