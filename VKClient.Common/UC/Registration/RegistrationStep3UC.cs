using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.Registration;
using VKClient.Common.Utils;

namespace VKClient.Common.UC.Registration
{
  public class RegistrationStep3UC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBox textBoxConfirmationCode;
    internal TextBlock textBlockConfirmationCodeWatermark;
    private bool _contentLoaded;

    public RegistrationStep3UC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.RegistrationStep3UC_Loaded));
    }

    private void RegistrationStep3UC_Loaded(object sender, RoutedEventArgs e)
    {
      ((UIElement) this.textBlockConfirmationCodeWatermark).Visibility = (this.textBoxConfirmationCode.Text == string.Empty ? Visibility.Visible : Visibility.Collapsed);
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
      ((UIElement) this.textBlockConfirmationCodeWatermark).Visibility = (this.textBoxConfirmationCode.Text == string.Empty ? Visibility.Visible : Visibility.Collapsed);
    }

    private void GridCallTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      (base.DataContext as RegistrationConfirmationCodeViewModel).RequestCall();
    }

    private void UpdateSource(TextBox textBox)
    {
      ((FrameworkElement) textBox).GetBindingExpression((DependencyProperty) TextBox.TextProperty).UpdateSource();
    }

    private void textBoxConfirmationCode_KeyDown(object sender, KeyEventArgs e)
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/Registration/RegistrationStep3UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBoxConfirmationCode = (TextBox) base.FindName("textBoxConfirmationCode");
      this.textBlockConfirmationCodeWatermark = (TextBlock) base.FindName("textBlockConfirmationCodeWatermark");
    }
  }
}
