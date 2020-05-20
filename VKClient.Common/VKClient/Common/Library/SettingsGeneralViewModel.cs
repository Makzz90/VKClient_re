using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.AudioCache;
using VKClient.Audio.Base.Events;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
    public class SettingsGeneralViewModel : ViewModelBase
    {
        private bool _isInProgress;
        private bool _shownAppRestartNeededMB;
        private bool _clearingCache;

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
                return this.GifAutoplayTypes.First<BGType>((Func<BGType, bool>)(g => (GifAutoplayMode)g.id == AppGlobalStateManager.Current.GlobalState.GifAutoplayType));
            }
            set
            {
                if (value == null)
                    return;
                AppGlobalStateManager.Current.GlobalState.GifAutoplayType = (GifAutoplayMode)value.id;
                this.NotifyPropertyChanged<BGType>((System.Linq.Expressions.Expression<Func<BGType>>)(() => this.GifAutoplayType));
                this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>)(() => this.GifAutoplayDesc));
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
                return this.FriendListOrders.FirstOrDefault<BGType>((Func<BGType, bool>)(t => t.id == AppGlobalStateManager.Current.GlobalState.FriendListOrder));
            }
            set
            {
                if (value == null)
                    return;
                AppGlobalStateManager.Current.GlobalState.FriendListOrder = value.id;
                this.NotifyPropertyChanged<BGType>((System.Linq.Expressions.Expression<Func<BGType>>)(() => this.FriendListOrder));
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
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsInProgress));
                this.SetInProgress(this._isInProgress, "");
            }
        }

        public BGType Language
        {
            get
            {
                ThemeSettings settings = ThemeSettingsManager.GetThemeSettings();
                return this.Languages.FirstOrDefault<BGType>((Func<BGType, bool>)(l => l.id == settings.LanguageSettings));
            }
            set
            {
                if (value == null)
                    return;
                ThemeSettings themeSettings = ThemeSettingsManager.GetThemeSettings();
                int id = value.id;
                themeSettings.LanguageSettings = id;
                ThemeSettingsManager.SetThemeSettings(themeSettings);
                this.NotifyPropertyChanged<BGType>((System.Linq.Expressions.Expression<Func<BGType>>)(() => this.Language));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.LanguageSettingsChangedVisibility));
                if (this._shownAppRestartNeededMB || !this.LanguageSettingsChanged)
                    return;
                int num = (int)MessageBox.Show(CommonResources.Settings_AppliedAfterRestart, CommonResources.AppRestartNeeded, MessageBoxButton.OK);
                this._shownAppRestartNeededMB = true;
            }
        }

        public BGType BackgroundType
        {
            get
            {
                ThemeSettings settings = ThemeSettingsManager.GetThemeSettings();
                return this.BackgroundTypes.FirstOrDefault<BGType>((Func<BGType, bool>)(b => b.id == settings.BackgroundSettings));
            }
            set
            {
                if (value == null)
                    return;
                ThemeSettings themeSettings = ThemeSettingsManager.GetThemeSettings();
                int id = value.id;
                themeSettings.BackgroundSettings = id;
                ThemeSettingsManager.SetThemeSettings(themeSettings);
                this.NotifyPropertyChanged<BGType>((System.Linq.Expressions.Expression<Func<BGType>>)(() => this.BackgroundType));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.BGSettingsChangedVisibility));
                if (this._shownAppRestartNeededMB || !this.BGSettingsChanged)
                    return;
                int num = (int)MessageBox.Show(CommonResources.Settings_AppliedAfterRestart, CommonResources.AppRestartNeeded, MessageBoxButton.OK);
                this._shownAppRestartNeededMB = true;
            }
        }

        public BGType TileColor
        {
            get
            {
                ThemeSettings settings = ThemeSettingsManager.GetThemeSettings();
                return this.AccentTypes.FirstOrDefault<BGType>((Func<BGType, bool>)(a => a.id == settings.TileSettings));
            }
            set
            {
                if (value == null)
                    return;
                ThemeSettings themeSettings = ThemeSettingsManager.GetThemeSettings();
                int id = value.id;
                themeSettings.TileSettings = id;
                ThemeSettingsManager.SetThemeSettings(themeSettings);
                this.NotifyPropertyChanged<BGType>((System.Linq.Expressions.Expression<Func<BGType>>)(() => this.TileColor));
                TileManager.Instance.UpdateTileColor();
            }
        }

        public Visibility BGSettingsChangedVisibility
        {
            get
            {
                return !this.BGSettingsChanged ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility LanguageSettingsChangedVisibility
        {
            get
            {
                return !this.LanguageSettingsChanged ? Visibility.Collapsed : Visibility.Visible;
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
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsAllowUseLocation));
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
                    EventAggregator.Current.Publish((object)new ContactsSyncEnabled());
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsAllowSendContacts));
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
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CompressPhotosOnUpload));
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
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.SaveLocationOnUpload));
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
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.SaveEditedPhotos));
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
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.LoadBigPhotosOverMobile));
            }
        }

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
    }
}
