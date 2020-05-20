using System;
using System.Collections.Generic;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.UC;

namespace VKClient.Common.Library.VirtItems
{
  public class UserNotificationNewsfeedItem : VirtualizableItemBase, IHaveUniqueKey
  {
    private readonly Action _hideCallback;
    private NewsfeedNotificationUC _notificationUC;
    private double _fixedHeight;

    public override double FixedHeight
    {
      get
      {
        return this._fixedHeight;
      }
    }

    public UserNotification UserNotification { get; set; }

    public List<User> Users { get; set; }

    public List<Group> Groups { get; set; }

    public long Id
    {
      get
      {
        return this.UserNotification.id;
      }
    }

    public UserNotificationNewsfeedItem(double width, Thickness margin, UserNotification userNotification, List<User> users, List<Group> groups, Action hideCallback)
      : base(width, margin, new Thickness())
    {
      this.UserNotification = userNotification;
      this.Users = users;
      this.Groups = groups;
      this._hideCallback = hideCallback;
      this.CreateLayout();
    }

    private void CreateLayout()
    {
      if (this._notificationUC == null)
      {
        NewsfeedNotificationUC newsfeedNotificationUc = new NewsfeedNotificationUC();
        double width = this.Width;
        newsfeedNotificationUc.Width = width;
        this._notificationUC = newsfeedNotificationUc;
        this._notificationUC.Initialize(this.UserNotification, this.Users, this.Groups, this.Width, this._hideCallback);
        this.VirtualizableChildren.Add((IVirtualizable) new UCItem(this.Width, this.Margin, (Func<UserControlVirtualizable>) (() => (UserControlVirtualizable) this._notificationUC), (Func<double>) (() => this.FixedHeight), (Action<UserControlVirtualizable>) null, 0.0, false));
      }
      this._fixedHeight = this._notificationUC.CalculateTotalHeight();
    }

    public string GetKey()
    {
      return string.Format("notification{0}", (object) this.Id);
    }
  }
}
