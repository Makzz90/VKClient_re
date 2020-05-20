using System;
using System.Windows;
using VKClient.Common.Localization;
using Windows.System;

namespace VKClient.Common.Framework
{
    public static class GeolocationHelper
    {
        public static async void HandleDisabledLocationSettings()
        {
            try
            {
                if (MessageBox.Show(CommonResources.GeolocationDisabled_Message, CommonResources.GeolocationDisabled_Title, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                    return;
                await Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
            }
            catch
            {
            }
        }
    }
}
