using System;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;
using VKClient.Groups.Localization;

namespace VKClient.Groups.Library
{
  public class ThemeHeader
  {
    private Topic _topic;
    private User _user;
    private Group _group;

    public Topic Topic
    {
      get
      {
        return this._topic;
      }
    }

    public string Header
    {
      get
      {
        return this._topic.title;
      }
    }

    public string MessagesCountStr
    {
      get
      {
        return UIStringFormatterHelper.FormatNumberOfSomething(this._topic.comments, GroupResources.OneMessageFrm, GroupResources.TwoFourMessagesFrm, GroupResources.FiveMessagesFrm, true,  null, false);
      }
    }

    public string ImageSrc
    {
      get
      {
        if (this._user != null)
          return this._user.photo_max;
        if (this._group != null)
          return this._group.photo_200;
        return "";
      }
    }

    public string Name
    {
      get
      {
        if (this._user != null)
          return this._user.Name;
        if (this._group != null)
          return this._group.name;
        return "";
      }
    }

    public string Text
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this._topic.last_comment))
          return UIStringFormatterHelper.SubstituteMentionsWithNames(this._topic.last_comment.Replace(Environment.NewLine, " "));
        return "...";
      }
    }

    public string Date
    {
      get
      {
        return UIStringFormatterHelper.FormatDateTimeForUI(this._topic.updated);
      }
    }

    public ThemeHeader(Topic t, User user, Group group)
    {
      this._topic = t;
      this._user = user;
      this._group = group;
    }
  }
}
