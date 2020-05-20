using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Audio.Base.Library
{
    public class FriendsCache
    {
        private static FriendsCache _instance;
        private SavedContacts _sc;

        private string Key
        {
            get
            {
                return "FriendsOf_" + AppGlobalStateManager.Current.LoggedInUserId;
            }
        }

        public static FriendsCache Instance
        {
            get
            {
                if (FriendsCache._instance == null)
                    FriendsCache._instance = new FriendsCache();
                return FriendsCache._instance;
            }
        }

        public async Task<SavedContacts> GetFriends()
        {
            if (this._sc == null || this._sc.CurrentUserId != AppGlobalStateManager.Current.LoggedInUserId)
            {
                SavedContacts sc = new SavedContacts();
                if (!await CacheManager.TryDeserializeAsync((IBinarySerializable)sc, this.Key, CacheManager.DataType.CachedData))
                    return sc;
                this._sc = sc;
                sc = null;
            }
            return this._sc;
        }

        public async void SetFriends(List<User> friends, FriendRequests requests)
        {
            if (friends == null && this._sc != null)
                friends = this._sc.SavedUsers;
            if (requests == null && this._sc != null)
                requests = this._sc.Requests;
            SavedContacts savedContacts = new SavedContacts();
            savedContacts.SyncedDate = DateTime.UtcNow;
            savedContacts.SavedUsers = friends;
            savedContacts.Requests = requests;
            savedContacts.CurrentUserId = AppGlobalStateManager.Current.LoggedInUserId;
            this._sc = savedContacts;
            EventAggregator.Current.Publish(new FriendsCacheUpdated());
            await CacheManager.TrySerializeAsync(savedContacts, this.Key, false, CacheManager.DataType.CachedData);
        }
    }
}
