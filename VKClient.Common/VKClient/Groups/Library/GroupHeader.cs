using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Groups.Library
{
  public class GroupHeader : ViewModelBase, IHaveUniqueKey, ISearchableItemHeader<Group>, IHandle<CommunityInformationChanged>, IHandle, IHandle<CommunityPhotoChanged>
  {
    private ObservableCollection<MenuItemData> _acceptDeclineMenuItems = new ObservableCollection<MenuItemData>();
    private Group _group;
    private User _userInvited;
    private string[] _groupNameSplit;
    private bool _isSelected;

    public User UserInvited
    {
      get
      {
        return this._userInvited;
      }
    }

    public ObservableCollection<MenuItemData> AcceptDeclineMenuItems
    {
      get
      {
        return this._acceptDeclineMenuItems;
      }
    }

    public Group Group
    {
      get
      {
        return this._group;
      }
    }

    public bool PastEvent
    {
      get
      {
        return Extensions.UnixTimeStampToDateTime((double) this._group.start_date, false) < DateTime.Now;
      }
    }

    public string Src
    {
      get
      {
        return this._group.photo_200;
      }
    }

    public string Title
    {
      get
      {
        return this._group.name;
      }
    }

    public Visibility VerificationVisibility
    {
      get
      {
        if (this._group.verified != 1)
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
        if (!string.IsNullOrWhiteSpace(this._group.activity) && this._group.GroupType == GroupType.PublicPage)
          return this._group.activity;
        switch (this._group.GroupType)
        {
          case GroupType.Group:
            return GroupHeader.GetGroupTypeText(this._group);
          case GroupType.PublicPage:
            return CommonResources.PublicPage;
          case GroupType.Event:
            return this.GetEventDate();
          default:
            return "";
        }
      }
    }

    public string Subtitle2
    {
      get
      {
        if (this._group.members_count <= 0)
          return "";
        switch (this._group.GroupType)
        {
          case GroupType.Group:
          case GroupType.Event:
            return UIStringFormatterHelper.FormatNumberOfSomething(this._group.members_count, CommonResources.OneMemberFrm, CommonResources.TwoFourMembersFrm, CommonResources.FiveMembersFrm, true,  null, false);
          case GroupType.PublicPage:
            return UIStringFormatterHelper.FormatNumberOfSomething(this._group.members_count, CommonResources.OneSubscriberFrm, CommonResources.TwoFourSubscribersFrm, CommonResources.FiveSubscribersFrm, true,  null, false);
          default:
            return "";
        }
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

    public bool IsLocalItem
    {
      get
      {
        return this._group.IsMember;
      }
    }

    public GroupHeader()
    {
      EventAggregator.Current.Subscribe(this);
    }

    public GroupHeader(Group group, User invitedBy = null)
      : this()
    {
      this._group = group;
      this._userInvited = invitedBy;
      string[] strArray;
      if (this._group.name == null)
        strArray = new string[0];
      else
        strArray = this._group.name.Split((char[]) new char[1]
        {
          ' '
        });
      this._groupNameSplit = strArray;
      this.CreateMenuItems();
    }

    public static string GetGroupTypeText(Group group)
    {
      if (group == null)
        return "";
      switch (group.Privacy)
      {
        case GroupPrivacy.Public:
          return CommonResources.PublicGroup.ToUpper()[0].ToString() + CommonResources.PublicGroup.ToLower().Substring(1);
        case GroupPrivacy.Closed:
          return CommonResources.ClosedGroup.ToUpper()[0].ToString() + CommonResources.ClosedGroup.ToLower().Substring(1);
        case GroupPrivacy.Private:
          return CommonResources.PrivateGroup.ToUpper()[0].ToString() + CommonResources.PrivateGroup.ToLower().Substring(1);
        default:
          return "";
      }
    }

    private string GetEventDate()
    {
      return UIStringFormatterHelper.FormatDateForMessageUI(Extensions.UnixTimeStampToDateTime((double) this._group.start_date, false));
    }

    public bool Matches(string searchString)
    {
      MatchStrings matchStrings = TransliterationHelper.GetMatchStrings(searchString);
      return this.MatchesAny(matchStrings.SearchStrings, matchStrings.LatinStrings, matchStrings.CyrillicStrings);
    }

    public bool MatchesAny(List<string> searchStrings, List<string> searchStringsLatin, List<string> searchStringsCyrillic)
    {
      if (!this.Matches(searchStrings) && !this.Matches(searchStringsLatin))
        return this.Matches(searchStringsCyrillic);
      return true;
    }

    private bool Matches(List<string> searchStrings)
    {
      // ISSUE: method pointer
        if (searchStrings.Count == 0 || Enumerable.All<string>(searchStrings, (Func<string, bool>)new Func<string, bool>(string.IsNullOrWhiteSpace)))
        return false;
      bool flag1 = true;
      List<string>.Enumerator enumerator = searchStrings.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          string current = enumerator.Current;
          bool flag2 = false;
          foreach (string str in this._groupNameSplit)
          {
            flag2 = str.StartsWith(current, (StringComparison) 3);
            if (flag2)
              break;
          }
          flag1 &= flag2;
          if (!flag1)
            break;
        }
      }
      finally
      {
        enumerator.Dispose();
      }
      return flag1;
    }

    private void CreateMenuItems()
    {
      if (this._group.GroupType == GroupType.Event)
      {
        ((Collection<MenuItemData>) this._acceptDeclineMenuItems).Add(new MenuItemData()
        {
          Title = CommonResources.EventJoin,
          Tag = "2"
        });
        ((Collection<MenuItemData>) this._acceptDeclineMenuItems).Add(new MenuItemData()
        {
          Title = CommonResources.EventMaybe,
          Tag = "1"
        });
        ((Collection<MenuItemData>) this._acceptDeclineMenuItems).Add(new MenuItemData()
        {
          Title = CommonResources.EventDecline,
          Tag = "0"
        });
      }
      else
      {
        ((Collection<MenuItemData>) this._acceptDeclineMenuItems).Add(new MenuItemData()
        {
          Title = CommonResources.AcceptInvitation,
          Tag = "2"
        });
        ((Collection<MenuItemData>) this._acceptDeclineMenuItems).Add(new MenuItemData()
        {
          Title = CommonResources.DeclineInvitation,
          Tag = "0"
        });
      }
    }

    public string GetKey()
    {
      if (this._group == null)
        return "";
      return (-this._group.id).ToString();
    }

    public void Handle(CommunityInformationChanged message)
    {
      if (message.Id != this._group.id)
        return;
      this._group.name = message.Name;
      this._group.activity = message.SubcategoryName ?? message.CategoryName;
      this._group.start_date = Extensions.DateTimeToUnixTimestamp(message.EventStartDate.ToUniversalTime(), false);
      this._group.Privacy = message.Privacy;
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.Title);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.Subtitle);
    }

    public void Handle(CommunityPhotoChanged message)
    {
      if (message.CommunityId != this._group.id)
        return;
      this._group.photo_200 = message.PhotoMax;
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.Src);
    }
      //
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
