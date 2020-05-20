using System.Collections.Generic;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Library
{
  public class NewsPhotosInfo
  {
    public long SourceId { get; set; }

    public int Date { get; set; }

    public NewsPhotosInfo.NewsPhotoType NewsType { get; set; }

    public int Count { get; set; }

    public List<Photo> Photos { get; set; }

    public enum NewsPhotoType
    {
      Photo,
      PhotoTag,
    }
  }
}
