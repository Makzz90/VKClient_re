using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Library.Registration
{
  public class RegistrationViewModel : ViewModelBase, IBinarySerializable
  {
    private RegistrationProfileViewModel _registrationProfileVM = new RegistrationProfileViewModel();
    private RegistrationPhoneNumberViewModel _registrationPhoneNumberVM = new RegistrationPhoneNumberViewModel();
    private RegistrationPasswordViewModel _registrationPasswordVM = new RegistrationPasswordViewModel();
    private RegistrationAddFriendsViewModel _registrationAddFriendsVM = new RegistrationAddFriendsViewModel();
    private RegistrationInterestingPagesViewModel _registrationInterestingPagesVM = new RegistrationInterestingPagesViewModel();
    private string _sid = "";
    private Action _onMovedForward = (Action) (() => {});
    private RegistrationConfirmationCodeViewModel _registrationConfirmationCodeVM;
    private ICompleteable _currentVM;
    private bool _friendsSearchDataLoaded;

    public Action OnMovedForward
    {
      get
      {
        return this._onMovedForward;
      }
      set
      {
        if (value == null)
          return;
        this._onMovedForward = value;
      }
    }

    public string Title
    {
      get
      {
        if (this.Step1Visibility == Visibility.Visible)
          return CommonResources.Registration_Title_Registration;
        if (this.Step2Visibility == Visibility.Visible)
          return CommonResources.Registration_Title_PhoneNumber;
        if (this.Step3Visibility == Visibility.Visible)
          return CommonResources.Registration_Title_Confirmation;
        if (this.Step4Visibility == Visibility.Visible)
          return CommonResources.Registration_Title_Password;
        if (this.Step5Visibility == Visibility.Visible)
          return CommonResources.Registration_Title_FindFriends;
        if (this.Step6Visibility == Visibility.Visible)
          return CommonResources.Registration_Title_InterestingPages;
        return "";
      }
    }

    public ICompleteable CurrentVM
    {
      get
      {
        return this._currentVM;
      }
      private set
      {
        if (this._currentVM != null)
          (this._currentVM as INotifyPropertyChanged).PropertyChanged -= new PropertyChangedEventHandler(this.ChildViewModel_PropertyChanged);
        this._currentVM = value;
        if (this._currentVM != null)
          (this._currentVM as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler(this.ChildViewModel_PropertyChanged);
        if (this._currentVM == this._registrationInterestingPagesVM)
          this._registrationInterestingPagesVM.EnsureLoadData();
        if (this._currentVM == this._registrationAddFriendsVM && !this._friendsSearchDataLoaded)
        {
          this._registrationAddFriendsVM.FriendsSearchVM.LoadData();
          this._friendsSearchDataLoaded = true;
        }
        this.NotifyPropertyChanged<ICompleteable>((System.Linq.Expressions.Expression<Func<ICompleteable>>) (() => this.CurrentVM));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanCompleteCurrentStep));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.Step1Visibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.Step2Visibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.Step3Visibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.Step4Visibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.Step5Visibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.Step6Visibility));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Title));
      }
    }

    public bool CanCompleteCurrentStep
    {
      get
      {
        return this.CurrentVM.IsCompleted;
      }
    }

    public RegistrationProfileViewModel RegistrationProfileVM
    {
      get
      {
        return this._registrationProfileVM;
      }
    }

    public RegistrationPhoneNumberViewModel RegistrationPhoneNumberVM
    {
      get
      {
        return this._registrationPhoneNumberVM;
      }
    }

    public RegistrationAddFriendsViewModel RegistrationAddFriendsVM
    {
      get
      {
        return this._registrationAddFriendsVM;
      }
    }

    public RegistrationInterestingPagesViewModel RegistrationInterestingPagesVM
    {
      get
      {
        return this._registrationInterestingPagesVM;
      }
    }

    public RegistrationConfirmationCodeViewModel RegistrationConfirmationCodeVM
    {
      get
      {
        return this._registrationConfirmationCodeVM;
      }
      private set
      {
        this._registrationConfirmationCodeVM = value;
        this.NotifyPropertyChanged<RegistrationConfirmationCodeViewModel>((System.Linq.Expressions.Expression<Func<RegistrationConfirmationCodeViewModel>>) (() => this.RegistrationConfirmationCodeVM));
      }
    }

    public RegistrationPasswordViewModel RegistrationPasswordVM
    {
      get
      {
        return this._registrationPasswordVM;
      }
    }

    public Visibility Step1Visibility
    {
      get
      {
        return this.CurrentStep != 1 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility Step2Visibility
    {
      get
      {
        return this.CurrentStep != 2 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility Step3Visibility
    {
      get
      {
        return this.CurrentStep != 3 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility Step4Visibility
    {
      get
      {
        return this.CurrentStep != 4 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility Step5Visibility
    {
      get
      {
        return this.CurrentStep != 5 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility Step6Visibility
    {
      get
      {
        return this.CurrentStep != 6 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public int CurrentStep
    {
      get
      {
        if (this.CurrentVM == this.RegistrationProfileVM)
          return 1;
        if (this.CurrentVM == this.RegistrationPhoneNumberVM)
          return 2;
        if (this.CurrentVM is RegistrationConfirmationCodeViewModel)
          return 3;
        if (this.CurrentVM == this.RegistrationPasswordVM)
          return 4;
        if (this.CurrentVM == this.RegistrationAddFriendsVM)
          return 5;
        return this.CurrentVM == this.RegistrationInterestingPagesVM ? 6 : 0;
      }
      private set
      {
        switch (value)
        {
          case 1:
            this.CurrentVM = (ICompleteable) this.RegistrationProfileVM;
            break;
          case 2:
            this.CurrentVM = (ICompleteable) this.RegistrationPhoneNumberVM;
            break;
          case 3:
            this.CurrentVM = (ICompleteable) this.RegistrationConfirmationCodeVM;
            break;
          case 4:
            this.CurrentVM = (ICompleteable) this.RegistrationPasswordVM;
            break;
          case 5:
            this.CurrentVM = (ICompleteable) this.RegistrationAddFriendsVM;
            break;
          case 6:
            this.CurrentVM = (ICompleteable) this.RegistrationInterestingPagesVM;
            break;
        }
      }
    }

    public RegistrationViewModel()
    {
      this.CurrentVM = (ICompleteable) this._registrationProfileVM;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write<RegistrationProfileViewModel>(this._registrationProfileVM, false);
      writer.Write<RegistrationPhoneNumberViewModel>(this._registrationPhoneNumberVM, false);
      writer.Write<RegistrationConfirmationCodeViewModel>(this._registrationConfirmationCodeVM, false);
      writer.Write<RegistrationPasswordViewModel>(this._registrationPasswordVM, false);
      writer.WriteString(this._sid);
      writer.Write(this.CurrentStep);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._registrationProfileVM = reader.ReadGeneric<RegistrationProfileViewModel>();
      this._registrationPhoneNumberVM = reader.ReadGeneric<RegistrationPhoneNumberViewModel>();
      this._registrationConfirmationCodeVM = reader.ReadGeneric<RegistrationConfirmationCodeViewModel>();
      if (this._registrationConfirmationCodeVM != null)
        this._registrationConfirmationCodeVM.RequestVoiceCallAction = new Action<Action<bool>>(this.RequestVoiceCall);
      this._registrationPasswordVM = reader.ReadGeneric<RegistrationPasswordViewModel>();
      this._sid = reader.ReadString();
      this.CurrentStep = reader.ReadInt32();
    }

    private void ChildViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "IsCompleted"))
        return;
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.CanCompleteCurrentStep));
    }

    internal bool HandleBackKey()
    {
      bool flag = false;
      if (this.Step2Visibility == Visibility.Visible)
      {
        this.CurrentVM = (ICompleteable) this.RegistrationProfileVM;
        flag = true;
      }
      else if (this.Step3Visibility == Visibility.Visible)
      {
        this.CurrentVM = (ICompleteable) this.RegistrationPhoneNumberVM;
        flag = true;
      }
      else if (this.Step4Visibility == Visibility.Visible)
      {
        this.CurrentVM = (ICompleteable) this.RegistrationConfirmationCodeVM;
        flag = true;
      }
      else if (this.Step6Visibility == Visibility.Visible)
      {
        this.CurrentVM = (ICompleteable) this.RegistrationAddFriendsVM;
        flag = true;
      }
      return flag;
    }

    public void CompleteCurrentStep()
    {
      if (this.IsInProgress || !this.CurrentVM.IsCompleted)
        return;
      this.SetInProgress(true, "");
      if (this.CurrentVM == this._registrationProfileVM)
        this._registrationPhoneNumberVM.EnsureInitialized((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (res)
          {
            this.CurrentVM = (ICompleteable) this._registrationPhoneNumberVM;
            this.OnMovedForward();
          }
          this.SetInProgress(false, "");
        }))));
      else if (this.CurrentVM == this._registrationPhoneNumberVM)
        SignUpService.Instance.SignUp(this._registrationPhoneNumberVM.PhoneNumberString, this._registrationProfileVM.FirstName, this._registrationProfileVM.LastName, false, this._registrationProfileVM.IsMale, "", (Action<BackendResult<string, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.SetInProgress(false, "");
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", res.Error);
          if (res.ResultCode != ResultCode.Succeeded)
            return;
          this.CurrentVM = this.CreateRegistrationConfirmationCodeViewModel();
          this.OnMovedForward();
          this._sid = res.ResultData;
        }))));
      else if (this.CurrentVM == this._registrationConfirmationCodeVM)
        SignUpService.Instance.ConfirmSignUp(this._registrationPhoneNumberVM.PhoneNumberString, this._registrationConfirmationCodeVM.ConfirmationCode, "", (Action<BackendResult<SignupConfirmation, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.SetInProgress(false, "");
          if (res.ResultCode == ResultCode.BadPassword)
          {
            this.CurrentVM = (ICompleteable) this._registrationPasswordVM;
            this.OnMovedForward();
          }
          else
            GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", res.Error);
        }))));
      else if (this.CurrentVM == this._registrationPasswordVM)
      {
        if (this._registrationPasswordVM.PasswordStr.Contains(" "))
        {
          new GenericInfoUC().ShowAndHideLater(CommonResources.PasswordCannotContainWhitespaces, null);
          this.SetInProgress(false, "");
        }
        else
          SignUpService.Instance.ConfirmSignUp(this._registrationPhoneNumberVM.PhoneNumberString, this._registrationConfirmationCodeVM.ConfirmationCode, this._registrationPasswordVM.PasswordStr, (Action<BackendResult<SignupConfirmation, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
          {
            this.SetInProgress(false, "");
            GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", res.Error);
            if (res.ResultCode != ResultCode.Succeeded || res.ResultData.success != 1)
              return;
            this.PerformLogin((Action<bool>) (loginResult =>
            {
              if (!loginResult)
                return;
              Execute.ExecuteOnUIThread((Action) (() =>
              {
                this.CurrentVM = (ICompleteable) this._registrationAddFriendsVM;
                this.OnMovedForward();
              }));
            }));
          }))));
      }
      else if (this.CurrentVM == this._registrationAddFriendsVM)
      {
        this.CurrentVM = (ICompleteable) this._registrationInterestingPagesVM;
        this.SetInProgress(false, "");
        this.OnMovedForward();
      }
      else
      {
        if (this.CurrentVM != this._registrationInterestingPagesVM)
          return;
        Navigator.Current.NavigateToMainPage();
      }
    }

    private void PerformLogin(Action<bool> resultCallback)
    {
      if (this.IsInProgress)
        return;
      this.SetInProgress(true, "");
      LoginService.Instance.GetAccessToken(this._registrationPhoneNumberVM.PhoneNumberString, this._registrationPasswordVM.PasswordStr, (Action<BackendResult<AutorizationData, ResultCode>>) (result =>
      {
        this.SetInProgress(false, "");
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
          {
            this.HandleLogin(result);
            resultCallback(true);
          }
          else
          {
            GenericInfoUC.ShowBasedOnResult((int) result.ResultCode, "", (VKRequestsDispatcher.Error) null);
            resultCallback(false);
          }
        }));
      }));
    }

    private void HandleLogin(BackendResult<AutorizationData, ResultCode> result)
    {
      ServiceLocator.Resolve<IAppStateInfo>().HandleSuccessfulLogin(result.ResultData, false);
      if (string.IsNullOrEmpty(this._registrationProfileVM.FullAvatarUri))
        return;
      SettingsEditProfileViewModel profileViewModel = new SettingsEditProfileViewModel();
      using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
      {
        MemoryStream memoryStream = new MemoryStream();
        using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(this._registrationProfileVM.FullAvatarUri, FileMode.Open, FileAccess.Read))
          memoryStream = StreamUtils.ReadFully((Stream) storageFileStream);
        memoryStream.Position = 0L;
        profileViewModel.UploadUserPhoto((Stream) memoryStream, this._registrationProfileVM.CropPhotoRect);
      }
    }

    private ICompleteable CreateRegistrationConfirmationCodeViewModel()
    {
      this.RegistrationConfirmationCodeVM = new RegistrationConfirmationCodeViewModel(this._registrationPhoneNumberVM.PhonePrefix, this._registrationPhoneNumberVM.PhoneNumber, new Action<Action<bool>>(this.RequestVoiceCall));
      return (ICompleteable) this.RegistrationConfirmationCodeVM;
    }

    private void RequestVoiceCall(Action<bool> resultCallback)
    {
      if (this.IsInProgress)
        return;
      this.SetInProgress(true, "");
      SignUpService.Instance.SignUp(this._registrationPhoneNumberVM.PhoneNumberString, this._registrationProfileVM.FirstName, this._registrationProfileVM.LastName, true, this._registrationProfileVM.IsMale, this._sid, (Action<BackendResult<string, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.SetInProgress(false, "");
        GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, "", (VKRequestsDispatcher.Error) null);
        resultCallback(res.ResultCode == ResultCode.Succeeded);
      }))));
    }

    internal void SetUserPhoto(Stream stream, Rect rect)
    {
      this._registrationProfileVM.SetUserPhoto(stream, rect);
    }
  }
}
