using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.Extensions;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class MediaListItemProductUC : UserControl
  {
      public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MediaListItemProductUC), new PropertyMetadata(new PropertyChangedCallback(MediaListItemProductUC.Title_OnChanged)));
    private const int TITLE_MAX_WIDTH = 180;
    internal TextBlock textBlockTitle;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) base.GetValue(MediaListItemProductUC.TitleProperty);
      }
      set
      {
        base.SetValue(MediaListItemProductUC.TitleProperty, value);
      }
    }

    public MediaListItemProductUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void Title_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((MediaListItemProductUC) d).UpdateTitle(e.NewValue as string ?? "");
    }

    private void UpdateTitle(string newTitle)
    {
      this.textBlockTitle.Text = newTitle;
      this.textBlockTitle.CorrectText(180.0);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/MediaListItemProductUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
    }
  }
}
