using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class LinkButton : IBinarySerializable
  {
    private string _title;

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

    public string url { get; set; }

    public LinkButtonAction action { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.WriteString(this.title);
      writer.WriteString(this.url);
      writer.Write<LinkButtonAction>(this.action, false);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.title = reader.ReadString();
      this.url = reader.ReadString();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.action = reader.ReadGeneric<LinkButtonAction>();
    }
  }
}
