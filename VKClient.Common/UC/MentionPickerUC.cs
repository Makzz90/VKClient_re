using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class MentionPickerUC : UserControl
  {
    private List<FriendHeader> _itemsSource;
    private bool _isVisible;
    internal Grid mainPanel;
    internal ExtendedLongListSelector listBox;
    private bool _contentLoaded;

    public char MentionStartSymbol { get; set; }

    public List<FriendHeader> ItemsSource
    {
      get
      {
        return this._itemsSource;
      }
      set
      {
        this._itemsSource = value;
        this.listBox.ItemsSource = ((IList) value);
        this.UpdateSize();
      }
    }

    public string SearchDomain { get; set; }

    public bool IsVisible
    {
      get
      {
        return this._isVisible;
      }
      set
      {
        if (this._isVisible == value)
          return;
        this._isVisible = value;
        if (value)
          this.Show();
        else
          this.Hide();
      }
    }

    public event EventHandler<object> ItemSelected;

    public event EventHandler Closed;

    public MentionPickerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    public void Show()
    {
      base.Visibility = Visibility.Visible;
      this.UpdateSize();
    }

    public void Hide()
    {
      base.Visibility = Visibility.Collapsed;
      // ISSUE: reference to a compiler-generated field
      EventHandler closed = this.Closed;
      if (closed == null)
        return;
      EventArgs empty = EventArgs.Empty;
      closed(this, empty);
    }

    private void UpdateSize()
    {
      if (base.Visibility == Visibility.Collapsed)
        return;
      int num = this.ItemsSource.Count * 48 + 16;
      if (num > 188)
        num = 188;
      ((FrameworkElement) this.mainPanel).Height=((double) num);
      ((FrameworkElement) this.mainPanel).Margin=(new Thickness(0.0, (double) (-num - 1), 0.0, 0.0));
    }

    private void listBox_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      object selectedItem = this.listBox.SelectedItem;
      if (selectedItem == null)
        return;
      // ISSUE: reference to a compiler-generated field
      EventHandler<object> itemSelected = this.ItemSelected;
      if (itemSelected == null)
        return;
      object e1 = selectedItem;
      itemSelected(this, e1);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/MentionPickerUC.xaml", UriKind.Relative));
      this.mainPanel = (Grid) base.FindName("mainPanel");
      this.listBox = (ExtendedLongListSelector) base.FindName("listBox");
    }
  }
}
