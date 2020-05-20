using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public class FriendHeader : ViewModelBase, IHaveUniqueKey, ISearchableItemHeader<User>
    {
        public static List<List<string>> _frListColors = new List<List<string>>() { new List<string>() { "c8e6c9", "73ba77" }, new List<string>() { "b6dedb", "50b3a9" }, new List<string>() { "fceab1", "e6c153" }, new List<string>() { "e6dad5", "ba9d93" }, new List<string>() { "ffccbc", "f09073" }, new List<string>() { "c4e5f5", "69b7db" }, new List<string>() { "c8cce6", "8b95cc" }, new List<string>() { "e3c8e8", "be8bc7" } };
        private User _user;
        private Group _group;
        private FriendsList _friendsList;
        private bool _isInSelectedState;
        private bool _isSelected;
        private char? _initial;
        private string _hint;

        public FriendsList FriendsList
        {
            get
            {
                return this._friendsList;
            }
        }

        public bool IsFriendList
        {
            get
            {
                return this._friendsList != null;
            }
        }

        public User User
        {
            get
            {
                return this._user;
            }
        }

        public bool IsGroupHeader
        {
            get
            {
                return this._group != null;
            }
        }

        public long GroupId
        {
            get
            {
                return this._group.id;
            }
        }

        public Visibility IsNotListVisibility
        {
            get
            {
                if (this._friendsList != null)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility IsListVisibility
        {
            get
            {
                if (this._friendsList == null)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public SolidColorBrush PlaceholderFill
        {
            get
            {
                if (this._friendsList == null)
                    return Application.Current.Resources["PhoneChromeBrush"] as SolidColorBrush;
                long num = Math.Abs(this._friendsList.lid);
                return ("#FF" + FriendHeader._frListColors[(int)(num % 8L)][0]).GetColor();
            }
        }

        public SolidColorBrush FriendListBackground
        {
            get
            {
                if (this._friendsList == null)
                    return new SolidColorBrush(Colors.Transparent);
                long num = Math.Abs(this._friendsList.lid);
                return ("#FF" + FriendHeader._frListColors[(int)(num % 8L)][1]).GetColor();
            }
        }

        public bool IsFriend { get; set; }

        public bool IsMenuEnabled { get; private set; }

        public bool IsInSelectedState
        {
            get
            {
                return this._isInSelectedState;
            }
            set
            {
                if (this._isInSelectedState == value)
                    return;
                this._isInSelectedState = value;
                this.NotifyPropertyChanged<bool>(() => this.IsInSelectedState);
                this.NotifyPropertyChanged<Visibility>(() => this.SelectionStateVisibility);
            }
        }

        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                if (this._isSelected == value)
                    return;
                this._isSelected = value;
                this.NotifyPropertyChanged<bool>(() => this.IsSelected);
            }
        }

        public Visibility SelectionStateVisibility
        {
            get
            {
                if (!this.IsInSelectedState)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public long UserId
        {
            get
            {
                if (this._user != null)
                    return this._user.uid;
                return 0;
            }
        }

        public long FriendListId
        {
            get
            {
                if (this._friendsList != null)
                    return this._friendsList.id;
                return 0;
            }
        }

        public string ImageUrl
        {
            get
            {
                if (this._user != null)
                    return this._user.photo_max;
                if (this._group != null)
                    return this._group.photo_200;
                if (this._friendsList == null)
                    return "";
                return MultiResolutionHelper.Instance.AppendResolutionSuffix("/Resources/New/PlaceholderGroup62Light.png", true, "");
            }
        }

        public string FullName
        {
            get
            {
                if (this._user != null)
                    return this._user.Name;
                if (this._group != null)
                    return this._group.name;
                if (this._friendsList == null)
                    return "";
                return this._friendsList.name;
            }
        }

        public string LastName
        {
            get
            {
                if (this._user == null)
                    return "";
                return this._user.last_name ?? "";
            }
        }

        public string FullNameGen
        {
            get
            {
                if (this._user != null)
                    return this._user.NameGen;
                if (this._friendsList == null)
                    return "";
                return this._friendsList.name;
            }
        }

        public Visibility IsOnline
        {
            get
            {
                if (this._user == null)
                    return Visibility.Collapsed;
                if (this._user.online != 1 || this._user.online_mobile != 0)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility IsOnlineMobile
        {
            get
            {
                if (this._user == null)
                    return Visibility.Collapsed;
                if (this._user.online_mobile != 1)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public char? Initial
        {
            get
            {
                return this._initial;
            }
            set
            {
                this._initial = value;
            }
        }

        public Visibility VerificationVisibility
        {
            get
            {
                if (this._user == null || this._user.verified != 1)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility SubtitleVisibility
        {
            get
            {
                if (string.IsNullOrEmpty(this.Subtitle))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility Subtitle2Visibility
        {
            get
            {
                if (string.IsNullOrEmpty(this.Subtitle2))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public string Subtitle
        {
            get
            {
                User user = this._user;
                if (string.IsNullOrWhiteSpace(user != null ? user.status : null))
                    return CommonResources.PersonalProfile;
                return this._user.status;
            }
        }

        public string Subtitle2
        {
            get
            {
                if (this._user != null && this._user.followers_count > 0)
                    return UIStringFormatterHelper.FormatNumberOfSomething(this._user.followers_count, CommonResources.OneSubscriberFrm, CommonResources.TwoFourSubscribersFrm, CommonResources.FiveSubscribersFrm, true, null, false);
                return "";
            }
        }

        public bool IsLocalItem
        {
            get
            {
                return false;
            }
        }

        public string Hint
        {
            get
            {
                return this._hint;
            }
            set
            {
                this._hint = value;
                this.NotifyPropertyChanged<string>(() => this.Hint);
            }
        }

        public string Occupation
        {
            get
            {
                User user = this._user;
                string str;
                if (user == null)
                {
                    str = null;
                }
                else
                {
                    VKClient.Common.Backend.DataObjects.Occupation occupation = user.occupation;
                    if (occupation == null)
                    {
                        str = null;
                    }
                    else
                    {
                        string name = occupation.name;
                        str = name != null ? Extensions.ForUI(name) : null;
                    }
                }
                return str ?? "";
            }
        }

        public Visibility OccupationVisibility
        {
            get
            {
                return (!string.IsNullOrWhiteSpace(this.Occupation)).ToVisiblity();
            }
        }

        public string Domain
        {
            get
            {
                User user = this._user;
                string str = user != null ? user.domain : null;
                if (string.IsNullOrWhiteSpace(str) || str == "id" + this._user.id)
                    return "";
                return "@" + str;
            }
        }

        public FriendHeader(User user, bool isMenuEnabled = false)
        {
            this._user = user;
            this.BindToUser();
            this.IsFriend = true;
            this.IsMenuEnabled = isMenuEnabled;
        }

        public FriendHeader(Group group)
        {
            this._group = group;
        }

        public FriendHeader(FriendsList cachedAllowedList)
        {
            this._friendsList = cachedAllowedList;
        }

        public bool Matches(IList<string> searchStrings)
        {
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: method pointer
            if (this._user == null || searchStrings.Count == 0 || Enumerable.All<string>(searchStrings, new Func<string, bool>((s) => { return string.IsNullOrWhiteSpace(s); })))
                return false;
            bool flag1 = true;
            IEnumerator<string> enumerator = searchStrings.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    string current = enumerator.Current;
                    bool flag2 = !string.IsNullOrEmpty(this._user.first_name) && this._user.first_name.StartsWith(current, (StringComparison)3) || !string.IsNullOrEmpty(this._user.last_name) && this._user.last_name.StartsWith(current, (StringComparison)3);
                    flag1 &= flag2;
                    if (!flag1)
                        break;
                }
            }
            finally
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
            return flag1;
        }

        internal void BindToUser()
        {
            if (this._user == null)
                return;
            if (AppGlobalStateManager.Current.GlobalState.FriendListOrder == 0)
                this._initial = new char?(string.IsNullOrEmpty(this.FullName) ? ' ' : (this.FullName.ToLower()).First<char>());
            else
                this._initial = new char?(string.IsNullOrEmpty(this._user.last_name) ? ' ' : (this._user.last_name.ToLower()).First<char>());
        }

        public bool Matches(string searchString)
        {
            MatchStrings matchStrings = TransliterationHelper.GetMatchStrings(searchString);
            return this.MatchesAny(matchStrings.SearchStrings, matchStrings.LatinStrings, matchStrings.CyrillicStrings);
        }

        internal bool MatchesAny(List<string> searchStrings, List<string> searchStringsLatin, List<string> searchStringsCyrillic)
        {
            if (!this.Matches((IList<string>)searchStrings) && !this.Matches((IList<string>)searchStringsLatin))
                return this.Matches((IList<string>)searchStringsCyrillic);
            return true;
        }

        public string GetKey()
        {
            if (this._user != null)
                return this._user.id.ToString();
            if (this._group != null)
                return (-this._group.id).ToString();
            if (this._friendsList != null)
                return "l" + this._friendsList.id;
            return "";
        }
        //
        internal double px_per_tick = 64.0 / 10.0 / 2.0;

        public double UserAvatarRadius
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * px_per_tick;
            }
        }
        //
    }
}
