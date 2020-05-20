using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.VirtItems
{
  public class GenericAttachmentItem : VirtualizableItemBase, IHandle<DocUploaded>, IHandle
  {
    private string _title = "";
    private string _subtitle = "";
    private string _iconSrc = "";
    private string _imageSrc = "";
    private string _navigateUri = "";
    private const double MARGIN_LEFT_RIGHT = 16.0;
    private readonly Attachment _attachment;
    private readonly double _horizontalWidth;
    private readonly double _verticalWidth;
    private bool _isHorizontal;
    private readonly string _hyperlinkId;

    public bool IsHorizontal
    {
      get
      {
        return this._isHorizontal;
      }
      set
      {
        if (this._isHorizontal == value)
          return;
        this._isHorizontal = value;
        this.Width = this._isHorizontal ? this._horizontalWidth : this._verticalWidth;
        this.UpdateLayout();
        this.UpdateClip();
      }
    }

    public override double FixedHeight
    {
      get
      {
        return 72.0;
      }
    }

    public GenericAttachmentItem(double width, Thickness margin, Attachment attachment, bool isHorizontal = false, double horizontalWidth = 0.0, string hyperlinkId = "")
        : base(!isHorizontal ? width : horizontalWidth, margin, new Thickness())
    {
      EventAggregator.Current.Subscribe(this);
      this._hyperlinkId = hyperlinkId;
      this._attachment = attachment;
      this._isHorizontal = isHorizontal;
      this._verticalWidth = width;
      this._horizontalWidth = horizontalWidth;
      this.ParseTitleSubtitle();
      this.UpdateClip();
      ((Panel) this._view).Background = ((Brush) new SolidColorBrush(Colors.Transparent));
    }

    private void UpdateClip()
    {
      Canvas view = this._view;
      RectangleGeometry rectangleGeometry = new RectangleGeometry();
      Rect rect = new Rect(-5.0, 0.0, this.Width + 5.0, this.FixedHeight + 10.0);
      rectangleGeometry.Rect = rect;
      ((UIElement) view).Clip=((Geometry) rectangleGeometry);
    }

    private static string ParseDomain(string uriStr)
    {
      try
      {
        return new Uri(uriStr).Host;
      }
      catch
      {
        return "";
      }
    }

    private void ParseTitleSubtitle()
    {
      if (this._attachment.type == "link" && this._attachment.link != null)
      {
        this._title = !string.IsNullOrWhiteSpace(this._attachment.link.title) ? this._attachment.link.title : CommonResources.Link;
        this._subtitle = GenericAttachmentItem.ParseDomain(this._attachment.link.url);
        this._navigateUri = this._attachment.link.url;
        this._iconSrc = "/Resources/WallPost/AttachLink.png";
      }
      if (this._attachment.type == "note" && this._attachment.note != null)
      {
        this._title = this._attachment.note.title ?? "";
        this._subtitle = CommonResources.Note;
        this._navigateUri = string.Format("vk.com/note{0}_{1}", this._attachment.note.owner_id, this._attachment.note.nid);
        this._iconSrc = "/Resources/WallPost/AttachNote.png";
      }
      if (this._attachment.type == "doc" && this._attachment.doc != null)
      {
        DocumentHeader documentHeader = new DocumentHeader(this._attachment.doc, 0, false, 0L);
        this._title = documentHeader.Name;
        this._subtitle = documentHeader.Description;
        this._navigateUri = documentHeader.Document.url;
        this._iconSrc = "/Resources/WallPost/AttachDoc.png";
      }
      if (this._attachment.type == "page" && this._attachment.Page != null)
      {
        this._title = this._attachment.Page.title ?? "";
        this._subtitle = CommonResources.WikiPage;
        this._navigateUri = string.Format("https://vk.com/club{0}?w=page-{0}_{1}", this._attachment.Page.gid, this._attachment.Page.pid);
        this._iconSrc = "/Resources/WallPost/AttachLink.png";
      }
      Product market = this._attachment.market;
      if (this._attachment.type == "market" && market != null)
      {
        this._title = market.title;
        this._subtitle = market.price.text;
        this._navigateUri = string.Format("http://m.vk.com/market{0}?w=product{1}_{2}", market.owner_id, market.owner_id, market.id);
        this._imageSrc = market.thumb_photo;
      }
      ((UIElement) this._view).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.View_OnTap));
      this._title = this._title.Replace(Environment.NewLine, " ");
      this._subtitle = this._subtitle.Replace(Environment.NewLine, " ");
      MetroInMotion.SetTilt((DependencyObject) this._view, 1.5);
    }

    private void View_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      if (!(this._navigateUri != ""))
        return;
      if (!string.IsNullOrEmpty(this._hyperlinkId))
        EventAggregator.Current.Publish(new HyperlinkClickedEvent()
        {
          HyperlinkOwnerId = this._hyperlinkId
        });
      Navigator.Current.NavigateToWebUri(this._navigateUri, false, false);
    }

    protected override void GenerateChildren()
    {
      if (string.IsNullOrEmpty(this._imageSrc))
      {
        Ellipse ellipse1 = new Ellipse();
        double num1 = 56.0;
        ((FrameworkElement) ellipse1).Width = num1;
        double num2 = 56.0;
        ((FrameworkElement) ellipse1).Height = num2;
        SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneAttachIconBackgroundBrush"];
        ((Shape) ellipse1).Fill = ((Brush) solidColorBrush1);
        Ellipse ellipse2 = ellipse1;
        Canvas.SetTop((UIElement) ellipse2, 8.0);
        Canvas.SetLeft((UIElement) ellipse2, 16.0);
        this.Children.Add((FrameworkElement) ellipse2);
        Border border1 = new Border();
        double num3 = 32.0;
        ((FrameworkElement) border1).Width = num3;
        double num4 = 32.0;
        ((FrameworkElement) border1).Height = num4;
        SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneGenericAttachmentIconBrush"];
        border1.Background = ((Brush) solidColorBrush2);
        Border border2 = border1;
        ImageBrush imageBrush = new ImageBrush();
        ImageLoader.SetImageBrushMultiResSource(imageBrush, this._iconSrc);
        ((UIElement) border2).OpacityMask=((Brush) imageBrush);
        Canvas.SetTop((UIElement) border2, 20.0);
        Canvas.SetLeft((UIElement) border2, 28.0);
        this.Children.Add((FrameworkElement) border2);
      }
      else
      {
        Rectangle rectangle1 = new Rectangle();
        double num1 = 56.0;
        ((FrameworkElement) rectangle1).Width = num1;
        double num2 = 56.0;
        ((FrameworkElement) rectangle1).Height = num2;
        SolidColorBrush solidColorBrush = (SolidColorBrush) Application.Current.Resources["PhoneAttachIconBackgroundBrush"];
        ((Shape) rectangle1).Fill = ((Brush) solidColorBrush);
        Rectangle rectangle2 = rectangle1;
        Canvas.SetTop((UIElement) rectangle2, 8.0);
        Canvas.SetLeft((UIElement) rectangle2, 16.0);
        this.Children.Add((FrameworkElement) rectangle2);
        Image image1 = new Image();
        double num3 = 56.0;
        ((FrameworkElement) image1).Width = num3;
        double num4 = 56.0;
        ((FrameworkElement) image1).Height = num4;
        Image image2 = image1;
        ImageLoader.SetUriSource(image2, this._imageSrc);
        Canvas.SetTop((UIElement) image2, 8.0);
        Canvas.SetLeft((UIElement) image2, 16.0);
        this.Children.Add((FrameworkElement) image2);
      }
      TextBlock textBlock1 = new TextBlock();
      SolidColorBrush solidColorBrush3 = (SolidColorBrush) Application.Current.Resources["PhoneAlmostBlackBrush"];
      textBlock1.Foreground = ((Brush) solidColorBrush3);
      double num = 22.7;
      textBlock1.FontSize = num;
      string title = this._title;
      textBlock1.Text = title;
      TextBlock textName1 = textBlock1;
      Canvas.SetTop((UIElement) textName1, 8.0);
      Canvas.SetLeft((UIElement) textName1, 84.0);
      double maxWidth1 = this.Width - (Canvas.GetLeft((UIElement) textName1) + 16.0);
      textName1.CorrectText(maxWidth1);
      this.Children.Add((FrameworkElement) textName1);
      TextBlock textBlock2 = new TextBlock();
      SolidColorBrush solidColorBrush4 = (SolidColorBrush) Application.Current.Resources["PhoneCaptionGrayBrush"];
      textBlock2.Foreground = ((Brush) solidColorBrush4);
      string subtitle = this._subtitle;
      textBlock2.Text = subtitle;
      TextBlock textName2 = textBlock2;
      Canvas.SetTop((UIElement) textName2, 34.0);
      Canvas.SetLeft((UIElement) textName2, 84.0);
      double maxWidth2 = this.Width - (Canvas.GetLeft((UIElement) textName2) + 16.0);
      textName2.CorrectText(maxWidth2);
      this.Children.Add((FrameworkElement) textName2);
    }

    public void Handle(DocUploaded message)
    {
      if (this._attachment == null || !(this._attachment.type == "doc") || (this._attachment.doc == null || !(this._attachment.doc.guid != Guid.Empty)) || !(message.Doc.guid == this._attachment.doc.guid))
        return;
      this._navigateUri = message.Doc.url;
    }
  }
}
