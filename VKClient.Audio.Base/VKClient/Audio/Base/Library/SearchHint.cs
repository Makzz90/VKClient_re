using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.Library
{
  public class SearchHint
  {
    public const string EXTENDED_SEARCH_TYPE = "extended_search";
    public const string INTERNAL_LINK_TYPE = "internal_link";

    public string type { get; set; }

    public Group group { get; set; }

    public User profile { get; set; }

    public string section { get; set; }

    public string description { get; set; }

    public int global { get; set; }
  }
}
