using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.UC
{
  public class GameNewsHeaderUC : UserControl
  {
    public static readonly DependencyProperty GameGroupIdProperty = DependencyProperty.Register("GameGroupId", typeof (long), typeof (GameNewsHeaderUC), new PropertyMetadata(0L));
    public static readonly DependencyProperty IsSubscribedProperty = DependencyProperty.Register("IsSubscribed", typeof(bool?), typeof(GameNewsHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GameNewsHeaderUC.OnIsSubscribedChanged)));
    private bool _isLoading;
    internal TextBlock textBlockSubscribe;
    private bool _contentLoaded;

    public long GameGroupId
    {
      get
      {
        return (long) base.GetValue(GameNewsHeaderUC.GameGroupIdProperty);
      }
      set
      {
        base.SetValue(GameNewsHeaderUC.GameGroupIdProperty, value);
      }
    }

    public bool? IsSubscribed
    {
      get
      {
        return new bool?((bool) base.GetValue(GameNewsHeaderUC.IsSubscribedProperty));
      }
      set
      {
        base.SetValue(GameNewsHeaderUC.IsSubscribedProperty, value);
      }
    }

    public GameNewsHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void OnIsSubscribedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameNewsHeaderUC gameNewsHeaderUc = d as GameNewsHeaderUC;
      if (gameNewsHeaderUc == null)
        return;
      // ISSUE: explicit reference operation
      bool? newValue = (bool?) e.NewValue;
      if (!newValue.HasValue)
      {
        ((UIElement) gameNewsHeaderUc.textBlockSubscribe).Visibility = Visibility.Collapsed;
      }
      else
      {
        ((UIElement) gameNewsHeaderUc.textBlockSubscribe).Visibility = Visibility.Visible;
        gameNewsHeaderUc.textBlockSubscribe.Text = (newValue.Value ? CommonResources.Games_NewsUnsubscribe : CommonResources.Games_NewsSubscribe);
      }
    }

    private void SubscribeUnsubscribe_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.GameGroupId == 0L || (!this.IsSubscribed.HasValue || this._isLoading))
        return;
      this._isLoading = true;
      GroupsService current = GroupsService.Current;
      if (this.IsSubscribed.Value)
        current.Leave(this.GameGroupId, (Action<BackendResult<OwnCounters, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.IsSubscribed = new bool?(false);
          this._isLoading = false;
        }))));
      else
        current.Join(this.GameGroupId, false, (Action<BackendResult<OwnCounters, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.IsSubscribed = new bool?(true);
          this._isLoading = false;
        }))),  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GameNewsHeaderUC.xaml", UriKind.Relative));
      this.textBlockSubscribe = (TextBlock) base.FindName("textBlockSubscribe");
    }
  }
}
