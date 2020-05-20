using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Events;
using VKClient.Common.Library.Games;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GamesCatalogBannersContainer : UserControl
  {
      public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(List<GameHeader>), typeof(GamesCatalogBannersContainer), new PropertyMetadata(new PropertyChangedCallback(GamesCatalogBannersContainer.OnItemsSourceChanged)));
    internal StackPanel panelCatalogBanners;
    internal GamesCatalogBannersSlideView slideView;
    internal GroupHeaderUC groupHeader;
    private bool _contentLoaded;

    public List<GameHeader> ItemsSource
    {
      get
      {
        return (List<GameHeader>) base.GetValue(GamesCatalogBannersContainer.ItemsSourceProperty);
      }
      set
      {
        base.SetValue(GamesCatalogBannersContainer.ItemsSourceProperty, value);
      }
    }

    public GamesCatalogBannersContainer()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((UIElement) this.panelCatalogBanners).Visibility = Visibility.Collapsed;
      this.slideView.CreateSingleElement = (Func<Control>) (() => (Control) new CatalogBannerUC());
      this.slideView.NextElementSwipeDelay = TimeSpan.FromSeconds(5.0);
      this.slideView.IsCycled = true;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesCatalogBannersContainer bannersContainer = d as GamesCatalogBannersContainer;
      if (bannersContainer == null)
        return;
      // ISSUE: explicit reference operation
      List<GameHeader> newValue = e.NewValue as List<GameHeader>;
      if (newValue == null)
      {
        ((UIElement) bannersContainer.panelCatalogBanners).Visibility = Visibility.Collapsed;
        ((UIElement) bannersContainer.groupHeader).Visibility = Visibility.Visible;
      }
      else
      {
        ((UIElement) bannersContainer.groupHeader).Visibility = Visibility.Collapsed;
        ((UIElement) bannersContainer.panelCatalogBanners).Visibility = Visibility.Visible;
        bannersContainer.slideView.Items = new ObservableCollection<object>((IEnumerable<object>) newValue);
      }
    }

    private void BorderCatalog_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      GameHeader currentGame = this.slideView.GetCurrentGame();
      if (currentGame == null || this.ItemsSource == null)
        return;
      FramePageUtils.CurrentPage.OpenGamesPopup((List<object>) Enumerable.ToList<object>(Enumerable.Cast<object>((IEnumerable) this.ItemsSource)), GamesClickSource.catalog, "", this.ItemsSource.IndexOf(currentGame),  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GamesCatalogBannersContainer.xaml", UriKind.Relative));
      this.panelCatalogBanners = (StackPanel) base.FindName("panelCatalogBanners");
      this.slideView = (GamesCatalogBannersSlideView) base.FindName("slideView");
      this.groupHeader = (GroupHeaderUC) base.FindName("groupHeader");
    }
  }
}
