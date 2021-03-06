using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class MediaListItemDiscussionUC : UserControl
  {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (MediaListItemDiscussionUC), new PropertyMetadata(new PropertyChangedCallback(MediaListItemDiscussionUC.Title_OnChanged)));
    private const int TITLE_MAX_WIDTH = 416;
    internal TextBlock textBlockTitle;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) this.GetValue(MediaListItemDiscussionUC.TitleProperty);
      }
      set
      {
        this.SetValue(MediaListItemDiscussionUC.TitleProperty, (object) value);
      }
    }

    public MediaListItemDiscussionUC()
    {
      this.InitializeComponent();
    }

    private static void Title_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((MediaListItemDiscussionUC) d).UpdateTitle(e.NewValue as string ?? "");
    }

    private void UpdateTitle(string newTitle)
    {
      this.textBlockTitle.Text = newTitle;
      this.UpdateLayout();
      double actualWidth = this.textBlockTitle.ActualWidth;
      if (actualWidth <= 0.0 || actualWidth <= 416.0)
        return;
      string text = this.textBlockTitle.Text;
      int length1 = text.Length;
      int length2 = Math.Min((int) (416.0 / (actualWidth / (double) length1)) - 3, length1);
      this.textBlockTitle.Text = text.Substring(0, length2).Trim() + "...";
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/MediaListItemDiscussionUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
    }
  }
}
