using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Common.Framework;

namespace VKClient.Common.ImageViewer
{
  public class ImageAnimator
  {
    private IEasingFunction _easingFunction;
    private int _animationDurationMs;

    public ImageAnimator(int animationDurationMs, IEasingFunction easingFunction)
    {
      this._animationDurationMs = animationDurationMs;
      this._easingFunction = easingFunction;
    }

    public void AnimateIn(Size imageSize, Image imageOriginal, Image imageFit, Action completionCallback = null, int startTime = 0)
    {
      if (imageOriginal == null)
      {
        completionCallback();
      }
      else
      {
        Size size = new Size(imageFit.Width, imageFit.Height);
        Rect fill = RectangleUtils.ResizeToFill(RectangleUtils.ResizeToFill(new Size(imageOriginal.ActualWidth, imageOriginal.ActualHeight), imageSize), size);
        Rect target1 = imageOriginal.TransformToVisual((UIElement) imageFit).TransformBounds(fill);
        imageFit.RenderTransform = (Transform) RectangleUtils.TransformRect(new Rect(new Point(), size), target1, false);
        GeneralTransform visual = imageOriginal.TransformToVisual((UIElement) imageFit);
        double y = this.GetDeltaYCrop(imageOriginal);
        if (imageOriginal.ActualHeight < y)
          y = imageOriginal.ActualHeight;
        Rect rect = new Rect(0.0, y, imageOriginal.ActualWidth, imageOriginal.ActualHeight - y);
        Rect source = visual.TransformBounds(rect);
        CompositeTransform target2 = new CompositeTransform();
        Image image = imageFit;
        RectangleGeometry rectangleGeometry = new RectangleGeometry();
        rectangleGeometry.Rect = source;
        CompositeTransform compositeTransform1 = target2;
        rectangleGeometry.Transform = (Transform) compositeTransform1;
        image.Clip = (Geometry) rectangleGeometry;
        CompositeTransform compositeTransform2 = RectangleUtils.TransformRect(source, new Rect(new Point(), size), false);
        target2.Animate(0.0, compositeTransform2.TranslateY, (object) CompositeTransform.TranslateYProperty, this._animationDurationMs, new int?(startTime), this._easingFunction, null);
        target2.Animate(0.0, compositeTransform2.TranslateX, (object) CompositeTransform.TranslateXProperty, this._animationDurationMs, new int?(startTime), this._easingFunction, null);
        target2.Animate(1.0, compositeTransform2.ScaleX, (object) CompositeTransform.ScaleXProperty, this._animationDurationMs, new int?(startTime), this._easingFunction, null);
        target2.Animate(1.0, compositeTransform2.ScaleY, (object) CompositeTransform.ScaleYProperty, this._animationDurationMs, new int?(startTime), this._easingFunction, null);
        CompositeTransform target3 = imageFit.RenderTransform as CompositeTransform;
        target3.Animate(target3.TranslateX, 0.0, (object) CompositeTransform.TranslateXProperty, this._animationDurationMs, new int?(startTime), this._easingFunction, completionCallback);
        target3.Animate(target3.TranslateY, 0.0, (object) CompositeTransform.TranslateYProperty, this._animationDurationMs, new int?(startTime), this._easingFunction, null);
        target3.Animate(target3.ScaleX, 1.0, (object) CompositeTransform.ScaleXProperty, this._animationDurationMs, new int?(startTime), this._easingFunction, null);
        target3.Animate(target3.ScaleY, 1.0, (object) CompositeTransform.ScaleYProperty, this._animationDurationMs, new int?(startTime), this._easingFunction, null);
      }
    }

    private double GetDeltaYCrop(Image imageOriginal)
    {
      ViewportControl viewportControl = (ViewportControl) null;
      for (FrameworkElement frameworkElement = (FrameworkElement) imageOriginal; frameworkElement != null; frameworkElement = VisualTreeHelper.GetParent((DependencyObject) frameworkElement) as FrameworkElement)
      {
        if (frameworkElement is ViewportControl)
        {
          viewportControl = frameworkElement as ViewportControl;
          break;
        }
      }
      double num1 = 0.0;
      if (viewportControl != null)
      {
        double num2 = imageOriginal.TransformToVisual((UIElement) viewportControl).TransformBounds(new Rect(0.0, 0.0, imageOriginal.ActualWidth, imageOriginal.ActualHeight)).Top - AttachedProperties.GetExtraDeltaYCropWhenHidingImage((DependencyObject) viewportControl);
        if (num2 < 0.0)
          num1 = -num2;
      }
      return num1;
    }

