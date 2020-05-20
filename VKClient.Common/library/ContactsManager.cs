using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Base.Social;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;
using Windows.Foundation;
using Windows.Phone.PersonalInformation;
using Windows.Phone.SocialInformation;
using Windows.System.Threading;

namespace VKClient.Common.Library
{
    public class ContactsManager : IHandle<FriendRequestAcceptedDeclined>, IHandle, IHandle<FriendRemoved>
    {
        private static readonly int MAX_FRIENDS_TO_SYNC = 10000;
        //private static readonly double MIN_SYNC_INTERVAL_HR = 24.0;
        private static readonly string SAVED_CONTACTS_FILE_ID = "SAVED_Contacts";
        private static ContactsManager _instance;
        private SavedContacts _savedContacts;
        private bool _needPersist;
        private bool _deleting;
        private bool _synching;

        public static ContactsManager Instance
        {
            get
            {
                if (ContactsManager._instance == null)
                    ContactsManager._instance = new ContactsManager();
                return ContactsManager._instance;
            }
        }

        public ContactsManager()
        {
            EventAggregator.Current.Subscribe(this);
        }

        private async Task<SavedContacts> GetSavedList()
        {
            if (this._savedContacts == null)
            {
                this._savedContacts = new SavedContacts();
                try
                {
                    int num = await CacheManager.TryDeserializeAsync((IBinarySerializable)this._savedContacts, ContactsManager.SAVED_CONTACTS_FILE_ID, CacheManager.DataType.CachedData) ? 1 : 0;
                }
                catch (Exception)
                {
                }
            }
            this._needPersist = true;
            return this._savedContacts;
        }

        public async Task EnsurePersistSavedContactsAsync()
        {
            if (!this._needPersist)
                return;
            Logger.Instance.Info("ContactsManager persisting saved contacts data");
            await CacheManager.TrySerializeAsync((IBinarySerializable)this._savedContacts, ContactsManager.SAVED_CONTACTS_FILE_ID, false, CacheManager.DataType.CachedData);
            this._needPersist = false;
        }

        public async Task EnsureInSyncAsync(bool forceNow = false)
        {
            bool forceNow1 = (forceNow ? 1 : 0) != 0;
            await ThreadPool.RunAsync((WorkItemHandler)(async o =>
            {
                bool syncContacts = AppGlobalStateManager.Current.GlobalState.SyncContacts;
                bool loggedInUser = (ulong)AppGlobalStateManager.Current.LoggedInUserId > 0UL;
                if (!loggedInUser || !syncContacts)
                    await this.DeleteAllContactsAsync();
                if (!loggedInUser)
                    return;
                if (!((System.DateTime.UtcNow - (await this.GetSavedList()).SyncedDate).TotalHours >= 24.0 | forceNow1))
                    return;
                UsersService.Instance.GetFriendsWithRequests((Action<BackendResult<AllFriendsList, ResultCode>>)(async res =>
                {
                    if (res.ResultCode != ResultCode.Succeeded)
                        return;
                    FriendsCache.Instance.SetFriends(res.ResultData.friends, res.ResultData.requests);
                    if (!syncContacts)
                        return;
                    await this.SyncContactsAsync(res.ResultData.friends);
                }));
            }));
        }

        public async Task DeleteAllContactsAsync()
        {
            if (this._deleting || this._synching)
                return;
            await this.DoDeleteAllContactsAsync();
        }

        private async Task DoDeleteAllContactsAsync()
        {
            this._deleting = true;
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                await (await ContactStore.CreateOrOpenAsync()).DeleteAsync();
                await SocialManager.DeprovisionAsync();
                SavedContacts arg_1E1_0 = await this.GetSavedList();
                arg_1E1_0.SyncedDate = DateTime.MinValue;
                arg_1E1_0.SavedUsers.Clear();
                await this.EnsurePersistSavedContactsAsync();
                stopwatch.Stop();
                stopwatch = null;
            }
            catch (Exception var_4_272)
            {
                Logger.Instance.Error("ContactsManager failed to delete all contacts", var_4_272);
            }
            finally
            {
                int num = 0;
                if (num < 0)
                {
                    this._deleting = false;
                }
            }
        }

