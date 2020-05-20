using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class AudioData
  {
    public int AllAlbumsCount { get; set; }

    public List<AudioAlbum> AllAlbums { get; set; }

    public List<AudioObj> AllTracks { get; set; }
  }
}
