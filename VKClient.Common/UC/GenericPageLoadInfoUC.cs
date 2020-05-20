using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
    public class GenericPageLoadInfoUC : UserControl
    {
        public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register("BackgroundBrush", typeof(Brush), typeof(GenericPageLoadInfoUC), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((GenericPageLoadInfoUC)d).UpdateBackground())));
        public static readonly DependencyProperty IsTernaryButtonProperty = DependencyProperty.Register("IsPrimaryButton", typeof(bool), typeof(GenericPageLoadInfoUC), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((GenericPageLoadInfoUC)d).UpdateIsPrimaryButton())));
        internal Grid gridRoot;
        internal Button buttonRetry;
        private bool _contentLoaded;

        public Brush BackgroundBrush
        {
            get
            {
                return (Brush)base.GetValue(GenericPageLoadInfoUC.BackgroundBrushProperty);
            }
            set
            {
                base.SetValue(GenericPageLoadInfoUC.BackgroundBrushProperty, value);
            }
        }

        public bool IsTernaryButton
        {
            get
            {
                return (bool)base.GetValue(GenericPageLoadInfoUC.IsTernaryButtonProperty);
            }
            set
            {
                base.SetValue(GenericPageLoadInfoUC.IsTernaryButtonProperty, value);
            }
        }

        public GenericPageLoadInfoUC()
        {
            //base.\u002Ector();
            this.InitializeComponent();
        }

        private void UpdateBackground()
        {
            ((Panel)this.gridRoot).Background = this.BackgroundBrush;
        }

        private void UpdateIsPrimaryButton()
        {
            this.buttonRetry.Style = (this.IsTernaryButton ? (Style)Application.Current.Resources["VKButtonTernaryStyle"] : (Style)Application.Current.Resources["VKButtonSecondaryStyle"]);
        }

        private void ButtonRetry_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModelStatefulBase dataContext = base.DataContext as ViewModelStatefulBase;
            if (dataContext == null)
                return;
            dataContext.Reload(true);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GenericPageLoadInfoUC.xaml", UriKind.Relative));
            this.gridRoot = (Grid)base.FindName("gridRoot");
            this.buttonRetry = (Button)base.FindName("buttonRetry");
        }
    }
}
