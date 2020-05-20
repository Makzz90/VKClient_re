using Microsoft.Phone.Tasks;
using Microsoft.Phone.UserData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.FriendsImport
{
  public class ContactsFriendsImportProvider : IFriendsImportProvider
  {
    private readonly List<Contact> _contacts = new List<Contact>();
    private static ContactsFriendsImportProvider _instance;

    public static ContactsFriendsImportProvider Instance
    {
      get
      {
        return ContactsFriendsImportProvider._instance ?? (ContactsFriendsImportProvider._instance = new ContactsFriendsImportProvider());
      }
    }

    public string ServiceName
    {
      get
      {
        return "phone";
      }
    }

    public Func<string, InvitationItemHeader> InvitationConverterFunc
    {
      get
      {
        return (Func<string, InvitationItemHeader>) (phoneNumber =>
        {
          Contact contact = this._contacts.FirstOrDefault<Contact>((Func<Contact, bool>) (c =>
          {
            if (c.PhoneNumbers != null)
              return c.PhoneNumbers.FirstOrDefault<ContactPhoneNumber>((Func<ContactPhoneNumber, bool>) (phone => phone.PhoneNumber == phoneNumber)) != null;
            return false;
          }));
          if (contact == null)
            return (InvitationItemHeader) null;
          Stream picture = contact.GetPicture();
          return new InvitationItemHeader(contact.DisplayName, string.Join(", ", contact.PhoneNumbers.Select<ContactPhoneNumber, string>((Func<ContactPhoneNumber, string>) (p => p.PhoneNumber))), picture, (Action<Action<bool>>) (callback => this.InviteUser(phoneNumber, callback)));
        });
      }
    }

    public string MyContact
    {
      get
      {
        return "";
      }
    }

    public bool SupportInvitation
    {
      get
      {
        return true;
      }
    }

    public void Login()
    {
    }

    public void CompleteLogin(Action<bool> callback)
    {
      callback(true);
    }

    public void LoadExternalUserIds(Action<List<string>, bool> callback)
    {
      ContactsSyncManager.Instance.Sync((Action) (() =>
      {
        Contacts contacts = new Contacts();
        contacts.SearchCompleted += (EventHandler<ContactsSearchEventArgs>) ((sender, args) =>
        {
          try
          {
            List<string> stringList = new List<string>();
            foreach (Contact contact in args.Results.Where<Contact>((Func<Contact, bool>) (c =>
            {
              if (c.PhoneNumbers != null)
                return c.PhoneNumbers.Any<ContactPhoneNumber>();
              return false;
            })))
            {
              this._contacts.Add(contact);
              stringList.AddRange(contact.PhoneNumbers.Select<ContactPhoneNumber, string>((Func<ContactPhoneNumber, string>) (c => c.PhoneNumber)));
            }
            if (callback == null)
              return;
            callback(stringList, true);
          }
          catch
          {
            if (callback == null)
              return;
            callback((List<string>) null, false);
          }
        });
        string filter = "";
        int num = 4;
        string str = "";
        contacts.SearchAsync(filter, (FilterKind) num, (object) str);
      }));
    }

    public void InviteUser(string id, Action<bool> callback)
    {
      new SmsComposeTask()
      {
        To = id,
        Body = CommonResources.InviteToVK
      }.Show();
      callback(false);
    }
  }
}
