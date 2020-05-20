using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Information
{
  public class PlacementSelectionPage : PageBase
  {
    private bool _isInitialized;
    private bool _canGetPlacement;
    private readonly MapOverlay _mapOverlayPushpin;
    internal ScrollViewer Viewer;
    internal StackPanel ViewerContent;
    internal Map Map;
    internal TextBoxPanelControl TextBoxPanel;
    private bool _contentLoaded;

    public PlacementSelectionViewModel ViewModel
    {
      get
      {
        return base.DataContext as PlacementSelectionViewModel;
      }
    }

    public PlacementSelectionPage()
    {
      MapOverlay mapOverlay = new MapOverlay();
      Point point = new Point(0.5, 1.0);
      mapOverlay.PositionOrigin = point;
      this._mapOverlayPushpin = mapOverlay;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.SuppressMenu = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      PlacementSelectionViewModel viewModel = new PlacementSelectionViewModel(long.Parse(((Page) this).NavigationContext.QueryString["CommunityId"]), (Place) ParametersRepository.GetParameterForIdAndReset("PlacementSelectionPlace"));
      base.DataContext = viewModel;
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string appBarMenuSave = CommonResources.AppBarMenu_Save;
      applicationBarIconButton1.Text = appBarMenuSave;
      int num = this.ViewModel.GeoCoordinate !=  null ? 1 : 0;
      applicationBarIconButton1.IsEnabled = (num != 0);
      ApplicationBarIconButton appBarButtonSave = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string appBarCancel = CommonResources.AppBar_Cancel;
      applicationBarIconButton2.Text = appBarCancel;
      ApplicationBarIconButton applicationBarIconButton3 = applicationBarIconButton2;
      appBarButtonSave.Click+=((EventHandler) ((p, f) =>
      {
        ((Control) this).Focus();
        viewModel.SaveChanges();
      }));
      applicationBarIconButton3.Click+=((EventHandler) ((p, f) => Navigator.Current.GoBack()));
      this.ApplicationBar = ((IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
      viewModel.PropertyChanged += (PropertyChangedEventHandler) ((p, f) => appBarButtonSave.IsEnabled = (viewModel.IsFormEnabled && viewModel.GeoCoordinate !=  null));
      this.ApplicationBar.Buttons.Add(appBarButtonSave);
      this.ApplicationBar.Buttons.Add(applicationBarIconButton3);
      try
      {
        if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked || !AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
        {
          bool flag = MessageBox.Show(CommonResources.MapAttachment_AllowUseLocation, CommonResources.AccessToLocation, (MessageBoxButton) 1) == MessageBoxResult.OK;
          AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked = true;
          AppGlobalStateManager.Current.GlobalState.AllowUseLocation = flag;
        }
        if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
          this._canGetPlacement = false;
      }
      catch (Exception ex)
      {
        if (ex.HResult == -2147467260)
        {
          GeolocationHelper.HandleDisabledLocationSettings();
          this._canGetPlacement = false;
        }
      }
      this._isInitialized = true;
    }

    private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      ((Control) this).Focus();
    }

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((FrameworkElement) sender).GetBindingExpression((DependencyProperty) TextBox.TextProperty).UpdateSource();
    }

    private void CountryPicker_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      this.ViewModel.ChooseCountry();
    }

    private void CityPicker_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      this.ViewModel.ChooseCity();
    }

    private void Map_OnLoaded(object sender, RoutedEventArgs e)
    {
      MapsSettings.ApplicationContext.ApplicationId=("55677f7c-3dab-4a57-95b2-4efd44a0e692");
      MapsSettings.ApplicationContext.AuthenticationToken=("1jh4FPILRSo9J1ADKx2CgA");
      MapLayer mapLayer = new MapLayer();
      MapOverlay mapOverlayPushpin = this._mapOverlayPushpin;
      ((Collection<MapOverlay>) mapLayer).Add(mapOverlayPushpin);
      this.Map.Layers.Add(mapLayer);
      if (this.ViewModel.GeoCoordinate !=  null)
      {
        this.SetPushpin(this.ViewModel.GeoCoordinate, true);
      }
      else
      {
        if (!this._canGetPlacement)
          return;
        try
        {
          GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
          watcher.PositionChanged+=((EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>) ((o, args) =>
          {
            if (!(this.ViewModel.GeoCoordinate ==  null))
              return;
            PageBase.SetInProgress(false);
            this.SetPushpin(args.Position.Location, true);
            watcher.Stop();
          }));
          watcher.StatusChanged+=((EventHandler<GeoPositionStatusChangedEventArgs>) ((o, args) =>
          {
            if (args.Status != GeoPositionStatus.Ready)
              return;
            GeolocationHelper.HandleDisabledLocationSettings();
          }));
          watcher.Start();
          PageBase.SetInProgress(true);
        }
        catch
        {
        }
      }
    }

    private void Map_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!this.ViewModel.IsFormEnabled)
        return;
      this.SetPushpin(this.Map.ConvertViewportPointToGeoCoordinate(e.GetPosition((UIElement) this.Map)), false);
      PageBase.SetInProgress(false);
    }

    private void SetPushpin(GeoCoordinate geoCoordinate, bool needCenter)
    {
      Image image1 = new Image();
      double num1 = 44.0;
      ((FrameworkElement) image1).Height = num1;
      double num2 = 28.0;
      ((FrameworkElement) image1).Width = num2;
      Image image2 = image1;
      MultiResImageLoader.SetUriSource(image2, "/Resources/MapPin.png");
      this._mapOverlayPushpin.Content = image2;
      this._mapOverlayPushpin.GeoCoordinate = geoCoordinate;
      this.ViewModel.GeoCoordinate = geoCoordinate;
      if (!needCenter)
        return;
      this.Map.Center = geoCoordinate;
      this.Map.ZoomLevel = 12.0;
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = true;
      Point relativePosition = ((UIElement) sender).GetRelativePosition((UIElement) this.ViewerContent);
      // ISSUE: explicit reference operation
      this.Viewer.ScrollToOffsetWithAnimation(((Point) @relativePosition).Y - 38.0, 0.2, false);
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.TextBoxPanel.IsOpen = false;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/Information/PlacementSelectionPage.xaml", UriKind.Relative));
      this.Viewer = (ScrollViewer) base.FindName("Viewer");
      this.ViewerContent = (StackPanel) base.FindName("ViewerContent");
      this.Map = (Map) base.FindName("Map");
      this.TextBoxPanel = (TextBoxPanelControl) base.FindName("TextBoxPanel");
    }
  }
}
