using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;

namespace VKMessenger.Views
{
  public class MapAttachmentPage : PageBase
  {
    private GeoCoordinateWatcher _watcher = new GeoCoordinateWatcher();
    private bool _isInitialized;
    private bool _shouldPick;
    private CredentialsProvider _credentialsProvider;
    private GeoCoordinate _lastPosition;
    internal ProgressIndicator progressIndicator;
    internal Map map;
    internal MapLayer pushpinLayer;
    private bool _contentLoaded;

    public MapAttachmentPage()
    {
      this.InitializeComponent();
      this._credentialsProvider = (CredentialsProvider) new ApplicationIdCredentialsProvider("AsAOCzjdoO4A8lKbpU4hZzrs4piUJ0g4jQZ-FbL4AUmy_cbfoOQaqN5usCNwG0Ua");
      this.map.CredentialsProvider = this._credentialsProvider;
      this._watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(this._watcher_PositionChanged);
    }

    private void _watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
    {
      this.MoveMapToPosition(e.Position.Location);
      this._watcher.Stop();
      this.SetProgressIndicator(false);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._shouldPick = ((Page) this).NavigationContext.QueryString.ContainsKey("Pick");
      if (!this._shouldPick)
      {
        this.MoveMapToPosition(new GeoCoordinate(double.Parse(((Page) this).NavigationContext.QueryString["latitude"], (IFormatProvider) CultureInfo.InvariantCulture), double.Parse(((Page) this).NavigationContext.QueryString["longitude"], (IFormatProvider) CultureInfo.InvariantCulture)));
      }
      else
      {
        if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked)
        {
          bool flag = false;
          if (MessageBox.Show(CommonResources.MapAttachment_AllowUseLocation, CommonResources.AccessToLocation, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            flag = true;
          AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked = true;
          AppGlobalStateManager.Current.GlobalState.AllowUseLocation = flag;
        }
        if (AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
        {
          this._watcher.Start();
          this.SetProgressIndicator(true);
        }
      }
      this.InitializeAppBar();
      this._isInitialized = true;
    }

    protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
    {
      base.HandleOnNavigatingFrom(e);
      this._watcher.Stop();
    }

    private void SetProgressIndicator(bool show)
    {
      this.progressIndicator.IsVisible = show;
      this.progressIndicator.IsIndeterminate = show;
    }

    private void MoveMapToPosition(GeoCoordinate geoCoordinate)
    {
      this.map.AnimationLevel = AnimationLevel.UserInput;
      this.map.Center = geoCoordinate;
      this.map.ZoomLevel = 15.0;
      Image image = new Image();
      image.Source = ((ImageSource) new BitmapImage(new Uri("/VKMessenger;component/Resources/Map_Pin.png", UriKind.Relative)));
      image.Stretch=((Stretch) 0);
      PositionOrigin bottomCenter = PositionOrigin.BottomCenter;
      this._lastPosition = geoCoordinate;
      ((PresentationFrameworkCollection<UIElement>) this.pushpinLayer.Children).Clear();
      this.pushpinLayer.AddChild((UIElement) image, geoCoordinate, bottomCenter);
    }

    private void InitializeAppBar()
    {
      if (this._shouldPick)
      {
        ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
        Uri uri1 = new Uri("./Resources/appbar.save.rest.png", UriKind.Relative);
        applicationBarIconButton1.IconUri = uri1;
        string chatEditAppBarSave = CommonResources.ChatEdit_AppBar_Save;
        applicationBarIconButton1.Text = chatEditAppBarSave;
        ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
        applicationBarIconButton2.Click+=(new EventHandler(this._appBarButtonSave_Click));
        ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
        Uri uri2 = new Uri("./Resources/appbar.cancel.rest.png", UriKind.Relative);
        applicationBarIconButton3.IconUri = uri2;
        string editAppBarCancel = CommonResources.ChatEdit_AppBar_Cancel;
        applicationBarIconButton3.Text = editAppBarCancel;
        ApplicationBarIconButton applicationBarIconButton4 = applicationBarIconButton3;
        applicationBarIconButton4.Click+=(new EventHandler(this._appBarButtonCancel_Click));
        ApplicationBar applicationBar1 = new ApplicationBar();
        Color appBarBgColor = VKConstants.AppBarBGColor;
        applicationBar1.BackgroundColor = appBarBgColor;
        Color appBarFgColor = VKConstants.AppBarFGColor;
        applicationBar1.ForegroundColor = appBarFgColor;
        ApplicationBar applicationBar2 = applicationBar1;
        applicationBar2.Buttons.Add(applicationBarIconButton2);
        applicationBar2.Buttons.Add(applicationBarIconButton4);
        this.ApplicationBar = ((IApplicationBar) applicationBar2);
      }
      else
        this.ApplicationBar = ( null);
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      ((Page) this).NavigationService.GoBackSafe();
    }

    private void _appBarButtonSave_Click(object sender, EventArgs e)
    {
      ParametersRepository.SetParameterForId("NewPositionToBeAttached", this._lastPosition);
      ((Page) this).NavigationService.GoBackSafe();
    }

    private void pushpinLayer_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this._shouldPick)
        return;
      Point position = e.GetPosition((UIElement) this.map);
      GeoCoordinate geoCoordinate = new GeoCoordinate();
      GeoCoordinate location = this.map.ViewportPointToLocation(position);
      ((PresentationFrameworkCollection<UIElement>) this.pushpinLayer.Children).Clear();
      Image image = new Image();
      image.Source = ((ImageSource) new BitmapImage(new Uri("/VKMessenger;component/Resources/Map_Pin.png", UriKind.Relative)));
      image.Stretch=((Stretch) 0);
      PositionOrigin bottomCenter = PositionOrigin.BottomCenter;
      this.pushpinLayer.AddChild((UIElement) image, location, bottomCenter);
      this._lastPosition = location;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/MapAttachmentPage.xaml", UriKind.Relative));
      this.progressIndicator = (ProgressIndicator) base.FindName("progressIndicator");
      this.map = (Map) base.FindName("map");
      this.pushpinLayer = (MapLayer) base.FindName("pushpinLayer");
    }
  }
}
