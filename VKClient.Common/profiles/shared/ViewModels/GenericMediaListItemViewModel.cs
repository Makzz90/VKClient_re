using System;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class GenericMediaListItemViewModel : MediaListItemViewModelBase, IHandle<DocumentEditedOrDeletedEvent>, IHandle, IHandle<DocumentUploadedEvent>
  {
    private string _id;
    private ProfileBlockType _blockType;

    public string GenericTitle { get; private set; }

    public string GenericSubtitle { get; private set; }

    public string IconUri { get; private set; }

    public override string Id
    {
      get
      {
        return this._id;
      }
    }

    public GenericMediaListItemViewModel(string id, string genericTitle, string genericSubtitle, string iconUri, ProfileBlockType blockType)
      : base(ProfileMediaListItemType.Generic)
    {
      this._id = id;
      this._blockType = blockType;
      this.GenericTitle = genericTitle;
      this.GenericSubtitle = genericSubtitle;
      this.IconUri = iconUri;
      EventAggregator.Current.Subscribe(this);
    }

    public void Handle(DocumentEditedOrDeletedEvent message)
    {
      if (this._blockType != ProfileBlockType.docs || !(string.Format("doc{0}_{1}", message.OwnerId, message.Id) == this._id) && message.IsEdited)
        return;
      this._id = message.NewFirstDocumentId;
      this.GenericTitle = message.Title;
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.GenericTitle);
      if (message.IsEdited)
        return;
      this.GenericSubtitle = message.SizeString;
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.GenericSubtitle);
    }

    public void Handle(DocumentUploadedEvent message)
    {
      if (this._blockType != ProfileBlockType.docs || message.OwnerId.ToString() != this._id.Remove(this._id.IndexOf('_')).Remove(0, 3))
        return;
      DocumentHeader documentHeader = new DocumentHeader(message.Document, 0, false, 0L);
      this._id = string.Format("doc{0}_{1}", message.Document.owner_id, message.Document.id);
      this.GenericTitle = documentHeader.Name;
      this.GenericSubtitle = documentHeader.GetSizeString();
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.GenericTitle);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.GenericSubtitle);
    }
  }
}
