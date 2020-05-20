using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
    public class NotificationData
    {
        public int TotalCount { get; set; }

        public List<Notification> items { get; set; }

        public List<User> profiles { get; set; }

        public List<Group> groups { get; set; }

        //public string next_from { get; set; }//todo:bug

        //public int new_offset { get; set; }//todo:bug

        public int last_viewed { get; set; }
    }
}
