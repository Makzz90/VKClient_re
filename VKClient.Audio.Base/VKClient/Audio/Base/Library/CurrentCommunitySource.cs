using VKClient.Audio.Base.Events;

namespace VKClient.Audio.Base.Library
{
  public static class CurrentCommunitySource
  {
    public static CommunityOpenSource Source { get; set; }

    public static string ToString(this CommunityOpenSource source)
    {
      switch (CurrentCommunitySource.Source)
      {
        case CommunityOpenSource.UserProfileOrUserMedia:
          return "profile";
        case CommunityOpenSource.OwnProfileOrOwnMedia:
          return "my_groups";
        case CommunityOpenSource.OtherCommunityOrOtherCommunityMedia:
          return "group";
        case CommunityOpenSource.Newsfeed:
          return "feed";
        case CommunityOpenSource.Search:
          return "search";
        case CommunityOpenSource.Recommendations:
          return "recommend";
        default:
          return  null;
      }
    }
  }
}
