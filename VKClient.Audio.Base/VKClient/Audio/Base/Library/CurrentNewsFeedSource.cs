using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.Library
{
  public static class CurrentNewsFeedSource
  {
    public static ViewPostSource Source { get; set; }

    public static NewsSourcesPredefined FeedSource { get; set; }
  }
}
