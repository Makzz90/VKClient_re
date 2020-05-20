using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VKClient.Common.Library.Posts
{
  public class DocumentsViewModel : ViewModelBase
  {
    private int _currentSectionIndex;

    public long OwnerId { get; set; }

    public ObservableCollection<DocumentsSectionViewModel> Sections { get; set; }

    public DocumentsSectionViewModel CurrentSection
    {
      get
      {
        return this.Sections[this._currentSectionIndex];
      }
      set
      {
        this._currentSectionIndex = this.Sections.IndexOf(value);
        this.NotifyPropertyChanged<DocumentsSectionViewModel>((Expression<Func<DocumentsSectionViewModel>>) (() => this.CurrentSection));
        this.LoadSection(this._currentSectionIndex);
      }
    }

    public DocumentsViewModel(long ownerId)
    {
      this.OwnerId = ownerId;
      this.Sections = new ObservableCollection<DocumentsSectionViewModel>();
    }

    public void LoadSection(int i)
    {
      this.Sections[i].Items.LoadData(false, false, (Action<BackendResult<DocumentsInfo, ResultCode>>) null, false);
    }

    public async void UploadDocument(StorageFile file)
    {
      Stream stream = ((IInputStream) await file.OpenAsync((FileAccessMode) 0)).AsStreamForRead();
      Action<BackendResult<Doc, ResultCode>> callback = (Action<BackendResult<Doc, ResultCode>>) (result =>
      {
        this.SetInProgress(false, "");
        if (result.ResultCode == ResultCode.Succeeded)
          Execute.ExecuteOnUIThread((Action) (() => EventAggregator.Current.Publish((object) new DocumentUploadedEvent()
          {
            OwnerId = this.OwnerId,
            Document = result.ResultData
          })));
        else
          ExtendedMessageBox.ShowSafe(CommonResources.Error_Generic, CommonResources.Error);
      });
      this.SetInProgress(true, "");
      DocumentsService.Current.UploadDocument(this.OwnerId >= 0L ? 0L : -this.OwnerId, file.Name, file.FileType, stream, callback, (Action<double>) null, (Cancellation) null);
    }

    public void DeleteDocument(DocumentHeader item)
    {
      DocumentsService.Current.Delete(item.Document.owner_id, item.Document.id, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>) (result => {}));
      EventAggregator.Current.Publish((object) new DocumentDeletedEvent()
      {
        OwnerId = item.Document.owner_id,
        Id = item.Document.id
      });
    }
  }
}
