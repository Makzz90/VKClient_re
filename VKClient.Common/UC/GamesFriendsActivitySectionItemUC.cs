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

namespace VKClient.Common.UC
{
  public class GamesFriendsActivitySectionItemUC : UserControl, INotifyPropertyChanged
  {
      public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(List<GameActivityHeader>), typeof(GamesFriendsActivitySectionItemUC), new PropertyMetadata(new PropertyChangedCallback(GamesFriendsActivitySectionItemUC.OnItemsSourceChanged)));
    public static readonly DependencyProperty MaxDisplayedItemsCountProperty = DependencyProperty.Register("MaxDisplayedItemsCount", typeof (int), typeof (GamesFriendsActivitySectionItemUC), new PropertyMetadata(3));
    public static readonly DependencyProperty GameIdProperty = DependencyProperty.Register("GameId", typeof (int), typeof (GamesFriendsActivitySectionItemUC), new PropertyMetadata(0));
    public static readonly DependencyProperty GameNameProperty = DependencyProperty.Register("GameName", typeof (string), typeof (GamesFriendsActivitySectionItemUC), new PropertyMetadata(null));
    public static readonly DependencyProperty ItemsDisplayTypeProperty = DependencyProperty.Register("ItemsDisplayType", typeof(FriendsActivitySectionItemType), typeof(GamesFriendsActivitySectionItemUC), new PropertyMetadata(new PropertyChangedCallback(GamesFriendsActivitySectionItemUC.OnItemsDisplayTypeChanged)));
    internal ItemsControl listBoxActivity;
    internal GroupFooterUC ucFooter;
    private bool _contentLoaded;

    public List<GameActivityHeader> ItemsSource
    {
      get
      {
        return (List<GameActivityHeader>) base.GetValue(GamesFriendsActivitySectionItemUC.ItemsSourceProperty);
      }
      set
      {
        this.SetDPValue(GamesFriendsActivitySectionItemUC.ItemsSourceProperty, value, "ItemsSource");
      }
    }

    public int MaxDisplayedItemsCount
    {
      get
      {
        return (int) base.GetValue(GamesFriendsActivitySectionItemUC.MaxDisplayedItemsCountProperty);
      }
      set
      {
        this.SetDPValue(GamesFriendsActivitySectionItemUC.MaxDisplayedItemsCountProperty, value, "MaxDisplayedItemsCount");
      }
    }

    public int GameId
    {
      get
      {
        return (int) base.GetValue(GamesFriendsActivitySectionItemUC.GameIdProperty);
      }
      set
      {
        base.SetValue(GamesFriendsActivitySectionItemUC.GameIdProperty, value);
      }
    }

    public string GameName
    {
      get
      {
        return (string) base.GetValue(GamesFriendsActivitySectionItemUC.GameNameProperty);
      }
      set
      {
        base.SetValue(GamesFriendsActivitySectionItemUC.GameNameProperty, value);
      }
    }

    public FriendsActivitySectionItemType ItemsDisplayType
    {
      get
      {
        return (FriendsActivitySectionItemType) base.GetValue(GamesFriendsActivitySectionItemUC.ItemsDisplayTypeProperty);
      }
      set
      {
        base.SetValue(GamesFriendsActivitySectionItemUC.ItemsDisplayTypeProperty, value);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GamesFriendsActivitySectionItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = this;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesFriendsActivitySectionItemUC activitySectionItemUc = d as GamesFriendsActivitySectionItemUC;
      // ISSUE: explicit reference operation
      if (activitySectionItemUc == null || !(e.NewValue is List<GameActivityHeader>))
        return;
      activitySectionItemUc.UpdateData();
    }

    private static void OnItemsDisplayTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesFriendsActivitySectionItemUC activitySectionItemUc = d as GamesFriendsActivitySectionItemUC;
      FriendsActivitySectionItemType result;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (activitySectionItemUc == null || e.NewValue == null || !Enum.TryParse<FriendsActivitySectionItemType>(e.NewValue.ToString(), out result))
        return;
      activitySectionItemUc.UpdateItemType(result);
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

    private void UpdateItemType(FriendsActivitySectionItemType itemType)
    {
      if (itemType != FriendsActivitySectionItemType.Full)
      {
        if (itemType != FriendsActivitySectionItemType.Short)
          return;
        this.listBoxActivity.ItemTemplate = ((DataTemplate) base.Resources["ShortItemTemplate"]);
      }
      else
        this.listBoxActivity.ItemTemplate = ((DataTemplate) base.Resources["FullItemTemplate"]);
    }

    private void UpdateData()
    {
      this.UpdateFooterVisibility();
      this.RebindItems();
      int count = this.ItemsSource.Count;
      if (count > this.MaxDisplayedItemsCount || count <= 0)
        return;
      if (count == 1)
        this.ItemsSource[0].IsSeparatorVisible = false;
      else
        this.ItemsSource[count - 1].IsSeparatorVisible = false;
    }

    public void UpdateFooterVisibility()
    {
      ((UIElement) this.ucFooter).Visibility = (this.ItemsSource.Count > this.MaxDisplayedItemsCount ? Visibility.Visible : Visibility.Collapsed);
    }

    private void RebindItems()
    {
      this.listBoxActivity.ItemsSource = ( null);
      this.listBoxActivity.ItemsSource = ((IEnumerable) Enumerable.ToList<GameActivityHeader>(Enumerable.Take<GameActivityHeader>(this.ItemsSource, this.MaxDisplayedItemsCount)));
    }

    private void Footer_OnMoreTapped(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToGamesFriendsActivity((long) this.GameId, this.GameName);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GamesFriendsActivitySectionItemUC.xaml", UriKind.Relative));
      this.listBoxActivity = (ItemsControl) base.FindName("listBoxActivity");
      this.ucFooter = (GroupFooterUC) base.FindName("ucFooter");
    }
  }
}
