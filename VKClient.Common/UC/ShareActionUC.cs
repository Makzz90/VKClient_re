using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class ShareActionUC : UserControl
  {
      public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(ShareActionUC), new PropertyMetadata(new PropertyChangedCallback(ShareActionUC.OnIconChanged)));
      public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ShareActionUC), new PropertyMetadata(new PropertyChangedCallback(ShareActionUC.OnTitleChanged)));
    internal ImageBrush imageBrushIcon;
    internal TextBlock textBlockTitle;
    private bool _contentLoaded;

    public string Icon
    {
      get
      {
        return (string) base.GetValue(ShareActionUC.IconProperty);
      }
      set
      {
        base.SetValue(ShareActionUC.IconProperty, value);
      }
    }

    public string Title
    {
      get
      {
        return (string) base.GetValue(ShareActionUC.TitleProperty);
      }
      set
      {
        base.SetValue(ShareActionUC.TitleProperty, value);
      }
    }

    public ShareActionUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ShareActionUC shareActionUc = d as ShareActionUC;
      if (shareActionUc == null)
        return;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      if (string.IsNullOrEmpty(newValue))
        return;
      ImageLoader.SetImageBrushMultiResSource(shareActionUc.imageBrushIcon, newValue);
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ShareActionUC shareActionUc = d as ShareActionUC;
      if (shareActionUc == null)
        return;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      if (string.IsNullOrEmpty(newValue))
        return;
      shareActionUc.textBlockTitle.Text = newValue;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ShareActionUC.xaml", UriKind.Relative));
      this.imageBrushIcon = (ImageBrush) base.FindName("imageBrushIcon");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
    }
  }
}
