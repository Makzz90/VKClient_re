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
//
using VKClient.Common.Library;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace VKMessenger.Views
{
    public class ConversationHeaderUC : UserControlVirtualizable
    {
        private bool _contentLoaded;
        //
        internal Border border;
        internal RectangleGeometry rectangleGeometry1;
        internal RectangleGeometry rectangleGeometry2;
        internal RectangleGeometry rectangleGeometry3;
        internal RectangleGeometry rectangleGeometry4;
        internal RectangleGeometry rectangleGeometry5;
        internal RectangleGeometry rectangleGeometry6;
        internal RectangleGeometry rectangleGeometry7;
        internal RectangleGeometry rectangleGeometry8;
        internal Rectangle rectangle;

        public bool IsLookup { get; set; }

        public ConversationHeaderUC()
        {
            this.InitializeComponent();
            //
            this.border.CornerRadius = new CornerRadius(AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.border.Width / 10.0 / 2.0);
            this.rectangleGeometry1.RadiusX = this.rectangleGeometry1.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry1.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry2.RadiusX = this.rectangleGeometry2.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry2.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry3.RadiusX = this.rectangleGeometry3.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry3.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry4.RadiusX = this.rectangleGeometry4.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry4.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry5.RadiusX = this.rectangleGeometry5.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry5.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry6.RadiusX = this.rectangleGeometry6.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry6.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry7.RadiusX = this.rectangleGeometry7.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry7.Rect.Width / 10.0 / 2.0;
            this.rectangleGeometry8.RadiusX = this.rectangleGeometry8.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry8.Rect.Width / 10.0 / 2.0;
            this.rectangle.RadiusX = this.rectangle.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangle.Width / 10.0 / 2.0;
        }

        private void MenuItemTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            DependencyObject dependencyObject = (DependencyObject)frameworkElement;
            while (!(dependencyObject is ContextMenu))
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            ContextMenu contextMenu = dependencyObject as ContextMenu;
            int num = 0;
            contextMenu.IsOpen = num != 0;
            ConversationHeader dataContext1 = ((FrameworkElement)contextMenu).DataContext as ConversationHeader;
            if (!(frameworkElement.DataContext is MenuItemData))
                return;
            MenuItemData dataContext2 = frameworkElement.DataContext as MenuItemData;
            if (dataContext2.Tag == "delete" && MessageBox.Show(CommonResources.Conversation_ConfirmDeletion, CommonResources.Conversation_DeleteDialog, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                ConversationsViewModel.Instance.DeleteConversation(dataContext1);
            if (dataContext2.Tag == "disableEnable")
            {
                ConversationsViewModel.Instance.SetInProgressMain(true, "");
                dataContext1.DisableEnableNotifications((Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
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
            dataContext1.AllowDenyMessagesFromGroup((Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
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
            Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/ConversationHeaderUC.xaml", UriKind.Relative));
            //
            this.border = (Border)base.FindName("border");
            this.rectangleGeometry1 = (RectangleGeometry)base.FindName("rectangleGeometry1");
            this.rectangleGeometry2 = (RectangleGeometry)base.FindName("rectangleGeometry2");
            this.rectangleGeometry3 = (RectangleGeometry)base.FindName("rectangleGeometry3");
            this.rectangleGeometry4 = (RectangleGeometry)base.FindName("rectangleGeometry4");
            this.rectangleGeometry5 = (RectangleGeometry)base.FindName("rectangleGeometry5");
            this.rectangleGeometry6 = (RectangleGeometry)base.FindName("rectangleGeometry6");
            this.rectangleGeometry7 = (RectangleGeometry)base.FindName("rectangleGeometry7");
            this.rectangleGeometry8 = (RectangleGeometry)base.FindName("rectangleGeometry8");
            this.rectangle = (Rectangle)base.FindName("rectangle");
        }
    }
}
