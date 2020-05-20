using System.Collections.Generic;
using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.AudioManager
{
  public class Playlist : IBinarySerializable
  {
    private List<AudioObj> _tracks;

    public List<AudioObj> Tracks
    {
      get
      {
        return this._tracks;
      }
      set
      {
        this._tracks = value;
        if (this._tracks == null)
          return;
        this.ShuffledIndexes = new List<int>();
        for (int index = 0; index < this._tracks.Count; ++index)
          this.ShuffledIndexes.Add(index);
        this.ShuffledIndexes.Shuffle<int>();
      }
    }

    public Metadata Metadata { get; set; }

    public List<int> ShuffledIndexes { get; set; }

    public Playlist()
    {
      this.Tracks = new List<AudioObj>();
      this.Metadata = new Metadata();
      this.ShuffledIndexes = new List<int>();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.Write<Metadata>(this.Metadata, false);
      writer.WriteList<AudioObj>((IList<AudioObj>) this.Tracks, 10000);
      writer.WriteList(this.ShuffledIndexes);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.Metadata = reader.ReadGeneric<Metadata>();
      this.Tracks = reader.ReadList<AudioObj>();
      this.ShuffledIndexes = reader.ReadListInt();
    }
  }
}
