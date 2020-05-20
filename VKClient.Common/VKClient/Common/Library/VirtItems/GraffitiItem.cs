using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.VirtItems
{
  public class GraffitiItem : VirtualizableItemBase
  {
    private const double MAX_WIDTH = 320.0;
    private const double GRAFFITI_MAX_SIDE_LENGTH = 720.0;
    private readonly double _portraitWidth;
    private readonly List<Attachment> _attachments;
    private readonly double _landscapeWidth;
    private readonly bool _rightAlign;
    private readonly bool _isBlackTheme;
    private readonly double _extraMargin;
    private bool _isHorizontal;
    private double _width;
    private double _height;

    private Thickness GraffitiMargin
    {
      get
      {
        if (!this._rightAlign)
          return new Thickness(this._extraMargin, this._extraMargin, 0.0, 0.0);
        return new Thickness(this.Width - this._width + this._extraMargin, this._extraMargin, 0.0, 0.0);
      }
    }

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
        this.Width = this._isHorizontal ? this._landscapeWidth : this._portraitWidth;
        ((VirtualizableItemBase) this.VirtualizableChildren.First<IVirtualizable>()).Margin = this.GraffitiMargin;
      }
    }

    public override double FixedHeight
    {
      get
      {
        return this._height;
      }
    }

    public GraffitiItem(double width, Thickness margin, List<Attachment> attachments, bool isHorizontal, double landscapeWidth, bool rightAlign)
      : base(width, margin, new Thickness())
    {
      this._isHorizontal = isHorizontal;
      this._portraitWidth = width;
      this._attachments = attachments;
      this._landscapeWidth = landscapeWidth;
      this._rightAlign = rightAlign;
      if (this._isHorizontal)
        this.Width = this._landscapeWidth;
      this._isBlackTheme = new ThemeHelper().PhoneDarkThemeVisibility == Visibility.Visible;
      if (this._isBlackTheme)
        this._extraMargin = 8.0;
      this.CreateOrUpdateLayout();
    }

    public void CreateOrUpdateLayout()
    {
      List<Attachment> source = this._attachments;
      DocPreviewGraffiti docPreviewGraffiti1;
      if (source == null)
      {
        docPreviewGraffiti1 = (DocPreviewGraffiti) null;
      }
      else
      {
        Attachment attachment = source.FirstOrDefault<Attachment>((Func<Attachment, bool>) (a => a.doc != null));
        if (attachment == null)
        {
          docPreviewGraffiti1 = (DocPreviewGraffiti) null;
        }
        else
        {
          Doc doc = attachment.doc;
          if (doc == null)
          {
            docPreviewGraffiti1 = (DocPreviewGraffiti) null;
          }
          else
          {
            DocPreview preview = doc.preview;
            docPreviewGraffiti1 = preview != null ? preview.graffiti : (DocPreviewGraffiti) null;
          }
        }
      }
      DocPreviewGraffiti docPreviewGraffiti2 = docPreviewGraffiti1;
      if (docPreviewGraffiti2 == null)
        return;
      double num1 = 4.0 / 9.0;
      double val1 = (double) docPreviewGraffiti2.width * num1;
      double num2 = (double) docPreviewGraffiti2.height * num1;
      double num3 = val1 * 1.0 / num2;
      double num4 = Math.Min(val1, 320.0);
      double num5 = num4 / num3;
      this._width = num4;
      this._height = num5;
      double width = this._width;
      double height = this._height;
      if (this._isBlackTheme)
      {
        this._width = this._width + this._extraMargin * 2.0;
        this._height = this._height + this._extraMargin * 2.0;
      }
      string src = docPreviewGraffiti2.src;
      Stream stream = (Stream) null;
      Uri uri = src.ConvertToUri();
      if (!uri.IsAbsoluteUri)
      {
        Stream cachedImageStream = ImageCache.Current.GetCachedImageStream(src);
        if (cachedImageStream != null)
        {
          stream = (Stream) new MemoryStream();
          cachedImageStream.CopyTo(stream);
          cachedImageStream.Close();
        }
      }
      this.VirtualizableChildren.Add(!uri.IsAbsoluteUri ? (IVirtualizable) new VirtualizableImage(width, height, this.GraffitiMargin, stream, (Action<VirtualizableImage>) null, "1", false, false, Stretch.Uniform, (Brush) null, -1.0, false, false) : (IVirtualizable) new VirtualizableImage(width, height, this.GraffitiMargin, src, (Action<VirtualizableImage>) null, "1", false, false, Stretch.Uniform, (Brush) null, -1.0, false, false));
    }

    protected override void GenerateChildren()
    {
      if (!this._isBlackTheme)
        return;
      Rectangle rectangle1 = new Rectangle();
      double num1 = this._width;
      rectangle1.Width = num1;
      double num2 = this._height;
      rectangle1.Height = num2;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.White);
      rectangle1.Fill = (Brush) solidColorBrush;
      double num3 = 16.0;
      rectangle1.RadiusX = num3;
      double num4 = 16.0;
      rectangle1.RadiusY = num4;
      Rectangle rectangle2 = rectangle1;
      if (this._rightAlign)
        rectangle2.Margin = new Thickness(this.Width - this._width, 0.0, 0.0, 0.0);
      this.Children.Add((FrameworkElement) rectangle2);
    }
  }
}
