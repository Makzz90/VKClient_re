using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.Events
{
  public sealed class CommunityBlockChanged
  {
    public long CommunityId { get; set; }

    public User User { get; set; }

    public bool IsEditing { get; set; }
  }
}
