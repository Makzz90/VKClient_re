using System;
using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
    public class User : IProfile, IBinarySerializable
    {
        public static readonly string DEACTIVATED_DELETED = "deleted";
        public static readonly string DEACTIVATED_BANNED = "banned";
        public static readonly long ADMIN_ID = 100;
        private string _fName = "";
        private string _lName = "";
        private string _firstNameGen = "";
        private string _lastNameGen = "";
        private string _firstNameDat = "";
        private string _lastNameDat = "";
        private string _home_town = "";
        private string _status = "";
        private string _activity = "";
        private string _activities = "";
        private string _interests = "";
        private string _music = "";
        private string _movies = "";
        private string _tv = "";
        private string _books = "";
        private string _games = "";
        private string _quotes = "";
        private string _about = "";
        private string _skype = "";
        private string _facebook = "";
        private string _first_name_acc;
        private string _last_name_acc;
        private string _bdate;
        private BirthDate _birthDate;
        private string _photo_rec;
        private string _photo_max;
        private string _facebookName;
        private string _twitter;
        private string _instagram;

        public string domain { get; set; }

        public DateTime CachedDateTime { get; set; }

        public long id { get; set; }

        public long uid
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public string first_name
        {
            get
            {
                return this._fName;
            }
            set
            {
                this._fName = (value ?? "").ForUI();
            }
        }

        public string FirstNameLastNameShort
        {
            get
            {
                string str = this.first_name;
                if (!string.IsNullOrWhiteSpace(this.last_name))
                    str = str + " " + this.last_name[0].ToString().ToUpperInvariant() + ".";
                return str;
            }
        }

        public string FirstNameDatLastNameShort
        {
            get
            {
                string str = this.first_name_dat;
                if (!string.IsNullOrWhiteSpace(this.last_name))
                    str = str + " " + this.last_name[0].ToString().ToUpperInvariant() + ".";
                return str;
            }
        }

        public string last_name
        {
            get
            {
                return this._lName;
            }
            set
            {
                this._lName = (value ?? "").ForUI();
            }
        }

        public string first_name_acc
        {
            get
            {
                return this._first_name_acc;
            }
            set
            {
                this._first_name_acc = (value ?? "").ForUI();
            }
        }

        public string last_name_acc
        {
            get
            {
                return this._last_name_acc;
            }
            set
            {
                this._last_name_acc = (value ?? "").ForUI();
            }
        }

        public string NameAcc
        {
            get
            {
                return "" + this.first_name_acc + " " + this.last_name_acc;
            }
        }

        public string NameGen
        {
            get
            {
                return "" + this.first_name_gen + " " + this.last_name_gen;
            }
        }

        public string first_name_gen
        {
            get
            {
                return this._firstNameGen;
            }
            set
            {
                this._firstNameGen = (value ?? "").ForUI();
            }
        }

        public string last_name_gen
        {
            get
            {
                return this._lastNameGen;
            }
            set
            {
                this._lastNameGen = (value ?? "").ForUI();
            }
        }

        public string NameDat
        {
            get
            {
                return "" + this.first_name_dat + " " + this.last_name_dat;
            }
        }

        public string first_name_dat
        {
            get
            {
                return this._firstNameDat;
            }
            set
            {
                this._firstNameDat = (value ?? "").ForUI();
            }
        }

        public string last_name_dat
        {
            get
            {
                return this._lastNameDat;
            }
            set
            {
                this._lastNameDat = (value ?? "").ForUI();
            }
        }

        public string deactivated { get; set; }

        public int blacklisted { get; set; }

        public int blacklisted_by_me { get; set; }

        public int is_favorite { get; set; }

        public int is_subscribed { get; set; }

        public string nickname { get; set; }

        public string screen_name { get; set; }

        public string description { get; set; }

        public int sex { get; set; }

        public bool IsFemale
        {
            get
            {
                return this.sex == 1;
            }
        }

        public string bdate
        {
            get
            {
                return this._bdate;
            }
            set
            {
                this._bdate = value;
                this._birthDate = null;
            }
        }

        public BirthDate BirthDate
        {
            get
            {
                return this._birthDate ?? (this._birthDate = new BirthDate(this));
            }
        }

        public string home_town
        {
            get
            {
                return this._home_town;
            }
            set
            {
                this._home_town = (value ?? "").ForUI();
            }
        }

        public string photo_50 { get; set; }

        public string photo_100 { get; set; }

        public string photo_200 { get; set; }

        public string photo_200_orig { get; set; }

        public string photo_400_orig { get; set; }

        public AudioObj status_audio { get; set; }

        public string status
        {
            get
            {
                return this._status;
            }
            set
            {
                this._status = (value ?? "").ForUI();
            }
        }

        public Occupation occupation { get; set; }

        public UserPersonal personal { get; set; }

        public int timezone { get; set; }

        public string photo_big
        {
            get
            {
                return this.photo_400_orig;
            }
            set
            {
                this.photo_400_orig = value;
            }
        }

        public int has_mobile { get; set; }

        public string mobile_phone { get; set; }

        public string home_phone { get; set; }

        public int university { get; set; }

        public string university_name { get; set; }

        public long university_group_id { get; set; }

        public int faculty { get; set; }

        public string faculty_name { get; set; }

        public int graduation { get; set; }

        public int online { get; set; }

        public string online_app { get; set; }

        public int online_mobile { get; set; }

        public int friend_status { get; set; }

        public int is_friend { get; set; }

        public int followers_count { get; set; }

        public int common_count { get; set; }

        public string wall_default { get; set; }

        public List<Relative> relatives { get; set; }

        public int verified { get; set; }

        public string photo_rec
        {
            get
            {
                return this.photo_50 ?? this.photo_100;
            }
            set
            {
                this.photo_50 = value;
            }
        }

        public bool IsFriend
        {
            get
            {
                return this.is_friend == 1;
            }
            set
            {
                this.is_friend = value ? 1 : 0;
            }
        }

        public string photo_max
        {
            get
            {
                if (this.id < -2000000000L)
                    return "/VKClient.Common;component/Resources/EmailUser.png";
                if (!string.IsNullOrEmpty(this._photo_max))
                    return this._photo_max;
                if (!string.IsNullOrEmpty(this.photo_200))
                    return this.photo_200;
                return this.photo_100;
            }
            set
            {
                this._photo_max = value;
            }
        }

        public int can_write_private_message { get; set; }

        public int is_messages_blocked { get; set; }

        public Exports exports { get; set; }

        public string activity
        {
            get
            {
                return this._activity;
            }
            set
            {
                this._activity = (value ?? "").ForUI();
            }
        }

        public UserStatus last_seen { get; set; }

        public int can_post { get; set; }

        public int can_see_all_posts { get; set; }

        public int can_see_audio { get; set; }

        public int can_send_friend_request { get; set; }

        public int can_see_gifts { get; set; }

        public int relation { get; set; }

        public User relation_partner { get; set; }

        public string activities
        {
            get
            {
                return this._activities;
            }
            set
            {
                this._activities = (value ?? "").ForUI();
            }
        }

        public string interests
        {
            get
            {
                return this._interests;
            }
            set
            {
                this._interests = (value ?? "").ForUI();
            }
        }

        public string music
        {
            get
            {
                return this._music;
            }
            set
            {
                this._music = (value ?? "").ForUI();
            }
        }

        public string movies
        {
            get
            {
                return this._movies;
            }
            set
            {
                this._movies = (value ?? "").ForUI();
            }
        }

        public string tv
        {
            get
            {
                return this._tv;
            }
            set
            {
                this._tv = (value ?? "").ForUI();
            }
        }

        public string books
        {
            get
            {
                return this._books;
            }
            set
            {
                this._books = (value ?? "").ForUI();
            }
        }

        public string games
        {
            get
            {
                return this._games;
            }
            set
            {
                this._games = (value ?? "").ForUI();
            }
        }

        public string quotes
        {
            get
            {
                return this._quotes;
            }
            set
            {
                this._quotes = (value ?? "").ForUI();
            }
        }

        public string about
        {
            get
            {
                return this._about;
            }
            set
            {
                this._about = (value ?? "").ForUI();
            }
        }

        public string site { get; set; }

        public Counters counters { get; set; }

        public City city { get; set; }

        public Country country { get; set; }

        public List<Career> career { get; set; }

        public List<Military> military { get; set; }

        public string skype
        {
            get
            {
                return this._skype;
            }
            set
            {
                this._skype = (value ?? "").ForUI();
            }
        }

        public string facebook
        {
            get
            {
                return this._facebook;
            }
            set
            {
                this._facebook = (value ?? "").ForUI();
            }
        }

        public string facebook_name
        {
            get
            {
                return this._facebookName;
            }
            set
            {
                this._facebookName = (value ?? "").ForUI();
            }
        }

        public string twitter
        {
            get
            {
                return this._twitter;
            }
            set
            {
                this._twitter = (value ?? "").ForUI();
            }
        }

        public string instagram
        {
            get
            {
                return this._instagram;
            }
            set
            {
                this._instagram = (value ?? "").ForUI();
            }
        }

        public List<University> universities { get; set; }

        public List<School> schools { get; set; }

        public CropPhoto crop_photo { get; set; }

        public string Name
        {
            get
            {
                return "" + this.first_name + " " + this.last_name;
            }
        }

        public string name
        {
            get
            {
                return this.Name;
            }
        }

        public string NameLink
        {
            get
            {
                return string.Format("[id{0}|{1}]", this.id, this.Name);
            }
        }

        public string contact { get; set; }

        public int contacts_import { get; set; }

        public BlockInformation ban_info { get; set; }

        public string role { get; set; }

        public CommunityManagementRole Role
        {
            get
            {
                string role = this.role;
                if (role == "moderator")
                    return CommunityManagementRole.Moderator;
                if (role == "editor")
                    return CommunityManagementRole.Editor;
                if (role == "administrator")
                    return CommunityManagementRole.Administrator;
                return role == "creator" ? CommunityManagementRole.Creator : CommunityManagementRole.Unknown;
            }
            set
            {
                switch (value)
                {
                    case CommunityManagementRole.Moderator:
                        this.role = "moderator";
                        break;
                    case CommunityManagementRole.Editor:
                        this.role = "editor";
                        break;
                    case CommunityManagementRole.Administrator:
                        this.role = "administrator";
                        break;
                    case CommunityManagementRole.Creator:
                        this.role = "creator";
                        break;
                    default:
                        this.role = null;
                        break;
                }
            }
        }

        public User()
        {
            this.last_seen = new UserStatus();
            this.counters = new Counters();
            this.exports = new Exports();

            this.domain = "";
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(9);
            writer.Write(this.uid);
            writer.WriteString(this.first_name);
            writer.WriteString(this.last_name);
            writer.WriteString(this.photo_rec);
            writer.WriteString(this.photo_max);
            writer.Write(this.sex);
            writer.Write<Exports>(this.exports, false);
            writer.WriteString(this.activity);
            writer.WriteString(this.photo_max);
            writer.WriteString(this.site);
            writer.WriteString(this.mobile_phone);
            writer.WriteString(this.home_phone);
            writer.WriteString(this.bdate);
            writer.WriteString(this.first_name_acc);
            writer.WriteString(this.last_name_acc);
            writer.WriteString(this.first_name_gen);
            writer.WriteString(this.last_name_gen);
            writer.WriteString(this.first_name_dat);
            writer.WriteString(this.last_name_dat);
            writer.Write(this.friend_status);
            writer.Write<BlockInformation>(this.ban_info, false);
            writer.WriteString(this.role);
            writer.WriteString(this.domain);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            this.uid = reader.ReadInt64();
            this.first_name = reader.ReadString();
            this.last_name = reader.ReadString();
            this.photo_rec = reader.ReadString();
            this.photo_max = reader.ReadString();
            this.sex = reader.ReadInt32();
            this.exports = reader.ReadGeneric<Exports>();
            this.activity = reader.ReadString();
            int num2 = 2;
            if (num1 >= num2)
            {
                this.photo_max = reader.ReadString();
                this.site = reader.ReadString();
                this.mobile_phone = reader.ReadString();
                this.home_phone = reader.ReadString();
                this.bdate = reader.ReadString();
            }
            int num3 = 3;
            if (num1 >= num3)
            {
                this.first_name_acc = reader.ReadString();
                this.last_name_acc = reader.ReadString();
            }
            int num4 = 4;
            if (num1 >= num4)
            {
                this.first_name_gen = reader.ReadString();
                this.last_name_gen = reader.ReadString();
            }
            int num5 = 5;
            if (num1 >= num5)
            {
                this.first_name_dat = reader.ReadString();
                this.last_name_dat = reader.ReadString();
            }
            int num6 = 6;
            if (num1 >= num6)
                this.friend_status = reader.ReadInt32();
            int num7 = 7;
            if (num1 >= num7)
                this.ban_info = reader.ReadGeneric<BlockInformation>();
            int num8 = 8;
            if (num1 >= num8)
                this.role = reader.ReadString();
            int num9 = 9;
            if (num1 < num9)
                return;
            this.domain = reader.ReadString();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", this.uid, this.first_name, this.last_name, this.online);
        }
    }
}
