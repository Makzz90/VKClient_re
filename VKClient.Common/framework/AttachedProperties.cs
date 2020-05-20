using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.Framework
{
    public class AttachedProperties
    {
        public static readonly DependencyProperty AutoSelectTextProperty = DependencyProperty.RegisterAttached("AutoSelectText", typeof(bool), typeof(AttachedProperties), new PropertyMetadata(new PropertyChangedCallback(AttachedProperties.OnAutoSelectTextChanged)));
        public static readonly DependencyProperty PreventAutoSelectTextProperty = DependencyProperty.RegisterAttached("PreventAutoSelectText", typeof(bool), typeof(AttachedProperties), null);
        public static readonly DependencyProperty ExtraDeltaYCropWhenHidingImageProperty = DependencyProperty.RegisterAttached("ExtraDeltaYCropWhenHidingImage", typeof(double), typeof(AttachedProperties), null);

        private static void OnAutoSelectTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement frameworkElement = d as FrameworkElement;
            if (frameworkElement == null)
                return;
            // ISSUE: explicit reference operation
            if ((bool)e.NewValue)
            {
                // ISSUE: method pointer
                frameworkElement.GotFocus += (new RoutedEventHandler(AttachedProperties.OnGotFocus));
            }
            else
            {
                // ISSUE: method pointer
                frameworkElement.GotFocus -= (new RoutedEventHandler(AttachedProperties.OnGotFocus));
            }
        }

        private static void OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox focusedElement1 = FocusManager.GetFocusedElement() as TextBox;
            if (focusedElement1 != null && !(bool)((DependencyObject)focusedElement1).GetValue(AttachedProperties.PreventAutoSelectTextProperty))
            {
                focusedElement1.SelectAll();
            }
            else
            {
                PasswordBox focusedElement2 = FocusManager.GetFocusedElement() as PasswordBox;
                if (focusedElement2 == null || (bool)((DependencyObject)focusedElement2).GetValue(AttachedProperties.PreventAutoSelectTextProperty))
                    return;
                focusedElement2.SelectAll();
            }
        }

        public static bool GetAutoSelectText(DependencyObject target)
        {
            return (bool)target.GetValue(AttachedProperties.AutoSelectTextProperty);
        }

        public static void SetAutoSelectText(DependencyObject target, bool value)
        {
            target.SetValue(AttachedProperties.AutoSelectTextProperty, value);
        }

        public static bool GetPreventAutoSelectText(DependencyObject target)
        {
            return (bool)target.GetValue(AttachedProperties.PreventAutoSelectTextProperty);
        }

        public static void SetPreventAutoSelectText(DependencyObject target, bool value)
        {
            target.SetValue(AttachedProperties.PreventAutoSelectTextProperty, value);
        }

        public static double GetExtraDeltaYCropWhenHidingImage(DependencyObject target)
        {
            return (double)target.GetValue(AttachedProperties.ExtraDeltaYCropWhenHidingImageProperty);
        }

        public static void SetExtraDeltaYCropWhenHidingImage(DependencyObject target, double value)
        {
            target.SetValue(AttachedProperties.ExtraDeltaYCropWhenHidingImageProperty, value);
        }
    }
}
