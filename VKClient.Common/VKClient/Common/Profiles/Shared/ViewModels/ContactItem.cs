using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class ContactItem : ProfileInfoItem, IProfileInfoSupportCopyToClipboard
  {
    private readonly string _copyData;

    public string GroupImage { get; private set; }

    public double TitleMaxWidth { get; private set; }

    public ContactItem(GroupContact contact, User user)
      : base(ProfileInfoItemType.Full)
    {
      this.Title = user.Name;
      string str = contact.phone ?? "";
      if (!string.IsNullOrEmpty(contact.email))
      {
        if (!string.IsNullOrEmpty(str))
          str += "\n";
        str += contact.email;
      }
      this._copyData = str;
      if (string.IsNullOrEmpty(str))
        str = contact.desc;
      this.Data = (object) str;
      this.GroupImage = user.photo_max;
      if (user.id > 0L)
        this.NavigationAction = (Action) (() => Navigator.Current.NavigateToUserProfile(user.id, user.Name, "", false));
      if (!string.IsNullOrEmpty(this.GroupImage))
        this.TitleMaxWidth = 370.0;
      else
        this.TitleMaxWidth = 448.0;
    }

    public string GetData()
    {
      return this._copyData;
    }

    public static IEnumerable<ContactItem> GetContactItems(List<GroupContact> contacts, List<User> users)
    {
      List<ContactItem> contactItemList = new List<ContactItem>();
      if (contacts.IsNullOrEmpty() || users.IsNullOrEmpty())
        return (IEnumerable<ContactItem>) contactItemList;
      foreach (GroupContact contact1 in contacts)
      {
        GroupContact contact = contact1;
        User user = users.FirstOrDefault<User>((Func<User, bool>) (u => u.id == contact.user_id));
        if (user != null)
          contactItemList.Add(new ContactItem(contact, user));
      }
      return (IEnumerable<ContactItem>) contactItemList;
    }
  }
}
