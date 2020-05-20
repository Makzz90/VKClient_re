using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GamesInvitesSectionItemUC : UserControl, INotifyPropertyChanged, IHandle<GameInvitationHiddenEvent>, IHandle
  {
      public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(List<GameRequestHeader>), typeof(GamesInvitesSectionItemUC), new PropertyMetadata(new PropertyChangedCallback(GamesInvitesSectionItemUC.OnItemsSourceChanged)));
    public const int MAX_DISPLAYED_ITEMS_COUNT = 2;
    internal GroupHeaderUC HeaderUC;
    internal ItemsControl InvitesListBox;
    internal GroupFooterUC FooterUC;
    private bool _contentLoaded;

    public List<GameRequestHeader> ItemsSource
    {
      get
      {
        return (List<GameRequestHeader>) base.GetValue(GamesInvitesSectionItemUC.ItemsSourceProperty);
      }
      set
      {
        this.SetDPValue(GamesInvitesSectionItemUC.ItemsSourceProperty, value, "ItemsSource");
      }
    }

    public event EventHandler ItemsCleared;

    public event PropertyChangedEventHandler PropertyChanged;

    public GamesInvitesSectionItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = this;
      EventAggregator.Current.Subscribe(this);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesInvitesSectionItemUC invitesSectionItemUc = d as GamesInvitesSectionItemUC;
      // ISSUE: explicit reference operation
      if (invitesSectionItemUc == null || !(e.NewValue is List<GameRequestHeader>))
        return;
      invitesSectionItemUc.UpdateData();
    }

    private void SetDPValue(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
    {
      base.SetValue(property, value);
      // ISSUE: reference to a compiler-generated field
      if (this.PropertyChanged == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    private void UpdateData()
    {
      this.UpdateHeaderTitle();
      this.UpdateFooterVisibility();
      this.RebindItems();
      int count = this.ItemsSource.Count;
      if (count > 2 || count <= 0)
        return;
      if (count == 1)
        this.ItemsSource[0].IsSeparatorVisible = false;
      else
        this.ItemsSource[count - 1].IsSeparatorVisible = false;
    }

    private void UpdateHeaderTitle()
    {
      this.HeaderUC.Title = UIStringFormatterHelper.FormatNumberOfSomething(this.ItemsSource.Count, CommonResources.OneInviteTitleFrm, CommonResources.TwoFourInvitesTitleFrm, CommonResources.FiveInvitesTitleFrm, true,  null, false).ToLowerInvariant();
    }

    public void UpdateFooterVisibility()
    {
      ((UIElement) this.FooterUC).Visibility = (this.ItemsSource.Count > 2 ? Visibility.Visible : Visibility.Collapsed);
    }

    private void RebindItems()
    {
      this.InvitesListBox.ItemsSource = ( null);
      this.InvitesListBox.ItemsSource = ((IEnumerable) Enumerable.ToList<GameRequestHeader>(Enumerable.Take<GameRequestHeader>(this.ItemsSource, 2)));
    }

    private void Footer_OnMoreTapped(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToGamesInvites();
    }

    public void Handle(GameInvitationHiddenEvent message)
    {
      this.RemoveInvitation(message.Invitation);
    }

    private void RemoveInvitation(GameRequestHeader invitation)
    {
      if (this.ItemsSource == null)
        return;
      long invitationId = invitation.GameRequest.id;
      IEnumerator<GameRequestHeader> enumerator = this.ItemsSource.Where<GameRequestHeader>((Func<GameRequestHeader, bool>)(item => item.GameRequest.id == invitationId)).GetEnumerator();
      try
      {
        if (!enumerator.MoveNext())
          return;
        this.ItemsSource.Remove(enumerator.Current);
        if (this.ItemsSource.Count > 0)
        {
          this.UpdateData();
        }
        else
        {
          // ISSUE: reference to a compiler-generated field
          if (this.ItemsCleared == null)
            return;
          // ISSUE: reference to a compiler-generated field
          this.ItemsCleared(this, EventArgs.Empty);
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GamesInvitesSectionItemUC.xaml", UriKind.Relative));
      this.HeaderUC = (GroupHeaderUC) base.FindName("HeaderUC");
      this.InvitesListBox = (ItemsControl) base.FindName("InvitesListBox");
      this.FooterUC = (GroupFooterUC) base.FindName("FooterUC");
    }
  }
}
