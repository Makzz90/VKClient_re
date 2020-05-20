using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
    public class NewsFeedGetParams
    {
        public int count { get; set; }

        public string from { get; set; }

        public bool photoFeed { get; set; }

        public List<long> source_ids { get; set; }

        public long NewsListId { get; set; }

        public NewsFeedType? FeedType { get; set; }

        public bool UpdateFeedType { get; set; }

        public bool SyncNotifications { get; set; }

        public int UpdateAwayTime { get; set; }

        public int UpdatePosition { get; set; }

        public string UpdatePost { get; set; }

        public bool? TopFeedPromoAnswer { get; set; }

        public long TopFeedPromoId { get; set; }

        public NewsFeedGetParams()
        {
            this.count = 20;
        }
    }
}
