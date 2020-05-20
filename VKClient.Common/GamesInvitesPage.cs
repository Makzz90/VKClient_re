using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class GamesInvitesPage : PageBase, IHandle<GameInvitationHiddenEvent>, IHandle
  {
    private bool _isInitialized;
    internal GenericHeaderUC HeaderUC;
    internal PullToRefreshUC PullToRefreshUC;
    internal ExtendedLongListSelector GamesInvitesListBox;
    private bool _contentLoaded;

    private GamesInvitesViewModel VM
    {
      get
      {
        return base.DataContext as GamesInvitesViewModel;
      }
    }

    public GamesInvitesPage()
    {
      this.InitializeComponent();
      this.HeaderUC.textBlockTitle.Text = (CommonResources.PageTitle_Games_Invites.ToUpperInvariant());
      this.PullToRefreshUC.TrackListBox((ISupportPullToRefresh) this.GamesInvitesListBox);
      this.GamesInvitesListBox.OnRefresh = (Action) (() => this.VM.GamesInvitesVM.LoadData(true, false,  null, false));
      EventAggregator.Current.Subscribe(this);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      GamesInvitesViewModel invitesViewModel = new GamesInvitesViewModel();
      invitesViewModel.GamesInvitesVM.LoadData(false, false,  null, false);
      base.DataContext = invitesViewModel;
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.VM.GamesInvitesVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    public void Handle(GameInvitationHiddenEvent message)
    {
      long invitationId = message.Invitation.GameRequest.id;
      IEnumerator<GameRequestHeader> enumerator = ((IEnumerable<GameRequestHeader>)Enumerable.Where<GameRequestHeader>(this.VM.GamesInvitesVM.Collection, (Func<GameRequestHeader, bool>)(invitation => invitation.GameRequest.id == invitationId))).GetEnumerator();
      try
      {
        if (((IEnumerator) enumerator).MoveNext())
          this.VM.GamesInvitesVM.Delete(enumerator.Current);
      }
      finally
      {
        if (enumerator != null)
          ((IDisposable) enumerator).Dispose();
      }
      if (((Collection<GameRequestHeader>) this.VM.GamesInvitesVM.Collection).Count != 0)
        return;
      Navigator.Current.GoBack();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/GamesInvitesPage.xaml", UriKind.Relative));
      this.HeaderUC = (GenericHeaderUC) base.FindName("HeaderUC");
      this.PullToRefreshUC = (PullToRefreshUC) base.FindName("PullToRefreshUC");
      this.GamesInvitesListBox = (ExtendedLongListSelector) base.FindName("GamesInvitesListBox");
    }
  }
}
