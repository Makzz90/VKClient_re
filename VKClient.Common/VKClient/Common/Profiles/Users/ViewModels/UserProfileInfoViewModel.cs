using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Users.ViewModels
{
  public class UserProfileInfoViewModel : ProfileInfoViewModelBase
  {
    private readonly UserData _userData;

    public override List<InfoListItem> InfoItems
    {
      get
      {
        List<InfoListItem> infoListItemList1 = new List<InfoListItem>();
        if (this._userData == null)
          return infoListItemList1;
        User user = this._userData.user;
        Counters counters = this._userData.counters;
        if (!string.IsNullOrEmpty(this._userData.Activity) || this._userData.AdminLevel > 1)
        {
          StatusInfoListItem statusInfoListItem1 = new StatusInfoListItem((IProfileData) this._userData);
          string str = "/Resources/Profile/ProfileStatus.png";
          statusInfoListItem1.IconUrl = str;
          StatusInfoListItem statusInfoListItem2 = statusInfoListItem1;
          infoListItemList1.Add((InfoListItem) statusInfoListItem2);
        }
        if (counters.friends > 0)
        {
          InfoListItem infoListItem1 = new InfoListItem();
          infoListItem1.IconUrl = "/Resources/Profile/ProfileFriends.png";
          InlinesCollection inlinesCollection = this.ComposeInlinesForFriends(counters);
          infoListItem1.Inlines = inlinesCollection;
          int num = 1;
          infoListItem1.IsTiltEnabled = num != 0;
          Action action = (Action) (() =>
          {
            this.PublishProfileBlockClickEvent(ProfileBlockType.friends);
            Navigator.Current.NavigateToFriends(user.id, "", false, FriendsPageMode.Default);
          });
          infoListItem1.TapAction = action;
          InfoListItem infoListItem2 = infoListItem1;
          List<User> randomMutualFriends = this._userData.randomMutualFriends;
          if (randomMutualFriends != null && user.id != AppGlobalStateManager.Current.LoggedInUserId)
          {
            if (randomMutualFriends.Count > 0)
              infoListItem2.Preview1 = randomMutualFriends[0].photo_max;
            if (randomMutualFriends.Count > 1)
              infoListItem2.Preview2 = randomMutualFriends[1].photo_max;
            if (randomMutualFriends.Count > 2)
              infoListItem2.Preview3 = randomMutualFriends[2].photo_max;
          }
          infoListItemList1.Add(infoListItem2);
        }
        if (counters.followers > 0)
        {
          List<InfoListItem> infoListItemList2 = infoListItemList1;
          InfoListItem infoListItem = new InfoListItem();
          infoListItem.IconUrl = "/Resources/Profile/ProfileFollowers.png";
          InlinesCollection inlinesCollection = this.ComposeInlinesForFollowers(counters);
          infoListItem.Inlines = inlinesCollection;
          int num = 1;
          infoListItem.IsTiltEnabled = num != 0;
          Action action = (Action) (() =>
          {
            this.PublishProfileBlockClickEvent(ProfileBlockType.followers);
            Navigator.Current.NavigateToFollowers(user.id, false, "");
          });
          infoListItem.TapAction = action;
          infoListItemList2.Add(infoListItem);
        }
        if (!string.IsNullOrEmpty(user.bdate) && user.id != AppGlobalStateManager.Current.LoggedInUserId)
        {
          InfoListItem infoListItem1 = new InfoListItem();
          infoListItem1.IconUrl = "/Resources/Profile/ProfileBirthday.png";
          string str = this.ComposeTextForBirthday(user);
          infoListItem1.Text = str;
          InfoListItem infoListItem2 = infoListItem1;
          DateTime birthday = this.GetBirthday(user);
          if (birthday > DateTime.MinValue)
          {
            infoListItem2.IsTiltEnabled = true;
            infoListItem2.TapAction = (Action) (() =>
            {
              SaveAppointmentTask saveAppointmentTask = new SaveAppointmentTask();
              saveAppointmentTask.StartTime = new DateTime?(birthday);
              saveAppointmentTask.Subject = string.Format("{0}: {1}", (object) CommonResources.ProfilePage_Info_Birthday, (object) user.Name);
              int num = 0;
              saveAppointmentTask.IsAllDayEvent = num != 0;
              saveAppointmentTask.Show();
            });
          }
          infoListItemList1.Add(infoListItem2);
        }
        if (user.city != null && user.city.id > 0L && user.id != AppGlobalStateManager.Current.LoggedInUserId)
        {
          List<InfoListItem> infoListItemList2 = infoListItemList1;
          InfoListItem infoListItem = new InfoListItem();
          infoListItem.IconUrl = "/Resources/Profile/ProfileHome.png";
          string str = string.Format("{0}: {1}", (object) CommonResources.City, (object) user.city.name);
          infoListItem.Text = str;
          infoListItemList2.Add(infoListItem);
        }
        if (user.occupation != null)
        {
          InfoListItem infoListItem = new InfoListItem()
          {
            IconUrl = "/Resources/Profile/" + (user.occupation.type == OccupationType.work ? "ProfileWork.png" : "ProfileEducation.png"),
            Text = string.Format("{0}: {1}", user.occupation.type == OccupationType.work ? (object) CommonResources.OccupationType_Work : (object) CommonResources.ProfilePage_Info_Education, (object) user.occupation.name)
          };
          if (user.occupation.type == OccupationType.work)
          {
            infoListItem.Text = string.Format("{0}: {1}", (object) CommonResources.OccupationType_Work, (object) user.occupation.name);
          }
          else
          {
            int graduation = user.graduation;
            string str = graduation > 0 ? string.Format("{0} '{1:00}", (object) user.occupation.name, (object) (graduation % 100)) : user.occupation.name;
            infoListItem.Text = string.Format("{0}: {1}", (object) CommonResources.ProfilePage_Info_Education, (object) str);
          }
          Group occupationGroup = this._userData.occupationGroup;
          if (occupationGroup != null)
          {
            infoListItem.Preview1 = occupationGroup.photo_200;
            infoListItem.IsTiltEnabled = true;
            infoListItem.TapAction = (Action) (() => Navigator.Current.NavigateToGroup(occupationGroup.id, occupationGroup.name, false));
          }
          infoListItemList1.Add(infoListItem);
        }
        return infoListItemList1;
      }
    }

    public UserProfileInfoViewModel(UserData userData)
    {
      this._userData = userData;
    }

    private InlinesCollection ComposeInlinesForFriends(Counters counters)
    {
      InlinesCollection inlinesCollection1 = new InlinesCollection();
      InlinesCollection inlinesCollection2 = inlinesCollection1;
      Run run1 = new Run();
      FontFamily fontFamily1 = new FontFamily("Segoe WP Semibold");
      run1.FontFamily = fontFamily1;
      string str1 = UIStringFormatterHelper.FormatNumberOfSomething(counters.friends, "{0} ", "{0} ", "{0} ", true, null, false);
      run1.Text = str1;
      inlinesCollection2.Add((Inline) run1);
      Run run2 = new Run()
      {
        Text = UIStringFormatterHelper.FormatNumberOfSomething(counters.friends, CommonResources.OneFriendFrm, CommonResources.TwoFourFriendsFrm, CommonResources.FiveFriendsFrm, false, null, false)
      };
      if (counters.mutual_friends > 0)
      {
        run2.Text += " · ";
        inlinesCollection1.Add((Inline) run2);
        InlinesCollection inlinesCollection3 = inlinesCollection1;
        Run run3 = new Run();
        FontFamily fontFamily2 = new FontFamily("Segoe WP Semibold");
        run3.FontFamily = fontFamily2;
        string str2 = UIStringFormatterHelper.FormatNumberOfSomething(counters.mutual_friends, "{0} ", "{0} ", "{0} ", true, null, false);
        run3.Text = str2;
        inlinesCollection3.Add((Inline) run3);
        inlinesCollection1.Add((Inline) new Run()
        {
          Text = UIStringFormatterHelper.FormatNumberOfSomething(counters.mutual_friends, CommonResources.OneMutualFrm, CommonResources.TwoFourMutualFrm, CommonResources.FiveMutualFrm, false, null, false)
        });
      }
      else
        inlinesCollection1.Add((Inline) run2);
      return inlinesCollection1;
    }

    private InlinesCollection ComposeInlinesForFollowers(Counters counters)
    {
      InlinesCollection inlinesCollection = new InlinesCollection();
      Run run = new Run();
      FontFamily fontFamily = new FontFamily("Segoe WP Semibold");
      run.FontFamily = fontFamily;
      string str = UIStringFormatterHelper.FormatNumberOfSomething(counters.followers, "{0} ", "{0} ", "{0} ", true, null, false);
      run.Text = str;
      inlinesCollection.Add((Inline) run);
      inlinesCollection.Add((Inline) new Run()
      {
        Text = UIStringFormatterHelper.FormatNumberOfSomething(counters.followers, CommonResources.OneFollowerFrm, CommonResources.TwoFourFollowersFrm, CommonResources.FiveFollowersFrm, false, null, false)
      });
      return inlinesCollection;
    }

    private DateTime GetBirthday(User user)
    {
      string[] strArray = user.bdate.Split('.');
      if (strArray.Length < 2)
        return DateTime.MinValue;
      int day = int.Parse(strArray[0]);
      DateTime dateTime = new DateTime(DateTime.Now.Year, int.Parse(strArray[1]), day);
      if (DateTime.Now > dateTime)
        dateTime = dateTime.AddYears(1);
      return dateTime;
    }

    private string ComposeTextForBirthday(User user)
    {
      string[] strArray = user.bdate.Split('.');
      if (strArray.Length < 2)
        return "";
      int num1 = int.Parse(strArray[0]);
      int month = int.Parse(strArray[1]);
      int num2 = 0;
      if (strArray.Length > 2)
        num2 = int.Parse(strArray[2]);
      string str = string.Format("{0} {1}", (object) num1, (object) UserProfileInfoViewModel.GetOfMonthStr(month));
      if (num2 != 0)
        str += string.Format(" {0}", (object) num2);
      return string.Format("{0}: {1}", (object) CommonResources.ProfilePage_Info_Birthday, (object) str);
    }

    private static string GetOfMonthStr(int month)
    {
      switch (month)
      {
        case 1:
          return CommonResources.OfJanuary;
        case 2:
          return CommonResources.OfFebruary;
        case 3:
          return CommonResources.OfMarch;
        case 4:
          return CommonResources.OfApril;
        case 5:
          return CommonResources.OfMay;
        case 6:
          return CommonResources.OfJune;
        case 7:
          return CommonResources.OfJuly;
        case 8:
          return CommonResources.OfAugust;
        case 9:
          return CommonResources.OfSeptember;
        case 10:
          return CommonResources.OfOctober;
        case 11:
          return CommonResources.OfNovember;
        case 12:
          return CommonResources.OfDecember;
        default:
          return "";
      }
    }

    protected override ProfileInfoFullViewModel GetFullInfoViewModel()
    {
      return (ProfileInfoFullViewModel) new UserFullInfoViewModel(this._userData);
    }

    private void PublishProfileBlockClickEvent(ProfileBlockType blockType)
    {
      EventAggregator.Current.Publish((object) new ProfileBlockClickEvent()
      {
        UserId = this._userData.Id,
        BlockType = blockType
      });
    }
  }
}
