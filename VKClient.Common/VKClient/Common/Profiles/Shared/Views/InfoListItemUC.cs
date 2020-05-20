using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Profiles.Shared.ViewModels;
using VKClient.Common.Utils;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class InfoListItemUC : UserControl
  {
    public static readonly DependencyProperty IconUrlProperty = DependencyProperty.Register("IconUrl", typeof (string), typeof (InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnIconUrlChanged)));
    public static readonly DependencyProperty InlinesProperty = DependencyProperty.Register("Inlines", typeof (InlinesCollection), typeof (InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnInlinesChanged)));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string), typeof (InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnTextChanged)));
    public static readonly DependencyProperty Preview1UrlProperty = DependencyProperty.Register("Preview1Url", typeof (string), typeof (InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnPreview1UrlChanged)));
    public static readonly DependencyProperty Preview2UrlProperty = DependencyProperty.Register("Preview2Url", typeof (string), typeof (InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnPreview2UrlChanged)));
    public static readonly DependencyProperty Preview3UrlProperty = DependencyProperty.Register("Preview3Url", typeof (string), typeof (InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnPreview3UrlChanged)));
    public static readonly DependencyProperty IsTiltEnabledProperty = DependencyProperty.Register("IsTiltEnabled", typeof (bool), typeof (InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnIsTiltEnabledChanged)));
    internal Grid gridRoot;
    internal Border borderIcon;
    internal TextBlock textBlockContent;
    internal ScrollableTextBlock textBlockContentFull;
    internal Grid gridPreview1;
    internal Image imagePreview1;
    internal Grid gridPreview2;
    internal Image imagePreview2;
    internal Grid gridPreview3;
    internal Image imagePreview3;
    private bool _contentLoaded;

    public string IconUrl
    {
      get
      {
        return (string) this.GetValue(InfoListItemUC.IconUrlProperty);
      }
      set
      {
        this.SetValue(InfoListItemUC.IconUrlProperty, (object) value);
      }
    }

    public InlinesCollection Inlines
    {
      get
      {
        return (InlinesCollection) this.GetValue(InfoListItemUC.InlinesProperty);
      }
      set
      {
        this.SetValue(InfoListItemUC.InlinesProperty, (object) value);
      }
    }

    public string Text
    {
      get
      {
        return (string) this.GetValue(InfoListItemUC.TextProperty);
      }
      set
      {
        this.SetValue(InfoListItemUC.TextProperty, (object) value);
      }
    }

    public string Preview1Url
    {
      get
      {
        return (string) this.GetValue(InfoListItemUC.Preview1UrlProperty);
      }
      set
      {
        this.SetValue(InfoListItemUC.Preview1UrlProperty, (object) value);
      }
    }

    public string Preview2Url
    {
      get
      {
        return (string) this.GetValue(InfoListItemUC.Preview2UrlProperty);
      }
      set
      {
        this.SetValue(InfoListItemUC.Preview2UrlProperty, (object) value);
      }
    }

    public string Preview3Url
    {
      get
      {
        return (string) this.GetValue(InfoListItemUC.Preview3UrlProperty);
      }
      set
      {
        this.SetValue(InfoListItemUC.Preview3UrlProperty, (object) value);
      }
    }

    public bool IsTiltEnabled
    {
      get
      {
        return (bool) this.GetValue(InfoListItemUC.IsTiltEnabledProperty);
      }
      set
      {
        this.SetValue(InfoListItemUC.IsTiltEnabledProperty, (object) value);
      }
    }

    public InfoListItemUC()
    {
      this.InitializeComponent();
      this.textBlockContent.Text = "";
      this.gridPreview1.Visibility = Visibility.Collapsed;
      this.gridPreview2.Visibility = Visibility.Collapsed;
      this.gridPreview3.Visibility = Visibility.Collapsed;
    }

    private static void OnIconUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((InfoListItemUC) d).UpdateIcon(e.NewValue as string);
    }

    private void UpdateIcon(string url)
    {
      ImageBrush imageBrush = new ImageBrush();
      this.borderIcon.OpacityMask = (Brush) imageBrush;
      if (string.IsNullOrEmpty(url))
        return;
      ImageLoader.SetImageBrushMultiResSource(imageBrush, url);
    }

    private static void OnInlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((InfoListItemUC) d).UpdateInlines(e.NewValue as InlinesCollection);
    }

    private void UpdateInlines(InlinesCollection inlines)
    {
      this.textBlockContent.Inlines.Clear();
      if (inlines == null)
        return;
      this.textBlockContent.Text = "";
      foreach (Inline inline in (List<Inline>) inlines)
        this.textBlockContent.Inlines.Add(inline);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((InfoListItemUC) d).UpdateText(e.NewValue as string);
    }

    private void UpdateText(string text)
    {
      this.Tap -= new EventHandler<GestureEventArgs>(this.TextBlockContent_OnTap);
      this.textBlockContent.Inlines.Clear();
      text = text != null ? text.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Replace("  ", " ") : "";
      text = Regex.Replace(text, "\\s+", " ");
      TextBlock textBlock = new TextBlock();
      double num1 = 306.0;
      textBlock.Width = num1;
      int num2 = 2;
      textBlock.TextWrapping = (TextWrapping) num2;
      int num3 = 1;
      textBlock.LineStackingStrategy = (LineStackingStrategy) num3;
      double num4 = 26.0;
      textBlock.LineHeight = num4;
      string str = text;
      textBlock.Text = str;
      this.textBlockContent.Text = UIStringFormatterHelper.SubstituteMentionsWithNames(text);
      if (textBlock.ActualHeight <= 78.0)
        return;
      this.IsTiltEnabled = true;
      this.Tap += new EventHandler<GestureEventArgs>(this.TextBlockContent_OnTap);
    }

    private static void OnPreview1UrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      InfoListItemUC infoListItemUc = (InfoListItemUC) d;
      InfoListItemUC.UpdatePreviewUrl((UIElement) infoListItemUc.gridPreview1, infoListItemUc.imagePreview1, e.NewValue as string);
    }

    private static void OnPreview2UrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      InfoListItemUC infoListItemUc = (InfoListItemUC) d;
      InfoListItemUC.UpdatePreviewUrl((UIElement) infoListItemUc.gridPreview2, infoListItemUc.imagePreview2, e.NewValue as string);
    }

    private static void OnPreview3UrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      InfoListItemUC infoListItemUc = (InfoListItemUC) d;
      InfoListItemUC.UpdatePreviewUrl((UIElement) infoListItemUc.gridPreview3, infoListItemUc.imagePreview3, e.NewValue as string);
    }

    private static void UpdatePreviewUrl(UIElement gridPreview, Image imagePreview, string url)
    {
      ImageLoader.SetUriSource(imagePreview, url);
      gridPreview.Visibility = !string.IsNullOrEmpty(url) ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void OnIsTiltEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      MetroInMotion.SetTilt((DependencyObject) ((InfoListItemUC) d).gridRoot, (bool) e.NewValue ? 1.5 : 0.0);
    }

    private void TextBlockContent_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      TextBlock textBlock = (TextBlock) sender;
      double height = e.NewSize.Height;
      if (height <= textBlock.LineHeight)
        return;
      foreach (Inline inline in (PresentationFrameworkCollection<Inline>) textBlock.Inlines)
      {
        Run run = inline as Run;
        if (run != null && run.Text.Contains(" · "))
          run.Text = run.Text.Replace(" · ", "\n");
      }
      if (height <= textBlock.MaxHeight)
        return;
      this.IsTiltEnabled = true;
      this.textBlockContent.MaxHeight = textBlock.MaxHeight;
      this.Tap += new EventHandler<GestureEventArgs>(this.TextBlockContent_OnTap);
    }

    private void TextBlockContent_OnTap(object sender, GestureEventArgs e)
    {
      this.textBlockContent.Tap -= new EventHandler<GestureEventArgs>(this.TextBlockContent_OnTap);
      this.textBlockContent.SizeChanged -= new SizeChangedEventHandler(this.TextBlockContent_OnSizeChanged);
      this.IsTiltEnabled = false;
      this.textBlockContent.Visibility = Visibility.Collapsed;
      this.textBlockContentFull.Text = this.Text;
      this.textBlockContentFull.Visibility = Visibility.Visible;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/InfoListItemUC.xaml", UriKind.Relative));
      this.gridRoot = (Grid) this.FindName("gridRoot");
      this.borderIcon = (Border) this.FindName("borderIcon");
      this.textBlockContent = (TextBlock) this.FindName("textBlockContent");
      this.textBlockContentFull = (ScrollableTextBlock) this.FindName("textBlockContentFull");
      this.gridPreview1 = (Grid) this.FindName("gridPreview1");
      this.imagePreview1 = (Image) this.FindName("imagePreview1");
      this.gridPreview2 = (Grid) this.FindName("gridPreview2");
      this.imagePreview2 = (Image) this.FindName("imagePreview2");
      this.gridPreview3 = (Grid) this.FindName("gridPreview3");
      this.imagePreview3 = (Image) this.FindName("imagePreview3");
    }
  }
}
