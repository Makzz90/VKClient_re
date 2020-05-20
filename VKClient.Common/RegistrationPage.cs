using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Library.Registration;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.UC.Registration;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class RegistrationPage : PageBase
  {
    private bool _isInitialized;
    private string _registrationVMFileID;
    private ApplicationBar _appBar;
    private ApplicationBarIconButton _appBarButtonCheck;
    private readonly DelayedExecutor _de;
    internal GenericHeaderUC Header;
    internal Rectangle rectProgress;
    internal RegistrationStep1UC ucRegistrationStep1;
    internal RegistrationStep2UC ucRegistrationStep2;
    internal RegistrationStep3UC ucRegistrationStep3;
    internal RegistrationStep4UC ucRegistrationStep4;
    private bool _contentLoaded;

    public RegistrationViewModel RegistrationVM
    {
      get
      {
        return base.DataContext as RegistrationViewModel;
      }
    }

    public RegistrationPage()
    {
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar.Opacity = num;
      this._appBar = applicationBar;
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton();
      Uri uri = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton.IconUri = uri;
      string chatEditAppBarSave = CommonResources.ChatEdit_AppBar_Save;
      applicationBarIconButton.Text = chatEditAppBarSave;
      this._appBarButtonCheck = applicationBarIconButton;
      this._de = new DelayedExecutor(300);
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.Header.HideSandwitchButton = true;
      this.SuppressMenu = true;
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      this._appBarButtonCheck.Click+=(new EventHandler(this._appBarButtonCheck_Click));
      this._appBar.Buttons.Add(this._appBarButtonCheck);
      this.ApplicationBar = ((IApplicationBar) this._appBar);
    }

    private void _appBarButtonCheck_Click(object sender, EventArgs e)
    {
      switch (this.RegistrationVM.CurrentStep)
      {
        case 1:
          if (this.ucRegistrationStep1.textBoxFirstName.Text.Length < 2 || this.ucRegistrationStep1.textBoxLastName.Text.Length < 2)
          {
            new GenericInfoUC().ShowAndHideLater(CommonResources.Registration_WrongName,  null);
            return;
          }
          break;
        case 4:
          if (this.ucRegistrationStep4.passwordBox.Password.Length < 6)
          {
            new GenericInfoUC().ShowAndHideLater(CommonResources.Registration_ShortPassword,  null);
            return;
          }
          break;
      }
      this.RegistrationVM.CompleteCurrentStep();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._registrationVMFileID = ((Page) this).NavigationContext.QueryString["SessionId"];
        RegistrationViewModel registrationViewModel = new RegistrationViewModel();
        CacheManager.TryDeserialize((IBinarySerializable) registrationViewModel, this._registrationVMFileID, CacheManager.DataType.CachedData);
        registrationViewModel.OnMovedForward = (Action) (() => this.HandleMoveBackOrForward());
        base.DataContext = registrationViewModel;
        this.HandleMoveBackOrForward();
        this._isInitialized = true;
        registrationViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
        this.UpdateButtonIsEnabled();
      }
      this.HandleInputParams();
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      CacheManager.TrySerialize((IBinarySerializable) this.RegistrationVM, this._registrationVMFileID, false, CacheManager.DataType.CachedData);
    }

    protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
    {
      base.OnRemovedFromJournal(e);
      CacheManager.TryDelete(this._registrationVMFileID, CacheManager.DataType.CachedData);
    }

    private void HandleMoveBackOrForward()
    {
      int num1 = this.RegistrationVM.CurrentStep - 1;
      bool flag = num1 <= 3;
      if (!flag)
        ((Page) this).NavigationService.ClearBackStack();
      ((FrameworkElement) this.rectProgress).Width=(flag ? 120.0 : 240.0);
      double num2 = flag ? (double) (120 * num1) : (double) (240 * (num1 - 4));
      TranslateTransform renderTransform = ((UIElement) this.rectProgress).RenderTransform as TranslateTransform;
      TranslateTransform translateTransform = renderTransform;
      double x = renderTransform.X;
      double to = num2;
      // ISSUE: variable of the null type
      int duration = 250;
      int? startTime = new int?(0);
      CubicEase cubicEase = new CubicEase();
      int num3 = 2;
      ((EasingFunctionBase) cubicEase).EasingMode = ((EasingMode) num3);
      // ISSUE: variable of the null type
      
      int num4 = 0;
      ((DependencyObject)translateTransform).Animate(x, to, TranslateTransform.XProperty, duration, startTime, (IEasingFunction)cubicEase, null, num4 != 0);
      switch (num1)
      {
        case 0:
          this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (string.IsNullOrEmpty(this.ucRegistrationStep1.textBoxFirstName.Text))
            {
              ((Control) this.ucRegistrationStep1.textBoxFirstName).Focus();
            }
            else
            {
              if (!string.IsNullOrEmpty(this.ucRegistrationStep1.textBoxLastName.Text))
                return;
              ((Control) this.ucRegistrationStep1.textBoxLastName).Focus();
            }
          }))));
          break;
        case 1:
          this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (!string.IsNullOrEmpty(this.ucRegistrationStep2.textBoxPhoneNumber.Text))
              return;
            ((Control) this.ucRegistrationStep2.textBoxPhoneNumber).Focus();
          }))));
          break;
        case 2:
          this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (!string.IsNullOrEmpty(this.ucRegistrationStep3.textBoxConfirmationCode.Text))
              return;
            ((Control) this.ucRegistrationStep3.textBoxConfirmationCode).Focus();
          }))));
          break;
        case 3:
          this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
          {
            if (!string.IsNullOrEmpty(this.ucRegistrationStep4.passwordBox.Password))
              return;
            ((Control) this.ucRegistrationStep4.passwordBox).Focus();
          }))));
          break;
      }
    }

    private void HandleInputParams()
    {
      List<Stream> parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("ChoosenPhotos") as List<Stream>;
      Rect rect = new Rect();
      if (ParametersRepository.Contains("UserPicSquare"))
        rect = (Rect) ParametersRepository.GetParameterForIdAndReset("UserPicSquare");
      if (parameterForIdAndReset == null || parameterForIdAndReset.Count <= 0)
        return;
      this.RegistrationVM.SetUserPhoto(parameterForIdAndReset[0], rect);
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "CanCompleteCurrentStep"))
        return;
      this.UpdateButtonIsEnabled();
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      base.OnBackKeyPress(e);
      if (this.ucRegistrationStep2.ShowingPopup || !this.RegistrationVM.HandleBackKey())
        return;
      this.HandleMoveBackOrForward();
      e.Cancel = true;
    }

    private void UpdateButtonIsEnabled()
    {
      this._appBarButtonCheck.IsEnabled = this.RegistrationVM.CanCompleteCurrentStep;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/RegistrationPage.xaml", UriKind.Relative));
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.rectProgress = (Rectangle) base.FindName("rectProgress");
      this.ucRegistrationStep1 = (RegistrationStep1UC) base.FindName("ucRegistrationStep1");
      this.ucRegistrationStep2 = (RegistrationStep2UC) base.FindName("ucRegistrationStep2");
      this.ucRegistrationStep3 = (RegistrationStep3UC) base.FindName("ucRegistrationStep3");
      this.ucRegistrationStep4 = (RegistrationStep4UC) base.FindName("ucRegistrationStep4");
    }
  }
}
