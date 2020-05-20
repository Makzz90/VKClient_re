using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public class SearchHintHeader : IHaveUniqueKey, ISearchableItemHeader<SearchHint>
  {
    private string _key;
    private SearchHint _searchHint;

    public bool IsCommunityType { get; private set; }

    public bool IsUserType { get; private set; }

    public bool IsExtendedSearchType { get; private set; }

    public bool IsInternalLinkType { get; set; }

    public long Id { get; private set; }

    public string Photo { get; private set; }

    public string Title { get; private set; }

    public string Subtitle { get; private set; }

    public Visibility SubtitleVisibility { get; private set; }

    public Visibility VerifiedVisibility { get; private set; }

    public Visibility OnlineVisibility { get; private set; }

    public Visibility OnlineMobileVisibility { get; private set; }

    public Visibility ExtendedSearchPhotoVisibility { get; private set; }

    public Visibility LinkPhotoVisibility { get; private set; }

    public double TitleWidth { get; private set; }

    public bool IsGlobal { get; private set; }

    public SearchHint SearchHint
    {
      get
      {
        return this._searchHint;
      }
    }

    public bool IsLocalItem
    {
      get
      {
        return !this.IsGlobal;
      }
    }

    public SearchHintHeader(SearchHint searchHint)
    {
      this.SetSearchHint(searchHint);
    }

    public string GetKey()
    {
      return this._key;
    }

    private void SetSearchHint(SearchHint searchHint)
    {
      this._searchHint = searchHint;
      this.VerifiedVisibility = Visibility.Collapsed;
      this.OnlineVisibility = Visibility.Collapsed;
      this.OnlineMobileVisibility = Visibility.Collapsed;
      this.ExtendedSearchPhotoVisibility = Visibility.Collapsed;
      this.LinkPhotoVisibility = Visibility.Collapsed;
      if (this._searchHint == null)
        return;
      this.IsCommunityType = this._searchHint.group != null && this._searchHint.type == "group";
      this.IsUserType = this._searchHint.profile != null && this._searchHint.type == "profile";
      this.IsExtendedSearchType = this._searchHint.type == "extended_search";
      this.IsInternalLinkType = this._searchHint.type == "internal_link";
      this.Id = this.GetId();
      this.Photo = this.GetPhoto();
      this.Title = this.GetTitle();
      this.Subtitle = this.GetSubtitle();
      this.SubtitleVisibility = this.GetSubtitleVisibility();
      this.VerifiedVisibility = this.GetVerifiedVisiblity();
      this.OnlineVisibility = this.GetOnlineVisiblity();
      this.OnlineMobileVisibility = this.GetOnlineMobileVisibility();
      this.ExtendedSearchPhotoVisibility = this.GetExtendedSearchPhotoVisibility();
      this.LinkPhotoVisibility = this.GetLinkPhotoVisibility();
      this.TitleWidth = this.GetTitleWidth();
      this.IsGlobal = this.GetIsGlobal();
      this._key = this.UpdateKey();
    }

    private long GetId()
    {
      if (this.IsCommunityType)
        return this._searchHint.group.id;
      if (this.IsUserType)
        return this._searchHint.profile.id;
      return 0;
    }

    private string GetPhoto()
    {
      if (this.IsCommunityType)
        return this._searchHint.group.photo_max;
      if (this.IsUserType)
        return this._searchHint.profile.photo_max;
      return "";
    }

    private string GetTitle()
    {
      if (this.IsCommunityType)
        return this._searchHint.group.name;
      if (this.IsUserType)
        return this._searchHint.profile.Name;
      if (this.IsExtendedSearchType)
        return CommonResources.FindFriends_Search_Title.ToLowerInvariant();
      if (this.IsInternalLinkType)
        return CommonResources.Link.ToLowerInvariant();
      return "";
    }

    private string GetSubtitle()
    {
      return this._searchHint.description;
    }

    private Visibility GetSubtitleVisibility()
    {
      return string.IsNullOrEmpty(this._searchHint.description) ? Visibility.Collapsed : Visibility.Visible;
    }

    private Visibility GetVerifiedVisiblity()
    {
      if (this.IsCommunityType)
        return this._searchHint.group.verified != 1 ? Visibility.Collapsed : Visibility.Visible;
      return this.IsUserType && this._searchHint.profile.verified == 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private Visibility GetOnlineVisiblity()
    {
      return this.IsUserType && this._searchHint.profile.online == 1 && this._searchHint.profile.online_mobile == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private Visibility GetOnlineMobileVisibility()
    {
      return this._searchHint != null && this.IsUserType && this._searchHint.profile.online_mobile == 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private Visibility GetExtendedSearchPhotoVisibility()
    {
      return !this.IsExtendedSearchType ? Visibility.Collapsed : Visibility.Visible;
    }

    private Visibility GetLinkPhotoVisibility()
    {
      return !this.IsInternalLinkType ? Visibility.Collapsed : Visibility.Visible;
    }

    private double GetTitleWidth()
    {
      int num = 374;
      if (this.IsUserType)
      {
        if (this._searchHint.profile.online == 1 || this._searchHint.profile.online_mobile == 1)
          num -= 26;
        if (this._searchHint.profile.verified == 1)
          num -= 32;
      }
      else if (this.IsCommunityType && this._searchHint.group.verified == 1)
        num -= 32;
      return (double) num;
    }

    private bool GetIsGlobal()
    {
      return this._searchHint.global == 1;
    }

    private string UpdateKey()
    {
      if (this._searchHint.group != null)
        return "group_" + (object) this._searchHint.group.id;
      if (this._searchHint.profile != null)
        return "user_" + (object) this._searchHint.profile.id;
      if (this._searchHint.type == "extended_search")
        return "extended_search";
      return this._searchHint.type == "internal_link" ? "internal_link" : "";
    }

    public bool Matches(string searchString)
    {
      MatchStrings matchStrings = TransliterationHelper.GetMatchStrings(searchString);
      return this.MatchesAny(matchStrings.SearchStrings, matchStrings.LatinStrings, matchStrings.CyrillicStrings);
    }

    internal bool MatchesAny(List<string> searchStrings, List<string> searchStringsLatin, List<string> searchStringsCyrillic)
    {
      if (!this.Matches((IList<string>) searchStrings) && !this.Matches((IList<string>) searchStringsLatin))
        return this.Matches((IList<string>) searchStringsCyrillic);
      return true;
    }

    public bool Matches(IList<string> searchStrings)
    {
      if (this._searchHint == null)
        return false;
      User profile = this._searchHint.profile;
      Group group = this._searchHint.group;
      string str1 = "";
      string str2;
      if (profile != null)
      {
        str2 = profile.first_name;
        str1 = profile.last_name;
      }
      else
      {
        if (group == null)
          return false;
        str2 = group.name;
      }
      if (searchStrings.Count == 0 || searchStrings.All<string>(new Func<string, bool>(string.IsNullOrWhiteSpace)))
        return false;
      bool flag = true;
      foreach (string searchString in (IEnumerable<string>) searchStrings)
      {
        flag = !string.IsNullOrEmpty(str2) && str2.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase) || !string.IsNullOrEmpty(str1) && str1.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase);
        if (!flag)
          break;
      }
      return flag;
    }
  }
}
