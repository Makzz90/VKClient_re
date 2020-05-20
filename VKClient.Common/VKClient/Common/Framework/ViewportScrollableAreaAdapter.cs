using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework
{
  public class ViewportScrollableAreaAdapter : IScrollableArea
  {
    private ViewportControl _vpCtrl;
    private Action _pendingScrollAction;
    private bool _justScrolledToOffset;
    private double _justScrolledOffsetValue;

    public Action OnCompressionTop { get; set; }

    public Action OnCompressionBottom { get; set; }

    public Projection Projection
    {
      get
      {
        return this._vpCtrl.Projection;
      }
      set
      {
        this._vpCtrl.Projection = value;
      }
    }

    public Action<double> OnVerticalOffsetChanged { get; set; }

    public double VerticalOffset
    {
      get
      {
        if (this._justScrolledToOffset)
          return this._justScrolledOffsetValue;
        return this._vpCtrl.Viewport.Y;
      }
    }

    public Action<bool, bool> OnScrollStateChanged { get; set; }

    public double ViewportHeight
    {
      get
      {
        return this._vpCtrl.ActualHeight;
      }
    }

    public double ExtentHeight
    {
      get
      {
        return this._vpCtrl.Bounds.Bottom;
      }
    }

    public bool IsEnabled
    {
      get
      {
        return this._vpCtrl.IsEnabled;
      }
      set
      {
        this._vpCtrl.IsEnabled = value;
      }
    }

    public Rect Bounds
    {
      get
      {
        return this._vpCtrl.Bounds;
      }
      set
      {
        this._vpCtrl.Bounds = value;
      }
    }

    public bool IsManipulating
    {
      get
      {
        return this._vpCtrl.ManipulationState == ManipulationState.Manipulating;
      }
    }

    public ViewportScrollableAreaAdapter(ViewportControl vpCtrl)
    {
      this._vpCtrl = vpCtrl;
      this._vpCtrl.ManipulationStateChanged += new EventHandler<ManipulationStateChangedEventArgs>(this._vpCtrl_ManipulationStateChanged);
      this._vpCtrl.ViewportChanged += new EventHandler<ViewportChangedEventArgs>(this._vpCtrl_ViewportChanged);
    }

    private void _vpCtrl_ViewportChanged(object sender, ViewportChangedEventArgs e)
    {
      if (this._pendingScrollAction != null)
      {
        this._pendingScrollAction();
        this._pendingScrollAction = null;
      }
      this._justScrolledToOffset = false;
      if (this.OnVerticalOffsetChanged == null)
        return;
      this.OnVerticalOffsetChanged(this._vpCtrl.Viewport.Y);
    }

    private void _vpCtrl_ManipulationStateChanged(object sender, ManipulationStateChangedEventArgs e)
    {
      if (this.OnScrollStateChanged == null)
        return;
      this.OnScrollStateChanged((uint) this._vpCtrl.ManipulationState > 0U, this._vpCtrl.ManipulationState == ManipulationState.Manipulating);
    }

    public void ScrollToTopOrBottom(bool toBottom = true, Action onCompletedCallback = null)
    {
      if (this._justScrolledToOffset)
        this._pendingScrollAction = (Action) (() => this.DoScrollToTopOrBottom(toBottom, onCompletedCallback));
      else
        this.DoScrollToTopOrBottom(toBottom, onCompletedCallback);
    }

    private void DoScrollToTopOrBottom(bool toBottom, Action onCompletedCallback)
    {
      Rect rect;
      double num1;
      if (!toBottom)
      {
        num1 = 0.0;
      }
      else
      {
        rect = this._vpCtrl.Bounds;
        num1 = rect.Bottom - this._vpCtrl.ActualHeight;
      }
      double to = num1;
      ViewportMediator target = new ViewportMediator();
      target.ViewportControl = this._vpCtrl;
      this._vpCtrl.ViewportChanged -= new EventHandler<ViewportChangedEventArgs>(this._vpCtrl_ViewportChanged);
      rect = this._vpCtrl.Viewport;
      double y = rect.Y;
      double to1 = to;
      DependencyProperty dependencyProperty = ViewportMediator.VerticalOffsetProperty;
      int duration = 250;
      int? startTime = new int?(0);
      CubicEase cubicEase = new CubicEase();
      Action completed = (Action) (() =>
      {
        this._vpCtrl.SetViewportOrigin(new Point(this._vpCtrl.Viewport.X, to));
        this._vpCtrl.ViewportChanged += new EventHandler<ViewportChangedEventArgs>(this._vpCtrl_ViewportChanged);
        if (onCompletedCallback == null)
          return;
        onCompletedCallback();
      });
      int num2 = 0;
      target.Animate(y, to1, (object) dependencyProperty, duration, startTime, (IEasingFunction) cubicEase, completed, num2 != 0);
    }

    public void ScrollToVerticalOffset(double offset)
    {
      if (this.VerticalOffset == offset)
        return;
      this._vpCtrl.SetViewportOrigin(new Point(0.0, offset));
      this._justScrolledOffsetValue = offset;
      this._justScrolledToOffset = true;
    }

    public void UpdateLayout()
    {
      try
      {
        this._vpCtrl.UpdateLayout();
      }
      catch
      {
      }
    }
  }
}
