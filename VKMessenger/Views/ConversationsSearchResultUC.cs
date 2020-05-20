using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKMessenger.Views
{
  public class ConversationsSearchResultUC : UserControl
  {
    private bool _contentLoaded;

    public ConversationsSearchResultUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/ConversationsSearchResultUC.xaml", UriKind.Relative));
    }
  }
}
