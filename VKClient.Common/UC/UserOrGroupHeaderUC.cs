using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class UserOrGroupHeaderUC : UserControl
  {
    private long _ownerId;
    internal Image imageUserOrGroup;
    internal TextBlock textBlockName;
    internal TextBlock textBlockDate;
    private bool _contentLoaded;

    public UserOrGroupHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    public void Initilize(string imageUri, string name, string date, long ownerId)
    {
      ImageLoader.SetUriSource(this.imageUserOrGroup, imageUri);
      this.textBlockName.Text = name;
      this.textBlockDate.Text = date;
      this._ownerId = ownerId;
    }

    private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this._ownerId < 0L)
      {
        Navigator.Current.NavigateToGroup(-this._ownerId, this.textBlockName.Text, false);
      }
      else
      {
        if (this._ownerId <= 0L)
          return;
        Navigator.Current.NavigateToUserProfile(this._ownerId, this.textBlockName.Text, "", false);
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/UserOrGroupHeaderUC.xaml", UriKind.Relative));
      this.imageUserOrGroup = (Image) base.FindName("imageUserOrGroup");
      this.textBlockName = (TextBlock) base.FindName("textBlockName");
      this.textBlockDate = (TextBlock) base.FindName("textBlockDate");
    }
  }
}
