using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.AudioManager
{
  public class PlaybackSettings : IBinarySerializable
  {
    public bool Shuffle { get; set; }

    public bool Repeat { get; set; }

    public bool Broadcast { get; set; }

    public Metadata Metadata { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.Shuffle);
      writer.Write(this.Repeat);
      writer.Write(this.Broadcast);
      writer.Write<Metadata>(this.Metadata, false);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.Shuffle = reader.ReadBoolean();
      this.Repeat = reader.ReadBoolean();
      this.Broadcast = reader.ReadBoolean();
      this.Metadata = reader.ReadGeneric<Metadata>();
    }
  }
}
