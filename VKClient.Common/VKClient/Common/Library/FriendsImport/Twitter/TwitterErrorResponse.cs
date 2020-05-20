using System.Collections.Generic;

namespace VKClient.Common.Library.FriendsImport.Twitter
{
  public class TwitterErrorResponse
  {
    public List<TwitterError> errors { get; set; }

    public TwitterErrorResponse()
    {
      this.errors = new List<TwitterError>();
    }
  }
}
