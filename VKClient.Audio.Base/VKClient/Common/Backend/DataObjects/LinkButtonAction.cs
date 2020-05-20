using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public sealed class LinkButtonAction : IBinarySerializable
  {
    private string _type;

    public string type
    {
      get
      {
        return this._type;
      }
      set
      {
        this._type = value;
        string type = this._type;
        if (!(type == "join_group_and_open_url"))
        {
          if (!(type == "open_url"))
            return;
          this.Type = LinkButtonActionType.OpenUrl;
        }
        else
          this.Type = LinkButtonActionType.JoinGroupAndOpenUrl;
      }
    }

    public LinkButtonActionType Type { get; private set; }

    public long group_id { get; set; }

    public string url { get; set; }

    public string target { get; set; }

    public bool IsExternal
    {
      get
      {
        return this.target == "external";
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.WriteString(this.type);
      writer.Write(this.group_id);
      writer.WriteString(this.url);
      writer.WriteString(this.target);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.type = reader.ReadString();
      this.group_id = reader.ReadInt64();
      this.url = reader.ReadString();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.target = reader.ReadString();
    }
  }
}
