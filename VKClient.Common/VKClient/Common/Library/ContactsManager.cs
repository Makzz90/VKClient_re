using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Base.Social;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Utils;
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
            EventAggregator.Current.Subscribe((object)this);
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
                catch
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
            int num = await CacheManager.TrySerializeAsync((IBinarySerializable)this._savedContacts, ContactsManager.SAVED_CONTACTS_FILE_ID, false, CacheManager.DataType.CachedData) ? 1 : 0;
            this._needPersist = false;
        }

        public async Task EnsureInSyncAsync(bool forceNow = false)
        {
            bool syncContacts = AppGlobalStateManager.Current.GlobalState.SyncContacts;
            bool flag = AppGlobalStateManager.Current.LoggedInUserId != 0;
            if (!flag || !syncContacts)
            {
                await this.DeleteAllContactsAsync();
            }
            if (flag)
            {
                SavedContacts var_2_158 = await this.GetSavedList();
                if ((DateTime.UtcNow - var_2_158.SyncedDate).TotalHours >= 24.0 | forceNow)
                {
                    VKClient.Common.Backend.UsersService.Instance.GetFriendsWithRequests(new Action<VKClient.Common.Backend.BackendResult<VKClient.Audio.Base.DataObjects.AllFriendsList, ResultCode>>(async (res) =>
                    {
                        if (res.ResultCode == ResultCode.Succeeded)
                        {
                            FriendsCache.Instance.SetFriends(res.ResultData.friends, res.ResultData.requests);
                            if (syncContacts)
                            {
                                await this.SyncContactsAsync(res.ResultData.friends);
                            }
                        }
                    }
                    ));
                }
            }
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
                Stopwatch sw = Stopwatch.StartNew();
                await (await ContactStore.CreateOrOpenAsync()).DeleteAsync();
                await SocialManager.DeprovisionAsync();
                SavedContacts savedList = await this.GetSavedList();
                DateTime dateTime = DateTime.MinValue;
                savedList.SyncedDate = dateTime;
                savedList.SavedUsers.Clear();
                await this.EnsurePersistSavedContactsAsync();
                sw.Stop();
                sw = (Stopwatch)null;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("ContactsManager failed to delete all contacts", ex);
            }
            finally
            {
                this._deleting = false;
            }
        }

        public async Task SyncContactsAsync(List<User> friendsList)
        {
            if (this._synching || this._deleting)
                return;
            this._synching = true;
            friendsList = friendsList.Take<User>(ContactsManager.MAX_FRIENDS_TO_SYNC).ToList<User>();
            long initiallyLoggedInUser = AppGlobalStateManager.Current.LoggedInUserId;
            try
            {
                SavedContacts savedContacts = await this.GetSavedList();
                List<User> savedList = savedContacts.SavedUsers;
                Func<User, string> getKey = (Func<User, string>)(u => u.uid.ToString());

                List<Tuple<User, User>> updatedUsersTuples;
                List<User> createdUsers;
                List<User> deletedUsers;

                ListUtils.GetListChanges<User>(savedList, friendsList, getKey, (Func<User, User, bool>)((u1, u2) =>
                {
                    if (ContactsManager.AreStringsEqualOrNullEmpty(u1.first_name, u2.first_name) && ContactsManager.AreStringsEqualOrNullEmpty(u1.last_name, u2.last_name) && (ContactsManager.AreStringsEqualOrNullEmpty(u1.mobile_phone, u2.mobile_phone) && ContactsManager.AreStringsEqualOrNullEmpty(u1.home_phone, u2.home_phone)) && (ContactsManager.AreStringsEqualOrNullEmpty(u1.site, u2.site) && ContactsManager.AreStringsEqualOrNullEmpty(u1.bdate, u2.bdate)))
                        return ContactsManager.AreStringsEqualOrNullEmpty(u1.photo_max, u2.photo_max);
                    return false;
                }), out updatedUsersTuples, out createdUsers, out deletedUsers);

                Logger.Instance.Info("ContactsManager got {0} updated users, {1} new users, {2} deleted users", updatedUsersTuples.Count, createdUsers.Count, deletedUsers.Count);
                int totalCountToSync = createdUsers.Count;
                int currentSyncing = 0;
                if (initiallyLoggedInUser != AppGlobalStateManager.Current.LoggedInUserId || !AppGlobalStateManager.Current.GlobalState.SyncContacts)
                {
                    await this.DoDeleteAllContactsAsync();
                }
                else
                {
                    ContactStore contactStore = await ContactStore.CreateOrOpenAsync();
                    await this.EnsureProvisioned(contactStore);
                    StoredContact meContact = await contactStore.FindContactByRemoteIdAsync(ContactsManager.GetRemoteId(AppGlobalStateManager.Current.GlobalState.LoggedInUser));
                    if (meContact != null)
                    {
                        await this.SetContactProperties(meContact, AppGlobalStateManager.Current.GlobalState.LoggedInUser, (User)null);
                        await meContact.SaveAsync();
                    }
                    contactStore.CreateContactQuery();
                    foreach (Tuple<User, User> tuple in updatedUsersTuples)
                    {
                        User updUser = tuple.Item2;
                        User originalUser = tuple.Item1;
                        if (initiallyLoggedInUser != AppGlobalStateManager.Current.LoggedInUserId || !AppGlobalStateManager.Current.GlobalState.SyncContacts)
                        {
                            await this.DoDeleteAllContactsAsync();
                            return;
                        }
                        try
                        {
                            StoredContact contact = await contactStore.FindContactByRemoteIdAsync(ContactsManager.GetRemoteId(updUser));
                            await this.SetContactProperties(contact, updUser, originalUser);
                            if (contact != null)
                                await contact.SaveAsync();
                            contact = null;
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Error("Failed to update contact for user " + updUser.Name, ex);
                        }
                        updUser = (User)null;
                        originalUser = (User)null;
                    }
                    //List<Tuple<User, User>>.Enumerator enumerator1 = new List<Tuple<User, User>>.Enumerator();
                    foreach (User user in createdUsers)
                    {
                        User newUser = user;
                        ++currentSyncing;
                        if (initiallyLoggedInUser != AppGlobalStateManager.Current.LoggedInUserId || !AppGlobalStateManager.Current.GlobalState.SyncContacts)
                        {
                            await this.DoDeleteAllContactsAsync();
                            return;
                        }
                        try
                        {
                            if (await contactStore.FindContactByRemoteIdAsync(ContactsManager.GetRemoteId(newUser)) == null)
                            {
                                Stopwatch sw = Stopwatch.StartNew();
                                this.FireSyncStatusChanged(currentSyncing, totalCountToSync);
                                Logger.Instance.Info("ContactsManager begin creating user");
                                StoredContact contact = new StoredContact(contactStore);
                                await this.SetContactProperties(contact, newUser, null);
                                await contact.SaveAsync();
                                Logger.Instance.Info("ContactsManager end creating user");
                                sw.Stop();
                                long num = 500L - sw.ElapsedMilliseconds;
                                if (num > 0L)
                                    await Task.Delay((int)num);
                                sw = null;
                                contact = null;
                            }
                            savedList.Add(newUser);
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Error("Failed to create contact for user " + newUser.Name, ex);
                        }
                        newUser = null;
                    }
                    //List<User>.Enumerator enumerator2 = new List<User>.Enumerator();
                    foreach (User user in deletedUsers)
                    {
                        User deletedUser = user;
                        if (initiallyLoggedInUser != AppGlobalStateManager.Current.LoggedInUserId || !AppGlobalStateManager.Current.GlobalState.SyncContacts)
                        {
                            await this.DoDeleteAllContactsAsync();
                            return;
                        }
                        try
                        {
                            StoredContact contactByRemoteIdAsync = await contactStore.FindContactByRemoteIdAsync(ContactsManager.GetRemoteId(deletedUser));
                            if (contactByRemoteIdAsync != null)
                                await contactStore.DeleteContactAsync(contactByRemoteIdAsync.Id);
                            savedList.Remove(deletedUser);
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Error("Failed to delete contact for user " + deletedUser.Name, ex);
                        }
                        deletedUser = null;
                    }
                    //enumerator2 = new List<User>.Enumerator();
                    savedContacts.SyncedDate = DateTime.UtcNow;
                    await this.EnsurePersistSavedContactsAsync();
                    savedContacts = null;
                    savedList = null;
                    deletedUsers = null;
                    createdUsers = null;
                    updatedUsersTuples = null;
                    contactStore = null;
                    meContact = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("Failed to sync contacts. ", ex);
            }
            finally
            {
                this._synching = false;
                this.FireSyncStatusChanged(0, 0);
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
            current.Publish((object)statusChangedEvent);
        }

        private async Task EnsureProvisioned(ContactStore store)
        {
            if (AppGlobalStateManager.Current.LoggedInUserId == 0L)
                return;
            bool ok = true;
            string meRemoteId = ContactsManager.GetRemoteId(AppGlobalStateManager.Current.GlobalState.LoggedInUser);
            try
            {
                if (await store.FindContactByRemoteIdAsync(meRemoteId) == null)
                    ok = false;
            }
            catch
            {
                ok = false;
            }
            if (!ok)
            {
                StoredContact meContactAsync = await store.CreateMeContactAsync(meRemoteId);
                string name = AppGlobalStateManager.Current.GlobalState.LoggedInUser.Name;
                meContactAsync.DisplayName=name;
                await meContactAsync.SaveAsync();
                try
                {
                    await SocialManager.ProvisionAsync();
                }
                catch
                {
                }
            }
            meRemoteId = null;
        }

        private async Task SetContactProperties(StoredContact contact, User user, User originalUser = null)
        {
            if (contact != null)
            {
                contact.RemoteId=ContactsManager.GetRemoteId(user);
                contact.GivenName=user.first_name;
                contact.FamilyName=user.last_name;
                if (!string.IsNullOrWhiteSpace(user.photo_max) && !user.photo_max.EndsWith(".gif"))
                {
                    Stream responseStreamAsync = await HttpExtensions.TryGetResponseStreamAsync(user.photo_max);
                    if (responseStreamAsync == null)
                        throw new Exception("failed to download contact pic " + user.photo_max);
                    await contact.SetDisplayPictureAsync(responseStreamAsync.AsInputStream());
                }
                IDictionary<string, object> propertiesAsync = await contact.GetPropertiesAsync();
                if (!string.IsNullOrWhiteSpace(user.site))
                    propertiesAsync[KnownContactProperties.Url] = (object)user.site;
                if (!string.IsNullOrWhiteSpace(user.mobile_phone) && this.IsPhoneNumber(user.mobile_phone))
                {
                    List<string> phoneNumbers = BaseFormatterHelper.ParsePhoneNumbers(user.mobile_phone);
                    if (phoneNumbers.Count >= 1)
                        propertiesAsync[KnownContactProperties.MobileTelephone] = (object)phoneNumbers[0];
                    if (phoneNumbers.Count >= 2)
                        propertiesAsync[KnownContactProperties.AlternateMobileTelephone] = (object)phoneNumbers[1];
                }
                if (!string.IsNullOrWhiteSpace(user.home_phone) && this.IsPhoneNumber(user.home_phone))
                {
                    List<string> phoneNumbers = BaseFormatterHelper.ParsePhoneNumbers(user.home_phone);
                    if (phoneNumbers.Count >= 1)
                        propertiesAsync[KnownContactProperties.Telephone] = (object)phoneNumbers[0];
                    if (phoneNumbers.Count >= 2)
                        propertiesAsync[KnownContactProperties.AlternateTelephone] = (object)phoneNumbers[1];
                }
                DateTime dateTime;
                if (!string.IsNullOrWhiteSpace(user.bdate) && ContactsManager.TryParseDateTimeFromString(user.bdate, out dateTime))
                {
                    TimeSpan timeSpan = DateTime.Now - DateTime.UtcNow;
                    dateTime = dateTime.Add(timeSpan);
                    DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0, TimeSpan.Zero);
                    propertiesAsync[KnownContactProperties.Birthdate] = (object)new DateTimeOffset(dateTime);
                }
            }
            if (originalUser == null)
                return;
            originalUser.first_name = user.first_name;
            originalUser.last_name = user.last_name;
            originalUser.site = user.site;
            originalUser.mobile_phone = user.mobile_phone;
            originalUser.home_phone = user.home_phone;
            originalUser.photo_max = user.photo_max;
            originalUser.bdate = user.bdate;
        }

        private bool IsPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return false;
            return phone.Any<char>((Func<char, bool>)(c => char.IsDigit(c)));
        }

        public static bool TryParseDateTimeFromString(string date, out DateTime dateTime)
        {
            dateTime = DateTime.MinValue;
            try
            {
                string[] strArray = date.Split('.');
                if (strArray.Length != 3)
                    return false;
                int day = int.Parse(strArray[0]);
                int month = int.Parse(strArray[1]);
                int year = int.Parse(strArray[2]);
                dateTime = new DateTime(year, month, day);
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

        public async void Handle(FriendRequestAcceptedDeclined message)//omg_re async
        {
            await this.EnsureInSyncAsync(true);
        }

        public async void Handle(FriendRemoved message)//omg_re async
        {
            await this.EnsureInSyncAsync(true);
        }
    }
}
