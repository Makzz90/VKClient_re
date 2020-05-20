using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class PollAnswerHeader : ViewModelBase
  {
    private readonly PollViewModel _pollVM;
    private int _maxVotes;

    public Answer Answer { get; private set; }

    public string AnswerStr
    {
      get
      {
        return this.Answer.text ?? "";
      }
    }

    public double PercentageValue
    {
      get
      {
        if (!this._pollVM.Voted)
          return 0.0;
        return this.GetAbsoluteRate();
      }
    }

    private double PercentageRelativeValue
    {
      get
      {
        if (!this._pollVM.Voted)
          return 0.0;
        return this.GetRelativeRate();
      }
    }

    public string PercentageStr
    {
      get
      {
        if (!this._pollVM.Voted)
          return "";
        return this.PercentageRelativeValue.ToString("0") + " %";
      }
    }

    public Visibility VotedCheckVisibility
    {
      get
      {
        if (!this._pollVM.Voted || this._pollVM.AnswerId != this.Answer.id)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public double Tilt
    {
      get
      {
        return this._pollVM.Voted && this.Answer.votes <= 0 ? 0.0 : 1.5;
      }
    }

    public PollAnswerHeader(PollViewModel pollVM, Answer answer)
    {
      this._pollVM = pollVM;
      this.Answer = answer;
    }

    private double GetAbsoluteRate()
    {
      List<Answer> answers = this._pollVM.Poll.answers;
      this._maxVotes = answers[0].votes;
      this._maxVotes = Enumerable.Max((IEnumerable<int>)Enumerable.Concat<int>(Enumerable.Select<Answer, int>(answers, (Func<Answer, int>)(a => a.votes)), new int[1]
      {
        (int) this._maxVotes
      }));
      if (this._maxVotes == 0)
        return 0.0;
      return (double) this.Answer.votes * 100.0 / (double) this._maxVotes;
    }

    private double GetRelativeRate()
    {
      int votes = this._pollVM.Poll.votes;
      if (votes == 0)
        return 0.0;
      return (double) this.Answer.votes * 100.0 / (double) votes;
    }

    public void ReadData()
    {
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.AnswerStr);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<double>(() => this.PercentageValue);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.PercentageStr);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.VotedCheckVisibility);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<double>(() => this.Tilt);
    }
  }
}
