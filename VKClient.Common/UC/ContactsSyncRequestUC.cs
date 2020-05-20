using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class ContactsSyncRequestUC : UserControl
  {
    internal GenericHeaderUC ucHeader;
    internal Grid gridFriends;
    internal ProgressRing progressRing;
    internal ItemsControl itemsControl;
    internal TextBlock textBlockFriendsCount;
    internal Button buttonContinue;
    private bool _contentLoaded;

    public ContactsSyncRequestUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucHeader.HideSandwitchButton = true;
      this.ucHeader.SupportMenu = false;
      this.ucHeader.Title = CommonResources.PageTitle_FindFriends;
    }

    private void LoadFriends()
    {
      UsersService.Instance.GetFeatureUsers(10, (Action<BackendResult<VKList<User>, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        ((UIElement) this.progressRing).Visibility = Visibility.Collapsed;
        if (result.ResultCode == ResultCode.Succeeded)
        {
          List<User> items = result.ResultData.items;
          if (items.Count > 0)
          {
            this.itemsControl.ItemsSource = Enumerable.Take<User>(items, 5);
            this.textBlockFriendsCount.Text = (UIStringFormatterHelper.FormatNumberOfSomething(items.Count, CommonResources.OneFriendContactsSyncFrm, CommonResources.TwoFourFriendsContactsSyncFrm, CommonResources.FiveFriendsContactsSyncFrm, true,  null, false));
          }
          else
            ((UIElement) this.gridFriends).Visibility = Visibility.Collapsed;
        }
        else
          ((UIElement) this.gridFriends).Visibility = Visibility.Collapsed;
      }))));
    }

    public static void OpenFriendsImportContacts(Action continueClickCallback)
    {
        if (AppGlobalStateManager.Current.GlobalState.AllowSendContacts)
        {
            if (continueClickCallback != null)
            {
                continueClickCallback.Invoke();
            }
            return;
        }
        DialogService popup = new DialogService
        {
            AnimationType = DialogService.AnimationTypes.None,
            AnimationTypeChild = DialogService.AnimationTypes.SlideInversed,
            BackgroundBrush = new SolidColorBrush(Colors.Transparent)
        };
        ContactsSyncRequestUC child = new ContactsSyncRequestUC();
        child.buttonContinue.Click+=(delegate(object sender, RoutedEventArgs args)
        {
            AppGlobalStateManager.Current.GlobalState.AllowSendContacts = true;
            EventAggregator.Current.Publish(new ContactsSyncEnabled());
            popup.Hide();
            if (continueClickCallback != null)
            {
                continueClickCallback.Invoke();
            }
        });
        popup.Child = child;
        popup.Opened += delegate(object sender, EventArgs args)
        {
            child.LoadFriends();
        };
        popup.Show(null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ContactsSyncRequestUC.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.gridFriends = (Grid) base.FindName("gridFriends");
      this.progressRing = (ProgressRing) base.FindName("progressRing");
      this.itemsControl = (ItemsControl) base.FindName("itemsControl");
      this.textBlockFriendsCount = (TextBlock) base.FindName("textBlockFriendsCount");
      this.buttonContinue = (Button) base.FindName("buttonContinue");
    }
  }
}
