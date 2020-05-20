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
        UserNotificationBubbleNewsfeed bubbleNewsfeed = this._bubbleNewsfeed;
        return (bubbleNewsfeed != null ? bubbleNewsfeed.title :  null) ?? "";
      }
    }

    public string Message
    {
      get
      {
        UserNotificationBubbleNewsfeed bubbleNewsfeed = this._bubbleNewsfeed;
        return (bubbleNewsfeed != null ? bubbleNewsfeed.message :  null) ?? "";
      }
    }

    public string ButtonPrimaryContent
    {
      get
      {
        UserNotificationBubbleNewsfeed bubbleNewsfeed = this._bubbleNewsfeed;
        string str;
        if (bubbleNewsfeed == null)
        {
          str =  null;
        }
        else
        {
          UserNotificationButton button = bubbleNewsfeed.button;
          str = button != null ? button.title :  null;
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
