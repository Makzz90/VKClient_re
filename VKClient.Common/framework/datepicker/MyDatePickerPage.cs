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
  public class MyDatePickerPage : DateTimePickerPageBase
  {
    internal Grid gridRoot;
    internal VisualStateGroup VisibilityStates;
    internal VisualState Open;
    internal VisualState Closed;
    internal PlaneProjection PlaneProjection;
    internal Rectangle SystemTrayPlaceholder;
    internal TextBlock HeaderTitle;
    internal LoopingSelector SecondarySelector;
    internal LoopingSelector TertiarySelector;
    internal LoopingSelector PrimarySelector;
    private bool _contentLoaded;

    public MyDatePickerPage()
    {
      this.InitializeComponent();
      this.PrimarySelector.DataSource = (ILoopingSelectorDataSource) new YearDataSource();
      this.SecondarySelector.DataSource = (ILoopingSelectorDataSource) new MonthDataSource();
      this.TertiarySelector.DataSource = (ILoopingSelectorDataSource) new DayDataSource();
      this.InitializeDateTimePickerPage(this.PrimarySelector, this.SecondarySelector, this.TertiarySelector);
      this.HeaderTitle.Text = CommonResources.Settings_EditProfile_ChooseBirthdate;
    }

    protected override IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern()
    {
      string pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToUpperInvariant();
      if (MyDatePickerPage.DateShouldFlowRTL())
      {
        char[] charArray = pattern.ToCharArray();
        Array.Reverse((Array) charArray);
        pattern = new string(charArray);
      }
      return DateTimePickerPageBase.GetSelectorsOrderedByCulturePattern(pattern, new char[3]{ 'Y', 'M', 'D' }, new LoopingSelector[3]{ this.PrimarySelector, this.SecondarySelector, this.TertiarySelector });
    }

    internal static bool DateShouldFlowRTL()
    {
      string letterIsoLanguageName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
      if (!(letterIsoLanguageName == "ar"))
        return letterIsoLanguageName == "fa";
      return true;
    }

    protected override void OnOrientationChanged(OrientationChangedEventArgs e)
    {
      if (e == null)
        throw new ArgumentNullException("e");
      base.OnOrientationChanged(e);
      ((UIElement)this.SystemTrayPlaceholder).Visibility = ((PageOrientation.Portrait & e.Orientation) != PageOrientation.None ? Visibility.Visible : Visibility.Collapsed);
    }

    public override void SetFlowDirection(FlowDirection flowDirection)
    {
      ((FrameworkElement) this.HeaderTitle).FlowDirection = flowDirection;
      ((FrameworkElement) this.PrimarySelector).FlowDirection = flowDirection;
      ((FrameworkElement) this.SecondarySelector).FlowDirection = flowDirection;
      ((FrameworkElement) this.TertiarySelector).FlowDirection = flowDirection;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Framework/DatePicker/MyDatePickerPage.xaml", UriKind.Relative));
      this.gridRoot = (Grid) base.FindName("gridRoot");
      this.VisibilityStates = (VisualStateGroup) base.FindName("VisibilityStates");
      this.Open = (VisualState) base.FindName("Open");
      this.Closed = (VisualState) base.FindName("Closed");
      this.PlaneProjection = (PlaneProjection) base.FindName("PlaneProjection");
      this.SystemTrayPlaceholder = (Rectangle) base.FindName("SystemTrayPlaceholder");
      this.HeaderTitle = (TextBlock) base.FindName("HeaderTitle");
      this.SecondarySelector = (LoopingSelector) base.FindName("SecondarySelector");
      this.TertiarySelector = (LoopingSelector) base.FindName("TertiarySelector");
      this.PrimarySelector = (LoopingSelector) base.FindName("PrimarySelector");
    }
  }
}
