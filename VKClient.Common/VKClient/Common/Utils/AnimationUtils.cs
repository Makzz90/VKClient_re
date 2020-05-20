using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VKClient.Common.Utils
{
    public static class AnimationUtils
    {
        public const string SwivelInStoryboard = "<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimation BeginTime=\"0:0:0\" Duration=\"0\" To=\".5\"\r\n                                Storyboard.TargetProperty=\"(UIElement.Projection).(PlaneProjection.CenterOfRotationY)\" />\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Projection).(PlaneProjection.RotationX)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"-30\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.35\" Value=\"0\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseOut\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Opacity)\">\r\n                <DiscreteDoubleKeyFrame KeyTime=\"0\" Value=\"1\" />\r\n            </DoubleAnimationUsingKeyFrames>\r\n        </Storyboard>";
        public const string SwivelOutStoryboard = "<Storyboard xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r\n            <DoubleAnimation BeginTime=\"0:0:0\" Duration=\"0\" \r\n                                Storyboard.TargetProperty=\"(UIElement.Projection).(PlaneProjection.CenterOfRotationY)\" \r\n                                To=\".5\"/>\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Projection).(PlaneProjection.RotationX)\">\r\n                <EasingDoubleKeyFrame KeyTime=\"0\" Value=\"0\"/>\r\n                <EasingDoubleKeyFrame KeyTime=\"0:0:0.25\" Value=\"45\">\r\n                    <EasingDoubleKeyFrame.EasingFunction>\r\n                        <ExponentialEase EasingMode=\"EaseIn\" Exponent=\"6\"/>\r\n                    </EasingDoubleKeyFrame.EasingFunction>\r\n                </EasingDoubleKeyFrame>\r\n            </DoubleAnimationUsingKeyFrames>\r\n            <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=\"(UIElement.Opacity)\">\r\n                <DiscreteDoubleKeyFrame KeyTime=\"0\" Value=\"1\" />\r\n                <DiscreteDoubleKeyFrame KeyTime=\"0:0:0.267\" Value=\"0\" />\r\n            </DoubleAnimationUsingKeyFrames>\r\n        </Storyboard>";

        public static void RunAnimation(string animationXaml, FrameworkElement target)
        {
            Storyboard storyboard = XamlReader.Load(animationXaml) as Storyboard;
            target.Projection = (Projection)new PlaneProjection();
            foreach (Timeline child in (PresentationFrameworkCollection<Timeline>)storyboard.Children)
                Storyboard.SetTarget(child, (DependencyObject)target);
            storyboard.Begin();
        }

        public static void Animate(double to, DependencyObject target, string propertyName, double durationSeconds)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.To = new double?(to);
            doubleAnimation.AutoReverse = false;
            doubleAnimation.Duration = (Duration)TimeSpan.FromSeconds(durationSeconds);
            doubleAnimation.EasingFunction = (IEasingFunction)new CubicEase();
            Storyboard.SetTargetProperty((Timeline)doubleAnimation, new PropertyPath(propertyName, new object[0]));
            Storyboard.SetTarget((Timeline)doubleAnimation, target);
            storyboard.Children.Add((Timeline)doubleAnimation);
            storyboard.Begin();
        }

        public static Storyboard Animate(this DependencyObject target, double from, double to, object propertyPath, int duration, int? startTime, IEasingFunction easing = null, Action completed = null, bool autoReverse = false)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.To = new double?(to);
            doubleAnimation.From = new double?(from);
            doubleAnimation.AutoReverse = autoReverse;
            doubleAnimation.EasingFunction = easing;
            doubleAnimation.Duration = (Duration)TimeSpan.FromMilliseconds((double)duration);
            Storyboard.SetTarget((Timeline)doubleAnimation, target);
            Storyboard.SetTargetProperty((Timeline)doubleAnimation, new PropertyPath(propertyPath));
            Storyboard storyboard = new Storyboard();
            if (startTime.HasValue)
                storyboard.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds((double)startTime.Value));
            else
                storyboard.BeginTime = new TimeSpan?();
            if (completed != null)
                storyboard.Completed += (EventHandler)((s, e) => completed());
            storyboard.Children.Add((Timeline)doubleAnimation);
            storyboard.Begin();
            return storyboard;
        }

        public static void SetHorizontalOffset(this FrameworkElement fe, double offset)
        {
            TranslateTransform translateTransform1 = fe.RenderTransform as TranslateTransform;
            if (translateTransform1 == null)
            {
                TranslateTransform translateTransform2 = new TranslateTransform()
                {
                    X = offset
                };
                fe.RenderTransform = (Transform)translateTransform2;
            }
            else
                translateTransform1.X = offset;
        }

        public static AnimationUtils.Offset GetHorizontalOffset(this FrameworkElement fe)
        {
            TranslateTransform translateTransform = fe.RenderTransform as TranslateTransform;
            if (translateTransform == null)
            {
                translateTransform = new TranslateTransform()
                {
                    X = 0.0
                };
                fe.RenderTransform = (Transform)translateTransform;
            }
            return new AnimationUtils.Offset()
            {
                Transform = translateTransform,
                Value = translateTransform.X
            };
        }

        public static void SetVerticalOffset(this FrameworkElement fe, double offset)
        {
            TranslateTransform translateTransform1 = fe.RenderTransform as TranslateTransform;
            if (translateTransform1 == null)
            {
                TranslateTransform translateTransform2 = new TranslateTransform()
                {
                    Y = offset
                };
                fe.RenderTransform = (Transform)translateTransform2;
            }
            else
                translateTransform1.Y = offset;
        }

        public static AnimationUtils.Offset GetVerticalOffset(this FrameworkElement fe)
        {
            TranslateTransform translateTransform = fe.RenderTransform as TranslateTransform;
            if (translateTransform == null)
            {
                translateTransform = new TranslateTransform()
                {
                    Y = 0.0
                };
                fe.RenderTransform = (Transform)translateTransform;
            }
            return new AnimationUtils.Offset()
            {
                Transform = translateTransform,
                Value = translateTransform.Y
            };
        }

        public static void InvokeOnNextLayoutUpdated(this FrameworkElement element, Action action)
        {
            EventHandler handler = null;
            handler = (EventHandler)((s, e2) =>
            {
                element.LayoutUpdated -= handler;
                action();
            });
            element.LayoutUpdated += handler;
        }

        public struct Offset
        {
            public double Value { get; set; }

            public TranslateTransform Transform { get; set; }
        }
    }
}
