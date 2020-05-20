using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class GamesCatalogHeaderUC : UserControl
  {
    internal StackPanel panelContent;
    internal TextBlock textBlockTitle;
    internal Border borderNew;
    private bool _contentLoaded;

    public GamesCatalogHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void BorderNew_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      TextBlock textBlockTitle = this.textBlockTitle;
      double actualWidth1 = ((FrameworkElement) this.panelContent).ActualWidth;
      double num1;
      if (((FrameworkElement) this.borderNew).ActualWidth <= 0.0)
      {
        num1 = 0.0;
      }
      else
      {
        double actualWidth2 = ((FrameworkElement) this.borderNew).ActualWidth;
        Thickness margin = ((FrameworkElement) this.borderNew).Margin;
        // ISSUE: explicit reference operation
        double left = ((Thickness) @margin).Left;
        num1 = actualWidth2 + left;
      }
      double num2 = actualWidth1 - num1;
      ((FrameworkElement) textBlockTitle).MaxWidth = num2;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GamesCatalogHeaderUC.xaml", UriKind.Relative));
      this.panelContent = (StackPanel) base.FindName("panelContent");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.borderNew = (Border) base.FindName("borderNew");
    }
  }
}
