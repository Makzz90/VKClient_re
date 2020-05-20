using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Utils;

namespace VKClient.Common.UC.Registration
{
  public class RegistrationStep4UC : UserControl
  {
    internal Grid LayoutRoot;
    internal PasswordBox passwordBox;
    internal TextBlock textBlockWatermark;
    private bool _contentLoaded;

    public RegistrationStep4UC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.RegistrationStep4UC_Loaded));
    }

    private void RegistrationStep4UC_Loaded(object sender, RoutedEventArgs e)
    {
      ((UIElement) this.textBlockWatermark).Visibility = (this.passwordBox.Password == string.Empty ? Visibility.Visible : Visibility.Collapsed);
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
      this.UpdateSource(sender as PasswordBox);
      ((UIElement) this.textBlockWatermark).Visibility = (this.passwordBox.Password == string.Empty ? Visibility.Visible : Visibility.Collapsed);
    }

    private void UpdateSource(PasswordBox textBox)
    {
      ((FrameworkElement) textBox).GetBindingExpression((DependencyProperty) PasswordBox.PasswordProperty).UpdateSource();
    }

    private void passwordBox_KeyDown(object sender, KeyEventArgs e)
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/Registration/RegistrationStep4UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.passwordBox = (PasswordBox) base.FindName("passwordBox");
      this.textBlockWatermark = (TextBlock) base.FindName("textBlockWatermark");
    }
  }
}