        public async Task SyncContactsAsync(List<User> friendsList)
        {
            if (!this._synching && !this._deleting)
            {
                this._synching = true;
                friendsList = Enumerable.ToList<User>(Enumerable.Take<User>(friendsList, ContactsManager.MAX_FRIENDS_TO_SYNC));
                long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
                try
                {
                    SavedContacts savedContacts = await this.GetSavedList();
                    SavedContacts savedContacts2 = savedContacts;
                    List<User> list = savedContacts2.SavedUsers;
                    List<User> arg_1E7_0 = list;
                    List<User> arg_1E7_1 = friendsList;
                    Func<User, string> arg_1E7_2 = new Func<User, string>((u) => { return u.uid.ToString(); });

                    Func<User, User, bool> arg_1E7_3 = new Func<User, User, bool>((u1, u2) =>
                    {
                        return ContactsManager.AreStringsEqualOrNullEmpty(u1.first_name, u2.first_name) && ContactsManager.AreStringsEqualOrNullEmpty(u1.last_name, u2.last_name) && ContactsManager.AreStringsEqualOrNullEmpty(u1.mobile_phone, u2.mobile_phone) && ContactsManager.AreStringsEqualOrNullEmpty(u1.home_phone, u2.home_phone) && ContactsManager.AreStringsEqualOrNullEmpty(u1.site, u2.site) && ContactsManager.AreStringsEqualOrNullEmpty(u1.bdate, u2.bdate) && ContactsManager.AreStringsEqualOrNullEmpty(u1.photo_max, u2.photo_max);
                    });

                    List<Tuple<User, User>> list2;
                    List<User> list3;
                    List<User> list4;
                    ListUtils.GetListChanges<User>(arg_1E7_0, arg_1E7_1, arg_1E7_2, arg_1E7_3, out list2, out list3, out list4);
                    Logger.Instance.Info("ContactsManager got {0} updated users, {1} new users, {2} deleted users", new object[]
			{
				list2.Count,
				list3.Count,
				list4.Count
			});
                    int count = list3.Count;
                    int num = 0;
                    if (loggedInUserId != AppGlobalStateManager.Current.LoggedInUserId || !AppGlobalStateManager.Current.GlobalState.SyncContacts)
                    {
                        await this.DoDeleteAllContactsAsync();
                    }
                    else
                    {
                        ContactStore contactStore = await ContactStore.CreateOrOpenAsync();
                        await this.EnsureProvisioned(contactStore);
                        StoredContact storedContact = await contactStore.FindContactByRemoteIdAsync(ContactsManager.GetRemoteId(AppGlobalStateManager.Current.GlobalState.LoggedInUser));
                        if (storedContact != null)
                        {
                            await this.SetContactProperties(storedContact, AppGlobalStateManager.Current.GlobalState.LoggedInUser, null);
                            await storedContact.SaveAsync();
                        }
                        contactStore.CreateContactQuery();
                        List<Tuple<User, User>>.Enumerator enumerator = list2.GetEnumerator();
                        //int num2;
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                Tuple<User, User> var_8_57F = enumerator.Current;
                                User user = var_8_57F.Item2;
                                User originalUser = var_8_57F.Item1;
                                if (loggedInUserId != AppGlobalStateManager.Current.LoggedInUserId || !AppGlobalStateManager.Current.GlobalState.SyncContacts)
                                {
                                    await this.DoDeleteAllContactsAsync();
                                    return;
                                }
                                try
                                {
                                    StoredContact storedContact2 = await contactStore.FindContactByRemoteIdAsync(ContactsManager.GetRemoteId(user));
                                    await this.SetContactProperties(storedContact2, user, originalUser);
                                    if (storedContact2 != null)
                                    {
                                        await storedContact2.SaveAsync();
                                    }
                                    storedContact2 = null;
                                }
                                catch (Exception var_9_7B5)
                                {
                                    Logger.Instance.Error("Failed to update contact for user " + user.Name, var_9_7B5);
                                }
                                user = null;
                                originalUser = null;
                            }
                        }
                        finally
                        {
                            //if (num2 < 0)
                            //{
                            enumerator.Dispose();
                            //}
                        }
                        enumerator = default(List<Tuple<User, User>>.Enumerator);
                        List<User>.Enumerator enumerator2 = list3.GetEnumerator();
                        try
                        {
                            while (enumerator2.MoveNext())
                            {
                                User user2 = enumerator2.Current;
                                num++;
                                if (loggedInUserId != AppGlobalStateManager.Current.LoggedInUserId || !AppGlobalStateManager.Current.GlobalState.SyncContacts)
                                {
                                    await this.DoDeleteAllContactsAsync();
                                    return;
                                }
                                try
                                {
                                    if (await contactStore.FindContactByRemoteIdAsync(ContactsManager.GetRemoteId(user2)) == null)//todo:bug?
                                    {
                                        Stopwatch stopwatch = Stopwatch.StartNew();
                                        this.FireSyncStatusChanged(num, count);
                                        Logger.Instance.Info("ContactsManager begin creating user", new object[0]);
                                        StoredContact storedContact3 = new StoredContact(contactStore);
                                        await this.SetContactProperties(storedContact3, user2, null);
                                        await storedContact3.SaveAsync();
                                        Logger.Instance.Info("ContactsManager end creating user", new object[0]);
                                        stopwatch.Stop();
                                        long var_11_AF3 = 500L - stopwatch.ElapsedMilliseconds;
                                        if (var_11_AF3 > 0L)
                                        {
                                            await Task.Delay((int)var_11_AF3);
                                        }
                                        stopwatch = null;
                                        storedContact3 = null;
                                    }
                                    list.Add(user2);
                                }
                                catch (Exception var_12_B82)
                                {
                                    Logger.Instance.Error("Failed to create contact for user " + user2.Name, var_12_B82);
                                }
                                user2 = null;
                            }
                        }
                        finally
                        {
                            //if (num2 < 0)
                            //{
                            enumerator2.Dispose();
                            //}
                        }
                        enumerator2 = default(List<User>.Enumerator);
                        enumerator2 = list4.GetEnumerator();
                        try
                        {
                            while (enumerator2.MoveNext())
                            {
                                User user3 = enumerator2.Current;
                                if (loggedInUserId != AppGlobalStateManager.Current.LoggedInUserId || !AppGlobalStateManager.Current.GlobalState.SyncContacts)
                                {
                                    await this.DoDeleteAllContactsAsync();
                                    return;
                                }
                                try
                                {
                                    StoredContact var_13_D35 = await contactStore.FindContactByRemoteIdAsync(ContactsManager.GetRemoteId(user3));
                                    if (var_13_D35 != null)
                                    {
                                        await contactStore.DeleteContactAsync(var_13_D35.Id);
                                    }
                                    list.Remove(user3);
                                }
                                catch (Exception var_14_DBF)
                                {
                                    Logger.Instance.Error("Failed to delete contact for user " + user3.Name, var_14_DBF);
                                }
                                user3 = null;
                            }
                        }
                        finally
                        {
                            //if (num2 < 0)
                            //{
                            enumerator2.Dispose();
                            //}
                        }
                        enumerator2 = default(List<User>.Enumerator);
                        savedContacts2.SyncedDate = DateTime.UtcNow;
                        await this.EnsurePersistSavedContactsAsync();
                        savedContacts2 = null;
                        list = null;
                        list4 = null;
                        list3 = null;
                        list2 = null;
                        contactStore = null;
                        storedContact = null;
                    }
                }
                catch (Exception var_15_ECB)
                {
                    Logger.Instance.Error("Failed to sync contacts. ", var_15_ECB);
                }
                finally
                {
                    //int num2;
                    //if (num2 < 0)
                    //{
                    this._synching = false;
                    this.FireSyncStatusChanged(0, 0);
                    //}
                }
            }
        }

        private void FireSyncStatusChanged(int currentSyncing = 0, int totalCountToSync = 0)
        {
            EventAggregator current = EventAggregator.Current;
            ContactsSyncStatusChangedEvent statusChangedEvent = new ContactsSyncStatusChangedEvent();
            int num1 = this._synching ? 1 : 0;
            statusChangedEvent.IsSyncing = num1 != 0;
            int num2 = currentSyncing;
            statusChangedEvent.Current = num2;
            int num3 = totalCountToSync;
            statusChangedEvent.Total = num3;
            current.Publish(statusChangedEvent);
        }

        private async Task EnsureProvisioned(ContactStore store)
        {
            if (AppGlobalStateManager.Current.LoggedInUserId != 0L)
            {
                bool flag = true;
                string text = ContactsManager.GetRemoteId(AppGlobalStateManager.Current.GlobalState.LoggedInUser);
                try
                {
                    if (await store.FindContactByRemoteIdAsync(text) == null)
                    {
                        flag = false;
                    }
                }
                catch (Exception)
                {
                    flag = false;
                }
                if (!flag)
                {
                    try
                    {
                        StoredContact arg_159_0 = await store.CreateMeContactAsync(text);
                        arg_159_0.DisplayName = (AppGlobalStateManager.Current.GlobalState.LoggedInUser.Name);
                        await arg_159_0.SaveAsync();
                    }
                    catch
                    {
                    }
                    try
                    {
                        await SocialManager.ProvisionAsync();
                    }
                    catch
                    {
                    }
                }
                text = null;
            }
        }


        private async Task SetContactProperties(StoredContact contact, User user, User originalUser = null)
        {
            if (contact != null)
            {
                contact.RemoteId = (ContactsManager.GetRemoteId(user));
                contact.GivenName = (user.first_name);
                contact.FamilyName = (user.last_name);
                if (!string.IsNullOrWhiteSpace(user.photo_max) && !user.photo_max.EndsWith(".gif"))
                {
                    Stream stream = await HttpExtensions.TryGetResponseStreamAsync(user.photo_max);
                    if (stream == null)
                    {
                        throw new Exception("failed to download contact pic " + user.photo_max);
                    }
                    await contact.SetDisplayPictureAsync(stream.AsInputStream());
                }
                IDictionary<string, object> dictionary = await contact.GetPropertiesAsync();
                if (!string.IsNullOrWhiteSpace(user.site))
                {
                    dictionary[KnownContactProperties.Url] = user.site;
                }
                if (!string.IsNullOrWhiteSpace(user.mobile_phone) && this.IsPhoneNumber(user.mobile_phone))
                {
                    List<string> var_6_262 = BaseFormatterHelper.ParsePhoneNumbers(user.mobile_phone);
                    if (var_6_262.Count >= 1)
                    {
                        dictionary[KnownContactProperties.MobileTelephone] = var_6_262[0];
                    }
                    if (var_6_262.Count >= 2)
                    {
                        dictionary[KnownContactProperties.AlternateMobileTelephone] = var_6_262[1];
                    }
                }
                if (!string.IsNullOrWhiteSpace(user.home_phone) && this.IsPhoneNumber(user.home_phone))
                {
                    List<string> var_7_2D8 = BaseFormatterHelper.ParsePhoneNumbers(user.home_phone);
                    if (var_7_2D8.Count >= 1)
                    {
                        dictionary[KnownContactProperties.Telephone] = var_7_2D8[0];
                    }
                    if (var_7_2D8.Count >= 2)
                    {
                        dictionary[KnownContactProperties.AlternateTelephone] = var_7_2D8[1];
                    }
                }
                DateTime var_8;
                if (!string.IsNullOrWhiteSpace(user.bdate) && ContactsManager.TryParseDateTimeFromString(user.bdate, out var_8))
                {
                    var_8 = var_8.Add(DateTime.Now - DateTime.UtcNow);
                    new DateTimeOffset(var_8.Year, var_8.Month, var_8.Day, 0, 0, 0, 0, TimeSpan.Zero);
                    dictionary[KnownContactProperties.Birthdate] = new DateTimeOffset(var_8);
                }
            }
            if (originalUser != null)
            {
                originalUser.first_name = user.first_name;
                originalUser.last_name = user.last_name;
                originalUser.site = user.site;
                originalUser.mobile_phone = user.mobile_phone;
                originalUser.home_phone = user.home_phone;
                originalUser.photo_max = user.photo_max;
                originalUser.bdate = user.bdate;
            }
        }


        private bool IsPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return false;
            }
            Func<char, bool> arg_2A_1 = new Func<char, bool>((c) => char.IsDigit(c));

            return Enumerable.Any<char>(phone, arg_2A_1);
        }

        public static bool TryParseDateTimeFromString(string date, out System.DateTime dateTime)
        {
            dateTime = System.DateTime.MinValue;
            try
            {
                string[] strArray = ((string)date).Split((char[])new char[1] { '.' });
                if (strArray.Length != 3)
                    return false;
                int day = int.Parse(strArray[0]);
                int month = int.Parse(strArray[1]);
                int year = int.Parse(strArray[2]);
                dateTime = new System.DateTime(year, month, day);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("ContactsManager failed to parse date", ex);
                return false;
            }
        }

        public static string GetRemoteId(User user)
        {
            return RemoteIdHelper.GenerateUniqueRemoteId(user.uid.ToString(), RemoteIdHelper.RemoteIdItemType.UserOrGroup);
        }

        public static bool AreStringsEqualOrNullEmpty(string str1, string str2)
        {
            if (str1 == null && str2 == string.Empty || str2 == null && str1 == string.Empty)
                return true;
            return str1 == str2;
        }

        public void Handle(FriendRequestAcceptedDeclined message)
        {
            this.EnsureInSyncAsync(true);
        }

        public void Handle(FriendRemoved message)
        {
            this.EnsureInSyncAsync(true);
        }
    }
}
