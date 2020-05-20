using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Utils;

namespace Microsoft.Phone.Controls
{
  public static class ThemeManager
  {
    private static readonly Color AlmostWhite = Color.FromArgb(byte.MaxValue, (byte) 254, (byte) 254, (byte) 254);
    private static Color _chrome;
    private static Color _background;
    private static Color _foreground;
    private static bool _applied;
    private static Theme _themeAtStartup;

    public static ThemeManagerOverrideOptions OverrideOptions { get; set; }

    static ThemeManager()
    {
      ThemeManager.OverrideOptions = ThemeManagerOverrideOptions.SystemTrayAndApplicationBars;
    }

    public static void MatchOverriddenTheme(this IApplicationBar bar)
    {
      if (bar == null || !ThemeManager._applied)
        return;
      bar.BackgroundColor = ThemeManager._chrome;
      bar.ForegroundColor = ThemeManager._foreground;
    }

    public static ApplicationBar CreateApplicationBar()
    {
      ApplicationBar applicationBar = new ApplicationBar();
      ((IApplicationBar) applicationBar).MatchOverriddenTheme();
      return applicationBar;
    }

    public static void OverrideTheme(Theme theme)
    {
      bool flag = ThemeManager.IsThemeAlready(theme);
      if (flag)
      {
        ThemeManager._themeAtStartup = theme;
      }
      else
      {
        ThemeManager._themeAtStartup = theme == Theme.Dark ? Theme.Light : Theme.Dark;
        ThemeManager._applied = true;
      }
      new ThemeManager.RuntimeThemeResources().Apply(theme, !flag);
    }

    private static bool IsThemeAlready(Theme theme)
    {
      return (double) Application.Current.Resources["PhoneDarkThemeOpacity"] == (theme == Theme.Dark ? 1.0 : 0.0);
    }

    public static void ToLightTheme()
    {
      ThemeManager.OverrideTheme(Theme.Light);
    }

    public static void ToDarkTheme()
    {
      ThemeManager.OverrideTheme(Theme.Dark);
    }

    public static void SetAccentColor(uint color)
    {
      ThemeManager.SetAccentColor(ThemeManager.RuntimeThemeResources.DualColorValue.ToColor(color));
    }

    public static void SetAccentColor(Color color)
    {
      ThemeManager.RuntimeThemeResources.DualColorValue.SetColorAndBrush("PhoneAccent", color);
      if (Environment.OSVersion.Version.Major != 8)
        return;
      ThemeManager.RuntimeThemeResources.DualColorValue.SetColorAndBrush("PhoneTextBoxEditBorder", color);
    }

    public static void SetAccentColor(AccentColor accentColor)
    {
      ThemeManager.SetAccentColor(ThemeManager.AccentColorEnumToColorValue(accentColor));
    }

    private static uint AccentColorEnumToColorValue(AccentColor accent)
    {
      switch (accent)
      {
        case AccentColor.Brown:
          return 4288696320;
        case AccentColor.Green:
          return 4281571635;
        case AccentColor.Pink:
          return 4293292472;
        case AccentColor.Purple:
          return 4288807167;
        case AccentColor.Red:
          return 4293202944;
        case AccentColor.Teal:
          return 4278234025;
        case AccentColor.Lime:
          return 4288856377;
        case AccentColor.Magenta:
          return 4292345971;
        case AccentColor.Mango:
          return 4293957129;
        case AccentColor.NokiaBlue:
          return 4279271645;
        case AccentColor.Gray:
          return 4283124555;
        case AccentColor.OrangeUK:
          return 4294402314;
        case AccentColor.O2Blue:
          return 4281446369;
        default:
          return 4280000994;
      }
    }

    private class RuntimeThemeResources
    {
      private List<ThemeManager.RuntimeThemeResources.ThemeValue> _values;

