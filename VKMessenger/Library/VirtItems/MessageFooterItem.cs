using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Localization;

namespace VKMessenger.Library.VirtItems
{
  public class MessageFooterItem : VirtualizableItemBase
  {
    private StackPanel _stackPanelCannotSend;
    private ProgressBar _progressBar;
    private Grid _gridDateStatus;
    private StackPanel _stackPanelDateStatus;
    private TextBlock _dateTextBlock;
    private MessageViewModel _mvm;
    private double _verticalWidth;
    private double _horizontalWidth;
    private bool _isHorizontalOrientation;
    private Border _borderStatus;

    public bool IsHorizontalOrientation
    {
      get
      {
        return this._isHorizontalOrientation;
      }
      set
      {
        if (this._isHorizontalOrientation == value)
          return;
        this._isHorizontalOrientation = value;
        this.UpdateLayout();
      }
    }

    public bool IsSticker
    {
      get
      {
        if (this._mvm.Attachments != null)
            return Enumerable.Any<AttachmentViewModel>(this._mvm.Attachments, (Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Sticker));
        return false;
      }
    }

    private bool IsStickerOrGraffiti
    {
      get
      {
          return Enumerable.Any<AttachmentViewModel>(this._mvm.Attachments, (Func<AttachmentViewModel, bool>)(a =>
        {
          if (a.AttachmentType == AttachmentType.Sticker)
            return true;
          if (a.AttachmentType != AttachmentType.Document)
            return false;
          Attachment attachment = a.Attachment;
          bool? nullable1;
          if (attachment == null)
          {
            nullable1 = new bool?();
          }
          else
          {
            Doc doc = attachment.doc;
            nullable1 = doc != null ? new bool?(doc.IsGraffiti) : new bool?();
          }
          bool? nullable2 = nullable1;
          if (!nullable2.HasValue)
            return false;
          return nullable2.GetValueOrDefault();
        }));
      }
    }

    public Brush ForegroundBrush
    {
      get
      {
        return Application.Current.Resources["PhoneForegroundBrush"] as Brush;
      }
    }

    public override double FixedHeight
    {
      get
      {
        return 20.0;
      }
    }

    public MessageFooterItem(double width, Thickness margin, MessageViewModel mvm, bool isHorizontalOrientation, double horizontalWidth)
      : base(width, margin, new Thickness())
    {
      this._mvm = mvm;
      this._verticalWidth = width;
      this._horizontalWidth = horizontalWidth;
      this._isHorizontalOrientation = isHorizontalOrientation;
      this.Width = this._isHorizontalOrientation ? this._horizontalWidth : this._verticalWidth;
      this.CreateLayout();
    }

    private new void UpdateLayout()
    {
      this.Width = this.IsHorizontalOrientation ? this._horizontalWidth : this._verticalWidth;
      ((FrameworkElement) this._gridDateStatus).Width = this.Width;
    }

