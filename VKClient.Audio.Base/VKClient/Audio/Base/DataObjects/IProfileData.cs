using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public interface IProfileData
  {
    long Id { get; }

    Counters counters { get; }

    VKList<Photo> photos { get; }

    VKList<User> followers { get; set; }

    VKList<SubscriptionObj> subscriptions { get; set; }

    VKList<GiftItemData> gifts { get; set; }

    Album mainAlbum { get; }

    Photo lastPhoto { get; }

    List<Topic> topics { get; }

    List<Video> videos { get; }

    List<Product> products { get; }

    bool isMarketMainAlbumEmpty { get; }

    List<AudioObj> audios { get; }

    Doc lastDoc { get; }

    int suggestedPostsCount { get; }

    int postponedPostsCount { get; }

    string FirstName { get; }

    string Name { get; }

    string NameGen { get; }

    bool ShowAllPostsByDefault { get; }

    int AdminLevel { get; }

    string PhotoMax { get; set; }

    bool IsFavorite { get; set; }

    bool IsSubscribed { get; set; }

    bool IsDeactivated { get; }

    bool CanPost { get; }

    bool CanSuggestAPost { get; }

    string Activity { get; set; }

    bool IsVerified { get; }

    ProfileMainSectionType MainSectionType { get; }

    bool CanAddPhotos { get; }

    bool CanAddTopics { get; }

    bool CanAddVideos { get; }

    bool CanAddProducts { get; }

    bool CanAddAudios { get; }

    bool CanAddDocs { get; }

    MediaSectionsSettings mediaSectionsSettings { get; }

    VKList<Group> invites { get; }

    int areFriendsStatus { get; }
  }
}
