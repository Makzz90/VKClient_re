using VKClient.Common.Library.Games;

namespace VKClient.Common.UC
{
  public class GameInvitationHiddenEvent
  {
    public GameRequestHeader Invitation { get; private set; }

    public GameInvitationHiddenEvent(GameRequestHeader invitation)
    {
      this.Invitation = invitation;
    }
  }
}
