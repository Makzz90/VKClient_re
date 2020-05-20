using System.Collections.Generic;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Common.Backend.DataObjects
{
  public class PushNotificationSettings : IBinarySerializable
  {
    public int Disabled { get; set; }

    public double disabled_until { get; set; }

    public IList<int> Conversations { get; set; }

    public PushNotificationSettings()
    {
      this.Disabled = -1;
      this.disabled_until = -1.0;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(this.Disabled);
    }

    public void Read(BinaryReader reader)
    {
      this.Disabled = reader.ReadInt32();
    }
  }
}
