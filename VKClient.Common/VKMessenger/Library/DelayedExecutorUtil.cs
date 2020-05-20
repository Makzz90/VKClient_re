using System;
using System.ComponentModel;
using System.Threading;

namespace VKMessenger.Library
{
  public static class DelayedExecutorUtil
  {
    public static void Execute(Action myMethod, int delayInMilliseconds)
    {
      if (delayInMilliseconds <= 0)
      {
        myMethod();
      }
      else
      {
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        DoWorkEventHandler workEventHandler = (DoWorkEventHandler) ((s, e) => Thread.Sleep(delayInMilliseconds));
        backgroundWorker.DoWork += workEventHandler;
        RunWorkerCompletedEventHandler completedEventHandler = (RunWorkerCompletedEventHandler) ((s, e) => myMethod());
        backgroundWorker.RunWorkerCompleted += completedEventHandler;
        backgroundWorker.RunWorkerAsync();
      }
    }
  }
}
