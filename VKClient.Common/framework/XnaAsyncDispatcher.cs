using Microsoft.Xna.Framework;
using System;
using System.Windows;
using System.Windows.Threading;

namespace VKClient.Common.Framework
{
  public class XnaAsyncDispatcher : IApplicationService
  {
    private static readonly TimeSpan _defaultDispatchInterval = TimeSpan.FromMilliseconds(33.0);
    private readonly DispatcherTimer _timer;
    private readonly Action _tickAction;

    public XnaAsyncDispatcher(Action tickAction, TimeSpan dispatchInterval)
    {
      FrameworkDispatcher.Update();
      this._timer = new DispatcherTimer();
      this._timer.Tick+=(new EventHandler(this.TimerTick));
      this._timer.Interval = dispatchInterval;
      this._tickAction = tickAction;
    }

    public XnaAsyncDispatcher(Action tickAction)
      : this(tickAction, XnaAsyncDispatcher._defaultDispatchInterval)
    {
    }

    public void StartService(ApplicationServiceContext context = null)
    {
      this._timer.Start();
    }

    public void StopService()
    {
      this._timer.Stop();
    }

    private void TimerTick(object sender, EventArgs eventArgs)
    {
      try
      {
        FrameworkDispatcher.Update();
      }
      catch (Exception )
      {
      }
      Action tickAction = this._tickAction;
      if (tickAction == null)
        return;
      tickAction();
    }
  }
}
