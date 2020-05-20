using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class InvitationItemHeaderUC : UserControl
  {
    private bool _contentLoaded;

    public InvitationItemHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void Invite_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      InvitationItemHeader dataContext = ((FrameworkElement) sender).DataContext as InvitationItemHeader;
      if (dataContext == null || dataContext.InviteTapFunc == null)
        return;
      dataContext.InviteTapFunc();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/InvitationItemHeaderUC.xaml", UriKind.Relative));
    }
  }
}
