using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public class CreateEditPollViewModel : ViewModelBase
  {
    private static readonly int MIN_POLL_OPTIONS_COUNT = 2;
    private ObservableCollection<PollOption> _pollOptions = new ObservableCollection<PollOption>();
    private CreateEditPollViewModel.Mode _mode;
    private Poll _initialPoll;
    private long _ownerId;
    private long _pollId;
    private string _question;
    private bool _isAnonymous;

    public CreateEditPollViewModel.Mode CurrentMode
    {
      get
      {
        return this._mode;
      }
    }

    public string Question
    {
      get
      {
        return this._question;
      }
      set
      {
        this._question = value;
        base.NotifyPropertyChanged<string>(() => this.Question);
				base.NotifyPropertyChanged<bool>(() => this.CanSave);
			}
    }

    public string Title
    {
      get
      {
        if (this._mode != CreateEditPollViewModel.Mode.Create)
          return CommonResources.Poll_EditPoll;
        return CommonResources.Poll_NewPoll;
      }
    }

    public bool IsAnonymous
    {
      get
      {
        return this._isAnonymous;
      }
      set
      {
        this._isAnonymous = value;
        base.NotifyPropertyChanged<bool>(() => this.IsAnonymous);
			}
    }

    public bool CanChangeIsAnonymous
    {
      get
      {
        return this._mode == CreateEditPollViewModel.Mode.Create;
      }
    }

    public ObservableCollection<PollOption> PollOptions
    {
      get
      {
        return this._pollOptions;
      }
    }

    public IEnumerable<PollOption> ValidPollOptions
    {
      get
      {
        return this._pollOptions.Where<PollOption>((Func<PollOption, bool>) (p => !string.IsNullOrWhiteSpace(p.Text)));
      }
    }

    public bool CanAdd
    {
      get
      {
        return ((Collection<PollOption>) this.PollOptions).Count < 10;
      }
    }

    public bool CanSave
    {
      get
      {
        if (!this.IsInProgress && !string.IsNullOrWhiteSpace(this.Question))
          return this.PollOptions.Count<PollOption>((Func<PollOption, bool>) (p => !string.IsNullOrWhiteSpace(p.Text))) >= CreateEditPollViewModel.MIN_POLL_OPTIONS_COUNT;
        return false;
      }
    }

    protected CreateEditPollViewModel()
    {
    }

    public void AddPollOption()
    {
      if (!this.CanAdd)
        return;
      this.PollOptions.Add(new PollOption(this));
      this.NotifyPropertyChanged<bool>((() => this.CanAdd));
    }

    public void RemovePollOption(PollOption pollOption)
    {
      this.PollOptions.Remove(pollOption);
      this.NotifyPropertyChanged<bool>((() => this.CanAdd));
      this.NotifyPropertyChanged<bool>((() => this.CanSave));
    }

    public void NotifyCanSaveChanged()
    {
      this.NotifyPropertyChanged<bool>((() => this.CanSave));
    }

    public static CreateEditPollViewModel CreateForNewPoll(long ownerId)
    {
      CreateEditPollViewModel editPollViewModel = new CreateEditPollViewModel();
      editPollViewModel._mode = CreateEditPollViewModel.Mode.Create;
      editPollViewModel._ownerId = ownerId;
      editPollViewModel.InitializeFromPoll();
      return editPollViewModel;
    }

    public static CreateEditPollViewModel CreateForEditPoll(long ownerId, long pollId, Poll poll = null)
    {
      CreateEditPollViewModel editPollViewModel = new CreateEditPollViewModel();
      editPollViewModel._mode = CreateEditPollViewModel.Mode.Edit;
      editPollViewModel._ownerId = ownerId;
      editPollViewModel._pollId = pollId;
      editPollViewModel._initialPoll = poll;
      if (poll == null)
        editPollViewModel.LoadPoll();
      else
        editPollViewModel.InitializeFromPoll();
      return editPollViewModel;
    }

    private void InitializeFromPoll()
    {
      if (this._initialPoll == null)
      {
        this.Question = "";
        this.IsAnonymous = false;
        this.PollOptions.Clear();
        this.PollOptions.Add(new PollOption(this));
        this.PollOptions.Add(new PollOption(this));
      }
      else
      {
        this.Question = this._initialPoll.question;
        this.IsAnonymous = this._initialPoll.is_closed == 1;
        foreach (Answer answer in this._initialPoll.answers)
          this.PollOptions.Add(new PollOption(this)
          {
            Text = answer.text,
            AnswerId = answer.id
          });
      }
      this.NotifyPropertyChanged<bool>((() => this.CanAdd));
      this.NotifyPropertyChanged<bool>((() => this.CanSave));
    }

    private void LoadPoll()
    {
      if (this.IsInProgress)
        return;
      this.SetInProgress(true, "");
      this.NotifyPropertyChanged<bool>((() => this.CanSave));
      PollService.Current.GetById(this._ownerId, this._pollId, false, (Action<BackendResult<Poll, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.SetInProgress(false, "");
        this.NotifyPropertyChanged<bool>((() => this.CanSave));
        if (res.ResultCode == ResultCode.Succeeded)
        {
          this._initialPoll = res.ResultData;
          this.InitializeFromPoll();
        }
        else
            VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
      }))));
    }

    public void SavePoll(Action<Poll> successCallback)
    {
      if (this.IsInProgress)
        return;
      this.SetInProgress(true, "");
      this.NotifyPropertyChanged<bool>((() => this.CanSave));
      if (this._mode == CreateEditPollViewModel.Mode.Create)
      {
        PollService.Current.CreatePoll(this.Question, this.IsAnonymous, this._ownerId, this.ValidPollOptions.Select<PollOption, string>((Func<PollOption, string>) (p => p.Text)).ToList<string>(), (Action<BackendResult<Poll, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.SetInProgress(false, "");
          this.NotifyPropertyChanged<bool>((() => this.CanSave));
          if (res.ResultCode == ResultCode.Succeeded)
            successCallback(res.ResultData);
          else
              VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
        }))));
      }
      else
      {
        List<string> list1 = this.ValidPollOptions.Where<PollOption>((Func<PollOption, bool>) (p => p.AnswerId == 0L)).Select<PollOption, string>((Func<PollOption, string>) (p => p.Text)).ToList<string>();
        IEnumerable<PollOption> source = this.ValidPollOptions.Where<PollOption>((Func<PollOption, bool>) (p =>
        {
          if (p.AnswerId != 0L)
            return p.IsChanged;
          return false;
        }));
        Func<PollOption, string> func = (Func<PollOption, string>) (p => p.AnswerId.ToString());

        Func<PollOption, string> keySelector = new Func<PollOption, string>(p => p.Text);
			

        Dictionary<string, string> dictionary = source.ToDictionary<PollOption, string, string>(keySelector, (Func<PollOption, string>) (p => p.Text));
        List<long> list2 = this._initialPoll.answers.Select<Answer, long>((Func<Answer, long>) (a => a.id)).Except<long>(this.ValidPollOptions.Select<PollOption, long>((Func<PollOption, long>) (p => p.AnswerId))).ToList<long>();
        PollService.Current.EditPoll(this._ownerId, this._pollId, this.Question, list1, dictionary, list2, (Action<BackendResult<Poll, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.SetInProgress(false, "");
          this.NotifyPropertyChanged<bool>((() => this.CanSave));
          if (res.ResultCode == ResultCode.Succeeded)
            successCallback(res.ResultData);
          else
              VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null);
        }))));
      }
    }

    public enum Mode
    {
      Create,
      Edit,
    }
  }
}
