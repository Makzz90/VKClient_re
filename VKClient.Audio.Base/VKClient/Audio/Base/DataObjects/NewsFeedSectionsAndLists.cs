using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class NewsFeedSectionsAndLists
  {
    public List<NewsFeedSection> sections { get; set; }

    public List<NewsFeedList> lists { get; set; }
  }
}
