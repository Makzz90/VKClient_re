using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.VirtItems
{
  public class ActivityItem : VirtualizableItemBase
  {
    private readonly Dictionary<NewsActivity, VirtualizableItemBase> _createdItems = new Dictionary<NewsActivity, VirtualizableItemBase>();
    private readonly List<NewsActivity> _activityItems;
    private readonly List<User> _users;
    private readonly List<Group> _groups;
    private readonly Action _commentsTapAction;
    private readonly Action _likesTapAction;
    private double _height;

    public override double FixedHeight
    {
      get
      {
        return this._height;
      }
    }

    public ActivityItem(double width, Thickness margin, List<NewsActivity> activityItems, List<User> users, List<Group> groups, Action commentsTapAction, Action likesTapAction)
      : base(width, margin, new Thickness())
    {
      this._activityItems = activityItems;
      this._users = users;
      this._groups = groups;
      this._commentsTapAction = commentsTapAction;
      this._likesTapAction = likesTapAction;
      this.CreateLayout();
    }

    private void CreateLayout()
    {
      double num = 0.0;
      for (int index = 0; index < this._activityItems.Count; ++index)
      {
        NewsActivity activityItem = this._activityItems[index];
        bool flag = index == this._activityItems.Count - 1;
        num += this.CreateActivityItem(activityItem, !flag);
      }
      this._height = num;
    }

    private double CreateActivityItem(NewsActivity activity, bool addSeparator)
    {
        VirtualizableItemBase virtualizableItemBase = (VirtualizableItemBase)null;
        switch (activity.Type)
        {
            case NewsActivityType.likes:
                virtualizableItemBase = this._createdItems.FirstOrDefault<KeyValuePair<NewsActivity, VirtualizableItemBase>>((Func<KeyValuePair<NewsActivity, VirtualizableItemBase>, bool>)(i =>
                {
                    if (i.Key.likes != null)
                        return i.Key.likes == activity.likes;
                    return false;
                })).Value;
                break;
            case NewsActivityType.comment:
                virtualizableItemBase = this._createdItems.FirstOrDefault<KeyValuePair<NewsActivity, VirtualizableItemBase>>((Func<KeyValuePair<NewsActivity, VirtualizableItemBase>, bool>)(i =>
                {
                    if (i.Key.comment != null)
                        return i.Key.comment == activity.comment;
                    return false;
                })).Value;
                break;
        }
        if (virtualizableItemBase == null)
        {
            switch (activity.Type)
            {
                case NewsActivityType.likes:
                    virtualizableItemBase = (VirtualizableItemBase)new UCItem(this.Width, new Thickness(), (Func<UserControlVirtualizable>)(() =>
                    {
                        VKClient.Common.UC.NewsActivityLikesUC newsActivityLikesUc = new VKClient.Common.UC.NewsActivityLikesUC();
                        double width = this.Width;
                        NewsActivityLikes likes = activity.likes;
                        List<User> users = this._users;
                        int num = addSeparator ? 1 : 0;
                        newsActivityLikesUc.Initialize(width, likes, users, num != 0);
                        EventHandler<System.Windows.Input.GestureEventArgs> eventHandler = (EventHandler<System.Windows.Input.GestureEventArgs>)((sender, e) =>
                        {
                            e.Handled=(true);
                            Action likesTapAction = this._likesTapAction;
                            if (likesTapAction == null)
                                return;
                            likesTapAction();
                        });
                        ((UIElement)newsActivityLikesUc).Tap+=(eventHandler);
                        return (UserControlVirtualizable)newsActivityLikesUc;
                    }), (Func<double>)(() => VKClient.Common.UC.NewsActivityLikesUC.CalculateHeight(this.Width, activity.likes, this._users)), null, 0.0, false);
                    break;
                case NewsActivityType.comment:
                    virtualizableItemBase = (VirtualizableItemBase)new UCItem(this.Width, new Thickness(), (Func<UserControlVirtualizable>)(() =>
                    {
                        VKClient.Common.UC.NewsActivityCommentUC activityCommentUc = new VKClient.Common.UC.NewsActivityCommentUC();
                        NewsActivityComment comment = activity.comment;
                        List<User> users = this._users;
                        List<Group> groups = this._groups;
                        int num = addSeparator ? 1 : 0;
                        activityCommentUc.Initialize(comment, (IEnumerable<User>)users, (IEnumerable<Group>)groups, num != 0);
                        EventHandler<System.Windows.Input.GestureEventArgs> eventHandler = (EventHandler<System.Windows.Input.GestureEventArgs>)((sender, e) =>
                        {
                            e.Handled=(true);
                            Action commentsTapAction = this._commentsTapAction;
                            if (commentsTapAction == null)
                                return;
                            commentsTapAction();
                        });
                        ((UIElement)activityCommentUc).Tap+=(eventHandler);
                        return (UserControlVirtualizable)activityCommentUc;
                    }), (Func<double>)(() => VKClient.Common.UC.NewsActivityCommentUC.CalculateHeight(activity.comment)), null, 0.0, false);
                    break;
                default:
                    return 0.0;
            }
            this.VirtualizableChildren.Add((IVirtualizable)virtualizableItemBase);
            if (!this._createdItems.ContainsKey(activity))
                this._createdItems.Add(activity, virtualizableItemBase);
            else
                this._createdItems[activity] = virtualizableItemBase;
        }
        return virtualizableItemBase.FixedHeight;
    }
  }
}
