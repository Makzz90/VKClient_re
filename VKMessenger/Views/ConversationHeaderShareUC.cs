using System;
using System.Diagnostics;
using System.Windows;
using VKClient.Common.Library.VirtItems;

namespace VKMessenger.Views
{
  public class ConversationHeaderShareUC : UserControlVirtualizable
  {
    private bool _contentLoaded;

    public bool IsLookup { get; set; }

    public ConversationHeaderShareUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/ConversationHeaderShareUC.xaml", UriKind.Relative));
    }
  }
}
