using VKClient.Audio.Base.Events;

namespace VKClient.Audio.Base.Library
{
  public static class CurrentMediaSource
  {
    public static StatisticsActionSource AudioSource { get; set; }

    public static StatisticsActionSource VideoSource { get; set; }

    public static string VideoContext { get; set; }

    public static StatisticsActionSource GifPlaySource { get; set; }
  }
}
