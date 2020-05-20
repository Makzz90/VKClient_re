using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Profiles.Shared.ViewModels;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class MediaListItemTitledUCBase : UserControl
  {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (MediaListItemTitledUCBase), new PropertyMetadata(new PropertyChangedCallback(MediaListItemTitledUCBase.Title_OnChanged)));
    public static readonly DependencyProperty CounterProperty = DependencyProperty.Register("Counter", typeof (string), typeof (MediaListItemTitledUCBase), new PropertyMetadata(new PropertyChangedCallback(MediaListItemTitledUCBase.Counter_OnChanged)));
    internal Canvas canvas;
    internal StackPanel stackPanelTitle;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockCounter;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) this.GetValue(MediaListItemTitledUCBase.TitleProperty);
      }
      set
      {
        this.SetValue(MediaListItemTitledUCBase.TitleProperty, (object) value);
      }
    }

    public string Counter
    {
      get
      {
        return (string) this.GetValue(MediaListItemTitledUCBase.CounterProperty);
      }
      set
      {
        this.SetValue(MediaListItemTitledUCBase.CounterProperty, (object) value);
      }
    }

    public MediaListItemTitledUCBase()
    {
      this.InitializeComponent();
    }

    private static void Title_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((MediaListItemTitledUCBase) d).UpdateTitle();
    }

    private void UpdateTitle()
    {
      this.textBlockTitle.Text = this.Title;
      this.CorrectTitle();
    }

    private void CorrectTitle()
    {
      this.textBlockTitle.CorrectText(this.canvas.ActualWidth - Canvas.GetLeft((UIElement) this.stackPanelTitle) - this.textBlockCounter.ActualWidth);
    }

    private static void Counter_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((MediaListItemTitledUCBase) d).UpdateCounter();
    }

    private void UpdateCounter()
    {
      this.textBlockCounter.Text = string.Format(" {0}", (object) this.Counter);
    }

    private void Canvas_OnTap(object sender, GestureEventArgs e)
    {
      MediaListSectionViewModel sectionViewModel = this.DataContext as MediaListSectionViewModel;
      if (sectionViewModel == null)
        return;
      Action tapAction = sectionViewModel.TapAction;
      if (tapAction == null)
        return;
      tapAction();
    }

    private void TextBlock_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.CorrectTitle();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/MediaListItemTitledUCBase.xaml", UriKind.Relative));
      this.canvas = (Canvas) this.FindName("canvas");
      this.stackPanelTitle = (StackPanel) this.FindName("stackPanelTitle");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.textBlockCounter = (TextBlock) this.FindName("textBlockCounter");
    }
  }
}
