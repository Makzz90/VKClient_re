using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ProfileMediaSectionsViewModel : ViewModelBase
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
      GroupData groupData = this._profileData as GroupData;
      if ((groupData != null ? groupData.group : (Group) null) != null)
      {
        Group group = groupData.group;
        if (this._mainSectionType != ProfileMainSectionType.Discussions && !topics.IsNullOrEmpty())
        {
          Topic topic = topics.First<Topic>();
          sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Discussions, counters.topics, (MediaListItemViewModelBase) new GenericMediaListItemViewModel(topic.ToString(), topic.title, UIStringFormatterHelper.FormatNumberOfSomething(topic.comments, CommonResources.OneMessageFrm, CommonResources.TwoFourMessagesFrm, CommonResources.FiveMessagesFrm, true, null, false), "/Resources/Profile/ProfileTopic.png"), (Action) (() => Navigator.Current.NavigateToGroupDiscussions(group.id, "", group.admin_level, false, group.CanCreateTopic))));
        }
        else if (this._profileData.CanAddTopics && counters.topics == 0)
          sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Discussions, 0, (MediaListItemViewModelBase) new EmptyDataMediaListItemViewModel("/Resources/Profile/Placeholders/ProfilePlaceholderTopics.png"), (Action) (() => Navigator.Current.NavigateToGroupDiscussions(group.id, "", group.admin_level, false, group.CanCreateTopic))));
      }
      List<VKClient.Common.Backend.DataObjects.Video> videos = this._profileData.videos;
      if (this._mainSectionType != ProfileMainSectionType.Videos && !videos.IsNullOrEmpty())
      {
          VKClient.Common.Backend.DataObjects.Video video = videos.First<VKClient.Common.Backend.DataObjects.Video>();
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
        Product product = products.First<Product>();
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Products, counters.market, (MediaListItemViewModelBase) new ProductsAlbumMediaListItemViewModel(product), (Action) (() => {})));
      }
      else if (this._isGroup && this._profileData.CanAddProducts && counters.market == 0)
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Videos, counters.topics, (MediaListItemViewModelBase) new EmptyDataMediaListItemViewModel("/Resources/Profile/Placeholders/ProfilePlaceholderVideos.png"), (Action) (() => {})));
      List<AudioObj> audios = this._profileData.audios;
      if (this._mainSectionType != ProfileMainSectionType.Audios && !audios.IsNullOrEmpty())
      {
        AudioObj audioObj = audios.First<AudioObj>();
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Audios, counters.audios, (MediaListItemViewModelBase) new GenericMediaListItemViewModel(audioObj.ToString(), audioObj.title, audioObj.artist, "/Resources/Profile/ProfileMusic.png"), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.audios);
          Navigator.Current.NavigateToAudio(0, this._profileData.Id, this._isGroup, 0L, 0L, "");
        })));
      }
      else if (this._isGroup && this._profileData.CanAddAudios && counters.audios == 0)
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Audios, 0, (MediaListItemViewModelBase) new EmptyDataMediaListItemViewModel("/Resources/Profile/Placeholders/ProfilePlaceholderAudios.png"), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.audios);
          Navigator.Current.NavigateToAudio(0, this._profileData.Id, this._isGroup, 0L, 0L, "");
        })));
      VKList<SubscriptionObj> subscriptions = this._profileData.subscriptions;
      int titleCounter = counters.pages + counters.groups + counters.subscriptions;
      if (subscriptions != null && titleCounter > 0)
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Subscriptions, titleCounter, (MediaListItemViewModelBase) new SubscriptionsMediaListItemViewModel(subscriptions.items.Select<SubscriptionObj, string>((Func<SubscriptionObj, string>) (item => item.photo_max)).ToList<string>()), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.subscriptions);
          Navigator.Current.NavigateToSubscriptions(this._profileData.Id);
        })));
      Doc lastDoc = this._profileData.lastDoc;
      if (lastDoc != null)
      {
        if (this._profileData.Id == AppGlobalStateManager.Current.LoggedInUserId || this._isGroup)
        {
          DocumentHeader documentHeader = new DocumentHeader(lastDoc, 0, false);
          sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Docs, counters.docs, (MediaListItemViewModelBase) new GenericMediaListItemViewModel(lastDoc.ToString(), documentHeader.Name, documentHeader.GetSizeString(), "/Resources/Profile/ProfileDocuments.png"), (Action) (() =>
          {
            this.PublishProfileBlockClickEvent(ProfileBlockType.docs);
            Navigator.Current.NavigateToDocuments(this._isGroup ? -this._profileData.Id : this._profileData.Id, this._profileData.AdminLevel > 1);
          })));
        }
      }
      else if (this._profileData.CanAddDocs && counters.docs == 0)
        sectionViewModelList.Add(new MediaListSectionViewModel(CommonResources.Profile_Docs, 0, (MediaListItemViewModelBase) new EmptyDataMediaListItemViewModel("/Resources/Profile/Placeholders/ProfilePlaceholderDocs.png"), (Action) (() =>
        {
          this.PublishProfileBlockClickEvent(ProfileBlockType.docs);
          Navigator.Current.NavigateToDocuments(this._isGroup ? -this._profileData.Id : this._profileData.Id, this._profileData.AdminLevel > 1);
        })));
      return sectionViewModelList;
    }

    private void PublishProfileBlockClickEvent(ProfileBlockType blockType)
    {
      if (this._isGroup)
        return;
      EventAggregator.Current.Publish((object) new ProfileBlockClickEvent()
      {
        UserId = this._profileData.Id,
        BlockType = blockType
      });
    }
  }
}
