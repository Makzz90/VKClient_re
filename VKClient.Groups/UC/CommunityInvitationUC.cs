using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Groups.UC
{
  public class CommunityInvitationUC : UserControl
  {
      public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(CommunityInvitation), typeof(CommunityInvitationUC), new PropertyMetadata(new PropertyChangedCallback(CommunityInvitationUC.OnModelChanged)));
      public static readonly DependencyProperty NeedBottomSeparatorLineProperty = DependencyProperty.Register("NeedBottomSeparatorLine", typeof(bool), typeof(CommunityInvitationUC), new PropertyMetadata(new PropertyChangedCallback(CommunityInvitationUC.OnNeedBottomSeparatorLineChanged)));
    private CommunityInvitation _model;
    internal Image InvitationPhoto;
    internal TextBlock InvitationName;
    internal TextBlock InvitationMembersCount;
    internal TextBlock InvitationInviterSex;
    internal TextBlock InvitationInviterName;
    internal Button JoinButton;
    internal Button HideButton;
    internal Rectangle BottomSeparatorRectangle;
    private bool _contentLoaded;

    public CommunityInvitation Model
    {
      get
      {
        return (CommunityInvitation) base.GetValue(CommunityInvitationUC.ModelProperty);
      }
      set
      {
        base.SetValue(CommunityInvitationUC.ModelProperty, value);
      }
    }

    public bool NeedBottomSeparatorLine
    {
      get
      {
        return (bool) base.GetValue(CommunityInvitationUC.NeedBottomSeparatorLineProperty);
      }
      set
      {
        base.SetValue(CommunityInvitationUC.NeedBottomSeparatorLineProperty, value);
      }
    }

    public CommunityInvitationUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((CommunityInvitationUC) d).UpdateDataView((CommunityInvitation) e.NewValue);
    }

    private static void OnNeedBottomSeparatorLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((UIElement) ((CommunityInvitationUC) d).BottomSeparatorRectangle).Visibility = ((bool) e.NewValue ? Visibility.Visible : Visibility.Collapsed);
    }

    private void UpdateDataView(CommunityInvitation model)
    {
      this._model = model;
      this.InvitationName.Text = model.community.name;
      ImageLoader.SetUriSource(this.InvitationPhoto, model.community.photo_100);
      int membersCount = model.community.members_count;
      if (model.community.type != "public")
        this.InvitationMembersCount.Text = (UIStringFormatterHelper.FormatNumberOfSomething(membersCount, CommonResources.OneMemberFrm, CommonResources.TwoFourMembersFrm, CommonResources.FiveMembersFrm, true,  null, false));
      else
        this.InvitationMembersCount.Text = (UIStringFormatterHelper.FormatNumberOfSomething(membersCount, CommonResources.OneSubscriberFrm, CommonResources.TwoFourSubscribersFrm, CommonResources.FiveSubscribersFrm, true,  null, false));
      this.InvitationInviterSex.Text = ((model.inviter.sex != 1 ? CommonResources.Communities_InvitationByM : CommonResources.Communities_InvitationByF) + " ");
      this.InvitationInviterName.Text = model.inviter.Name;
    }

    private void Invitation_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Group community = this._model.community;
      Navigator.Current.NavigateToGroup(community.id, community.name, false);
    }

    private void InvitationInviterName_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      Navigator.Current.NavigateToUserProfile(this._model.inviter.id, "", "", false);
    }

    private void Button_OnClicked(object sender, RoutedEventArgs e)
    {
        string text = string.Format("\r\n\r\nvar result=API.groups.{0}({{group_id:{1}}});;\r\nif (result==1)\r\n{{\r\n    var invitations=API.groups.getInvites({{count:1,\"fields\":\"members_count\"}});\r\n\r\n    var first_invitation_community=null;\r\n    var first_invitation_inviter=null;\r\n\r\n    if (invitations.items.length>0)\r\n    {{\r\n        first_invitation_community=invitations.items[0];\r\n        first_invitation_inviter=API.users.get({{user_ids:first_invitation_community.invited_by,fields:\"sex\"}})[0];\r\n    }}\r\n\r\n    return\r\n    {{\r\n        \"count\":invitations.count,\r\n        \"first_invitation\":\r\n        {{\r\n            \"community\":first_invitation_community,\r\n            \"inviter\":first_invitation_inviter\r\n        }}\r\n    }};\r\n}}\r\nreturn 0;\r\n\r\n", (sender == this.JoinButton) ? "join" : "leave", this._model.community.id);
        CommunityInvitation model = this.Model;
        Action<BackendResult<CommunityInvitations, ResultCode>> callback = delegate(BackendResult<CommunityInvitations, ResultCode> result)
        {
            Execute.ExecuteOnUIThread(delegate
            {
                if (result.ResultCode == ResultCode.Succeeded)
                {
                    CommunityInvitations resultData = result.ResultData;
                    model.InvitationHandledAction.Invoke(resultData);
                    CountersManager.Current.Counters.groups = resultData.count;
                    EventAggregator.Current.Publish(new CountersChanged(CountersManager.Current.Counters));
                }
                PageBase.SetInProgress(false);
            });
        };
        PageBase.SetInProgress(true);
        string arg_7C_0 = "execute";
        Dictionary<string, string> expr_62 = new Dictionary<string, string>();
        expr_62.Add("code", text);
        VKRequestsDispatcher.DispatchRequestToVK<CommunityInvitations>(arg_7C_0, expr_62, callback, null, false, true, default(CancellationToken?), null);
    }


    private void InviterName_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    private void Button_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/UC/CommunityInvitationUC.xaml", UriKind.Relative));
      this.InvitationPhoto = (Image) base.FindName("InvitationPhoto");
      this.InvitationName = (TextBlock) base.FindName("InvitationName");
      this.InvitationMembersCount = (TextBlock) base.FindName("InvitationMembersCount");
      this.InvitationInviterSex = (TextBlock) base.FindName("InvitationInviterSex");
      this.InvitationInviterName = (TextBlock) base.FindName("InvitationInviterName");
      this.JoinButton = (Button) base.FindName("JoinButton");
      this.HideButton = (Button) base.FindName("HideButton");
      this.BottomSeparatorRectangle = (Rectangle) base.FindName("BottomSeparatorRectangle");
    }
  }
}
