using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.UC
{
  public class BirthdayInfo
  {
    public User friend;

    public string ImageSrc { get; set; }

    public string Title { get; set; }

    public string Subtitle { get; set; }

    public BirthdayInfo(User friend, string subtitle)
    {
      this.friend = friend;
      this.ImageSrc = friend.photo_max;
      this.Title = friend.Name;
      this.Subtitle = subtitle;
    }
  }
}
