using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Photos.UC
{
  public class EditPhotoTextUC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBox textBoxText;
    internal Button buttonSave;
    private bool _contentLoaded;

    public Button ButtonSave
    {
      get
      {
        return this.buttonSave;
      }
    }

    public TextBox TextBoxText
    {
      get
      {
        return this.textBoxText;
      }
    }

    public EditPhotoTextUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.EditPhotoTextUC_Loaded));
    }

    private void EditPhotoTextUC_Loaded(object sender, RoutedEventArgs e)
    {
        ((DependencyObject)Deployment.Current).Dispatcher.BeginInvoke((Action)(() =>
        {
            ((Control)this.textBoxText).Focus();
            this.textBoxText.Select(this.textBoxText.Text.Length, 0);
        }));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Photos;component/UC/EditPhotoTextUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBoxText = (TextBox) base.FindName("textBoxText");
      this.buttonSave = (Button) base.FindName("buttonSave");
    }
  }
}
