using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.Framework
{
  public class AttachedProperties
  {
    public static readonly DependencyProperty AutoSelectTextProperty = DependencyProperty.RegisterAttached("AutoSelectText", typeof (bool), typeof (AttachedProperties), new PropertyMetadata(new PropertyChangedCallback(AttachedProperties.OnAutoSelectTextChanged)));
    public static readonly DependencyProperty PreventAutoSelectTextProperty = DependencyProperty.RegisterAttached("PreventAutoSelectText", typeof (bool), typeof (AttachedProperties), (PropertyMetadata) null);
    public static readonly DependencyProperty ExtraDeltaYCropWhenHidingImageProperty = DependencyProperty.RegisterAttached("ExtraDeltaYCropWhenHidingImage", typeof (double), typeof (AttachedProperties), (PropertyMetadata) null);

    private static void OnAutoSelectTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FrameworkElement frameworkElement = d as FrameworkElement;
      if (frameworkElement == null)
        return;
      if ((bool) e.NewValue)
        frameworkElement.GotFocus += new RoutedEventHandler(AttachedProperties.OnGotFocus);
      else
        frameworkElement.GotFocus -= new RoutedEventHandler(AttachedProperties.OnGotFocus);
    }

    private static void OnGotFocus(object sender, RoutedEventArgs e)
    {
      TextBox textBox = FocusManager.GetFocusedElement() as TextBox;
      if (textBox != null && !(bool) textBox.GetValue(AttachedProperties.PreventAutoSelectTextProperty))
      {
        textBox.SelectAll();
      }
      else
      {
        PasswordBox passwordBox = FocusManager.GetFocusedElement() as PasswordBox;
        if (passwordBox == null || (bool) passwordBox.GetValue(AttachedProperties.PreventAutoSelectTextProperty))
          return;
        passwordBox.SelectAll();
      }
    }

    public static bool GetAutoSelectText(DependencyObject target)
    {
      return (bool) target.GetValue(AttachedProperties.AutoSelectTextProperty);
    }

    public static void SetAutoSelectText(DependencyObject target, bool value)
    {
      target.SetValue(AttachedProperties.AutoSelectTextProperty, (object) value);
    }

    public static bool GetPreventAutoSelectText(DependencyObject target)
    {
      return (bool) target.GetValue(AttachedProperties.PreventAutoSelectTextProperty);
    }

    public static void SetPreventAutoSelectText(DependencyObject target, bool value)
    {
      target.SetValue(AttachedProperties.PreventAutoSelectTextProperty, (object) value);
    }

    public static double GetExtraDeltaYCropWhenHidingImage(DependencyObject target)
    {
      return (double) target.GetValue(AttachedProperties.ExtraDeltaYCropWhenHidingImageProperty);
    }

    public static void SetExtraDeltaYCropWhenHidingImage(DependencyObject target, double value)
    {
      target.SetValue(AttachedProperties.ExtraDeltaYCropWhenHidingImageProperty, (object) value);
    }
  }
}
