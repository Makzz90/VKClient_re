using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Groups.Management.Information.Library;

namespace VKClient.Groups.Management.Information.UC
{
  public class EventOrganizerUC : UserControl
  {
    private bool _contentLoaded;

    public EventOrganizerViewModel ViewModel
    {
      get
      {
        return base.DataContext as EventOrganizerViewModel;
      }
    }

    public EventOrganizerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void SetContacts_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this.ViewModel.ParentViewModel.IsFormEnabled)
        return;
      this.ViewModel.ContactsFieldsVisibility = Visibility.Visible;
    }

    private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      ((Control) this).Focus();
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((FrameworkElement) sender).GetBindingExpression((DependencyProperty) TextBox.TextProperty).UpdateSource();
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      RoutedEventHandler onTextBoxGotFocus = this.ViewModel.ParentViewModel.OnTextBoxGotFocus;
      if (onTextBoxGotFocus == null)
        return;
      object obj = sender;
      RoutedEventArgs routedEventArgs = e;
      onTextBoxGotFocus.Invoke(obj, routedEventArgs);
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      RoutedEventHandler textBoxLostFocus = this.ViewModel.ParentViewModel.OnTextBoxLostFocus;
      if (textBoxLostFocus == null)
        return;
      object obj = sender;
      RoutedEventArgs routedEventArgs = e;
      textBoxLostFocus.Invoke(obj, routedEventArgs);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/Information/UC/EventOrganizerUC.xaml", UriKind.Relative));
    }
  }
}
