using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class SettingsEditProfileViewModel : ViewModelStatefulBase, IHandle<BaseDataChangedEvent>, IHandle
  {
    private static readonly List<BGType> _birthdayShowTypesList = new List<BGType>()
    {
      new BGType()
      {
        id = 1,
        name = CommonResources.Settings_EditProfile_ShowDayMonthAndYear
      },
      new BGType()
      {
        id = 2,
        name = CommonResources.Settings_EditProfile_ShowDayAndMonth
      },
      new BGType()
      {
        id = 0,
        name = CommonResources.Settings_EditProfile_DoNotShow
      }
    };
    private readonly Dictionary<string, string> _updateDictionary = new Dictionary<string, string>();
    private static readonly List<BGType> _relationshipTypesListMale;
    private static readonly List<BGType> _relationshipTypesListFemale;
    private bool _isLoaded;
    private ProfileInfo _profileInfo;
    //private UploadPhotoResponseData _uploadResponseData;
    private static readonly City _defaultCity;
    private static readonly Country _defaultCountry;
    private bool _isSaving;

    public string PendingPartnerText
    {
      get
      {
        if (this._profileInfo != null && this._profileInfo.relation_pending == 1 && this._profileInfo.relation_partner != null)
          return CommonResources.Settings_EditProfile_PendingConfirmation;
        return "";
      }
    }

    public bool HavePendingPartner
    {
      get
      {
        if (this._profileInfo != null)
          return this._profileInfo.relation_pending == 1;
        return false;
      }
    }

    public string RelationRequestsText
    {
      get
      {
        if (this._profileInfo == null || this._profileInfo.relation_requests == null || this._profileInfo.relation_requests.Count <= 0)
          return "";
        if (this._profileInfo.relation_requests.Count == 1)
        {
          User user = this._profileInfo.relation_requests[0];
          return string.Format(user.IsFemale ? CommonResources.Settings_EditProfile_SomebodyIsInLoveWithYouFemaleFrm : CommonResources.Settings_EditProfile_SomebodyIsInLoveWithYouMaleFrm, (object) user.NameLink);
        }
        string commaSeparated = this._profileInfo.relation_requests.Select<User, string>((Func<User, string>) (u => u.NameLink)).ToList<string>().GetCommaSeparated(", ");
        string.Format(CommonResources.Settings_EditProfile_SomebodyIsInLoveWithYouManyFrm, (object) commaSeparated);
        return commaSeparated;
      }
    }

    public bool HaveRelationRequests
    {
      get
      {
        return !string.IsNullOrEmpty(this.RelationRequestsText);
      }
    }

    public string FirstName
    {
      get
      {
        if (this._profileInfo != null)
          return this._profileInfo.first_name;
        return string.Empty;
      }
      set
      {
        if (this._profileInfo == null)
          return;
        this._profileInfo.first_name = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.FirstName));
        this.AddToUpdateDictionary("first_name", this.FirstName);
      }
    }

    public string LastName
    {
      get
      {
        if (this._profileInfo != null)
          return this._profileInfo.last_name;
        return string.Empty;
      }
      set
      {
        if (this._profileInfo == null)
          return;
        this._profileInfo.last_name = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.LastName));
        this.AddToUpdateDictionary("last_name", this.LastName);
      }
    }

    public bool IsLoaded
    {
      get
      {
        return this._isLoaded;
      }
      private set
      {
        this._isLoaded = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsLoaded));
      }
    }

    public bool HaveNameRequestInProgress
    {
      get
      {
        if (this._profileInfo != null && this._profileInfo.name_request != null)
          return this._profileInfo.name_request.status == "processing";
        return false;
      }
    }

    public string RequestedName
    {
      get
      {
        if (this._profileInfo != null && this._profileInfo.name_request != null)
          return this._profileInfo.name_request.first_name + " " + this._profileInfo.name_request.last_name;
        return "";
      }
    }

    public bool HavePhoto
    {
      get
      {
        if (!string.IsNullOrEmpty(this.AvatarUri))
          return !this.AvatarUri.EndsWith("camera_200.png");
        return false;
      }
    }

    public string AvatarUri
    {
      get
      {
        if (this._profileInfo != null)
          return this._profileInfo.photo_max;
        return "";
      }
    }

    public bool IsMale
    {
      get
      {
        if (this._profileInfo != null)
          return this._profileInfo.sex == 2;
        return false;
      }
      set
      {
        if (!value || this._profileInfo == null)
          return;
        this._profileInfo.sex = 2;
        this.AddToUpdateDictionary("sex", "2");
        int saveRelTypeId = this.RelationshipType.id;
        this.NotifyPropertyChanged<List<BGType>>((System.Linq.Expressions.Expression<Func<List<BGType>>>) (() => this.RelationshipTypes));
        this.RelationshipType = this.RelationshipTypes.FirstOrDefault<BGType>((Func<BGType, bool>) (r => r.id == saveRelTypeId));
      }
    }

    public bool IsFemale
    {
      get
      {
        if (this._profileInfo != null)
          return this._profileInfo.sex == 1;
        return false;
      }
      set
      {
        if (!value || this._profileInfo == null)
          return;
        this._profileInfo.sex = 1;
        this.AddToUpdateDictionary("sex", "1");
        int saveRelTypeId = this.RelationshipType.id;
        this.NotifyPropertyChanged<List<BGType>>((System.Linq.Expressions.Expression<Func<List<BGType>>>) (() => this.RelationshipTypes));
        this.RelationshipType = this.RelationshipTypes.FirstOrDefault<BGType>((Func<BGType, bool>) (r => r.id == saveRelTypeId));
      }
    }

    public string BirthDateStr
    {
      get
      {
        if (this._profileInfo == null)
          return "";
        if (this.IsBDateSet)
          return this.BirthDateValue.ToString("dd.MM.yyyy");
        return CommonResources.Settings_EditProfile_Birthdate_Select;
      }
    }

    public DateTime BirthDateValue
    {
      get
      {
        DateTime dateTime = new DateTime(1960, 1, 1);
        if (!this.IsBDateSet)
          return dateTime;
        string[] strArray = this._profileInfo.bdate.Split('.');
        if (strArray.Length == 3)
          dateTime = new DateTime(int.Parse(strArray[2]), int.Parse(strArray[1]), int.Parse(strArray[0]));
        return dateTime;
      }
      set
      {
        DateTime dateTime = value;
        if (value >= DateTime.Now.AddYears(-14))
          dateTime = new DateTime(DateTime.Now.AddYears(-14).Year, value.Month, value.Day);
        this._profileInfo.bdate = dateTime.ToString("dd.MM.yyyy");
        this.AddToUpdateDictionary("bdate", this._profileInfo.bdate);
        this.NotifyPropertyChanged<DateTime>((System.Linq.Expressions.Expression<Func<DateTime>>) (() => this.BirthDateValue));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.BirthDateStr));
      }
    }

    private bool IsBDateSet
    {
      get
      {
        return this._profileInfo != null && !string.IsNullOrWhiteSpace(this._profileInfo.bdate) && !(this._profileInfo.bdate == "0.0.0");
      }
    }

    public List<BGType> BirthdaysShowTypes
    {
      get
      {
        return SettingsEditProfileViewModel._birthdayShowTypesList;
      }
    }

    public BGType BirthdayShowType
    {
      get
      {
        if (this._profileInfo != null)
          return SettingsEditProfileViewModel._birthdayShowTypesList.FirstOrDefault<BGType>((Func<BGType, bool>) (b => b.id == this._profileInfo.bdate_visibility));
        return SettingsEditProfileViewModel._birthdayShowTypesList.First<BGType>();
      }
      set
      {
        if (value == null || this._profileInfo == null)
          return;
        this._profileInfo.bdate_visibility = value.id;
        this.AddToUpdateDictionary("bdate_visibility", value.id.ToString());
      }
    }

    public List<BGType> RelationshipTypes
    {
      get
      {
        if (!this.IsFemale)
          return SettingsEditProfileViewModel._relationshipTypesListMale;
        return SettingsEditProfileViewModel._relationshipTypesListFemale;
      }
    }

    public BGType RelationshipType
    {
      get
      {
        if (this._profileInfo != null)
          return this.RelationshipTypes.FirstOrDefault<BGType>((Func<BGType, bool>) (r => r.id == this._profileInfo.relation));
        return this.RelationshipTypes.First<BGType>();
      }
      set
      {
        if (value == null || this._profileInfo == null)
          return;
        int num = this.Partner == null ? 0 : this.Partner.sex;
        if (value.id == 3 || value.id == 4)
          num = this.IsMale ? 1 : 2;
        if (this.Partner != null && this.Partner.sex != num)
          this.Partner = (User) null;
        this._profileInfo.relation = value.id;
        this.AddToUpdateDictionary("relation", value.id.ToString());
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsPartnerApplicable));
        this.NotifyPropertyChanged<BGType>((System.Linq.Expressions.Expression<Func<BGType>>) (() => this.RelationshipType));
      }
    }

    public bool IsPartnerApplicable
    {
      get
      {
        if (this.RelationshipType.id != 1 && this.RelationshipType.id != 6)
          return (uint) this.RelationshipType.id > 0U;
        return false;
      }
    }

    public bool HavePartner
    {
      get
      {
        if (this._profileInfo != null)
          return this._profileInfo.relation_partner != null;
        return false;
      }
    }

    public User Partner
    {
      get
      {
        if (this._profileInfo != null)
          return this._profileInfo.relation_partner;
        return (User) null;
      }
      set
      {
        if (this._profileInfo == null)
          return;
        this._profileInfo.relation_partner = value;
        this.AddToUpdateDictionary("relation_partner_id", value == null ? "0" : value.id.ToString());
        this.NotifyPropertyChanged<User>((System.Linq.Expressions.Expression<Func<User>>) (() => this.Partner));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HavePartner));
        this._profileInfo.relation_pending = 0;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HavePendingPartner));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.PendingPartnerText));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.RelationRequestsText));
      }
    }

    public Country Country
    {
      get
      {
        if (this._profileInfo != null && this._profileInfo.country != null)
          return this._profileInfo.country;
        return SettingsEditProfileViewModel._defaultCountry;
      }
      set
      {
        if (value == null)
          return;
        this._profileInfo.country = value;
        this.AddToUpdateDictionary("country_id", value == null ? "0" : value.id.ToString());
        this.ResetCity();
        this.NotifyPropertyChanged<Country>((System.Linq.Expressions.Expression<Func<Country>>) (() => this.Country));
      }
    }

    public City City
    {
      get
      {
        if (this._profileInfo != null && this._profileInfo.city != null)
          return this._profileInfo.city;
        return SettingsEditProfileViewModel._defaultCity;
      }
      set
      {
        if (value == null)
          return;
        this._profileInfo.city = value;
        this.AddToUpdateDictionary("city_id", value == null ? "0" : value.id.ToString());
        this.NotifyPropertyChanged<City>((System.Linq.Expressions.Expression<Func<City>>) (() => this.City));
      }
    }

    public bool IsDirty
    {
      get
      {
        return this._updateDictionary.Count > 0;
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
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsSaving));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanSave));
      }
    }

    public bool CanSave
    {
      get
      {
        if (this._isLoaded && !this._isSaving && (this.IsDirty && !string.IsNullOrWhiteSpace(this.FirstName)))
          return !string.IsNullOrWhiteSpace(this.LastName);
        return false;
      }
    }

    static SettingsEditProfileViewModel()
    {
      List<BGType> bgTypeList1 = new List<BGType>();
      BGType bgType1 = new BGType();
      bgType1.id = 0;
      string lowerInvariant1 = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant();
      bgType1.name = lowerInvariant1;
      bgTypeList1.Add(bgType1);
      BGType bgType2 = new BGType();
      bgType2.id = 1;
      string lowerInvariant2 = CommonResources.Relationship_Single_Male.ToLowerInvariant();
      bgType2.name = lowerInvariant2;
      bgTypeList1.Add(bgType2);
      BGType bgType3 = new BGType();
      bgType3.id = 2;
      string lowerInvariant3 = CommonResources.Relationship_InARelationship_Male.ToLowerInvariant();
      bgType3.name = lowerInvariant3;
      bgTypeList1.Add(bgType3);
      BGType bgType4 = new BGType();
      bgType4.id = 3;
      string lowerInvariant4 = CommonResources.Relationship_Engaged_Male.ToLowerInvariant();
      bgType4.name = lowerInvariant4;
      bgTypeList1.Add(bgType4);
      BGType bgType5 = new BGType();
      bgType5.id = 4;
      string lowerInvariant5 = CommonResources.Relationship_Married_Male.ToLowerInvariant();
      bgType5.name = lowerInvariant5;
      bgTypeList1.Add(bgType5);
      BGType bgType6 = new BGType();
      bgType6.id = 5;
      string lowerInvariant6 = CommonResources.Relationship_ItIsComplicated.ToLowerInvariant();
      bgType6.name = lowerInvariant6;
      bgTypeList1.Add(bgType6);
      BGType bgType7 = new BGType();
      bgType7.id = 6;
      string lowerInvariant7 = CommonResources.Relationship_ActivelySearching.ToLowerInvariant();
      bgType7.name = lowerInvariant7;
      bgTypeList1.Add(bgType7);
      BGType bgType8 = new BGType();
      bgType8.id = 7;
      string lowerInvariant8 = CommonResources.Relationship_InLove_Male.ToLowerInvariant();
      bgType8.name = lowerInvariant8;
      bgTypeList1.Add(bgType8);
      SettingsEditProfileViewModel._relationshipTypesListMale = bgTypeList1;
      List<BGType> bgTypeList2 = new List<BGType>();
      BGType bgType9 = new BGType();
      bgType9.id = 0;
      string lowerInvariant9 = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant();
      bgType9.name = lowerInvariant9;
      bgTypeList2.Add(bgType9);
      BGType bgType10 = new BGType();
      bgType10.id = 1;
      string lowerInvariant10 = CommonResources.Relationship_Single_Female.ToLowerInvariant();
      bgType10.name = lowerInvariant10;
      bgTypeList2.Add(bgType10);
      BGType bgType11 = new BGType();
      bgType11.id = 2;
      string lowerInvariant11 = CommonResources.Relationship_InARelationship_Female.ToLowerInvariant();
      bgType11.name = lowerInvariant11;
      bgTypeList2.Add(bgType11);
      BGType bgType12 = new BGType();
      bgType12.id = 3;
      string lowerInvariant12 = CommonResources.Relationship_Engaged_Female.ToLowerInvariant();
      bgType12.name = lowerInvariant12;
      bgTypeList2.Add(bgType12);
      BGType bgType13 = new BGType();
      bgType13.id = 4;
      string lowerInvariant13 = CommonResources.Relationship_Married_Female.ToLowerInvariant();
      bgType13.name = lowerInvariant13;
      bgTypeList2.Add(bgType13);
      BGType bgType14 = new BGType();
      bgType14.id = 5;
      string lowerInvariant14 = CommonResources.Relationship_ItIsComplicated.ToLowerInvariant();
      bgType14.name = lowerInvariant14;
      bgTypeList2.Add(bgType14);
      BGType bgType15 = new BGType();
      bgType15.id = 6;
      string lowerInvariant15 = CommonResources.Relationship_ActivelySearching.ToLowerInvariant();
      bgType15.name = lowerInvariant15;
      bgTypeList2.Add(bgType15);
      BGType bgType16 = new BGType();
      bgType16.id = 7;
      string lowerInvariant16 = CommonResources.Relationship_InLove_Female.ToLowerInvariant();
      bgType16.name = lowerInvariant16;
      bgTypeList2.Add(bgType16);
      SettingsEditProfileViewModel._relationshipTypesListFemale = bgTypeList2;
      SettingsEditProfileViewModel._defaultCity = new City()
      {
        id = 0L,
        title = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant()
      };
      SettingsEditProfileViewModel._defaultCountry = new Country()
      {
        id = 0L,
        title = CommonResources.Settings_EditProfile_NoneSelected.ToLowerInvariant()
      };
    }

    public SettingsEditProfileViewModel()
    {
      EventAggregator.Current.Subscribe((object) this);
    }

    public void ResetCountry()
    {
      this._profileInfo.country = (Country) null;
      this.AddToUpdateDictionary("country_id", "0");
      this.ResetCity();
      this.NotifyPropertyChanged<Country>((System.Linq.Expressions.Expression<Func<Country>>) (() => this.Country));
    }

    public void ResetCity()
    {
      this._profileInfo.city = (City) null;
      this.AddToUpdateDictionary("city_id", "0");
      this.NotifyPropertyChanged<City>((System.Linq.Expressions.Expression<Func<City>>) (() => this.City));
    }

    public void AddToUpdateDictionary(string key, string value)
    {
      this._updateDictionary[key] = value;
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsDirty));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanSave));
    }

    public override void Load(Action<bool> callback)
    {
      AccountService.Instance.GetSettingsProfileInfo((Action<BackendResult<ProfileInfo, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          this.IsLoaded = true;
          this._profileInfo = result.ResultData;
          callback(true);
          this.NotifyProperties();
        }
        else
          callback(false);
      }))));
    }

    public void UploadUserPhoto(Stream stream, Rect rect)
    {
      if (this.IsSaving)
        return;
      this.IsSaving = true;
      this.SetInProgress(true, "");
      ImagePreprocessor.PreprocessImage(stream, VKConstants.ResizedImageSize, true, (Action<ImagePreprocessResult>) (pres =>
      {
        Stream stream1 = pres.Stream;
        byte[] numArray = new byte[stream1.Length];
        stream1.Read(numArray, 0, (int) stream1.Length);
        stream1.Close();
        UsersService.Instance.SaveProfilePhoto(ImagePreprocessor.GetThumbnailRect((double) pres.Width, (double) pres.Height, rect), numArray, (Action<BackendResult<ProfilePhoto, ResultCode>>) (res =>
        {
          this.IsSaving = false;
          this.SetInProgress(false, "");
          if (res.ResultCode == ResultCode.Succeeded)
          {
            BaseDataManager.Instance.NeedRefreshBaseData = true;
            BaseDataManager.Instance.RefreshBaseDataIfNeeded();
          }
          else
            ExtendedMessageBox.ShowSafe(CommonResources.Error);
        }));
      }));
    }

    public void Save()
    {
      if (!this.CanSave)
        return;
      this.IsSaving = true;
      this.SetInProgress(true, "");
      AccountService.Instance.SaveSettingsAccountInfo(this._updateDictionary, (Action<BackendResult<SaveProfileResponse, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.IsSaving = false;
        this.SetInProgress(false, "");
        if (res.ResultCode == ResultCode.Succeeded)
        {
          if (this._updateDictionary.ContainsKey("first_name") || this._updateDictionary.ContainsKey("last_name"))
          {
            int num = (int) MessageBox.Show(CommonResources.Settings_EditProfile_ChangeNameRequestDesc, CommonResources.Settings_EditProfile_ChangeNameRequest, MessageBoxButton.OK);
          }
          EventAggregator.Current.Publish((object) new BaseDataChangedEvent()
          {
            IsProfileUpdateRequired = true
          });
          Navigator.Current.GoBack();
        }
        else
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
      })))/*, this._uploadResponseData*/);
    }

    private void NotifyProperties()
    {
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.AvatarUri));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.BirthDateStr));
      this.NotifyPropertyChanged<BGType>((System.Linq.Expressions.Expression<Func<BGType>>) (() => this.BirthdayShowType));
      this.NotifyPropertyChanged<List<BGType>>((System.Linq.Expressions.Expression<Func<List<BGType>>>) (() => this.BirthdaysShowTypes));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveNameRequestInProgress));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HavePhoto));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsFemale));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsMale));
      this.NotifyPropertyChanged<BGType>((System.Linq.Expressions.Expression<Func<BGType>>) (() => this.RelationshipType));
      this.NotifyPropertyChanged<List<BGType>>((System.Linq.Expressions.Expression<Func<List<BGType>>>) (() => this.RelationshipTypes));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.RequestedName));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsPartnerApplicable));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HavePartner));
      this.NotifyPropertyChanged<User>((System.Linq.Expressions.Expression<Func<User>>) (() => this.Partner));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.FirstName));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.LastName));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.PendingPartnerText));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HavePendingPartner));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.RelationRequestsText));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveRelationRequests));
    }

    internal void CancelNameRequest()
    {
      if (this._profileInfo == null || this._profileInfo.name_request == null || (this._profileInfo.name_request.id == 0L || this.IsSaving))
        return;
      this.IsSaving = true;
      this.SetInProgress(true, "");
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["cancel_request_id"] = this._profileInfo.name_request.id.ToString();
      AccountService.Instance.SaveSettingsAccountInfo(parameters, (Action<BackendResult<SaveProfileResponse, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.IsSaving = false;
        this.SetInProgress(false, "");
        if (res.ResultCode == ResultCode.Succeeded)
        {
          this._profileInfo.name_request = (NameChangeRequest) null;
          this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HaveNameRequestInProgress));
        }
        else
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
      }))), (UploadPhotoResponseData) null);
    }

    internal void DeletePhoto()
    {
      this.SetInProgress(true, "");
      AccountService.Instance.DeleteProfilePhoto((Action<BackendResult<User, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.SetInProgress(false, "");
        if (res.ResultCode != ResultCode.Succeeded)
          return;
        if (this._profileInfo != null)
        {
          if (AppGlobalStateManager.Current.GlobalState.LoggedInUser == null)
            return;
          AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max = res.ResultData.photo_max;
          EventAggregator.Current.Publish((object) new BaseDataChangedEvent()
          {
            IsProfileUpdateRequired = true
          });
        }
        else
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
      }))));
    }

    public void Handle(BaseDataChangedEvent message)
    {
      if (this._profileInfo == null)
        return;
      this._profileInfo.photo_max = AppGlobalStateManager.Current.GlobalState.LoggedInUser.photo_max;
      this.NotifyAvatarChanged();
    }

    public void NotifyAvatarChanged()
    {
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.AvatarUri));
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.HavePhoto));
    }
  }
}
