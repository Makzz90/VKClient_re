using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class PollAnswerHeader : ViewModelBase
  {
    private readonly PollViewModel _pollVM;
    private int _maxVotes;

    public Answer Answer { get; set; }

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
        return !this._pollVM.Voted || this._pollVM.AnswerId != this.Answer.id ? Visibility.Collapsed : Visibility.Visible;
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
      this._maxVotes = answers.Select<Answer, int>((Func<Answer, int>) (a => a.votes)).Concat<int>((IEnumerable<int>) new int[1]{ this._maxVotes }).Max();
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
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.AnswerStr));
      this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.PercentageValue));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.PercentageStr));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.VotedCheckVisibility));
      this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.Tilt));
    }
  }
}
