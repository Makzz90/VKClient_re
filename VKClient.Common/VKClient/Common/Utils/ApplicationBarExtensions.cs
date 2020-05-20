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
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton()
      {
        Text = text,
        IconUri = iconUri
      };
      if (clickAction != null)
        applicationBarIconButton.Click += (EventHandler) ((sender, args) => clickAction());
      appBar.Buttons.Add((object) applicationBarIconButton);
    }

    public static void AddMenuItem(this ApplicationBar appBar, string text, Action clickAction = null)
    {
      ApplicationBarMenuItem applicationBarMenuItem = new ApplicationBarMenuItem(text);
      if (clickAction != null)
        applicationBarMenuItem.Click += (EventHandler) ((sender, args) => clickAction());
      appBar.MenuItems.Add((object) applicationBarMenuItem);
    }
  }
}
