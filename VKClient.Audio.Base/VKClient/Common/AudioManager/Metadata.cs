using System;
using System.IO;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;

namespace VKClient.Common.AudioManager
{
  public class Metadata : IBinarySerializable
  {
    public DateTime LastUpdated { get; set; }

    public StatisticsActionSource ActionSource { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(2);
      writer.Write(this.LastUpdated);
      writer.Write((int) this.ActionSource);
    }

    public void Read(BinaryReader reader)
    {
      int num1 = reader.ReadInt32();
      this.LastUpdated = reader.ReadDateTime();
      int num2 = 2;
      if (num1 < num2)
        return;
      this.ActionSource = (StatisticsActionSource) reader.ReadInt32();
    }
  }
}
