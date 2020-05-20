using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GameInvitationHeaderUC : UserControl
  {
      public static readonly DependencyProperty DataProviderProperty = DependencyProperty.Register("DataProvider", typeof(GameRequestHeader), typeof(GameInvitationHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GameInvitationHeaderUC.OnDataProviderChanged)));
      public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register("IsSeparatorVisible", typeof(bool), typeof(GameInvitationHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GameInvitationHeaderUC.OnIsSeparatorVisibleChanged)));
    private bool _isInPlayHandler;
    internal Image UserIconImage;
    internal TextBlock InvitationTextBlock;
    internal Image GameIconImage;
    internal TextBlock GameTitleTextBlock;
    internal TextBlock GameGenreTextBlock;
    internal Rectangle BottomSeparator;
    private bool _contentLoaded;

    public GameRequestHeader DataProvider
    {
      get
      {
        return (GameRequestHeader) base.GetValue(GameInvitationHeaderUC.DataProviderProperty);
      }
      set
      {
        this.SetDPValue(GameInvitationHeaderUC.DataProviderProperty, value, "DataProvider");
      }
    }

    public bool IsSeparatorVisible
    {
      get
      {
        return (bool) base.GetValue(GameInvitationHeaderUC.IsSeparatorVisibleProperty);
      }
      set
      {
        this.SetDPValue(GameInvitationHeaderUC.IsSeparatorVisibleProperty, value, "IsSeparatorVisible");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GameInvitationHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = this;
    }

    private static void OnDataProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameInvitationHeaderUC invitationHeaderUc = d as GameInvitationHeaderUC;
      if (invitationHeaderUc == null)
        return;
      // ISSUE: explicit reference operation
      GameRequestHeader newValue = e.NewValue as GameRequestHeader;
      if (newValue == null)
        return;
      Game game = newValue.Game;
      User user = newValue.User;
      ((FrameworkElement) invitationHeaderUc.UserIconImage).Tag = user;
      ImageLoader.SetUriSource(invitationHeaderUc.UserIconImage, user.photo_max);
      ((FrameworkElement) invitationHeaderUc.GameIconImage).Tag = game;
      ImageLoader.SetUriSource(invitationHeaderUc.GameIconImage, game.icon_150);
      invitationHeaderUc.GameTitleTextBlock.Text = game.title;
      invitationHeaderUc.GameGenreTextBlock.Text = game.genre;
      List<Inline> inlineList = GameInvitationHeaderUC.ComposeInvitationText(user.Name);
      if (((IList) inlineList).IsNullOrEmpty())
        return;
      ((PresentationFrameworkCollection<Inline>) invitationHeaderUc.InvitationTextBlock.Inlines).Clear();
      using (List<Inline>.Enumerator enumerator = inlineList.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          Inline current = enumerator.Current;
          ((PresentationFrameworkCollection<Inline>) invitationHeaderUc.InvitationTextBlock.Inlines).Add(current);
        }
      }
    }

    private static List<Inline> ComposeInvitationText(string userName)
    {
      FontFamily fontFamily1 = new FontFamily("Segoe WP Semilight");
      Brush brush1 = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
      List<Inline> inlineList = new List<Inline>();
      Run run1 = new Run();
      string str1 = userName;
      run1.Text = str1;
      inlineList.Add((Inline) run1);
      Run run2 = new Run();
      string str2 = " " + CommonResources.Games_FriendInvitedToGame;
      run2.Text = str2;
      FontFamily fontFamily2 = fontFamily1;
      ((TextElement) run2).FontFamily = fontFamily2;
      Brush brush2 = brush1;
      ((TextElement) run2).Foreground = brush2;
      inlineList.Add((Inline) run2);
      return inlineList;
    }

    private static void OnIsSeparatorVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameInvitationHeaderUC invitationHeaderUc = d as GameInvitationHeaderUC;
      if (invitationHeaderUc == null)
        return;
      // ISSUE: explicit reference operation
      bool newValue = (bool) e.NewValue;
      ((UIElement) invitationHeaderUc.BottomSeparator).Visibility = (newValue ? Visibility.Visible : Visibility.Collapsed);
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

    private void User_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      User tag = ((FrameworkElement) this.UserIconImage).Tag as User;
      if (tag == null)
        return;
      Navigator.Current.NavigateToUserProfile(tag.uid, tag.Name, "", false);
    }

    private void Game_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Game tag = ((FrameworkElement) this.GameIconImage).Tag as Game;
      if (tag == null)
        return;
      GameRequest gameRequest = this.DataProvider.GameRequest;
      FramePageUtils.CurrentPage.OpenGamesPopup(new List<object>()
      {
        new GameHeader(tag)
      }, GamesClickSource.request, gameRequest.name, 0,  null);
    }

    private async void PlayButton_OnClicked(object sender, RoutedEventArgs e)
    {
      if (this._isInPlayHandler)
        return;
      this._isInPlayHandler = true;
      GameInvitationHeaderUC.HideInvitation(this.DataProvider);
      Game game = this.DataProvider.Game;
      GameRequest gameRequest = this.DataProvider.GameRequest;
      bool flag = InstalledPackagesFinder.Instance.IsPackageInstalled(game.platform_id);
      EventAggregator.Current.Publish(new GamesActionEvent()
      {
        game_id = game.id,
        visit_source = AppGlobalStateManager.Current.GlobalState.GamesVisitSource,
        action_type = (GamesActionType) (flag ? 0 : 1),
        click_source = GamesClickSource.request,
        request_name = gameRequest.name
      });
      await Task.Delay(1000);
      Navigator.Current.OpenGame(game);
      this._isInPlayHandler = false;
    }

    private void HideButton_OnClicked(object sender, RoutedEventArgs e)
    {
      GameInvitationHeaderUC.HideInvitation(this.DataProvider);
    }

    private static void HideInvitation(GameRequestHeader gameRequest)
    {
      AppsService.Instance.DeleteRequest(gameRequest.GameRequest.id, (Action<BackendResult<OwnCounters, ResultCode>>) (result =>
      {
        if (result.ResultCode != ResultCode.Succeeded)
          return;
        CountersManager.Current.Counters = result.ResultData;
        EventAggregator.Current.Publish(new GameInvitationHiddenEvent(gameRequest));
      }));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GameInvitationHeaderUC.xaml", UriKind.Relative));
      this.UserIconImage = (Image) base.FindName("UserIconImage");
      this.InvitationTextBlock = (TextBlock) base.FindName("InvitationTextBlock");
      this.GameIconImage = (Image) base.FindName("GameIconImage");
      this.GameTitleTextBlock = (TextBlock) base.FindName("GameTitleTextBlock");
      this.GameGenreTextBlock = (TextBlock) base.FindName("GameGenreTextBlock");
      this.BottomSeparator = (Rectangle) base.FindName("BottomSeparator");
    }
  }
}
