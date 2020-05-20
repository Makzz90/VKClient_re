using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKMessenger.Library;

namespace VKMessenger.Views
{
  public class ConversationHeaderUCEmoji : UserControlVirtualizable
  {
    private bool _contentLoaded;

    public bool IsLookup { get; set; }

    public ConversationHeaderUCEmoji()
    {
      this.InitializeComponent();
    }

    private void MenuItemTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      DependencyObject dependencyObject = (DependencyObject) frameworkElement;
      while (!(dependencyObject is ContextMenu))
        dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
      ContextMenu contextMenu = dependencyObject as ContextMenu;
      int num = 0;
      contextMenu.IsOpen = num != 0;
      ConversationHeader dataContext1 = ((FrameworkElement) contextMenu).DataContext as ConversationHeader;
      if (!(frameworkElement.DataContext is MenuItemData))
        return;
      MenuItemData dataContext2 = frameworkElement.DataContext as MenuItemData;
      if (dataContext2.Tag == "delete" && MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.Conversation_DeleteDialog, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        ConversationsViewModel.Instance.DeleteConversation(dataContext1);
      if (dataContext2.Tag == "disableEnable")
      {
        ConversationsViewModel.Instance.SetInProgressMain(true, "");
        dataContext1.DisableEnableNotifications((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          ConversationsViewModel.Instance.SetInProgressMain(false, "");
          if (res)
            return;
          ExtendedMessageBox.ShowSafe(CommonResources.Error);
        }))));
      }
      if (!(dataContext2.Tag == "messagesFromGroup") || dataContext1 == null)
        return;
      ConversationsViewModel.Instance.SetInProgressMain(true, "");
      dataContext1.AllowDenyMessagesFromGroup((Action<bool>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        ConversationsViewModel.Instance.SetInProgressMain(false, "");
        if (res)
          return;
        ExtendedMessageBox.ShowSafe(CommonResources.Error);
      }))));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/ConversationHeaderUCEmoji.xaml", UriKind.Relative));
    }
  }
}
