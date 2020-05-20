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
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GamesFriendsActivityShortHeaderUC : UserControl, INotifyPropertyChanged
  {
      public static readonly DependencyProperty DataProviderProperty = DependencyProperty.Register("DataProvider", typeof(GameActivityHeader), typeof(GamesFriendsActivityShortHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GamesFriendsActivityShortHeaderUC.OnDataProviderChanged)));
      public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register("IsSeparatorVisible", typeof(bool), typeof(GamesFriendsActivityShortHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GamesFriendsActivityShortHeaderUC.OnIsSeparatorVisibleChanged)));
    internal Image imageUserIcon;
    internal TextBlock textBlockDescription;
    internal TextBlock textBlockDate;
    internal Rectangle BottomSeparator;
    private bool _contentLoaded;

    public GameActivityHeader DataProvider
    {
      get
      {
        return (GameActivityHeader) base.GetValue(GamesFriendsActivityShortHeaderUC.DataProviderProperty);
      }
      set
      {
        this.SetDPValue(GamesFriendsActivityShortHeaderUC.DataProviderProperty, value, "DataProvider");
      }
    }

    public bool IsSeparatorVisible
    {
      get
      {
        return (bool) base.GetValue(GamesFriendsActivityShortHeaderUC.IsSeparatorVisibleProperty);
      }
      set
      {
        this.SetDPValue(GamesFriendsActivityShortHeaderUC.IsSeparatorVisibleProperty, value, "IsSeparatorVisible");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GamesFriendsActivityShortHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = this;
    }

    private static void OnDataProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesFriendsActivityShortHeaderUC activityShortHeaderUc = d as GamesFriendsActivityShortHeaderUC;
      if (activityShortHeaderUc == null)
        return;
      // ISSUE: explicit reference operation
      GameActivityHeader newValue = e.NewValue as GameActivityHeader;
      if (newValue == null)
        return;
      ((FrameworkElement) activityShortHeaderUc.imageUserIcon).Tag = newValue.User;
      ImageLoader.SetUriSource(activityShortHeaderUc.imageUserIcon, newValue.User.photo_max);
      activityShortHeaderUc.textBlockDate.Text = (UIStringFormatterHelper.FormatDateTimeForUI(newValue.GameActivity.date));
      ((FrameworkElement) activityShortHeaderUc.textBlockDescription).Tag = newValue.Game;
      ((PresentationFrameworkCollection<Inline>) activityShortHeaderUc.textBlockDescription.Inlines).Clear();
      List<Inline> inlineList = newValue.ComposeActivityText(false);
      if (((IList) inlineList).IsNullOrEmpty())
        return;
      for (int index = 0; index < inlineList.Count; ++index)
      {
        Run run1 = inlineList[index] as Run;
        if (run1 != null)
        {
          ((PresentationFrameworkCollection<Inline>) activityShortHeaderUc.textBlockDescription.Inlines).Add((Inline) run1);
          if (index < inlineList.Count - 1)
          {
            Run run2 = run1;
            string str = run2.Text + " ";
            run2.Text = str;
          }
        }
      }
    }

    private static void OnIsSeparatorVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesFriendsActivityShortHeaderUC activityShortHeaderUc = d as GamesFriendsActivityShortHeaderUC;
      if (activityShortHeaderUc == null)
        return;
      // ISSUE: explicit reference operation
      bool newValue = (bool) e.NewValue;
      ((UIElement) activityShortHeaderUc.BottomSeparator).Visibility = (newValue ? Visibility.Visible : Visibility.Collapsed);
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
      User tag = ((FrameworkElement) this.imageUserIcon).Tag as User;
      if (tag == null)
        return;
      Navigator.Current.NavigateToUserProfile(tag.uid, tag.Name, "", false);
    }

    private void Description_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      User tag = ((FrameworkElement) this.imageUserIcon).Tag as User;
      if (tag == null)
        return;
      Navigator.Current.NavigateToUserProfile(tag.uid, tag.Name, "", false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GamesFriendsActivityShortHeaderUC.xaml", UriKind.Relative));
      this.imageUserIcon = (Image) base.FindName("imageUserIcon");
      this.textBlockDescription = (TextBlock) base.FindName("textBlockDescription");
      this.textBlockDate = (TextBlock) base.FindName("textBlockDate");
      this.BottomSeparator = (Rectangle) base.FindName("BottomSeparator");
    }
  }
}
