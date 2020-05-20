using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;
using VKMessenger.Backend;

namespace VKClient.Audio.Base
{
  public class MessageListResponse
  {
    public int TotalCount { get; set; }

    public int Unread { get; set; }

    public int Skipped { get; set; }

    public List<Message> Messages { get; set; }

    public List<DialogHeaderInfo> DialogHeaders { get; set; }

    public List<User> Users { get; set; }
  }
}
