using System;
using System.Collections;
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
      public static readonly DependencyProperty IconUrlProperty = DependencyProperty.Register("IconUrl", typeof(string), typeof(InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnIconUrlChanged)));
      public static readonly DependencyProperty InlinesProperty = DependencyProperty.Register("Inlines", typeof(InlinesCollection), typeof(InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnInlinesChanged)));
      public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnTextChanged)));
      public static readonly DependencyProperty Preview1UrlProperty = DependencyProperty.Register("Preview1Url", typeof(string), typeof(InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnPreview1UrlChanged)));
      public static readonly DependencyProperty Preview2UrlProperty = DependencyProperty.Register("Preview2Url", typeof(string), typeof(InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnPreview2UrlChanged)));
      public static readonly DependencyProperty Preview3UrlProperty = DependencyProperty.Register("Preview3Url", typeof(string), typeof(InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnPreview3UrlChanged)));
      public static readonly DependencyProperty IsTiltEnabledProperty = DependencyProperty.Register("IsTiltEnabled", typeof(bool), typeof(InfoListItemUC), new PropertyMetadata(new PropertyChangedCallback(InfoListItemUC.OnIsTiltEnabledChanged)));
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
        return (string) base.GetValue(InfoListItemUC.IconUrlProperty);
      }
      set
      {
        base.SetValue(InfoListItemUC.IconUrlProperty, value);
      }
    }

    public InlinesCollection Inlines
    {
      get
      {
        return (InlinesCollection) base.GetValue(InfoListItemUC.InlinesProperty);
      }
      set
      {
        base.SetValue(InfoListItemUC.InlinesProperty, value);
      }
    }

    public string Text
    {
      get
      {
        return (string) base.GetValue(InfoListItemUC.TextProperty);
      }
      set
      {
        base.SetValue(InfoListItemUC.TextProperty, value);
      }
    }

    public string Preview1Url
    {
      get
      {
        return (string) base.GetValue(InfoListItemUC.Preview1UrlProperty);
      }
      set
      {
        base.SetValue(InfoListItemUC.Preview1UrlProperty, value);
      }
    }

    public string Preview2Url
    {
      get
      {
        return (string) base.GetValue(InfoListItemUC.Preview2UrlProperty);
      }
      set
      {
        base.SetValue(InfoListItemUC.Preview2UrlProperty, value);
      }
    }

    public string Preview3Url
    {
      get
      {
        return (string) base.GetValue(InfoListItemUC.Preview3UrlProperty);
      }
      set
      {
        base.SetValue(InfoListItemUC.Preview3UrlProperty, value);
      }
    }

    public bool IsTiltEnabled
    {
      get
      {
        return (bool) base.GetValue(InfoListItemUC.IsTiltEnabledProperty);
      }
      set
      {
        base.SetValue(InfoListItemUC.IsTiltEnabledProperty, value);
      }
    }

    public InfoListItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.textBlockContent.Text = ("");
      ((UIElement) this.gridPreview1).Visibility = Visibility.Collapsed;
      ((UIElement) this.gridPreview2).Visibility = Visibility.Collapsed;
      ((UIElement) this.gridPreview3).Visibility = Visibility.Collapsed;
    }

    private static void OnIconUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((InfoListItemUC) d).UpdateIcon(e.NewValue as string);
    }

    private void UpdateIcon(string url)
    {
      ImageBrush imageBrush = new ImageBrush();
      ((UIElement) this.borderIcon).OpacityMask=((Brush) imageBrush);
      if (string.IsNullOrEmpty(url))
        return;
      ImageLoader.SetImageBrushMultiResSource(imageBrush, url);
    }

    private static void OnInlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((InfoListItemUC) d).UpdateInlines(e.NewValue as InlinesCollection);
    }

    private void UpdateInlines(InlinesCollection inlines)
    {
      ((PresentationFrameworkCollection<Inline>) this.textBlockContent.Inlines).Clear();
      if (inlines == null)
        return;
      this.textBlockContent.Text = ("");
      using (List<Inline>.Enumerator enumerator = inlines.GetEnumerator())
      {
        while (enumerator.MoveNext())
          ((PresentationFrameworkCollection<Inline>) this.textBlockContent.Inlines).Add(enumerator.Current);
      }
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((InfoListItemUC) d).UpdateText(e.NewValue as string);
    }

    private void UpdateText(string text)
    {
      base.Tap -= (new EventHandler<System.Windows.Input.GestureEventArgs>(this.TextBlockContent_OnTap));
      ((PresentationFrameworkCollection<Inline>) this.textBlockContent.Inlines).Clear();
      text = text != null ? text.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Replace("  ", " ") : "";
      text = Regex.Replace(text, "\\s+", " ");
      TextBlock textBlock = new TextBlock();
      double num1 = 306.0;
      ((FrameworkElement) textBlock).Width = num1;
      int num2 = 2;
      textBlock.TextWrapping=((TextWrapping) num2);
      int num3 = 1;
      textBlock.LineStackingStrategy=((LineStackingStrategy) num3);
      double num4 = 26.0;
      textBlock.LineHeight = num4;
      string str = text;
      textBlock.Text = str;
      this.textBlockContent.Text = (UIStringFormatterHelper.SubstituteMentionsWithNames(text));
      if (((FrameworkElement) textBlock).ActualHeight <= 78.0)
        return;
      this.IsTiltEnabled = true;
      base.Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.TextBlockContent_OnTap));
    }

    private static void OnPreview1UrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      InfoListItemUC infoListItemUc = (InfoListItemUC) d;
      // ISSUE: explicit reference operation
      InfoListItemUC.UpdatePreviewUrl((UIElement) infoListItemUc.gridPreview1, infoListItemUc.imagePreview1, e.NewValue as string);
    }

    private static void OnPreview2UrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      InfoListItemUC infoListItemUc = (InfoListItemUC) d;
      // ISSUE: explicit reference operation
      InfoListItemUC.UpdatePreviewUrl((UIElement) infoListItemUc.gridPreview2, infoListItemUc.imagePreview2, e.NewValue as string);
    }

    private static void OnPreview3UrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      InfoListItemUC infoListItemUc = (InfoListItemUC) d;
      // ISSUE: explicit reference operation
      InfoListItemUC.UpdatePreviewUrl((UIElement) infoListItemUc.gridPreview3, infoListItemUc.imagePreview3, e.NewValue as string);
    }

    private static void UpdatePreviewUrl(UIElement gridPreview, Image imagePreview, string url)
    {
      ImageLoader.SetUriSource(imagePreview, url);
      gridPreview.Visibility = (!string.IsNullOrEmpty(url) ? Visibility.Visible : Visibility.Collapsed);
    }

    private static void OnIsTiltEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      MetroInMotion.SetTilt((DependencyObject) ((InfoListItemUC) d).gridRoot, (bool) e.NewValue ? 1.5 : 0.0);
    }

    private void TextBlockContent_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      TextBlock textBlock = (TextBlock) sender;
      Size newSize = e.NewSize;
      // ISSUE: explicit reference operation
      double height = ((Size) @newSize).Height;
      if (height <= textBlock.LineHeight)
        return;
      using (IEnumerator<Inline> enumerator = ((PresentationFrameworkCollection<Inline>) textBlock.Inlines).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          Run current = enumerator.Current as Run;
          if (current != null && current.Text.Contains(" · "))
            current.Text = (current.Text.Replace(" · ", "\n"));
        }
      }
      if (height <= ((FrameworkElement) textBlock).MaxHeight)
        return;
      this.IsTiltEnabled = true;
      ((FrameworkElement) this.textBlockContent).MaxHeight=(((FrameworkElement) textBlock).MaxHeight);
      base.Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.TextBlockContent_OnTap));
    }

    private void TextBlockContent_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ((UIElement) this.textBlockContent).Tap -= (new EventHandler<System.Windows.Input.GestureEventArgs>(this.TextBlockContent_OnTap));
      // ISSUE: method pointer
      ((FrameworkElement) this.textBlockContent).SizeChanged-=(new SizeChangedEventHandler( this.TextBlockContent_OnSizeChanged));
      this.IsTiltEnabled = false;
      ((UIElement) this.textBlockContent).Visibility = Visibility.Collapsed;
      this.textBlockContentFull.Text = this.Text;
      ((UIElement) this.textBlockContentFull).Visibility = Visibility.Visible;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/InfoListItemUC.xaml", UriKind.Relative));
      this.gridRoot = (Grid) base.FindName("gridRoot");
      this.borderIcon = (Border) base.FindName("borderIcon");
      this.textBlockContent = (TextBlock) base.FindName("textBlockContent");
      this.textBlockContentFull = (ScrollableTextBlock) base.FindName("textBlockContentFull");
      this.gridPreview1 = (Grid) base.FindName("gridPreview1");
      this.imagePreview1 = (Image) base.FindName("imagePreview1");
      this.gridPreview2 = (Grid) base.FindName("gridPreview2");
      this.imagePreview2 = (Image) base.FindName("imagePreview2");
      this.gridPreview3 = (Grid) base.FindName("gridPreview3");
      this.imagePreview3 = (Image) base.FindName("imagePreview3");
    }
  }
}
