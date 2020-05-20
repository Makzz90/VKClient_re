using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Groups.ViewModels
{
  public class GroupFullInfoViewModel : ProfileInfoFullViewModel
  {
    private readonly GroupData _groupData;

    public override string Name
    {
      get
      {
        return this._groupData.Name.ToUpperInvariant();
      }
    }

    public GroupFullInfoViewModel(GroupData groupData)
    {
      this._groupData = groupData;
      this.CreateData();
    }

    private void CreateData()
    {
      if (this._groupData == null)
        return;
      Group group = this._groupData.group;
      if (!string.IsNullOrEmpty(this._groupData.Activity) || this._groupData.AdminLevel > 1)
        this.InfoSections.Add(new ProfileInfoSectionItem()
        {
          Items = new List<ProfileInfoItem>()
          {
            (ProfileInfoItem) new StatusItem((IProfileData) this._groupData)
          }
        });
      List<ProfileInfoItem> profileInfoItemList1 = new List<ProfileInfoItem>();
      if (!string.IsNullOrEmpty(group.description))
        profileInfoItemList1.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_Description, group.description,  null, ProfileInfoItemType.RichText));
      if (group.start_date > 0 && group.GroupType == GroupType.Event)
        profileInfoItemList1.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_StartDate, UIStringFormatterHelper.FormatDateTimeForUI(group.start_date), (Action) (() => this.CreateAppointment(group.start_date, group.finish_date, group.name, group.place != null ? group.place.address : "")), ProfileInfoItemType.RichText));
      if (group.finish_date > 0)
        profileInfoItemList1.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_FinishDate, UIStringFormatterHelper.FormatDateTimeForUI(group.finish_date), (Action) (() => this.CreateAppointment(group.start_date, group.finish_date, group.name, group.place != null ? group.place.address : "")), ProfileInfoItemType.RichText));
      string description = "";
      if (group.place != null && !string.IsNullOrEmpty(group.place.address))
        description = group.place.address;
      if (group.city != null && !string.IsNullOrEmpty(group.city.title))
      {
        if (!string.IsNullOrEmpty(description))
          description += ", ";
        description += group.city.title;
      }
      if (group.country != null && !string.IsNullOrEmpty(group.country.title))
      {
        if (!string.IsNullOrEmpty(description))
          description += ", ";
        description += group.country.title;
      }
      if (!string.IsNullOrEmpty(description))
      {
        Action navigationAction =  null;
        if (group.place != null && group.place.latitude != 0.0 && group.place.longitude != 0.0)
          navigationAction = (Action) (() => Navigator.Current.NavigateToMap(false, group.place.latitude, group.place.longitude));
        profileInfoItemList1.Add((ProfileInfoItem) new CustomItem(CommonResources.ProfilePage_Info_Location.ToLowerInvariant(), description, navigationAction, ProfileInfoItemType.RichText));
      }
      if (profileInfoItemList1.Count > 0)
        this.InfoSections.Add(new ProfileInfoSectionItem()
        {
          Items = profileInfoItemList1
        });
      List<ProfileInfoItem> profileInfoItemList2 = new List<ProfileInfoItem>() { (ProfileInfoItem) new VKSocialNetworkItem((IProfileData) this._groupData) };
      if (!string.IsNullOrEmpty(group.site))
        profileInfoItemList2.Add((ProfileInfoItem) new SiteItem(group.site));
      if (profileInfoItemList2.Count > 0)
        this.InfoSections.Add(new ProfileInfoSectionItem(CommonResources.ProfilePage_Info_ContactInformation)
        {
          Items = profileInfoItemList2
        });
      List<ProfileInfoItem> profileInfoItemList3 = new List<ProfileInfoItem>();
      if (!group.links.IsNullOrEmpty())
      {
        profileInfoItemList3.AddRange((IEnumerable<ProfileInfoItem>) LinkItem.GetLinkItems(group.links));
        if (profileInfoItemList3.Count > 0)
          this.InfoSections.Add(new ProfileInfoSectionItem(CommonResources.ProfilePage_Info_Links)
          {
            Items = profileInfoItemList3
          });
      }
      List<ProfileInfoItem> profileInfoItemList4 = new List<ProfileInfoItem>();
      if (!group.contacts.IsNullOrEmpty())
      {
        profileInfoItemList4.AddRange((IEnumerable<ProfileInfoItem>) ContactItem.GetContactItems(group.contacts, this._groupData.contactsUsers));
        if (profileInfoItemList4.Count > 0)
          this.InfoSections.Add(new ProfileInfoSectionItem(CommonResources.ProfilePage_Info_Contacts)
          {
            Items = profileInfoItemList4
          });
      }
      if (this.InfoSections.Count <= 0)
        return;
      ((ProfileInfoSectionItem) Enumerable.Last<ProfileInfoSectionItem>(this.InfoSections)).DividerVisibility = Visibility.Collapsed;
    }

    private void CreateAppointment(int startDate, int finishDate, string groupName, string address)
    {
      DateTime dateTime = Extensions.UnixTimeStampToDateTime((double) startDate, true);
      SaveAppointmentTask saveAppointmentTask1 = new SaveAppointmentTask();
      DateTime? nullable = new DateTime?(dateTime);
      saveAppointmentTask1.StartTime = nullable;
      string str = groupName;
      saveAppointmentTask1.Subject = str;
      int num = 0;
      saveAppointmentTask1.IsAllDayEvent = (num != 0);
      SaveAppointmentTask saveAppointmentTask2 = saveAppointmentTask1;
      if (!string.IsNullOrEmpty(address))
        saveAppointmentTask2.Location = address;
      if (finishDate > startDate)
        saveAppointmentTask2.EndTime=(new DateTime?(Extensions.UnixTimeStampToDateTime((double) finishDate, true)));
      saveAppointmentTask2.Show();
    }
  }
}
