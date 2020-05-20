using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class CareerItemInfoUC : UserControl
  {
      public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(CareerItemInfoUC), new PropertyMetadata(new PropertyChangedCallback(CareerItemInfoUC.OnDescriptionChanged)));
      public static readonly DependencyProperty GroupImageProperty = DependencyProperty.Register("GroupImage", typeof(string), typeof(CareerItemInfoUC), new PropertyMetadata(new PropertyChangedCallback(CareerItemInfoUC.OnGroupImageChanged)));
    internal TextBlock textBlockDescription;
    internal Ellipse imageGroupPlaceholder;
    internal Image imageGroup;
    private bool _contentLoaded;

    public string Description
    {
      get
      {
        return (string) base.GetValue(CareerItemInfoUC.DescriptionProperty);
      }
      set
      {
        base.SetValue(CareerItemInfoUC.DescriptionProperty, value);
      }
    }

    public string GroupImage
    {
      get
      {
        return (string) base.GetValue(CareerItemInfoUC.GroupImageProperty);
      }
      set
      {
        base.SetValue(CareerItemInfoUC.GroupImageProperty, value);
      }
    }

    public CareerItemInfoUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((UIElement) this.imageGroupPlaceholder).Visibility = Visibility.Collapsed;
      ((UIElement) this.imageGroup).Visibility = Visibility.Collapsed;
      this.textBlockDescription.Text=("");
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      CareerItemInfoUC careerItemInfoUc = d as CareerItemInfoUC;
      if (careerItemInfoUc == null)
        return;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      careerItemInfoUc.textBlockDescription.Text = newValue;
    }

    private static void OnGroupImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      CareerItemInfoUC careerItemInfoUc = d as CareerItemInfoUC;
      if (careerItemInfoUc == null)
        return;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      if (!string.IsNullOrEmpty(newValue))
      {
        ImageLoader.SetUriSource(careerItemInfoUc.imageGroup, newValue);
        ((UIElement) careerItemInfoUc.imageGroupPlaceholder).Visibility = Visibility.Visible;
        ((UIElement) careerItemInfoUc.imageGroup).Visibility = Visibility.Visible;
      }
      else
      {
        ImageLoader.SetUriSource(careerItemInfoUc.imageGroup, "");
        ((UIElement) careerItemInfoUc.imageGroupPlaceholder).Visibility = Visibility.Collapsed;
        ((UIElement) careerItemInfoUc.imageGroup).Visibility = Visibility.Collapsed;
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/CareerItemInfoUC.xaml", UriKind.Relative));
      this.textBlockDescription = (TextBlock) base.FindName("textBlockDescription");
      this.imageGroupPlaceholder = (Ellipse) base.FindName("imageGroupPlaceholder");
      this.imageGroup = (Image) base.FindName("imageGroup");
    }
  }
}