    public void AnimateOut(Size imageSize, Image imageOriginal, Image imageFit, bool? clockwiseRotation, Action completionCallback = null)
    {
      CompositeTransform compositeTransform1 = imageFit.RenderTransform as CompositeTransform;
      if (imageOriginal == null || compositeTransform1.ScaleX != 1.0)
      {
        this.AnimateFlyout(completionCallback, compositeTransform1);
      }
      else
      {
        Size size = new Size(imageFit.ActualWidth, imageFit.ActualHeight);
        Rect fill = RectangleUtils.ResizeToFill(RectangleUtils.ResizeToFill(new Size(imageOriginal.ActualWidth, imageOriginal.ActualHeight), imageSize), size);
        Rect rect = imageOriginal.TransformToVisual((UIElement) imageFit).TransformBounds(fill);
        if (clockwiseRotation.HasValue)
          rect = RectangleUtils.Rotate90(rect);
        CompositeTransform compositeTransform2 = RectangleUtils.TransformRect(new Rect(new Point(), size), rect, true);
        compositeTransform1.CenterX = imageFit.Width / 2.0;
        compositeTransform1.CenterY = imageFit.Height / 2.0;
        double num1 = imageFit.Width / fill.Width;
        double num2 = this.GetDeltaYCrop(imageOriginal);
        if (imageOriginal.ActualHeight < num2)
          num2 = imageOriginal.ActualHeight;
        Rect target1 = new Rect(-fill.X * num1, (-fill.Y + num2) * num1, imageOriginal.ActualWidth * num1, (imageOriginal.ActualHeight - num2) * num1);
        if (target1.Width < 10.0 || target1.Height < 10.0)
        {
          this.AnimateFlyout(completionCallback, compositeTransform1);
        }
        else
        {
          RectangleGeometry rectangleGeometry = new RectangleGeometry();
          Rect source = new Rect(0.0, 0.0, imageFit.Width, imageFit.Height);
          rectangleGeometry.Rect = source;
          imageFit.Clip = (Geometry) rectangleGeometry;
          CompositeTransform target2 = new CompositeTransform();
          rectangleGeometry.Transform = (Transform) target2;
          CompositeTransform compositeTransform3 = RectangleUtils.TransformRect(source, target1, false);
          target2.Animate(0.0, compositeTransform3.TranslateY, (object) CompositeTransform.TranslateYProperty, this._animationDurationMs, new int?(0), this._easingFunction, null);
          target2.Animate(0.0, compositeTransform3.TranslateX, (object) CompositeTransform.TranslateXProperty, this._animationDurationMs, new int?(0), this._easingFunction, null);
          target2.Animate(1.0, compositeTransform3.ScaleX, (object) CompositeTransform.ScaleXProperty, this._animationDurationMs, new int?(0), this._easingFunction, null);
          target2.Animate(1.0, compositeTransform3.ScaleY, (object) CompositeTransform.ScaleYProperty, this._animationDurationMs, new int?(0), this._easingFunction, null);
          if (clockwiseRotation.HasValue)
            compositeTransform1.Animate(compositeTransform1.Rotation, clockwiseRotation.Value ? compositeTransform1.Rotation + 90.0 : compositeTransform1.Rotation - 90.0, (object) CompositeTransform.RotationProperty, this._animationDurationMs, new int?(0), this._easingFunction, null);
          compositeTransform1.Animate(compositeTransform1.TranslateX, compositeTransform1.TranslateX + compositeTransform2.TranslateX, (object) CompositeTransform.TranslateXProperty, this._animationDurationMs, new int?(0), this._easingFunction, null);
          compositeTransform1.Animate(compositeTransform1.TranslateY, compositeTransform1.TranslateY + compositeTransform2.TranslateY, (object) CompositeTransform.TranslateYProperty, this._animationDurationMs, new int?(0), this._easingFunction, null);
          compositeTransform1.Animate(compositeTransform1.ScaleX, compositeTransform2.ScaleX, (object) CompositeTransform.ScaleXProperty, this._animationDurationMs, new int?(0), this._easingFunction, null);
          compositeTransform1.Animate(compositeTransform1.ScaleY, compositeTransform2.ScaleY, (object) CompositeTransform.ScaleYProperty, this._animationDurationMs, new int?(0), this._easingFunction, completionCallback);
        }
      }
    }

    private void AnimateFlyout(Action completionCallback, CompositeTransform imageFitTransform)
    {
      CompositeTransform target = imageFitTransform;
      double translateY = imageFitTransform.TranslateY;
      double to = 1000.0;
      DependencyProperty dependencyProperty = CompositeTransform.TranslateYProperty;
      int duration = this._animationDurationMs;
      int? startTime = new int?(0);
      ExponentialEase exponentialEase = new ExponentialEase();
      exponentialEase.Exponent = 6.0;
      int num = 1;
      exponentialEase.EasingMode = (EasingMode) num;
      Action completed = completionCallback;
      target.Animate(translateY, to, (object) dependencyProperty, duration, startTime, (IEasingFunction) exponentialEase, completed);
    }
  }
}
