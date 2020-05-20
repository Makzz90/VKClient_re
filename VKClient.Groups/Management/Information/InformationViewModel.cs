using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Management.Information.Library;

namespace VKClient.Groups.Management.Information
{
    public sealed class InformationViewModel : ViewModelStatefulBase
    {
        private bool _isFormEnabled = true;
        public readonly long CommunityId;
        private CommunitySettings _information;
        public RoutedEventHandler OnTextBoxGotFocus;
        public RoutedEventHandler OnTextBoxLostFocus;

        public CommonFieldsViewModel CommonFieldsViewModel { get; private set; }

        public FoundationDateViewModel FoundationDateViewModel { get; private set; }

        public AgeLimitsViewModel AgeLimitsViewModel { get; private set; }

        public CommunityTypeSelectionViewModel CommunityTypeSelectionViewModel { get; private set; }

        public EventOrganizerViewModel EventOrganizerViewModel { get; private set; }

        public EventDatesViewModel EventDatesViewModel { get; private set; }

        public CommunityPlacementViewModel CommunityPlacementViewModel { get; private set; }

        public bool IsFormCompleted
        {
            get
            {
                return this.CommonFieldsViewModel.Name.Length >= 2 && !string.IsNullOrWhiteSpace(this.CommonFieldsViewModel.Name) && (this.CommonFieldsViewModel.Domain == "" || this.CommonFieldsViewModel.Domain.Length >= 5 && !string.IsNullOrWhiteSpace(this.CommonFieldsViewModel.Domain));
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
                this.NotifyPropertyChanged<bool>((Expression<Func<bool>>)(() => this.IsFormEnabled));
            }
        }

        public InformationViewModel(long communityId)
        {
            this.CommunityId = communityId;
            this.CommonFieldsViewModel = new CommonFieldsViewModel(this);
            this.FoundationDateViewModel = new FoundationDateViewModel(this);
            this.AgeLimitsViewModel = new AgeLimitsViewModel(this);
            this.CommunityTypeSelectionViewModel = new CommunityTypeSelectionViewModel(this);
            this.EventOrganizerViewModel = new EventOrganizerViewModel(this);
            this.EventDatesViewModel = new EventDatesViewModel(this);
            this.CommunityPlacementViewModel = new CommunityPlacementViewModel(this);
            this.CommonFieldsViewModel.PropertyChanged += (PropertyChangedEventHandler)((o, e) => this.NotifyPropertyChanged(".ctor"));
            this.FoundationDateViewModel.PropertyChanged += (PropertyChangedEventHandler)((o, e) => this.NotifyPropertyChanged(".ctor"));
            this.AgeLimitsViewModel.PropertyChanged += (PropertyChangedEventHandler)((o, e) => this.NotifyPropertyChanged(".ctor"));
            this.CommunityTypeSelectionViewModel.PropertyChanged += (PropertyChangedEventHandler)((o, e) => this.NotifyPropertyChanged(".ctor"));
            this.EventOrganizerViewModel.PropertyChanged += (PropertyChangedEventHandler)((o, e) => this.NotifyPropertyChanged(".ctor"));
            this.EventDatesViewModel.PropertyChanged += (PropertyChangedEventHandler)((o, e) => this.NotifyPropertyChanged(".ctor"));
            this.CommunityPlacementViewModel.PropertyChanged += (PropertyChangedEventHandler)((o, e) => this.NotifyPropertyChanged(".ctor"));
        }

