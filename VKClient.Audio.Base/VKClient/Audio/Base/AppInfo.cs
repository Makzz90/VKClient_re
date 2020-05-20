using Microsoft.Phone.Info;
using System;
using System.Threading;
using VKClient.Common.Backend;
using Windows.ApplicationModel;

namespace VKClient.Audio.Base
{
    public static class AppInfo
    {
        private static string _version;

        public static string Version
        {
            get
            {
                return AppInfo.GetVersion();
            }
        }

        public static string AppVersionForUserAgent
        {
            get { return string.Format(string.Format("com.vk.wp_app/{0} ({1}, {2})", AppInfo.Version.Replace(".", "").PadRight(3, '0'), AppInfo.OSTypeAndVersion, AppInfo.Device)); }
        }

        public static string AppVersionForPushes
        {
            get
            {
                string[] strArray = AppInfo.Version.Split('.');
                return strArray[0] + (strArray[1].Length == 1 ? "0" : "") + strArray[1] + (strArray.Length == 2 ? "00" : (strArray[2].Length == 1 ? "0" : "") + strArray[2]);
            }
        }

        public static string OSTypeAndVersion
        {
            get { return string.Format("WindowsPhone_{0}", Environment.OSVersion.Version); }
        }

        public static string OSVersion
        {
            get
            {
                return Environment.OSVersion.Version.ToString();
            }
        }

        public static string Device
        {
            get
            {
                return DeviceStatus.DeviceManufacturer + "__" + DeviceStatus.DeviceName;
            }
        }

        public static string DeviceViewable
        {
            get
            {
                return DeviceStatus.DeviceManufacturer + " " + DeviceStatus.DeviceName;
            }
        }

        public static string Locale
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.ToString();
            }
        }

        private static string GetVersion()
        {
            if (!string.IsNullOrEmpty(AppInfo._version))
                return AppInfo._version;
            PackageVersion version = Package.Current.Id.Version;
            AppInfo._version = string.Format("{0}.{1}", version.Major, version.Minor);
            if (version.Build > 0)
                AppInfo._version += string.Format(".{0}", version.Build);
            return AppInfo._version;
        }

        public static PhoneAppInfo GetPhoneAppInfo()
        {
            return new PhoneAppInfo()
            {
                AppVersion = AppInfo.Version,
                Device = AppInfo.Device,
                OS = AppInfo.OSTypeAndVersion,
                Locale = AppInfo.Locale
            };
        }
    }
}
