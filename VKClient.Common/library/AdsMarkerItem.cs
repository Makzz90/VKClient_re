using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
    public class AdsMarkerItem : VirtualizableItemBase
    {
        public override double FixedHeight
        {
            get
            {
                return 40.0;
            }
        }

        public AdsMarkerItem(double width, Thickness margin)
            : base(width, margin, new Thickness())
        {
            ((Panel)this._view).Background = ((Brush)new SolidColorBrush(Colors.Transparent));
        }

        protected override void GenerateChildren()
        {
            ImageBrush imageBrush1 = new ImageBrush();
            ImageLoader.SetImageBrushMultiResSource(imageBrush1, "/Resources/MoneyCircle24px.png");
            Border border1 = new Border();
            double num1 = 24.0;
            ((FrameworkElement)border1).Width = num1;
            double num2 = 24.0;
            ((FrameworkElement)border1).Height = num2;
            ImageBrush imageBrush2 = imageBrush1;
            ((UIElement)border1).OpacityMask = ((Brush)imageBrush2);
            SolidColorBrush solidColorBrush1 = (SolidColorBrush)Application.Current.Resources["PhoneWallPostIconBackgroundInactiveBrush"];
            border1.Background = ((Brush)solidColorBrush1);
            Border border2 = border1;
            Canvas.SetLeft((UIElement)border2, 16.0);
            Canvas.SetTop((UIElement)border2, 8.0);
            this.Children.Add((FrameworkElement)border2);
            TextBlock textBlock1 = new TextBlock();
            SolidColorBrush solidColorBrush2 = (SolidColorBrush)Application.Current.Resources["PhoneVKSubtleBrush"];
            textBlock1.Foreground = ((Brush)solidColorBrush2);
            string adsInCommunity = CommonResources.AdsInCommunity;
            textBlock1.Text = adsInCommunity;
            double num3 = 20.0;
            textBlock1.FontSize = num3;
            TextBlock textBlock2 = textBlock1;
            Canvas.SetLeft((UIElement)textBlock2, 48.0);
            Canvas.SetTop((UIElement)textBlock2, 5.0);
            this.Children.Add((FrameworkElement)textBlock2);
        }
    }
}
