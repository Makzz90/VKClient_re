using System.Collections.Generic;

namespace VKClient.Audio.Base.DataObjects
{
  public interface IVoiceMessage
  {
    int Duration { get; }

    List<int> Waveform { get; }

    string LinkOgg { get; }
  }
}
