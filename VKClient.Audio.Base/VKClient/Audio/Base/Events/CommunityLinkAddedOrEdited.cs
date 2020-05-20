using VKClient.Audio.Base.DataObjects;

namespace VKClient.Audio.Base.Events
{
  public sealed class CommunityLinkAddedOrEdited
  {
    public long CommunityId { get; set; }

    public GroupLink Link { get; set; }

    public bool IsEditing { get; set; }
  }
}
