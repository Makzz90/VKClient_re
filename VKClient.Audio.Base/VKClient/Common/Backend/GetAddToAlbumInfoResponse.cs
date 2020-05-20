using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class GetAddToAlbumInfoResponse
  {
    public VKList<Group> Groups { get; set; }

    public List<long> AlbumsByVideo { get; set; }

    public VKList<VideoAlbum> Albums { get; set; }

    public GetAddToAlbumInfoResponse()
    {
      this.Groups = new VKList<Group>();
      this.AlbumsByVideo = new List<long>();
      this.Albums = new VKList<VideoAlbum>();
    }
  }
}
