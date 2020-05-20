using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Groups.Management.Library
{
  public sealed class ManagerEditingViewModel : ViewModelBase
  {
    private bool _isFormEnabled = true;
    private readonly bool _isEditing;
    private readonly bool _fromPicker;
    private readonly GroupType _communityType;
    private readonly long _communityId;
    private readonly User _manager;
    private readonly NavigationService _navigationService;
    private bool? _isModeratorSelected;
    private bool? _isEditorSelected;
    private bool? _isAdministratorSelected;
    private bool? _isContact;
    private string _position;
    private string _email;
    private string _phone;

    public bool IsFormEnabled
    {
      get
      {
        return this._isFormEnabled;
      }
      set
      {
        this._isFormEnabled = value;
        this.NotifyPropertyChanged<bool>((() => this.IsFormEnabled));
      }
    }

    public string ManagerPhoto
    {
      get
      {
        return this._manager.photo_max;
      }
    }

    public string ManagerName
    {
      get
      {
        return this._manager.Name;
      }
    }

    public string Case
    {
      get
      {
        if (!this._isEditing)
          return string.Format(CommonResources.WillBeCommunityManager, this._manager.first_name_acc);
        return CommonResources.IsCommunityManager;
      }
    }

    public string PageTitle
    {
      get
      {
        if (!this._isEditing)
          return CommonResources.CommunityManager_Adding;
        return CommonResources.CommunityManager_Editing;
      }
    }

    public bool? IsModeratorSelected
    {
      get
      {
        return this._isModeratorSelected;
      }
      set
      {
        this._isModeratorSelected = value;
        this.NotifyPropertyChanged<bool?>((() => this.IsModeratorSelected));
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public bool? IsEditorSelected
    {
      get
      {
        return this._isEditorSelected;
      }
      set
      {
        this._isEditorSelected = value;
        this.NotifyPropertyChanged<bool?>((() => this.IsEditorSelected));
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public bool? IsAdministratorSelected
    {
      get
      {
        return this._isAdministratorSelected;
      }
      set
      {
        this._isAdministratorSelected = value;
        this.NotifyPropertyChanged<bool?>((() => this.IsAdministratorSelected));
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public Visibility RemoveButtonVisibility
    {
      get
      {
        return this._isEditing.ToVisiblity();
      }
    }

    public bool IsFormCompleted
    {
      get
      {
        if (this._isEditing)
          return true;
        bool? nullable = this.IsModeratorSelected;
        bool flag1 = true;
        if ((nullable.GetValueOrDefault() == flag1 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
        {
          nullable = this.IsEditorSelected;
          bool flag2 = true;
          if ((nullable.GetValueOrDefault() == flag2 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
          {
            nullable = this.IsAdministratorSelected;
            bool flag3 = true;
            if (nullable.GetValueOrDefault() != flag3)
              return false;
            return nullable.HasValue;
          }
        }
        return true;
      }
    }

    public bool? IsContact
    {
      get
      {
        return this._isContact;
      }
      set
      {
        this._isContact = value;
        this.NotifyPropertyChanged<bool?>((() => this.IsContact));
        this.NotifyPropertyChanged<Visibility>((() => this.ContactFieldsVisibility));
      }
    }

    public Visibility ContactFieldsVisibility
    {
      get
      {
        bool? isContact = this.IsContact;
        bool flag = true;
        return (isContact.GetValueOrDefault() == flag && isContact.HasValue).ToVisiblity();
      }
    }

    public string Position
    {
      get
      {
        return this._position;
      }
      set
      {
        this._position = value;
        this.NotifyPropertyChanged<string>((() => this.Position));
        this.NotifyPropertyChanged<double>((() => this.PositionPlaceholderOpacity));
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
        this.NotifyPropertyChanged<string>((() => this.Email));
        this.NotifyPropertyChanged<double>((() => this.EmailPlaceholderOpacity));
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
        this.NotifyPropertyChanged<string>((() => this.Phone));
        this.NotifyPropertyChanged<double>((() => this.PhonePlaceholderOpacity));
      }
    }

    public double PositionPlaceholderOpacity
    {
      get
      {
        return string.IsNullOrEmpty(this.Position) ? 1.0 : 0.0;
      }
    }

    public double EmailPlaceholderOpacity
    {
      get
      {
        return string.IsNullOrEmpty(this.Email) ? 1.0 : 0.0;
      }
    }

    public double PhonePlaceholderOpacity
    {
      get
      {
        return string.IsNullOrEmpty(this.Phone) ? 1.0 : 0.0;
      }
    }

    public ManagerEditingViewModel(long communityId, GroupType communityType, NavigationService navigationService, User manager, bool isContact, string position, string email, string phone, bool isEditing, bool fromPicker)
    {
      this._isEditing = isEditing;
      this._fromPicker = fromPicker;
      this._communityType = communityType;
      this._communityId = communityId;
      this._manager = manager;
      switch (manager.Role)
      {
        case CommunityManagementRole.Moderator:
          this.IsModeratorSelected = new bool?(true);
          break;
        case CommunityManagementRole.Editor:
          this.IsEditorSelected = new bool?(true);
          break;
        case CommunityManagementRole.Administrator:
          this.IsAdministratorSelected = new bool?(true);
          break;
      }
      this.IsContact = new bool?(isContact);
      this.Position = position ?? "";
      this.Email = email ?? "";
      this.Phone = phone ?? "";
      this._navigationService = navigationService;
    }

    public void SaveChanges(bool isRemoving, NavigationService navigationService)
    {
        CommunityManagementRole role = CommunityManagementRole.Unknown;
        if (!isRemoving)
        {
            bool? moderatorSelected = this.IsModeratorSelected;
            bool flag1 = true;
            if ((moderatorSelected.GetValueOrDefault() == flag1 ? (moderatorSelected.HasValue ? 1 : 0) : 0) != 0)
                role = CommunityManagementRole.Moderator;
            bool? isEditorSelected = this.IsEditorSelected;
            bool flag2 = true;
            if ((isEditorSelected.GetValueOrDefault() == flag2 ? (isEditorSelected.HasValue ? 1 : 0) : 0) != 0)
                role = CommunityManagementRole.Editor;
            bool? administratorSelected = this.IsAdministratorSelected;
            bool flag3 = true;
            if ((administratorSelected.GetValueOrDefault() == flag3 ? (administratorSelected.HasValue ? 1 : 0) : 0) != 0)
                role = CommunityManagementRole.Administrator;
        }
        EditingMode editingMode = this._isEditing ? EditingMode.Editing : EditingMode.Adding;
        if (isRemoving)
            editingMode = EditingMode.Removing;
        this.SetInProgress(true, "");
        this.IsFormEnabled = false;
        GroupsService current1 = GroupsService.Current;
        long communityId = this._communityId;
        long id = this._manager.id;
        int num1 = (int)role;
        bool? isContact1 = this.IsContact;
        bool flag4 = true;
        int num2 = isContact1.GetValueOrDefault() == flag4 ? (isContact1.HasValue ? 1 : 0) : 0;
        string position1 = this.Position;
        string email1 = this.Email;
        string phone1 = this.Phone;
        Action<BackendResult<int, ResultCode>> callback = (Action<BackendResult<int, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
        {
            if (result.ResultCode == ResultCode.Succeeded)
            {
                EventAggregator current2 = EventAggregator.Current;
                CommunityManagerChanged communityManagerChanged = new CommunityManagerChanged();
                communityManagerChanged.CommunityId = this._communityId;
                communityManagerChanged.ManagerId = this._manager.id;
                communityManagerChanged.EditingMode = editingMode;
                communityManagerChanged.Role = role;
                bool? isContact2 = this.IsContact;
                bool flag1 = true;
                int num3 = isContact2.GetValueOrDefault() == flag1 ? (isContact2.HasValue ? 1 : 0) : 0;
                communityManagerChanged.IsContact = num3 != 0;
                string position2 = this.Position;
                communityManagerChanged.Position = position2;
                string email2 = this.Email;
                communityManagerChanged.Email = email2;
                string phone2 = this.Phone;
                communityManagerChanged.Phone = phone2;
                User manager = this._manager;
                communityManagerChanged.User = manager;
                current2.Publish((object)communityManagerChanged);
                if (!this._isEditing && this._fromPicker)
                    navigationService.RemoveBackEntry();
                if (this._manager.id != VKClient.Common.Library.AppGlobalStateManager.Current.LoggedInUserId || role == CommunityManagementRole.Administrator)
                    Navigator.Current.GoBack();
                else if (role != CommunityManagementRole.Unknown)
                {
                    Navigator.Current.NavigateToCommunityManagement(this._communityId, this._communityType, false);
                    this._navigationService.RemoveBackEntry();
                    this._navigationService.RemoveBackEntry();
                    this._navigationService.RemoveBackEntry();
                }
                else
                {
                    this._navigationService.RemoveBackEntry();
                    this._navigationService.RemoveBackEntry();
                    Navigator.Current.GoBack();
                }
            }
            else
            {
                this.SetInProgress(false, "");
                this.IsFormEnabled = true;
                VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            }
        })));
        current1.EditManager(communityId, id, (CommunityManagementRole)num1, num2 != 0, position1, email1, phone1, callback);
    }
  }
}
