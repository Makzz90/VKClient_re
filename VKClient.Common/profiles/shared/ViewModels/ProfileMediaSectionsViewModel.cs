using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileMediaSectionsViewModel : ViewModelBase, IHandle<DocumentUploadedEvent>, IHandle, IHandle<DocumentEditedOrDeletedEvent>
  {
    private IProfileData _profileData;
    private bool _isGroup;
    private ProfileMainSectionType _mainSectionType;
    private List<MediaListSectionViewModel> _items;

    public bool CanDisplay
    {
      get
      {
        if (this._items != null)
          return this._items.Count > 0;
        return false;
      }
    }

    public List<MediaListSectionViewModel> Items
    {
      get
      {
        return this._items;
      }
      private set
      {
        this._items = value;
        this.NotifyPropertyChanged("Items");
      }
    }

    public ProfileMediaSectionsViewModel()
    {
      this._items = new List<MediaListSectionViewModel>();
      EventAggregator.Current.Subscribe(this);
    }

    public void Init(IProfileData profileData, ProfileMainSectionType mainSectionType)
    {
      this._profileData = profileData;
      this._isGroup = this._profileData is GroupData;
      this._mainSectionType = mainSectionType;
      List<MediaListSectionViewModel> mediaItems = this.CreateMediaItems();
      if (ProfileMediaSectionsViewModel.AreItemsEqual((IList<MediaListSectionViewModel>) mediaItems, (IList<MediaListSectionViewModel>) this._items))
        return;
      this.Items = new List<MediaListSectionViewModel>((IEnumerable<MediaListSectionViewModel>) mediaItems);
    }

    private static bool AreItemsEqual(IList<MediaListSectionViewModel> items1, IList<MediaListSectionViewModel> items2)
    {
      if (items1.Count != items2.Count)
        return false;
      for (int index = 0; index < items1.Count; ++index)
      {
        if (items1[index].ListItemViewModel.Id != items2[index].ListItemViewModel.Id)
          return false;
      }
      return true;
    }

    private List<MediaListSectionViewModel> CreateMediaItems()
    {
      List<MediaListSectionViewModel> sectionViewModelList = new List<MediaListSectionViewModel>();
      if (this._profileData == null)
        return sectionViewModelList;
      Counters counters = this._profileData.counters;
      Photo lastPhoto = this._profileData.lastPhoto;
      if ((this._mainSectionType != ProfileMainSectionType.Photos || this._profileData.photos == null || this._profileData.photos.items.Count == 0) && lastPhoto != null)
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Photos, counters.photos, (MediaListItemViewModelBase) new PhotoAlbumMediaListItemViewModel(lastPhoto), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.photos);
          Navigator.Current.NavigateToPhotoAlbums(false, this._profileData.Id, this._isGroup, this._profileData.AdminLevel);
        })));
      else if (this._isGroup && this._profileData.CanAddPhotos && counters.photos == 0)
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Photos, 0, (MediaListItemViewModelBase) new EmptyDataMediaListItemViewModel("/Resources/Profile/Placeholders/ProfilePlaceholderPhotos.png"), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.photos);
          Navigator.Current.NavigateToPhotoAlbums(false, this._profileData.Id, this._isGroup, this._profileData.AdminLevel);
        })));
      List<Topic> topics = this._profileData.topics;
      GroupData profileData = this._profileData as GroupData;
      if ((profileData != null ? profileData.group :  null) != null)
      {
        Group group = profileData.group;
        if (this._mainSectionType != ProfileMainSectionType.Discussions && !topics.IsNullOrEmpty())
        {
          Topic topic = (Topic) Enumerable.First<Topic>(topics);
          sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Discussions, counters.topics, (MediaListItemViewModelBase) new GenericMediaListItemViewModel(topic.ToString(), topic.title, UIStringFormatterHelper.FormatNumberOfSomething(topic.comments, CommonResources.OneMessageFrm, CommonResources.TwoFourMessagesFrm, CommonResources.FiveMessagesFrm, true,  null, false), "/Resources/Profile/ProfileTopic.png", ProfileBlockType.discussions), (Action) (() => Navigator.Current.NavigateToGroupDiscussions(group.id, "", group.admin_level, false, group.CanCreateTopic))));
        }
        else if (this._profileData.CanAddTopics && counters.topics == 0)
          sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Discussions, 0, (MediaListItemViewModelBase) new EmptyDataMediaListItemViewModel("/Resources/Profile/Placeholders/ProfilePlaceholderTopics.png"), (Action) (() => Navigator.Current.NavigateToGroupDiscussions(group.id, "", group.admin_level, false, group.CanCreateTopic))));
      }
      List<VKClient.Common.Backend.DataObjects.Video> videos = this._profileData.videos;
      if (this._mainSectionType != ProfileMainSectionType.Videos && !videos.IsNullOrEmpty())
      {
        VKClient.Common.Backend.DataObjects.Video video = Enumerable.First<VKClient.Common.Backend.DataObjects.Video>(videos);
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Videos, counters.videos, (MediaListItemViewModelBase) new VideoAlbumMediaListItemViewModel(video), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.videos);
          Navigator.Current.NavigateToVideo(false, this._profileData.Id, this._isGroup, this._profileData.AdminLevel > 1);
        })));
      }
      else if (this._isGroup && this._profileData.CanAddVideos && counters.videos == 0)
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Videos, counters.topics, (MediaListItemViewModelBase) new EmptyDataMediaListItemViewModel("/Resources/Profile/Placeholders/ProfilePlaceholderVideos.png"), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.videos);
          Navigator.Current.NavigateToVideo(false, this._profileData.Id, this._isGroup, this._profileData.AdminLevel > 1);
        })));
      List<Product> products = this._profileData.products;
      if (this._mainSectionType != ProfileMainSectionType.Market && !products.IsNullOrEmpty())
      {
        Product product = (Product) Enumerable.First<Product>(products);
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Products, counters.market, (MediaListItemViewModelBase) new ProductsAlbumMediaListItemViewModel(product), (Action) (() => {})));
      }
      else if (this._isGroup && this._profileData.CanAddProducts && counters.market == 0)
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Videos, counters.topics, (MediaListItemViewModelBase) new EmptyDataMediaListItemViewModel("/Resources/Profile/Placeholders/ProfilePlaceholderVideos.png"), (Action) (() => {})));
      List<AudioObj> audios = this._profileData.audios;
      if (this._mainSectionType != ProfileMainSectionType.Audios && !audios.IsNullOrEmpty())
      {
        AudioObj audioObj = (AudioObj) Enumerable.First<AudioObj>(audios);
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Audios, counters.audios, (MediaListItemViewModelBase) new GenericMediaListItemViewModel(audioObj.ToString(), audioObj.title, audioObj.artist, "/Resources/Profile/ProfileMusic.png", ProfileBlockType.audios), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.audios);
          Navigator.Current.NavigateToAudio(0, this._profileData.Id, this._isGroup, 0, 0, "");
        })));
      }
      else if (this._isGroup && this._profileData.CanAddAudios && counters.audios == 0)
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Audios, 0, (MediaListItemViewModelBase) new EmptyDataMediaListItemViewModel("/Resources/Profile/Placeholders/ProfilePlaceholderAudios.png"), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.audios);
          Navigator.Current.NavigateToAudio(0, this._profileData.Id, this._isGroup, 0, 0, "");
        })));
      VKList<SubscriptionObj> subscriptions = this._profileData.subscriptions;
      int titleCounter = counters.pages + counters.groups + counters.subscriptions;
      if (subscriptions != null && titleCounter > 0)
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: method pointer
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Subscriptions, titleCounter, (MediaListItemViewModelBase) new SubscriptionsMediaListItemViewModel((List<string>) Enumerable.ToList<string>(Enumerable.Select<SubscriptionObj, string>(subscriptions.items, new Func<SubscriptionObj, string>((item)=>{return item.photo_max;})))), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.subscriptions);
          Navigator.Current.NavigateToSubscriptions(this._profileData.Id);
        })));
      }
      Doc lastDoc = this._profileData.lastDoc;
      if (lastDoc != null)
      {
        if (this._profileData.Id == AppGlobalStateManager.Current.LoggedInUserId || this._isGroup)
          sectionViewModelList.Add(this.GetDocumentsModel(lastDoc, counters.docs));
      }
      else if (this._profileData.CanAddDocs && counters.docs == 0)
        sectionViewModelList.Add(this.GetDocumentsEmptyModel());
      int gifts1 = counters.gifts;
      VKList<GiftItemData> gifts2 = this._profileData.gifts;
      List<GiftItemData> giftItemDataList = gifts2 != null ? gifts2.items :  null;
      if (gifts1 > 0 && giftItemDataList != null)
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: method pointer
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: method pointer
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Gifts, gifts1, new GiftsMediaListItemViewModel((List<string>) Enumerable.ToList<string>(Enumerable.Select<GiftItemData, string>(Enumerable.Where<GiftItemData>(giftItemDataList, new Func<GiftItemData, bool>((item)=>{return item != null;})), new Func<GiftItemData, string>((item)=>{return item.gift.thumb_256;})))), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.gifts);
          Navigator.Current.NavigateToGifts(this._profileData.Id, this._profileData.FirstName, this._profileData.NameGen);
        })));
      }
      return sectionViewModelList;
    }

    private void PublishProfileBlockClickEvent(ProfileBlockType blockType)
    {
      if (this._isGroup)
        return;
      EventAggregator.Current.Publish(new ProfileBlockClickEvent()
      {
        UserId = this._profileData.Id,
        BlockType = blockType
      });
    }

    public void Handle(DocumentUploadedEvent message)
    {
      if (message.OwnerId != this._profileData.Id)
        return;
      List<MediaListSectionViewModel>.Enumerator enumerator = this.Items.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          MediaListSectionViewModel current = enumerator.Current;
          if (current.Title == CommonResources.Profile_Docs)
          {
            if (!(current.ListItemViewModel is EmptyDataMediaListItemViewModel))
              break;
            int index = this.Items.IndexOf(current);
            this.Items.RemoveAt(index);
            this.Items.Insert(index, this.GetDocumentsModel(message.Document, 1));
            break;
          }
        }
      }
      finally
      {
        enumerator.Dispose();
      }
    }

    public void Handle(DocumentEditedOrDeletedEvent message)
    {
      if (message.OwnerId != this._profileData.Id || message.IsEdited || message.NewDocumentsCount != 0)
        return;
      List<MediaListSectionViewModel>.Enumerator enumerator = this.Items.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          MediaListSectionViewModel current = enumerator.Current;
          if (current.Title == CommonResources.Profile_Docs)
          {
            if (!(current.ListItemViewModel is GenericMediaListItemViewModel))
              break;
            int index = this.Items.IndexOf(current);
            this.Items.RemoveAt(index);
            this.Items.Insert(index, this.GetDocumentsEmptyModel());
            break;
          }
        }
      }
      finally
      {
        enumerator.Dispose();
      }
    }

    private MediaListSectionViewModel GetDocumentsModel(Doc document, int count)
    {
      DocumentHeader documentHeader = new DocumentHeader(document, 0, false, 0L);
      return new MediaListSectionViewModel(CommonResources.Profile_Docs, count, (MediaListItemViewModelBase) new GenericMediaListItemViewModel(document.ToString(), documentHeader.Name, documentHeader.GetSizeString(), "/Resources/Profile/ProfileDocuments.png", ProfileBlockType.docs), (Action) (() =>
      {
        this.PublishProfileBlockClickEvent(ProfileBlockType.docs);
        Navigator.Current.NavigateToDocuments(this._isGroup ? -this._profileData.Id : this._profileData.Id, this._profileData.AdminLevel > 1);
      }));
    }

    private MediaListSectionViewModel GetDocumentsEmptyModel()
    {
      return new MediaListSectionViewModel(CommonResources.Profile_Docs, 0, (MediaListItemViewModelBase) new EmptyDataMediaListItemViewModel("/Resources/Profile/Placeholders/ProfilePlaceholderDocs.png"), (Action) (() =>
      {
        this.PublishProfileBlockClickEvent(ProfileBlockType.docs);
        Navigator.Current.NavigateToDocuments(this._isGroup ? -this._profileData.Id : this._profileData.Id, this._profileData.AdminLevel > 1);
      }));
    }
  }
}
