using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class BlockInformation : IBinarySerializable
  {
    public long admin_id { get; set; }

    public int reason { get; set; }

    public string comment { get; set; }

    public int comment_visible { get; set; }

    public int date { get; set; }

    public int end_date { get; set; }

    public User manager { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.admin_id);
      writer.Write(this.reason);
      writer.WriteString(this.comment);
      writer.Write(this.comment_visible);
      writer.Write(this.date);
      writer.Write(this.end_date);
      writer.Write<User>(this.manager, false);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.admin_id = reader.ReadInt64();
      this.reason = reader.ReadInt32();
      this.comment = reader.ReadString();
      this.comment_visible = reader.ReadInt32();
      this.date = reader.ReadInt32();
      this.end_date = reader.ReadInt32();
      this.manager = reader.ReadGeneric<User>();
    }
  }
}
