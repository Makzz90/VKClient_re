using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class OccupationInfoUC : UserControl
  {
      public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(OccupationType), typeof(OccupationInfoUC), new PropertyMetadata(new PropertyChangedCallback(OccupationInfoUC.OnTypeChanged)));
      public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(OccupationInfoUC), new PropertyMetadata(new PropertyChangedCallback(OccupationInfoUC.OnDescriptionChanged)));
      public static readonly DependencyProperty GroupImageProperty = DependencyProperty.Register("GroupImage", typeof(string), typeof(OccupationInfoUC), new PropertyMetadata(new PropertyChangedCallback(OccupationInfoUC.OnGroupImageChanged)));
    internal ScrollableTextBlock textBlockDescription;
    internal Image imageGroup;
    private bool _contentLoaded;

    public OccupationType Type
    {
      get
      {
        return (OccupationType) base.GetValue(OccupationInfoUC.TypeProperty);
      }
      set
      {
        base.SetValue(OccupationInfoUC.TypeProperty, value);
      }
    }

    public string Description
    {
      get
      {
        return (string) base.GetValue(OccupationInfoUC.DescriptionProperty);
      }
      set
      {
        base.SetValue(OccupationInfoUC.DescriptionProperty, value);
      }
    }

    public string GroupImage
    {
      get
      {
        return (string) base.GetValue(OccupationInfoUC.GroupImageProperty);
      }
      set
      {
        base.SetValue(OccupationInfoUC.GroupImageProperty, value);
      }
    }

    public OccupationInfoUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((UIElement) this.imageGroup).Visibility = Visibility.Collapsed;
      this.textBlockDescription.Text = "";
    }

    private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      OccupationInfoUC occupationInfoUc = d as OccupationInfoUC;
      if (occupationInfoUc == null)
        return;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      occupationInfoUc.textBlockDescription.Text = newValue;
    }

    private static void OnGroupImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      OccupationInfoUC occupationInfoUc = d as OccupationInfoUC;
      if (occupationInfoUc == null)
        return;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      if (!string.IsNullOrEmpty(newValue))
      {
        ImageLoader.SetUriSource(occupationInfoUc.imageGroup, newValue);
        ((UIElement) occupationInfoUc.imageGroup).Visibility = Visibility.Visible;
      }
      else
      {
        ImageLoader.SetUriSource(occupationInfoUc.imageGroup, "");
        ((UIElement) occupationInfoUc.imageGroup).Visibility = Visibility.Collapsed;
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/OccupationInfoUC.xaml", UriKind.Relative));
      this.textBlockDescription = (ScrollableTextBlock) base.FindName("textBlockDescription");
      this.imageGroup = (Image) base.FindName("imageGroup");
    }
  }
}
