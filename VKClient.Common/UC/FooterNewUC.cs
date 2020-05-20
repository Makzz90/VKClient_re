using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class FooterNewUC : UserControl
  {
    private bool _contentLoaded;

    public FooterNewUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void ButtonTryAgain_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      ISupportReload dataContext = ((FrameworkElement) sender).DataContext as ISupportReload;
      if (dataContext == null)
        return;
      dataContext.Reload();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/FooterNewUC.xaml", UriKind.Relative));
    }
  }
}
