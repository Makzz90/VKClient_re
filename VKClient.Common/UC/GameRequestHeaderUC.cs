using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Games;

namespace VKClient.Common.UC
{
  public class GameRequestHeaderUC : UserControl, INotifyPropertyChanged
  {
      public static readonly DependencyProperty DataProviderProperty = DependencyProperty.Register("DataProvider", typeof(GameRequestHeader), typeof(GameRequestHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GameRequestHeaderUC.DataProvider_OnChanged)));
      public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register("IsSeparatorVisible", typeof(bool), typeof(GameRequestHeaderUC), new PropertyMetadata(true, new PropertyChangedCallback(GameRequestHeaderUC.OnIsSeparatorVisibleChanged)));
    private bool _isInPlayHandler;
    internal VisualStateGroup Common;
    internal VisualState ReadState;
    internal Border borderUnread;
    internal Image imageUserIcon;
    internal TextBlock textBlockDescription;
    internal Grid separator;
    private bool _contentLoaded;

    public GameRequestHeader DataProvider
    {
      get
      {
        return (GameRequestHeader) base.GetValue(GameRequestHeaderUC.DataProviderProperty);
      }
      set
      {
        base.SetValue(GameRequestHeaderUC.DataProviderProperty, value);
      }
    }

    public bool IsSeparatorVisible
    {
      get
      {
        return (bool) base.GetValue(GameRequestHeaderUC.IsSeparatorVisibleProperty);
      }
      set
      {
        this.SetDPValue(GameRequestHeaderUC.IsSeparatorVisibleProperty, value, "IsSeparatorVisible");
      }
    }

    public Action<GameRequestHeader, Action> DeleteRequestAction { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public GameRequestHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = this;
      ((UIElement) this.separator).Opacity = 1.0;
    }

    private static void DataProvider_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameRequestHeaderUC gameRequestHeaderUc = d as GameRequestHeaderUC;
      if (gameRequestHeaderUc == null)
        return;
      // ISSUE: explicit reference operation
      GameRequestHeader newValue = e.NewValue as GameRequestHeader;
      if (newValue == null)
        return;
      ((UIElement) gameRequestHeaderUc.textBlockDescription).Visibility = (!string.IsNullOrEmpty(newValue.GameRequest.text) ? Visibility.Visible : Visibility.Collapsed);
    }

    private static void OnIsSeparatorVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameRequestHeaderUC gameRequestHeaderUc = d as GameRequestHeaderUC;
      if (gameRequestHeaderUc == null)
        return;
      // ISSUE: explicit reference operation
      bool newValue = (bool) e.NewValue;
      ((UIElement) gameRequestHeaderUc.separator).Opacity = (newValue ? 1.0 : 0.0);
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

    public void MarkAsRead()
    {
      VisualStateManager.GoToState((Control) this, "ReadState", false);
    }

    private void MarkAsReadAnimation_OnCompleted(object sender, EventArgs e)
    {
      if (this.DataProvider == null)
        return;
      this.DataProvider.MarkAsRead();
    }

    private void User_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      User tag = ((FrameworkElement) this.imageUserIcon).Tag as User;
      if (tag == null)
        return;
      Navigator.Current.NavigateToUserProfile(tag.uid, tag.Name, "", false);
    }

    private void TextBlockDescription_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
    }

    private void OpenGame_OnClick(object sender, RoutedEventArgs e)
    {
      if (this.DeleteRequestAction == null)
        return;
      this.DeleteRequestAction(this.DataProvider, new Action(this.OpenGame));
    }

    private async void OpenGame()
    {
      if (this.DataProvider == null || this.DataProvider.Game == null || this._isInPlayHandler)
        return;
      this._isInPlayHandler = true;
      Game game = this.DataProvider.Game;
      GameRequest gameRequest = this.DataProvider.GameRequest;
      bool flag = InstalledPackagesFinder.Instance.IsPackageInstalled(game.platform_id);
      EventAggregator.Current.Publish(new GamesActionEvent()
      {
        game_id = game.id,
        visit_source = AppGlobalStateManager.Current.GlobalState.GamesVisitSource,
        action_type = (GamesActionType) (flag ? 0 : 1),
        click_source = GamesClickSource.request,
        request_name = gameRequest.name
      });
      await Task.Delay(1000);
      Navigator.Current.OpenGame(game);
      this._isInPlayHandler = false;
    }

    private void HideButton_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.DeleteRequestAction == null)
        return;
      this.DeleteRequestAction(this.DataProvider,  null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GameRequestHeaderUC.xaml", UriKind.Relative));
      this.Common = (VisualStateGroup) base.FindName("Common");
      this.ReadState = (VisualState) base.FindName("ReadState");
      this.borderUnread = (Border) base.FindName("borderUnread");
      this.imageUserIcon = (Image) base.FindName("imageUserIcon");
      this.textBlockDescription = (TextBlock) base.FindName("textBlockDescription");
      this.separator = (Grid) base.FindName("separator");
    }
  }
}
