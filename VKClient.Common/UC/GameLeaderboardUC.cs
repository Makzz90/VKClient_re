using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;

namespace VKClient.Common.UC
{
  public class GameLeaderboardUC : UserControl
  {
      public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(List<GameLeaderboardItemHeader>), typeof(GameLeaderboardUC), new PropertyMetadata(new PropertyChangedCallback(GameLeaderboardUC.OnItemsSourceChanged)));
    internal ItemsControl itemsControl;
    private bool _contentLoaded;

    public List<GameLeaderboardItemHeader> ItemsSource
    {
      get
      {
        return (List<GameLeaderboardItemHeader>) base.GetValue(GameLeaderboardUC.ItemsSourceProperty);
      }
      set
      {
        base.SetValue(GameLeaderboardUC.ItemsSourceProperty, value);
      }
    }

    public GameLeaderboardUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameLeaderboardUC gameLeaderboardUc = (GameLeaderboardUC) d;
      // ISSUE: explicit reference operation
      List<GameLeaderboardItemHeader> newValue = e.NewValue as List<GameLeaderboardItemHeader>;
      gameLeaderboardUc.itemsControl.ItemsSource = ( null);
      gameLeaderboardUc.itemsControl.ItemsSource = ((IEnumerable) newValue);
    }

    private void LeaderboardItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      GameLeaderboardItemHeader dataContext = ((FrameworkElement) sender).DataContext as GameLeaderboardItemHeader;
      if (dataContext == null || dataContext.UserId <= 0L)
        return;
      Navigator.Current.NavigateToUserProfile(dataContext.UserId, dataContext.UserName, "", false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GameLeaderboardUC.xaml", UriKind.Relative));
      this.itemsControl = (ItemsControl) base.FindName("itemsControl");
    }
  }
}
