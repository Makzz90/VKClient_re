using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public class PollVotedUnvoted
  {
      public Poll Poll { get; set; }

    public PollAnswerHeader AnswerHeader { get; set; }

    public PollVotedUnvoted(Poll poll, PollAnswerHeader answerHeader)
    {
      this.Poll = poll;
      this.AnswerHeader = answerHeader;
    }
  }
}
