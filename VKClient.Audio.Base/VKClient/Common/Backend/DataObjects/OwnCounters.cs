using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class OwnCounters : IBinarySerializable
  {
    public int friends { get; set; }

    public int messages { get; set; }

    public int photos { get; set; }

    public int videos { get; set; }

    public int groups { get; set; }

    public int notifications { get; set; }

    public int sdk { get; set; }

    public int app_requests { get; set; }

    public bool IsEqual(OwnCounters anotherInstance)
    {
      if (this.friends == anotherInstance.friends && this.messages == anotherInstance.messages && (this.photos == anotherInstance.photos && this.videos == anotherInstance.videos) && (this.groups == anotherInstance.groups && this.notifications == anotherInstance.notifications))
        return this.app_requests == anotherInstance.app_requests;
      return false;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(3);
      writer.Write(this.friends);
      writer.Write(this.messages);
      writer.Write(this.photos);
      writer.Write(this.videos);
      writer.Write(this.groups);
      writer.Write(this.notifications);
      writer.Write(this.sdk);
      writer.Write(this.app_requests);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.friends = reader.ReadInt32();
      this.messages = reader.ReadInt32();
      this.photos = reader.ReadInt32();
      this.videos = reader.ReadInt32();
      this.groups = reader.ReadInt32();
      this.notifications = reader.ReadInt32();
      int num2 = 2;
      if (num1 >= num2)
        this.sdk = reader.ReadInt32();
      int num3 = 3;
      if (num1 < num3)
        return;
      this.app_requests = reader.ReadInt32();
    }
  }
}
