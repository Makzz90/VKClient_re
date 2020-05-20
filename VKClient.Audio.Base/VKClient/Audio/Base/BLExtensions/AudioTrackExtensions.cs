using Microsoft.Phone.BackgroundAudio;

namespace VKClient.Audio.Base.BLExtensions
{
  public static class AudioTrackExtensions
  {
    public static string GetTagId(this AudioTrack track)
    {
      if (track == null || string.IsNullOrWhiteSpace(track.Tag))
        return "";
      string[] strArray = track.Tag.Split('|');
      if (strArray.Length != 0)
        return strArray[0];
      return "";
    }

    public static long GetTagOwnerId(this AudioTrack track)
    {
      return long.Parse(track.GetTagId().Split('_')[0]);
    }

    public static int GetTagDuration(this AudioTrack track)
    {
      if (track == null || string.IsNullOrWhiteSpace(track.Tag))
        return 0;
      string[] strArray = track.Tag.Split('|');
      int result = 0;
      if (strArray.Length != 0)
        int.TryParse(strArray[1], out result);
      return result;
    }
  }
}
