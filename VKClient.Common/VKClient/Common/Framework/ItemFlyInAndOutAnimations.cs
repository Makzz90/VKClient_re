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
      this._popup.Child = (UIElement) this._popupCanvas;
    }

    public static void TitleFlyIn(FrameworkElement title)
    {
      TranslateTransform translateTransform = new TranslateTransform();
      translateTransform.X = 300.0;
      translateTransform.Y = -50.0;
      title.RenderTransform = (Transform) translateTransform;
      Storyboard storyboard = new Storyboard();
      DoubleAnimation doubleAnimation1 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(300.0, 0.0, (IEasingFunction) new SineEase(), (DependencyObject) translateTransform, (object) TranslateTransform.XProperty, ItemFlyInAndOutAnimations._flyInSpeed);
      storyboard.Children.Add((Timeline) doubleAnimation1);
      DoubleAnimation doubleAnimation2 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(-100.0, 0.0, (IEasingFunction) new SineEase(), (DependencyObject) translateTransform, (object) TranslateTransform.YProperty, ItemFlyInAndOutAnimations._flyInSpeed);
      storyboard.Children.Add((Timeline) doubleAnimation2);
      storyboard.Begin();
    }

    public void ItemFlyIn()
    {
      if (this._popupCanvas.Children.Count != 2)
        return;
      this._popup.IsOpen = true;
      this._backgroundMask.Opacity = 0.0;
      UIElement uiElement = this._popupCanvas.Children[1];
      Storyboard storyboard = new Storyboard();
      DoubleAnimation doubleAnimation1 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(this._targetElementPosition.X - 100.0, this._targetElementPosition.X, (IEasingFunction) new SineEase(), (DependencyObject) this._targetElementClone, (object) Canvas.LeftProperty, ItemFlyInAndOutAnimations._flyInSpeed);
      storyboard.Children.Add((Timeline) doubleAnimation1);
      DoubleAnimation doubleAnimation2 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(this._targetElementPosition.Y - 50.0, this._targetElementPosition.Y, (IEasingFunction) new SineEase(), (DependencyObject) this._targetElementClone, (object) Canvas.TopProperty, ItemFlyInAndOutAnimations._flyInSpeed);
      storyboard.Children.Add((Timeline) doubleAnimation2);
      EventHandler eventHandler = (EventHandler) ((s, e) =>
      {
        this._popup.IsOpen = false;
        this._targetElement.Opacity = 1.0;
        this._popupCanvas.Children.Clear();
      });
      storyboard.Completed += eventHandler;
      storyboard.Begin();
    }

    public void ItemFlyOut(FrameworkElement element, Action action)
    {
      this._targetElement = element;
      FrameworkElement frameworkElement = Application.Current.RootVisual as FrameworkElement;
      Rectangle rectangle = new Rectangle();
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Black);
      rectangle.Fill = (Brush) solidColorBrush;
      double num1 = 0.0;
      rectangle.Opacity = num1;
      double actualWidth = frameworkElement.ActualWidth;
      rectangle.Width = actualWidth;
      double actualHeight = frameworkElement.ActualHeight;
      rectangle.Height = actualHeight;
      this._backgroundMask = rectangle;
      this._popupCanvas.Children.Add((UIElement) this._backgroundMask);
      this._targetElementClone = new Image()
      {
        Source = (ImageSource) new WriteableBitmap((UIElement) element, (Transform) null)
      };
      this._popupCanvas.Children.Add((UIElement) this._targetElementClone);
      this._targetElementPosition = element.GetRelativePosition((UIElement) frameworkElement);
      Canvas.SetTop((UIElement) this._targetElementClone, this._targetElementPosition.Y);
      Canvas.SetLeft((UIElement) this._targetElementClone, this._targetElementPosition.X);
      Storyboard storyboard = new Storyboard();
      double x = this._targetElementPosition.X;
      double to1 = this._targetElementPosition.X + 500.0;
      SineEase sineEase1 = new SineEase();
      int num2 = 1;
      sineEase1.EasingMode = (EasingMode) num2;
      Image image1 = this._targetElementClone;
      DependencyProperty dependencyProperty1 = Canvas.LeftProperty;
      TimeSpan duration1 = ItemFlyInAndOutAnimations._flyOutSpeed;
      DoubleAnimation doubleAnimation1 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(x, to1, (IEasingFunction) sineEase1, (DependencyObject) image1, (object) dependencyProperty1, duration1);
      storyboard.Children.Add((Timeline) doubleAnimation1);
      double y = this._targetElementPosition.Y;
      double to2 = this._targetElementPosition.Y + 50.0;
      SineEase sineEase2 = new SineEase();
      int num3 = 0;
      sineEase2.EasingMode = (EasingMode) num3;
      Image image2 = this._targetElementClone;
      DependencyProperty dependencyProperty2 = Canvas.TopProperty;
      TimeSpan duration2 = ItemFlyInAndOutAnimations._flyOutSpeed;
      DoubleAnimation doubleAnimation2 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(y, to2, (IEasingFunction) sineEase2, (DependencyObject) image2, (object) dependencyProperty2, duration2);
      storyboard.Children.Add((Timeline) doubleAnimation2);
      DoubleAnimation doubleAnimation3 = ItemFlyInAndOutAnimations.CreateDoubleAnimation(0.0, 1.0, null, (DependencyObject) this._backgroundMask, (object) UIElement.OpacityProperty, ItemFlyInAndOutAnimations._flyOutSpeed);
      storyboard.Children.Add((Timeline) doubleAnimation3);
      EventHandler eventHandler = (EventHandler) ((s, e2) =>
      {
        action();
        element.Dispatcher.BeginInvoke((Action) (() => this._popup.IsOpen = false));
      });
      storyboard.Completed += eventHandler;
      element.Opacity = 0.0;
      this._popup.IsOpen = true;
      storyboard.Begin();
    }

    public static DoubleAnimation CreateDoubleAnimation(double from, double to, IEasingFunction easing, DependencyObject target, object propertyPath, TimeSpan duration)
    {
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.To = new double?(to);
      doubleAnimation.From = new double?(from);
      doubleAnimation.EasingFunction = easing;
      Duration duration1 = (Duration) duration;
      doubleAnimation.Duration = duration1;
      DependencyObject target1 = target;
      Storyboard.SetTarget((Timeline) doubleAnimation, target1);
      PropertyPath path = new PropertyPath(propertyPath);
      Storyboard.SetTargetProperty((Timeline) doubleAnimation, path);
      return doubleAnimation;
    }
  }
}
