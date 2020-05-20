using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class ProgressRing : Control
  {
      public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(ProgressRing), new PropertyMetadata(false, new PropertyChangedCallback(ProgressRing.IsActiveChanged)));
    public static readonly DependencyProperty TemplateSettingsProperty = DependencyProperty.Register("TemplateSettings", typeof (TemplateSettingValues), typeof (ProgressRing), new PropertyMetadata(new TemplateSettingValues(100.0, 10)));
    public static readonly DependencyProperty EllipseDiameterFactorProperty = DependencyProperty.Register("EllipseDiameterFactor", typeof (int), typeof (ProgressRing), new PropertyMetadata(10));
    private bool _hasAppliedTemplate;
    private const int DEFAULT_ELLIPSE_DIAMETER_FACTOR = 10;

    public bool IsActive
    {
      get
      {
        return (bool) base.GetValue(ProgressRing.IsActiveProperty);
      }
      set
      {
        base.SetValue(ProgressRing.IsActiveProperty, value);
      }
    }

    public TemplateSettingValues TemplateSettings
    {
      get
      {
        return (TemplateSettingValues) base.GetValue(ProgressRing.TemplateSettingsProperty);
      }
      set
      {
        base.SetValue(ProgressRing.TemplateSettingsProperty, value);
      }
    }

    public int EllipseDiameterFactor
    {
      get
      {
        return (int) base.GetValue(ProgressRing.EllipseDiameterFactorProperty);
      }
      set
      {
        base.SetValue(ProgressRing.EllipseDiameterFactorProperty, value);
      }
    }

    public ProgressRing()
    {
      //base.\u002Ector();
      this.DefaultStyleKey = (typeof (ProgressRing));
      this.TemplateSettings = new TemplateSettingValues(60.0, this.EllipseDiameterFactor);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this._hasAppliedTemplate = true;
      this.UpdateState(this.IsActive);
    }

    private void UpdateState(bool isActive)
    {
      if (!this._hasAppliedTemplate)
        return;
      VisualStateManager.GoToState((Control) this, isActive ? "Active" : "Inactive", true);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      double width = 100.0;
      if (!DesignerProperties.IsInDesignTool)
      {
        // ISSUE: explicit reference operation
        width = base.Width != double.NaN ? base.Width : ((Size) @availableSize).Width;
      }
      this.TemplateSettings = new TemplateSettingValues(width, this.EllipseDiameterFactor);
      return base.MeasureOverride(availableSize);
    }

    private static void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
      // ISSUE: explicit reference operation
      ((ProgressRing) d).UpdateState((bool) ((DependencyPropertyChangedEventArgs) @args).NewValue);
    }
  }
}
