using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Library;

namespace VKClient.Common
{
    public static class VKConstants
    {
        public static readonly string API_VERSION = "5.64";//todo: bug? 5.63
        public static readonly string UsersWithSyncedContactsKey = "SettingsKey_UsersWithSyncedContacts";
        public static readonly int SerializationVersion = 1;
        public static readonly string ApplicationID = "3502561";
        public static readonly string ApplicationSecretKey = "omvP3y2MZmpREFZJDNHd";
        public static readonly string HttpPushNotificationName = "VKMessengerPushTileNotification";
        public static readonly int DefaultOtherUsersPageCount = 20;
        public static readonly int DefaultFriendRequestPageCount = 20;
        public static readonly int DefaultSearchMessagesCount = 20;
        //public static readonly string SupportEmail = "support.vkmessenger@windowslive.com";
        public static readonly double LineHeight = 23.0;
        public static readonly string GrayColorHex = "#FF878787";
        public static readonly string LightGrayColorHex = "#FF8C8C8C";
        public static readonly string ForegroundWhiteTheme = "#ff51647a";
        public static readonly int VideosReadCount = 20;
        public static readonly int AlbumsReadCount = 20;
        public static readonly int TracksReadCount = 20;
        public static readonly int MAX_PHOTO_WIDTH = 2560;
        public static readonly int MAX_PHOTO_HEIGHT = 2048;
        public static int[] MOBILE_ONLINE_TYPES = new int[5] { 1, 2, 3, 4, 5 };
        public static readonly int JPEGQUALITY = 90;
        public static double LoadMoreNewsThreshold = 10000.0;
        public static readonly double GenericWidth = 448.0;
        public static readonly int MaxChatCount = 50;
        public static readonly List<string> SupportedImageExtensions = new List<string>() { ".JPG", ".JPE", ".BMP", ".PNG" };
        public static readonly List<string> SupportedVideoExtensions = new List<string>()
    {
      ".mp4",
      ".mov",
      ".wmv",
      ".3gp",
      ".3g2"
    };
        public static readonly List<string> SupportedAudioExtensions = new List<string>()
    {
      ".mp3"
    };
        public static readonly List<string> SupportedDocLibraryExtensions = new List<string>()
    {
      ".mp4",
      ".mov",
      ".wmv",
      ".3gp",
      ".3g2",
      ".jpg",
      ".png",
      ".gif"
    };
        public static readonly List<string> SupportedDocExtensions = new List<string>()
    {
      "*"
    };
        public static readonly double DefaultTilt = 2.5;
        public const int MAX_RECENT_STICKERS_COUNT = 32;
        public const int ONLINE_TYPE_DESKTOP = 7;
        public const int ONLINE_TYPE_MOBILE = 1;
        public const int ONLINE_TYPE_IPHONE = 2;
        public const int ONLINE_TYPE_IPAD = 3;
        public const int ONLINE_TYPE_ANDROID = 4;
        public const int ONLINE_TYPE_WINPHONE = 5;
        public const int ONLINE_TYPE_WINDOWS8 = 6;
        public const double GENERIC_WIDTH_FULL = 480.0;
        public const double GENERIC_MARGIN = 16.0;
        public const double AppBarOpacity = 0.9;
        public const long MAX_MESSAGES_GROUP_ID = 2000000000;
        public const string AlbumCameraRoll = "Camera Roll";
        public const string AlbumSavedPictures = "Saved Pictures";
        public const string AlbumSamplePictures = "Sample Pictures";
        public const string AlbumScreenshots = "Screenshots";

        public static int ResizedImageSize
        {
            get { return !AppGlobalStateManager.Current.GlobalState.CompressPhotosOnUpload ? 10000000 : 1000000; }
        }

        public static Color AppBarBGColor
        {
            get { return ((SolidColorBrush)Application.Current.Resources["PhoneAppBarBackgroundBrush"]).Color; }
        }

        public static Color AppBarFGColor
        {
            get { return ((SolidColorBrush)Application.Current.Resources["PhoneAppBarForegroundBrush"]).Color; }
        }

        public static Color VKColor
        {
            get { return VKConstants.GetColorFromString("#FF406894"); }
        }

        public static double MarginBetweenElements
        {
            get { return 12.0; }
        }

        public static double MarginBetweenNewsItems
        {
            get { return 36.0; }
        }

        public static Color GetColorFromString(string colorHex)
        {
            return Color.FromArgb(Convert.ToByte(colorHex.Substring(1, 2), 16), Convert.ToByte(colorHex.Substring(3, 2), 16), Convert.ToByte(colorHex.Substring(5, 2), 16), Convert.ToByte(colorHex.Substring(7, 2), 16));
        }
    }
}
