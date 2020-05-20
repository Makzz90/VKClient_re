using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.UC
{
  public class ShareContentActionsUC : UserControl
  {
    private bool _contentLoaded;

    public event EventHandler ShareWallPostItemSelected;

    public event EventHandler ShareCommunityItemSelected;

    public ShareContentActionsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void ShareWallPostItem_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.ShareWallPostItemSelected == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.ShareWallPostItemSelected(this, EventArgs.Empty);
    }

    private void ShareCommunityItem_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.ShareCommunityItemSelected == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.ShareCommunityItemSelected(this, EventArgs.Empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ShareContentActionsUC.xaml", UriKind.Relative));
    }
  }
}
