using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Framework;
//
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace VKClient.Common.UC
{
    public class PullToRefreshUC : UserControl
    {
        private double _previousP = -1.0;
        internal Rectangle rectProgress;
        private bool _contentLoaded;

        public Brush ForegroundBrush
        {
            get
            {
                return this.rectProgress.Fill;
            }
            set
            {
                this.rectProgress.Fill = value;
            }
        }

        public PullToRefreshUC()
        {
            //base.\u002Ector();
            this.InitializeComponent();
        }

        public void TrackListBox(ISupportPullToRefresh lb)
        {
            lb.OnPullPercentageChanged = (Action)(() => this.OnPullPercentageChanged(lb));
            this.Update(0.0);
        }

        private void OnPullPercentageChanged(ISupportPullToRefresh lb)
        {
            this.Update(lb.PullPercentage);
        }

        private void Update(double p)
        {
            if (this._previousP == p)
                return;
            (this.rectProgress.RenderTransform as ScaleTransform).ScaleX = (1.0 + p / 100.0);
            this.rectProgress.Opacity = ((p + 50.0) * 0.667 / 100.0);
            this.rectProgress.Visibility = (p > 0.0 ? Visibility.Visible : Visibility.Collapsed);
            //from 4.7
            try
            {
                if (VKClient.Common.Library.AppGlobalStateManager.Current.GlobalState.HideSystemTray == true)
                {
                    if (Application.Current.RootVisual is PhoneApplicationFrame)
                    {
                        if ((Application.Current.RootVisual as PhoneApplicationFrame).Content is PhoneApplicationPage)
                            SystemTray.IsVisible = this.rectProgress.Visibility != Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.Instance.Error("PullToRefreshUC Failed to set systemtray visibility", ex);
            }
            //
            this._previousP = p;
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/PullToRefreshUC.xaml", UriKind.Relative));
            this.rectProgress = (Rectangle)base.FindName("rectProgress");
        }
    }
}
