using System;
using System.IO;
using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class PickableItem : IBinarySerializable
  {
    private string _alias;

    public long ID { get; set; }

    public string Name { get; set; }

    public string Alias
    {
      get
      {
        return this._alias ?? this.Name.ToLowerInvariant();
      }
      set
      {
        this._alias = value;
      }
    }

    public Uri ImageSource { get; set; }

    public Visibility ToolImageVisibility
    {
      get
      {
        if (!(this.ImageSource ==  null))
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.ID);
      writer.WriteString(this.Name);
      writer.WriteString(this.Alias);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.ID = reader.ReadInt64();
      this.Name = reader.ReadString();
      this.Alias = reader.ReadString();
    }
  }
}
