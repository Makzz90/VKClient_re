using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library.Registration;
using VKClient.Common.Utils;

namespace VKClient.Common.UC.Registration
{
  public class RegistrationStep2UC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBox textBoxCountry;
    internal TextBox textBoxPhoneNumber;
    internal TextBlock textBlockPhoneNumberWatermark;
    private bool _contentLoaded;

    private RegistrationPhoneNumberViewModel RegistrationPhoneNumberVM
    {
      get
      {
        return base.DataContext as RegistrationPhoneNumberViewModel;
      }
    }

    public bool ShowingPopup { get; set; }

    public RegistrationStep2UC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.RegistrationStep2UC_Loaded));
    }

    private void RegistrationStep2UC_Loaded(object sender, RoutedEventArgs e)
    {
      ((UIElement) this.textBlockPhoneNumberWatermark).Visibility = (this.textBoxPhoneNumber.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible);
    }

    private void textBoxTextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
      ((UIElement) this.textBlockPhoneNumberWatermark).Visibility = (this.textBoxPhoneNumber.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible);
    }

    private void UpdateSource(TextBox textBox)
    {
      ((FrameworkElement) textBox).GetBindingExpression((DependencyProperty) TextBox.TextProperty).UpdateSource();
    }

    private void textBoxCountry_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ShowingPopup = true;
      CountryPickerUC.Show(this.RegistrationPhoneNumberVM.Country, false, (Action<Country>) (c =>
      {
        this.ShowingPopup = false;
        this.RegistrationPhoneNumberVM.Country = c;
        ((Control) this.textBoxPhoneNumber).Focus();
      }), (Action) (() => this.ShowingPopup = false));
    }

    private void textBoxPhoneNumber_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      RegistrationPage currentPage = FramePageUtils.CurrentPage as RegistrationPage;
      if (currentPage == null)
        return;
      currentPage.RegistrationVM.CompleteCurrentStep();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/Registration/RegistrationStep2UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBoxCountry = (TextBox) base.FindName("textBoxCountry");
      this.textBoxPhoneNumber = (TextBox) base.FindName("textBoxPhoneNumber");
      this.textBlockPhoneNumberWatermark = (TextBlock) base.FindName("textBlockPhoneNumberWatermark");
    }
  }
}
