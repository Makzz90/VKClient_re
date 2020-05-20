using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class LinkApplication : IBinarySerializable
  {
    public LinkStore store { get; set; }

    public string app_id { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write<LinkStore>(this.store, false);
      writer.WriteString(this.app_id);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.store = reader.ReadGeneric<LinkStore>();
      this.app_id = reader.ReadString();
    }
  }
}
