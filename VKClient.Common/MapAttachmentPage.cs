using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common
{
    public class MapAttachmentPage : PageBase
    {
        private bool _isInitialized;
        private bool _shouldPick;
        private readonly MapLayer _mapLayerPushpin;
        private readonly MapOverlay _mapOverlayPushpin;
        private readonly Image _pinImage;
        private GeoCoordinateWatcher _watcher;
        private GeoCoordinate _lastPosition;
        internal ProgressIndicator progressIndicator;
        internal Map map;
        private bool _contentLoaded;

        public MapAttachmentPage()
        {
            MapOverlay mapOverlay = new MapOverlay();
            Point point = new Point(0.5, 1.0);
            mapOverlay.PositionOrigin = point;
            this._mapOverlayPushpin = mapOverlay;
            // ISSUE: explicit constructor call
            //base.\u002Ector();
            try
            {
                this.InitializeComponent();
                Image image = new Image();
                double num1 = 44.0;
                ((FrameworkElement)image).Height = num1;
                double num2 = 28.0;
                ((FrameworkElement)image).Width = num2;
                this._pinImage = image;
                MultiResImageLoader.SetUriSource(this._pinImage, "Resources/MapPin.png");
            }
            catch (Exception ex)
            {
                Logger.Instance.ErrorAndSaveToIso("Failed to create MapAttachmentPage", ex);
            }
        }

        private void Map_OnLoaded(object sender, RoutedEventArgs e)
        {
            MapsSettings.ApplicationContext.ApplicationId = ("55677f7c-3dab-4a57-95b2-4efd44a0e692");
            MapsSettings.ApplicationContext.AuthenticationToken = ("1jh4FPILRSo9J1ADKx2CgA");
            this._mapLayerPushpin.Add(this._mapOverlayPushpin);
            this.map.Layers.Add(this._mapLayerPushpin);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                base.HandleOnNavigatedTo(e);
                if (!this._isInitialized)
                {
                    base.DataContext = (new ViewModelBase());
                    this._shouldPick = ((Page)this).NavigationContext.QueryString.ContainsKey("Pick") && ((Page)this).NavigationContext.QueryString["Pick"] == true.ToString();
                    if (!this._shouldPick)
                    {
                        this.MoveMapToPosition(new GeoCoordinate(double.Parse(((Page)this).NavigationContext.QueryString["latitude"], (IFormatProvider)CultureInfo.InvariantCulture), double.Parse(((Page)this).NavigationContext.QueryString["longitude"], (IFormatProvider)CultureInfo.InvariantCulture)));
                        return;
                    }
                    if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked || !AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
                    {
                        bool flag = MessageBox.Show(CommonResources.MapAttachment_AllowUseLocation, CommonResources.AccessToLocation, (MessageBoxButton)1) == MessageBoxResult.OK;
                        AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked = true;
                        AppGlobalStateManager.Current.GlobalState.AllowUseLocation = flag;
                    }
                    this.InitializeAppBar();
                    this._isInitialized = true;
                }
                if (!(this._lastPosition == null))
                    return;
                if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
                    return;
                try
                {
                    if (this._watcher != null)
                    {
                        this._watcher.PositionChanged -= new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(this.Watcher_OnPositionChanged);
                        this._watcher.StatusChanged -= new EventHandler<GeoPositionStatusChangedEventArgs>(this.Watcher_OnStatusChanged);
                    }
                    this._watcher = new GeoCoordinateWatcher();
                    this._watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(this.Watcher_OnPositionChanged);
                    this._watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(this.Watcher_OnStatusChanged);
                    GeoCoordinateWatcher watcher = this._watcher;
                    if (watcher != null)
                    {
                        // ISSUE: explicit non-virtual call
                        watcher.Start();
                    }
                    this.SetProgressIndicator(true);
                }
                catch (Exception ex)
                {
                    this.SetProgressIndicator(false);
                    if (ex.HResult != -2147467260)
                        return;
                    GeolocationHelper.HandleDisabledLocationSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.ErrorAndSaveToIso("Failed to OnNavigatedTo MapAttachmentPage", ex);
            }
        }

        private void Watcher_OnPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Logger.Instance.Info("WatcherPositionChanged Enter");
            this.MoveMapToPosition(e.Position.Location);
            this.StopGeoWatcher();
            Logger.Instance.Info("WatcherPositionChanged Exit");
        }

        private void Watcher_OnStatusChanged(object sender, GeoPositionStatusChangedEventArgs args)
        {
            if (args.Status != GeoPositionStatus.Ready)
                return;
            this.SetProgressIndicator(false);
            GeolocationHelper.HandleDisabledLocationSettings();
        }

        private void StopGeoWatcher()
        {
            GeoCoordinateWatcher watcher = this._watcher;
            if (watcher != null)
            {
                // ISSUE: explicit non-virtual call
                watcher.Stop();
            }
            this.SetProgressIndicator(false);
        }

        protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Logger.Instance.Info("MapAttachmentHandleOnNavigatingFrom Enter");
            base.HandleOnNavigatingFrom(e);
            ThreadPool.QueueUserWorkItem((WaitCallback)(callback =>
            {
                try
                {
                    GeoCoordinateWatcher watcher = this._watcher;
                    if (watcher == null)
                        return;
                    // ISSUE: explicit non-virtual call
                    watcher.Stop();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("failed to stop watcher", ex);
                }
            }));
            Logger.Instance.Info("MapAttachmentHandleOnNavigatingFrom Exit");
        }

        private void SetProgressIndicator(bool show)
        {
            this.progressIndicator.IsVisible = show;
            this.progressIndicator.IsIndeterminate = show;
        }

        private void MoveMapToPosition(GeoCoordinate geoCoordinate)
        {
            try
            {
                this.map.Center = geoCoordinate;
                this.map.ZoomLevel = 16.0;
                this._mapOverlayPushpin.Content = this._pinImage;
                this._mapOverlayPushpin.GeoCoordinate = geoCoordinate;
                this._lastPosition = geoCoordinate;
            }
            catch (Exception ex)
            {
                Logger.Instance.ErrorAndSaveToIso("Failed to MoveMapToPosition MapAttachmentPage", ex);
            }
        }

        private void InitializeAppBar()
        {
            if (this._shouldPick)
            {
                ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
                Uri uri1 = new Uri("/Resources/check.png", UriKind.Relative);
                applicationBarIconButton1.IconUri = uri1;
                string barAttachLocation = CommonResources.Conversation_AppBar_AttachLocation;
                applicationBarIconButton1.Text = barAttachLocation;
                ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
                applicationBarIconButton2.Click += (new EventHandler(this.AppBarButtonSave_OnClick));
                ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
                Uri uri2 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
                applicationBarIconButton3.IconUri = uri2;
                string editAppBarCancel = CommonResources.ChatEdit_AppBar_Cancel;
                applicationBarIconButton3.Text = editAppBarCancel;
                ApplicationBarIconButton applicationBarIconButton4 = applicationBarIconButton3;
                applicationBarIconButton4.Click += (new EventHandler(this.AppBarButtonCancel_OnClick));
                ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
                applicationBar.Buttons.Add(applicationBarIconButton2);
                applicationBar.Buttons.Add(applicationBarIconButton4);
                this.ApplicationBar = ((IApplicationBar)applicationBar);
            }
            else
                this.ApplicationBar = (null);
        }

        private void AppBarButtonCancel_OnClick(object sender, EventArgs e)
        {
            ((Page)this).NavigationService.GoBackSafe();
        }

        private void AppBarButtonSave_OnClick(object sender, EventArgs e)
        {
            ParametersRepository.SetParameterForId("NewPositionToBeAttached", this._lastPosition);
            ((Page)this).NavigationService.GoBackSafe();
        }

        private void Map_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!this._shouldPick)
                return;
            GeoCoordinate geoCoordinate = this.map.ConvertViewportPointToGeoCoordinate(e.GetPosition((UIElement)this.map));
            this._mapOverlayPushpin.Content = this._pinImage;
            this._mapOverlayPushpin.GeoCoordinate = geoCoordinate;
            this._lastPosition = geoCoordinate;
            this.StopGeoWatcher();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/MapAttachmentPage.xaml", UriKind.Relative));
            this.progressIndicator = (ProgressIndicator)base.FindName("progressIndicator");
            this.map = (Map)base.FindName("map");
        }
    }
}
