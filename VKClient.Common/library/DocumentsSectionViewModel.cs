using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
  public sealed class DocumentsSectionViewModel : ViewModelBase, ICollectionDataProvider<DocumentsInfo, DocumentHeader>, IHandle<DocumentUploadedEvent>, IHandle, IHandle<DocumentEditedOrDeletedEvent>
  {
    private readonly DocumentsViewModel _parentPageViewModel;
    private readonly bool _isOwnerCommunityAdmined;
    private readonly bool _isPickerModel;
    private readonly long _ownerId;
    private bool _isSelected;

    public GenericCollectionViewModel<DocumentsInfo, DocumentHeader> Items { get; private set; }

    public string Title { get; private set; }

    public long SectionId { get; private set; }

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
        base.NotifyPropertyChanged<SolidColorBrush>(() => this.Foreground);
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
          if (list.types != null && ((Collection<DocumentsSectionViewModel>) this._parentPageViewModel.Sections).Count <= 1)
          {
            List<Category>.Enumerator enumerator = list.types.items.GetEnumerator();
            try
            {
              while (enumerator.MoveNext())
              {
                Category current = enumerator.Current;
                string sectionTitle = current.Name;
                if (!this._isPickerModel)
                  sectionTitle = ((string) sectionTitle).ToLower();
                ((Collection<DocumentsSectionViewModel>) this._parentPageViewModel.Sections).Add(new DocumentsSectionViewModel(this._parentPageViewModel, this._ownerId, current.id, sectionTitle, this._isOwnerCommunityAdmined, this._isPickerModel));
              }
            }
            finally
            {
              enumerator.Dispose();
            }
          }
          return new ListWithCount<DocumentHeader>() { TotalCount = list.documents.count, List = ((IEnumerable<DocumentHeader>)Enumerable.Select<Doc, DocumentHeader>(list.documents.items, (Func<Doc, DocumentHeader>)(i => new DocumentHeader(i, (int)this.SectionId, this._isOwnerCommunityAdmined, 0L)))).ToList<DocumentHeader>() };
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
      EventAggregator.Current.Subscribe(this);
    }

    public void GetData(GenericCollectionViewModel<DocumentsInfo, DocumentHeader> caller, int offset, int count, Action<BackendResult<DocumentsInfo, ResultCode>> callback)
    {
      DocumentsService.Current.GetDocuments(callback, offset, count, this._ownerId, this.SectionId);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<DocumentsInfo, DocumentHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.Documents_NoDocuments;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneDocFrm, CommonResources.TwoFourDocumentsFrm, CommonResources.FiveDocumentsFrm, true,  null, false);
    }

    public void Handle(DocumentUploadedEvent message)
    {
        Execute.ExecuteOnUIThread(delegate
        {
            if (message.OwnerId == this._ownerId && (this.SectionId == 0L || (long)message.Document.type == this.SectionId))
            {
                DocumentHeader item = new DocumentHeader(message.Document, (int)this.SectionId, this._isOwnerCommunityAdmined, 0L);
                this.Items.Insert(item, 0);
                GenericCollectionViewModel<DocumentsInfo, DocumentHeader> expr_8D = this.Items;
                int totalCount = expr_8D.TotalCount;
                expr_8D.TotalCount = totalCount + 1;
                this.Items.NotifyChanged();
            }
        });
    }

    public void Handle(DocumentEditedOrDeletedEvent message)
		{
			Execute.ExecuteOnUIThread(delegate
			{
				IEnumerable<DocumentHeader> arg_2F_0 = this.Items.Collection;
				Func<DocumentHeader, bool> arg_2F_1 = ((DocumentHeader i) => i.Document.owner_id == message.OwnerId && i.Document.id == message.Id);
				
				DocumentHeader documentHeader = Enumerable.FirstOrDefault<DocumentHeader>(arg_2F_0, arg_2F_1);
				if (documentHeader != null)
				{
					if (message.IsEdited)
					{
						documentHeader.Name = message.Title;
						documentHeader.Document.title = message.Title;
						return;
					}
					this.Items.Delete(documentHeader);
				}
			});
		}
  }
}
