using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class GroupGenericHeaderUC : UserControl
  {
      public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(GroupGenericHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GroupGenericHeaderUC.OnTitleChanged)));
    internal TextBlock textBlockTitle;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) base.GetValue(GroupGenericHeaderUC.TitleProperty);
      }
      set
      {
        base.SetValue(GroupGenericHeaderUC.TitleProperty, value);
      }
    }

    public GroupGenericHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GroupGenericHeaderUC groupGenericHeaderUc = (GroupGenericHeaderUC) d;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      groupGenericHeaderUc.textBlockTitle.Text = (!string.IsNullOrEmpty(newValue) ? newValue.ToUpperInvariant() : "");
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GroupGenericHeaderUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
    }
  }
}
