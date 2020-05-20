using System;
using System.Windows;

namespace VKClient.Common.Framework
{
  public class Execute
  {
    public static void ExecuteOnUIThread(Action action)
    {
      if (((DependencyObject) Deployment.Current).Dispatcher.CheckAccess())
        action();
      else
        ((DependencyObject) Deployment.Current).Dispatcher.BeginInvoke(action);
    }
  }
}
