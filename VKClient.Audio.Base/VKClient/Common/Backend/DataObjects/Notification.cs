using Newtonsoft.Json;
using System;
using VKClient.Audio.Base.DataObjects;

namespace VKClient.Common.Backend.DataObjects
{
    public class Notification
    {
        private NotificationType _nType = NotificationType.unknown;
        private object _parsedParent;
        private object _parsedFeedback;

        public NotificationType NotType
        {
            get
            {
                return this._nType;
            }
        }

        public string type { get; set; }

        public string date { get; set; }//todo: was int

        public object parent { get; set; }

        public object feedback { get; set; }

        //public object reply { get; set; }

        public object ParsedParent
        {
            get
            {
                if (this._parsedParent != null)
                    return this._parsedParent;
                if (this.parent == null)
                    return null;
                string str = this.parent.ToString();
                switch (this.NotType)
                {
                    case NotificationType.mention_comments:
                    case NotificationType.comment_post:
                    case NotificationType.like_post:
                    case NotificationType.copy_post:
                        this._parsedParent = JsonConvert.DeserializeObject<WallPost>(str);
                        break;
                    case NotificationType.comment_photo:
                    case NotificationType.like_photo:
                    case NotificationType.copy_photo:
                    case NotificationType.mention_comment_photo:
                        this._parsedParent = JsonConvert.DeserializeObject<Photo>(str);
                        break;
                    case NotificationType.comment_video:
                    case NotificationType.like_video:
                    case NotificationType.copy_video:
                    case NotificationType.mention_comment_video:
                        this._parsedParent = JsonConvert.DeserializeObject<Video>(str);
                        break;
                    case NotificationType.reply_comment:
                    case NotificationType.like_comment:
                    case NotificationType.reply_comment_photo:
                    case NotificationType.reply_comment_video:
                    case NotificationType.like_comment_photo:
                    case NotificationType.like_comment_video:
                    case NotificationType.like_comment_topic:
                    case NotificationType.reply_comment_market:
                        this._parsedParent = JsonConvert.DeserializeObject<Comment>(str);
                        break;
                    case NotificationType.reply_topic:
                        Topic topic = JsonConvert.DeserializeObject<Topic>(str);
                        topic.tid = topic.id;
                        this._parsedParent = topic;
                        break;
                }
                return this._parsedParent;
            }
        }

        public object ParsedFeedback
        {
            get
            {
                if (this._parsedFeedback != null)
                    return this._parsedFeedback;
                string str = this.feedback.ToString();
                switch (this.NotType)
                {
                    case NotificationType.follow:
                    case NotificationType.friend_accepted:
                    case NotificationType.like_post:
                    case NotificationType.like_comment:
                    case NotificationType.like_photo:
                    case NotificationType.like_video:
                    case NotificationType.like_comment_photo:
                    case NotificationType.like_comment_video:
                    case NotificationType.like_comment_topic:
                        this._parsedFeedback = JsonConvert.DeserializeObject<VKList<FeedbackUser>>(str).items;
                        break;
                    case NotificationType.mention:
                    case NotificationType.wall:
                    case NotificationType.wall_publish:
                        this._parsedFeedback = JsonConvert.DeserializeObject<WallPost>(str);
                        break;
                    case NotificationType.mention_comments:
                    case NotificationType.comment_post:
                    case NotificationType.comment_photo:
                    case NotificationType.comment_video:
                    case NotificationType.reply_comment:
                    case NotificationType.reply_topic:
                    case NotificationType.reply_comment_photo:
                    case NotificationType.reply_comment_video:
                    case NotificationType.mention_comment_photo:
                    case NotificationType.mention_comment_video:
                    case NotificationType.reply_comment_market:
                        this._parsedFeedback = JsonConvert.DeserializeObject<Comment>(str);
                        break;
                    case NotificationType.copy_post:
                    case NotificationType.copy_photo:
                    case NotificationType.copy_video:
                        this._parsedFeedback = JsonConvert.DeserializeObject<VKList<FeedbackCopyInfo>>(str).items;
                        break;
                    case NotificationType.money_transfer_received:
                    case NotificationType.money_transfer_accepted:
                    case NotificationType.money_transfer_declined:
                        this._parsedFeedback = JsonConvert.DeserializeObject<MoneyTransfer>(str);
                        break;
                }
                if (this._parsedFeedback != null)
                    return this._parsedFeedback;
                if (this.parent == null)
                    return "";
                return this.parent.ToString();
            }
        }

        public void UpdateNotificationType()
        {
            if (Enum.TryParse<NotificationType>(this.type, out this._nType))
                return;
            this._nType = NotificationType.unknown;
        }

        public int Date
        {
            get
            {
                return int.Parse(this.date);
            }
        }
    }
}
