using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

using VKClient.Common.Library.Events;
namespace VKClient.Audio.Library
{
    public sealed class AddEditAudioViewModel : ViewModelBase
    {
        private bool _isFormEnabled = true;
        private string _artist = "";
        private string _title = "";
        private string _pageTitle = "";
        private Visibility _progressVisibility = Visibility.Collapsed;
        private StorageFile _storageFile;
        private Cancellation _cancellation;
        private readonly AudioObj _audioForEdit;
        private double _progress;

        public bool IsFormCompleted
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.Artist) && !string.IsNullOrWhiteSpace(this.Title) && (this.Artist.Length >= 2 && this.Title.Length >= 2);
            }
        }

        public bool IsFormEnabled
        {
            get
            {
                return this._isFormEnabled;
            }
            set
            {
                this._isFormEnabled = value;
                this.ProgressVisibility = value ? Visibility.Collapsed : Visibility.Visible;
                this.NotifyPropertyChanged<bool>(() => this.IsFormEnabled);
            }
        }

        public string Artist
        {
            get
            {
                return this._artist;
            }
            set
            {
                this._artist = value;
                this.NotifyPropertyChanged<string>(() => this.Artist);
                this.NotifyPropertyChanged<bool>(() => this.IsFormCompleted);
            }
        }

        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
                this.NotifyPropertyChanged<string>(() => this.Title);
                this.NotifyPropertyChanged<bool>(() => this.IsFormCompleted);
            }
        }

        public string PageTitle
        {
            get
            {
                return this._pageTitle;
            }
            set
            {
                this._pageTitle = value.ToUpper();
                this.NotifyPropertyChanged<string>(() => this.PageTitle);
            }
        }

        public double Progress
        {
            get
            {
                return this._progress;
            }
            set
            {
                this._progress = value;
                this.NotifyPropertyChanged<double>(() => this.Progress);
            }
        }

        public Visibility ProgressVisibility
        {
            get
            {
                return this._progressVisibility;
            }
            set
            {
                this._progressVisibility = value;
                this.NotifyPropertyChanged<Visibility>(() => this.ProgressVisibility);
            }
        }

        public AddEditAudioViewModel(StorageFile audioForUpload, AudioObj audioForEdit)
        {
            this._storageFile = audioForUpload;
            this._audioForEdit = audioForEdit;
            if (this._audioForEdit == null)
            {
                this.PageTitle = CommonResources.AudioAdding;
            }
            else
            {
                this.PageTitle = CommonResources.AudioEditing;
                this.Artist = Extensions.ForUI(audioForEdit.artist);
                this.Title = Extensions.ForUI(audioForEdit.title);
            }
        }


        public async void SaveChanges()
        {
            this.SetInProgress(true, "");
            this.Progress = 0.0;
            this.IsFormEnabled = false;
            if (this._audioForEdit == null)
            {
                try
                {
                    Stream stream = (await this._storageFile.OpenAsync(0)).AsStreamForRead();
                    this._cancellation = new Cancellation();
                    AudioService.Instance.UploadAudio(stream, this.Artist, this.Title, delegate(BackendResult<AudioObj, ResultCode> result)
                    {
                        Execute.ExecuteOnUIThread(delegate
                        {
                            if (result.ResultCode == ResultCode.Succeeded)
                            {
                                EventAggregator.Current.Publish(new AudioTrackAddedRemoved
                                {
                                    Audio = result.ResultData,
                                    Added = true
                                });
                                Navigator.Current.GoBack();
                                return;
                            }
                            if (!this._cancellation.IsSet)
                            {
                                this.SetInProgress(false, "");
                                this.IsFormEnabled = true;
                                GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                            }
                        });
                    }, delegate(double progress)
                    {
                        if (progress > 0.0 && this.Progress == 0.0)
                        {
                            base.SetInProgress(false, "");
                            this.ProgressVisibility = 0;
                        }
                        this.Progress = progress;
                    }, this._cancellation);
                    return;
                }
                catch
                {
                    this.SetInProgress(false, "");
                    this.IsFormEnabled = true;
                    GenericInfoUC.ShowBasedOnResult(ResultCode.UnknownError, "", null);
                    return;
                }
            }
            AudioService.Instance.EditAudio(this._audioForEdit.owner_id, this._audioForEdit.id, this.Artist, this.Title, delegate(BackendResult<long, ResultCode> result)
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    if (result.ResultCode == ResultCode.Succeeded)
                    {
                        EventAggregator.Current.Publish(new AudioTrackEdited
                        {
                            OwnerId = this._audioForEdit.owner_id,
                            Id = this._audioForEdit.id,
                            Artist = this.Artist,
                            Title = this.Title
                        });
                        Navigator.Current.GoBack();
                        return;
                    }
                    this.SetInProgress(false, "");
                    this.IsFormEnabled = true;
                    GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
                });
            });
        }


        public void CancelUploading()
        {
            Cancellation cancellation = this._cancellation;
            if ((cancellation != null ? (!cancellation.IsSet ? 1 : 0) : 0) == 0)
                return;
            this._cancellation.Set();
        }
    }
}
