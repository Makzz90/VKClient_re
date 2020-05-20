using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Audio.UserControls
{
  public class EditAlbumUC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBlock textBlockCaption;
    internal TextBox textBoxText;
    internal Button buttonSave;
    private bool _contentLoaded;

    public EditAlbumUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void textBoxText_TextChanged(object sender, TextChangedEventArgs e)
    {
      ((Control) this.buttonSave).IsEnabled = (this.textBoxText.Text.Length >= 2);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Audio;component/UserControls/EditAlbumUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBlockCaption = (TextBlock) base.FindName("textBlockCaption");
      this.textBoxText = (TextBox) base.FindName("textBoxText");
      this.buttonSave = (Button) base.FindName("buttonSave");
    }
  }
}
