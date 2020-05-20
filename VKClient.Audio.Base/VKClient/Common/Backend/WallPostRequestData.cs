using System.Collections.Generic;

namespace VKClient.Common.Backend
{
  public class WallPostRequestData
  {
    public long owner_id { get; set; }

    public long post_id { get; set; }

    public long comment_id { get; set; }

    public string message { get; set; }

    public List<string> AttachmentIds { get; set; }

    public double? latitude { get; set; }

    public double? longitude { get; set; }

    public long? publish_date { get; set; }

    public bool PublishOnTwitter { get; set; }

    public bool PublishOnFacebook { get; set; }

    public bool OnBehalfOfGroup { get; set; }

    public bool Sign { get; set; }

    public bool FriendsOnly { get; set; }
  }
}
