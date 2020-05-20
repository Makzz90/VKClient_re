using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Users.ViewModels
{
  public class UserFullInfoViewModel : ProfileInfoFullViewModel
  {
    private readonly UserData _userData;

    public override string Name
    {
      get
      {
        return this._userData.Name.ToUpperInvariant();
      }
    }

    public UserFullInfoViewModel(UserData userData)
    {
      this._userData = userData;
      this.CreateData();
    }

    private void CreateData()
    {
      if (this._userData == null)
        return;
      User user = this._userData.user;
      if (!string.IsNullOrEmpty(this._userData.Activity) || this._userData.AdminLevel > 1)
        this.InfoSections.Add(new ProfileInfoSectionItem()
        {
          Items = new List<ProfileInfoItem>()
          {
            (ProfileInfoItem) new StatusItem((IProfileData) this._userData)
          }
        });
      List<ProfileInfoItem> profileInfoItemList1 = new List<ProfileInfoItem>();
      if (!string.IsNullOrEmpty(user.bdate))
        profileInfoItemList1.Add((ProfileInfoItem) new BirthdayItem(this._userData));
      if (!string.IsNullOrEmpty(user.home_town))
        profileInfoItemList1.Add((ProfileInfoItem) new HomeTownItem(user.home_town));
      Occupation occupation = user.occupation;
      if (occupation != null)
      {
        string str = occupation.name;
        if (occupation.type == OccupationType.university && user.universities != null)
        {
          University university = user.universities.FirstOrDefault<University>((Func<University, bool>) (u => u.id == occupation.id));
          if (university != null && university.graduation > 0)
            str = string.Format("{0} '{1:00}", (object) university.name, (object) (university.graduation % 100));
        }
        else if (occupation.type == OccupationType.school && user.schools != null)
        {
          foreach (School school in user.schools)
          {
            long result;
            if (long.TryParse(school.id, out result) && result == occupation.id && school.year_graduated > 0)
              str = string.Format("{0} '{1:00}", (object) school.name, (object) (school.year_graduated % 100));
          }
        }
        occupation.name = str;
        profileInfoItemList1.Add((ProfileInfoItem) OccupationItem.GetOccupationItem(occupation, this._userData.occupationGroup));
      }
      if (this._userData.user.relation != 0)
        profileInfoItemList1.Add((ProfileInfoItem) new RelationshipItem(this._userData));
      if (user.personal != null && !user.personal.langs.IsNullOrEmpty())
        profileInfoItemList1.Add((ProfileInfoItem) new LanguagesItem(this._userData));
      if (user.relatives != null && user.relatives.Count > 0)
      {
        List<Relative> relatives1 = user.relatives;
        List<User> relatives2 = this._userData.relatives;
        List<Relative> list1 = relatives1.Where<Relative>((Func<Relative, bool>) (r => r.type == RelativeType.grandparent)).ToList<Relative>();
        if (!list1.IsNullOrEmpty())
          profileInfoItemList1.Add((ProfileInfoItem) new GrandparentsItem((IEnumerable<Relative>) list1, relatives2));
        List<Relative> list2 = relatives1.Where<Relative>((Func<Relative, bool>) (r => r.type == RelativeType.parent)).ToList<Relative>();
        if (!list2.IsNullOrEmpty())
          profileInfoItemList1.Add((ProfileInfoItem) new ParentsItem((IEnumerable<Relative>) list2, relatives2));
        List<Relative> list3 = relatives1.Where<Relative>((Func<Relative, bool>) (r => r.type == RelativeType.sibling)).ToList<Relative>();
        if (!list3.IsNullOrEmpty())
          profileInfoItemList1.Add((ProfileInfoItem) new SiblingsItem((IEnumerable<Relative>) list3, relatives2));
        List<Relative> list4 = relatives1.Where<Relative>((Func<Relative, bool>) (r => r.type == RelativeType.child)).ToList<Relative>();
        if (!list4.IsNullOrEmpty())
          profileInfoItemList1.Add((ProfileInfoItem) new ChildrenItem((IEnumerable<Relative>) list4, relatives2));
        List<Relative> list5 = relatives1.Where<Relative>((Func<Relative, bool>) (r => r.type == RelativeType.grandchild)).ToList<Relative>();
        if (!list5.IsNullOrEmpty())
          profileInfoItemList1.Add((ProfileInfoItem) new GrandchildrenItem((IEnumerable<Relative>) list5, relatives2));
      }
      if (profileInfoItemList1.Count > 0)
        this.InfoSections.Add(new ProfileInfoSectionItem()
        {
          Items = profileInfoItemList1
        });
      List<ProfileInfoItem> profileInfoItemList2 = new List<ProfileInfoItem>();
      profileInfoItemList2.AddRange((IEnumerable<ProfileInfoItem>) PhoneItem.GetPhones(this._userData));
      if (this._userData.city != null)
        profileInfoItemList2.Add((ProfileInfoItem) new CityItem(this._userData.city.name));
      profileInfoItemList2.Add((ProfileInfoItem) new VKSocialNetworkItem((IProfileData) this._userData));
      if (!string.IsNullOrEmpty(user.skype))
        profileInfoItemList2.Add((ProfileInfoItem) new SkypeSocialNetworkItem(user.skype));
      if (!string.IsNullOrEmpty(user.facebook))
        profileInfoItemList2.Add((ProfileInfoItem) new FacebookSocialNetworkItem(user.facebook, user.facebook_name));
      if (!string.IsNullOrEmpty(user.twitter))
        profileInfoItemList2.Add((ProfileInfoItem) new TwitterSocialNetworkItem(user.twitter));
      if (!string.IsNullOrEmpty(user.instagram))
        profileInfoItemList2.Add((ProfileInfoItem) new InstagramSocialNetworkItem(user.instagram));
      if (!string.IsNullOrEmpty(user.site))
        profileInfoItemList2.Add((ProfileInfoItem) new SiteItem(user.site));
      if (profileInfoItemList2.Count > 0)
        this.InfoSections.Add(new ProfileInfoSectionItem(CommonResources.ProfilePage_Info_ContactInformation)
        {
          Items = profileInfoItemList2
        });
      List<ProfileInfoItem> profileInfoItemList3 = new List<ProfileInfoItem>();
      profileInfoItemList3.AddRange((IEnumerable<ProfileInfoItem>) UniversityItem.GetUniversities(user.universities, this._userData.universityGroups));
      profileInfoItemList3.AddRange((IEnumerable<ProfileInfoItem>) SchoolItem.GetSchools(user.schools, this._userData.schoolGroups));
      if (profileInfoItemList3.Count > 0)
        this.InfoSections.Add(new ProfileInfoSectionItem(CommonResources.ProfilePage_Info_Education.ToUpperInvariant())
        {
          Items = profileInfoItemList3
        });
      List<ProfileInfoItem> profileInfoItemList4 = new List<ProfileInfoItem>();
      UserPersonal personal = user.personal;
      if (personal != null)
      {
        if (personal.political > 0 && personal.political <= 9)
          profileInfoItemList4.Add((ProfileInfoItem) new PoliticalViewsItem(personal.political));
        if (!string.IsNullOrEmpty(personal.religion))
          profileInfoItemList4.Add((ProfileInfoItem) new WorldViewItem(personal.religion));
        if (personal.life_main > 0 && personal.life_main <= 8)
          profileInfoItemList4.Add((ProfileInfoItem) new PersonalPriorityItem(personal.life_main));
        if (personal.people_main > 0 && personal.people_main <= 6)
          profileInfoItemList4.Add((ProfileInfoItem) new ImportantInOthersItem(personal.people_main));
        if (personal.smoking > 0 && personal.smoking <= 5)
          profileInfoItemList4.Add((ProfileInfoItem) new BadHabitsItem(CommonResources.ProfilePage_Info_ViewsOnSmoking, personal.smoking));
        if (personal.alcohol > 0 && personal.alcohol <= 5)
          profileInfoItemList4.Add((ProfileInfoItem) new BadHabitsItem(CommonResources.ProfilePage_Info_ViewsOnAlcohol, personal.alcohol));
        if (!string.IsNullOrEmpty(personal.inspired_by))
          profileInfoItemList4.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_InspiredBy, personal.inspired_by, null, ProfileInfoItemType.RichText));
      }
      if (profileInfoItemList4.Count > 0)
        this.InfoSections.Add(new ProfileInfoSectionItem(CommonResources.ProfilePage_Info_Beliefs)
        {
          Items = profileInfoItemList4
        });
      List<ProfileInfoItem> profileInfoItemList5 = new List<ProfileInfoItem>();
      if (!string.IsNullOrEmpty(user.activities))
        profileInfoItemList5.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_Activities, user.activities, null, ProfileInfoItemType.RichText));
      if (!string.IsNullOrEmpty(user.interests))
        profileInfoItemList5.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_Interests, user.interests, null, ProfileInfoItemType.RichText));
      if (!string.IsNullOrEmpty(user.music))
        profileInfoItemList5.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_Music, user.music, null, ProfileInfoItemType.RichText));
      if (!string.IsNullOrEmpty(user.movies))
        profileInfoItemList5.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_Movies, user.movies, null, ProfileInfoItemType.RichText));
      if (!string.IsNullOrEmpty(user.tv))
        profileInfoItemList5.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_TV, user.tv, null, ProfileInfoItemType.RichText));
      if (!string.IsNullOrEmpty(user.books))
        profileInfoItemList5.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_Books, user.books, null, ProfileInfoItemType.RichText));
      if (!string.IsNullOrEmpty(user.games))
        profileInfoItemList5.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_Games, user.games, null, ProfileInfoItemType.RichText));
      if (!string.IsNullOrEmpty(user.quotes))
        profileInfoItemList5.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_Quotes, user.quotes, null, ProfileInfoItemType.RichText));
      if (!string.IsNullOrEmpty(user.about))
        profileInfoItemList5.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_About, user.about, null, ProfileInfoItemType.RichText));
      if (profileInfoItemList5.Count > 0)
        this.InfoSections.Add(new ProfileInfoSectionItem(CommonResources.ProfilePage_Info_PersonalInfomation)
        {
          Items = profileInfoItemList5
        });
      List<ProfileInfoItem> profileInfoItemList6 = new List<ProfileInfoItem>();
      if (!user.military.IsNullOrEmpty())
      {
        profileInfoItemList6.AddRange((IEnumerable<ProfileInfoItem>) MilitaryItem.GetMilitaryItems(user.military, this._userData.militaryCountries));
        if (profileInfoItemList6.Count > 0)
          this.InfoSections.Add(new ProfileInfoSectionItem(CommonResources.ProfilePage_Info_MilitaryService)
          {
            Items = profileInfoItemList6
          });
      }
      List<ProfileInfoItem> profileInfoItemList7 = new List<ProfileInfoItem>();
      CareerData careerData = this._userData.careerData;
      if (careerData != null && !careerData.Items.IsNullOrEmpty())
      {
        profileInfoItemList7.AddRange((IEnumerable<ProfileInfoItem>) CareerItem.GetCareerItems(careerData.Items, careerData.Cities, careerData.Groups));
        if (profileInfoItemList7.Count > 0)
          this.InfoSections.Add(new ProfileInfoSectionItem(CommonResources.ProfilePage_Info_Career)
          {
            Items = profileInfoItemList7
          });
      }
      if (this.InfoSections.Count <= 0)
        return;
      this.InfoSections.Last<ProfileInfoSectionItem>().DividerVisibility = Visibility.Collapsed;
    }
  }
}
