using System.Collections.Generic;
using Windows.Storage;

namespace VKClient.Audio.Base.Events
{
  public class VoiceMessageSentEvent
  {
      public StorageFile File { get; private set; }

      public int Duration { get; private set; }

      public List<int> Waveform { get; private set; }

    public VoiceMessageSentEvent(StorageFile file, int duration, List<int> waveform)
    {
      this.File = file;
      this.Duration = duration;
      this.Waveform = waveform;
    }
  }
}
