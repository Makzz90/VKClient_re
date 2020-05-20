using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

namespace VKClient.Groups.UC
{
  public class CommunityInvitationsUC : UserControl
  {
      public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(CommunityInvitations), typeof(CommunityInvitationsUC), new PropertyMetadata(new PropertyChangedCallback(CommunityInvitationsUC.OnModelChanged)));
    internal TextBlock TitleBlock;
    internal Border ShowAllBlock;
    internal ContentControl InvitationView;
    private bool _contentLoaded;

    public CommunityInvitations Model
    {
      get
      {
        return (CommunityInvitations) base.GetValue(CommunityInvitationsUC.ModelProperty);
      }
      set
      {
        base.SetValue(CommunityInvitationsUC.ModelProperty, value);
      }
    }

    public CommunityInvitationsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((CommunityInvitationsUC) d).UpdateDataView((CommunityInvitations) e.NewValue);
    }

    public void UpdateDataView(CommunityInvitations model)
    {
      if (model == null || model.count == 0)
        return;
      this.TitleBlock.Text = (UIStringFormatterHelper.FormatNumberOfSomething(model.count, CommonResources.Communities_InvitationOneFrm, CommonResources.Communities_InvitationTwoFrm, CommonResources.Communities_InvitationFiveFrm, true,  null, false));
      ((UIElement) this.ShowAllBlock).Visibility = (model.count > 1 ? Visibility.Visible : Visibility.Collapsed);
      model.first_invitation.InvitationHandledAction = (Action<CommunityInvitations>) (invitations => ((GroupsListViewModel) base.DataContext).InvitationsViewModel = invitations);
      ContentControl invitationView = this.InvitationView;
      CommunityInvitationUC communityInvitationUc = new CommunityInvitationUC();
      communityInvitationUc.Model = model.first_invitation;
      int num = 0;
      communityInvitationUc.NeedBottomSeparatorLine = num != 0;
      invitationView.Content = communityInvitationUc;
    }

    public void UpdateDataView(CommunityInvitationsList invitationsList)
    {
        CommunityInvitation first_invitation = null;
        if (Enumerable.Any<Group>(invitationsList.invitations))
        {
            User inviter = Enumerable.First<User>(invitationsList.inviters, (User i) => i.id == Enumerable.First<Group>(invitationsList.invitations).invited_by);
            first_invitation = new CommunityInvitation
            {
                community = Enumerable.First<Group>(invitationsList.invitations),
                inviter = inviter
            };
        }
        this.UpdateDataView(new CommunityInvitations
        {
            count = invitationsList.count,
            first_invitation = first_invitation
        });
    }

    private void ShowAllBlock_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.Model == null)
        return;
      ParametersRepository.SetParameterForId("CommunityInvitationsUC", this);
      Navigator.Current.NavigateToGroupInvitations();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/UC/CommunityInvitationsUC.xaml", UriKind.Relative));
      this.TitleBlock = (TextBlock) base.FindName("TitleBlock");
      this.ShowAllBlock = (Border) base.FindName("ShowAllBlock");
      this.InvitationView = (ContentControl) base.FindName("InvitationView");
    }
  }
}
