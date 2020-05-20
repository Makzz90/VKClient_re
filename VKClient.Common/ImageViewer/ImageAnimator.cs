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
        Size childSize=new Size(((FrameworkElement) imageFit).Width, ((FrameworkElement) imageFit).Height);
        Rect fill = RectangleUtils.ResizeToFill(RectangleUtils.ResizeToFill(new Size(((FrameworkElement) imageOriginal).ActualWidth, ((FrameworkElement) imageOriginal).ActualHeight), imageSize), childSize);
        Rect target = ((UIElement) imageOriginal).TransformToVisual((UIElement) imageFit).TransformBounds(fill);
        ((UIElement) imageFit).RenderTransform = ((Transform) RectangleUtils.TransformRect(new Rect(new Point(), childSize), target, false));
        GeneralTransform visual = ((UIElement) imageOriginal).TransformToVisual((UIElement) imageFit);
        double num = this.GetDeltaYCrop(imageOriginal);
        if (((FrameworkElement) imageOriginal).ActualHeight < num)
          num = ((FrameworkElement) imageOriginal).ActualHeight;
        Rect rect1 = new Rect(0.0, num, ((FrameworkElement) imageOriginal).ActualWidth, ((FrameworkElement) imageOriginal).ActualHeight - num);
        Rect source = visual.TransformBounds(rect1);
        CompositeTransform compositeTransform1 = new CompositeTransform();
        Image image = imageFit;
        RectangleGeometry rectangleGeometry = new RectangleGeometry();
        Rect rect2 = source;
        rectangleGeometry.Rect = rect2;
        CompositeTransform compositeTransform2 = compositeTransform1;
        ((Geometry) rectangleGeometry).Transform=((Transform) compositeTransform2);
        ((UIElement) image).Clip=((Geometry) rectangleGeometry);
        CompositeTransform compositeTransform3 = RectangleUtils.TransformRect(source, new Rect( new Point(), childSize), false);
        ((DependencyObject) compositeTransform1).Animate(0.0, compositeTransform3.TranslateY, CompositeTransform.TranslateYProperty, this._animationDurationMs, new int?(startTime), this._easingFunction,  null);
        ((DependencyObject) compositeTransform1).Animate(0.0, compositeTransform3.TranslateX, CompositeTransform.TranslateXProperty, this._animationDurationMs, new int?(startTime), this._easingFunction,  null);
        ((DependencyObject) compositeTransform1).Animate(1.0, compositeTransform3.ScaleX, CompositeTransform.ScaleXProperty, this._animationDurationMs, new int?(startTime), this._easingFunction,  null);
        ((DependencyObject) compositeTransform1).Animate(1.0, compositeTransform3.ScaleY, CompositeTransform.ScaleYProperty, this._animationDurationMs, new int?(startTime), this._easingFunction,  null);
        CompositeTransform renderTransform = ((UIElement) imageFit).RenderTransform as CompositeTransform;
        ((DependencyObject) renderTransform).Animate(renderTransform.TranslateX, 0.0, CompositeTransform.TranslateXProperty, this._animationDurationMs, new int?(startTime), this._easingFunction, completionCallback);
        ((DependencyObject) renderTransform).Animate(renderTransform.TranslateY, 0.0, CompositeTransform.TranslateYProperty, this._animationDurationMs, new int?(startTime), this._easingFunction,  null);
        ((DependencyObject) renderTransform).Animate(renderTransform.ScaleX, 1.0, CompositeTransform.ScaleXProperty, this._animationDurationMs, new int?(startTime), this._easingFunction,  null);
        ((DependencyObject) renderTransform).Animate(renderTransform.ScaleY, 1.0, CompositeTransform.ScaleYProperty, this._animationDurationMs, new int?(startTime), this._easingFunction,  null);
      }
    }

    private double GetDeltaYCrop(Image imageOriginal)
    {
      ViewportControl viewportControl =  null;
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
        Rect rect = ((UIElement) imageOriginal).TransformToVisual((UIElement) viewportControl).TransformBounds(new Rect(0.0, 0.0, ((FrameworkElement) imageOriginal).ActualWidth, ((FrameworkElement) imageOriginal).ActualHeight));
        // ISSUE: explicit reference operation
        double num2 = rect.Top - AttachedProperties.GetExtraDeltaYCropWhenHidingImage((DependencyObject) viewportControl);
        if (num2 < 0.0)
          num1 = -num2;
      }
      return num1;
    }

    public void AnimateOut(Size imageSize, Image imageOriginal, Image imageFit, bool? clockwiseRotation, Action completionCallback = null)
    {
      CompositeTransform renderTransform = ((UIElement) imageFit).RenderTransform as CompositeTransform;
      if (imageOriginal == null || renderTransform.ScaleX != 1.0)
      {
        this.AnimateFlyout(completionCallback, renderTransform);
      }
      else
      {
        Size childSize=new Size(((FrameworkElement) imageFit).ActualWidth, ((FrameworkElement) imageFit).ActualHeight);
        Rect fill = RectangleUtils.ResizeToFill(RectangleUtils.ResizeToFill(new Size(((FrameworkElement) imageOriginal).ActualWidth, ((FrameworkElement) imageOriginal).ActualHeight), imageSize), childSize);
        Rect rect = ((UIElement) imageOriginal).TransformToVisual((UIElement) imageFit).TransformBounds(fill);
        if (clockwiseRotation.HasValue)
          rect = RectangleUtils.Rotate90(rect);
        CompositeTransform compositeTransform1 = RectangleUtils.TransformRect(new Rect( new Point(), childSize), rect, true);
        renderTransform.CenterX=(((FrameworkElement) imageFit).Width / 2.0);
        renderTransform.CenterY=(((FrameworkElement) imageFit).Height / 2.0);
        // ISSUE: explicit reference operation
        double num1 = ((FrameworkElement) imageFit).Width / ((Rect) @fill).Width;
        double num2 = this.GetDeltaYCrop(imageOriginal);
        if (((FrameworkElement) imageOriginal).ActualHeight < num2)
          num2 = ((FrameworkElement) imageOriginal).ActualHeight;
        Rect target=new Rect(-((Rect) @fill).X * num1, (-((Rect) @fill).Y + num2) * num1, ((FrameworkElement) imageOriginal).ActualWidth * num1, (((FrameworkElement) imageOriginal).ActualHeight - num2) * num1);
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        if (((Rect) @target).Width < 10.0 || ((Rect) @target).Height < 10.0)
        {
          this.AnimateFlyout(completionCallback, renderTransform);
        }
        else
        {
          RectangleGeometry rectangleGeometry = new RectangleGeometry();
          Rect source=new Rect(0.0, 0.0, ((FrameworkElement) imageFit).Width, ((FrameworkElement) imageFit).Height);
          rectangleGeometry.Rect = source;
          ((UIElement) imageFit).Clip=((Geometry) rectangleGeometry);
          CompositeTransform compositeTransform2 = new CompositeTransform();
          ((Geometry) rectangleGeometry).Transform=((Transform) compositeTransform2);
          CompositeTransform compositeTransform3 = RectangleUtils.TransformRect(source, target, false);
          ((DependencyObject) compositeTransform2).Animate(0.0, compositeTransform3.TranslateY, CompositeTransform.TranslateYProperty, this._animationDurationMs, new int?(0), this._easingFunction,  null);
          ((DependencyObject) compositeTransform2).Animate(0.0, compositeTransform3.TranslateX, CompositeTransform.TranslateXProperty, this._animationDurationMs, new int?(0), this._easingFunction,  null);
          ((DependencyObject) compositeTransform2).Animate(1.0, compositeTransform3.ScaleX, CompositeTransform.ScaleXProperty, this._animationDurationMs, new int?(0), this._easingFunction,  null);
          ((DependencyObject) compositeTransform2).Animate(1.0, compositeTransform3.ScaleY, CompositeTransform.ScaleYProperty, this._animationDurationMs, new int?(0), this._easingFunction,  null);
          if (clockwiseRotation.HasValue)
            ((DependencyObject) renderTransform).Animate(renderTransform.Rotation, clockwiseRotation.Value ? renderTransform.Rotation + 90.0 : renderTransform.Rotation - 90.0, CompositeTransform.RotationProperty, this._animationDurationMs, new int?(0), this._easingFunction,  null);
          ((DependencyObject) renderTransform).Animate(renderTransform.TranslateX, renderTransform.TranslateX + compositeTransform1.TranslateX, CompositeTransform.TranslateXProperty, this._animationDurationMs, new int?(0), this._easingFunction,  null);
          ((DependencyObject) renderTransform).Animate(renderTransform.TranslateY, renderTransform.TranslateY + compositeTransform1.TranslateY, CompositeTransform.TranslateYProperty, this._animationDurationMs, new int?(0), this._easingFunction,  null);
          ((DependencyObject) renderTransform).Animate(renderTransform.ScaleX, compositeTransform1.ScaleX, CompositeTransform.ScaleXProperty, this._animationDurationMs, new int?(0), this._easingFunction,  null);
          ((DependencyObject) renderTransform).Animate(renderTransform.ScaleY, compositeTransform1.ScaleY, CompositeTransform.ScaleYProperty, this._animationDurationMs, new int?(0), this._easingFunction, completionCallback);
        }
      }
    }

    private void AnimateFlyout(Action completionCallback, CompositeTransform imageFitTransform)
    {
      CompositeTransform compositeTransform = imageFitTransform;
      double translateY = imageFitTransform.TranslateY;
      double to = 1000.0;
      int animationDurationMs = this._animationDurationMs;
      int? startTime = new int?(0);
      ExponentialEase exponentialEase = new ExponentialEase();
      double num1 = 6.0;
      exponentialEase.Exponent = num1;
      int num2 = 1;
      ((EasingFunctionBase) exponentialEase).EasingMode=((EasingMode) num2);
      Action completed = completionCallback;
      ((DependencyObject)compositeTransform).Animate(translateY, to, CompositeTransform.TranslateYProperty, animationDurationMs, startTime, (IEasingFunction)exponentialEase, completed);
    }
  }
}
