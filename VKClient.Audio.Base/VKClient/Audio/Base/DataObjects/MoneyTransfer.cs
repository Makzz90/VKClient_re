using System.IO;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class MoneyTransfer : IBinarySerializable
  {
    public long id { get; set; }

    public long from_id { get; set; }

    public long to_id { get; set; }

    public int status { get; set; }

    public int date { get; set; }

    public Amount amount { get; set; }

    public string comment { get; set; }

    public string accept_url { get; set; }

    public bool IsOutbox
    {
      get
      {
        return this.from_id == AppGlobalStateManager.Current.LoggedInUserId;
      }
    }

    public bool IsInbox
    {
      get
      {
        return this.to_id == AppGlobalStateManager.Current.LoggedInUserId;
      }
    }

    public long OtherUserId
    {
      get
      {
        if (!this.IsOutbox)
          return this.from_id;
        return this.to_id;
      }
    }

    public bool IsAwaits
    {
      get
      {
        return this.status == 0;
      }
    }

    public bool IsCompleted
    {
      get
      {
        return this.status == 1;
      }
    }

    public bool IsCancelled
    {
      get
      {
        return this.status == 2;
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(3);
      writer.Write(this.from_id);
      writer.Write(this.to_id);
      writer.Write(this.date);
      writer.Write<Amount>(this.amount, false);
      writer.WriteString(this.comment);
      writer.Write(this.status);
      writer.Write(this.id);
      writer.WriteString(this.accept_url);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.from_id = reader.ReadInt64();
      this.to_id = reader.ReadInt64();
      this.date = reader.ReadInt32();
      this.amount = reader.ReadGeneric<Amount>();
      this.comment = reader.ReadString();
      this.status = reader.ReadInt32();
      int num2 = 2;
      if (num1 >= num2)
        this.id = reader.ReadInt64();
      int num3 = 3;
      if (num1 < num3)
        return;
      this.accept_url = reader.ReadString();
    }

    public bool IsEquals(long transferId, long fromId, long toId)
    {
      if (this.id == transferId && this.from_id == fromId)
        return this.to_id == toId;
      return false;
    }
  }
}
