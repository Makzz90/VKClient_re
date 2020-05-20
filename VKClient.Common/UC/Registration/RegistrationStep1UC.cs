using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library.Registration;
using VKClient.Common.Utils;

namespace VKClient.Common.UC.Registration
{
  public class RegistrationStep1UC : UserControl
  {
    internal Grid LayoutRoot;
    internal ContextMenu PhotoMenu;
    internal TextBox textBoxFirstName;
    internal TextBlock textBlockFirstNameWatermark;
    internal TextBox textBoxLastName;
    internal TextBlock textBlockLastNameWatermark;
    private bool _contentLoaded;

    private RegistrationProfileViewModel VM
    {
      get
      {
        return base.DataContext as RegistrationProfileViewModel;
      }
    }

    public RegistrationStep1UC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.RegistrationStep1UC_Loaded));
    }

    private void RegistrationStep1UC_Loaded(object sender, RoutedEventArgs e)
    {
      ((UIElement) this.textBlockFirstNameWatermark).Visibility = (this.textBoxFirstName.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible);
      ((UIElement) this.textBlockLastNameWatermark).Visibility = (this.textBoxLastName.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible);
    }

    private void ChoosePhotoTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.DoChoosePhoto();
    }

    private void ChosePhotoMenuClick(object sender, RoutedEventArgs e)
    {
      this.DoChoosePhoto();
    }

    private void DoChoosePhoto()
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(1, true, false);
    }

    private void DeletePhotoMenuClick(object sender, RoutedEventArgs e)
    {
      this.VM.DeletePhoto();
    }

    private void GridPhotoTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.PhotoMenu.IsOpen = true;
    }

    private void textBoxFirstNameChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
      ((UIElement) this.textBlockFirstNameWatermark).Visibility = (this.textBoxFirstName.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible);
    }

    private void textBoxLastNameChanged(object sender, TextChangedEventArgs e)
    {
      this.UpdateSource(sender as TextBox);
      ((UIElement) this.textBlockLastNameWatermark).Visibility = (this.textBoxLastName.Text != string.Empty ? Visibility.Collapsed : Visibility.Visible);
    }

    private void UpdateSource(TextBox textBox)
    {
      ((FrameworkElement) textBox).GetBindingExpression((DependencyProperty) TextBox.TextProperty).UpdateSource();
    }

    private void TermsClick(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToWebUri("https://vk.com/terms", true, false);
    }

    private void PrivacyClick(object sender, RoutedEventArgs e)
    {
      Navigator.Current.NavigateToWebUri("https://vk.com/privacy", true, false);
    }

    private void textBoxFirstName_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key.IsDigit())
        e.Handled = true;
      if (e.Key != Key.Enter)
        return;
      ((Control) this.textBoxLastName).Focus();
    }

    private void textBoxLastName_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key.IsDigit())
        e.Handled = true;
      if (e.Key != Key.Enter)
        return;
      PageBase currentPage = FramePageUtils.CurrentPage;
      if (currentPage == null)
        return;
      ((Control) currentPage).Focus();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/Registration/RegistrationStep1UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.PhotoMenu = (ContextMenu) base.FindName("PhotoMenu");
      this.textBoxFirstName = (TextBox) base.FindName("textBoxFirstName");
      this.textBlockFirstNameWatermark = (TextBlock) base.FindName("textBlockFirstNameWatermark");
      this.textBoxLastName = (TextBox) base.FindName("textBoxLastName");
      this.textBlockLastNameWatermark = (TextBlock) base.FindName("textBlockLastNameWatermark");
    }
  }
}
