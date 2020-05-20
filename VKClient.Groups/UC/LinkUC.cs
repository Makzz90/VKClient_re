using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library;

namespace VKClient.Groups.UC
{
  public class LinkUC : UserControl
  {
    private bool _contentLoaded;

    public LinkUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void ActionButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      ((LinkHeader) base.DataContext).ActionButtonAction((FrameworkElement) this);
    }

    private void ActionButton_OnPressed(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/UC/LinkUC.xaml", UriKind.Relative));
    }
  }
}
