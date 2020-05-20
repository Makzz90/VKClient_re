using System;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.UC;

namespace VKClient.Groups.Library
{
  public sealed class CommunityCreationViewModel : ViewModelBase
  {
    private bool _isFormEnabled = true;
    private string _name = "";
    private bool? _areTermsAccepted = new bool?(false);
    private readonly NavigationService _navigationService;
    private bool? _isGroupSelected;
    private bool? _isPublicPageSelected;
    private bool? _isEventSelected;
    private bool? _isPlaceSelected;
    private bool? _isCompanySelected;
    private bool? _isPersonSelected;
    private bool? _isProductionSelected;

    public bool IsFormCompleted
    {
      get
      {
        if (this.Name.Length >= 2 && !string.IsNullOrWhiteSpace(this.Name))
        {
          bool? nullable = this.IsGroupSelected;
          bool flag1 = true;
          if ((nullable.GetValueOrDefault() == flag1 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
          {
            nullable = this.IsEventSelected;
            bool flag2 = true;
            if ((nullable.GetValueOrDefault() == flag2 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
            {
              nullable = this.IsPublicPageSelected;
              bool flag3 = true;
              if ((nullable.GetValueOrDefault() == flag3 ? (nullable.HasValue ? 1 : 0) : 0) != 0)
              {
                nullable = this.AreTermsAccepted;
                bool flag4 = true;
                if ((nullable.GetValueOrDefault() == flag4 ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                {
                  nullable = this.IsPlaceSelected;
                  bool flag5 = true;
                  if ((nullable.GetValueOrDefault() == flag5 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                  {
                    nullable = this.IsCompanySelected;
                    bool flag6 = true;
                    if ((nullable.GetValueOrDefault() == flag6 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                    {
                      nullable = this.IsPersonSelected;
                      bool flag7 = true;
                      if ((nullable.GetValueOrDefault() == flag7 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                      {
                        nullable = this.IsProductionSelected;
                        bool flag8 = true;
                        if ((nullable.GetValueOrDefault() == flag8 ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                          goto label_11;
                      }
                    }
                  }
                  return true;
                }
                goto label_11;
              }
              else
                goto label_11;
            }
          }
          return true;
        }
label_11:
        return false;
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
        this.NotifyPropertyChanged<bool>((() => this.IsFormEnabled));
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
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public bool? IsGroupSelected
    {
      get
      {
        return this._isGroupSelected;
      }
      set
      {
        this._isGroupSelected = value;
        this.NotifyPropertyChanged<Visibility>((() => this.PublicPageTypeFormPartVisibility));
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public bool? IsPublicPageSelected
    {
      get
      {
        return this._isPublicPageSelected;
      }
      set
      {
        this._isPublicPageSelected = value;
        this.NotifyPropertyChanged<Visibility>((() => this.PublicPageTypeFormPartVisibility));
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public bool? IsEventSelected
    {
      get
      {
        return this._isEventSelected;
      }
      set
      {
        this._isEventSelected = value;
        this.NotifyPropertyChanged<Visibility>((() => this.PublicPageTypeFormPartVisibility));
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public Visibility PublicPageTypeFormPartVisibility
    {
      get
      {
        bool? publicPageSelected = this.IsPublicPageSelected;
        bool flag = true;
        if ((publicPageSelected.GetValueOrDefault() == flag ? (publicPageSelected.HasValue ? 1 : 0) : 0) == 0)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public bool? IsPlaceSelected
    {
      get
      {
        return this._isPlaceSelected;
      }
      set
      {
        this._isPlaceSelected = value;
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public bool? IsCompanySelected
    {
      get
      {
        return this._isCompanySelected;
      }
      set
      {
        this._isCompanySelected = value;
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public bool? IsPersonSelected
    {
      get
      {
        return this._isPersonSelected;
      }
      set
      {
        this._isPersonSelected = value;
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public bool? IsProductionSelected
    {
      get
      {
        return this._isProductionSelected;
      }
      set
      {
        this._isProductionSelected = value;
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public bool? AreTermsAccepted
    {
      get
      {
        return this._areTermsAccepted;
      }
      set
      {
        this._areTermsAccepted = value;
        this.NotifyPropertyChanged<bool>((() => this.IsFormCompleted));
      }
    }

    public CommunityCreationViewModel(NavigationService navigationService)
    {
      this._navigationService = navigationService;
    }

    public void CreateCommunity()
    {
        string type = "group";
        int subtype = 0;
        bool? publicPageSelected = this.IsPublicPageSelected;
        bool flag1 = true;
        if ((publicPageSelected.GetValueOrDefault() == flag1 ? (publicPageSelected.HasValue ? 1 : 0) : 0) != 0)
        {
            type = "public";
            bool? isPlaceSelected = this.IsPlaceSelected;
            bool flag2 = true;
            if ((isPlaceSelected.GetValueOrDefault() == flag2 ? (isPlaceSelected.HasValue ? 1 : 0) : 0) != 0)
            {
                subtype = 1;
            }
            else
            {
                bool? isCompanySelected = this.IsCompanySelected;
                bool flag3 = true;
                if ((isCompanySelected.GetValueOrDefault() == flag3 ? (isCompanySelected.HasValue ? 1 : 0) : 0) != 0)
                {
                    subtype = 2;
                }
                else
                {
                    bool? isPersonSelected = this.IsPersonSelected;
                    bool flag4 = true;
                    subtype = (isPersonSelected.GetValueOrDefault() == flag4 ? (isPersonSelected.HasValue ? 1 : 0) : 0) == 0 ? 4 : 3;
                }
            }
        }
        else
        {
            bool? isEventSelected = this.IsEventSelected;
            bool flag2 = true;
            if ((isEventSelected.GetValueOrDefault() == flag2 ? (isEventSelected.HasValue ? 1 : 0) : 0) != 0)
                type = "event";
        }
        this.SetInProgress(true, "");
        this.IsFormEnabled = false;
        GroupsService.Current.CreateCommunity(this.Name, type, subtype, (Action<BackendResult<Group, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
        {
            if (result.ResultCode == ResultCode.Succeeded)
            {
                EventAggregator.Current.Publish((object)new GroupMembershipStatusUpdated(result.ResultData.id, true));
                Navigator.Current.NavigateToGroup(result.ResultData.id, "", false);
                this._navigationService.RemoveBackEntry();
            }
            else
            {
                this.SetInProgress(false, "");
                this.IsFormEnabled = true;
                GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            }
        }))));
    }
  }
}
