using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Utils;

namespace VKClient.Common.Framework
{
    public class VirtualizableImage : VirtualizableItemBase
    {
        private static int _TotalCount;
        private string _uriStr;
        private Uri _uri;
        private Stream _stream;
        private double _height;
        private Action<VirtualizableImage> _callbackOnTap;
        private string _tag;
        private bool _showPlaceholder;
        private Stretch _stretch;
        private Brush _placeholderBackgroundBrush;
        private double _placeholderOpacity;
        private bool _blackRectOnTop;
        private readonly bool _isRounded;

        public string Tag
        {
            get
            {
                return this._tag;
            }
        }

        public Image ImageControl
        {
            get
            {
                return this.Children.FirstOrDefault<FrameworkElement>((Func<FrameworkElement, bool>)(c => c is Image)) as Image;
            }
        }

        public string Tag2 { get; set; }

        public object OverlayControl { get; set; }

        public override double FixedHeight
        {
            get
            {
                return this._height;
            }
        }

        // UPDATE: 4.8.0
        public VirtualizableImage(double width, double height, Thickness margin, string uriStr, Action<VirtualizableImage> callbackOnTap, string tag, bool enableTilt = false, bool showPlaceholder = true, Stretch stretch = Stretch.UniformToFill, Brush placeholderBackgroundBrush = null, double placeholderOpacity = -1.0, bool blackRectOnTop = false, bool isRounded = false)
            : this(width, height, margin, callbackOnTap, tag, enableTilt, showPlaceholder, stretch, placeholderBackgroundBrush, placeholderOpacity, blackRectOnTop, isRounded)
        {
            this._uriStr = uriStr ?? "";
            this._uri = uriStr.ConvertToUri();
        }

        public VirtualizableImage(double width, double height, Thickness margin, Stream stream, Action<VirtualizableImage> callbackOnTap, string tag, bool enableTilt = false, bool showPlaceholder = true, Stretch stretch = Stretch.UniformToFill, Brush placeholderBackgroundBrush = null, double placeholderOpacity = -1.0, bool blackRectOnTop = false, bool isRounded = false)
            : this(width, height, margin, callbackOnTap, tag, enableTilt, showPlaceholder, stretch, placeholderBackgroundBrush, placeholderOpacity, blackRectOnTop, isRounded)
        {
            this._stream = stream;
        }

        private VirtualizableImage(double width, double height, Thickness margin, Action<VirtualizableImage> callbackOnTap, string tag, bool enableTilt = false, bool showPlaceholder = true, Stretch stretch = Stretch.UniformToFill, Brush placeholderBackgroundBrush = null, double placeholderOpacity = -1.0, bool blackRectOnTop = false, bool isRounded = false)
            : base(width, margin, new Thickness())
        {
            ++VirtualizableImage._TotalCount;
            this._height = height;
            this._callbackOnTap = callbackOnTap;
            this._tag = tag;
            this._showPlaceholder = showPlaceholder;
            this._stretch = stretch;
            this._placeholderBackgroundBrush = placeholderBackgroundBrush;
            this._placeholderOpacity = placeholderOpacity;
            this._blackRectOnTop = blackRectOnTop;
            this._isRounded = isRounded;
            if (!enableTilt)
                return;
            MetroInMotion.SetTilt((DependencyObject)this._view, VKConstants.DefaultTilt);
        }

        ~VirtualizableImage()
        {
            --VirtualizableImage._TotalCount;
        }

        protected override void GenerateChildren()
        {
            Geometry geometry = (Geometry)null;
            if (this._showPlaceholder)
            {
                if (this._isRounded)
                {
                    Ellipse ellipse1 = new Ellipse();
                    double width = this.Width;
                    ellipse1.Width = width;
                    double num = this._height;
                    ellipse1.Height = num;
                    Style style = Application.Current.Resources["PhotoPlaceholderEllipse"] as Style;
                    ellipse1.Style = style;
                    Ellipse ellipse2 = ellipse1;
                    if (this._placeholderBackgroundBrush != null)
                        ellipse2.Fill = this._placeholderBackgroundBrush;
                    if (this._placeholderOpacity >= 0.0)
                        ellipse2.Opacity = this._placeholderOpacity;
                    this.Children.Add((FrameworkElement)ellipse2);
                    geometry = (Geometry)new EllipseGeometry()
                    {
                        Center = new Point(this.Width / 2.0, this._height / 2.0),
                        RadiusX = (this.Width / 2.0),
                        RadiusY = (this._height / 2.0)
                    };
                }
                else
                {
                    Rectangle rectangle1 = new Rectangle();
                    double width = this.Width;
                    rectangle1.Width = width;
                    double num = this._height;
                    rectangle1.Height = num;
                    Style style = Application.Current.Resources["PhotoPlaceholderRectangle"] as Style;
                    rectangle1.Style = style;
                    Rectangle rectangle2 = rectangle1;
                    if (this._placeholderBackgroundBrush != null)
                        rectangle2.Fill = this._placeholderBackgroundBrush;
                    if (this._placeholderOpacity >= 0.0)
                        rectangle2.Opacity = this._placeholderOpacity;
                    this.Children.Add((FrameworkElement)rectangle2);
                }
            }
            Image image1 = new Image();
            double width1 = this.Width;
            image1.Width = width1;
            double num1 = this._height;
            image1.Height = num1;
            int num2 = (int)this._stretch;
            image1.Stretch = (Stretch)num2;
            Image image2 = image1;
            if (geometry != null)
                image2.Clip = geometry;
            this._view.Tap += new EventHandler<GestureEventArgs>(this.image_Tap);
            image2.Tap += new EventHandler<GestureEventArgs>(this.image_Tap);
            this.Children.Add((FrameworkElement)image2);
            if (!this._blackRectOnTop)
                return;
            Rectangle rectangle = new Rectangle();
            double width2 = this.Width;
            rectangle.Width = width2;
            double num3 = this._height;
            rectangle.Height = num3;
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Black);
            rectangle.Fill = (Brush)solidColorBrush;
            double num4 = 0.2;
            rectangle.Opacity = num4;
            int num5 = 0;
            rectangle.IsHitTestVisible = num5 != 0;
            this.Children.Add((FrameworkElement)rectangle);
        }

        private void image_Tap(object sender, GestureEventArgs e)
        {
            if (this._callbackOnTap == null)
                return;
            this._callbackOnTap(this);
            e.Handled = true;
        }

        // UPDATE: 4.8.0
        protected override void LoadFullyNonVirtualizableItems()
        {
            Image image = this.Children.FirstOrDefault<FrameworkElement>((Func<FrameworkElement, bool>)(c => c is Image)) as Image;
            if (this._uri != null)
            {
                VeryLowProfileImageLoader.SetUriSource(image, this._uri);
            }
            else
            {
                if (this._stream == null)
                    return;
                ImageLoader.SetStreamSource(image, this._stream);
            }
        }

        protected override void ReleaseResourcesOnUnload()
        {
            VeryLowProfileImageLoader.SetUriSource(this.Children.FirstOrDefault<FrameworkElement>((Func<FrameworkElement, bool>)(c => c is Image)) as Image, null);
        }

        protected override void ShownOnScreen()
        {
            if (!(this._uri != null) || !this._uri.IsAbsoluteUri)
                return;
            VeryLowProfileImageLoader.SetPriority(this._uri.OriginalString, DateTime.Now.Ticks);
        }
    }
}
