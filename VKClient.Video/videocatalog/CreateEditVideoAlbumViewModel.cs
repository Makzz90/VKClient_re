using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Video.VideoCatalog
{
    public class CreateEditVideoAlbumViewModel : ViewModelBase
    {
        private long _albumId;
        private long _groupId;
        private string _name;
        private PrivacyInfo _pi;
        private EditPrivacyViewModel _albumPrivacyVM;
        private bool _isSaving;

        public bool IsNewAlbum
        {
            get
            {
                return this._albumId == 0L;
            }
        }

        public string Title
        {
            get
            {
                if (!this.IsNewAlbum)
                    return CommonResources.VideoCatalog_EditVideoAlbum;
                return CommonResources.VideoCatalog_NewVideoAlbum;
            }
        }

        public Visibility AllowEditPrivacyVisibility
        {
            get
            {
                if (this._groupId == 0L)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.Name));
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.CanSave));
            }
        }

        public EditPrivacyViewModel AlbumPrivacyVM
        {
            get
            {
                return this._albumPrivacyVM;
            }
            set
            {
                this._albumPrivacyVM = value;
                this.NotifyPropertyChanged<EditPrivacyViewModel>((Expression<Func<EditPrivacyViewModel>>)(() => this.AlbumPrivacyVM));
            }
        }

        public bool IsSaving
        {
            get
            {
                return this._isSaving;
            }
            set
            {
                this._isSaving = value;
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsSaving));
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.CanSave));
            }
        }

        public bool CanSave
        {
            get
            {
                if (!this._isSaving)
                    return !string.IsNullOrWhiteSpace(this.Name);
                return false;
            }
        }

        public CreateEditVideoAlbumViewModel(long albumId = 0, long groupId = 0, string name = "", PrivacyInfo pi = null)
        {
            this._albumId = albumId;
            this._groupId = groupId;
            this._name = name;
            this._pi = pi ?? new PrivacyInfo();
            this.AlbumPrivacyVM = new EditPrivacyViewModel(CommonResources.VideoCatalog_WhoCanViewThisAlbum, this._pi, "", (List<string>)null);
        }

        public void Save(Action<bool> resultCallback)
        {
            this._isSaving = true;
            this.SetInProgress(true, "");
            if (this.IsNewAlbum)
                VideoService.Instance.AddAlbum(this.Name, this.AlbumPrivacyVM.GetAsPrivacyInfo(), (Action<BackendResult<VideoAlbum, ResultCode>>)(res =>
                {
                    this._isSaving = false;
                    this.SetInProgress(false, "");
                    if (res.ResultCode != ResultCode.Succeeded)
                    {
                        Execute.ExecuteOnUIThread((Action)(() => GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null)));
                    }
                    else
                    {
                        EventAggregator current = EventAggregator.Current;
                        VideoAlbumAddedDeletedEvent addedDeletedEvent = new VideoAlbumAddedDeletedEvent();
                        addedDeletedEvent.AlbumId = this._albumId;
                        addedDeletedEvent.OwnerId = this._groupId > 0L ? -this._groupId : AppGlobalStateManager.Current.LoggedInUserId;
                        int num = 1;
                        addedDeletedEvent.IsAdded = num != 0;
                        current.Publish((object)addedDeletedEvent);
                    }
                    resultCallback(res.ResultCode == ResultCode.Succeeded);
                }), new long?(this._groupId));
            else
                VideoService.Instance.EditAlbum(this.Name, this._albumId, this.AlbumPrivacyVM.GetAsPrivacyInfo(), (Action<BackendResult<object, ResultCode>>)(res =>
                {
                    this._isSaving = false;
                    this.SetInProgress(false, "");
                    if (res.ResultCode != ResultCode.Succeeded)
                        Execute.ExecuteOnUIThread((Action)(() => GenericInfoUC.ShowBasedOnResult((int)res.ResultCode, "", null)));
                    else
                        EventAggregator.Current.Publish((object)new VideoAlbumEditedEvent()
                        {
                            AlbumId = this._albumId,
                            OwnerId = (this._groupId > 0L ? -this._groupId : AppGlobalStateManager.Current.LoggedInUserId),
                            Name = this.Name,
                            Privacy = this.AlbumPrivacyVM.GetAsPrivacyInfo()
                        });
                    resultCallback(res.ResultCode == ResultCode.Succeeded);
                }), new long?(this._groupId));
        }
    }
}
