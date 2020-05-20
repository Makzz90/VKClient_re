using Microsoft.Phone.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VKClient.Common.Backend;

namespace VKClient.Common.UC
{
  public class CaptchaRequestControl : UserControl
  {
    private CaptchaUserRequest _captchaUserRequest;
    private Action<CaptchaUserResponse> _callback;
    private PhoneApplicationPage _parentPage;
    private bool? _isParentPageAppBarVisible;
    private ITransition _currentAnimation;
    private bool _currentAnimationIsOpening;
    internal Grid BackgroundPanel;
    internal Grid ContentPanel;
    internal Image CaptchaImage;
    internal TextBox CaptchaBox;
    private bool _contentLoaded;

    private Visibility Visibility
    {
      set
      {
        if (base.Visibility == value && this._currentAnimation != null)
        {
          this._currentAnimation.Stop();
          if (!this._currentAnimationIsOpening)
            base.Visibility = Visibility.Collapsed;
          this._currentAnimation =  null;
        }
        if (base.Visibility == value)
          return;
        if (value == Visibility.Visible)
        {
          base.Visibility = Visibility.Visible;
          this._currentAnimationIsOpening = true;
          SwivelTransition swivelTransition = new SwivelTransition();
          swivelTransition.Mode = SwivelTransitionMode.ForwardIn;
          Grid contentPanel = this.ContentPanel;
          this._currentAnimation = swivelTransition.GetTransition((UIElement) contentPanel);
          this._currentAnimation.Begin();
          if (this._parentPage.ApplicationBar != null)
          {
            this._isParentPageAppBarVisible = new bool?(this._parentPage.ApplicationBar.IsVisible);
            this._parentPage.ApplicationBar.IsVisible = false;
          }
          else
            this._isParentPageAppBarVisible = new bool?();
          this._parentPage.BackKeyPress += (new EventHandler<CancelEventArgs>(this.ParentPage_OnBackKeyPressed));
          ((Control) this.CaptchaBox).Focus();
        }
        else
        {
          if (this._parentPage.ApplicationBar != null && this._isParentPageAppBarVisible.HasValue)
            this._parentPage.ApplicationBar.IsVisible = this._isParentPageAppBarVisible.Value;
          this._parentPage.BackKeyPress -= (new EventHandler<CancelEventArgs>(this.ParentPage_OnBackKeyPressed));
          this._currentAnimationIsOpening = false;
          SwivelTransition swivelTransition = new SwivelTransition();
          swivelTransition.Mode = SwivelTransitionMode.ForwardOut;
          Grid contentPanel = this.ContentPanel;
          this._currentAnimation = swivelTransition.GetTransition((UIElement) contentPanel);
          this._currentAnimation.Completed += (EventHandler) ((o, e) => base.Visibility = Visibility.Collapsed);
          this._currentAnimation.Begin();
        }
      }
    }

    public CaptchaRequestControl()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      base.Visibility = Visibility.Collapsed;
    }

    public void ShowCaptchaRequest(PhoneApplicationPage parentPage, CaptchaUserRequest captchaUserRequest, Action<CaptchaUserResponse> callback)
    {
      this._captchaUserRequest = captchaUserRequest;
      this._parentPage = parentPage;
      this._callback = callback;
      this.Visibility = Visibility.Visible;
      this.CaptchaBox.Text = string.Empty;
      this.CaptchaImage.Source = ((ImageSource) new BitmapImage(new Uri(captchaUserRequest.Url)));
    }

    private void ValidateCaptcha()
    {
      CaptchaUserResponse captchaUserResponse = new CaptchaUserResponse();
      captchaUserResponse.EnteredString = this.CaptchaBox.Text;
      captchaUserResponse.Request = this._captchaUserRequest;
      int num = 0;
      captchaUserResponse.IsCancelled = num != 0;
      this._callback(captchaUserResponse);
      this.Visibility = Visibility.Collapsed;
    }

    private void CancelCaptcha()
    {
      CaptchaUserResponse captchaUserResponse = new CaptchaUserResponse();
      captchaUserResponse.Request = this._captchaUserRequest;
      int num = 1;
      captchaUserResponse.IsCancelled = num != 0;
      this._callback(captchaUserResponse);
      this.Visibility = Visibility.Collapsed;
    }

    private void CaptchaImage_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ValidateCaptcha();
    }

    private void CaptchaBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      this.ValidateCaptcha();
    }

    private void SendButton_OnClick(object sender, RoutedEventArgs e)
    {
      this.ValidateCaptcha();
    }

    private void BackgroundPanel_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (((RoutedEventArgs) e).OriginalSource != this.BackgroundPanel)
        return;
      this.CancelCaptcha();
    }

    private void ParentPage_OnBackKeyPressed(object sender, CancelEventArgs e)
    {
      e.Cancel = true;
      this.CancelCaptcha();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/CaptchaRequestControl.xaml", UriKind.Relative));
      this.BackgroundPanel = (Grid) base.FindName("BackgroundPanel");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
      this.CaptchaImage = (Image) base.FindName("CaptchaImage");
      this.CaptchaBox = (TextBox) base.FindName("CaptchaBox");
    }
  }
}
