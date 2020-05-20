using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

using VKClient.Audio.Base.AudioCache;//mod
namespace VKClient.Common.Library
{
    public class SettingsGeneralViewModel : ViewModelBase
    {
        private bool _isInProgress;
        private bool _shownAppRestartNeededMB;
        private bool _clearingCache;//mod

        public List<BGType> BackgroundTypes
        {
            get
            {
                return BGTypes.GetBGTypes();
            }
        }

        public List<BGType> AccentTypes
        {
            get
            {
                return AccentColorTypes.GetAccentTypes();
            }
        }

        public List<BGType> GifAutoplayTypes
        {
            get
            {
                return BGAutoplayTypes.GetBGTypes();
            }
        }

        public BGType GifAutoplayType
        {
            get
            {
                return (BGType)Enumerable.First<BGType>(this.GifAutoplayTypes, (Func<BGType, bool>)(g => (GifAutoplayMode)g.id == AppGlobalStateManager.Current.GlobalState.GifAutoplayType));
            }
            set
            {
                if (value == null)
                    return;
                AppGlobalStateManager.Current.GlobalState.GifAutoplayType = (GifAutoplayMode)value.id;
                this.NotifyPropertyChanged<BGType>(() => this.GifAutoplayType);
                this.NotifyPropertyChanged<string>(() => this.GifAutoplayDesc);
            }
        }

