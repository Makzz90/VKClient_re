using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Framework
{
    public class ThemeHelper
    {
        public Visibility PhoneDarkThemeVisibility
        {
            get
            {
                return (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"];
            }
        }

        public Visibility PhoneLightThemeVisibility
        {
            get
            {
                return (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"];
            }
        }

        public SolidColorBrush InvertedOpaqueBG
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources["PhoneMenuBackgroundBrush"];
            }
        }

        public SolidColorBrush InvertedOpaqueFG
        {
            get
            {
                return (SolidColorBrush)Application.Current.Resources["PhoneMenuForegroundBrush"];
            }
        }
    }
}
