namespace VKClient.Audio.Localization
{
  public class AudioLocalizedStrings
  {
    private static AudioResources _localizedResources = new AudioResources();

    public AudioResources LocalizedResources
    {
      get
      {
        return AudioLocalizedStrings._localizedResources;
      }
    }
  }
}
