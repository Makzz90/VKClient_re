using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VKClient.Common.Framework
{
    public class ItemFlyInAndOutAnimations
    {
        private static TimeSpan _flyInSpeed = TimeSpan.FromMilliseconds(200.0);
        private static TimeSpan _flyOutSpeed = TimeSpan.FromMilliseconds(300.0);
        private Popup _popup;
        private Canvas _popupCanvas;
        private FrameworkElement _targetElement;
        private Point _targetElementPosition;
        private Image _targetElementClone;
        private Rectangle _backgroundMask;

        public ItemFlyInAndOutAnimations()
        {
            this._popup = new Popup();
            this._popupCanvas = new Canvas();
            this._popup.Child = ((UIElement)this._popupCanvas);
        }

        public static void TitleFlyIn(FrameworkElement title)
        {
            TranslateTransform translateTransform = new TranslateTransform();
            translateTransform.X = 300.0;
            translateTransform.Y = (-50.0);
            ((UIElement)title).RenderTransform = ((Transform)translateTransform);
            Storyboard storyboard = new Storyboard();
            DoubleAnimation doubleAnimation1 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(300.0, 0.0, (IEasingFunction)new SineEase(), (DependencyObject)translateTransform, TranslateTransform.XProperty, ItemFlyInAndOutAnimations._flyInSpeed);
            ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation1);
            DoubleAnimation doubleAnimation2 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(-100.0, 0.0, (IEasingFunction)new SineEase(), (DependencyObject)translateTransform, TranslateTransform.YProperty, ItemFlyInAndOutAnimations._flyInSpeed);
            ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation2);
            storyboard.Begin();
        }

        public void ItemFlyIn()
        {
            if (((PresentationFrameworkCollection<UIElement>)((Panel)this._popupCanvas).Children).Count != 2)
                return;
            this._popup.IsOpen = true;
            ((UIElement)this._backgroundMask).Opacity = 0.0;
            //      ((PresentationFrameworkCollection<UIElement>) ((Panel) this._popupCanvas).Children)[1];
            Storyboard storyboard = new Storyboard();
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            DoubleAnimation doubleAnimation1 = ItemFlyInAndOutAnimations.CreateDoubleAnimation((this._targetElementPosition).X - 100.0, (this._targetElementPosition).X, (IEasingFunction)new SineEase(), (DependencyObject)this._targetElementClone, Canvas.LeftProperty, ItemFlyInAndOutAnimations._flyInSpeed);
            ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation1);
            // ISSUE: explicit reference operation
            // ISSUE: explicit reference operation
            DoubleAnimation doubleAnimation2 = ItemFlyInAndOutAnimations.CreateDoubleAnimation((this._targetElementPosition).Y - 50.0, (this._targetElementPosition).Y, (IEasingFunction)new SineEase(), (DependencyObject)this._targetElementClone, Canvas.TopProperty, ItemFlyInAndOutAnimations._flyInSpeed);
            ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation2);
            EventHandler eventHandler = (EventHandler)((s, e) =>
            {
                this._popup.IsOpen = false;
                ((UIElement)this._targetElement).Opacity = 1.0;
                ((PresentationFrameworkCollection<UIElement>)((Panel)this._popupCanvas).Children).Clear();
            });
            ((Timeline)storyboard).Completed += (eventHandler);
            storyboard.Begin();
        }

        public void ItemFlyOut(FrameworkElement element, Action action)
        {
            this._targetElement = element;
            FrameworkElement rootVisual = Application.Current.RootVisual as FrameworkElement;
            Rectangle rectangle = new Rectangle();
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Black);
            ((Shape)rectangle).Fill = ((Brush)solidColorBrush);
            double num1 = 0.0;
            ((UIElement)rectangle).Opacity = num1;
            double actualWidth = rootVisual.ActualWidth;
            ((FrameworkElement)rectangle).Width = actualWidth;
            double actualHeight = rootVisual.ActualHeight;
            ((FrameworkElement)rectangle).Height = actualHeight;
            this._backgroundMask = rectangle;
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._popupCanvas).Children).Add((UIElement)this._backgroundMask);
            Image image = new Image();
            WriteableBitmap writeableBitmap = new WriteableBitmap((UIElement)element, null);
            image.Source = ((ImageSource)writeableBitmap);
            this._targetElementClone = image;
            ((PresentationFrameworkCollection<UIElement>)((Panel)this._popupCanvas).Children).Add((UIElement)this._targetElementClone);
            this._targetElementPosition = ((UIElement)element).GetRelativePosition((UIElement)rootVisual);
            // ISSUE: explicit reference operation
            Canvas.SetTop((UIElement)this._targetElementClone, (this._targetElementPosition).Y);
            // ISSUE: explicit reference operation
            Canvas.SetLeft((UIElement)this._targetElementClone, (this._targetElementPosition).X);
            Storyboard storyboard = new Storyboard();
            // ISSUE: explicit reference operation
            double x = (this._targetElementPosition).X;
            // ISSUE: explicit reference operation
            double to1 = (this._targetElementPosition).X + 500.0;
            SineEase sineEase1 = new SineEase();
            int num2 = 1;
            ((EasingFunctionBase)sineEase1).EasingMode = ((EasingMode)num2);
            Image targetElementClone1 = this._targetElementClone;
            // ISSUE: variable of the null type
            TimeSpan flyOutSpeed1 = ItemFlyInAndOutAnimations._flyOutSpeed;
            DoubleAnimation doubleAnimation1 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(x, to1, (IEasingFunction)sineEase1, (DependencyObject)targetElementClone1, Canvas.LeftProperty, flyOutSpeed1);
            ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation1);
            // ISSUE: explicit reference operation
            double y = (this._targetElementPosition).Y;
            // ISSUE: explicit reference operation
            double to2 = (this._targetElementPosition).Y + 50.0;
            SineEase sineEase2 = new SineEase();
            int num3 = 0;
            ((EasingFunctionBase)sineEase2).EasingMode = ((EasingMode)num3);
            Image targetElementClone2 = this._targetElementClone;
            // ISSUE: variable of the null type
            TimeSpan flyOutSpeed2 = ItemFlyInAndOutAnimations._flyOutSpeed;
            DoubleAnimation doubleAnimation2 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(y, to2, (IEasingFunction)sineEase2, (DependencyObject)targetElementClone2, Canvas.TopProperty, flyOutSpeed2);
            ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation2);
            DoubleAnimation doubleAnimation3 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(0.0, 1.0, null, (DependencyObject)this._backgroundMask, UIElement.OpacityProperty, ItemFlyInAndOutAnimations._flyOutSpeed);
            ((PresentationFrameworkCollection<Timeline>)storyboard.Children).Add((Timeline)doubleAnimation3);
            EventHandler eventHandler = (EventHandler)((s, e2) =>
            {
                action();
                ((DependencyObject)element).Dispatcher.BeginInvoke((Action)(() => this._popup.IsOpen = false));
            });
            ((Timeline)storyboard).Completed += (eventHandler);
            ((UIElement)element).Opacity = 0.0;
            this._popup.IsOpen = true;
            storyboard.Begin();
        }

        public static DoubleAnimation CreateDoubleAnimation(double from, double to, IEasingFunction easing, DependencyObject target, object propertyPath, TimeSpan duration)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            double? nullable1 = new double?(to);
            doubleAnimation.To = nullable1;
            double? nullable2 = new double?(from);
            doubleAnimation.From = nullable2;
            IEasingFunction ieasingFunction = easing;
            doubleAnimation.EasingFunction = ieasingFunction;
            Duration duration1 = (duration);
            ((Timeline)doubleAnimation).Duration = duration1;
            DependencyObject dependencyObject = target;
            Storyboard.SetTarget((Timeline)doubleAnimation, dependencyObject);
            PropertyPath propertyPath1 = new PropertyPath(propertyPath);
            Storyboard.SetTargetProperty((Timeline)doubleAnimation, propertyPath1);
            return doubleAnimation;
        }
    }
}
