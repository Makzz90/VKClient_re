using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.Events
{
  public sealed class CommunityManagerChanged
  {
    public long CommunityId { get; set; }

    public long ManagerId { get; set; }

    public EditingMode EditingMode { get; set; }

    public CommunityManagementRole Role { get; set; }

    public bool IsContact { get; set; }

    public string Position { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public User User { get; set; }
  }
}
