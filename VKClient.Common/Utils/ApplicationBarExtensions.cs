using Microsoft.Phone.Shell;
using System;

namespace VKClient.Common.Utils
{
  public static class ApplicationBarExtensions
  {
    public static void AddButton(this ApplicationBar appBar, string text, string iconUri, Action clickAction = null)
    {
      appBar.AddButton(text, new Uri(iconUri, UriKind.Relative), clickAction);
    }

    public static void AddButton(this ApplicationBar appBar, string text, Uri iconUri, Action clickAction = null)
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      string str = text;
      applicationBarIconButton1.Text = str;
      Uri uri = iconUri;
      applicationBarIconButton1.IconUri = uri;
      ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
      if (clickAction != null)
        applicationBarIconButton2.Click+=((EventHandler) ((sender, args) => clickAction()));
      appBar.Buttons.Add(applicationBarIconButton2);
    }

    public static void AddMenuItem(this ApplicationBar appBar, string text, Action clickAction = null)
    {
      ApplicationBarMenuItem applicationBarMenuItem = new ApplicationBarMenuItem(text);
      if (clickAction != null)
        applicationBarMenuItem.Click+=((EventHandler) ((sender, args) => clickAction()));
      appBar.MenuItems.Add(applicationBarMenuItem);
    }
  }
}
