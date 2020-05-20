using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;

using System.Windows.Shapes;
using System.Windows.Media;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
    public class NewsfeedNewPostUC : UserControl
    {
        private bool _contentLoaded;
        //
        internal Rectangle rectangle;
        internal RectangleGeometry RectangleGeometry1;

        public NewsfeedNewPostUC()
        {
            this.InitializeComponent();

            //
            this.rectangle.RadiusX = this.rectangle.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.rectangle.Width / 10.0 / 2.0;
            this.RectangleGeometry1.RadiusX = this.RectangleGeometry1.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * this.RectangleGeometry1.Rect.Width / 10.0 / 2.0;
        }

        private void NewPost_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Navigator.Current.NavigateToNewWallPost(0, false, 0, false, false, false);
        }

        private void Photo_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ParametersRepository.SetParameterForId("GoPickImage", true);
            Navigator.Current.NavigateToNewWallPost(0, false, 0, false, false, false);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsfeedNewPostUC.xaml", UriKind.Relative));
            //
            this.rectangle = (Rectangle)base.FindName("rectangle");
            this.RectangleGeometry1 = (RectangleGeometry)base.FindName("RectangleGeometry1");
        }
    }
}
