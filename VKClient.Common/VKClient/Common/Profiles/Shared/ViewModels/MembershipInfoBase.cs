using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Profiles.Shared.ViewModels
{
  public abstract class MembershipInfoBase : ViewModelBase
  {
    public const double SECONDARY_BUTTONS_MIN_WIDTH_DEFAULT = 246.0;
    public const double SECONDARY_BUTTONS_MIN_WIDTH_FULL = 480.0;

    public abstract long Id { get; }

    public abstract Visibility InvitedByUserVisibility { get; }

    public abstract string InvitationStr { get; }

    public abstract string InvitedByUserPhoto { get; }

    public abstract string TextButtonInvitationReply { get; }

    public abstract string TextButtonInvitationDecline { get; }

    public abstract Visibility VisibilityButtonSendMessage { get; }

    public abstract Visibility VisibilityButtonPrimary { get; }

    public abstract Visibility VisibilityButtonSecondary { get; }

    public abstract Visibility VisibilityButtonSecondaryAction { get; }

    public abstract double SecondaryButtonsMinWidth { get; }

    public abstract string TextButtonSendMessage { get; }

    public abstract string TextButtonPrimary { get; }

    public abstract string TextButtonSecondary { get; }

    public abstract string TextButtonSecondaryAction { get; }

    public abstract IList<MenuItem> MenuItems { get; }

    public abstract bool SupportMultipleAddActions { get; }

    public abstract Action SecondaryAction { get; }

    public abstract void Add();

    public abstract void Remove();
  }
}
