namespace VKClient.Common.Utils
{
    public static class KeyboardHelper
    {
        private const int PortraitOrientationHeightDefault = 408;
        private const int PortraitOrientationHeight225 = 334;
        private const int LandscapeOrientationHeightDefault = 328;
        private const int LandscapeOrientationHeight225 = 268;

        public static double PortraitHeight { get; set; }

        public static double LandscapeHeight { get; set; }

        private static int PortraitDefaultHeight
        {
            get
            {
                return !DeviceStatusExtensions.IsLargeScreen ? 408 : 334;
            }
        }

        private static int LandscapeDefaultHeight
        {
            get
            {
                return !DeviceStatusExtensions.IsLargeScreen ? 328 : 268;
            }
        }

        static KeyboardHelper()
        {
            KeyboardHelper.PortraitHeight = (double)KeyboardHelper.PortraitDefaultHeight;
            KeyboardHelper.LandscapeHeight = (double)KeyboardHelper.LandscapeDefaultHeight;
        }
    }
}
