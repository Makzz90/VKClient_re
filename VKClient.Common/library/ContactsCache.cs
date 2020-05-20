using System.Collections.Generic;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class ContactsCache : IBinarySerializable
  {
    public List<string> PhoneNumbers { get; set; }

    public ContactsCache()
    {
      this.PhoneNumbers = new List<string>();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteList(this.PhoneNumbers);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.PhoneNumbers = reader.ReadList();
    }
  }
}
