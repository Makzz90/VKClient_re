using System.Collections.Generic;

namespace VKClient.Audio.Base
{
  public class SendMessageRequest
  {
    public string MessageBody { get; set; }

    public long UserOrCharId { get; set; }

    public bool IsChat { get; set; }

    public List<string> AttachmentIds { get; set; }

    public List<int> ForwardedMessagesIds { get; set; }

    public bool IsGeoAttached { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int StickerId { get; set; }

    public string StickerReferrer { get; set; }
  }
}
