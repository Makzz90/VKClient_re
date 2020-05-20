using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GamesFriendsActivityHeaderUC : UserControl, INotifyPropertyChanged
  {
      public static readonly DependencyProperty DataProviderProperty = DependencyProperty.Register("DataProvider", typeof(GameActivityHeader), typeof(GamesFriendsActivityHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GamesFriendsActivityHeaderUC.OnDataProviderChanged)));
      public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register("IsSeparatorVisible", typeof(bool), typeof(GamesFriendsActivityHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GamesFriendsActivityHeaderUC.OnIsSeparatorVisibleChanged)));
    internal Image imageUser;
    internal TextBlock textBlockDescription;
    internal Image imageGame;
    internal TextBlock textBlockDate;
    internal Rectangle rectSeparator;
    private bool _contentLoaded;

    public GameActivityHeader DataProvider
    {
      get
      {
        return (GameActivityHeader) base.GetValue(GamesFriendsActivityHeaderUC.DataProviderProperty);
      }
      set
      {
        this.SetDPValue(GamesFriendsActivityHeaderUC.DataProviderProperty, value, "DataProvider");
      }
    }

    public bool IsSeparatorVisible
    {
      get
      {
        return (bool) base.GetValue(GamesFriendsActivityHeaderUC.IsSeparatorVisibleProperty);
      }
      set
      {
        this.SetDPValue(GamesFriendsActivityHeaderUC.IsSeparatorVisibleProperty, value, "IsSeparatorVisible");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GamesFriendsActivityHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = this;
    }

    private static void OnDataProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesFriendsActivityHeaderUC activityHeaderUc = d as GamesFriendsActivityHeaderUC;
      if (activityHeaderUc == null)
        return;
      // ISSUE: explicit reference operation
      GameActivityHeader newValue = e.NewValue as GameActivityHeader;
      if (newValue == null)
        return;
      ImageLoader.SetUriSource(activityHeaderUc.imageUser, newValue.User.photo_max);
      ImageLoader.SetUriSource(activityHeaderUc.imageGame, newValue.Game.icon_150);
      ((PresentationFrameworkCollection<Inline>) activityHeaderUc.textBlockDescription.Inlines).Clear();
      List<Inline> inlineList = newValue.ComposeActivityText(true);
      if (!((IList) inlineList).IsNullOrEmpty())
      {
        for (int index = 0; index < inlineList.Count; ++index)
        {
          Run run1 = inlineList[index] as Run;
          if (run1 != null)
          {
            ((PresentationFrameworkCollection<Inline>) activityHeaderUc.textBlockDescription.Inlines).Add((Inline) run1);
            if (index < inlineList.Count - 1)
            {
              Run run2 = run1;
              string str = run2.Text + " ";
              run2.Text = str;
            }
          }
        }
      }
      activityHeaderUc.textBlockDate.Text = (UIStringFormatterHelper.FormatDateTimeForUI(newValue.GameActivity.date));
    }

    private static void OnIsSeparatorVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesFriendsActivityHeaderUC activityHeaderUc = d as GamesFriendsActivityHeaderUC;
      if (activityHeaderUc == null)
        return;
      // ISSUE: explicit reference operation
      bool newValue = (bool) e.NewValue;
      ((UIElement) activityHeaderUc.rectSeparator).Visibility = (newValue ? Visibility.Visible : Visibility.Collapsed);
    }

    private void SetDPValue(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
    {
      base.SetValue(property, value);
      // ISSUE: reference to a compiler-generated field
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
      propertyChanged(this, e);
    }

    private void User_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      User user = this.DataProvider.User;
      if (user == null)
        return;
      Navigator.Current.NavigateToUserProfile(user.uid, user.Name, "", false);
    }

    private void Game_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      GamesFriendsActivityHeaderUC.OpenGame(this.DataProvider.Game);
    }

    private void Description_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      GamesFriendsActivityHeaderUC.OpenGame(this.DataProvider.Game);
    }

    private static void OpenGame(Game game)
    {
      if (game == null)
        return;
      FramePageUtils.CurrentPage.OpenGamesPopup(new List<object>()
      {
        new GameHeader(game)
      }, GamesClickSource.activity, "", 0,  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GamesFriendsActivityHeaderUC.xaml", UriKind.Relative));
      this.imageUser = (Image) base.FindName("imageUser");
      this.textBlockDescription = (TextBlock) base.FindName("textBlockDescription");
      this.imageGame = (Image) base.FindName("imageGame");
      this.textBlockDate = (TextBlock) base.FindName("textBlockDate");
      this.rectSeparator = (Rectangle) base.FindName("rectSeparator");
    }
  }
}
