using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
    public class NewsFeedData
    {
        public int TotalCount
        {
            get
            {
                return this.count;
            }
            set
            {
                this.count = value;
            }
        }

        public int count { get; set; }

        public List<NewsItem> items { get; set; }

        public List<User> profiles { get; set; }

        public List<Group> groups { get; set; }

        public int new_offset { get; set; }

        public string next_from { get; set; }

        public VKList<UserNotification> notifications { get; set; }

        public bool notifications_updated { get; set; }

        public NewsFeedSectionsAndLists lists { get; set; }

        public string feed_type { get; set; }

        public NewsFeedType? FeedType { get; set; }

        public NewsFeedConsts consts { get; set; }

        public NewsFeedData()
        {
            this.items = new List<NewsItem>();
            this.profiles = new List<User>();
            this.groups = new List<Group>();
        }
    }
}
