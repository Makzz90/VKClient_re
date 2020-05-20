using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class EditStatusUC : UserControl
  {
    internal TextBox textBoxText;
    internal Button buttonSave;
    private bool _contentLoaded;

    public TextBox TextBoxText
    {
      get
      {
        return this.textBoxText;
      }
    }

    public Button ButtonSave
    {
      get
      {
        return this.buttonSave;
      }
    }

    public EditStatusUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.EditStatusUC_Loaded));
    }

    private void EditStatusUC_Loaded(object sender, RoutedEventArgs e)
    {
      ((Control) this.textBoxText).Focus();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/EditStatusUC.xaml", UriKind.Relative));
      this.textBoxText = (TextBox) base.FindName("textBoxText");
      this.buttonSave = (Button) base.FindName("buttonSave");
    }
  }
}
