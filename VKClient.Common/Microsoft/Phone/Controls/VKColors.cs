using System;

namespace Microsoft.Phone.Controls
{
  public static class VKColors
  {
    public const uint Gray000 = 4294967295;
    public const uint Gray050 = 4293651952;
    public const uint Gray100 = 4292862694;
    public const uint Gray200 = 4291086284;
    public const uint Gray300 = 4289375923;
    public const uint Gray400 = 4287665305;
    public const uint Gray500 = 4285625722;
    public const uint Gray600 = 4284112225;
    public const uint Gray700 = 4282664263;
    public const uint Gray800 = 4281085230;
    public const uint Gray900 = 4279966748;
    public const uint Gray950 = 4279505940;
    public const uint Blue050 = 4283472107;
    public const uint Blue100 = 4283536608;
    public const uint Blue200 = 4283599820;
    public const uint Blue300 = 4283531704;
    public const uint Blue350 = 4282873000;
    public const uint Blue400 = 4282938019;
    public const uint Blue500 = 4282541711;
    public const uint Blue600 = 4282079354;
    public const uint Blue700 = 4281551718;
    public const uint Blue800 = 4280958290;
    public const uint Blue900 = 4280364605;
    public const uint GrayBlue050 = 4290171634;
    public const uint GrayBlue100 = 4289513445;
    public const uint GrayBlue200 = 4288131276;
    public const uint GrayBlue300 = 4286683315;
    public const uint GrayBlue400 = 4285696419;
    public const uint GrayBlue500 = 4284577423;
    public const uint GrayBlue600 = 4283524218;
    public const uint GrayBlue700 = 4282602598;
    public const uint GrayBlue800 = 4281877844;
    public const uint GrayBlue900 = 4281021760;
    public const uint Black = 4278190080;
    public const uint AlmostBlack = 4281217075;
    public const uint DarkGray = 4285165429;
    public const uint Separator = 4294111986;
    public const uint IconGray = 4289441715;
    public const uint ProfileBlueSubheader = 4286482063;
    public const uint ProfileBlueHeader = 4283587430;
    public const uint DarkThemeButtonBackground = 4281414195;
    public const uint DarkThemeCard = 4280756265;
    public const uint CardOverlay = 4294309623;
    public const uint SecondaryButtonBackground = 4293323760;
    public const uint DarkCardOverlayOverlay = 4281019438;

    public static uint WithOpacity(this uint color, int opacity)
    {
      return (uint) (((int) (uint) Math.Round((double) opacity / 100.0 * (double) byte.MaxValue, MidpointRounding.AwayFromZero) << 24) + ((int) color & 16777215));
    }
  }
}
