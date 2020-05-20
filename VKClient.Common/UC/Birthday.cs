using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;

namespace VKClient.Common.UC
{
  public class Birthday
  {
      public User User { get; private set; }

    public long UserId
    {
      get
      {
        return this.User.id;
      }
    }

    public string UserPhoto
    {
      get
      {
        return this.User.photo_max;
      }
    }

    public string UserName
    {
      get
      {
        return this.User.Name;
      }
    }

    public string Description { get; private set; }

    public Visibility DescriptionVisibility
    {
      get
      {
        return (!string.IsNullOrEmpty(this.Description)).ToVisiblity();
      }
    }

    public Visibility GiftVisibility { get; private set; }

    public Birthday(User user, string subtitle = "", bool canSendGift = false)
    {
      this.User = user;
      this.Description = subtitle;
      this.GiftVisibility = canSendGift.ToVisiblity();
    }
  }
}
