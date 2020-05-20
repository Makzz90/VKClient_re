using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Common.Utils
{
  public static class FramePageUtils
  {
    public static PhoneApplicationFrame Frame
    {
      get
      {
        return Application.Current.RootVisual as PhoneApplicationFrame;
      }
    }

    public static bool IsHorizontal
    {
      get
      {
        PhoneApplicationFrame frame = FramePageUtils.Frame;
        if (frame == null)
          return false;
        if (frame.Orientation != PageOrientation.Landscape && frame.Orientation != PageOrientation.LandscapeLeft)
            return frame.Orientation == PageOrientation.LandscapeRight;
        return true;
      }
    }

    public static double SoftNavButtonsCurrentSize
    {
      get
      {
        PageBase currentPage = FramePageUtils.CurrentPage;
        if (currentPage == null)
          return 0.0;
        double num = ((FrameworkElement) currentPage).ActualWidth > ((FrameworkElement) currentPage).ActualHeight ? ((FrameworkElement) currentPage).ActualWidth : ((FrameworkElement) currentPage).ActualHeight;
        double actualWidth = Application.Current.Host.Content.ActualWidth;
        double actualHeight = Application.Current.Host.Content.ActualHeight;
        return Math.Max((actualWidth > actualHeight ? actualWidth : actualHeight) - num, 0.0);
      }
    }

    public static PageBase CurrentPage
    {
      get
      {
        PhoneApplicationFrame rootVisual = Application.Current.RootVisual as PhoneApplicationFrame;
        if (rootVisual != null)
          return ((ContentControl) rootVisual).Content as PageBase;
        return  null;
      }
    }

    public static bool IsFrameTransformed()
    {
      PhoneApplicationFrame rootVisual = Application.Current.RootVisual as PhoneApplicationFrame;
      if (rootVisual != null && ((UIElement) rootVisual).RenderTransform != null)
      {
        TranslateTransform translateTransform =  null;
        if (((UIElement) rootVisual).RenderTransform is TransformGroup && !((IList) (((UIElement) rootVisual).RenderTransform as TransformGroup).Children).IsNullOrEmpty())
          translateTransform = ((PresentationFrameworkCollection<Transform>) (((UIElement) rootVisual).RenderTransform as TransformGroup).Children)[0] as TranslateTransform;
        else if (((UIElement) rootVisual).RenderTransform is TranslateTransform)
          translateTransform = ((UIElement) rootVisual).RenderTransform as TranslateTransform;
        if (translateTransform != null)
          return translateTransform.Y != 0.0;
      }
      return false;
    }

    public static TextBox FindNextTextBox(DependencyObject parent, TextBox textbox)
    {
      List<TextBox> textBoxList = FramePageUtils.AllTextBoxes(parent);
      int num = textBoxList.IndexOf(textbox);
      if (num >= 0 && num < textBoxList.Count - 1)
        return textBoxList[num + 1];
      return  null;
    }

    public static List<TextBox> AllTextBoxes(DependencyObject parent)
    {
      List<TextBox> textBoxList = new List<TextBox>();
      for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); ++index)
      {
        DependencyObject child = VisualTreeHelper.GetChild(parent, index);
        if (child is TextBox)
          textBoxList.Add(child as TextBox);
        textBoxList.AddRange((IEnumerable<TextBox>) FramePageUtils.AllTextBoxes(child));
      }
      return textBoxList;
    }
  }
}
