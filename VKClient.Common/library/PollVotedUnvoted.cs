using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public class PollVotedUnvoted
  {
      public Poll Poll { get; private set; }

      public PollAnswerHeader AnswerHeader { get; private set; }

    public PollVotedUnvoted(Poll poll, PollAnswerHeader answerHeader)
    {
      this.Poll = poll;
      this.AnswerHeader = answerHeader;
    }
  }
}
