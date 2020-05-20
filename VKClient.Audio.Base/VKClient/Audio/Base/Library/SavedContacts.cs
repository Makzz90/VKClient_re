using System;
using System.Collections.Generic;
using System.IO;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.Library
{
  public class SavedContacts : IBinarySerializable
  {
    public List<User> SavedUsers { get; set; }

    public DateTime SyncedDate { get; set; }

    public FriendRequests Requests { get; set; }

    public long CurrentUserId { get; set; }

    public SavedContacts()
    {
      this.SavedUsers = new List<User>();
      this.SyncedDate = DateTime.MinValue;
      this.Requests =  null;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(3);
      writer.WriteList<User>((IList<User>) this.SavedUsers, 10000);
      writer.Write(this.SyncedDate);
      writer.Write(this.CurrentUserId);
      writer.Write<FriendRequests>(this.Requests, false);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.SavedUsers = reader.ReadList<User>();
      this.SyncedDate = reader.ReadDateTime();
      int num2 = 2;
      if (num1 >= num2)
        this.CurrentUserId = reader.ReadInt64();
      int num3 = 3;
      if (num1 < num3)
        return;
      this.Requests = reader.ReadGeneric<FriendRequests>();
    }
  }
}
