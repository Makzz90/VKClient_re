using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Games;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
    public class GameView : UserControl, ISupportState
    {
        private static readonly object lockObj = new object();
        private GameViewModel _viewModel;
        private GameHeader _gameHeader;
        private bool _isActiveState;
        private bool _isLoaded;
        public const int HEADER_STICK_THRESHOLD = 56;
        private static int _instancesCount;
        private readonly DelayedExecutor _delayedExecutor = new DelayedExecutor(500);
        private bool _isInPlayHandler;
        internal Grid gridRoot;
        internal Border borderBackground;
        internal ViewportControl ViewportCtrl;
        internal Border borderBackgroundMock;
        internal MyVirtualizingStackPanel ContentPanel;
        internal StackPanel HeaderContentPanel;
        internal GameViewHeaderUC Header;
        internal FooterUC ucFooter;
        internal ProgressBar loadingBar;
        internal GameRequestsSectionItemUC ucGameRequests;
        internal MyVirtualizingPanel2 WallPanel;
        internal GameViewHeaderUC HeaderSticky;
        internal Border panelOverlay;
        private bool _contentLoaded;

        public DialogService Flyout { get; set; }

        public double NewsItemsWidth { get; set; }

        public GamesClickSource GamesClickSource { get; set; }

        public string GameRequestName { get; set; }

        public GameView()
        {
            //base.\u002Ector();
            this.InitializeComponent();
            this.WallPanel.ExtraOffsetY = 126.0;
            this.WallPanel.InitializeWithScrollViewer((IScrollableArea)new ViewportScrollableAreaAdapter(this.ViewportCtrl), false);
            this.ViewportCtrl.BindViewportBoundsTo((FrameworkElement)this.ContentPanel);
            this.WallPanel.ScrollPositionChanged += (EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>)((sender, args) =>
            {
                if (args.CurrentPosition > 300.0)
                {
                    ((UIElement)this.borderBackgroundMock).Visibility = Visibility.Collapsed;
                    if (ThemeSettings.IsLightTheme)
                        ((UIElement)this.borderBackground).Visibility = Visibility.Visible;
                }
                else
                {
                    ((UIElement)this.borderBackgroundMock).Visibility = Visibility.Visible;
                    if (ThemeSettings.IsLightTheme)
                        ((UIElement)this.borderBackground).Visibility = Visibility.Collapsed;
                }
                ((UIElement)this.HeaderSticky).Visibility = (args.CurrentPosition >= 56.0 ? Visibility.Visible : Visibility.Collapsed);
            });
            ++GameView._instancesCount;
        }

        ~GameView()
        {
            //try
            //{
            --GameView._instancesCount;
            //}
            //finally
            //{
            //  // ISSUE: explicit finalizer call
            //  // ISSUE: explicit non-virtual call
            //  this.Finalize();
            //}
        }

        public static void Cleanup()
        {
        }

        public async void SetState(bool isActive)
        {
            this._isActiveState = isActive;
            if (this._isActiveState)
            {
                ((UIElement)this.panelOverlay).Visibility = Visibility.Collapsed;
                this.loadingBar.IsIndeterminate = true;
                if (this._gameHeader != null)
                {
                    long id = this._gameHeader.Game.id;
                    lock (GameView.lockObj)
                    {
                        this._viewModel = new GameViewModel(this._gameHeader, this.NewsItemsWidth);
                        base.DataContext = this._viewModel;
                        this._delayedExecutor.AddToDelayedExecution(new Action(this.LoadingTimerOnTick));
                    }
                }
            }
            else
            {
                ((UIElement)this.panelOverlay).Visibility = Visibility.Visible;
                this.loadingBar.IsIndeterminate = false;
                this._viewModel = null;
                base.DataContext = null;
            }
            if (!this._isLoaded)
                return;
            await Task.Delay(2000);
            if (!this._isActiveState || base.DataContext == null || !this.Flyout.IsOpen)
                return;
            this.ucGameRequests.MarkAllAsRead();
        }

        public void SetDataContext(object obj)
        {
            this._gameHeader = obj as GameHeader;
            if (this._gameHeader == null)
            {
                Game game = obj as Game;
                if (game != null)
                    this._gameHeader = new GameHeader(game);
            }
            ((UIElement)this.gridRoot).Visibility = (this._gameHeader != null ? Visibility.Visible : Visibility.Collapsed);
            this.Header.GameHeader = this._gameHeader;
            this.HeaderSticky.GameHeader = this._gameHeader;
        }

        public void SetNextDataContext(object obj)
        {
            this.Header.NextGameHeader = obj as GameHeader;
        }

        private void LoadingTimerOnTick()
        {
            if (this._viewModel == null)
                return;
            this._viewModel.LoadGameInfo((Action)(async () =>
            {
                this.loadingBar.IsIndeterminate = false;
                this._isLoaded = true;
                if (this._viewModel != null)
                {
                    this._viewModel.LoadGameGroupInfo(null);
                    GameViewModel viewModel;
                    this._viewModel.WallVM.LoadData(false, false, (Action<BackendResult<WallData, ResultCode>>)(res => viewModel = this._viewModel), false);
                }
                await Task.Delay(2000);
                if (!this._isActiveState || base.DataContext == null || !this.Flyout.IsOpen)
                    return;
                this.ucGameRequests.MarkAllAsRead();
            }));
        }

        private void HeaderSticky_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ((UIElement)this.borderBackground).Visibility = Visibility.Collapsed;
            ((UIElement)this.borderBackgroundMock).Visibility = Visibility.Visible;
            ((UIElement)this.HeaderSticky).Visibility = Visibility.Collapsed;
            this.WallPanel.ScrollToBottom(false);
        }

        private void HeaderPanel_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.WallPanel.DeltaOffset = -((FrameworkElement)this.HeaderContentPanel).ActualHeight - 182.0;
        }

        private async void PlayButton_OnClicked(object sender, RoutedEventArgs e)
        {
            if (this._isInPlayHandler)
                return;
            this._isInPlayHandler = true;
            Game game = this._viewModel.GameHeader.Game;
            bool flag = InstalledPackagesFinder.Instance.IsPackageInstalled(game.platform_id);
            GamesActionEvent gamesActionEvent = new GamesActionEvent()
            {
                game_id = game.id,
                visit_source = AppGlobalStateManager.Current.GlobalState.GamesVisitSource,
                action_type = (GamesActionType)(flag ? 0 : 1),
                click_source = this.GamesClickSource
            };
            if (!string.IsNullOrEmpty(this.GameRequestName))
                gamesActionEvent.request_name = this.GameRequestName;
            EventAggregator.Current.Publish(gamesActionEvent);
            await Task.Delay(1000);
            Navigator.Current.OpenGame(game);
            this._isInPlayHandler = false;
        }

        private void TransparentBorder_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.Flyout == null)
                return;
            this.Flyout.Hide();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GameView.xaml", UriKind.Relative));
            this.gridRoot = (Grid)base.FindName("gridRoot");
            this.borderBackground = (Border)base.FindName("borderBackground");
            this.ViewportCtrl = (ViewportControl)base.FindName("ViewportCtrl");
            this.borderBackgroundMock = (Border)base.FindName("borderBackgroundMock");
            this.ContentPanel = (MyVirtualizingStackPanel)base.FindName("ContentPanel");
            this.HeaderContentPanel = (StackPanel)base.FindName("HeaderContentPanel");
            this.Header = (GameViewHeaderUC)base.FindName("Header");
            this.ucFooter = (FooterUC)base.FindName("ucFooter");
            this.loadingBar = (ProgressBar)base.FindName("loadingBar");
            this.ucGameRequests = (GameRequestsSectionItemUC)base.FindName("ucGameRequests");
            this.WallPanel = (MyVirtualizingPanel2)base.FindName("WallPanel");
            this.HeaderSticky = (GameViewHeaderUC)base.FindName("HeaderSticky");
            this.panelOverlay = (Border)base.FindName("panelOverlay");
        }
    }
}
