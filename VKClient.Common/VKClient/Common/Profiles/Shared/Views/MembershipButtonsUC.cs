using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.Profiles.Users.ViewModels;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class MembershipButtonsUC : UserControl
  {
    internal ContextMenu contextMenu;
    private bool _contentLoaded;

    private MembershipInfoBase MembershipInfo
    {
      get
      {
        return this.DataContext as MembershipInfoBase;
      }
    }

    public MembershipButtonsUC()
    {
      this.InitializeComponent();
    }

    private void ButtonSendMessage_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.MembershipInfo == null)
        return;
      long userOrChatId = this.MembershipInfo is UserMembershipInfo ? this.MembershipInfo.Id : -this.MembershipInfo.Id;
      if (userOrChatId == 0L)
        return;
      Navigator.Current.NavigateToConversation(userOrChatId, false, false, "", 0L, false);
    }

    private void ButtonPrimary_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.MembershipInfo == null)
        return;
      if (this.MembershipInfo.SupportMultipleAddActions)
      {
        this.contextMenu.ItemsSource = (IEnumerable) this.MembershipInfo.MenuItems;
        this.contextMenu.IsOpen = true;
      }
      else
        this.MembershipInfo.Add();
    }

    private void ButtonSecondary_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.MembershipInfo == null || this.MembershipInfo.SecondaryAction == null)
        return;
      this.MembershipInfo.SecondaryAction();
    }

    private void ButtonSecondaryAction_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.contextMenu.ItemsSource = (IEnumerable) this.MembershipInfo.MenuItems;
      if (this.contextMenu.ItemsSource == null || this.contextMenu.Items.Count <= 0)
        return;
      this.contextMenu.IsOpen = true;
    }

    private void ButtonReply_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.contextMenu.ItemsSource = this.MembershipInfo != null ? (IEnumerable) this.MembershipInfo.MenuItems : null;
      if (this.contextMenu.ItemsSource != null && this.contextMenu.Items.Count > 0)
      {
        this.contextMenu.IsOpen = true;
      }
      else
      {
        if (this.MembershipInfo == null)
          return;
        this.MembershipInfo.Add();
      }
    }

    private void ButtonDecline_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.MembershipInfo.Remove();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/MembershipButtonsUC.xaml", UriKind.Relative));
      this.contextMenu = (ContextMenu) this.FindName("contextMenu");
    }
  }
}
