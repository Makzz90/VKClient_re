using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class Note : IBinarySerializable
  {
    private string _title = "";

    public long nid
    {
      get
      {
        return this.id;
      }
      set
      {
        this.id = value;
      }
    }

    public long id { get; set; }

    public long user_id { get; set; }

    public long owner_id
    {
      get
      {
        return this.user_id;
      }
      set
      {
        this.user_id = value;
      }
    }

    public string title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = (value ?? "").ForUI();
      }
    }

    public int ncom
    {
      get
      {
        return this.comments;
      }
      set
      {
        this.comments = value;
      }
    }

    public int comments { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(VKConstants.SerializationVersion);
      writer.Write(this.nid);
      writer.Write(this.owner_id);
      writer.WriteString(this.title);
      writer.Write(this.ncom);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.nid = reader.ReadInt64();
      this.owner_id = reader.ReadInt64();
      this.title = reader.ReadString();
      this.ncom = reader.ReadInt32();
    }

    public override string ToString()
    {
      return string.Format("note{0}_{1}", this.owner_id, this.id);
    }
  }
}
