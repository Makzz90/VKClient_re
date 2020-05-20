using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
//
using VKClient.Common.Library;
using System.Windows.Media;

namespace VKClient.Common.Profiles.Groups.Views
{
    public class ProfileInfoHeaderUC : UserControl
    {
        internal Rectangle rectBackground;
        private bool _contentLoaded;
        //
        internal RectangleGeometry rectangleGeometry;

        public event EventHandler<System.Windows.Input.GestureEventArgs> PhotoTapped;

        public ProfileInfoHeaderUC()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            //
            this.rectangleGeometry.RadiusX = this.rectangleGeometry.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangleGeometry.Rect.Width / 10.0 / 2.0;
        }

        public void SetOverlayOpacity(double opacity)
        {
            this.rectBackground.Opacity = opacity;
        }

        private void GridPhoto_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // ISSUE: reference to a compiler-generated field
            EventHandler<System.Windows.Input.GestureEventArgs> photoTapped = this.PhotoTapped;
            if (photoTapped == null)
                return;
            photoTapped(sender, e);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Groups/Views/ProfileInfoHeaderUC.xaml", UriKind.Relative));
            this.rectBackground = (Rectangle)base.FindName("rectBackground");
            //
            this.rectangleGeometry = (RectangleGeometry)base.FindName("rectangleGeometry");
        }
    }
}
