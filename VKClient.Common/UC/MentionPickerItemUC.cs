using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class MentionPickerItemUC : UserControl
  {
    private bool _contentLoaded;

    public MentionPickerItemUC()
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MentionPickerItemUC.xaml", UriKind.Relative));
    }
  }
}
