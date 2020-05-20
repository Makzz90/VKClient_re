using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.UC
{
  public class AppTipContainerUC : UserControl
  {
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    public Action OnTap { get; set; }

    public AppTipContainerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    public void InitForSwipeFromLeftSideTip()
    {
      AppTipBubbleUC appTipBubbleUc = new AppTipBubbleUC();
      ((FrameworkElement) appTipBubbleUc.LayoutRoot).Width = 293.0;
      ((FrameworkElement) appTipBubbleUc).Margin=(new Thickness(16.0, 96.0, 0.0, 0.0));
      ((FrameworkElement) appTipBubbleUc.imageTip).Width = 100.0;
      ((FrameworkElement) appTipBubbleUc.imageTip).Height = 92.0;
      ImageLoader.SetUriSource(appTipBubbleUc.imageTip, "/Resources/New/SwipeMenuTip.png");
      appTipBubbleUc.textBlockTip.Text = CommonResources.SwipeToOpenMenuTip;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this.LayoutRoot).Children).Add((UIElement) appTipBubbleUc);
    }

    public void InitForPullToRefresh()
    {
      AppTipBubbleUC appTipBubbleUc = new AppTipBubbleUC();
      ((FrameworkElement) appTipBubbleUc.LayoutRoot).Width = 277.0;
      ((FrameworkElement) appTipBubbleUc).Margin=(new Thickness(90.0, 96.0, 0.0, 0.0));
      ((FrameworkElement) appTipBubbleUc.imageTip).Width = 66.0;
      ((FrameworkElement) appTipBubbleUc.imageTip).Height = 117.0;
      ImageLoader.SetUriSource(appTipBubbleUc.imageTip, "/Resources/New/PullToRefreshTip.png");
      appTipBubbleUc.textBlockTip.Text = CommonResources.PullToRefreshTip;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this.LayoutRoot).Children).Add((UIElement) appTipBubbleUc);
    }

    private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnTap == null)
        return;
      this.OnTap();
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/AppTipContainerUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
    }
  }
}
