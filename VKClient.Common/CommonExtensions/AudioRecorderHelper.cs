using Microsoft.Xna.Framework.Audio;

namespace VKClient.Common.CommonExtensions
{
  public static class AudioRecorderHelper
  {
    public static bool CanRecord
    {
      get
      {
        return Microphone.Default != null;
      }
    }
  }
}
