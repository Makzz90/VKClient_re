using System;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Audio.Base.BackendServices;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;

namespace VKClient.Common.Library
{
  public sealed class DocumentEditingViewModel : ViewModelBase
  {
    private bool _isFormEnabled = true;
    private readonly long _ownerId;
    private readonly long _id;
    private string _title;
    private string _tags;

    public bool IsFormEnabled
    {
      get
      {
        return this._isFormEnabled;
      }
      set
      {
        this._isFormEnabled = value;
        this.NotifyPropertyChanged<bool>(() => this.IsFormEnabled);
      }
    }

    public bool IsFormCompleted
    {
      get
      {
        return !string.IsNullOrWhiteSpace(this.Title);
      }
    }

    public string Title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = value;
        this.NotifyPropertyChanged<string>(() => this.Title);
        this.NotifyPropertyChanged<bool>(() => this.IsFormCompleted);
      }
    }

    public string Tags
    {
      get
      {
        return this._tags;
      }
      set
      {
        this._tags = value;
        this.NotifyPropertyChanged<string>(() => this.Tags);
      }
    }

    public DocumentEditingViewModel(long ownerId, long id, string title)
    {
      this._ownerId = ownerId;
      this._id = id;
      this.Title = title;
      this.Tags = "";
    }

    public void SaveChanges()
    {
        base.SetInProgress(true, "");
        this.IsFormEnabled = false;
        DocumentsService.Current.Edit(this._ownerId, this._id, this.Title, this.Tags, delegate(BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode> result)
        {
            Execute.ExecuteOnUIThread(delegate
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    EventAggregator.Current.Publish(new DocumentEditedOrDeletedEvent
                    {
                        OwnerId = this._ownerId,
                        Id = this._id,
                        Title = this.Title.Trim(),
                        IsEdited = true
                    });
                    Navigator.Current.GoBack();
                    return;
                }
                this.SetInProgress(false, "");
                this.IsFormEnabled = true;
                VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            });
        });
    }
  }
}
