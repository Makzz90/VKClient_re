using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;

namespace VKClient.Common.Framework.DatePicker
{
  [TemplatePart(Name = "DateTimeButton", Type = typeof (ButtonBase))]
  public class DateTimePickerBase : Control
  {
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof (DateTime?), typeof (DateTimePickerBase), new PropertyMetadata(null, new PropertyChangedCallback(DateTimePickerBase.OnValueChanged)));
    public static readonly DependencyProperty ValueStringProperty = DependencyProperty.Register("ValueString", typeof (string), typeof (DateTimePickerBase), (PropertyMetadata) null);
    public static readonly DependencyProperty ValueStringFormatProperty = DependencyProperty.Register("ValueStringFormat", typeof (string), typeof (DateTimePickerBase), new PropertyMetadata(null, new PropertyChangedCallback(DateTimePickerBase.OnValueStringFormatChanged)));
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof (object), typeof (DateTimePickerBase), (PropertyMetadata) null);
    public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof (DataTemplate), typeof (DateTimePickerBase), (PropertyMetadata) null);
    public static readonly DependencyProperty PickerPageUriProperty = DependencyProperty.Register("PickerPageUri", typeof (Uri), typeof (DateTimePickerBase), (PropertyMetadata) null);
    private const string ButtonPartName = "DateTimeButton";
    private ButtonBase _dateButtonPart;
    private PhoneApplicationFrame _frame;
    private object _frameContentWhenOpened;
    private NavigationInTransition _savedNavigationInTransition;
    private NavigationOutTransition _savedNavigationOutTransition;
    private IDateTimePickerPage _dateTimePickerPage;

    [TypeConverter(typeof (TimeTypeConverter))]
    public DateTime? Value
    {
      get
      {
        return (DateTime?) this.GetValue(DateTimePickerBase.ValueProperty);
      }
      set
      {
        this.SetValue(DateTimePickerBase.ValueProperty, (object) value);
      }
    }

    public string ValueString
    {
      get
      {
        return (string) this.GetValue(DateTimePickerBase.ValueStringProperty);
      }
      private set
      {
        this.SetValue(DateTimePickerBase.ValueStringProperty, (object) value);
      }
    }

    public string ValueStringFormat
    {
      get
      {
        return (string) this.GetValue(DateTimePickerBase.ValueStringFormatProperty);
      }
      set
      {
        this.SetValue(DateTimePickerBase.ValueStringFormatProperty, (object) value);
      }
    }

    public object Header
    {
      get
      {
        return this.GetValue(DateTimePickerBase.HeaderProperty);
      }
      set
      {
        this.SetValue(DateTimePickerBase.HeaderProperty, value);
      }
    }

    public DataTemplate HeaderTemplate
    {
      get
      {
        return (DataTemplate) this.GetValue(DateTimePickerBase.HeaderTemplateProperty);
      }
      set
      {
        this.SetValue(DateTimePickerBase.HeaderTemplateProperty, (object) value);
      }
    }

    public Uri PickerPageUri
    {
      get
      {
        return (Uri) this.GetValue(DateTimePickerBase.PickerPageUriProperty);
      }
      set
      {
        this.SetValue(DateTimePickerBase.PickerPageUriProperty, (object) value);
      }
    }

    protected virtual string ValueStringFormatFallback
    {
      get
      {
        return "{0}";
      }
    }

    public event EventHandler<DateTimeValueChangedEventArgs> ValueChanged;

    private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ((DateTimePickerBase) o).OnValueChanged((DateTime?) e.OldValue, (DateTime?) e.NewValue);
    }

    private void OnValueChanged(DateTime? oldValue, DateTime? newValue)
    {
      this.UpdateValueString();
      this.OnValueChanged(new DateTimeValueChangedEventArgs(oldValue, newValue));
    }

    protected virtual void OnValueChanged(DateTimeValueChangedEventArgs e)
    {
      EventHandler<DateTimeValueChangedEventArgs> eventHandler = this.ValueChanged;
      if (eventHandler == null)
        return;
      eventHandler((object) this, e);
    }

    private static void OnValueStringFormatChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      ((DateTimePickerBase) o).OnValueStringFormatChanged();
    }

    private void OnValueStringFormatChanged()
    {
      this.UpdateValueString();
    }

    public override void OnApplyTemplate()
    {
      if (this._dateButtonPart != null)
        this._dateButtonPart.Click -= new RoutedEventHandler(this.OnDateButtonClick);
      base.OnApplyTemplate();
      this._dateButtonPart = this.GetTemplateChild("DateTimeButton") as ButtonBase;
      if (this._dateButtonPart == null)
        return;
      this._dateButtonPart.Click += new RoutedEventHandler(this.OnDateButtonClick);
    }

    internal static bool DateShouldFlowRTL()
    {
      string letterIsoLanguageName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
      if (!(letterIsoLanguageName == "ar"))
        return letterIsoLanguageName == "fa";
      return true;
    }

    internal static bool IsRTLLanguage()
    {
      string letterIsoLanguageName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
      if (!(letterIsoLanguageName == "ar") && !(letterIsoLanguageName == "he"))
        return letterIsoLanguageName == "fa";
      return true;
    }

    private void OnDateButtonClick(object sender, RoutedEventArgs e)
    {
      this.OpenPickerPage();
    }

    private void UpdateValueString()
    {
      this.ValueString = string.Format((IFormatProvider) CultureInfo.CurrentCulture, this.ValueStringFormat ?? this.ValueStringFormatFallback, new object[1]
      {
        (object) this.Value
      });
    }

    private void OpenPickerPage()
    {
      if ((Uri) null == this.PickerPageUri)
        throw new ArgumentException("PickerPageUri property must not be null.");
      if (this._frame != null)
        return;
      this._frame = Application.Current.RootVisual as PhoneApplicationFrame;
      if (this._frame == null)
        return;
      this._frameContentWhenOpened = this._frame.Content;
      UIElement element = this._frameContentWhenOpened as UIElement;
      if (element != null)
      {
        this._savedNavigationInTransition = TransitionService.GetNavigationInTransition(element);
        TransitionService.SetNavigationInTransition(element, (NavigationInTransition) null);
        this._savedNavigationOutTransition = TransitionService.GetNavigationOutTransition(element);
        TransitionService.SetNavigationOutTransition(element, (NavigationOutTransition) null);
      }
      this._frame.Navigated += new NavigatedEventHandler(this.OnFrameNavigated);
      this._frame.NavigationStopped += new NavigationStoppedEventHandler(this.OnFrameNavigationStoppedOrFailed);
      this._frame.NavigationFailed += new NavigationFailedEventHandler(this.OnFrameNavigationStoppedOrFailed);
      this._frame.Navigate(this.PickerPageUri);
    }

    private void ClosePickerPage()
    {
      if (this._frame != null)
      {
        this._frame.Navigated -= new NavigatedEventHandler(this.OnFrameNavigated);
        this._frame.NavigationStopped -= new NavigationStoppedEventHandler(this.OnFrameNavigationStoppedOrFailed);
        this._frame.NavigationFailed -= new NavigationFailedEventHandler(this.OnFrameNavigationStoppedOrFailed);
        UIElement element = this._frameContentWhenOpened as UIElement;
        if (element != null)
        {
          TransitionService.SetNavigationInTransition(element, this._savedNavigationInTransition);
          this._savedNavigationInTransition = (NavigationInTransition) null;
          TransitionService.SetNavigationOutTransition(element, this._savedNavigationOutTransition);
          this._savedNavigationOutTransition = (NavigationOutTransition) null;
        }
        this._frame = (PhoneApplicationFrame) null;
        this._frameContentWhenOpened = null;
      }
      if (this._dateTimePickerPage == null)
        return;
      DateTime? nullable = this._dateTimePickerPage.Value;
      if (nullable.HasValue)
      {
        nullable = this._dateTimePickerPage.Value;
        this.Value = new DateTime?(nullable.Value);
      }
      this._dateTimePickerPage = (IDateTimePickerPage) null;
    }

    private void OnFrameNavigated(object sender, NavigationEventArgs e)
    {
      if (e.Content == this._frameContentWhenOpened)
      {
        this.ClosePickerPage();
      }
      else
      {
        if (this._dateTimePickerPage != null)
          return;
        IDateTimePickerPage dateTimePickerPage = e.Content as IDateTimePickerPage;
        if (dateTimePickerPage == null)
          return;
        this._dateTimePickerPage = dateTimePickerPage;
        this._dateTimePickerPage.Value = new DateTime?(this.Value.GetValueOrDefault(DateTime.Now));
        dateTimePickerPage.SetFlowDirection(this.FlowDirection);
      }
    }

    private void OnFrameNavigationStoppedOrFailed(object sender, EventArgs e)
    {
      this.ClosePickerPage();
    }
  }
}
