using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Audio.Base.DataObjects
{
  public class Chat : ChatBase, IBinarySerializable
  {
    public List<long> users { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.WriteString(this.type);
      writer.Write(this.chat_id);
      writer.WriteString(this.title);
      writer.WriteString(this.admin_id);
      if (this.users != null)
        writer.WriteString(this.users.GetCommaSeparated());
      else
        writer.WriteString("");
    }

    public void Read(BinaryReader reader)
    {
      this.type = reader.ReadString();
      this.chat_id = reader.ReadInt64();
      this.title = reader.ReadString();
      this.admin_id = reader.ReadString();
      this.users = reader.ReadString().ParseCommaSeparated();
    }
  }
}
