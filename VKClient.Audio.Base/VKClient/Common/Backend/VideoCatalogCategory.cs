using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class VideoCatalogCategory
  {
    public const string ID_MUSIC = "1";
    public const string ID_SPORT = "2";
    public const string ID_GAMES = "3";
    public const string ID_HUMOUR = "4";
    public const string ID_ANIMALS = "5";
    public const string ID_DANCE = "6";
    public const string ID_COOKING = "7";
    public const string ID_VEHICLES = "8";
    public const string ID_BEAUTY = "9";
    public const string ID_SCIENCE = "10";
    public const string ID_FUN = "12";
    public const string ID_FAMILY = "13";
    public const string ID_EROTICS = "15";
    public const string ID_EDUCATION = "16";
    public const string ID_TRAILER = "17";
    public const string ID_SOCIAL = "18";
    public const string ID_GAME_STREAMS = "19";
    public const string ID_COGNITIVE = "63";
    public const string ID_CARTOONS = "69";
    public const string ID_FITNESS = "108";
    public const string ID_MY = "my";
    public const string ID_SERIES = "series";
    public const string ID_FEED = "feed";
    public const string ID_UGC = "ugc";
    public const string VIEW_TYPE_HORIZONTAL = "horizontal";
    public const string VIEW_TYPE_HORIZONTAL_COMPACT = "horizontal_compact";
    public const string VIEW_TYPE_VERTICAL = "vertical";
    public const string VIEW_TYPE_VERTICAL_COMPACT = "vertical_compact";
    public const string CATEGORY_TYPE_CHANNEL = "channel";
    public const string CATEGORY_TYPE_CATEGORY = "category";

    public string id { get; set; }

    public string view { get; set; }

    public string next { get; set; }

    public string name { get; set; }

    public int can_hide { get; set; }

    public string type { get; set; }

    public List<VideoCatalogItem> items { get; set; }
  }
}