    private void CreateLayout()
    {
      StackPanel stackPanel1 = new StackPanel();
      int num1 = 1;
      stackPanel1.Orientation=((Orientation) num1);
      this._stackPanelCannotSend = stackPanel1;
      UIElementCollection children = ((Panel) this._stackPanelCannotSend).Children;
      TextBlock textBlock1 = new TextBlock();
      string messageWasNotSent = CommonResources.Conversation_MessageWasNotSent;
      textBlock1.Text = messageWasNotSent;
      double num2 = 18.0;
      textBlock1.FontSize = num2;
      FontFamily fontFamily1 = new FontFamily("Segoe WP");
      textBlock1.FontFamily = fontFamily1;
      Brush foregroundBrush1 = this.ForegroundBrush;
      textBlock1.Foreground = foregroundBrush1;
      double textOpacity1 = this._mvm.TextOpacity;
      ((UIElement) textBlock1).Opacity = textOpacity1;
      ((PresentationFrameworkCollection<UIElement>) children).Add((UIElement) textBlock1);
      HyperlinkButton hyperlinkButton1 = new HyperlinkButton();
      ((Control)hyperlinkButton1).FontSize = 18.0;
      string conversationRetry = CommonResources.Conversation_Retry;
      ((ContentControl) hyperlinkButton1).Content = conversationRetry;
      Brush foregroundBrush2 = this.ForegroundBrush;
      ((Control) hyperlinkButton1).Foreground = foregroundBrush2;
      double textOpacity2 = this._mvm.TextOpacity;
      ((UIElement) hyperlinkButton1).Opacity = textOpacity2;
      HyperlinkButton hyperlinkButton2 = hyperlinkButton1;
      // ISSUE: method pointer
      ((ButtonBase) hyperlinkButton2).Click+=(new RoutedEventHandler( this.hb_Click));
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._stackPanelCannotSend).Children).Add((UIElement) hyperlinkButton2);
      Grid grid = new Grid();
      grid.Width = this.Width;
      grid.Height = this.FixedHeight + 8.0;
      this._gridDateStatus = grid;
      ((PresentationFrameworkCollection<ColumnDefinition>) this._gridDateStatus.ColumnDefinitions).Add(new ColumnDefinition());
      ColumnDefinitionCollection columnDefinitions = this._gridDateStatus.ColumnDefinitions;
      ColumnDefinition columnDefinition = new ColumnDefinition();
      GridLength auto = GridLength.Auto;
      columnDefinition.Width = auto;
      ((PresentationFrameworkCollection<ColumnDefinition>) columnDefinitions).Add(columnDefinition);
      StackPanel stackPanel2 = new StackPanel();
      stackPanel2.Orientation=((Orientation) 1);
      this._stackPanelDateStatus = stackPanel2;
      if (!this.IsStickerOrGraffiti || this._mvm.Message.@out == 1)
      {
        ((FrameworkElement) this._stackPanelDateStatus).HorizontalAlignment = ((HorizontalAlignment) 2);
        Grid.SetColumn((FrameworkElement) this._stackPanelDateStatus, 1);
      }
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._gridDateStatus).Children).Add((UIElement) this._stackPanelDateStatus);
      if (this._mvm.Message.@out == 1)
      {
        ProgressBar progressBar = new ProgressBar();
        double num6 = 100.0;
        ((RangeBase) progressBar).Maximum = num6;
        Thickness thickness = new Thickness(-12.0, 3.0, 0.0, 0.0);
        ((FrameworkElement) progressBar).Margin = thickness;
        int num7 = 0;
        progressBar.IsIndeterminate=(num7 != 0);
        int num8 = 1;
        ((UIElement) progressBar).Visibility = ((Visibility) num8);
        Brush foregroundBrush3 = this.ForegroundBrush;
        ((Control) progressBar).Foreground = foregroundBrush3;
        double textOpacity3 = this._mvm.TextOpacity;
        ((UIElement) progressBar).Opacity = textOpacity3;
        int num9 = 3;
        ((FrameworkElement) progressBar).HorizontalAlignment = ((HorizontalAlignment) num9);
        this._progressBar = progressBar;
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this._gridDateStatus).Children).Add((UIElement) this._progressBar);
      }
      TextBlock textBlock2 = new TextBlock();
      double textOpacity4 = this._mvm.TextOpacity;
      ((UIElement) textBlock2).Opacity = textOpacity4;
      string uiDate = this._mvm.UIDate;
      textBlock2.Text = uiDate;
      double num10 = 18.0;
      textBlock2.FontSize = num10;
      Thickness thickness1 = new Thickness(0.0, 2.0, 0.0, 0.0);
      ((FrameworkElement) textBlock2).Margin = thickness1;
      FontFamily fontFamily2 = new FontFamily("Segoe WP");
      textBlock2.FontFamily = fontFamily2;
      Brush foregroundBrush4 = this.ForegroundBrush;
      textBlock2.Foreground = foregroundBrush4;
      this._dateTextBlock = textBlock2;
      Border border = new Border();
      Brush foregroundBrush5 = this.ForegroundBrush;
      border.Background = foregroundBrush5;
      double textOpacity5 = this._mvm.TextOpacity;
      ((UIElement) border).Opacity = textOpacity5;
      double num11 = 21.0;
      ((FrameworkElement) border).Width = num11;
      double num12 = 18.0;
      ((FrameworkElement) border).Height = num12;
      Thickness thickness2 = new Thickness(5.0, 0.0, 0.0, 0.0);
      ((FrameworkElement) border).Margin = thickness2;
      this._borderStatus = border;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._stackPanelDateStatus).Children).Add((UIElement) this._dateTextBlock);
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._stackPanelDateStatus).Children).Add((UIElement) this._borderStatus);
    }

    private void hb_Click(object sender, RoutedEventArgs e)
    {
      PhoneApplicationPage content = ((ContentControl) Application.Current.RootVisual).Content as PhoneApplicationPage;
      if (content != null)
        ((Control) content).Focus();
      this._mvm.Send();
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      this._mvm.PropertyChanged += new PropertyChangedEventHandler(this._mvm_PropertyChanged);
      if (this._mvm.OutboundMessageVM != null)
        this._mvm.OutboundMessageVM.PropertyChanged += new PropertyChangedEventHandler(this.OutboundMessageVM_PropertyChanged);
      this.UpdateState();
    }

    private void OutboundMessageVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "UploadProgress"))
        return;
      this.UpdateProgress();
    }

    private void UpdateState()
    {
      this.UpdateProgress();
      if (this._mvm.Message.@out == 1 && this._mvm.SendStatus == OutboundMessageStatus.Failed)
      {
        if (!((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Contains((UIElement) this._stackPanelCannotSend))
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Add((UIElement) this._stackPanelCannotSend);
        if (((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Contains((UIElement) this._gridDateStatus))
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Remove((UIElement) this._gridDateStatus);
      }
      else
      {
        if (((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Contains((UIElement) this._stackPanelCannotSend))
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Remove((UIElement) this._stackPanelCannotSend);
        if (!((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Contains((UIElement) this._gridDateStatus))
          ((PresentationFrameworkCollection<UIElement>) ((Panel) this._view).Children).Add((UIElement) this._gridDateStatus);
      }
      ((UIElement) this._borderStatus).Visibility = (this._mvm.Message.@out == 1 ? Visibility.Visible : Visibility.Collapsed);
      if (this._mvm.Message.@out != 1)
        return;
      Border borderStatus = this._borderStatus;
      ImageBrush imageBrush = new ImageBrush();
      BitmapImage bitmapImage = new BitmapImage(new Uri(this._mvm.StatusImage, UriKind.Relative));
      int num = 18;
      bitmapImage.CreateOptions = ((BitmapCreateOptions) num);
      imageBrush.ImageSource=((ImageSource) bitmapImage);
      ((UIElement) borderStatus).OpacityMask=((Brush) imageBrush);
    }

    private void UpdateProgress()
    {
      if (this._progressBar == null || this._mvm.OutboundMessageVM == null || this._mvm.OutboundMessageVM.CountUploadableAttachments <= 0)
        return;
      ((UIElement) this._progressBar).Visibility = (this._mvm.SendStatus == OutboundMessageStatus.SendingNow ? Visibility.Visible : Visibility.Collapsed);
      ((DependencyObject) this._progressBar).Animate(((RangeBase) this._progressBar).Value, this._mvm.OutboundMessageVM.UploadProgress, RangeBase.ValueProperty, 100, new int?(),  null,  null);
    }

    private void _mvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "UIStatusDelivered")
      {
        VirtualizableState currentState = this.CurrentState;
        this.Unload();
        this.Load(currentState);
      }
      if (!(e.PropertyName == "UIDate"))
        return;
      this._dateTextBlock.Text = this._mvm.UIDate;
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      this._mvm.PropertyChanged -= new PropertyChangedEventHandler(this._mvm_PropertyChanged);
      if (this._mvm.OutboundMessageVM != null)
        this._mvm.OutboundMessageVM.PropertyChanged -= new PropertyChangedEventHandler(this.OutboundMessageVM_PropertyChanged);
      ((DependencyObject) this._borderStatus).ClearValue((DependencyProperty) UIElement.OpacityMaskProperty);
    }
  }
}
