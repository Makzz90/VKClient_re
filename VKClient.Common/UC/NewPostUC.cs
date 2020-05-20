using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class NewPostUC : UserControl
  {
    private bool _isLoaded;
    private ScrollViewer _scroll;
    private double savedHeight;
    internal Grid LayoutRoot;
    internal TextBox textBoxPost;
    internal TextBlock textBlockWatermarkText;
    internal ItemsControl itemsControlAttachments;
    private bool _contentLoaded;

    public TextBox TextBoxPost
    {
      get
      {
        return this.textBoxPost;
      }
    }

    public TextBlock TextBlockWatermarkText
    {
      get
      {
        return this.textBlockWatermarkText;
      }
    }

    public ItemsControl ItemsControlAttachments
    {
      get
      {
        return this.itemsControlAttachments;
      }
    }

    public Action<object> OnImageDeleteTap { get; set; }

    public Action OnAddAttachmentTap { get; set; }

    public bool IsFocused { get; set; }

    public NewPostUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.NewPostUC_Loaded));
    }

    private void NewPostUC_Loaded(object sender, RoutedEventArgs e)
    {
      if (this._isLoaded)
        return;
      this._scroll = Enumerable.FirstOrDefault<DependencyObject>(this.Ancestors<ScrollViewer>()) as ScrollViewer;
      ((UIElement) this.textBlockWatermarkText).Opacity = (this.textBoxPost.Text == "" ? 1.0 : 0.0);
      this._isLoaded = true;
    }

    private void textBoxPost_TextChanged_1(object sender, TextChangedEventArgs e)
    {
      ((UIElement) this.textBlockWatermarkText).Opacity = (this.textBoxPost.Text == "" ? 1.0 : 0.0);
      base.Dispatcher.BeginInvoke((Action) (() =>
      {
        double actualHeight = ((FrameworkElement) this.textBoxPost).ActualHeight;
        Thickness padding = ((Control) this.textBoxPost).Padding;
        // ISSUE: explicit reference operation
        double bottom = ((Thickness) @padding).Bottom;
        double num = actualHeight - bottom;
        if (this.savedHeight > 0.0)
        {
          bool flag = false;
          if (num < this.savedHeight && this._scroll.ExtentHeight == this._scroll.VerticalOffset + this._scroll.ViewportHeight)
            flag = true;
          if (!flag)
            this._scroll.ScrollToOffsetWithAnimation(this._scroll.VerticalOffset + num - this.savedHeight, 0.15, false);
        }
        this.savedHeight = num;
      }));
    }

    private void Image_Delete_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnImageDeleteTap == null)
        return;
      this.OnImageDeleteTap(sender);
      this.ForceFocusIfNeeded();
    }

    private void AddAttachmentTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnAddAttachmentTap == null)
        return;
      this.OnAddAttachmentTap();
    }

    private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ForceFocusIfNeeded();
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      IOutboundAttachment dataContext1 = frameworkElement.DataContext as IOutboundAttachment;
      WallPostViewModel dataContext2 = base.DataContext as WallPostViewModel;
      if (dataContext2 == null || dataContext1 == null)
        return;
      dataContext2.UploadAttachment(dataContext1,  null);
    }

    private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ForceFocusIfNeeded();
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      IOutboundAttachment dataContext1 = frameworkElement.DataContext as IOutboundAttachment;
      if (dataContext1 is IHandleTap)
      {
        (dataContext1 as IHandleTap).OnTap();
      }
      else
      {
        if (dataContext1 == null)
          return;
        WallPostViewModel dataContext2 = base.DataContext as WallPostViewModel;
        if (dataContext2 == null || dataContext1 == null)
          return;
        dataContext2.UploadAttachment(dataContext1,  null);
      }
    }

    public void ForceFocusIfNeeded()
    {
      if (!this.IsFocused)
        return;
      ((Control) this.textBoxPost).Focus();
      TextBoxPanelControl textBoxPanelControl = Enumerable.FirstOrDefault<DependencyObject>(((DependencyObject) FramePageUtils.CurrentPage).Descendants<TextBoxPanelControl>()) as TextBoxPanelControl;
      if (textBoxPanelControl == null)
        return;
      textBoxPanelControl.IgnoreNextLostGotFocus();
    }

    private void Rectangle_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      Rectangle rectangle = sender as Rectangle;
      if (rectangle == null)
        return;
      ((UIElement) rectangle).Opacity = 0.3;
    }

    private void Rectangle_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      Rectangle rectangle = sender as Rectangle;
      if (rectangle == null)
        return;
      ((UIElement) rectangle).Opacity = 0.2;
    }

    private void Rectangle_ManipulationStarted2(object sender, ManipulationStartedEventArgs e)
    {
      Rectangle rectangle = sender as Rectangle;
      if (rectangle == null)
        return;
      ((UIElement) rectangle).Opacity = 0.05;
    }

    private void Rectangle_ManipulationCompleted2(object sender, ManipulationCompletedEventArgs e)
    {
      Rectangle rectangle = sender as Rectangle;
      if (rectangle == null)
        return;
      ((UIElement) rectangle).Opacity = 0.0;
    }

    private void itemsControlAttachments_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      ICollection itemsSource = this.itemsControlAttachments.ItemsSource as ICollection;
      if (itemsSource == null)
        return;
      bool flag = itemsSource.Count > 0;
      ((Control) this.textBoxPost).Padding=(flag ? new Thickness(0.0, 0.0, 0.0, 100.0) :  new Thickness());
      ((FrameworkElement) this.itemsControlAttachments).Margin=(flag ? new Thickness(-6.0, -105.0, -6.0, 6.0) : new Thickness(-6.0, -5.0, -6.0, 6.0));
    }

    private void TextBlock_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      TextBlock textName = (TextBlock) sender;
      OutboundAttachmentBase dataContext = ((FrameworkElement) textName).DataContext as OutboundAttachmentBase;
      if (dataContext == null)
        return;
      textName.CorrectText(dataContext.Width - 8.0);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewPostUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBoxPost = (TextBox) base.FindName("textBoxPost");
      this.textBlockWatermarkText = (TextBlock) base.FindName("textBlockWatermarkText");
      this.itemsControlAttachments = (ItemsControl) base.FindName("itemsControlAttachments");
    }
  }
}
