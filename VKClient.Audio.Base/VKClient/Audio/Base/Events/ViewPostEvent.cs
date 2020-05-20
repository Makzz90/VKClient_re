using System.Collections.Generic;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;

namespace VKClient.Audio.Base.Events
{
  public class ViewPostEvent : StatEventBase
  {
    public string PostIdExtended
    {
      get
      {
        string str = this.PostId + "|" + this.ItemType + "|" + this.Source;
        if (this.Source == ViewPostSource.NewsFeed)
          str = str + "_" + (this.IsTopFeed ? "Top" : this.FeedSource.ToString());
        return str + "|" + this.Position;
      }
    }

    public string PostId { get; set; }

    public List<string> CopyPostIds { get; set; }

    public int Position { get; set; }

    public NewsFeedItemType ItemType { get; set; }

    public ViewPostSource Source { get; set; }

    public NewsSourcesPredefined FeedSource { get; set; }

    public bool IsTopFeed { get; set; }

    public ViewPostEvent()
    {
      this.CopyPostIds = new List<string>();
      this.Source = CurrentNewsFeedSource.Source;
      this.FeedSource = CurrentNewsFeedSource.FeedSource;
      this.IsTopFeed = AppGlobalStateManager.Current.GlobalState.NewsfeedTopEnabled;
    }
  }
}
