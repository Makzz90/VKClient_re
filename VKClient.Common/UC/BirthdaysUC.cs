using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class BirthdaysUC : UserControl
  {
    private bool _contentLoaded;

    public BirthdaysUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      base.DataContext = (new BirthdaysViewModel());
    }

    private void Header_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      MenuUC.Instance.NavigateToBirthdays(false);
    }

    private void Header_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
    {
      MenuUC.Instance.NavigateToBirthdays(true);
    }

    private void Birthday_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      BirthdaysUC.NavigateToUserProfile(sender, false);
    }

    private void Birthday_OnHolding(object sender, System.Windows.Input.GestureEventArgs e)
    {
      BirthdaysUC.NavigateToUserProfile(sender, true);
    }

    private static void NavigateToUserProfile(object sender, bool isHoldingEvent)
    {
      FrameworkElement frameworkElement = (FrameworkElement) sender;
      Birthday birthday = (frameworkElement != null ? frameworkElement.DataContext : null) as Birthday;
      if (birthday == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.left_menu, GiftPurchaseStepsAction.profile));
      MenuUC.Instance.NavigateToUserProfile(birthday.UserId, birthday.UserName, isHoldingEvent);
    }

    private void SendGift_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      BirthdaysUC.NavigateToGiftsCatalog(sender, false);
    }

    private void SendGift_OnHold(object sender, System.Windows.Input.GestureEventArgs e)
    {
      BirthdaysUC.NavigateToGiftsCatalog(sender, true);
    }

    private static void NavigateToGiftsCatalog(object sender, bool isHoldingEvent)
    {
      FrameworkElement frameworkElement = (FrameworkElement) sender;
      Birthday birthday = (frameworkElement != null ? frameworkElement.DataContext : null) as Birthday;
      if (birthday == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.left_menu, GiftPurchaseStepsAction.store));
      MenuUC.Instance.NavigateToGiftsCatalog(birthday.UserId, isHoldingEvent);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/BirthdaysUC.xaml", UriKind.Relative));
    }
  }
}
