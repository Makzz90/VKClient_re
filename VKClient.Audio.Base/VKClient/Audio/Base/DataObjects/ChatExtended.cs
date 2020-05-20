using System.Collections.Generic;
using System.IO;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class ChatExtended : ChatBase, IBinarySerializable
  {
    public List<User> users { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.WriteString(this.type);
      writer.Write(this.chat_id);
      writer.WriteString(this.title);
      writer.WriteString(this.admin_id);
      writer.WriteList<User>((IList<User>) this.users, 10000);
      writer.WriteString(this.photo_100);
      writer.WriteString(this.photo_200);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.type = reader.ReadString();
      this.chat_id = reader.ReadInt64();
      this.title = reader.ReadString();
      this.admin_id = reader.ReadString();
      this.users = reader.ReadList<User>();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.photo_100 = reader.ReadString();
      this.photo_200 = reader.ReadString();
    }
  }
}
