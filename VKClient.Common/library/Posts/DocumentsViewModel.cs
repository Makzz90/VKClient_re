using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VKClient.Common.Library.Posts
{
  public class DocumentsViewModel : ViewModelBase
  {
    private int _currentSectionIndex;

    public long OwnerId { get;set; }

    public ObservableCollection<DocumentsSectionViewModel> Sections { get;set; }

    public DocumentsSectionViewModel CurrentSection
    {
      get
      {
        return this.Sections[this._currentSectionIndex];
      }
      set
      {
        this._currentSectionIndex = this.Sections.IndexOf(value);
        base.NotifyPropertyChanged<DocumentsSectionViewModel>(() => this.CurrentSection);
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
      this.Sections[i].Items.LoadData(false, false,  null, false);
    }

    public void UploadDocuments(List<StorageFile> files)
		{
			base.SetInProgress(true, "");
			int i0 = 0;
			Action<BackendResult<Doc, ResultCode>> callback = null;
			callback = delegate(BackendResult<Doc, ResultCode> result)
			{
				if (result.ResultCode == ResultCode.Succeeded)
				{
					int i;
					if (i0 == files.Count - 1)
					{
						this.SetInProgress(false, "");
					}
					Execute.ExecuteOnUIThread(delegate
					{
						DocumentUploadedEvent message = new DocumentUploadedEvent
						{
							OwnerId = this.OwnerId,
							Document = result.ResultData
						};
						EventAggregator.Current.Publish(message);
					});
					i = i0;
					i0++;
					this.UploadFile(files[i], callback);
					return;
				}
				this.SetInProgress(false, "");
				ExtendedMessageBox.ShowSafe(CommonResources.Error_Generic, CommonResources.Error);
			};
            this.UploadFile(files[i0], callback);
		}

    private async void UploadFile(StorageFile file, Action<BackendResult<Doc, ResultCode>> callback)
    {
        Stream stream = (await file.OpenAsync(0)).AsStreamForRead();
        DocumentsService.Current.UploadDocument((this.OwnerId >= 0L) ? 0L : (-this.OwnerId), file.Name, file.FileType, stream, callback, null, null);
    }

    public void DeleteDocument(DocumentHeader item)
    {
      ObservableCollection<DocumentHeader> collection = this.CurrentSection.Items.Collection;
      DocumentHeader documentHeader = ((IEnumerable<DocumentHeader>) collection).FirstOrDefault<DocumentHeader>();
      if (documentHeader == item && collection.Count > 1)
        documentHeader = collection[1];
      if (documentHeader == null)
        return;
      DocumentEditedOrDeletedEvent editedOrDeletedEvent1 = new DocumentEditedOrDeletedEvent();
      editedOrDeletedEvent1.OwnerId = item.Document.owner_id;
      editedOrDeletedEvent1.Id = item.Document.id;
      editedOrDeletedEvent1.Title = documentHeader.Name;
      editedOrDeletedEvent1.SizeString = documentHeader.GetSizeString();
      int num1 = 0;
      editedOrDeletedEvent1.IsEdited = num1 != 0;
      int num2 = this.CurrentSection.Items.TotalCount - 1;
      editedOrDeletedEvent1.NewDocumentsCount = num2;
      string str = string.Format("doc{0}_{1}", documentHeader.Document.owner_id, documentHeader.Document.id);
      editedOrDeletedEvent1.NewFirstDocumentId = str;
      DocumentEditedOrDeletedEvent editedOrDeletedEvent2 = editedOrDeletedEvent1;
      DocumentsService.Current.Delete(item.Document.owner_id, item.Document.id, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>) (result => {}));
      EventAggregator.Current.Publish(editedOrDeletedEvent2);
    }
  }
}
