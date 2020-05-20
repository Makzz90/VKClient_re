using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class MediaListItemEmptyDataUC : UserControl
  {
    private bool _contentLoaded;

    public MediaListItemEmptyDataUC()
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/MediaListItemEmptyDataUC.xaml", UriKind.Relative));
    }
  }
}
