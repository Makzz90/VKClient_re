using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
    public class AppGlobalStateData : IBinarySerializable
    {
        private object _lockObj = new object();
        private bool _needReferchStickers = true;
        public DateTime _lastTimeShownBDNotification = DateTime.MinValue;
        private bool _showBirthdaysNotifications = true;
        private DateTime _lastDeactTime = DateTime.MinValue;
        private Dictionary<string, List<long>> _uidToListDisabledUidsParsed = new Dictionary<string, List<long>>();
        private Dictionary<string, List<long>> _uidToListDisabledChatIdsParsed = new Dictionary<string, List<long>>();
        private long _maxMessageId;
        private bool _canSendMoneyTransfers;
        private bool _canSendMoneyTransfersToGroups;
        private string _accessToken;
        private bool _gifAutoplayAvailable;
        private bool _isPhotoViewerOrientationLocked;

        public GifAutoplayMode GifAutoplayType { get; set; }

        public List<PendingStatisticsEvent> PendingStatisticsEvents { get; set; }

        public List<StoreProduct> Stickers { get; set; }

        public List<StockItem> StickersStockItems { get; set; }

        public string SupportUri { get; set; }

        public string DeviceId
        {
            get
            {
                return Convert.ToBase64String((byte[])DeviceExtendedProperties.GetValue("DeviceUniqueId"));
            }
        }

        public bool NeedRefetchStickers
        {
            get
            {
                return this._needReferchStickers;
            }
            set
            {
                this._needReferchStickers = value;
            }
        }

        public bool GamesSectionEnabled { get; set; }

        public bool MoneyTransfersEnabled { get; set; }

        public int MoneyTransferMinAmount { get; set; }

        public int MoneyTransferMaxAmount { get; set; }

        public bool CanSendMoneyTransfers
        {
            get
            {
                if (this._canSendMoneyTransfers)
                    return this.MoneyTransfersEnabled;
                return false;
            }
            set
            {
                this._canSendMoneyTransfers = value;
            }
        }

        public bool CanSendMoneyTransfersToGroups
        {
            get
            {
                if (this._canSendMoneyTransfersToGroups)
                    return this.MoneyTransfersEnabled;
                return false;
            }
            set
            {
                this._canSendMoneyTransfersToGroups = value;
            }
        }

        public string AccessToken { get; set; }

        public string Secret { get; set; }

        public long LoggedInUserId { get; set; }

        public DateTime LastTimeShownBSNotification
        {
            get
            {
                return this._lastTimeShownBDNotification;
            }
            set
            {
                this._lastTimeShownBDNotification = value;
            }
        }

        public bool ShowBirthdaysNotifications
        {
            get
            {
                return this._showBirthdaysNotifications;
            }
            set
            {
                this._showBirthdaysNotifications = value;
            }
        }

        public DateTime PushNotificationsBlockedUntil { get; set; }

        public User LoggedInUser { get; set; }

        public bool SyncContacts { get; set; }

        public long MaxMessageId
        {
            get
            {
                return this._maxMessageId;
            }
            set
            {
                lock (this._lockObj)
                {
                    if (value <= this._maxMessageId)
                        return;
                    this._maxMessageId = value;
                }
            }
        }

        public int TipsShownCount { get; set; }

        public string NotificationsUri { get; set; }

        public long LastTS { get; set; }

        public bool VibrationsEnabled { get; set; }

        public bool SoundEnabled { get; set; }

        public bool NotificationsEnabled { get; set; }

        public int ServerMinusLocalTimeDelta { get; set; }

        public bool MessageNotificationsEnabled { get; set; }

        public bool FriendsNotificationsEnabled { get; set; }

        public bool MentionsNotificationsEnabled { get; set; }

        public bool ReplyNotificationsEnabled { get; set; }

        public bool MessageTextInNotification { get; set; }

        public bool PushNotificationsEnabled { get; set; }

        public PushSettings PushSettings { get; set; }

        public int FavoritesDefaultSection { get; set; }

        public string DefaultVideoResolution { get; set; }

        public string RegisteredDeviceId { get; set; }

        public bool AllowToastNotificationsQuestionAsked { get; set; }

        public bool AllowUseLocationQuestionAsked { get; set; }

        public bool AllowUseLocation { get; set; }

        public bool AllowSendContacts { get; set; }

        public PickableItem SelectedNewsSource { get; set; }

        public int FriendListOrder { get; set; }

        public bool CompressPhotosOnUpload { get; set; }

        public bool SaveLocationDataOnUpload { get; set; }

        public bool SaveEditedPhotos { get; set; }

        public bool LoadBigPhotosOverMobile { get; set; }

        public bool IsMusicCachingEnabled { get; set; }

        public GamesVisitSource GamesVisitSource { get; set; }

        public List<long> MyGamesIds { get; set; }

        public string BaseDomain { get; set; }

        public string BaseLoginDomain { get; set; }

        public bool ForceStatsSend { get; set; }

        public bool NewsfeedTopEnabled { get; set; }

        public bool AudioRecordingMaxDemo { get; set; }

        private bool CanUseInApps { get; set; }

        public bool StickersAutoSuggestEnabled { get; set; }

        public AccountPaymentType PaymentType { get; set; }

        public int NewStoreItemsCount { get; set; }

        public bool HasStickersUpdates { get; set; }

        public StoreStickers RecentStickers { get; set; }

        public DateTime LastDeactivatedTime
        {
            get
            {
                return this._lastDeactTime;
            }
            set
            {
                this._lastDeactTime = value;
            }
        }

        public bool GifAutoplayFeatureAvailable
        {
            get
            {
                if (this.GifAutoplayManualSetting.HasValue)
                    return this.GifAutoplayManualSetting.Value;
                return this._gifAutoplayAvailable;
            }
            set
            {
                this._gifAutoplayAvailable = value;
            }
        }

        public bool? GifAutoplayManualSetting { get; set; }

        public bool? AdsDemoManualSetting { get; set; }

        public bool PhotoFeedMoveHintShown { get; set; }

        public bool IsPhotoViewerOrientationLocked
        {
            get
            {
                return this._isPhotoViewerOrientationLocked;
            }
            set
            {
                this._isPhotoViewerOrientationLocked = value;
                EventAggregator.Current.Publish(new PhotoViewerOrientationLockedModeChanged());
            }
        }

        public bool DebugDisabled { get; set; }
        //
        public bool IsLogsEnabled { get; set; }
        public int UserAvatarRadius { get; set; }
        public int NotifyRadius { get; set; }
        public bool HideSystemTray { get; set; }
        public bool HideADs { get; set; }
        public bool HideFriendsRecommended { get; set; }
        //
        public AppGlobalStateData()
        {
            this.LoggedInUser = new User();
            this.SoundEnabled = true;
            this.VibrationsEnabled = true;
            this.NotificationsEnabled = true;
            this.SyncContacts = true;
            this.PendingStatisticsEvents = new List<PendingStatisticsEvent>();
            this.CompressPhotosOnUpload = true;
            this.SaveEditedPhotos = false;
            this.LoadBigPhotosOverMobile = true;
            this.IsMusicCachingEnabled = true;//mod:false
            this.SaveLocationDataOnUpload = true;
            this.PushSettings = new PushSettings();
            this.FavoritesDefaultSection = 0;
            this.DefaultVideoResolution = "360";
            this.StickersAutoSuggestEnabled = true;

            this.GifAutoplayType = GifAutoplayMode.Always;

            this.IsLogsEnabled = false;
            this.UserAvatarRadius = 1;
            this.NotifyRadius = 3;
            this.HideSystemTray = false;
            this.HideADs = true;
            this.HideFriendsRecommended = true;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(38);
            writer.WriteString(this.AccessToken);
            writer.Write(this.LoggedInUserId);
            writer.Write(this.MaxMessageId);
            writer.Write(this.LastTS);
            writer.WriteString(this.NotificationsUri);
            writer.Write(this.PushNotificationsBlockedUntil);
            writer.Write(this.VibrationsEnabled);
            writer.Write(this.SoundEnabled);
            writer.Write(this.NotificationsEnabled);
            writer.WriteString(this.Secret);
            writer.Write(this.MessageNotificationsEnabled);
            writer.Write(this.AllowUseLocationQuestionAsked);
            writer.Write(this.AllowUseLocation);
            writer.Write<User>(this.LoggedInUser, false);
            writer.Write(this.AllowToastNotificationsQuestionAsked);
            writer.Write(this.FriendsNotificationsEnabled);
            writer.Write(this.MentionsNotificationsEnabled);
            writer.Write(this.ReplyNotificationsEnabled);
            writer.Write(this.MessageTextInNotification);
            writer.Write(this.ServerMinusLocalTimeDelta);
            writer.Write(this.LastDeactivatedTime);
            writer.WriteDictionary(this.ConvertToDictStringString(this._uidToListDisabledUidsParsed));
            writer.WriteDictionary(this.ConvertToDictStringString(this._uidToListDisabledChatIdsParsed));
            writer.Write<PickableItem>(this.SelectedNewsSource, false);
            writer.Write(this.SyncContacts);
            writer.WriteList<PendingStatisticsEvent>((IList<PendingStatisticsEvent>)this.PendingStatisticsEvents, 10000);
            writer.WriteList<StoreProduct>((IList<StoreProduct>)this.Stickers, 10000);
            writer.Write(this.LastTimeShownBSNotification);
            writer.Write(this.ShowBirthdaysNotifications);
            writer.WriteString(this.SupportUri);
            writer.Write(this.TipsShownCount);
            writer.Write(this.FriendListOrder);
            writer.Write(this.CompressPhotosOnUpload);
            writer.Write(this.SaveEditedPhotos);
            writer.Write(this.LoadBigPhotosOverMobile);
            writer.Write(this.IsMusicCachingEnabled);
            writer.Write(this.SaveLocationDataOnUpload);
            writer.Write<PushSettings>(this.PushSettings, false);
            writer.WriteString(this.RegisteredDeviceId);
            writer.Write(this.PushNotificationsEnabled);
            writer.Write((int)this.GamesVisitSource);
            writer.WriteList(this.MyGamesIds);
            writer.Write(this.FavoritesDefaultSection);
            writer.Write(this.AllowSendContacts);
            writer.Write(this.GamesSectionEnabled);
            writer.Write(this.DefaultVideoResolution);
            writer.WriteString(this.BaseDomain);
            writer.WriteString(this.BaseLoginDomain);
            writer.Write(this.ForceStatsSend);
            writer.Write(this.NewsfeedTopEnabled);
            writer.Write((int)this.GifAutoplayType);
            writer.Write(this._gifAutoplayAvailable);
            writer.WriteBoolNullable(this.GifAutoplayManualSetting);
            writer.Write(this.CanUseInApps);
            writer.Write(this.StickersAutoSuggestEnabled);
            writer.WriteList<StockItem>((IList<StockItem>)this.StickersStockItems, 10000);
            writer.Write((int)this.PaymentType);
            writer.Write(this.NewStoreItemsCount);
            writer.Write(this.HasStickersUpdates);
            writer.Write(this.PhotoFeedMoveHintShown);
            writer.Write(this.MoneyTransfersEnabled);
            writer.Write(this._canSendMoneyTransfers);
            writer.Write(this.MoneyTransferMinAmount);
            writer.Write(this.MoneyTransferMaxAmount);
            writer.Write(this.IsPhotoViewerOrientationLocked);
            writer.Write(this.DebugDisabled);
            writer.WriteBoolNullable(this.AdsDemoManualSetting);
            writer.Write(this.CanSendMoneyTransfersToGroups);
            writer.Write(this.AudioRecordingMaxDemo);
            //
            writer.Write(this.IsLogsEnabled);
            writer.Write(this.UserAvatarRadius);
            writer.Write(this.NotifyRadius);
            writer.Write(this.HideSystemTray);
            writer.Write(this.HideADs);
            writer.Write(this.HideFriendsRecommended);
        }

        public void Read(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            this.AccessToken = reader.ReadString();
            this.LoggedInUserId = reader.ReadInt64();
            this.MaxMessageId = reader.ReadInt64();
            this.LastTS = reader.ReadInt64();
            this.NotificationsUri = reader.ReadString();
            this.PushNotificationsBlockedUntil = reader.ReadDateTime();
            this.VibrationsEnabled = reader.ReadBoolean();
            this.SoundEnabled = reader.ReadBoolean();
            this.NotificationsEnabled = reader.ReadBoolean();
            this.Secret = reader.ReadString();
            this.MessageNotificationsEnabled = reader.ReadBoolean();
            this.AllowUseLocationQuestionAsked = reader.ReadBoolean();
            this.AllowUseLocation = reader.ReadBoolean();
            this.LoggedInUser = reader.ReadGeneric<User>();
            this.AllowToastNotificationsQuestionAsked = reader.ReadBoolean();
            this.FriendsNotificationsEnabled = reader.ReadBoolean();
            this.MentionsNotificationsEnabled = reader.ReadBoolean();
            this.ReplyNotificationsEnabled = reader.ReadBoolean();
            this.MessageTextInNotification = reader.ReadBoolean();
            this.ServerMinusLocalTimeDelta = reader.ReadInt32();
            this.LastDeactivatedTime = reader.ReadDateTime();
            if (num >= 2)
            {
                this._uidToListDisabledUidsParsed = this.ConvertToDictStringListLong(reader.ReadDictionary());
                this._uidToListDisabledChatIdsParsed = this.ConvertToDictStringListLong(reader.ReadDictionary());
            }
            if (num >= 3)
                this.SelectedNewsSource = reader.ReadGeneric<PickableItem>();
            if (num >= 4)
                this.SyncContacts = reader.ReadBoolean();
            if (num >= 5)
                this.PendingStatisticsEvents = reader.ReadList<PendingStatisticsEvent>();
            if (num >= 6)
                this.Stickers = reader.ReadList<StoreProduct>();
            if (num >= 7)
            {
                this.LastTimeShownBSNotification = reader.ReadDateTime();
                this.ShowBirthdaysNotifications = reader.ReadBoolean();
            }
            if (num >= 8)
                this.SupportUri = reader.ReadString();
            if (num >= 9)
                this.TipsShownCount = reader.ReadInt32();
            if (num >= 10)
            {
                this.FriendListOrder = reader.ReadInt32();
                this.CompressPhotosOnUpload = reader.ReadBoolean();
                this.SaveEditedPhotos = reader.ReadBoolean();
                this.LoadBigPhotosOverMobile = reader.ReadBoolean();
                this.IsMusicCachingEnabled = reader.ReadBoolean();
                //this.IsMusicCachingEnabled = false;//mod
            }
            if (num >= 11)
                this.SaveLocationDataOnUpload = reader.ReadBoolean();
            bool flag = false;
            if (num >= 12)
            {
                flag = true;
                this.PushSettings = reader.ReadGeneric<PushSettings>();
                this.RegisteredDeviceId = reader.ReadString();
                this.PushNotificationsEnabled = reader.ReadBoolean();
            }
            if (!flag)
                this.MigratePushSettingsFromOldSettings();
            if (num >= 13)
            {
                this.GamesVisitSource = (GamesVisitSource)reader.ReadInt32();
                this.MyGamesIds = reader.ReadListLong();
            }
            if (num >= 14)
                this.FavoritesDefaultSection = reader.ReadInt32();
            if (num >= 15)
                this.AllowSendContacts = reader.ReadBoolean();
            if (num >= 16)
                this.GamesSectionEnabled = reader.ReadBoolean();
            if (num >= 17)
                this.DefaultVideoResolution = reader.ReadString();
            if (num >= 18)
            {
                this.BaseDomain = reader.ReadString();
                this.BaseLoginDomain = reader.ReadString();
            }
            if (num >= 19)
                this.ForceStatsSend = reader.ReadBoolean();
            if (num >= 20)
                this.NewsfeedTopEnabled = reader.ReadBoolean();
            if (num >= 21)
                this.GifAutoplayType = (GifAutoplayMode)reader.ReadInt32();
            if (num >= 22)
                this._gifAutoplayAvailable = reader.ReadBoolean();
            if (num >= 23)
                this.GifAutoplayManualSetting = reader.ReadBoolNullable();
            if (num >= 24)
                this.CanUseInApps = reader.ReadBoolean();
            if (num >= 25)
                this.StickersAutoSuggestEnabled = reader.ReadBoolean();
            if (num >= 26)
            {
                this.StickersStockItems = reader.ReadList<StockItem>();
                this.PaymentType = (AccountPaymentType)reader.ReadInt32();
            }
            if (num >= 27)
            {
                this.NewStoreItemsCount = reader.ReadInt32();
                this.HasStickersUpdates = reader.ReadBoolean();
            }
            if (num >= 29)
                this.PhotoFeedMoveHintShown = reader.ReadBoolean();
            if (num >= 30)
                this.MoneyTransfersEnabled = reader.ReadBoolean();
            if (num >= 31)
            {
                if (num < 34)
                    reader.ReadBoolean();
                this._canSendMoneyTransfers = reader.ReadBoolean();
            }
            if (num >= 32)
            {
                this.MoneyTransferMinAmount = reader.ReadInt32();
                this.MoneyTransferMaxAmount = reader.ReadInt32();
            }
            if (num >= 33)
                this.IsPhotoViewerOrientationLocked = reader.ReadBoolean();
            if (num >= 35)
                this.DebugDisabled = reader.ReadBoolean();
            if (num >= 36)
                this.AdsDemoManualSetting = reader.ReadBoolNullable();
            if (num >= 37)
                this.CanSendMoneyTransfersToGroups = reader.ReadBoolean();
            if (num >= 38)
                this.AudioRecordingMaxDemo = reader.ReadBoolean();
            //
            try
            {
                this.IsLogsEnabled = reader.ReadBoolean();
                this.UserAvatarRadius = reader.ReadInt32();
                this.NotifyRadius = reader.ReadInt32();
                this.HideSystemTray = reader.ReadBoolean();
                this.HideADs = reader.ReadBoolean();
                this.HideFriendsRecommended = reader.ReadBoolean();
            }
            catch
            {

            }
        }

        private void MigratePushSettingsFromOldSettings()
        {
            PushSettings pushSettings = new PushSettings();
            pushSettings.msg = pushSettings.chat = this.MessageNotificationsEnabled;
            pushSettings.msg_no_text = pushSettings.chat_no_text = !this.MessageTextInNotification;
            pushSettings.friend = this.FriendsNotificationsEnabled;
            pushSettings.mention = this.MentionsNotificationsEnabled;
            pushSettings.reply = this.ReplyNotificationsEnabled;
            this.PushSettings = pushSettings;
            this.PushNotificationsEnabled = pushSettings.msg || pushSettings.chat || (pushSettings.friend || pushSettings.mention) || pushSettings.reply;
            if (!this.PushNotificationsEnabled)
                return;
            pushSettings.comment = true;
            pushSettings.event_soon = true;
            pushSettings.event_soon = true;
            pushSettings.friend_accepted = true;
            pushSettings.friend_found = true;
            pushSettings.group_accepted = true;
            pushSettings.group_invite = true;
            pushSettings.like = true;
            pushSettings.new_post = true;
            pushSettings.reply = true;
            pushSettings.repost = true;
            pushSettings.wall_post = true;
            pushSettings.wall_publish = true;
        }

        private Dictionary<string, string> ConvertToDictStringString(Dictionary<string, List<long>> dict)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string key in dict.Keys)
                dictionary[key] = dict[key].GetCommaSeparated();
            return dictionary;
        }

        private Dictionary<string, List<long>> ConvertToDictStringListLong(Dictionary<string, string> dict)
        {
            Dictionary<string, List<long>> dictionary = new Dictionary<string, List<long>>();
            foreach (string key in dict.Keys)
                dictionary[key] = dict[key].ParseCommaSeparated();
            return dictionary;
        }

        internal void ResetForNewUser()
        {
            this.AccessToken = "";
            this.LoggedInUserId = 0L;
            this.MaxMessageId = 0L;
            this.LastTS = 0L;
            this.NotificationsUri = "";
            this.PushNotificationsBlockedUntil = DateTime.MinValue;
            this.Secret = "";
            this.LoggedInUser = new User();
            this.SelectedNewsSource = null;
            this.LastTimeShownBSNotification = DateTime.MinValue;
            this.PendingStatisticsEvents.Clear();
            this.SupportUri = "";
        }
    }
}
