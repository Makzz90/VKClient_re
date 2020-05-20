using System.Collections.Generic;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public class DocPreviewVoiceMessage : IBinarySerializable, IVoiceMessage
  {
    public int duration { get; set; }

    public List<int> waveform { get; set; }

    public string link_ogg { get; set; }

    public string link_mp3 { get; set; }

    public int Duration
    {
      get
      {
        return this.duration;
      }
    }

    public List<int> Waveform
    {
      get
      {
        return this.waveform;
      }
    }

    public string LinkOgg
    {
      get
      {
        return this.link_ogg;
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.Write(this.duration);
      writer.WriteList(this.waveform);
      writer.WriteString(this.link_ogg);
      writer.WriteString(this.link_mp3);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.duration = reader.ReadInt32();
      this.waveform = reader.ReadListInt();
      this.link_ogg = reader.ReadString();
      this.link_mp3 = reader.ReadString();
    }
  }
}
