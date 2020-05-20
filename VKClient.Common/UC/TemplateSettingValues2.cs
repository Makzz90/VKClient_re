using System.Windows;

namespace VKClient.Common.UC
{
  public class TemplateSettingValues2 : DependencyObject
  {
    public static readonly DependencyProperty MaxSideLengthProperty = DependencyProperty.Register("MaxSideLength", typeof (double), typeof (TemplateSettingValues2), new PropertyMetadata(0.0));
    public static readonly DependencyProperty EllipseDiameterProperty = DependencyProperty.Register("EllipseDiameter", typeof (double), typeof (TemplateSettingValues2), new PropertyMetadata(0.0));
    public static readonly DependencyProperty EllipseOffsetProperty = DependencyProperty.Register("EllipseOffset", typeof(Thickness), typeof(TemplateSettingValues2), new PropertyMetadata(null));

    public double MaxSideLength
    {
      get
      {
        return (double) this.GetValue(TemplateSettingValues2.MaxSideLengthProperty);
      }
      set
      {
        this.SetValue(TemplateSettingValues2.MaxSideLengthProperty, value);
      }
    }

    public double EllipseDiameter
    {
      get
      {
        return (double) this.GetValue(TemplateSettingValues2.EllipseDiameterProperty);
      }
      set
      {
        this.SetValue(TemplateSettingValues2.EllipseDiameterProperty, value);
      }
    }

    public Thickness EllipseOffset
    {
      get
      {
        return (Thickness) this.GetValue(TemplateSettingValues2.EllipseOffsetProperty);
      }
      set
      {
        this.SetValue(TemplateSettingValues2.EllipseOffsetProperty, value);
      }
    }

    public TemplateSettingValues2(double width)
      : this()
    {
      this.EllipseDiameter = width;
      this.EllipseOffset = new Thickness(this.EllipseDiameter);
    }

    public TemplateSettingValues2()
    {
      //base.\u002Ector();
      this.MaxSideLength = 400.0;
    }
  }
}
