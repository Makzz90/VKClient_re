using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Utils;
//
using VKClient.Common.Library;

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
                return Enumerable.FirstOrDefault<FrameworkElement>(this.Children, (Func<FrameworkElement, bool>)(c => c is Image)) as Image;
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
            Geometry geometry = null;
            if (this._showPlaceholder)
            {
                if (this._isRounded)
                {
                    Ellipse ellipse1 = new Ellipse();
                    double width = this.Width;
                    ellipse1.Width = width;
                    ellipse1.Height = this._height;
                    ellipse1.Style = Application.Current.Resources["PhotoPlaceholderEllipse"] as Style;
                    if (this._placeholderBackgroundBrush != null)
                        ellipse1.Fill = this._placeholderBackgroundBrush;
                    if (this._placeholderOpacity >= 0.0)
                        ellipse1.Opacity = this._placeholderOpacity;
                    this.Children.Add(ellipse1);
                    double px_per_tick_x = this.Width / 10.0 / 2.0;
                    double px_per_tick_y = this._height / 10.0 / 2.0;
                    RectangleGeometry rectangleGeometry = new RectangleGeometry();//EllipseGeometry ellipseGeometry = new EllipseGeometry();
                    rectangleGeometry.Rect = new Rect(0, 0, this.Width, this._height);//Point point = new Point(this.Width / 2.0, this._height / 2.0);
                    //ellipseGeometry.Center = point;
                    rectangleGeometry.RadiusX = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * px_per_tick_x;
                    rectangleGeometry.RadiusY = AppGlobalStateManager.Current.GlobalState.UserAvatarRadius * px_per_tick_y;
                    //ellipseGeometry.RadiusY = this._height / 2.0;
                    geometry = rectangleGeometry;//geometry = ellipseGeometry;
                }
                else
                {
                    Rectangle rectangle1 = new Rectangle();
                    double width = this.Width;
                    ((FrameworkElement)rectangle1).Width = width;
                    double height = this._height;
                    ((FrameworkElement)rectangle1).Height = height;
                    Style style = Application.Current.Resources["PhotoPlaceholderRectangle"] as Style;
                    ((FrameworkElement)rectangle1).Style = style;
                    Rectangle rectangle2 = rectangle1;
                    if (this._placeholderBackgroundBrush != null)
                        ((Shape)rectangle2).Fill = this._placeholderBackgroundBrush;
                    if (this._placeholderOpacity >= 0.0)
                        ((UIElement)rectangle2).Opacity = this._placeholderOpacity;
                    this.Children.Add((FrameworkElement)rectangle2);
                }
            }
            Image image1 = new Image();
            double width1 = this.Width;
            ((FrameworkElement)image1).Width = width1;
            double height1 = this._height;
            ((FrameworkElement)image1).Height = height1;
            Stretch stretch = this._stretch;
            image1.Stretch = stretch;
            if (geometry != null)
                image1.Clip = geometry;
            ((UIElement)this._view).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.image_Tap));
            image1.Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.image_Tap));
            this.Children.Add(image1);
            if (!this._blackRectOnTop)
                return;
            Rectangle rectangle = new Rectangle();
            double width2 = this.Width;
            ((FrameworkElement)rectangle).Width = width2;
            double height2 = this._height;
            ((FrameworkElement)rectangle).Height = height2;
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Black);
            ((Shape)rectangle).Fill = ((Brush)solidColorBrush);
            double num3 = 0.2;
            ((UIElement)rectangle).Opacity = num3;
            int num4 = 0;
            ((UIElement)rectangle).IsHitTestVisible = (num4 != 0);
            this.Children.Add((FrameworkElement)rectangle);
        }

        private void image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this._callbackOnTap == null)
                return;
            this._callbackOnTap(this);
            e.Handled = true;
        }

        protected override void LoadFullyNonVirtualizableItems()
        {
            Image image = Enumerable.FirstOrDefault<FrameworkElement>(this.Children, (Func<FrameworkElement, bool>)(c => c is Image)) as Image;
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
            VeryLowProfileImageLoader.SetUriSource(Enumerable.FirstOrDefault<FrameworkElement>(this.Children, (Func<FrameworkElement, bool>)(c => c is Image)) as Image, null);
        }

        protected override void ShownOnScreen()
        {
            if (!(this._uri != null) || !this._uri.IsAbsoluteUri)
                return;
            VeryLowProfileImageLoader.SetPriority(this._uri.OriginalString, DateTime.Now.Ticks);
        }
    }
}
