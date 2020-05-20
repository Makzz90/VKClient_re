using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GamesMySectionItemUC : UserControl, INotifyPropertyChanged, IHandle<GameRequestReadEvent>, IHandle, IHandle<GameDisconnectedEvent>
  {
      public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<GameHeader>), typeof(GamesMySectionItemUC), new PropertyMetadata(new PropertyChangedCallback(GamesMySectionItemUC.OnItemsSourceChanged)));
    public static readonly DependencyProperty RootProperty = DependencyProperty.Register("Root", typeof (FrameworkElement), typeof (GamesMySectionItemUC), new PropertyMetadata(null));
    private ObservableCollection<GameHeader> _actualItemsSource;
    internal ListBox listBoxGames;
    private bool _contentLoaded;

    public ObservableCollection<GameHeader> ItemsSource
    {
      get
      {
        return (ObservableCollection<GameHeader>) base.GetValue(GamesMySectionItemUC.ItemsSourceProperty);
      }
      set
      {
        this.SetDPValue(GamesMySectionItemUC.ItemsSourceProperty, value, "ItemsSource");
      }
    }

    public FrameworkElement Root
    {
      get
      {
        return (FrameworkElement) base.GetValue(GamesMySectionItemUC.RootProperty);
      }
      set
      {
        base.SetValue(GamesMySectionItemUC.RootProperty, value);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GamesMySectionItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = this;
      EventAggregator.Current.Subscribe(this);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesMySectionItemUC gamesMySectionItemUc = d as GamesMySectionItemUC;
      if (gamesMySectionItemUc == null)
        return;
      // ISSUE: explicit reference operation
      IEnumerable<GameHeader> newValue = e.NewValue as IEnumerable<GameHeader>;
      ((ItemsControl) gamesMySectionItemUc.listBoxGames).ItemsSource = ( null);
      gamesMySectionItemUc._actualItemsSource =  null;
      if (newValue == null)
        return;
      gamesMySectionItemUc._actualItemsSource = new ObservableCollection<GameHeader>((IEnumerable<GameHeader>)Enumerable.OrderByDescending<GameHeader, long>(newValue, (Func<GameHeader, long>)(game => game.LastRequestDate)));
      ((ItemsControl) gamesMySectionItemUc.listBoxGames).ItemsSource = ((IEnumerable) gamesMySectionItemUc._actualItemsSource);
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

    private void Game_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.OpenGame(((Selector) this.listBoxGames).SelectedIndex);
    }

    private void OpenGame(int gameIndex)
    {
      if (gameIndex < 0)
        return;
      FramePageUtils.CurrentPage.OpenGamesPopup(new List<object>((IEnumerable<object>) this._actualItemsSource), GamesClickSource.catalog, "", gameIndex, this.Root);
    }

    private void GroupHeader_OnMoreTapped(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToMyGames();
    }

    public void Handle(GameRequestReadEvent data)
    {
      if (this.ItemsSource == null)
        return;
      IEnumerator<GameHeader> enumerator = ((IEnumerable<GameHeader>)Enumerable.Where<GameHeader>(this.ItemsSource, (Func<GameHeader, bool>)(game =>
      {
        if (game.Game.id == data.GameRequestHeader.Game.id)
          return game.Requests != null;
        return false;
      }))).GetEnumerator();
      try
      {
        if (enumerator.MoveNext())
          enumerator.Current.Requests.Clear();
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
      this._actualItemsSource = new ObservableCollection<GameHeader>((IEnumerable<GameHeader>)Enumerable.OrderByDescending<GameHeader, long>(this._actualItemsSource, (Func<GameHeader, long>)(game => game.LastRequestDate)));
      ((ItemsControl) this.listBoxGames).ItemsSource = ((IEnumerable) this._actualItemsSource);
    }

    public void Handle(GameDisconnectedEvent data)
    {
      if (this.ItemsSource == null)
        return;
      IEnumerator<GameHeader> enumerator = ((IEnumerable<GameHeader>)Enumerable.Where<GameHeader>(this.ItemsSource, (Func<GameHeader, bool>)(game => game.Game.id == data.GameId))).GetEnumerator();
      try
      {
        if (enumerator.MoveNext())
          ((Collection<GameHeader>) this.ItemsSource).Remove(enumerator.Current);
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
      ((ItemsControl) this.listBoxGames).ItemsSource = ( null);
      ((ItemsControl) this.listBoxGames).ItemsSource = ((IEnumerable) this.ItemsSource);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GamesMySectionItemUC.xaml", UriKind.Relative));
      this.listBoxGames = (ListBox) base.FindName("listBoxGames");
    }
  }
}
