using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class BirthdayRecordListItemUC : UserControl
  {
      public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register("UserName", typeof(string), typeof(BirthdayRecordListItemUC), new PropertyMetadata(new PropertyChangedCallback((d, e) => ((BirthdayRecordListItemUC)d).UpdateUserName())));
      public static readonly DependencyProperty GiftVisibilityProperty = DependencyProperty.Register("GiftVisibility", typeof(Visibility), typeof(BirthdayRecordListItemUC), new PropertyMetadata((object)Visibility.Collapsed, new PropertyChangedCallback((d, e) => ((BirthdayRecordListItemUC)d).UpdateGiftVisibility())));
      private const int USER_NAME_TOTAL_WIDTH = 372;
    private const int EXTRA_TEXT_MARGIN = 8;
    internal TextBlock textBlockUserName;
    internal Border borderGift;
    private bool _contentLoaded;

    public string UserName
    {
      get
      {
        return (string) base.GetValue(BirthdayRecordListItemUC.UserNameProperty);
      }
      set
      {
        base.SetValue(BirthdayRecordListItemUC.UserNameProperty, value);
      }
    }

    public Visibility GiftVisibility
    {
      get
      {
        return (Visibility) base.GetValue(BirthdayRecordListItemUC.GiftVisibilityProperty);
      }
      set
      {
        base.SetValue(BirthdayRecordListItemUC.GiftVisibilityProperty, value);
      }
    }

    public BirthdayRecordListItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((UIElement) this.borderGift).Visibility = Visibility.Collapsed;
    }

    private void UpdateUserName()
    {
      this.textBlockUserName.Text = this.UserName;
      double maxWidth = 372.0;
      if (this.GiftVisibility == Visibility.Visible)
        maxWidth = maxWidth - ((FrameworkElement) this.borderGift).Width + 8.0;
      this.textBlockUserName.CorrectText(maxWidth);
    }

    private void UpdateGiftVisibility()
    {
      ((UIElement) this.borderGift).Visibility = this.GiftVisibility;
      this.UpdateUserName();
    }

    private void Gift_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      Birthday birthday = (frameworkElement != null ? frameworkElement.DataContext : null) as Birthday;
      if (birthday == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.birthdays, GiftPurchaseStepsAction.store));
      Navigator.Current.NavigateToGiftsCatalog(birthday.UserId, false);
    }

    private void User_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      Birthday birthday = (frameworkElement != null ? frameworkElement.DataContext : null) as Birthday;
      if (birthday == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.birthdays, GiftPurchaseStepsAction.profile));
      Navigator.Current.NavigateToUserProfile(birthday.UserId, birthday.UserName, "", false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/BirthdayRecordListItemUC.xaml", UriKind.Relative));
      this.textBlockUserName = (TextBlock) base.FindName("textBlockUserName");
      this.borderGift = (Border) base.FindName("borderGift");
    }
  }
}
