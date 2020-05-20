using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class MediaListItemPhotoUC : UserControl
  {
    private bool _contentLoaded;

    public MediaListItemPhotoUC()
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/MediaListItemPhotoUC.xaml", UriKind.Relative));
    }
  }
}
