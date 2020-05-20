using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;

namespace VKClient.Common.Library
{
  public class NewsfeedTopPromoViewModel
  {
    private readonly UserNotificationBubbleNewsfeed _bubbleNewsfeed;

    public string Title
    {
      get
      {
        UserNotificationBubbleNewsfeed notificationBubbleNewsfeed = this._bubbleNewsfeed;
        return (notificationBubbleNewsfeed != null ? notificationBubbleNewsfeed.title : null) ?? "";
      }
    }

    public string Message
    {
      get
      {
        UserNotificationBubbleNewsfeed notificationBubbleNewsfeed = this._bubbleNewsfeed;
        return (notificationBubbleNewsfeed != null ? notificationBubbleNewsfeed.message : null) ?? "";
      }
    }

    public string ButtonPrimaryContent
    {
      get
      {
        UserNotificationBubbleNewsfeed notificationBubbleNewsfeed = this._bubbleNewsfeed;
        string str;
        if (notificationBubbleNewsfeed == null)
        {
          str = null;
        }
        else
        {
          UserNotificationButton button = notificationBubbleNewsfeed.button;
          str = button != null ? button.title : null;
        }
        return str ?? "";
      }
    }

    public string ButtonSecondaryContent
    {
      get
      {
        return CommonResources.Hide;
      }
    }

    public NewsfeedTopPromoViewModel(UserNotificationBubbleNewsfeed bubbleNewsfeed)
    {
      this._bubbleNewsfeed = bubbleNewsfeed;
    }
  }
}
