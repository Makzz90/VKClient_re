using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class MediaListItemVideoUC : UserControl
  {
      public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MediaListItemVideoUC), new PropertyMetadata(new PropertyChangedCallback(MediaListItemVideoUC.Title_OnChanged)));
    private const int TITLE_MAX_WIDTH = 256;
    internal TextBlock textBlockTitle;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) base.GetValue(MediaListItemVideoUC.TitleProperty);
      }
      set
      {
        base.SetValue(MediaListItemVideoUC.TitleProperty, value);
      }
    }

    public MediaListItemVideoUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void Title_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((MediaListItemVideoUC) d).UpdateTitle(e.NewValue as string ?? "");
    }

    private void UpdateTitle(string newTitle)
    {
      this.textBlockTitle.Text = newTitle;
      base.UpdateLayout();
      double actualWidth = ((FrameworkElement) this.textBlockTitle).ActualWidth;
      if (actualWidth <= 0.0 || actualWidth <= 256.0)
        return;
      string text = this.textBlockTitle.Text;
      int length1 = text.Length;
      int length2 = Math.Min((int) (256.0 / (actualWidth / (double) length1)) - 3, length1);
      this.textBlockTitle.Text = (text.Substring(0, length2).Trim() + "...");
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/MediaListItemVideoUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
    }
  }
}
