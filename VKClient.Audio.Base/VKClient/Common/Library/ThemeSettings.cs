using System.IO;
using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class ThemeSettings : IBinarySerializable
  {
    private static bool? _isLightTheme;

    public static bool IsLightTheme
    {
      get
      {
        if (!ThemeSettings._isLightTheme.HasValue)
          ThemeSettings._isLightTheme = new bool?((Visibility) Application.Current.Resources["PhoneLightThemeVisibility"] == 0);
        return ThemeSettings._isLightTheme.Value;
      }
    }

    public int BackgroundSettings { get; set; }

    public int AccentSettings { get; set; }

    public int TileSettings { get; set; }

    public int LanguageSettings { get; set; }

    public string LanguageCultureString
    {
      get
      {
        string str = "";
        switch (this.LanguageSettings)
        {
          case 1:
            str = "en-US";
            break;
          case 2:
            str = "ru-RU";
            break;
          case 3:
            str = "uk-UA";
            break;
          case 4:
            str = "be-BY";
            break;
          case 5:
            str = "pt-BR";
            break;
          case 6:
            str = "kk-KZ";
            break;
        }
        return str;
      }
    }

    public ThemeSettings()
    {
      this.BackgroundSettings = 3;
      this.AccentSettings = 1;
      this.TileSettings = 1;
    }

    public static ThemeSettings CreateNew(ThemeSettings settings)
    {
      return new ThemeSettings()
      {
        BackgroundSettings = settings.BackgroundSettings,
        AccentSettings = settings.AccentSettings,
        LanguageSettings = settings.LanguageSettings,
        TileSettings = settings.TileSettings
      };
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(3);
      writer.Write(this.BackgroundSettings);
      writer.Write(this.AccentSettings);
      writer.Write(this.LanguageSettings);
      writer.Write(this.TileSettings);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      int num2 = 1;
      if (num1 >= num2)
      {
        this.BackgroundSettings = reader.ReadInt32();
        this.AccentSettings = reader.ReadInt32();
      }
      int num3 = 2;
      if (num1 >= num3)
        this.LanguageSettings = reader.ReadInt32();
      int num4 = 3;
      if (num1 < num4)
        return;
      this.TileSettings = reader.ReadInt32();
    }
  }
}
