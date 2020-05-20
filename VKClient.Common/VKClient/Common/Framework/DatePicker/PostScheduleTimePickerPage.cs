using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Localization;

namespace VKClient.Common.Framework.DatePicker
{
  public class PostScheduleTimePickerPage : DateTimePickerPageBase
  {
    internal Grid gridRoot;
    internal VisualStateGroup VisibilityStates;
    internal VisualState Open;
    internal VisualState Closed;
    internal PlaneProjection PlaneProjection;
    internal Rectangle SystemTrayPlaceholder;
    internal TextBlock HeaderTitle;
    internal LoopingSelector PrimarySelector;
    internal LoopingSelector SecondarySelector;
    internal LoopingSelector TertiarySelector;
    private bool _contentLoaded;

    public PostScheduleTimePickerPage()
    {
      this.InitializeComponent();
      this.PrimarySelector.DataSource = (ILoopingSelectorDataSource) new TwentyFourHourDataSource();
      this.SecondarySelector.DataSource = (ILoopingSelectorDataSource) new MinuteDataSource();
      this.TertiarySelector.DataSource = (ILoopingSelectorDataSource) new AmPmDataSource();
      this.InitializeDateTimePickerPage(this.PrimarySelector, this.SecondarySelector, this.TertiarySelector);
      this.HeaderTitle.Text = CommonResources.PostponedNews_PublishTime;
    }

    protected override IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern()
    {
      string pattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.ToUpperInvariant();
      if (DateTimePickerBase.IsRTLLanguage())
      {
        string[] strArray = pattern.Split(' ');
        Array.Reverse((Array) strArray);
        pattern = string.Join(" ", strArray);
      }
      return DateTimePickerPageBase.GetSelectorsOrderedByCulturePattern(pattern, new char[3]{ 'H', 'M', 'T' }, new LoopingSelector[3]
      {
        this.PrimarySelector,
        this.SecondarySelector,
        this.TertiarySelector
      });
    }

    protected override void OnOrientationChanged(OrientationChangedEventArgs e)
    {
      if (e == null)
        throw new ArgumentNullException("e");
      base.OnOrientationChanged(e);
      this.SystemTrayPlaceholder.Visibility = (PageOrientation.Portrait & e.Orientation) != PageOrientation.None ? Visibility.Visible : Visibility.Collapsed;
    }

    public override void SetFlowDirection(FlowDirection flowDirection)
    {
      this.HeaderTitle.FlowDirection = flowDirection;
      this.PrimarySelector.FlowDirection = flowDirection;
      this.SecondarySelector.FlowDirection = flowDirection;
      this.TertiarySelector.FlowDirection = flowDirection;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Framework/DatePicker/PostScheduleTimePickerPage.xaml", UriKind.Relative));
      this.gridRoot = (Grid) this.FindName("gridRoot");
      this.VisibilityStates = (VisualStateGroup) this.FindName("VisibilityStates");
      this.Open = (VisualState) this.FindName("Open");
      this.Closed = (VisualState) this.FindName("Closed");
      this.PlaneProjection = (PlaneProjection) this.FindName("PlaneProjection");
      this.SystemTrayPlaceholder = (Rectangle) this.FindName("SystemTrayPlaceholder");
      this.HeaderTitle = (TextBlock) this.FindName("HeaderTitle");
      this.PrimarySelector = (LoopingSelector) this.FindName("PrimarySelector");
      this.SecondarySelector = (LoopingSelector) this.FindName("SecondarySelector");
      this.TertiarySelector = (LoopingSelector) this.FindName("TertiarySelector");
    }
  }
}
