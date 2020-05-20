using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Groups.UC
{
    public class ServiceOptionUC : UserControl
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(ServiceOptionUC), new PropertyMetadata("", new PropertyChangedCallback(ServiceOptionUC.IconPropertyChangedCallback)));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ServiceOptionUC), new PropertyMetadata("", new PropertyChangedCallback(ServiceOptionUC.TitlePropertyChangedCallback)));
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(string), typeof(ServiceOptionUC), new PropertyMetadata("", new PropertyChangedCallback(ServiceOptionUC.StatePropertyChangedCallback)));
        internal Border IconBorder;
        internal TextBlock TitleBlock;
        internal TextBlock StateBlock;
        private bool _contentLoaded;

        public string Icon
        {
            get
            {
                return (string)base.GetValue(ServiceOptionUC.IconProperty);
            }
            set
            {
                base.SetValue(ServiceOptionUC.IconProperty, value);
            }
        }

        public string Title
        {
            get
            {
                return (string)base.GetValue(ServiceOptionUC.TitleProperty);
            }
            set
            {
                base.SetValue(ServiceOptionUC.TitleProperty, value);
            }
        }

        public string State
        {
            get
            {
                return (string)base.GetValue(ServiceOptionUC.StateProperty);
            }
            set
            {
                base.SetValue(ServiceOptionUC.StateProperty, value);
            }
        }

        public ServiceOptionUC()
        {
            //base.\u002Ector();
            this.InitializeComponent();
        }

        private static void IconPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ServiceOptionUC serviceOptionUc = (ServiceOptionUC)sender;
            ImageBrush imageBrush = new ImageBrush();
            // ISSUE: explicit reference operation
            ImageLoader.SetImageBrushMultiResSource(imageBrush, (string)e.NewValue);
            ((UIElement)serviceOptionUc.IconBorder).OpacityMask = ((Brush)imageBrush);
        }

        private static void TitlePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // ISSUE: explicit reference operation
            ((ServiceOptionUC)sender).TitleBlock.Text = ((string)e.NewValue);
        }

        private static void StatePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // ISSUE: explicit reference operation
            ((ServiceOptionUC)sender).StateBlock.Text = ((string)e.NewValue);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Groups;component/UC/ServiceOptionUC.xaml", UriKind.Relative));
            this.IconBorder = (Border)base.FindName("IconBorder");
            this.TitleBlock = (TextBlock)base.FindName("TitleBlock");
            this.StateBlock = (TextBlock)base.FindName("StateBlock");
        }
    }
}
