using Microsoft.Phone.UserData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class ContactsSyncManager
  {
    private static ContactsSyncManager _instance;
    private const string CONTACTS_FILENAME = "SAVED_ContactsPhoneNumbers";
    private bool _isSynching;

    public static ContactsSyncManager Instance
    {
      get
      {
        return ContactsSyncManager._instance ?? (ContactsSyncManager._instance = new ContactsSyncManager());
      }
    }

    private ContactsSyncManager()
    {
    }

    private static async Task<ContactsCache> RetrieveContactsAsync()
    {
      ContactsCache contactsCache = new ContactsCache();
      try
      {
        await CacheManager.TryDeserializeAsync((IBinarySerializable) contactsCache, "SAVED_ContactsPhoneNumbers", CacheManager.DataType.CachedData);
      }
      catch
      {
      }
      return contactsCache;
    }

    private static async Task StoreContactsAsync(ContactsCache contactsCache)
    {
      await CacheManager.TrySerializeAsync((IBinarySerializable) contactsCache, "SAVED_ContactsPhoneNumbers", false, CacheManager.DataType.CachedData);
    }

    private static async Task<List<string>> GetPhoneContactsAsync()
    {
      TaskCompletionSource<List<string>> tcs = new TaskCompletionSource<List<string>>();
      Contacts contacts = new Contacts();
      EventHandler<ContactsSearchEventArgs> eventHandler = (EventHandler<ContactsSearchEventArgs>) ((sender, args) =>
      {
        try
        {
          List<string> result = new List<string>();
          IEnumerator<Contact> enumerator = ((IEnumerable<Contact>)Enumerable.Where<Contact>(args.Results, (Func<Contact, bool>)(c =>
          {
            if (c.PhoneNumbers != null)
              return Enumerable.Any<ContactPhoneNumber>(c.PhoneNumbers);
            return false;
          }))).GetEnumerator();
          try
          {
            while (((IEnumerator) enumerator).MoveNext())
            {
              Contact current = enumerator.Current;
              result.AddRange((IEnumerable<string>)Enumerable.Select<ContactPhoneNumber, string>(current.PhoneNumbers, (Func<ContactPhoneNumber, string>)(c => c.PhoneNumber)));
            }
          }
          finally
          {
            if (enumerator != null)
              ((IDisposable) enumerator).Dispose();
          }
          tcs.SetResult(result);
        }
        catch
        {
          tcs.SetResult( null);
        }
      });
      contacts.SearchCompleted+=(eventHandler);
      string str1 = "";
      int num = 4;
      string str2 = "";
      contacts.SearchAsync(str1, (FilterKind) num, str2);
      return await tcs.Task;
    }

    public async void Sync(Action callback = null)
    {
      if (!AppGlobalStateManager.Current.GlobalState.AllowSendContacts || this._isSynching)
      {
        // ISSUE: reference to a compiler-generated field
        if (callback == null)
          return;
        // ISSUE: reference to a compiler-generated field
        callback.Invoke();
      }
      else
      {
        this._isSynching = true;
        // ISSUE: variable of a compiler-generated type
//        ContactsSyncManager.<>c__DisplayClass9_0 cDisplayClass90_2 = cDisplayClass90_1;
        // ISSUE: reference to a compiler-generated field
//        List<string> phoneNumbers = cDisplayClass90_2.phoneNumbers;
        List<string> phoneContactsAsync = await ContactsSyncManager.GetPhoneContactsAsync();
        // ISSUE: reference to a compiler-generated field
//        cDisplayClass90_2.phoneNumbers = phoneContactsAsync;
//        cDisplayClass90_2 = (ContactsSyncManager.<>c__DisplayClass9_0) null;
        // ISSUE: reference to a compiler-generated field
        if (phoneContactsAsync == null)
        {
          // ISSUE: reference to a compiler-generated field
          if (callback != null)
          {
            // ISSUE: reference to a compiler-generated field
            callback.Invoke();
          }
          this._isSynching = false;
        }
        else
        {
          ContactsCache contactsCache = await ContactsSyncManager.RetrieveContactsAsync();
          // ISSUE: reference to a compiler-generated field
          if (!ContactsSyncManager.AreListsEqual(phoneContactsAsync, contactsCache.PhoneNumbers))
          {
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated method
           // AccountService.Instance.LookupContacts("phone", "", phoneContactsAsync, new Action<BackendResult<LookupContactsResponse, ResultCode>>(cDisplayClass90_1.<Sync>b__0));

              AccountService.Instance.LookupContacts("phone", "", phoneContactsAsync,async delegate(BackendResult<LookupContactsResponse, ResultCode> result)
						{
							if (result.ResultCode == ResultCode.Succeeded)
							{
								await ContactsSyncManager.StoreContactsAsync(new ContactsCache
								{
									PhoneNumbers = phoneContactsAsync
								});
							}
							this._isSynching = false;
							if (callback != null)
							{
								callback.Invoke();
							}
						});
          }
          this._isSynching = false;
          // ISSUE: reference to a compiler-generated field
          if (callback == null)
            return;
          // ISSUE: reference to a compiler-generated field
          callback.Invoke();
        }
      }
    }

    private static bool AreListsEqual(List<string> list1, List<string> list2)
    {
        if (list1.Count != list2.Count)
        {
            return false;
        }
        list1.Sort();
        list2.Sort();
        return !Enumerable.Any<string>(Enumerable.Where<string>(list1, (string t, int i) => t != list2[i]));
    }
  }
}
