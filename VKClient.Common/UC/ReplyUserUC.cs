using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class ReplyUserUC : UserControl
  {
      public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ReplyUserUC), new PropertyMetadata(new PropertyChangedCallback(ReplyUserUC.OnTitleChanged)));
    internal TextBlock textBlockTitle;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) base.GetValue(ReplyUserUC.TitleProperty);
      }
      set
      {
        base.SetValue(ReplyUserUC.TitleProperty, value);
      }
    }

    public event EventHandler TitleChanged;

    public ReplyUserUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.textBlockTitle.Text=("");
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ReplyUserUC replyUserUc = d as ReplyUserUC;
      if (replyUserUc == null)
        return;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      replyUserUc.textBlockTitle.Text = (!string.IsNullOrEmpty(newValue) ? newValue : "");
      replyUserUc.FireTitleChangedEvent();
    }

    private void FireTitleChangedEvent()
    {
      // ISSUE: reference to a compiler-generated field
      if (this.TitleChanged == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.TitleChanged(this, EventArgs.Empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ReplyUserUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
    }
  }
}
