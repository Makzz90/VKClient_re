using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public class DiscussionMediaListItemViewModel : MediaListItemViewModelBase
  {
    private readonly Topic _topic;

    public string Title
    {
      get
      {
        return this._topic.title;
      }
    }

    public string DateStr
    {
      get
      {
        return UIStringFormatterHelper.FormatDateTimeForUI(this._topic.updated);
      }
    }

    public string CommentsCountStr
    {
      get
      {
        return UIStringFormatterHelper.FormatNumberOfSomething(this._topic.comments, CommonResources.PostCommentPage_OneCommentFrm, CommonResources.PostCommentPage_TwoFourCommentsFrm, CommonResources.PostCommentPage_FiveCommentsFrm, true,  null, false);
      }
    }

    public Topic Topic
    {
      get
      {
        return this._topic;
      }
    }

    public override string Id
    {
      get
      {
        return this._topic.ToString();
      }
    }

    public DiscussionMediaListItemViewModel(Topic topic)
      : base(ProfileMediaListItemType.Discussions)
    {
      this._topic = topic;
    }
  }
}
