using System.Collections.Generic;
using System.IO;
using System.Linq;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class FriendRequests : IBinarySerializable
  {
    public int count { get; set; }

    public int menu_counter { get; set; }

    public List<FriendRequest> requests { get; set; }

    public List<User> profiles { get; set; }

    public bool are_suggested_friends { get; set; }

    public FriendRequests()
    {
      this.requests = new List<FriendRequest>();
      this.profiles = new List<User>();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.Write(this.count);
      writer.Write(this.are_suggested_friends);
      writer.WriteList<FriendRequest>((IList<FriendRequest>) this.requests, 10000);
      writer.WriteList<User>((IList<User>) this.profiles.ToList<User>(), 10000);
      writer.Write(this.menu_counter);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.count = reader.ReadInt32();
      this.are_suggested_friends = reader.ReadBoolean();
      this.requests = reader.ReadList<FriendRequest>();
      this.profiles = reader.ReadList<User>();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.menu_counter = reader.ReadInt32();
    }
  }
}
