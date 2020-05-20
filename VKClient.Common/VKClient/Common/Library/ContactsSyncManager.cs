using Microsoft.Phone.UserData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

using System.Runtime.CompilerServices;

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
                /*int num = */await CacheManager.TryDeserializeAsync((IBinarySerializable)contactsCache, "SAVED_ContactsPhoneNumbers", CacheManager.DataType.CachedData);// ? 1 : 0;
            }
            catch
            {
            }
            return contactsCache;
        }

        private static async Task StoreContactsAsync(ContactsCache contactsCache)
        {
            int num = await CacheManager.TrySerializeAsync((IBinarySerializable)contactsCache, "SAVED_ContactsPhoneNumbers", false, CacheManager.DataType.CachedData) ? 1 : 0;
        }

        private static async Task<List<string>> GetPhoneContactsAsync()
        {
            TaskCompletionSource<List<string>> tcs = new TaskCompletionSource<List<string>>();
            Contacts contacts = new Contacts();
            EventHandler<ContactsSearchEventArgs> eventHandler = (EventHandler<ContactsSearchEventArgs>)((sender, args) =>
            {
                try
                {
                    List<string> result = new List<string>();
                    foreach (Contact contact in args.Results.Where<Contact>((Func<Contact, bool>)(c =>
                    {
                        if (c.PhoneNumbers != null)
                            return c.PhoneNumbers.Any<ContactPhoneNumber>();
                        return false;
                    })))
                        result.AddRange(contact.PhoneNumbers.Select<ContactPhoneNumber, string>((Func<ContactPhoneNumber, string>)(c => c.PhoneNumber)));
                    tcs.SetResult(result);
                }
                catch
                {
                    tcs.SetResult(null);
                }
            });
            contacts.SearchCompleted += eventHandler;
            string filter = "";
            string str = "";
            contacts.SearchAsync(filter, FilterKind.DisplayName, (object)str);
            return await tcs.Task;
        }

        private sealed class c__DisplayClass9_0
        {
            public List<string> phoneNumbers;
            public Action callback;
            public ContactsSyncManager _4__this;

            internal void b__0(BackendResult<LookupContactsResponse, ResultCode> result)
            {
                ContactsSyncManager.c__DisplayClass9_0.d stateMachine;
                stateMachine._4__this = this;
                stateMachine.result = result;
                stateMachine.t__builder = AsyncVoidMethodBuilder.Create();
                stateMachine._1__state = -1;
                stateMachine.t__builder.Start<ContactsSyncManager.c__DisplayClass9_0.d>(ref stateMachine);
            }


            private struct d : IAsyncStateMachine
            {
                public int _1__state;
                public AsyncVoidMethodBuilder t__builder;
                public BackendResult<LookupContactsResponse, ResultCode> result;
                public ContactsSyncManager.c__DisplayClass9_0 _4__this;
                private TaskAwaiter u__1;

                void IAsyncStateMachine.MoveNext()
                {
                    int num1 = this._1__state;
                    try
                    {
                        TaskAwaiter awaiter;
                        if (num1 != 0)
                        {
                            if (this.result.ResultCode == ResultCode.Succeeded)
                            {
                                ContactsCache contactsCache = new ContactsCache();
                                List<string> stringList = this._4__this.phoneNumbers;
                                contactsCache.PhoneNumbers = stringList;
                                awaiter = ContactsSyncManager.StoreContactsAsync(contactsCache).GetAwaiter();
                                if (!awaiter.IsCompleted)
                                {
                                    this._1__state = 0;
                                    this.u__1 = awaiter;
                                    this.t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, ContactsSyncManager.c__DisplayClass9_0.d>(ref awaiter, ref this);
                                    return;
                                }
                            }
                            else
                                goto label_7;
                        }
                        else
                        {
                            awaiter = this.u__1;
                            this.u__1 = new TaskAwaiter();
                            this._1__state = -1;
                        }
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                    label_7:
                        this._4__this._4__this._isSynching = false;
                        if (this._4__this.callback != null)
                            this._4__this.callback();
                    }
                    catch (Exception ex)
                    {
                        this._1__state = -2;
                        this.t__builder.SetException(ex);
                        return;
                    }
                    this._1__state = -2;
                    this.t__builder.SetResult();
                }

                //[DebuggerHidden]
                void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    this.t__builder.SetStateMachine(stateMachine);
                }
            }
        }

        private struct d__9 : IAsyncStateMachine
        {
            public int _1__state;
            public AsyncVoidMethodBuilder t__builder;
            public ContactsSyncManager _4__this;
            public Action callback;
            private ContactsSyncManager.c__DisplayClass9_0 _8__1;
            private ContactsSyncManager.c__DisplayClass9_0 _7__wrap1;
            private TaskAwaiter<List<string>> u__1;
            private TaskAwaiter<ContactsCache> u__2;

            void IAsyncStateMachine.MoveNext()
            {
                int num1 = this._1__state;
                try
                {
                    TaskAwaiter<List<string>> awaiter1;
                    
                    TaskAwaiter<ContactsCache> awaiter2;
                    if (num1 != 0)
                    {
                        if (num1 != 1)
                        {
                            this._8__1 = new ContactsSyncManager.c__DisplayClass9_0();
                            this._8__1._4__this = this._4__this;
                            this._8__1.callback = this.callback;
                            if (!AppGlobalStateManager.Current.GlobalState.AllowSendContacts || this._4__this._isSynching)
                            {
                                if (this._8__1.callback != null)
                                {
                                    this._8__1.callback();
                                    goto label_21;
                                }
                                else
                                    goto label_21;
                            }
                            else
                            {
                                this._4__this._isSynching = true;
                                this._7__wrap1 = this._8__1;
                                List<string> stringList = this._7__wrap1.phoneNumbers;
                                awaiter1 = ContactsSyncManager.GetPhoneContactsAsync().GetAwaiter();
                                if (!awaiter1.IsCompleted)
                                {
                                    this._1__state = 0;
                                    this.u__1 = awaiter1;
                                    this.t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<List<string>>, ContactsSyncManager.d__9>(ref awaiter1, ref this);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            awaiter2 = this.u__2;
                            this.u__2 = new TaskAwaiter<ContactsCache>();
                            this._1__state = -1;
                            goto label_16;
                        }
                    }
                    else
                    {
                        awaiter1 = this.u__1;
                        this.u__1 = new TaskAwaiter<List<string>>();
                        this._1__state = -1;
                    }
                    List<string> result1 = awaiter1.GetResult();
                    awaiter1 = new TaskAwaiter<List<string>>();
                    this._7__wrap1.phoneNumbers = result1;
                    this._7__wrap1 = null;
                    if (this._8__1.phoneNumbers == null)
                    {
                        if (this._8__1.callback != null)
                            this._8__1.callback();
                        this._4__this._isSynching = false;
                        goto label_21;
                    }
                    else
                    {
                        awaiter2 = ContactsSyncManager.RetrieveContactsAsync().GetAwaiter();
                        if (!awaiter2.IsCompleted)
                        {
                            this._1__state = 1;
                            this.u__2 = awaiter2;
                            this.t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<ContactsCache>, ContactsSyncManager.d__9>(ref awaiter2, ref this);
                            return;
                        }
                    }
                label_16:
                    ContactsCache result2 = awaiter2.GetResult();
                    awaiter2 = new TaskAwaiter<ContactsCache>();
                    if (!ContactsSyncManager.AreListsEqual(this._8__1.phoneNumbers, result2.PhoneNumbers))
                    {
                        AccountService.Instance.LookupContacts("phone", "", this._8__1.phoneNumbers, new Action<BackendResult<LookupContactsResponse, ResultCode>>(this._8__1.b__0));
                    }
                    this._4__this._isSynching = false;
                    if (this._8__1.callback != null)
                        this._8__1.callback();
                }
                catch (Exception ex)
                {
                    this._1__state = -2;
                    this.t__builder.SetException(ex);
                    return;
                }
            label_21:
                this._1__state = -2;
                this.t__builder.SetResult();
            }

            //[DebuggerHidden]
            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.t__builder.SetStateMachine(stateMachine);
            }

        }
        public async void Sync(Action callback = null)
        {
            ContactsSyncManager.c__DisplayClass9_0 stateMachine = new ContactsSyncManager.c__DisplayClass9_0();
            stateMachine._4__this = this;
            stateMachine.callback = callback;
            if (!AppGlobalStateManager.Current.GlobalState.AllowSendContacts || this._isSynching)
            {
                if (stateMachine.callback == null)
                    return;
                stateMachine.callback();
            }
            else
            {
                this._isSynching = true;
                ContactsSyncManager.c__DisplayClass9_0 cDisplayClass90_2 = stateMachine;
                List<string> stringList = cDisplayClass90_2.phoneNumbers;
                List<string> phoneContactsAsync = await ContactsSyncManager.GetPhoneContactsAsync();
                cDisplayClass90_2.phoneNumbers = phoneContactsAsync;
                cDisplayClass90_2 = null;
                if (stateMachine.phoneNumbers == null)
                {
                    if (stateMachine.callback != null)
                    {
                        stateMachine.callback();
                    }
                    this._isSynching = false;
                }
                else
                {
                    ContactsCache contactsCache = await ContactsSyncManager.RetrieveContactsAsync();
                    if (!ContactsSyncManager.AreListsEqual(stateMachine.phoneNumbers, contactsCache.PhoneNumbers))
                    {
                        AccountService.Instance.LookupContacts("phone", "", stateMachine.phoneNumbers, new Action<BackendResult<LookupContactsResponse, ResultCode>>(stateMachine.b__0));
                    }
                    this._isSynching = false;
                    if (stateMachine.callback == null)
                        return;
                    stateMachine.callback();
                }
            }

        }

        private static bool AreListsEqual(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
                return false;
            list1.Sort();
            list2.Sort();
            return !list1.Where<string>((Func<string, int, bool>)((t, i) => t != list2[i])).Any<string>();
        }
    }
}
