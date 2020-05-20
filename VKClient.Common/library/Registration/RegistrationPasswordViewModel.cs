using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Registration
{
  public class RegistrationPasswordViewModel : ViewModelBase, ICompleteable, IBinarySerializable
  {
    private string _passwordStr;

    public bool IsCompleted
    {
      get
      {
        return !string.IsNullOrEmpty(this.PasswordStr);
      }
    }

    public string PasswordStr
    {
      get
      {
        return this._passwordStr;
      }
      set
      {
        this._passwordStr = value;
        this.NotifyPropertyChanged<string>(() => this.PasswordStr);
        this.NotifyPropertyChanged<bool>(() => this.IsCompleted);
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteString(this._passwordStr);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this._passwordStr = reader.ReadString();
    }
  }
}
