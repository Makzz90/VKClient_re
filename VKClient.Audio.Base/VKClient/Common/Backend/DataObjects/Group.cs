using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Group : IProfile, IBinarySerializable
  {
    private string _description = "";
    private Counters _counters = new Counters();
    private string _name;
    private string _type;

    public long id { get; set; }

    public string activity { get; set; }

    public string status { get; set; }

    public string name
    {
      get
      {
        return this._name;
      }
      set
      {
        this._name = (value ?? "").ForUI();
      }
    }

    public string first_name_gen
    {
      get
      {
        return this.name;
      }
    }

    public string first_name
    {
      get
      {
        return this.name;
      }
    }

    public string deactivated { get; set; }

    public string link { get; set; }

    public string description
    {
      get
      {
        return this._description;
      }
      set
      {
        this._description = (value ?? "").ForUI();
      }
    }

    public string wiki_page { get; set; }

    public int members_count { get; set; }

    public Counters counters
    {
      get
      {
        return this._counters;
      }
      set
      {
        this._counters = value;
      }
    }

    public Place place { get; set; }

    public string screen_name { get; set; }

    public string type
    {
      get
      {
        return this._type;
      }
      set
      {
        this._type = value;
        if (!string.IsNullOrEmpty(this._type))
        {
          string type = this._type;
          if (!(type == "group"))
          {
            if (!(type == "page"))
            {
              if (type == "event")
              {
                this.GroupType = GroupType.Event;
                return;
              }
            }
            else
            {
              this.GroupType = GroupType.PublicPage;
              return;
            }
          }
          else
          {
            this.GroupType = GroupType.Group;
            return;
          }
        }
        this.GroupType = GroupType.Group;
      }
    }

    public string photo
    {
      get
      {
        return this.photo_50;
      }
      set
      {
        this.photo_50 = value;
      }
    }

    public string photo_medium
    {
      get
      {
        return this.photo_100;
      }
      set
      {
        this.photo_100 = value;
      }
    }

    public string photo_big
    {
      get
      {
        return this.photo_200;
      }
      set
      {
        this.photo_200 = value;
      }
    }

    public string photo_50 { get; set; }

    public string photo_100 { get; set; }

    public string photo_200 { get; set; }

    public string photo_max { get; set; }

    public int start_date { get; set; }

    public int finish_date { get; set; }

    public long invited_by { get; set; }

    public int fixed_post { get; set; }

    public int SuggestedPostsCount { get; set; }

    public int PostponedPostsCount { get; set; }

    public GroupMembershipType MembershipType { get; set; }

    public GroupType GroupType { get; private set; }

    public int is_subscribed { get; set; }

    public bool IsSubscribed
    {
      get
      {
        return this.is_subscribed == 1;
      }
      set
      {
        this.is_subscribed = value ? 1 : 0;
      }
    }

    public int is_favorite { get; set; }

    public bool IsFavorite
    {
      get
      {
        return this.is_favorite == 1;
      }
      set
      {
        this.is_favorite = value ? 1 : 0;
      }
    }

    public int verified { get; set; }

    public bool IsVerified
    {
      get
      {
        return this.verified == 1;
      }
      set
      {
        this.verified = value ? 1 : 0;
      }
    }

    public int can_post { get; set; }

    public bool CanPost
    {
      get
      {
        return this.can_post == 1;
      }
      set
      {
        this.can_post = value ? 1 : 0;
      }
    }

    public int can_see_all_posts { get; set; }

    public bool CanSeeAllPosts
    {
      get
      {
        return this.can_see_all_posts == 1;
      }
      set
      {
        this.can_see_all_posts = value ? 1 : 0;
      }
    }

    public int can_see_gifts
    {
      get
      {
        return 0;
      }
    }

    public int is_closed { get; set; }

    public GroupPrivacy Privacy
    {
      get
      {
        return (GroupPrivacy) this.is_closed;
      }
      set
      {
        this.is_closed = (int) value;
      }
    }

    public bool CanJoin
    {
      get
      {
        if ((this.MembershipType != GroupMembershipType.Member && this.MembershipType != GroupMembershipType.NotSure && this.Privacy != GroupPrivacy.Private || this.MembershipType == GroupMembershipType.InvitationReceived) && (this.ban_info == null || this.ban_info.end_date > 0L))
          return string.IsNullOrEmpty(this.deactivated);
        return false;
      }
    }

    private int is_admin { get; set; }

    public int admin_level { get; set; }

    public int is_member { get; set; }

    public bool IsMember
    {
      get
      {
        return this.is_member == 1;
      }
      set
      {
        this.is_member = value ? 1 : 0;
      }
    }

    public int member_status { get; set; }

    public int can_upload_video { get; set; }

    public bool CanUploadVideo
    {
      get
      {
        return this.can_upload_video == 1;
      }
      set
      {
        this.can_upload_video = value ? 1 : 0;
      }
    }

    public int can_create_topic { get; set; }

    public bool CanCreateTopic
    {
      get
      {
        return this.can_create_topic == 1;
      }
      set
      {
        this.can_create_topic = value ? 1 : 0;
      }
    }

    public int can_message { get; set; }

    public int is_messages_blocked { get; set; }

    public BanInfo ban_info { get; set; }

    public string site { get; set; }

    public List<GroupLink> links { get; set; }

    public List<GroupContact> contacts { get; set; }

    public City city { get; set; }

    public Country country { get; set; }

    public long main_album_id { get; set; }

    public int main_section { get; set; }

    public ProfileMainSectionType MainSection { get; set; }

    public Market market { get; set; }

    public AppButton app_button { get; set; }

    public Cover cover { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.id);
      writer.WriteString(this.name);
      writer.WriteString(this.photo_200);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.name = reader.ReadString();
      this.photo_200 = reader.ReadString();
    }
  }
}
