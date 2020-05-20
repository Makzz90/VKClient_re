using System.ComponentModel;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class FriendsList : INotifyPropertyChanged, IBinarySerializable
  {
    private string _name = "";

    public long lid
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

    public string name
    {
      get
      {
        return this._name;
      }
      set
      {
        this._name = (value ?? "").ForUI();
        if (this.PropertyChanged == null)
          return;
        this.PropertyChanged(this, new PropertyChangedEventArgs("name"));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public override string ToString()
    {
      return this.name;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.id);
      writer.WriteString(this.name);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.id = reader.ReadInt64();
      this.name = reader.ReadString();
    }
  }
}
