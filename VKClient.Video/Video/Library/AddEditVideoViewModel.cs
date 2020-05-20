using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace VKClient.Video.Library
{
    public class AddEditVideoViewModel : ViewModelBase
    {
        private bool _isInEditMode;
        private long _videoId;
        private long _ownerId;
        private string _filePath;
        private string _name;
        private string _description;
        private bool _autoReplay;
        private bool _isSaving;
        private double _progress;
        private StorageFile _sf;
        private Cancellation _c;
        //private AccessType _accessType;
        //private AccessType _accessTypeComments;
        private string _localThumbPath;
        public static StorageFile PickedExternalFile;
        private EditPrivacyViewModel _viewVideoPricacyVM;
        private EditPrivacyViewModel _commentVideoPrivacyVM;

        public Cancellation C
        {
            get
            {
                return this._c;
            }
        }

        public string LocalThumbPath
        {
            get
            {
                return this._localThumbPath;
            }
        }

        public EditPrivacyViewModel ViewVideoPrivacyVM
        {
            get
            {
                return this._viewVideoPricacyVM;
            }
            set
            {
                this._viewVideoPricacyVM = value;
                this.NotifyPropertyChanged<EditPrivacyViewModel>((Expression<Func<EditPrivacyViewModel>>)(() => this.ViewVideoPrivacyVM));
            }
        }

        public EditPrivacyViewModel CommentVideoPrivacyVM
        {
            get
            {
                return this._commentVideoPrivacyVM;
            }
            set
            {
                this._commentVideoPrivacyVM = value;
                this.NotifyPropertyChanged<EditPrivacyViewModel>((Expression<Func<EditPrivacyViewModel>>)(() => this.CommentVideoPrivacyVM));
            }
        }

        public bool IsSaving
        {
            get
            {
                return this._isSaving;
            }
            private set
            {
                this._isSaving = value;
                this.SetInProgress(value, "");
                this.NotifyPropertyChanged<bool>((() => this.IsSaving));
                this.NotifyPropertyChanged<bool>((() => this.CanEdit));
                this.NotifyPropertyChanged<Visibility>((() => this.IsUploadingVisibility));
            }
        }

        public bool CanEdit
        {
            get
            {
                return !this.IsSaving;
            }
        }

        public Visibility IsUploadingVisibility
        {
            get
            {
                if (!this._isSaving || this._isInEditMode)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public double Progress
        {
            get
            {
                return this._progress;
            }
            private set
            {
                this._progress = value;
                this.NotifyPropertyChanged<double>((() => this.Progress));
            }
        }

        public Visibility IsUserVideo
        {
            get
            {
                if (this._ownerId <= 0L)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public string Title
        {
            get
            {
                if (!this._isInEditMode)
                    return CommonResources.AddEditVideo_Add;
                return CommonResources.AddEditVideo_Edit;
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
                this.NotifyPropertyChanged<string>((() => this.Name));
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
                this.NotifyPropertyChanged<string>((() => this.Description));
            }
        }

        public bool AutoReplay
        {
            get
            {
                return this._autoReplay;
            }
            set
            {
                this._autoReplay = value;
                this.NotifyPropertyChanged<bool>((() => this.AutoReplay));
            }
        }

        private AddEditVideoViewModel()
        {
        }

        public static AddEditVideoViewModel CreateForNewVideo(string filePath, long ownerId)
        {
            AddEditVideoViewModel editVideoViewModel = new AddEditVideoViewModel();
            editVideoViewModel._ownerId = ownerId;
            editVideoViewModel._filePath = filePath;
            EditPrivacyViewModel privacyViewModel1 = new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanView, new PrivacyInfo(), "", null);
            editVideoViewModel.ViewVideoPrivacyVM = privacyViewModel1;
            EditPrivacyViewModel privacyViewModel2 = new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanComment, new PrivacyInfo(), "", null);
            editVideoViewModel.CommentVideoPrivacyVM = privacyViewModel2;
            editVideoViewModel.PrepareVideo();
            return editVideoViewModel;
        }

        public static AddEditVideoViewModel CreateForEditVideo(long ownerId, long videoId, VKClient.Common.Backend.DataObjects.Video video = null)
        {
            AddEditVideoViewModel vm = new AddEditVideoViewModel();
            vm._ownerId = ownerId;
            vm._videoId = videoId;
            vm._isInEditMode = true;
            if (video != null)
            {
                vm.InitializeWithVideo(video);
            }
            else
            {
                VideoService.Instance.GetVideoById(ownerId, videoId, "", delegate(BackendResult<List<VKClient.Common.Backend.DataObjects.Video>, ResultCode> res)
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        VKClient.Common.Backend.DataObjects.Video vid = Enumerable.First<VKClient.Common.Backend.DataObjects.Video>(res.ResultData);
                        Execute.ExecuteOnUIThread(delegate
                        {
                            vm.InitializeWithVideo(vid);
                        });
                    }
                });
            }
            return vm;
        }

        private void InitializeWithVideo(VKClient.Common.Backend.DataObjects.Video video)
        {
            this.Name = video.title;
            this.Description = video.description;
            this.ViewVideoPrivacyVM = new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanView, video.PrivacyViewInfo, "", null);
            this.CommentVideoPrivacyVM = new EditPrivacyViewModel(CommonResources.AddEditVideo_WhoCanComment, video.PrivacyCommentInfo, "", null);
            this._localThumbPath = video.photo_320;
        }

        private async void PrepareVideo()
        {
            try
            {
                if (this._filePath != "")
                {
                    AddEditVideoViewModel addEditVideoViewModel = this;
                    StorageFile arg_5D_0 = addEditVideoViewModel._sf;
                    StorageFile sf = await StorageFile.GetFileFromPathAsync(this._filePath);
                    addEditVideoViewModel._sf = sf;
                    addEditVideoViewModel = null;
                }
                else
                {
                    this._sf = AddEditVideoViewModel.PickedExternalFile;
                }
                await this._sf.Properties.GetVideoPropertiesAsync();
                StorageItemThumbnail windowsRuntimeStream = await this._sf.GetThumbnailAsync(ThumbnailMode.VideosView);
                this._localThumbPath = "/" + Guid.NewGuid().ToString();
                ImageCache.Current.TrySetImageForUri(this._localThumbPath, windowsRuntimeStream.AsStream());
                this.NotifyPropertyChanged<string>(() => this.LocalThumbPath);
            }
            catch (Exception var_7_261)
            {
                Logger.Instance.Error("Failed to prepare video data", var_7_261);
            }
        }

        public async void Save(Action<bool> resultCallback)
        {
            if (!this._isSaving)
            {
                this.IsSaving = true;
                if (!this._isInEditMode)
                {
                    try
                    {
                        if (this._sf == null)
                        {
                            if (this._filePath != "")
                            {
                                AddEditVideoViewModel addEditVideoViewModel = this;
                                StorageFile arg_B8_0 = addEditVideoViewModel._sf;
                                StorageFile sf = await StorageFile.GetFileFromPathAsync(this._filePath);
                                addEditVideoViewModel._sf = sf;
                                addEditVideoViewModel = null;
                            }
                            else
                            {
                                this._sf = AddEditVideoViewModel.PickedExternalFile;
                                AddEditVideoViewModel.PickedExternalFile = null;
                            }
                        }
                        Stream stream = (await this._sf.OpenAsync(0)).AsStreamForRead();
                        this._c = new Cancellation();
                        VideoService.Instance.UploadVideo(stream, false, 0, (this._ownerId < 0L) ? (-this._ownerId) : 0, this.Name, this.Description, delegate(BackendResult<SaveVideoResponse, ResultCode> res)
                        {
                            this.IsSaving = false;
                            if (res.ResultCode == ResultCode.Succeeded)
                            {
                                EventAggregator.Current.Publish(new VideoAddedDeleted
                                {
                                    IsAdded = true,
                                    VideoId = res.ResultData.video_id,
                                    OwnerId = res.ResultData.owner_id
                                });
                                resultCallback.Invoke(true);
                                return;
                            }
                            this.Progress = 0.0;
                            resultCallback.Invoke(false);
                        }, delegate(double progress)
                        {
                            this.Progress = progress;
                        }, this._c, this.ViewVideoPrivacyVM.GetAsPrivacyInfo(), this.CommentVideoPrivacyVM.GetAsPrivacyInfo());
                        goto IL_309;
                    }
                    catch (Exception)
                    {
                        this.IsSaving = false;
                        resultCallback.Invoke(false);
                        goto IL_309;
                    }
                }
                VideoService.Instance.EditVideo(this._videoId, (this._ownerId < 0L) ? (-this._ownerId) : 0, this.Name, this.Description, this.ViewVideoPrivacyVM.GetAsPrivacyInfo(), this.CommentVideoPrivacyVM.GetAsPrivacyInfo(), delegate(BackendResult<ResponseWithId, ResultCode> res)
                {
                    this.IsSaving = false;
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        this.FireEditedEvent();
                        resultCallback.Invoke(true);
                        return;
                    }
                    resultCallback.Invoke(false);
                });
            IL_309: ;
            }
        }


        private void FireEditedEvent()
        {
            VKClient.Common.Backend.DataObjects.Video basedOnCurrentState = this.CreateVideoBasedOnCurrentState();
            EventAggregator.Current.Publish(new VideoEdited()
            {
                Video = basedOnCurrentState
            });
        }

        private VKClient.Common.Backend.DataObjects.Video CreateVideoBasedOnCurrentState()
        {
            return new VKClient.Common.Backend.DataObjects.Video()
            {
                id = this._videoId,
                owner_id = this._ownerId,
                title = this.Name,
                description = this.Description,
                privacy_view = this.ViewVideoPrivacyVM.GetAsPrivacyInfo().ToStringList(),
                privacy_comment = this.CommentVideoPrivacyVM.GetAsPrivacyInfo().ToStringList()
            };
        }

        public void Cancel()
        {
            if (this._c == null)
                return;
            this._c.Set();
        }
    }
}
