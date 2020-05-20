using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GroupHeaderUC : UserControl
  {
      public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(GroupHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GroupHeaderUC.Title_OnChanged)));
      public static readonly DependencyProperty CounterProperty = DependencyProperty.Register("Counter", typeof(int), typeof(GroupHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GroupHeaderUC.Counter_OnChanged)));
      public static readonly DependencyProperty IsShowAllVisibleProperty = DependencyProperty.Register("IsShowAllVisible", typeof(bool), typeof(GroupHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GroupHeaderUC.IsShowAllVisible_OnChanged)));
      public static readonly DependencyProperty ShowAllTitleProperty = DependencyProperty.Register("ShowAllTitle", typeof(string), typeof(GroupHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GroupHeaderUC.ShowAllTitle_OnChanged)));
    public static readonly DependencyProperty IsTopSeparatorVisibleProperty = DependencyProperty.Register("IsTopSeparatorVisible", typeof(bool), typeof(GroupHeaderUC), new PropertyMetadata(true, new PropertyChangedCallback(GroupHeaderUC.IsTopSeparatorVisible_OnChanged)));
    internal Rectangle rectTopSeparator;
    internal Border gridContainer;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockCounter;
    internal TextBlock textBlockShowAll;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) base.GetValue(GroupHeaderUC.TitleProperty);
      }
      set
      {
        base.SetValue(GroupHeaderUC.TitleProperty, value);
      }
    }

    public int Counter
    {
      get
      {
        return (int) base.GetValue(GroupHeaderUC.CounterProperty);
      }
      set
      {
        base.SetValue(GroupHeaderUC.CounterProperty, value);
      }
    }

    public bool IsShowAllVisible
    {
      get
      {
        return (bool) base.GetValue(GroupHeaderUC.IsShowAllVisibleProperty);
      }
      set
      {
        base.SetValue(GroupHeaderUC.IsShowAllVisibleProperty, value);
      }
    }

    public string ShowAllTitle
    {
      get
      {
        return (string) base.GetValue(GroupHeaderUC.ShowAllTitleProperty);
      }
      set
      {
        base.SetValue(GroupHeaderUC.ShowAllTitleProperty, value);
      }
    }

    public bool IsTopSeparatorVisible
    {
      get
      {
        return (bool) base.GetValue(GroupHeaderUC.IsTopSeparatorVisibleProperty);
      }
      set
      {
        base.SetValue(GroupHeaderUC.IsTopSeparatorVisibleProperty, value);
      }
    }

    public event RoutedEventHandler HeaderTap;/*
    {
      add
      {
        RoutedEventHandler routedEventHandler = this.HeaderTap;
        RoutedEventHandler comparand;
        do
        {
          comparand = routedEventHandler;
          routedEventHandler = Interlocked.CompareExchange<RoutedEventHandler>(ref this.HeaderTap, (RoutedEventHandler) Delegate.Combine((Delegate) comparand, (Delegate) value), comparand);
        }
        while (routedEventHandler != comparand);
      }
      remove
      {
        RoutedEventHandler routedEventHandler = this.HeaderTap;
        RoutedEventHandler comparand;
        do
        {
          comparand = routedEventHandler;
          routedEventHandler = Interlocked.CompareExchange<RoutedEventHandler>(ref this.HeaderTap, (RoutedEventHandler) Delegate.Remove((Delegate) comparand, (Delegate) value), comparand);
        }
        while (routedEventHandler != comparand);
      }
    }*/

    public GroupHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.textBlockTitle.Text = ("");
      this.textBlockCounter.Text = ("");
      ((UIElement) this.textBlockShowAll).Visibility = Visibility.Collapsed;
    }

    private static void Title_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((GroupHeaderUC) d).textBlockTitle.Text = (e.NewValue as string ?? "");
    }

    private static void Counter_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      if (!(e.NewValue is int))
        return;
      // ISSUE: explicit reference operation
      ((GroupHeaderUC) d).textBlockCounter.Text = (UIStringFormatterHelper.FormatForUIShort((long) (int) e.NewValue));
    }

    private static void IsShowAllVisible_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GroupHeaderUC groupHeaderUc = d as GroupHeaderUC;
      if (groupHeaderUc == null)
        return;
      // ISSUE: explicit reference operation
      if ((bool) e.NewValue)
      {
        MetroInMotion.SetTilt((DependencyObject) groupHeaderUc.gridContainer, 1.5);
        ((UIElement) groupHeaderUc.textBlockShowAll).Visibility = Visibility.Visible;
      }
      else
      {
        MetroInMotion.SetTilt((DependencyObject) groupHeaderUc.gridContainer, 0.0);
        ((UIElement) groupHeaderUc.textBlockShowAll).Visibility = Visibility.Collapsed;
      }
    }

    private static void ShowAllTitle_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((GroupHeaderUC) d).textBlockShowAll.Text = (e.NewValue as string ?? "");
    }

    private static void IsTopSeparatorVisible_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((UIElement) ((GroupHeaderUC) d).rectTopSeparator).Visibility = ((bool) e.NewValue ? Visibility.Visible : Visibility.Collapsed);
    }

    private void ShowAll_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.HeaderTap == null || !this.IsShowAllVisible)
        return;
      // ISSUE: reference to a compiler-generated field
      this.HeaderTap.Invoke(this, (RoutedEventArgs) e);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GroupHeaderUC.xaml", UriKind.Relative));
      this.rectTopSeparator = (Rectangle) base.FindName("rectTopSeparator");
      this.gridContainer = (Border) base.FindName("gridContainer");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.textBlockCounter = (TextBlock) base.FindName("textBlockCounter");
      this.textBlockShowAll = (TextBlock) base.FindName("textBlockShowAll");
    }
  }
}
