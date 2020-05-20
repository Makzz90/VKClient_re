using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Exports : IBinarySerializable
  {
    public int twitter { get; set; }

    public int facebook { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(VKConstants.SerializationVersion);
      writer.Write(this.twitter);
      writer.Write(this.facebook);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.twitter = reader.ReadInt32();
      this.facebook = reader.ReadInt32();
    }
  }
}
