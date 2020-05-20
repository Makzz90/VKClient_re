using System;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
    public class ProfileMediaViewModelFacade : ViewModelBase
    {
        private IProfileData _profileData;
        private IMediaHorizontalItemsViewModel _mediaHorizontalItemsViewModel;
        private IMediaVerticalItemsViewModel _mediaVerticalItemsViewModel;
        private readonly ProfileMediaSectionsViewModel _mediaSectionsViewModel;
        private bool _canDisplayHorizontalItems;
        private bool _canDisplayVerticalItems;

        public IMediaHorizontalItemsViewModel MediaHorizontalItemsViewModel
        {
            get
            {
                return this._mediaHorizontalItemsViewModel;
            }
        }

        public IMediaVerticalItemsViewModel MediaVerticalItemsViewModel
        {
            get
            {
                return this._mediaVerticalItemsViewModel;
            }
        }

        public ProfileMediaSectionsViewModel MediaSectionsViewModel
        {
            get
            {
                return this._mediaSectionsViewModel;
            }
        }

        public Visibility MediaHorizontalItemsVisibility
        {
            get
            {
                return !this._canDisplayHorizontalItems || !this._mediaHorizontalItemsViewModel.CanDisplay ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility MediaVerticalItemsVisibility
        {
            get
            {
                return !this._canDisplayVerticalItems || !this._mediaVerticalItemsViewModel.CanDisplay ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility MediaSectionsVisibility
        {
            get
            {
                return !this._mediaSectionsViewModel.CanDisplay ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool CanDisplay
        {
            get
            {
                if ((this._mediaHorizontalItemsViewModel == null || !this._mediaHorizontalItemsViewModel.CanDisplay) && (this._mediaVerticalItemsViewModel == null || !this._mediaVerticalItemsViewModel.CanDisplay))
                    return this._mediaSectionsViewModel.CanDisplay;
                return true;
            }
        }

        public ProfileMediaViewModelFacade()
        {
            this._mediaHorizontalItemsViewModel = (IMediaHorizontalItemsViewModel)new ProfilePhotosViewModel();
            this._mediaVerticalItemsViewModel = (IMediaVerticalItemsViewModel)new ProfileAudiosViewModel();
            this._mediaSectionsViewModel = new ProfileMediaSectionsViewModel();
        }

        public void Init(IProfileData profileData)
        {
            if (this._profileData == null || profileData.MainSectionType != this._profileData.MainSectionType)
            {
                this._canDisplayHorizontalItems = false;
                this._canDisplayVerticalItems = false;
                if (profileData.MainSectionType == ProfileMainSectionType.None)
                {
                    this._canDisplayHorizontalItems = false;
                    this._canDisplayVerticalItems = false;
                }
                else
                {
                    switch (profileData.MainSectionType)
                    {
                        case ProfileMainSectionType.Photos:
                            this._mediaHorizontalItemsViewModel = (IMediaHorizontalItemsViewModel)new ProfilePhotosViewModel();
                            this._canDisplayHorizontalItems = true;
                            break;
                        case ProfileMainSectionType.Discussions:
                            this._mediaVerticalItemsViewModel = (IMediaVerticalItemsViewModel)new ProfileDiscussionsViewModel();
                            break;
                        case ProfileMainSectionType.Audios:
                            this._mediaVerticalItemsViewModel = (IMediaVerticalItemsViewModel)new ProfileAudiosViewModel();
                            break;
                        case ProfileMainSectionType.Videos:
                            this._mediaHorizontalItemsViewModel = (IMediaHorizontalItemsViewModel)new ProfileVideosViewModel();
                            this._canDisplayHorizontalItems = true;
                            break;
                        case ProfileMainSectionType.Market:
                            this._mediaHorizontalItemsViewModel = (IMediaHorizontalItemsViewModel)new ProfileMarketViewModel();
                            this._canDisplayHorizontalItems = true;
                            break;
                    }
                    this._canDisplayVerticalItems = !this._canDisplayHorizontalItems;
                }
            }
            this._profileData = profileData;
            if (this._canDisplayHorizontalItems)
            {
                this.NotifyPropertyChanged<IMediaHorizontalItemsViewModel>((System.Linq.Expressions.Expression<Func<IMediaHorizontalItemsViewModel>>)(() => this.MediaHorizontalItemsViewModel));
                this._mediaHorizontalItemsViewModel.Init(this._profileData);
            }
            else
            {
                this.NotifyPropertyChanged<IMediaVerticalItemsViewModel>((System.Linq.Expressions.Expression<Func<IMediaVerticalItemsViewModel>>)(() => this.MediaVerticalItemsViewModel));
                this._mediaVerticalItemsViewModel.Init(this._profileData);
            }
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.MediaHorizontalItemsVisibility));
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.MediaVerticalItemsVisibility));
            this._mediaSectionsViewModel.Init(this._profileData, this._profileData.MainSectionType);
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.MediaSectionsVisibility));
            this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.CanDisplay));
        }
    }
}
