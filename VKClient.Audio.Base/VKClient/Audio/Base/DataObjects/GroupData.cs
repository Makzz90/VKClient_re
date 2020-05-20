using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GroupData : IProfileData
  {
    public Group group { get; set; }

    public GroupMembership membership { get; set; }

    public long Id
    {
      get
      {
        Group group = this.group;
        if (group == null)
          return 0;
        return group.id;
      }
    }

    public Counters counters
    {
      get
      {
        Group group = this.group;
        if (group == null)
          return  null;
        return group.counters;
      }
    }

    public VKList<Photo> photos { get; set; }

    public VKList<User> followers { get; set; }

    public List<User> friends { get; set; }

    public int friendsCount { get; set; }

    public VKList<SubscriptionObj> subscriptions { get; set; }

    public VKList<GiftItemData> gifts { get; set; }

    public Album mainAlbum { get; set; }

    public Photo lastPhoto { get; set; }

    public List<Topic> topics { get; set; }

    public List<Video> videos { get; set; }

    public List<Product> products { get; set; }

    public bool isMarketMainAlbumEmpty { get; set; }

    public List<AudioObj> audios { get; set; }

    public Doc lastDoc { get; set; }

    public int suggestedPostsCount { get; set; }

    public int postponedPostsCount { get; set; }

    public int wallPostsCount { get; set; }

    public List<User> contactsUsers { get; set; }

    public string FirstName
    {
      get
      {
        Group group = this.group;
        return (group != null ? group.name :  null) ?? "";
      }
    }

    public string Name
    {
      get
      {
        Group group = this.group;
        return (group != null ? group.name :  null) ?? "";
      }
    }

    public string NameGen
    {
      get
      {
        return "";
      }
    }

    public bool ShowAllPostsByDefault
    {
      get
      {
        return true;
      }
    }

    public int AdminLevel
    {
      get
      {
        Group group = this.group;
        if (group == null)
          return 0;
        return group.admin_level;
      }
    }

    public string PhotoMax
    {
      get
      {
        if (this.group == null)
          return "";
        return this.group.photo_max ?? this.group.photo_200;
      }
      set
      {
        if (this.group == null)
          return;
        this.group.photo_max = value;
      }
    }

    public bool IsFavorite
    {
      get
      {
        if (this.group != null)
          return this.group.is_favorite == 1;
        return false;
      }
      set
      {
        if (this.group == null)
          return;
        this.group.is_favorite = value ? 1 : 0;
      }
    }

    public bool IsSubscribed
    {
      get
      {
        if (this.group != null)
          return this.group.is_subscribed == 1;
        return false;
      }
      set
      {
        if (this.group == null)
          return;
        this.group.is_subscribed = value ? 1 : 0;
      }
    }

    public bool IsDeactivated
    {
      get
      {
        Group group = this.group;
        return !string.IsNullOrEmpty(group != null ? group.deactivated :  null);
      }
    }

    public bool CanPost
    {
      get
      {
        if (this.group != null)
          return this.group.can_post == 1;
        return false;
      }
    }

    public bool CanSuggestAPost
    {
      get
      {
        if (!this.CanPost)
          return this.group.GroupType == GroupType.PublicPage;
        return false;
      }
    }

    public string Activity
    {
      get
      {
        if (this.group == null)
          return "";
        return this.group.status;
      }
      set
      {
        if (this.group == null)
          return;
        this.group.status = value;
      }
    }

    public bool IsVerified
    {
      get
      {
        if (this.group != null)
          return this.group.verified == 1;
        return false;
      }
    }

    public CoverImage CoverImage
    {
      get
      {
        Group group = this.group;
        if (group == null)
          return  null;
        Cover cover = group.cover;
        if (cover == null)
          return  null;
        return cover.CurrentImage;
      }
    }

    public ProfileMainSectionType MainSectionType
    {
      get
      {
        Group group = this.group;
        if (group == null)
          return ProfileMainSectionType.None;
        return group.MainSection;
      }
    }

    public bool CanAddPhotos
    {
      get
      {
        if (this.mediaSectionsSettings != null)
          return this.CanAddMediaData(this.mediaSectionsSettings.photos);
        return false;
      }
    }

    public bool CanAddTopics
    {
      get
      {
        if (this.mediaSectionsSettings != null)
          return this.CanAddMediaData(this.mediaSectionsSettings.topics);
        return false;
      }
    }

    public bool CanAddVideos
    {
      get
      {
        if (this.mediaSectionsSettings != null)
          return this.CanAddMediaData(this.mediaSectionsSettings.video);
        return false;
      }
    }

    public bool CanAddProducts
    {
      get
      {
        return false;
      }
    }

    public bool CanAddAudios
    {
      get
      {
        if (this.mediaSectionsSettings != null)
          return this.CanAddMediaData(this.mediaSectionsSettings.audio);
        return false;
      }
    }

    public bool CanAddDocs
    {
      get
      {
        if (this.mediaSectionsSettings != null)
          return this.CanAddMediaData(this.mediaSectionsSettings.docs);
        return false;
      }
    }

    public MediaSectionsSettings mediaSectionsSettings { get; set; }

    public VKList<Group> invites { get; set; }

    public int areFriendsStatus
    {
      get
      {
        return 0;
      }
    }

    public string wiki_view_url { get; set; }

    private bool CanAddMediaData(int sectionSettings)
    {
      if (sectionSettings > 1)
        return true;
      if (sectionSettings > 0)
        return this.AdminLevel > 1;
      return false;
    }
  }
}
