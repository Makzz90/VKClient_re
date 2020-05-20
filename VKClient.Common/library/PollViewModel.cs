using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class PollViewModel : ViewModelBase, IHandle<PollVotedUnvoted>, IHandle
  {
    private readonly long _topicId;
    public bool IsVoting;

    public Poll Poll { get; private set; }

    public bool Voted
    {
      get
      {
        return (ulong) this.Poll.answer_id > 0UL;
      }
    }

    public long AnswerId
    {
      get
      {
        return this.Poll.answer_id;
      }
    }

    public string Question
    {
      get
      {
        return this.Poll.question ?? "";
      }
    }

    public string VotedCountStr
    {
      get
      {
        int votes = this.Poll.votes;
        if (votes == 0)
          return string.Format(CommonResources.Poll_FiveVotesFrm, "0");
        if (votes < 1000)
          return UIStringFormatterHelper.FormatNumberOfSomething(votes, CommonResources.Poll_OneVoteFrm, CommonResources.Poll_TwoFourVotesFrm, CommonResources.Poll_FiveVotesFrm, true,  null, false);
        string str = UIStringFormatterHelper.FormatNumberOfSomething(5, CommonResources.Poll_OneVoteFrm, CommonResources.Poll_TwoFourVotesFrm, CommonResources.Poll_FiveVotesFrm, false,  null, false);
        return string.Format("{0} {1}", UIStringFormatterHelper.FormatForUIVeryShort((long) votes), str);
      }
    }

    public string PollTypeStr
    {
      get
      {
        if (this.Poll.anonymous != 0)
          return CommonResources.Poll_AnonymousPoll;
        return CommonResources.Poll_PublicPoll;
      }
    }

    public string Description
    {
      get
      {
        return string.Format("{0} · {1}", this.PollTypeStr, this.VotedCountStr);
      }
    }

    public List<PollAnswerHeader> Answers { get; private set; }

    public PollViewModel(Poll poll, long topicId = 0)
    {
      this.Poll = poll;
      this._topicId = topicId;
      this.Answers = new List<PollAnswerHeader>((IEnumerable<PollAnswerHeader>)Enumerable.Select<Answer, PollAnswerHeader>(poll.answers, (Func<Answer, PollAnswerHeader>)(answer => new PollAnswerHeader(this, answer))));
      EventAggregator.Current.Subscribe(this);
    }

    public void VoteUnvote(PollAnswerHeader answerHeader)
    {
      if (this.IsVoting)
        return;
      this.IsVoting = true;
      if (!this.Voted)
      {
        this.Poll.answer_id = answerHeader.Answer.id;
        ++this.Poll.votes;
        ++answerHeader.Answer.votes;
        this.DoVote(this.Poll.answer_id);
      }
      else if (this.Poll.answer_id == answerHeader.Answer.id)
      {
        this.Poll.answer_id = 0L;
        --this.Poll.votes;
        --answerHeader.Answer.votes;
        this.DoUnvote(answerHeader.Answer.id);
      }
      else
      {
        long currentAnswer = this.Poll.answer_id;
        this.Poll.answer_id = answerHeader.Answer.id;
        ++answerHeader.Answer.votes;
        PollAnswerHeader pollAnswerHeader = (PollAnswerHeader)Enumerable.FirstOrDefault<PollAnswerHeader>(this.Answers, (Func<PollAnswerHeader, bool>)(ah => ah.Answer.id == currentAnswer));
        if (pollAnswerHeader != null)
          --pollAnswerHeader.Answer.votes;
        this.DoUnvoteVote(currentAnswer, this.Poll.answer_id);
      }
      this.NotifyUpdates();
      EventAggregator.Current.Publish(new PollVotedUnvoted(this.Poll, answerHeader));
    }

    private void DoUnvoteVote(long currentAnswerId, long newAnswerId)
    {
      PollService.Current.AddRemoveVote(false, this.Poll.owner_id, this.Poll.poll_id, currentAnswerId, (Action<BackendResult<long, ResultCode>>) (res => PollService.Current.AddRemoveVote(true, this.Poll.owner_id, this.Poll.poll_id, newAnswerId, (Action<BackendResult<long, ResultCode>>) (result => this.IsVoting = false), this._topicId)), this._topicId);
    }

    private void DoUnvote(long answerId)
    {
      PollService.Current.AddRemoveVote(false, this.Poll.owner_id, this.Poll.poll_id, answerId, (Action<BackendResult<long, ResultCode>>) (res => this.IsVoting = false), this._topicId);
    }

    private void DoVote(long answerId)
    {
      PollService.Current.AddRemoveVote(true, this.Poll.owner_id, this.Poll.poll_id, answerId, (Action<BackendResult<long, ResultCode>>) (res => this.IsVoting = false), this._topicId);
    }

    private void NotifyUpdates()
    {
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<bool>(() => this.Voted);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.VotedCountStr);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.Description);
      List<PollAnswerHeader>.Enumerator enumerator = this.Answers.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
          enumerator.Current.ReadData();
      }
      finally
      {
        enumerator.Dispose();
      }
    }

    public void Handle(PollVotedUnvoted message)
    {
      if (message.Poll.id != this.Poll.id || message.Poll.owner_id != this.Poll.owner_id)
        return;
      PollAnswerHeader pollAnswerHeader1 = (PollAnswerHeader)Enumerable.FirstOrDefault<PollAnswerHeader>(this.Answers, (Func<PollAnswerHeader, bool>)(a => a.Answer.id == message.AnswerHeader.Answer.id));
      if (pollAnswerHeader1 == null)
        return;
      if (!this.Voted)
      {
        this.Poll.answer_id = pollAnswerHeader1.Answer.id;
        ++this.Poll.votes;
        ++pollAnswerHeader1.Answer.votes;
      }
      else if (this.Poll.answer_id == pollAnswerHeader1.Answer.id)
      {
        this.Poll.answer_id = 0L;
        --this.Poll.votes;
        --pollAnswerHeader1.Answer.votes;
      }
      else
      {
        long currentAnswer = this.Poll.answer_id;
        this.Poll.answer_id = pollAnswerHeader1.Answer.id;
        ++pollAnswerHeader1.Answer.votes;
        PollAnswerHeader pollAnswerHeader2 = (PollAnswerHeader)Enumerable.FirstOrDefault<PollAnswerHeader>(this.Answers, (Func<PollAnswerHeader, bool>)(ah => ah.Answer.id == currentAnswer));
        if (pollAnswerHeader2 != null)
          --pollAnswerHeader2.Answer.votes;
      }
      this.NotifyUpdates();
    }
  }
}
