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
            Contact contact = (Contact)Enumerable.FirstOrDefault<Contact>(this._contacts, (Func<Contact, bool>)(c =>
          {
            if (c.PhoneNumbers != null)
                return Enumerable.FirstOrDefault<ContactPhoneNumber>(c.PhoneNumbers, (Func<ContactPhoneNumber, bool>)(phone => phone.PhoneNumber == phoneNumber)) != null;
            return false;
          }));
          if (contact == null)
            return  null;
          Stream picture = contact.GetPicture();
          return new InvitationItemHeader(contact.DisplayName, string.Join(", ", (IEnumerable<string>)Enumerable.Select<ContactPhoneNumber, string>(contact.PhoneNumbers, (Func<ContactPhoneNumber, string>)(p => p.PhoneNumber))), picture, (Action<Action<bool>>)(callback => this.InviteUser(phoneNumber, callback)));
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
	EventHandler<ContactsSearchEventArgs> _9__1=null;
	ContactsSyncManager.Instance.Sync(delegate
	{
		Contacts expr_05 = new Contacts();
		EventHandler<ContactsSearchEventArgs> arg_25_1;
		if ((arg_25_1 = _9__1) == null)
		{
            arg_25_1 = (_9__1 = delegate(object sender, ContactsSearchEventArgs args)
			{
				try
				{
					List<string> list = new List<string>();
					IEnumerable<Contact> arg_2B_0 = args.Results;
					Func<Contact, bool> arg_2B_1 = new Func<Contact, bool>((c)=>{return c.PhoneNumbers != null && Enumerable.Any<ContactPhoneNumber>(c.PhoneNumbers);});
					
					using (IEnumerator<Contact> enumerator = Enumerable.Where<Contact>(arg_2B_0, arg_2B_1).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Contact current = enumerator.Current;
							this._contacts.Add(current);
							List<string> arg_7B_0 = list;
							IEnumerable<ContactPhoneNumber> arg_76_0 = current.PhoneNumbers;
							Func<ContactPhoneNumber, string> arg_76_1 = new Func<ContactPhoneNumber, string>((c)=>{return c.PhoneNumber;});
							
							arg_7B_0.AddRange(Enumerable.Select<ContactPhoneNumber, string>(arg_76_0, arg_76_1));
						}
					}
					if (callback != null)
					{
						callback.Invoke(list, true);
					}
				}
				catch
				{
					if (callback != null)
					{
						callback.Invoke(null, false);
					}
				}
			});
		}
		expr_05.SearchCompleted+=(arg_25_1);
		expr_05.SearchAsync("", FilterKind.DisplayName, "");
	});
}


    public void InviteUser(string id, Action<bool> callback)
    {
      SmsComposeTask smsComposeTask = new SmsComposeTask();
      string str = id;
      smsComposeTask.To = str;
      string inviteToVk = CommonResources.InviteToVK;
      smsComposeTask.Body = inviteToVk;
      smsComposeTask.Show();
      callback(false);
    }
  }
}
