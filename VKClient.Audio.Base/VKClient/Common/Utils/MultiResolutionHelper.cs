using System;

namespace VKClient.Common.Utils
{
    public class MultiResolutionHelper
    {
        public static MultiResolutionHelper Instance = new MultiResolutionHelper();

        public string Suffix
        {
            get
            {
                switch (ScaleFactor.GetScaleFactor())
                {
                    case 100:
                        return "-WVGA";
                    case 150:
                        return "-720p";
                    default:
                        return "-WXGA";
                }
            }
        }

        public string SuffixHDSupport
        {
            get
            {
                switch (ScaleFactor.GetRealScaleFactor())
                {
                    case 100:
                        return "-WVGA";
                    case 150:
                        return "-720p";
                    case 225:
                        return "-1080p";
                    default:
                        return "-WXGA";
                }
            }
        }

        public string AppendResolutionSuffix(string imageUri, bool supportFullHD = false, string predefinedSuffix = "")
        {
            int num = Math.Max(0, imageUri.Length - 4);
            string str1 = imageUri.Substring(0, num);
            string str2 = imageUri.Substring(num);
            string str3 = predefinedSuffix;
            if (predefinedSuffix == "")
                str3 = supportFullHD ? this.SuffixHDSupport : this.Suffix;
            string str4 = str3;
            string str5 = str2;
            //
            //System.Diagnostics.Debug.WriteLine(str1 + str4 + str5);
            //
            return str1 + str4 + str5;
        }
    }
}