      public RuntimeThemeResources()
      {
        this._values = new List<ThemeManager.RuntimeThemeResources.ThemeValue>()
        {
          new ThemeManager.RuntimeThemeResources.ThemeValue("Background", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("Foreground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4278190080U.WithOpacity(87))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("Chrome", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281085230U, 4292862694U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("Foreground2", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4278190080U.WithOpacity(87))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("Accent", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283991480U, 4283991480U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("RadioCheckBoxPressed", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283991480U, 4283991480U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ContrastForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ContrastBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4278190080U.WithOpacity(87))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("Disabled", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U.WithOpacity(40), 4287336340U.WithOpacity(40))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("Subtle", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(60), 4278190080U.WithOpacity(40))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextCaret", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4278190080U.WithOpacity(87))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBox", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4278190080U.WithOpacity(87))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxEditBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxReadOnly", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4293980400U, 4293980400U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxEditBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283531704U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("RadioCheckBox", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(75), 4278190080U.WithOpacity(15))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("RadioCheckBoxDisabled", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(40), 4278190080U.WithOpacity(0))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("RadioCheckBoxCheck", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4287336340U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("RadioCheckBoxCheckDisabled", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U.WithOpacity(40), 4278190080U.WithOpacity(30))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("RadioCheckBoxPressedBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4278190080U.WithOpacity(87))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("Semitransparent", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U.WithOpacity(50), uint.MaxValue.WithOpacity(50))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("Inactive", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(20), 4278190080U.WithOpacity(20))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("InverseInactive", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4291611852U, 4293256677U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("InverseBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4292730333U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("Border", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(75), 4278190080U.WithOpacity(60))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DarkThemeVisibility", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualValue<Visibility>(Visibility.Visible, Visibility.Collapsed)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("LightThemeVisibility", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualValue<Visibility>(Visibility.Collapsed, Visibility.Visible)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DarkThemeOpacity", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualValue<double>(1.0, 0.0)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("LightThemeOpacity", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualValue<double>(0.0, 1.0)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SidebarBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4281877844U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SidebarAppBarBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4280427558U, 4281087554U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SidebarSubtle", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4287337630U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SidebarCounterBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279835420U, 4284772218U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogsUnreadBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4280559145U, 4293784311U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogsUnreadBadgeBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283587430U, 4287339437U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogsTitleForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4278190080U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogsTextForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287731097U, 4287336340U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogsTextUnreadForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287468441U, 4285889410U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogsDivider", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4292336864U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("HeaderMenuBadgeBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4292864242U, 4293259775U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("NameBlue", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289513445U, 4283005101U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("VKSubtle", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4287336340U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("NewsDivider", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279176975U, 4293651952U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ActiveAreaBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4280756008U, 4293980400U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ContrastTitle", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4281217075U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("OnlineIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287337630U, 4287340211U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("OnlineIconWallPost", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283587430U, 4290169310U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("PhotoHeaderBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4281877844U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SystemTrayForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4294967294U, 4291286244U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("NewsDivider", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4293651952U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("GreyIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284901483U, 4290033336U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("NewsActionForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4286678410U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ActiveIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4285108671U, 4285178078U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("NewsActionLikedForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286031321U, 4282878678U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AppBarBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279769370U.WithOpacity(90), uint.MaxValue.WithOpacity(90))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AppBarForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4288919244U, 4284313475U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SmallBlueIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284901483U, 4288591569U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("NewsBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279966748U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("RequestOrInvitationBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TableSeparator", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4294111986U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogOutMessageBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281747033U, 4292207602U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogInMessageBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4280888883U, 4293454315U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogGiftMessageBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283583809U, 4294306771U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogGiftMessageForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4286213200U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogGiftCaptionIconBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(40), 4290951071U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogGiftCaptionForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(40), 4288581490U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DialogGiftForwardedCaptionForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(1728053247U, 1711276032U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WatermarkTextForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283453778U, 4289836989U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MenuBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4280559145U, 4292994536U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MenuForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4281217075U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("GreyDivider", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281217075U, 4294111986U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("GrayTextOverlayForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4287336340U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AudioPlayerForeground1", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4283524218U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AudioPlayerForeground2", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4285694095U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AudioPlayerBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279966748U, 4292994536U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AudioPlayerSliderBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281677624U, 4290823628U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MiniPlayerBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279966492U, 4281350983U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SidebarSelectedIconBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284314767U, 4283991480U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SidebarIconBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287995302U, 4291152593U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("PollSliderBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4280756008U, 4293520882U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("PollSliderForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281877844U, 4291812069U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("PollSliderTextForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284973470U, 4283724953U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AudioPlayerArtworkBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281085230U, 4292205022U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AudioPlayerArtworkForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282072125U, 4291547350U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MenuImagePlaceholderBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4280558886U, 4282865254U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("CameraIconBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283587430U, 4289246143U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("VerifiedIconBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284375654U, 4288261055U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SettingsIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287995302U, 4289441715U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ReplyUserBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279966748U, 4294111986U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ReplyUserForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4283005101U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ReplyUserIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(40), 4283005101U.WithOpacity(40))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("VKCheckBoxBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4290033336U, 4291086540U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("VKCheckBoxForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282873000U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("VKCheckboxBackgroundHover", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4290033336U.WithOpacity(40), 4291086540U.WithOpacity(40))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("VKCheckboxBorderDisabled", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284900966U, 4293256677U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AddFriendIconButtonBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279966748U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AddFriendIconButtonForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286031321U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AddedFriendIconButtonForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282993997U, 4289508799U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AddFriendVerifiedIconBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283656320U, 4285108671U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SeparatorBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279966748U, 4294111986U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxDefaultBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4291086540U, 4278190080U.WithOpacity(0))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxDefaultBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4291086540U, 4291086540U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxDefaultFocusedBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286031321U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxDefaultFocusedBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4278190080U.WithOpacity(0))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxDefaultCaret", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4291086540U, 4283002777U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchFocusedBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286031321U, 4283002777U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchFocusedBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4283002777U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchCaret", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchWatermarkForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4287801538U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchMenuForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchMenuForegroundFocused", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchMenuBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(10), uint.MaxValue.WithOpacity(10))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchMenuFocusedBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286031321U, uint.MaxValue.WithOpacity(10))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchMenuFocusedBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, uint.MaxValue.WithOpacity(10))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchMenuCaret", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxSearchMenuWatermarkForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4287337630U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WelcomePageLogoForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281221721U, 4282806179U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("LoginPageLogoForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281221721U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WelcomePageForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4290564843U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("LoginPageForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4283005101U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WelcomePageButtonBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286031321U, 4278190080U.WithOpacity(0))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SignUpButtonBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U.WithOpacity(0), 4292995053U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("SignUpButtonForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286031321U, 4283991480U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("LogInButtonBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U.WithOpacity(0), 4283463853U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("LogInButtonForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286031321U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("RatingFilledBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4287336079U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("RatingUnfilledBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(40), 4292598747U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ProfilePhotoPlaceholder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281875261U, 4290035660U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ProfileDeactivatedIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284375654U, 4287799213U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ProfileInfoLoadingPlaceholder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279966748U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ProfileInfoLoadingForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287862425U, 4287665305U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ListItemAccentForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289513445U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ListItemForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287862425U, 4285165429U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ProfileBlueHeader", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4283587430U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ProfileBlueSubheader", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(60), 4286482063U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ProfileBlueIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284835947U, 4288981939U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ProfilePostsToggleActive", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289513445U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ProfilePostsToggleInactive", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286151808U, 4287665305U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ProfilePostsToggleSearch", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4285757050U, 4289441715U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("VerifiedIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4288591569U, 4286162644U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("FriendRecommendationBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281085230U, 4294309623U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AccentBlue", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289513445U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DarkBlue", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289513445U, 4282873000U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AlmostBlack", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4281217075U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("DarkGray", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287072652U, 4285165429U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("CaptionGray", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287072652U, 4287665305U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("PageBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4293980658U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("CardBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4280756265U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("HeaderBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("Separator", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(10), 4278190080U.WithOpacity(10))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("IconGray", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289441715U, 4289441715U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("GenericPlaceholderBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4280756265U, 4293520882U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("GenericPlaceholderForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282598727U, 4290561745U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AttachIconBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281744192U, 4291086540U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("GenericBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4290033336U, 4291086540U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("CardOverlay", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281085230U, 4294309623U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AttachmentBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(25), 4278193178U.WithOpacity(16))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WallPostIconBackgroundInactive", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284112225U, 4290823628U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WallPostIconCounterForegroundInactive", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4285625722U, 4287534750U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WallPostIconBackgroundActive", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4285769958U, 4284257233U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WallPostIconCounterForegroundActive", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287280614U, 4283136947U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WallPostLikesSeparator", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue.WithOpacity(10), 4278190080U.WithOpacity(10))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WallPostActivityBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281085230U, 4294309623U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("WallPostActivityCaptionForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289375923U, 4284968565U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("PollBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281414195U, 4293520882U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("PollForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282600793U, 4291747826U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("GenericAttachmentIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4290954700U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AttachmentCaptionIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289513445U.WithOpacity(70), 4282873000U.WithOpacity(70))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("RepostHeaderIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289513445U.WithOpacity(50), 4283531448U.WithOpacity(50))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("HideWallPostBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279966748U, 16448250U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("HideWallPostForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4287072652U, 4285297530U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("HideWallPostAccentForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289513445U, 4282738575U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonTextForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286031321U, 4282873000U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonHover", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286031321U, 4282543518U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonPrimaryBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281414195U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonPrimaryBackgroundHover", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282730055U, 4282938019U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonPrimaryForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonPrimaryDisabledBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281414195U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonPrimaryDisabledForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286217600U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonPrimaryAccentBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284050304U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonPrimaryAccentBackgroundHover", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284050304U, 4282938019U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonSecondaryBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281414195U, 4293323760U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonSecondaryBackgroundHover", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282730055U, 4292403174U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonSecondaryForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4282873000U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonSecondaryDisabledBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281414195U, 4293323760U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonSecondaryDisabledForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286217600U, 4282873000U.WithOpacity(40))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonCardOverlayBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282598727U, 4293323760U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonCardOverlayBackgroundHover", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283848796U, 4292403174U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonCardOverlayForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4282873000U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonCardOverlayDisabledBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282598727U, 4293323760U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonCardOverlayDisabledForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4286217600U, 4282873000U.WithOpacity(40))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonGreenForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289521324U, 4283151179U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonGreenBackgroundHover", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289521324U.WithOpacity(20), 4283151179U.WithOpacity(20))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonAppBarNoFillBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4288919244U, 4283524218U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ButtonAppBarNoFillBackgroundHover", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(1940109004U, 4283524218U.WithOpacity(20))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ConversationNewMessagesCountBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283002777U, 4284650444U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MutedConversationNewMessagesCountBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282863705U, 4290299089U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ToggleControlInactiveBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4284112225U, 4291086540U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ToggleControlActiveBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4289513445U.WithOpacity(90), 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("ToggleControlThumbFill", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281085230U, 4294309623U.WithOpacity(90))),
          new ThemeManager.RuntimeThemeResources.ThemeValue("FreshNewsPanelBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281019438U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("CommunityManagementSectionIcon", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4285625722U, 4289375923U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("CommunityManagementSectionTitle", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4281085230U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("CommunityDomainTextBoxCaret", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("NewsfeedPromoToggleFadeOut", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282664263U, 4290171378U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("NotificationBubbleMessageBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281019438U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("NotificationBubbleButtonForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4282873000U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AccentRed", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4293289308U, 4293281350U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("GraffitiPageBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281085230U, 4293980658U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AttachmentPickerBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281085230U, uint.MaxValue)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MessageSnippetButtonBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(652603647U, 436222323U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MessageSnippetButtonBackgroundHover", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(870707455U, 637548915U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("AudioRecorderVolumeBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4283332259U, 4283531704U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MoreActions", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4285625722U, 4291349196U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("PlaceholderBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4282598727U, 4293520882U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("PlaceholderForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279966748U, 4289706695U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MainMenuBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4278190080U, 4281877844U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MainMenuIcons", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4285625722U, 4287930024U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MainMenuCountersBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4281085230U, 4282996582U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MainMenuSearchBoxBackground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4279966492U, 4282996582U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("MainMenuStatusForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4285625722U, 4287666595U)),
          new ThemeManager.RuntimeThemeResources.ThemeValue("GiftsDescriptionForeground", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(4285625722U, 4285297530U))
        };
        this._values.AddRange(ThemeManager.RuntimeThemeResources.AddResources((IEnumerable<string>) new List<string>()
        {
          "Gray000_Gray800",
          "Gray000_Gray900",
          "Gray000_Gray950",
          "Gray050_Gray800",
          "Gray100_Gray700",
          "Gray200_Gray500",
          "Gray200_Gray700",
          "Gray300_Gray400",
          "Gray300_Gray500",
          "Gray300_Gray600",
          "Gray400_Gray500",
          "Gray500_Gray000",
          "Gray600_Gray100",
          "Gray800_Gray000",
          "Blue200_GrayBlue100",
          "Blue300_GrayBlue200",
          "Blue300_GrayBlue100",
          "Blue300_GrayBlue400",
          "Blue300_GrayBlue500",
          "GrayBlue500_GrayBlue400",
          "GrayBlue600_GrayBlue100"
        }));
        if (Environment.OSVersion.Version.Major != 7)
          return;
        this._values.Add(new ThemeManager.RuntimeThemeResources.ThemeValue("RadioCheckBoxPressed", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4278190080U.WithOpacity(0))));
        this._values.Add(new ThemeManager.RuntimeThemeResources.ThemeValue("TextBoxEditBorder", (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(uint.MaxValue, 4278190080U.WithOpacity(87))));
      }

      private static IEnumerable<ThemeManager.RuntimeThemeResources.ThemeValue> AddResources(IEnumerable<string> resourceNames)
      {
        List<ThemeManager.RuntimeThemeResources.ThemeValue> themeValueList = new List<ThemeManager.RuntimeThemeResources.ThemeValue>();
        FieldInfo[] fields = typeof (VKColors).GetFields();
        Func<FieldInfo, string> func1 = (Func<FieldInfo, string>) (info => ((MemberInfo) info).Name);
        Dictionary<string, uint> dictionary = (Dictionary<string, uint>)Enumerable.ToDictionary<FieldInfo, string, uint>(fields, func1, (Func<FieldInfo, uint>)(info => (uint)info.GetValue(null)));
        IEnumerator<string> enumerator1 = resourceNames.GetEnumerator();
        try
        {
          while (((IEnumerator) enumerator1).MoveNext())
          {
            string current1 = enumerator1.Current;
            string[] strArray1 = ((string) current1).Split((char[]) new char[1]
            {
              '_'
            });
            if (strArray1.Length >= 2)
            {
              string[] strArray2 = ((string) strArray1[0]).Split((char[]) new char[1]
              {
                '.'
              });
              string[] strArray3 = ((string) strArray1[1]).Split((char[]) new char[1]
              {
                '.'
              });
              uint? nullable1 = new uint?();
              uint? nullable2 = new uint?();
              bool flag = false;
              Dictionary<string, uint>.KeyCollection.Enumerator enumerator2 = dictionary.Keys.GetEnumerator();
              try
              {
                while (enumerator2.MoveNext())
                {
                  string current2 = enumerator2.Current;
                  uint color = dictionary[current2];
                  if (current2 == strArray2[0])
                  {
                    int result;
                    nullable1 = strArray2.Length <= 1 || !int.TryParse(strArray2[1], out result) ? new uint?(color) : new uint?(color.WithOpacity(result));
                  }
                  if (current2 == strArray3[0])
                  {
                    int result;
                    nullable2 = strArray3.Length <= 1 || !int.TryParse(strArray3[1], out result) ? new uint?(color) : new uint?(color.WithOpacity(result));
                  }
                  if (nullable1.HasValue && nullable2.HasValue)
                  {
                    flag = true;
                    break;
                  }
                }
              }
              finally
              {
                enumerator2.Dispose();
              }
              if (flag)
                themeValueList.Add(new ThemeManager.RuntimeThemeResources.ThemeValue(current1, (ThemeManager.RuntimeThemeResources.IDualValue) new ThemeManager.RuntimeThemeResources.DualColorValue(nullable2.Value, nullable1.Value)));
            }
          }
        }
        finally
        {
          if (enumerator1 != null)
            ((IDisposable) enumerator1).Dispose();
        }
        return (IEnumerable<ThemeManager.RuntimeThemeResources.ThemeValue>) themeValueList;
      }

      private void PrintValues()
      {
          foreach (ThemeManager.RuntimeThemeResources.ThemeValue themeValue in this._values.Where<ThemeManager.RuntimeThemeResources.ThemeValue>((Func<ThemeManager.RuntimeThemeResources.ThemeValue, bool>)(item => item._value is ThemeManager.RuntimeThemeResources.DualColorValue)))
          {
              ThemeManager.RuntimeThemeResources.DualColorValue dualColorValue = (ThemeManager.RuntimeThemeResources.DualColorValue)themeValue._value;
          }
      }

      public void Apply(Theme theme, bool affectRootFrame)
      {
        List<ThemeManager.RuntimeThemeResources.ThemeValue>.Enumerator enumerator = this._values.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
            enumerator.Current.Apply(theme);
        }
        finally
        {
          enumerator.Dispose();
        }
        ThemeManager.RuntimeThemeResources.IDualValue dualValue1 = this._values[0]._value;
        ThemeManager.RuntimeThemeResources.IDualValue dualValue2 = this._values[1]._value;
        ThemeManager.RuntimeThemeResources.IDualValue dualValue3 = this._values[2]._value;
        if (affectRootFrame)
          this.AttachRootFrameNavigationEvents((Color) dualValue1.Value(theme), (Color) dualValue2.Value(theme), (Color) dualValue3.Value(theme));
        this._values = (List<ThemeManager.RuntimeThemeResources.ThemeValue>) null;
      }

      private void AttachOnRootFrameReady(PhoneApplicationFrame frame, Color background, Color foreground, Color chrome)
      {
          frame.Navigated += (NavigatedEventHandler)((x, xe) =>
          {
              PhoneApplicationPage page = xe.Content as PhoneApplicationPage;
              if (page == null)
                  return;
              this.SetSystemComponentColors(page, background, foreground, chrome);
          });
          this.SetSystemComponentColors(frame.Content as PhoneApplicationPage, background, foreground, chrome);
      }

      private void SetSystemComponentColors(PhoneApplicationPage page, Color background, Color foreground, Color chrome)
      {
        if (page == null)
          return;
        Color color = foreground;
        if ((Colors.White== foreground) && ThemeManager._themeAtStartup == Theme.Light)
          color = ThemeManager.AlmostWhite;
        SystemTray.SetBackgroundColor((DependencyObject) page, background);
        SystemTray.SetForegroundColor((DependencyObject) page, color);
        if (ThemeManager.OverrideOptions != ThemeManagerOverrideOptions.SystemTrayAndApplicationBars)
          return;
        IApplicationBar applicationBar = page.ApplicationBar;
        if (applicationBar == null)
          return;
        applicationBar.MatchOverriddenTheme();
      }

      private void AttachRootFrameNavigationEvents(Color background, Color foreground, Color chrome)
      {
          ThemeManager._chrome = chrome;
          ThemeManager._background = background;
          ThemeManager._foreground = foreground;
          UIElement rootVisual = Application.Current.RootVisual;
          if (rootVisual != null && !(rootVisual is Canvas))
          {
              Control control = rootVisual as Control;
              if (control != null)
              {
                  control.Background=(new SolidColorBrush(background));
                  control.CacheMode=(new BitmapCache());
                  if (ScaleFactor.GetScaleFactor() == 150)
                  {
                      control.Margin=(new Thickness(0.0, 0.0, 0.0, -1.0));
                  }
              }
              if (ThemeManager.OverrideOptions == ThemeManagerOverrideOptions.SystemTrayAndApplicationBars || ThemeManager.OverrideOptions == ThemeManagerOverrideOptions.SystemTrayColors)
              {
                  PhoneApplicationFrame phoneApplicationFrame = rootVisual as PhoneApplicationFrame;
                  if (phoneApplicationFrame != null)
                  {
                      this.AttachOnRootFrameReady(phoneApplicationFrame, background, foreground, chrome);
                  }
              }
              return;
          }
          Deployment.Current.Dispatcher.BeginInvoke(delegate
          {
              this.AttachRootFrameNavigationEvents(background, foreground, chrome);
          });
      }

      private uint ColorToUInt(Color color)
      {
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        return (uint) ((int) ((Color) @color).A << 24 | (int) ((Color) @color).R << 16 | (int) ((Color) @color).G << 8) | (uint) ((Color) @color).B;
      }

      internal class DualColorValue : ThemeManager.RuntimeThemeResources.IDualValue
      {
        public uint _dark;
        public uint _light;

        public DualColorValue(uint dark, uint light)
        {
          this._dark = dark;
          this._light = light;
        }

        internal static void SetColorAndBrush(string prefix, Color color)
        {
          Color color1;
          if (Application.Current.Resources.Contains(string.Concat(prefix, "Color")))
          {
            color1 = (Color) Application.Current.Resources[string.Concat(prefix, "Color")];
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            color1.A = (((Color)@color).A);
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            color1.B = (((Color)@color).B);
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            color1.G = (((Color)@color).G);
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            color1.R = (((Color)@color).R);
          }
          else
          {
            color1 = color;
            Application.Current.Resources.Add(string.Concat(prefix, "Color"), color1);
          }
          if (Application.Current.Resources.Contains(string.Concat(prefix, "Brush")))
          {
            ((SolidColorBrush) Application.Current.Resources[string.Concat(prefix, "Brush")]).Color = color1;
          }
          else
          {
            SolidColorBrush solidColorBrush1 = new SolidColorBrush();
            Color color2 = color1;
            solidColorBrush1.Color = color2;
            SolidColorBrush solidColorBrush2 = solidColorBrush1;
            Application.Current.Resources.Add(string.Concat(prefix, "Brush"), solidColorBrush2);
          }
        }

        public object Value(Theme theme)
        {
          return ThemeManager.RuntimeThemeResources.DualColorValue.ToColor(theme == Theme.Dark ? this._dark : this._light);
        }

        internal static Color ToColor(uint argb)
        {
          return Color.FromArgb((byte) (((long) argb & -16777216L) >> 24), (byte) ((argb & 16711680U) >> 16), (byte) ((argb & 65280U) >> 8), (byte) (argb & (uint) byte.MaxValue));
        }

        public void Apply(Theme theme, string prefix)
        {
          ThemeManager.RuntimeThemeResources.DualColorValue.SetColorAndBrush(string.Concat("Phone", prefix), (Color) this.Value(theme));
        }
      }

      private class DualValue<T> : ThemeManager.RuntimeThemeResources.IDualValue
      {
        private T _dark;
        private T _light;

        public DualValue(T dark, T light)
        {
          this._dark = dark;
          this._light = light;
        }

        public object Value(Theme theme)
        {
          return (theme == Theme.Dark ? this._dark : this._light);
        }

        public void Apply(Theme theme, string prefix)
        {
          string str = string.Concat("Phone", prefix);
          Application.Current.Resources.Remove(str);
          Application.Current.Resources.Add(str, this.Value(theme));
        }
      }

      private interface IDualValue
      {
        object Value(Theme theme);

        void Apply(Theme theme, string prefix);
      }

      private class ThemeValue
      {
        public string _prefix;
        public ThemeManager.RuntimeThemeResources.IDualValue _value;

        public ThemeValue(string prefix, ThemeManager.RuntimeThemeResources.IDualValue val)
        {
          this._prefix = prefix;
          this._value = val;
        }

        public void Apply(Theme theme)
        {
          this._value.Apply(theme, this._prefix);
        }
      }
    }
  }
}
