using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

using VKClient.Common.Library;
using System.Windows.Media;

namespace VKMessenger.Views
{
    public class ConversationAvatarUC : UserControl
    {
        internal Border gridConversationHeader;
        private bool _contentLoaded;

        internal double px_per_tick = 56.0 / 10.0 / 2.0;
        internal RectangleGeometry RectangleGeometry1;
        internal RectangleGeometry RectangleGeometry2;
        internal RectangleGeometry RectangleGeometry3;
        internal RectangleGeometry RectangleGeometry4;
        internal RectangleGeometry RectangleGeometry5;
        internal RectangleGeometry RectangleGeometry6;
        internal RectangleGeometry RectangleGeometry7;

        public ConversationAvatarUC()
        {
            this.InitializeComponent();
            //
            this.gridConversationHeader.CornerRadius = new CornerRadius( AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * px_per_tick );
            this.RectangleGeometry1.RadiusX = this.RectangleGeometry1.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.RectangleGeometry1.Rect.Width / 10.0 / 2.0;
            this.RectangleGeometry2.RadiusX = this.RectangleGeometry2.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.RectangleGeometry2.Rect.Width / 10.0 / 2.0;
            this.RectangleGeometry3.RadiusX = this.RectangleGeometry3.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.RectangleGeometry3.Rect.Width / 10.0 / 2.0;
            this.RectangleGeometry4.RadiusX = this.RectangleGeometry4.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.RectangleGeometry4.Rect.Width / 10.0 / 2.0;
            this.RectangleGeometry5.RadiusX = this.RectangleGeometry5.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.RectangleGeometry5.Rect.Width / 10.0 / 2.0;
            this.RectangleGeometry6.RadiusX = this.RectangleGeometry6.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.RectangleGeometry6.Rect.Width / 10.0 / 2.0;
            this.RectangleGeometry7.RadiusX = this.RectangleGeometry7.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.RectangleGeometry7.Rect.Width / 10.0 / 2.0;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/ConversationAvatarUC.xaml", UriKind.Relative));
            this.gridConversationHeader = (Border)base.FindName("gridConversationHeader");
            this.RectangleGeometry1 = (RectangleGeometry)base.FindName("RectangleGeometry1");
            this.RectangleGeometry2 = (RectangleGeometry)base.FindName("RectangleGeometry2");
            this.RectangleGeometry3 = (RectangleGeometry)base.FindName("RectangleGeometry3");
            this.RectangleGeometry4 = (RectangleGeometry)base.FindName("RectangleGeometry4");
            this.RectangleGeometry5 = (RectangleGeometry)base.FindName("RectangleGeometry5");
            this.RectangleGeometry6 = (RectangleGeometry)base.FindName("RectangleGeometry6");
            this.RectangleGeometry7 = (RectangleGeometry)base.FindName("RectangleGeometry7");
        }
    }
}
