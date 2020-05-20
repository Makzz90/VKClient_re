using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
    public class NewsfeedHeaderUC : UserControl
    {
        public static readonly DependencyProperty IsLoadingFreshNewsProperty = DependencyProperty.Register("IsLoadingFreshNews", typeof(bool), typeof(NewsfeedHeaderUC), new PropertyMetadata(new PropertyChangedCallback(NewsfeedHeaderUC.IsLoadingFreshNews_OnChanged)));
        internal Border borderFreshNews;
        internal TranslateTransform translateFreshNews;
        internal Image imageArrowFreshNews;
        internal ProgressRing2 progressRingFreshNews;
        internal GenericHeaderUC ucHeader;
        private bool _contentLoaded;

        public Action OnFreshNewsTap { get; set; }

        public bool IsLoadingFreshNews
        {
            get
            {
                return (bool)base.GetValue(NewsfeedHeaderUC.IsLoadingFreshNewsProperty);
            }
            set
            {
                base.SetValue(NewsfeedHeaderUC.IsLoadingFreshNewsProperty, value);
            }
        }

        public NewsfeedHeaderUC()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            this.translateFreshNews.Y = 0.0;
            this.borderFreshNews.Visibility = Visibility.Collapsed;
            this.UpdateFreshNewsLoadingState();
        }

        private void BorderFreshNews_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            e.Handled = true;
            Action onFreshNewsTap = this.OnFreshNewsTap;
            if (onFreshNewsTap == null)
                return;
            onFreshNewsTap();
        }

        private static void IsLoadingFreshNews_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NewsfeedHeaderUC)d).UpdateFreshNewsLoadingState();
        }

        private void UpdateFreshNewsLoadingState()
        {
            if (this.IsLoadingFreshNews)
            {
                ((UIElement)this.imageArrowFreshNews).Visibility = Visibility.Collapsed;
                ((UIElement)this.progressRingFreshNews).Visibility = Visibility.Visible;
                this.progressRingFreshNews.IsActive = true;
            }
            else
            {
                ((UIElement)this.imageArrowFreshNews).Visibility = Visibility.Visible;
                ((UIElement)this.progressRingFreshNews).Visibility = Visibility.Collapsed;
                this.progressRingFreshNews.IsActive = false;
            }
        }

        private void Header_OnOptionsMenuItemSelected(object sender, OptionsMenuItemType e)
        {
            if (e != OptionsMenuItemType.Search)
                return;
            Navigator.Current.NavigateToNewsSearch("");
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsfeedHeaderUC.xaml", UriKind.Relative));
            this.borderFreshNews = (Border)base.FindName("borderFreshNews");
            this.translateFreshNews = (TranslateTransform)base.FindName("translateFreshNews");
            this.imageArrowFreshNews = (Image)base.FindName("imageArrowFreshNews");
            this.progressRingFreshNews = (ProgressRing2)base.FindName("progressRingFreshNews");
            this.ucHeader = (GenericHeaderUC)base.FindName("ucHeader");
        }
    }
}
