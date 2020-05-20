using VKClient.Common.Library.Games;

namespace VKClient.Common.UC
{
  public class GameRequestReadEvent
  {
    public GameRequestHeader GameRequestHeader { get; private set; }

    public GameRequestReadEvent(GameRequestHeader gameRequestHeader)
    {
      this.GameRequestHeader = gameRequestHeader;
    }
  }
}
