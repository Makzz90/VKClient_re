namespace VKClient.Audio.Base.Events
{
  public sealed class CommunityPhotoChanged
  {
    public long CommunityId { get; set; }

    public string PhotoMax { get; set; }
  }
}
