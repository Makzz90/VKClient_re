using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class NewFriendsListUC : UserControl
  {
    internal Grid LayoutRoot;
    internal TextBox textBoxTitle;
    internal Button buttonCreate;
    internal Button buttonSave;
    private bool _contentLoaded;

    public bool IsNew { get; set; }

    public Visibility IsEditListVisibility
    {
      get
      {
        if (!this.IsNew)
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public Visibility IsNewListVisibility
    {
      get
      {
        if (!this.IsNew)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public NewFriendsListUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.UpdateButtonEnabled();
    }

    public void Initialize(bool isNew)
    {
      this.IsNew = isNew;
      base.DataContext = this;
      this.UpdateButtonEnabled();
    }

    private void textBoxTitle_TextChanged_1(object sender, TextChangedEventArgs e)
    {
      this.UpdateButtonEnabled();
    }

    private void UpdateButtonEnabled()
    {
      Button buttonCreate = this.buttonCreate;
      bool flag;
      ((Control) this.buttonSave).IsEnabled = (flag = !string.IsNullOrWhiteSpace(this.textBoxTitle.Text));
      int num = flag ? 1 : 0;
      ((Control) buttonCreate).IsEnabled = (num != 0);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewFriendsListUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBoxTitle = (TextBox) base.FindName("textBoxTitle");
      this.buttonCreate = (Button) base.FindName("buttonCreate");
      this.buttonSave = (Button) base.FindName("buttonSave");
    }
  }
}
