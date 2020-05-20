using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;

namespace VKClient.Audio.Base.DataObjects
{
  public class UserData : IProfileData
  {
    public User user { get; set; }

    public City city { get; set; }

    public User full_gen { get; set; }

    public FriendStatus friend { get; set; }

    public List<User> randomMutualFriends { get; set; }

    public List<User> relatives { get; set; }

    public NameCases relPartnerNameCases { get; set; }

    public int docsCount { get; set; }

    public CareerData careerData { get; set; }

    public List<Country> militaryCountries { get; set; }

    public List<Group> universityGroups { get; set; }

    public List<Group> schoolGroups { get; set; }

    public Group occupationGroup { get; set; }

    public VKList<User> followers { get; set; }

    public VKList<SubscriptionObj> subscriptions { get; set; }

    public VKList<GiftItemData> gifts { get; set; }

    public long Id
    {
      get
      {
        User user = this.user;
        if (user == null)
          return 0;
        return user.id;
      }
    }

    public Counters counters
    {
      get
      {
        User user = this.user;
        if (user == null)
          return  null;
        return user.counters;
      }
    }

    public VKList<Photo> photos { get; set; }

    public Album mainAlbum { get; set; }

    public Photo lastPhoto { get; set; }

    public List<Topic> topics { get; set; }

    public List<Video> videos { get; set; }

    public List<Product> products { get; set; }

    public bool isMarketMainAlbumEmpty
    {
      get
      {
        return false;
      }
    }

    public List<AudioObj> audios { get; set; }

    public Doc lastDoc { get; set; }

    public int postponedPostsCount { get; set; }

    public int suggestedPostsCount
    {
      get
      {
        return 0;
      }
    }

    public string FirstName
    {
      get
      {
        if (this.user == null)
          return "";
        return this.user.first_name;
      }
    }

    public string Name
    {
      get
      {
        if (this.user == null)
          return "";
        return this.user.Name;
      }
    }

    public string NameGen
    {
      get
      {
        if (this.full_gen == null)
          return "";
        return this.full_gen.first_name;
      }
    }

    public bool ShowAllPostsByDefault
    {
      get
      {
        if (this.user != null)
          return this.user.wall_default == "all";
        return true;
      }
    }

    public int AdminLevel
    {
      get
      {
        return this.user == null || this.user.id != AppGlobalStateManager.Current.LoggedInUserId ? 0 : 3;
      }
    }

    public string PhotoMax
    {
      get
      {
        if (this.user == null)
          return "";
        return this.user.photo_max;
      }
      set
      {
        if (this.user == null)
          return;
        this.user.photo_max = value;
      }
    }

    public bool IsFavorite
    {
      get
      {
        if (this.user != null)
          return this.user.is_favorite == 1;
        return false;
      }
      set
      {
        if (this.user == null)
          return;
        this.user.is_favorite = value ? 1 : 0;
      }
    }

    public bool IsSubscribed
    {
      get
      {
        if (this.user != null)
          return this.user.is_subscribed == 1;
        return false;
      }
      set
      {
        if (this.user == null)
          return;
        this.user.is_subscribed = value ? 1 : 0;
      }
    }

    public bool IsDeactivated
    {
      get
      {
        User user = this.user;
        return !string.IsNullOrEmpty(user != null ? user.deactivated :  null);
      }
    }

    public bool CanPost
    {
      get
      {
        if (this.user != null)
          return this.user.can_post == 1;
        return false;
      }
    }

    public bool CanSuggestAPost
    {
      get
      {
        return false;
      }
    }

    public string Activity
    {
      get
      {
        if (this.user == null)
          return "";
        return this.user.activity;
      }
      set
      {
        if (this.user == null)
          return;
        this.user.activity = value;
      }
    }

    public bool IsVerified
    {
      get
      {
        if (this.user != null)
          return this.user.verified == 1;
        return false;
      }
    }

    public ProfileMainSectionType MainSectionType
    {
      get
      {
        return ProfileMainSectionType.Photos;
      }
    }

    public bool CanAddPhotos
    {
      get
      {
        return false;
      }
    }

    public bool CanAddTopics
    {
      get
      {
        return false;
      }
    }

    public bool CanAddVideos
    {
      get
      {
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
        return false;
      }
    }

    public bool CanAddDocs
    {
      get
      {
        return AppGlobalStateManager.Current.LoggedInUserId == this.user.id;
      }
    }

    public MediaSectionsSettings mediaSectionsSettings
    {
      get
      {
        return  null;
      }
    }

    public VKList<Group> invites
    {
      get
      {
        return  null;
      }
    }

    public int areFriendsStatus { get; set; }
  }
}
