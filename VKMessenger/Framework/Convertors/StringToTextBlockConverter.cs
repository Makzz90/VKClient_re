using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using VKMessenger.Views;

namespace VKMessenger.Framework.Convertors
{
  public class StringToTextBlockConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return new TextBlock();
      string str1 = value as string;
      TextBlock textBlock = new TextBlock();
      ((FrameworkElement) textBlock).Style=((Style) Application.Current.Resources[parameter]);
      textBlock.TextWrapping=((TextWrapping) 1);
      SolidColorBrush solidColorBrush1 = new SolidColorBrush((Color) Application.Current.Resources["PhoneAccentColor"]);
      char[] chArray = new char[1]{ ' ' };
      IEnumerator<string> enumerator1 = ((IEnumerable<string>)Enumerable.Where<string>(str1.Split(chArray), (Func<string, bool>)(s => !string.IsNullOrWhiteSpace(s)))).GetEnumerator();
      try
      {
        while (((IEnumerator) enumerator1).MoveNext())
        {
          string current1 = enumerator1.Current;
          bool flag = false;
          IList<string> stringList = (IList<string>) new List<string>();
          if (((ContentControl) MessengerStateManagerInstance.Current.RootFrame).Content is ConversationsSearch)
            stringList = (IList<string>) MessengerStateManagerInstance.Current.ConversationSearchStrings;
          IEnumerator<string> enumerator2 = stringList.GetEnumerator();
          try
          {
            while (((IEnumerator) enumerator2).MoveNext())
            {
              string current2 = enumerator2.Current;
              if (current1.StartsWith(current2, StringComparison.CurrentCultureIgnoreCase))
              {
                InlineCollection inlines1 = textBlock.Inlines;
                Run run1 = new Run();
                string str2 = current1.Substring(0, current2.Length);
                run1.Text = str2;
                SolidColorBrush solidColorBrush2 = solidColorBrush1;
                ((TextElement) run1).Foreground = ((Brush) solidColorBrush2);
                ((PresentationFrameworkCollection<Inline>) inlines1).Add((Inline) run1);
                InlineCollection inlines2 = textBlock.Inlines;
                Run run2 = new Run();
                string str3 = current1.Substring(current2.Length);
                run2.Text = str3;
                ((PresentationFrameworkCollection<Inline>) inlines2).Add((Inline) run2);
                flag = true;
                break;
              }
            }
          }
          finally
          {
            if (enumerator2 != null)
              ((IDisposable) enumerator2).Dispose();
          }
          if (!flag)
          {
            InlineCollection inlines = textBlock.Inlines;
            Run run = new Run();
            string str2 = current1;
            run.Text = str2;
            ((PresentationFrameworkCollection<Inline>) inlines).Add((Inline) run);
          }
          InlineCollection inlines3 = textBlock.Inlines;
          Run run3 = new Run();
          string str4 = " ";
          run3.Text = str4;
          ((PresentationFrameworkCollection<Inline>) inlines3).Add((Inline) run3);
        }
      }
      finally
      {
        if (enumerator1 != null)
          ((IDisposable) enumerator1).Dispose();
      }
      return textBlock;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException("This converter cannot be used in two-way binding.");
    }
  }
}
