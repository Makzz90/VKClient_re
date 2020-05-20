using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.UC
{
  public class GroupFooterUC : UserControl
  {
      public static readonly DependencyProperty FooterTextProperty = DependencyProperty.Register("FooterText", typeof(string), typeof(GroupFooterUC), new PropertyMetadata(new PropertyChangedCallback(GroupFooterUC.OnFooterTextChanged)));
    internal TextBlock textBlockFooter;
    private bool _contentLoaded;

    public string FooterText
    {
      get
      {
        return (string) base.GetValue(GroupFooterUC.FooterTextProperty);
      }
      set
      {
        base.SetValue(GroupFooterUC.FooterTextProperty, value);
      }
    }

    public event EventHandler MoreTapped;

    public GroupFooterUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void OnFooterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GroupFooterUC groupFooterUc = d as GroupFooterUC;
      if (groupFooterUc == null)
        return;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      groupFooterUc.textBlockFooter.Text = (!string.IsNullOrEmpty(newValue) ? newValue : "");
    }

    private void More_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.MoreTapped == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.MoreTapped(this, EventArgs.Empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GroupFooterUC.xaml", UriKind.Relative));
      this.textBlockFooter = (TextBlock) base.FindName("textBlockFooter");
    }
  }
}
