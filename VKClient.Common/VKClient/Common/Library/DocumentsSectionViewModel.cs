using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public sealed class DocumentsSectionViewModel : ViewModelBase, ICollectionDataProvider<DocumentsInfo, DocumentHeader>, IHandle<DocumentUploadedEvent>, IHandle, IHandle<DocumentDeletedEvent>
  {
    private readonly DocumentsViewModel _parentPageViewModel;
    private readonly bool _isOwnerCommunityAdmined;
    private readonly bool _isPickerModel;
    private readonly long _ownerId;
    private bool _isSelected;

    public GenericCollectionViewModel<DocumentsInfo, DocumentHeader> Items { get; set; }

    public string Title { get; set; }

    public long SectionId { get; set; }

    public bool IsSelected
    {
      get
      {
        return this._isSelected;
      }
      set
      {
        this._isSelected = value;
        this.NotifyPropertyChanged("IsSelected");
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.Foreground));
      }
    }

    public SolidColorBrush Foreground
    {
      get
      {
        if (!this._isSelected)
          return (SolidColorBrush) Application.Current.Resources["PhoneAlmostBlackBrush"];
        return (SolidColorBrush) Application.Current.Resources["PhoneAccentBlueBrush"];
      }
    }

    public Func<DocumentsInfo, ListWithCount<DocumentHeader>> ConverterFunc
    {
      get
      {
        return (Func<DocumentsInfo, ListWithCount<DocumentHeader>>) (list =>
        {
          if (list.types != null && this._parentPageViewModel.Sections.Count <= 1)
          {
            foreach (Category category in list.types.items)
            {
              string sectionTitle = category.Name;
              if (!this._isPickerModel)
                sectionTitle = sectionTitle.ToLower();
              this._parentPageViewModel.Sections.Add(new DocumentsSectionViewModel(this._parentPageViewModel, this._ownerId, category.id, sectionTitle, this._isOwnerCommunityAdmined, this._isPickerModel));
            }
          }
          return new ListWithCount<DocumentHeader>()
          {
            TotalCount = list.documents.count,
            List = list.documents.items.Select<Doc, DocumentHeader>((Func<Doc, DocumentHeader>) (i => new DocumentHeader(i, (int) this.SectionId, this._isOwnerCommunityAdmined))).ToList<DocumentHeader>()
          };
        });
      }
    }

    public DocumentsSectionViewModel(DocumentsViewModel parentPageViewModel, long ownerId, long sectionId, string sectionTitle, bool isOwnerCommunityAdmined, bool isPickerModel)
    {
      this.Items = new GenericCollectionViewModel<DocumentsInfo, DocumentHeader>((ICollectionDataProvider<DocumentsInfo, DocumentHeader>) this);
      this._parentPageViewModel = parentPageViewModel;
      this._ownerId = ownerId;
      this.SectionId = sectionId;
      this.Title = sectionTitle;
      this._isOwnerCommunityAdmined = isOwnerCommunityAdmined;
      this._isPickerModel = isPickerModel;
      EventAggregator.Current.Subscribe((object) this);
    }

    public void GetData(GenericCollectionViewModel<DocumentsInfo, DocumentHeader> caller, int offset, int count, Action<BackendResult<DocumentsInfo, ResultCode>> callback)
    {
      DocumentsService.Current.GetDocuments(callback, offset, count, this._ownerId, this.SectionId);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<DocumentsInfo, DocumentHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.Documents_NoDocuments;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneDocFrm, CommonResources.TwoFourDocumentsFrm, CommonResources.FiveDocumentsFrm, true, null, false);
    }

    public void Handle(DocumentUploadedEvent message)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (message.OwnerId != this._ownerId || this.SectionId != 0L && (long) message.Document.type != this.SectionId)
          return;
        this.Items.Insert(new DocumentHeader(message.Document, (int) this.SectionId, this._isOwnerCommunityAdmined), 0);
        ++this.Items.TotalCount;
        this.Items.NotifyChanged();
      }));
    }

    public void Handle(DocumentDeletedEvent message)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        DocumentHeader documentHeader = this.Items.Collection.FirstOrDefault<DocumentHeader>((Func<DocumentHeader, bool>) (i =>
        {
          if (i.Document.owner_id == message.OwnerId)
            return i.Document.id == message.Id;
          return false;
        }));
        if (documentHeader == null)
          return;
        this.Items.Delete(documentHeader);
      }));
    }
  }
}
