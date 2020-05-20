using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Groups.ViewModels
{
  public class GroupProfileInfoViewModel : ProfileInfoViewModelBase
  {
    private readonly GroupData _groupData;

    public override List<InfoListItem> InfoItems
    {
      get
      {
        List<InfoListItem> infoListItemList1 = new List<InfoListItem>();
        if (this._groupData == null)
          return infoListItemList1;
        Group group = this._groupData.group;
        if (!string.IsNullOrEmpty(this._groupData.Activity) || this._groupData.AdminLevel > 1)
        {
          StatusInfoListItem statusInfoListItem1 = new StatusInfoListItem((IProfileData) this._groupData);
          string str = "/Resources/Profile/ProfileStatus.png";
          statusInfoListItem1.IconUrl = str;
          StatusInfoListItem statusInfoListItem2 = statusInfoListItem1;
          infoListItemList1.Add((InfoListItem) statusInfoListItem2);
        }
        if (group.GroupType == GroupType.Event)
        {
          int eventStartDate = group.start_date;
          int eventFinishDate = group.finish_date;
          if (eventStartDate > 0)
          {
            InfoListItem infoListItem = new InfoListItem()
            {
              IconUrl = "/Resources/Profile/ProfileEventTime.png"
            };
            DateTime eventStartDateTime = Extensions.UnixTimeStampToDateTime((double) eventStartDate, true);
            if (Extensions.UnixTimeStampToDateTime((double) eventFinishDate, true) < DateTime.Now || eventStartDateTime < DateTime.Now && eventStartDate >= eventFinishDate)
            {
              infoListItem.Text = string.Format("{0} {1}", (object) CommonResources.Event_Happened, (object) eventStartDateTime.ToString("dd MMMM yyyy"));
            }
            else
            {
              infoListItem.Text = UIStringFormatterHelper.FormatDateTimeForUI(eventStartDate);
              infoListItem.IsTiltEnabled = true;
              infoListItem.TapAction = (Action) (() =>
              {
                SaveAppointmentTask saveAppointmentTask1 = new SaveAppointmentTask();
                saveAppointmentTask1.StartTime = new DateTime?(eventStartDateTime);
                saveAppointmentTask1.Subject = group.name;
                int num = 0;
                saveAppointmentTask1.IsAllDayEvent = num != 0;
                SaveAppointmentTask saveAppointmentTask2 = saveAppointmentTask1;
                Place place = group.place;
                if (!string.IsNullOrEmpty(place != null ? place.address : null))
                  saveAppointmentTask2.Location = group.place.address;
                if (eventFinishDate > eventStartDate)
                  saveAppointmentTask2.EndTime = new DateTime?(Extensions.UnixTimeStampToDateTime((double) eventFinishDate, true));
                saveAppointmentTask2.Show();
              });
            }
            infoListItemList1.Add(infoListItem);
          }
          string str = "";
          Place place1 = group.place;
          if (!string.IsNullOrEmpty(place1 != null ? place1.address : null))
            str = group.place.address;
          City city = group.city;
          if (!string.IsNullOrEmpty(city != null ? city.title : null))
          {
            if (!string.IsNullOrEmpty(str))
              str += ", ";
            str += group.city.title;
          }
          Country country = group.country;
          if (!string.IsNullOrEmpty(country != null ? country.title : null))
          {
            if (!string.IsNullOrEmpty(str))
              str += ", ";
            str += group.country.title;
          }
          if (!string.IsNullOrEmpty(str))
          {
            Action action = null;
            if (group.place != null && group.place.latitude != 0.0 && group.place.longitude != 0.0)
              action = (Action) (() => Navigator.Current.NavigateToMap(false, group.place.latitude, group.place.longitude));
            InfoListItem infoListItem1 = new InfoListItem();
            infoListItem1.IconUrl = "/Resources/Profile/ProfileLocation.png";
            infoListItem1.Text = str;
            infoListItem1.TapAction = action;
            int num = action != null ? 1 : 0;
            infoListItem1.IsTiltEnabled = num != 0;
            InfoListItem infoListItem2 = infoListItem1;
            infoListItemList1.Add(infoListItem2);
          }
        }
        if (group.members_count > 0)
        {
          InfoListItem infoListItem1 = new InfoListItem();
          infoListItem1.IconUrl = "/Resources/Profile/ProfileFriends.png";
          InlinesCollection inlinesCollection = this.ComposeInlinesForMembers();
          infoListItem1.Inlines = inlinesCollection;
          int num = 1;
          infoListItem1.IsTiltEnabled = num != 0;
          Action action = (Action) (() => Navigator.Current.NavigateToCommunitySubscribers(group.id, group.GroupType, false, false, false));
          infoListItem1.TapAction = action;
          InfoListItem infoListItem2 = infoListItem1;
          List<User> friends = this._groupData.friends;
          if (friends != null)
          {
            if (friends.Count > 0)
              infoListItem2.Preview1 = friends[0].photo_max;
            if (friends.Count > 1)
              infoListItem2.Preview2 = friends[1].photo_max;
            if (friends.Count > 2)
              infoListItem2.Preview3 = friends[2].photo_max;
          }
          infoListItemList1.Add(infoListItem2);
        }
        if (!string.IsNullOrEmpty(group.description))
        {
          List<InfoListItem> infoListItemList2 = infoListItemList1;
          InfoListItem infoListItem = new InfoListItem();
          infoListItem.IconUrl = "/Resources/Profile/ProfileAbout.png";
          string description = group.description;
          infoListItem.Text = description;
          infoListItemList2.Add(infoListItem);
        }
        return infoListItemList1;
      }
    }

    public override Visibility WikiPageVisibility
    {
      get
      {
        return (!string.IsNullOrEmpty(this.WikiPageName)).ToVisiblity();
      }
    }

    public override Visibility LinkVisibility
    {
      get
      {
        return (!string.IsNullOrEmpty(this.Link)).ToVisiblity();
      }
    }

    public override string WikiPageName
    {
      get
      {
        GroupData groupData = this._groupData;
        string str;
        if (groupData == null)
        {
          str = null;
        }
        else
        {
          Group group = groupData.group;
          str = group != null ? group.wiki_page : null;
        }
        return str ?? "";
      }
    }

    public override string Link
    {
      get
      {
        GroupData groupData = this._groupData;
        string str;
        if (groupData == null)
        {
          str = null;
        }
        else
        {
          Group group = groupData.group;
          str = group != null ? group.site : null;
        }
        return str ?? "";
      }
    }

    public GroupProfileInfoViewModel(GroupData groupData)
    {
      this._groupData = groupData;
    }

    public string GetEventFinishedStr()
    {
      if (this._groupData == null || this._groupData.group == null || (this._groupData.group.GroupType != GroupType.Event || this._groupData.group.start_date == 0))
        return "";
      DateTime dateTime = Extensions.UnixTimeStampToDateTime((double) Math.Max(this._groupData.group.start_date, this._groupData.group.finish_date), true);
      if (dateTime < DateTime.Now)
        return CommonResources.Event_Happened + " " + dateTime.ToString("dd MMMM yyyy");
      return "";
    }

    private InlinesCollection ComposeInlinesForMembers()
    {
      InlinesCollection inlinesCollection1 = new InlinesCollection();
      Group group = this._groupData.group;
      InlinesCollection inlinesCollection2 = inlinesCollection1;
      Run run1 = new Run();
      FontFamily fontFamily1 = new FontFamily("Segoe WP Semibold");
      run1.FontFamily = fontFamily1;
      string str1 = UIStringFormatterHelper.FormatForUI((long) group.members_count) + Convert.ToChar(160).ToString();
      run1.Text = str1;
      inlinesCollection2.Add((Inline) run1);
      bool flag = group.GroupType == GroupType.PublicPage;
      Run run2 = new Run()
      {
        Text = UIStringFormatterHelper.FormatNumberOfSomething(group.members_count, flag ? CommonResources.OneSubscriberFrm : CommonResources.OneMemberFrm, flag ? CommonResources.TwoFourSubscribersFrm : CommonResources.TwoFourMembersFrm, flag ? CommonResources.FiveSubscribersFrm : CommonResources.FiveMembersFrm, false, null, false)
      };
      if (this._groupData.friendsCount > 0)
      {
        run2.Text += " · ";
        inlinesCollection1.Add((Inline) run2);
        InlinesCollection inlinesCollection3 = inlinesCollection1;
        Run run3 = new Run();
        FontFamily fontFamily2 = new FontFamily("Segoe WP Semibold");
        run3.FontFamily = fontFamily2;
        string str2 = UIStringFormatterHelper.FormatForUI((long) this._groupData.friendsCount) + Convert.ToChar(160).ToString();
        run3.Text = str2;
        inlinesCollection3.Add((Inline) run3);
        inlinesCollection1.Add((Inline) new Run()
        {
          Text = UIStringFormatterHelper.FormatNumberOfSomething(this._groupData.friendsCount, CommonResources.OneFriendFrm, CommonResources.TwoFourFriendsFrm, CommonResources.FiveFriendsFrm, false, null, false)
        });
      }
      else
        inlinesCollection1.Add((Inline) run2);
      return inlinesCollection1;
    }

    protected override ProfileInfoFullViewModel GetFullInfoViewModel()
    {
      return (ProfileInfoFullViewModel) new GroupFullInfoViewModel(this._groupData);
    }

    public override void OpenWikiPage()
    {
      GroupData groupData = this._groupData;
      if (string.IsNullOrEmpty(groupData != null ? groupData.wiki_view_url : null))
        return;
      Navigator.Current.NavigateToWebUri(this._groupData.wiki_view_url, true, false);
    }

    public override void OpenLink()
    {
      GroupData groupData = this._groupData;
      string str;
      if (groupData == null)
      {
        str = null;
      }
      else
      {
        Group group = groupData.group;
        str = group != null ? group.site : null;
      }
      if (string.IsNullOrEmpty(str))
        return;
      Navigator.Current.NavigateToWebUri(this._groupData.group.site, true, false);
    }
  }
}
