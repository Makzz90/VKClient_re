using System;
using System.Linq.Expressions;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class PollOption : ViewModelBase, IRemovableWithText
  {
    private CreateEditPollViewModel _parent;
    private string _text;

    public string Text
    {
      get
      {
        return this._text;
      }
      set
      {
        this._text = value;
        this.NotifyPropertyChanged<string>((Expression<Func<string>>) (() => this.Text));
        this._parent.NotifyCanSaveChanged();
        this.IsChanged = true;
      }
    }

    public bool IsChanged { get; set; }

    public long AnswerId { get; set; }

    public PollOption(CreateEditPollViewModel parent)
    {
      this._parent = parent;
    }

    public void Remove()
    {
      this._parent.RemovePollOption(this);
    }
  }
}
