using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class LinkToUserOrGroupItem : VirtualizableItemBase
  {
    private long? _userOrGroupId;
    private readonly List<User> _profiles;
    private readonly List<Group> _groups;
    private User _profile;
    private Group _group;
    private string _name;
    private readonly Action _tapCallback;

    public override double FixedHeight
    {
      get
      {
        return !string.IsNullOrEmpty(this._name) ? 40.0 : 0.0;
      }
    }

    public LinkToUserOrGroupItem(double width, Thickness margin, long? userOrGroupId, List<User> profiles, List<Group> groups, Action tapCallback = null)
      : base(width, margin, new Thickness())
    {
      this._userOrGroupId = userOrGroupId;
      this._profiles = profiles;
      this._groups = groups;
      this._tapCallback = tapCallback;
      this.InitializeName();
      this._view.Tap += new EventHandler<GestureEventArgs>(this.View_OnTap);
      this._view.Background = (Brush) new SolidColorBrush(Colors.Transparent);
      MetroInMotion.SetTilt((DependencyObject) this._view, 1.5);
    }

    private void View_OnTap(object sender, GestureEventArgs e)
    {
      if (this._tapCallback != null)
        this._tapCallback();
      else if (this._profile != null)
        Navigator.Current.NavigateToUserProfile(this._profile.uid, this._profile.Name, "", false);
      else
        Navigator.Current.NavigateToGroup(this._group.id, this._group.name, false);
      e.Handled = true;
    }

    private void InitializeName()
    {
      this._name = "";
      if (!this._userOrGroupId.HasValue)
        return;
      if (this._userOrGroupId.Value > 0L)
      {
        User user = this._profiles.FirstOrDefault<User>((Func<User, bool>) (p => p.uid == this._userOrGroupId.Value));
        if (user == null)
          return;
        this._name = user.Name;
        this._profile = user;
      }
      else
      {
        Group group = this._groups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this._userOrGroupId.Value));
        if (group == null || group.name == null)
          return;
        this._name = group.name;
        this._group = group;
      }
    }

    protected override void GenerateChildren()
    {
      if (string.IsNullOrEmpty(this._name))
        return;
      Border border1 = new Border();
      double num1 = 24.0;
      border1.Width = num1;
      double num2 = 24.0;
      border1.Height = num2;
      SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneAttachmentCaptionIconBrush"];
      border1.Background = (Brush) solidColorBrush;
      Border border2 = border1;
      ImageBrush imageBrush = new ImageBrush();
      ImageLoader.SetImageBrushMultiResSource(imageBrush, "/Resources/User24px.png");
      border2.OpacityMask = (Brush) imageBrush;
      Canvas.SetLeft((UIElement) border2, 16.0);
      Canvas.SetTop((UIElement) border2, 8.0);
      this.Children.Add((FrameworkElement) border2);
      TextBlock textBlock = new TextBlock()
      {
        Foreground = (Brush) (Application.Current.Resources["PhoneDarkBlueBrush"] as SolidColorBrush),
        FontFamily = new FontFamily("Segoe WP Semibold"),
        FontSize = 20.0,
        Text = this._name
      };
      Canvas.SetLeft((UIElement) textBlock, 48.0);
      Canvas.SetTop((UIElement) textBlock, 5.0);
      this.Children.Add((FrameworkElement) textBlock);
    }
  }
}