        public override void Load(Action<ResultCode> callback)
        {
            GroupsService.Current.GetCommunitySettings(this.CommunityId, (Action<BackendResult<CommunitySettings, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                ResultCode resultCode = result.ResultCode;
                if (resultCode == ResultCode.Succeeded)
                {
                    this._information = result.ResultData;
                    this.CommonFieldsViewModel.Read(this._information);
                    this.FoundationDateViewModel.Read(this._information);
                    this.AgeLimitsViewModel.Read(this._information);
                    this.CommunityTypeSelectionViewModel.Read(this._information);
                    this.EventOrganizerViewModel.Read(this._information);
                    this.EventDatesViewModel.Read(this._information);
                    this.CommunityPlacementViewModel.Read(this._information);
                }
                Action<ResultCode> action = callback;
                if (action == null)
                    return;
                int num = (int)resultCode;
                action((ResultCode)num);
            }))));
        }

        public void SaveChanges()
        {
            this.SetInProgress(true, "");
            this.IsFormEnabled = false;
            string str1 = this.CommonFieldsViewModel.Domain != this.CommonFieldsViewModel.CurrentDomain ? this.CommonFieldsViewModel.Domain : (string)null;
            if (str1 == "")
                str1 = "club" + (object)this.CommunityId;
            int num1 = 0;
            bool? nullable1 = this.CommunityTypeSelectionViewModel.IsClosedSelected;
            bool flag1 = true;
            if ((nullable1.GetValueOrDefault() == flag1 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
                num1 = 1;
            nullable1 = this.CommunityTypeSelectionViewModel.IsPrivateSelected;
            bool flag2 = true;
            if ((nullable1.GetValueOrDefault() == flag2 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
                num1 = 2;
            int num2 = 1;
            nullable1 = this.AgeLimitsViewModel.From16Only;
            bool flag3 = true;
            if ((nullable1.GetValueOrDefault() == flag3 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
                num2 = 2;
            nullable1 = this.AgeLimitsViewModel.From18Only;
            bool flag4 = true;
            if ((nullable1.GetValueOrDefault() == flag4 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
                num2 = 3;
            string str2 = (string)null;
            if (this._information.Type == GroupType.PublicPage)
            {
                string str3 = this.FoundationDateViewModel.Day.Id.ToString();
                string str4 = this.FoundationDateViewModel.Month.Id.ToString();
                string str5 = this.FoundationDateViewModel.Year.Id.ToString();
                if (str3.Length == 1)
                    str3 = "0" + str3;
                if (str4.Length == 1)
                    str4 = "0" + str4;
                if (str5.Length == 1)
                    str5 = "0000";
                str2 = string.Format("{0}.{1}.{2}", (object)str3, (object)str4, (object)str5);
            }
            GroupPrivacy accessLevel = GroupPrivacy.Public;
            if (num1 == 1)
                accessLevel = GroupPrivacy.Closed;
            if (num1 == 2)
                accessLevel = GroupPrivacy.Private;
            string categoryName = (string)null;
            string subcategoryName = (string)null;
            if (this.CommonFieldsViewModel.Category != null && this.CommonFieldsViewModel.Category.Id != 0L)
                categoryName = this.CommonFieldsViewModel.Category.Name;
            if (this.CommonFieldsViewModel.Subcategory != null && this.CommonFieldsViewModel.Subcategory.Id != 0L)
                subcategoryName = this.CommonFieldsViewModel.Subcategory.Name;
            long? nullable2 = new long?();
            DateTime eventStartDate = new DateTime();
            DateTime dateTime = new DateTime();
            if (this._information.Type == GroupType.Event)
            {
                nullable2 = new long?(this.EventOrganizerViewModel.Organizer.Id < 0L ? -this.EventOrganizerViewModel.Organizer.Id : 0L);
                DateTime startDate = this.EventDatesViewModel.StartDate;
                DateTime startTime = this.EventDatesViewModel.StartTime;
                eventStartDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startTime.Hour, startTime.Minute, 0, DateTimeKind.Local);
                DateTime finishDate = this.EventDatesViewModel.FinishDate;
                DateTime finishTime = this.EventDatesViewModel.FinishTime;
                dateTime = new DateTime(finishDate.Year, finishDate.Month, finishDate.Day, finishTime.Hour, finishTime.Minute, 0, DateTimeKind.Local);
            }
            GroupsService current = GroupsService.Current;
            long communityId = this.CommunityId;
            string name = this.CommonFieldsViewModel.Name;
            string description = this.CommonFieldsViewModel.Description;
            long id = this.CommonFieldsViewModel.Category.Id;
            CustomListPickerItem subcategory1 = this.CommonFieldsViewModel.Subcategory;
            long subcategory2 = subcategory1 != null ? subcategory1.Id : 0L;
            string site = this.CommonFieldsViewModel.Site;
            int accessLevel1 = num1;
            string domain = str1;
            int ageLimits = num2;
            string foundationDate = str2;
            long? eventOrganizerId = nullable2;
            string phone = this.EventOrganizerViewModel.Phone;
            string email = this.EventOrganizerViewModel.Email;
            int unixTimestamp1 = Extensions.DateTimeToUnixTimestamp(eventStartDate.ToUniversalTime(), false);
            int unixTimestamp2 = Extensions.DateTimeToUnixTimestamp(dateTime.ToUniversalTime(), false);
            Action<BackendResult<int, ResultCode>> callback = (Action<BackendResult<int, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    EventAggregator.Current.Publish((object)new CommunityInformationChanged()
                    {
                        Id = this.CommunityId,
                        Name = this.CommonFieldsViewModel.Name,
                        Privacy = accessLevel,
                        CategoryName = categoryName,
                        SubcategoryName = subcategoryName,
                        EventStartDate = eventStartDate
                    });
                    Navigator.Current.GoBack();
                }
                else
                {
                    this.SetInProgress(false, "");
                    this.IsFormEnabled = true;
                    VKRequestsDispatcher.Error error = result.Error;
                    switch (error != null ? error.error_text : (string)null)
                    {
                        case null:
                            GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", result.Error);
                            break;
                        default:
                            result.Error.error_text += ".";
                            goto case null;
                    }
                }
            })));
            current.SetCommunityInformation(communityId, name, description, id, subcategory2, site, accessLevel1, domain, ageLimits, foundationDate, eventOrganizerId, phone, email, unixTimestamp1, unixTimestamp2, callback);
        }
    }
}
