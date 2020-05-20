using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class GameNotificationsSettingsUC : UserControl
  {
    public static readonly DependencyProperty GameIdProperty = DependencyProperty.Register("GameId", typeof (long), typeof (GameNotificationsSettingsUC), new PropertyMetadata(0L));
    private bool _contentLoaded;

    public long GameId
    {
      get
      {
        return (long) base.GetValue(GameNotificationsSettingsUC.GameIdProperty);
      }
      set
      {
        base.SetValue(GameNotificationsSettingsUC.GameIdProperty, value);
      }
    }

    public GameNotificationsSettingsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void GameNotificationsSettings_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.GameId < 1L)
        return;
      Navigator.Current.NavigateToGameSettings(this.GameId);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GameNotificationsSettingsUC.xaml", UriKind.Relative));
    }
  }
}
