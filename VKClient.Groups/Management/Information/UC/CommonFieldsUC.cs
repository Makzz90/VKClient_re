using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Groups.Management.Information.Library;
using VKClient.Groups.UC;

namespace VKClient.Groups.Management.Information.UC
{
  public class CommonFieldsUC : UserControl
  {
    private bool _contentLoaded;

    public CommonFieldsViewModel ViewModel
    {
      get
      {
        return base.DataContext as CommonFieldsViewModel;
      }
    }

    public CommonFieldsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void DomainTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((FrameworkElement) sender).GetBindingExpression(CommunityDomainTextBox.TextProperty).UpdateSource();
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((FrameworkElement) sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      base.Focus();
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
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/Information/UC/CommonFieldsUC.xaml", UriKind.Relative));
    }
  }
}
