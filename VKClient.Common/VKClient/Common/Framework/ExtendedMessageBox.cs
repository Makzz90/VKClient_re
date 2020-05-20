using System;
using System.Windows;
using VKClient.Common.UC;

namespace VKClient.Common.Framework
{
  public static class ExtendedMessageBox
  {
    public static void ShowSafe(string message)
    {
      Execute.ExecuteOnUIThread((Action) (() => new GenericInfoUC().ShowAndHideLater(message, null)));
    }

    public static void ShowSafe(string message, string title)
    {
      int num;
      Execute.ExecuteOnUIThread((Action) (() => num = (int) MessageBox.Show(message, title, MessageBoxButton.OK)));
    }
  }
}