        public Visibility GifAutoplayFeatureAvailableVisibility
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.GifAutoplayFeatureAvailable.ToVisiblity();
            }
        }

        public string GifAutoplayDesc
        {
            get
            {
                switch (this.GifAutoplayType.id)
                {
                    case 0:
                        return CommonResources.Settings_General_AutoplayGifWiFiOnlyDesc;
                    case 1:
                        return CommonResources.Settings_General_AutoplayGifAlwaysDesc;
                    case 2:
                        return CommonResources.Settings_General_AutoplayGifNeverDesc;
                    default:
                        return "";
                }
            }
        }

        public List<BGType> Languages
        {
            get
            {
                return LanguagesList.GetLanguages();
            }
        }

        public List<BGType> FriendListOrders
        {
            get
            {
                return FriedListOrdersList.GetOrders();
            }
        }

        public BGType FriendListOrder
        {
            get
            {
                return (BGType)Enumerable.FirstOrDefault<BGType>(this.FriendListOrders, (Func<BGType, bool>)(t => t.id == AppGlobalStateManager.Current.GlobalState.FriendListOrder));
            }
            set
            {
                if (value == null)
                    return;
                AppGlobalStateManager.Current.GlobalState.FriendListOrder = value.id;
                this.NotifyPropertyChanged<BGType>(() => this.FriendListOrder);
            }
        }

        public new bool IsInProgress
        {
            get
            {
                return this._isInProgress;
            }
            set
            {
                this._isInProgress = value;
                this.NotifyPropertyChanged<bool>(() => this.IsInProgress);
                this.SetInProgress(this._isInProgress, "");
            }
        }

        public BGType Language
        {
            get
            {
                ThemeSettings settings = ThemeSettingsManager.GetThemeSettings();
                return (BGType)Enumerable.FirstOrDefault<BGType>(this.Languages, (Func<BGType, bool>)(l => l.id == settings.LanguageSettings));
            }
            set
            {
                if (value == null)
                    return;
                ThemeSettings themeSettings = ThemeSettingsManager.GetThemeSettings();
                int id = value.id;
                themeSettings.LanguageSettings = id;
                ThemeSettingsManager.SetThemeSettings(themeSettings);
                this.NotifyPropertyChanged<BGType>(() => this.Language);
                this.NotifyPropertyChanged<Visibility>(() => this.LanguageSettingsChangedVisibility);
                if (this._shownAppRestartNeededMB || !this.LanguageSettingsChanged)
                    return;
                MessageBox.Show(CommonResources.Settings_AppliedAfterRestart, CommonResources.AppRestartNeeded, (MessageBoxButton)0);
                this._shownAppRestartNeededMB = true;
            }
        }

        public BGType BackgroundType
        {
            get
            {
                ThemeSettings settings = ThemeSettingsManager.GetThemeSettings();
                return (BGType)Enumerable.FirstOrDefault<BGType>(this.BackgroundTypes, (Func<BGType, bool>)(b => b.id == settings.BackgroundSettings));
            }
            set
            {
                if (value == null)
                    return;
                ThemeSettings themeSettings = ThemeSettingsManager.GetThemeSettings();
                int id = value.id;
                themeSettings.BackgroundSettings = id;
                ThemeSettingsManager.SetThemeSettings(themeSettings);
                this.NotifyPropertyChanged<BGType>(() => this.BackgroundType);
                this.NotifyPropertyChanged<Visibility>(() => this.BGSettingsChangedVisibility);
                if (this._shownAppRestartNeededMB || !this.BGSettingsChanged)
                    return;
                MessageBox.Show(CommonResources.Settings_AppliedAfterRestart, CommonResources.AppRestartNeeded, (MessageBoxButton)0);
                this._shownAppRestartNeededMB = true;
            }
        }

        public BGType TileColor
        {
            get
            {
                ThemeSettings settings = ThemeSettingsManager.GetThemeSettings();
                return (BGType)Enumerable.FirstOrDefault<BGType>(this.AccentTypes, (Func<BGType, bool>)(a => a.id == settings.TileSettings));
            }
            set
            {
                if (value == null)
                    return;
                ThemeSettings themeSettings = ThemeSettingsManager.GetThemeSettings();
                int id = value.id;
                themeSettings.TileSettings = id;
                ThemeSettingsManager.SetThemeSettings(themeSettings);
                this.NotifyPropertyChanged<BGType>(() => this.TileColor);
                TileManager.Instance.UpdateTileColor();
            }
        }

        public Visibility BGSettingsChangedVisibility
        {
            get
            {
                if (!this.BGSettingsChanged)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public Visibility LanguageSettingsChangedVisibility
        {
            get
            {
                if (!this.LanguageSettingsChanged)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public bool BGSettingsChanged
        {
            get
            {
                return AppliedSettingsInfo.AppliedBGSetting != ThemeSettingsManager.GetThemeSettings().BackgroundSettings;
            }
        }

        public bool LanguageSettingsChanged
        {
            get
            {
                return AppliedSettingsInfo.AppliedLanguageSetting != ThemeSettingsManager.GetThemeSettings().LanguageSettings;
            }
        }

        public bool IsPhoneIntegrationEnabled
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.SyncContacts;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.SyncContacts)
                    return;
                AppGlobalStateManager.Current.GlobalState.SyncContacts = value;
                ContactsManager.Instance.EnsureInSyncAsync(false);
            }
        }

        public bool IsAllowUseLocation
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.AllowUseLocation;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
                    return;
                AppGlobalStateManager.Current.GlobalState.AllowUseLocation = value;
                AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked = true;
                this.NotifyPropertyChanged<bool>(() => this.IsAllowUseLocation);
            }
        }

        public bool IsAllowSendContacts
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.AllowSendContacts;
            }
            set
            {
                if (this._isInProgress || value == this.IsAllowSendContacts)
                    return;
                AppGlobalStateManager.Current.GlobalState.AllowSendContacts = value;
                if (value)
                    EventAggregator.Current.Publish(new ContactsSyncEnabled());
                this.NotifyPropertyChanged<bool>(() => this.IsAllowSendContacts);
            }
        }

        public bool CompressPhotosOnUpload
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.CompressPhotosOnUpload;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.CompressPhotosOnUpload)
                    return;
                AppGlobalStateManager.Current.GlobalState.CompressPhotosOnUpload = value;
                this.NotifyPropertyChanged<bool>(() => this.CompressPhotosOnUpload);
            }
        }

        public bool SaveLocationOnUpload
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.SaveLocationDataOnUpload;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.SaveLocationDataOnUpload)
                    return;
                AppGlobalStateManager.Current.GlobalState.SaveLocationDataOnUpload = value;
                this.NotifyPropertyChanged<bool>(() => this.SaveLocationOnUpload);
            }
        }

        public bool SaveEditedPhotos
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.SaveEditedPhotos;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.SaveEditedPhotos)
                    return;
                AppGlobalStateManager.Current.GlobalState.SaveEditedPhotos = value;
                this.NotifyPropertyChanged<bool>(() => this.SaveEditedPhotos);
            }
        }

        public bool LoadBigPhotosOverMobile
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.LoadBigPhotosOverMobile;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.LoadBigPhotosOverMobile)
                    return;
                AppGlobalStateManager.Current.GlobalState.LoadBigPhotosOverMobile = value;
                this.NotifyPropertyChanged<bool>(() => this.LoadBigPhotosOverMobile);
            }
        }


        //mod:
        public bool IsMusicCachingEnabled
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.IsMusicCachingEnabled;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.IsMusicCachingEnabled)
                    return;
                AppGlobalStateManager.Current.GlobalState.IsMusicCachingEnabled = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsMusicCachingEnabled));
                AppGlobalStateManager.Current.SaveState();
            }
        }

        public async void ClearMusicCache()
        {
            if (this._clearingCache)
                return;
            this._clearingCache = true;
            this.SetInProgress(true, "");
            await AudioCacheManager.Instance.ClearCache();
            this.SetInProgress(false, "");
            this._clearingCache = false;
        }
        //
        public double UserAvatarRadius
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.UserAvatarRadius;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.UserAvatarRadius)
                    return;
                AppGlobalStateManager.Current.GlobalState.UserAvatarRadius = (int)value;
                this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.UserAvatarRadius));
                AppGlobalStateManager.Current.SaveState();
            }
        }
        public double NotifyRadius
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.NotifyRadius;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.NotifyRadius)
                    return;
                AppGlobalStateManager.Current.GlobalState.NotifyRadius = (int)value;
                this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>)(() => this.NotifyRadius));
                AppGlobalStateManager.Current.SaveState();
            }
        }

        public bool HideSystemTray
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.HideSystemTray;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.HideSystemTray)
                    return;
                AppGlobalStateManager.Current.GlobalState.HideSystemTray = (bool)value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.HideSystemTray));
                AppGlobalStateManager.Current.SaveState();
            }
        }

        public bool HideADs
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.HideADs;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.HideADs)
                    return;
                AppGlobalStateManager.Current.GlobalState.HideADs = (bool)value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.HideADs));
                AppGlobalStateManager.Current.SaveState();
            }
        }

        public bool HideFriendsRecommended
        {
            get
            {
                return AppGlobalStateManager.Current.GlobalState.HideFriendsRecommended;
            }
            set
            {
                if (value == AppGlobalStateManager.Current.GlobalState.HideFriendsRecommended)
                    return;
                AppGlobalStateManager.Current.GlobalState.HideFriendsRecommended = (bool)value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.HideFriendsRecommended));
                AppGlobalStateManager.Current.SaveState();
            }
        }
        //
        
    }
}
