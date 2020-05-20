using System.Windows;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Groups.Management.Library
{
    public sealed class ServiceSwitchViewModel : ViewModelBase
    {
        private readonly CommunityService _service;
        private readonly CommunityServiceState _currentState;

        public string PageTitle
        {
            get
            {
                switch (this._service)
                {
                    case CommunityService.Wall:
                        return CommonResources.Wall.ToUpper();
                    case CommunityService.Photos:
                        return CommonResources.Profile_Photos.ToUpper();
                    case CommunityService.Videos:
                        return CommonResources.Profile_Videos.ToUpper();
                    case CommunityService.Audios:
                        return CommonResources.Profile_Audios.ToUpper();
                    case CommunityService.Documents:
                        return CommonResources.Profile_Docs.ToUpper();
                    case CommunityService.Discussions:
                        return CommonResources.Profile_Discussions.ToUpper();
                    default:
                        return "";
                }
            }
        }

        public string DisabledTitle
        {
            get
            {
                if (this._service != CommunityService.Wall)
                    return CommonResources.Disabled_Form2;
                return CommonResources.Disabled_Form1;
            }
        }

        public SolidColorBrush DisabledForeground
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources[this._currentState == CommunityServiceState.Disabled ? "PhoneBlue300Brush" : "PhoneContrastTitleBrush"];
            }
        }

        public string OpenedTitle
        {
            get
            {
                if (this._service != CommunityService.Wall)
                    return CommonResources.Opened_Form2;
                return CommonResources.Opened_Form1;
            }
        }

        public SolidColorBrush OpenedForeground
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources[this._currentState == CommunityServiceState.Opened ? "PhoneBlue300Brush" : "PhoneContrastTitleBrush"];
            }
        }

        public string OpenedDescription
        {
            get
            {
                switch (this._service)
                {
                    case CommunityService.Wall:
                        return CommonResources.OpenedWallDescription;
                    case CommunityService.Photos:
                        return CommonResources.OpenedPhotosDescription;
                    case CommunityService.Videos:
                        return CommonResources.OpenedVideosDescription;
                    case CommunityService.Audios:
                        return CommonResources.OpenedAudiosDescription;
                    case CommunityService.Documents:
                        return CommonResources.OpenedDocumentsDescription;
                    case CommunityService.Discussions:
                        return CommonResources.OpenedDiscussionsDescription;
                    default:
                        return "";
                }
            }
        }

        public string LimitedTitle
        {
            get
            {
                if (this._service != CommunityService.Wall)
                    return CommonResources.Limited_Form2;
                return CommonResources.Limited_Form1;
            }
        }

        public SolidColorBrush LimitedForeground
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources[this._currentState == CommunityServiceState.Limited ? "PhoneBlue300Brush" : "PhoneContrastTitleBrush"];
            }
        }

        public string LimitedDescription
        {
            get
            {
                switch (this._service)
                {
                    case CommunityService.Wall:
                        return CommonResources.LimitedWallDescription;
                    case CommunityService.Photos:
                        return CommonResources.LimitedPhotosDescription;
                    case CommunityService.Videos:
                        return CommonResources.LimitedVideosDescription;
                    case CommunityService.Audios:
                        return CommonResources.LimitedAudiosDescription;
                    case CommunityService.Documents:
                        return CommonResources.LimitedDocumentsDescription;
                    case CommunityService.Discussions:
                        return CommonResources.LimitedDiscussionsDescription;
                    default:
                        return "";
                }
            }
        }

        public Visibility ClosedVisibility
        {
            get
            {
                return (this._service == CommunityService.Wall).ToVisiblity();
            }
        }

        public SolidColorBrush ClosedForeground
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources[this._currentState == CommunityServiceState.Closed ? "PhoneBlue300Brush" : "PhoneContrastTitleBrush"];
            }
        }

        public ServiceSwitchViewModel(CommunityService service, CommunityServiceState currentState)
        {
            this._service = service;
            this._currentState = currentState;
        }

        public void SaveResult(CommunityServiceState newState)
        {
            ParametersRepository.SetParameterForId("CommunityManagementService", this._service);
            ParametersRepository.SetParameterForId("CommunityManagementServiceNewState", newState);
            Navigator.Current.GoBack();
        }
    }
}
