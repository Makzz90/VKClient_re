using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Groups.Management.Information.Library
{
    public sealed class EventOrganizerViewModel : ViewModelBase
    {
        private string _phone = "";
        private string _email = "";
        private Visibility _visibility;
        private List<CustomListPickerItem> _availableOrganizers;
        private CustomListPickerItem _organizer;
        private Visibility _contactsFieldsVisibility;

        public InformationViewModel ParentViewModel { get; set; }

        public Visibility Visibility
        {
            get
            {
                return this._visibility;
            }
            set
            {
                this._visibility = value;
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.Visibility));
            }
        }

        public List<CustomListPickerItem> AvailableOrganizers
        {
            get
            {
                return this._availableOrganizers;
            }
            set
            {
                this._availableOrganizers = value;
                this.NotifyPropertyChanged<List<CustomListPickerItem>>((Expression<Func<List<CustomListPickerItem>>>)(() => this.AvailableOrganizers));
            }
        }

        public CustomListPickerItem Organizer
        {
            get
            {
                return this._organizer;
            }
            set
            {
                this._organizer = value;
                this.NotifyPropertyChanged<CustomListPickerItem>((Expression<Func<CustomListPickerItem>>)(() => this.Organizer));
            }
        }

        public Visibility ContactsFieldsVisibility
        {
            get
            {
                return this._contactsFieldsVisibility;
            }
            set
            {
                this._contactsFieldsVisibility = value;
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.ContactsFieldsVisibility));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.SetContactsButtonVisibility));
            }
        }

        public Visibility SetContactsButtonVisibility
        {
            get
            {
                return (this.ContactsFieldsVisibility == Visibility.Collapsed).ToVisiblity();
            }
        }

        public string Phone
        {
            get
            {
                return this._phone;
            }
            set
            {
                this._phone = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.Phone));
            }
        }

        public string Email
        {
            get
            {
                return this._email;
            }
            set
            {
                this._email = value;
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.Email));
            }
        }

        public EventOrganizerViewModel(InformationViewModel parentViewModel)
        {
            this.ParentViewModel = parentViewModel;
        }

        public void Read(CommunitySettings information)
        {
            if (information.Type != GroupType.Event)
            {
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                List<CustomListPickerItem> list = information.event_available_organizers.Where<Group>((Func<Group, bool>)(o => o.type != "event")).Select<Group, CustomListPickerItem>((Func<Group, CustomListPickerItem>)(o => new CustomListPickerItem()
                {
                    Id = -o.id,
                    Name = o.name
                })).ToList<CustomListPickerItem>();
                list.Insert(0, new CustomListPickerItem()
                {
                    Id = information.event_creator.id,
                    Name = information.event_creator.Name
                });
                this.AvailableOrganizers = list;
                this.Organizer = this.AvailableOrganizers.First<CustomListPickerItem>((Func<CustomListPickerItem, bool>)(o =>
                {
                    if (information.event_group_id != 0L)
                        return information.event_group_id == -o.Id;
                    return true;
                }));
                this.Phone = information.phone;
                this.Email = information.email;
                this.ContactsFieldsVisibility = (!string.IsNullOrWhiteSpace(information.phone) || !string.IsNullOrWhiteSpace(information.email)).ToVisiblity();
            }
        }
    }
}
