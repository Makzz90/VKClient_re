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
      public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(DateTime?), typeof(DateTimePickerBase), new PropertyMetadata(null, new PropertyChangedCallback(DateTimePickerBase.OnValueChanged)));
    public static readonly DependencyProperty ValueStringProperty = DependencyProperty.Register("ValueString", typeof (string), typeof (DateTimePickerBase),  null);
    public static readonly DependencyProperty ValueStringFormatProperty = DependencyProperty.Register("ValueStringFormat", typeof(string), typeof(DateTimePickerBase), new PropertyMetadata(null, new PropertyChangedCallback(DateTimePickerBase.OnValueStringFormatChanged)));
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof (object), typeof (DateTimePickerBase),  null);
    public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof (DataTemplate), typeof (DateTimePickerBase),  null);
    public static readonly DependencyProperty PickerPageUriProperty = DependencyProperty.Register("PickerPageUri", typeof (Uri), typeof (DateTimePickerBase),  null);
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
        return (DateTime?) base.GetValue(DateTimePickerBase.ValueProperty);
      }
      set
      {
        base.SetValue(DateTimePickerBase.ValueProperty, value);
      }
    }

    public string ValueString
    {
      get
      {
        return (string) base.GetValue(DateTimePickerBase.ValueStringProperty);
      }
      private set
      {
        base.SetValue(DateTimePickerBase.ValueStringProperty, value);
      }
    }

    public string ValueStringFormat
    {
      get
      {
        return (string) base.GetValue(DateTimePickerBase.ValueStringFormatProperty);
      }
      set
      {
        base.SetValue(DateTimePickerBase.ValueStringFormatProperty, value);
      }
    }

    public object Header
    {
      get
      {
        return base.GetValue(DateTimePickerBase.HeaderProperty);
      }
      set
      {
        base.SetValue(DateTimePickerBase.HeaderProperty, value);
      }
    }

    public DataTemplate HeaderTemplate
    {
      get
      {
        return (DataTemplate) base.GetValue(DateTimePickerBase.HeaderTemplateProperty);
      }
      set
      {
        base.SetValue(DateTimePickerBase.HeaderTemplateProperty, value);
      }
    }

    public Uri PickerPageUri
    {
      get
      {
        return (Uri) base.GetValue(DateTimePickerBase.PickerPageUriProperty);
      }
      set
      {
        base.SetValue(DateTimePickerBase.PickerPageUriProperty, value);
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

    public DateTimePickerBase()
    {
      //base.\u002Ector();
    }

    private static void OnValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      ((DateTimePickerBase) o).OnValueChanged((DateTime?) e.OldValue, (DateTime?) e.NewValue);
    }

    private void OnValueChanged(DateTime? oldValue, DateTime? newValue)
    {
      this.UpdateValueString();
      this.OnValueChanged(new DateTimeValueChangedEventArgs(oldValue, newValue));
    }

    protected virtual void OnValueChanged(DateTimeValueChangedEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      EventHandler<DateTimeValueChangedEventArgs> valueChanged = this.ValueChanged;
      if (valueChanged == null)
        return;
      valueChanged(this, e);
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
      {
        this._dateButtonPart.Click-=(new RoutedEventHandler( this.OnDateButtonClick));
      }
      base.OnApplyTemplate();
      this._dateButtonPart = this.GetTemplateChild("DateTimeButton") as ButtonBase;
      if (this._dateButtonPart == null)
        return;
      this._dateButtonPart.Click+=(new RoutedEventHandler( this.OnDateButtonClick));
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
        this.Value
      });
    }

    private void OpenPickerPage()
    {
      if ( null == this.PickerPageUri)
        throw new ArgumentException("PickerPageUri property must not be null.");
      if (this._frame != null)
        return;
      this._frame = Application.Current.RootVisual as PhoneApplicationFrame;
      if (this._frame == null)
        return;
      this._frameContentWhenOpened = ((ContentControl) this._frame).Content;
      UIElement contentWhenOpened = this._frameContentWhenOpened as UIElement;
      if (contentWhenOpened != null)
      {
        this._savedNavigationInTransition = TransitionService.GetNavigationInTransition(contentWhenOpened);
        TransitionService.SetNavigationInTransition(contentWhenOpened,  null);
        this._savedNavigationOutTransition = TransitionService.GetNavigationOutTransition(contentWhenOpened);
        TransitionService.SetNavigationOutTransition(contentWhenOpened,  null);
      }
      // ISSUE: method pointer
      ((Frame) this._frame).Navigated+=(new NavigatedEventHandler( this.OnFrameNavigated));
      // ISSUE: method pointer
      ((Frame) this._frame).NavigationStopped+=(new NavigationStoppedEventHandler( this.OnFrameNavigationStoppedOrFailed));
      // ISSUE: method pointer
      ((Frame) this._frame).NavigationFailed += (new NavigationFailedEventHandler( this.OnFrameNavigationStoppedOrFailed));
      ((Frame) this._frame).Navigate(this.PickerPageUri);
    }

    private void ClosePickerPage()
    {
      if (this._frame != null)
      {
        // ISSUE: method pointer
        ((Frame) this._frame).Navigated -= (new NavigatedEventHandler( this.OnFrameNavigated));
        // ISSUE: method pointer
        ((Frame) this._frame).NavigationStopped-=(new NavigationStoppedEventHandler( this.OnFrameNavigationStoppedOrFailed));
        // ISSUE: method pointer
        ((Frame) this._frame).NavigationFailed -= (new NavigationFailedEventHandler( this.OnFrameNavigationStoppedOrFailed));
        UIElement contentWhenOpened = this._frameContentWhenOpened as UIElement;
        if (contentWhenOpened != null)
        {
          TransitionService.SetNavigationInTransition(contentWhenOpened, this._savedNavigationInTransition);
          this._savedNavigationInTransition =  null;
          TransitionService.SetNavigationOutTransition(contentWhenOpened, this._savedNavigationOutTransition);
          this._savedNavigationOutTransition =  null;
        }
        this._frame =  null;
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
      this._dateTimePickerPage =  null;
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
        IDateTimePickerPage content = e.Content as IDateTimePickerPage;
        if (content == null)
          return;
        this._dateTimePickerPage = content;
        this._dateTimePickerPage.Value = new DateTime?(this.Value.GetValueOrDefault(DateTime.Now));
        content.SetFlowDirection(base.FlowDirection);
      }
    }

    private void OnFrameNavigationStoppedOrFailed(object sender, EventArgs e)
    {
      this.ClosePickerPage();
    }
  }
}
