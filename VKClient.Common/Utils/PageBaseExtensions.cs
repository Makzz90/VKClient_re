using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.UC;

namespace VKClient.Common.Utils
{
  public static class PageBaseExtensions
  {
    private static readonly List<MyVirtualizingPanel2> _panels = new List<MyVirtualizingPanel2>();

    public static void OpenGamesPopup(this PageBase page, List<object> games, GamesClickSource clickSource, string requestName = "", int selectedIndex = 0, FrameworkElement root = null)
    {
      double num1 = games.Count > 1 ? 32.0 : 0.0;
      bool isScrollListeningEnabled = true;
      GamesSlideView gamesSlideView1 = new GamesSlideView();
      gamesSlideView1.AllowVerticalSwipe = true;
      gamesSlideView1.NextHeaderMargin = num1;
      SolidColorBrush solidColorBrush1 = new SolidColorBrush(Colors.Transparent);
      gamesSlideView1.BackgroundColor = (Brush) solidColorBrush1;
      GamesSlideView slideView = gamesSlideView1;
      DialogService dialogService = new DialogService();
      dialogService.KeepAppBar = false;
      dialogService.HideOnNavigation = false;
      dialogService.HasPopup = true;
      GamesSlideView gamesSlideView2 = slideView;
      dialogService.Child = (FrameworkElement) gamesSlideView2;
      int num2 = 5;
      dialogService.AnimationType = (DialogService.AnimationTypes) num2;
      int num3 = 1;
      dialogService.AnimationTypeChild = (DialogService.AnimationTypes) num3;
      SolidColorBrush solidColorBrush2 = new SolidColorBrush(Colors.Black);
      double num4 = 0.5;
      ((Brush) solidColorBrush2).Opacity = num4;
      dialogService.BackgroundBrush = (Brush) solidColorBrush2;
      DialogService flyout = dialogService;
      CurrentNewsFeedSource.Source = ViewPostSource.GameWall;
      flyout.Closing += (EventHandler) ((sender, args) =>
      {
        isScrollListeningEnabled = false;
        if (root == null)
          return;
        ((UIElement) root).Opacity = 1.0;
      });
      slideView.CreateSingleElement = (Func<GameView>) (() =>
      {
        GameView gameView = new GameView()
        {
          Flyout = flyout,
          NewsItemsWidth = 480.0,
          GamesClickSource = clickSource,
          GameRequestName = requestName
        };
        ViewportControl viewportControl = gameView.ViewportCtrl;
        double viewportY = 0.0;
        viewportControl.ViewportChanged += ((EventHandler<ViewportChangedEventArgs>) ((sender, args) =>
        {
          Rect viewport = viewportControl.Viewport;
          // ISSUE: explicit reference operation
          viewportY = ((Rect) @viewport).Y;
        }));
        viewportControl.ManipulationStateChanged += ((EventHandler<ManipulationStateChangedEventArgs>) ((sender, args) =>
        {
            if (viewportControl.ManipulationState == ManipulationState.Manipulating || viewportY > -100.0)
            return;
          Rect bounds = viewportControl.Bounds;
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          viewportControl.Bounds=(new Rect(((Rect) @bounds).X, viewportY, ((Rect) @bounds).Width, ((Rect) @bounds).Height));
          flyout.Hide();
        }));
        gameView.WallPanel.ScrollPositionChanged += (EventHandler<MyVirtualizingPanel2.ScrollPositionChangedEventAgrs>) ((sender, args) =>
        {
          if (!isScrollListeningEnabled)
            return;
          if (args.CurrentPosition > 56.0)
            slideView.DisableSwipe();
          else
            slideView.EnableSwipe();
          if (root == null)
            return;
          ((UIElement) root).Opacity = (args.CurrentPosition > 200.0 ? 0.0 : 1.0);
        });
        PageBaseExtensions._panels.Add(gameView.WallPanel);
        return gameView;
      });
      slideView.ItemsCleared += (EventHandler) ((sender, args) => flyout.Hide());
      slideView.Items = new ObservableCollection<object>(games);
      slideView.SelectedIndex = selectedIndex;
      flyout.Closed += (EventHandler) ((o, args) =>
      {
        foreach (MyVirtualizingPanel2 panel in PageBaseExtensions._panels)
          panel.Cleanup();
        PageBaseExtensions._panels.Clear();
        GameView.Cleanup();
        GC.Collect();
      });
      flyout.Show( null);
      page.InitializeAdornerControls();
    }
  }
}
