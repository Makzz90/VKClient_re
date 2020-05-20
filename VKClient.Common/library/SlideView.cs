using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;

namespace VKClient.Common.Library
{
  public class SlideView : Grid, INotifyPropertyChanged
  {
    private const double MOVE_TO_NEXT_VELOCITY_THRESHOLD = 100.0;
    private const int DURATION_BOUNCING = 175;
    private const int DURATION_MOVE_TO_NEXT = 200;
    private static readonly IEasingFunction ANIMATION_EASING;
    public static readonly DependencyProperty ItemTemplateProperty;
    public static readonly DependencyProperty ItemsSourceProperty;
    public static readonly DependencyProperty IsCycledProperty;
    public static readonly DependencyProperty AutoSlideIntervalProperty;
    private int _selectedIndex;
    private List<FrameworkElement> _elements;
    private bool _isAnimating;
    private DispatcherTimer _autoSlideTimer;

    public DataTemplate ItemTemplate
    {
      get
      {
        return (DataTemplate) base.GetValue(SlideView.ItemTemplateProperty);
      }
      set
      {
        base.SetValue(SlideView.ItemTemplateProperty, value);
      }
    }

    public IList ItemsSource
    {
      get
      {
        return (IList) base.GetValue(SlideView.ItemsSourceProperty);
      }
      set
      {
        base.SetValue(SlideView.ItemsSourceProperty, value);
      }
    }

    public bool IsCycled
    {
      get
      {
        return (bool) base.GetValue(SlideView.IsCycledProperty);
      }
      set
      {
        base.SetValue(SlideView.IsCycledProperty, value);
      }
    }

    public TimeSpan? AutoSlideInterval
    {
      get
      {
        return (TimeSpan?) base.GetValue(SlideView.AutoSlideIntervalProperty);
      }
      set
      {
        base.SetValue(SlideView.AutoSlideIntervalProperty, value);
      }
    }

    public int SelectedIndex
    {
      get
      {
        return this._selectedIndex;
      }
      set
      {
        int selectedIndex = this._selectedIndex;
        this._selectedIndex = value;
        this.NotifyPropertyChanged("SelectedIndex");
        if (this._selectedIndex - selectedIndex == 2)
        {
          this.SwapElements(0, 2);
          this.UpdateSources(false, true, true);
          this.ArrangeElements();
        }
        else if (selectedIndex - this._selectedIndex == 2)
        {
          this.SwapElements(0, 2);
          this.UpdateSources(true, true, false);
          this.ArrangeElements();
        }
        else if (this._selectedIndex - selectedIndex == 1)
        {
          this.MoveToNextOrPrevious(true);
          this.ArrangeElements();
        }
        else if (selectedIndex - this._selectedIndex == 1)
        {
          this.MoveToNextOrPrevious(false);
          this.ArrangeElements();
        }
        else
          this.UpdateSources(new bool?());
        if (this.SelectionChanged == null)
          return;
        this.SelectionChanged(this, this._selectedIndex);
      }
    }

    public IManipulationHandler ParentManipulationHandler { get; set; }

    public event EventHandler<int> SelectionChanged;

    public event PropertyChangedEventHandler PropertyChanged;

    static SlideView()
    {
      CubicEase cubicEase = new CubicEase();
      int num = 0;
      ((EasingFunctionBase) cubicEase).EasingMode = ((EasingMode) num);
      SlideView.ANIMATION_EASING = (IEasingFunction) cubicEase;
      // ISSUE: method pointer
      SlideView.ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(SlideView), new PropertyMetadata(new PropertyChangedCallback(SlideView.ItemTemplate_OnChanged)));
      // ISSUE: method pointer
      SlideView.ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(SlideView), new PropertyMetadata(new PropertyChangedCallback(SlideView.ItemsSource_OnChanged)));
      // ISSUE: method pointer
      SlideView.IsCycledProperty = DependencyProperty.Register("IsCycled", typeof(bool), typeof(SlideView), new PropertyMetadata(new PropertyChangedCallback(SlideView.IsCycled_OnChanged)));
      // ISSUE: method pointer
      SlideView.AutoSlideIntervalProperty = DependencyProperty.Register("AutoSlideInterval", typeof(TimeSpan?), typeof(SlideView), new PropertyMetadata(new PropertyChangedCallback(SlideView.AutoSlideInterval_OnChanged)));
    }

    public SlideView()
    {
      //base.\u002Ector();
      // ISSUE: method pointer
      base.SizeChanged += (new SizeChangedEventHandler( this.OnSizeChanged));
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.OnLoaded));
      // ISSUE: method pointer
      base.Unloaded += (new RoutedEventHandler( this.OnUnloaded));
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      this.StartAutoSlide();
    }

    private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
    {
      this.StopAutoSlide();
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.CreateElements();
      this.ArrangeElements();
      this.UpdateSources(new bool?());
    }

    private static void ItemTemplate_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((SlideView) d).UpdateItemTemplate();
    }

    private void UpdateItemTemplate()
    {
      if (this.ItemTemplate == null)
        return;
      this.CreateElements();
      using (List<FrameworkElement>.Enumerator enumerator = this._elements.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ContentPresenter contentPresenter = this.GetContentPresenter(enumerator.Current);
          if (contentPresenter != null)
          {
            contentPresenter.ContentTemplate = this.ItemTemplate;
            BindingOperations.SetBinding((DependencyObject) contentPresenter, (DependencyProperty) ContentPresenter.ContentProperty, (BindingBase) new Binding());
          }
        }
      }
    }

    private static void ItemsSource_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      SlideView slideView = (SlideView) d;
      slideView.UpdateSources(new bool?());
      // ISSUE: explicit reference operation
      if (e.NewValue != null)
        slideView.SelectedIndex = 0;
      slideView.StartAutoSlide();
    }

    private static void IsCycled_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((SlideView) d).UpdateSources(new bool?());
    }

    private static void AutoSlideInterval_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((SlideView) d).StartAutoSlide();
    }

    private void CreateElements()
    {
      if (this._elements != null)
        return;
      this._elements = new List<FrameworkElement>(3);
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this).Children).Clear();
      for (int index = 0; index < 3; ++index)
      {
        Border border1 = new Border();
        TranslateTransform translateTransform = new TranslateTransform();
        ((UIElement) border1).RenderTransform = ((Transform) translateTransform);
        BitmapCache bitmapCache = new BitmapCache();
        ((UIElement) border1).CacheMode = ((CacheMode) bitmapCache);
        SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
        border1.Background = ((Brush) solidColorBrush);
        Border border2 = border1;
        ((UIElement) border2).ManipulationStarted += (new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted));
        ((UIElement) border2).ManipulationDelta += (new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta));
        ((UIElement) border2).ManipulationCompleted += (new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted));
        if (this.ItemTemplate != null)
        {
          ContentPresenter contentPresenter1 = new ContentPresenter();
          DataTemplate itemTemplate = this.ItemTemplate;
          contentPresenter1.ContentTemplate = itemTemplate;
          ContentPresenter contentPresenter2 = contentPresenter1;
          BindingOperations.SetBinding((DependencyObject) contentPresenter2, (DependencyProperty) ContentPresenter.ContentProperty, (BindingBase) new Binding());
          border2.Child = ((UIElement) contentPresenter2);
        }
        this._elements.Add((FrameworkElement) border2);
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this).Children).Add((UIElement) border2);
      }
    }

    private ContentPresenter GetContentPresenter(FrameworkElement element)
    {
      if (element == null)
        return  null;
      return (ContentPresenter) ((Border) element).Child;
    }

    private TranslateTransform GetElementTransform(int index)
    {
      return (TranslateTransform) ((UIElement) this._elements[index]).RenderTransform;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.ArrangeElements();
    }

    private void ArrangeElements()
    {
      if (this._elements == null)
        return;
      this.GetElementTransform(0).X=(-base.ActualWidth);
      this.GetElementTransform(1).X = 0.0;
      this.GetElementTransform(2).X=(base.ActualWidth);
    }

    private void UpdateSources(bool update0, bool update1, bool update2)
    {
      if (update1)
        SlideView.SetDataContext(this._elements[1], this.GetItem(this._selectedIndex));
      if (update0)
        SlideView.SetDataContext(this._elements[0], this.GetItem(this._selectedIndex - 1));
      if (!update2)
        return;
      SlideView.SetDataContext(this._elements[2], this.GetItem(this._selectedIndex + 1));
    }

    private void UpdateSources(bool? movedForvard = null)
    {
      if (this._elements == null)
        return;
      if (!movedForvard.HasValue)
        SlideView.SetDataContext(this._elements[1], this.GetItem(this._selectedIndex));
      int num = !movedForvard.HasValue ? 1 : (movedForvard.Value ? 1 : 0);
      if ((!movedForvard.HasValue ? 1 : (!movedForvard.Value ? 1 : 0)) != 0)
        SlideView.SetDataContext(this._elements[0], this.GetItem(this._selectedIndex - 1));
      if (num == 0)
        return;
      SlideView.SetDataContext(this._elements[2], this.GetItem(this._selectedIndex + 1));
    }

    private static void SetDataContext(FrameworkElement element, object dataContext)
    {
      ISupportDataContext supportDataContext = element as ISupportDataContext;
      if (supportDataContext != null)
        supportDataContext.SetDataContext(dataContext);
      else
        element.DataContext = dataContext;
    }

    private object GetItem(int index)
    {
      if (this.ItemsSource == null || this.ItemsSource.Count == 0)
        return null;
      if (index < 0)
      {
        if (!this.IsCycled)
          return null;
        return this.ItemsSource[this.ItemsSource.Count - 1];
      }
      if (index < this.ItemsSource.Count)
        return this.ItemsSource[index];
      if (!this.IsCycled)
        return null;
      return this.ItemsSource[0];
    }

    private void Element_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      DispatcherTimer autoSlideTimer = this._autoSlideTimer;
      if (autoSlideTimer != null)
        autoSlideTimer.Stop();
      if (this.ParentManipulationHandler != null)
        return;
      e.Handled = true;
    }

    private void Element_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      if (e.PinchManipulation != null || this.ItemsSource == null)
        return;
      Point translation = e.DeltaManipulation.Translation;
      if (this.ParentManipulationHandler == null)
      {
        e.Handled = true;
        this.HandleDragDelta(translation);
      }
      else
      {
        if (this.ParentManipulationHandler.Handled.HasValue && this.ParentManipulationHandler.Handled.Value)
          return;
        this.HandleDragDelta(translation);
        bool? handled = this.ParentManipulationHandler.Handled;
        if (!handled.HasValue)
          return;
        handled = this.ParentManipulationHandler.Handled;
        if (handled.Value)
          return;
        e.Handled = true;
      }
    }

    private void Element_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      DispatcherTimer autoSlideTimer = this._autoSlideTimer;
      if (autoSlideTimer != null)
        autoSlideTimer.Start();
      if (this.ParentManipulationHandler == null)
        e.Handled = true;
      if (this.ItemsSource == null)
        return;
      Point linearVelocity = e.FinalVelocities.LinearVelocity;
      // ISSUE: explicit reference operation
      this.HandleDragCompleted(((Point) @linearVelocity).X);
    }

    private void HandleDragDelta(Point translation)
    {
      if (this._isAnimating)
        return;
      // ISSUE: explicit reference operation
      double x = ((Point) @translation).X;
      // ISSUE: explicit reference operation
      if (Math.Abs(((Point) @translation).Y) > Math.Abs(x))
        return;
      if ((this._selectedIndex == 0 && x > 0.0 || this._selectedIndex == this.ItemsSource.Count - 1 && x < 0.0) && !this.IsCycled)
        x /= 3.0;
      using (List<FrameworkElement>.Enumerator enumerator = this._elements.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          TranslateTransform elementTransform = this.GetElementTransform(this._elements.IndexOf(enumerator.Current));
          double num = elementTransform.X + x;
          elementTransform.X = num;
        }
      }
    }

    private void HandleDragCompleted(double hVelocity)
    {
      if (this._isAnimating)
        return;
      double num1 = hVelocity;
      bool? moveNext = new bool?();
      double x = this.GetElementTransform(1).X;
      double num2 = num1;
      if ((num2 < -100.0 && x < 0.0 || x <= -base.Width / 2.0) && (this._selectedIndex < this.ItemsSource.Count - 1 || this.IsCycled))
        moveNext = new bool?(true);
      else if ((num2 > 100.0 && x > 0.0 || x >= base.Width / 2.0) && (this._selectedIndex > 0 || this.IsCycled))
        moveNext = new bool?(false);
      this.SlideElements(moveNext);
    }

    private void SlideElements(bool? moveNext)
    {
      bool flag1 = this.SelectedIndex <= 1;
      bool flag2 = this.SelectedIndex >= this.ItemsSource.Count - 2;
      bool? nullable1 = moveNext;
      bool flag3 = true;
      double num1;
      if ((nullable1.GetValueOrDefault() == flag3 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
      {
        num1 = !flag2 ? -base.ActualWidth : -base.ActualWidth;
      }
      else
      {
        bool? nullable2 = moveNext;
        bool flag4 = false;
        num1 = (nullable2.GetValueOrDefault() == flag4 ? (nullable2.HasValue ? 1 : 0) : 0) == 0 ? (this.SelectedIndex <= this.ItemsSource.Count - 2 ? (this.SelectedIndex >= 1 ? 0.0 : 0.0) : (this.ItemsSource.Count <= 1 ? 0.0 : 0.0)) : (!flag1 ? base.ActualWidth : base.ActualWidth);
      }
      double x = this.GetElementTransform(1).X;
      double delta1 = num1 - x;
      if (moveNext.HasValue)
      {
        this._isAnimating = true;
        List<int> intList = new List<int>();
        if (moveNext.Value)
        {
          intList.Add(1);
          intList.Add(2);
        }
        else
        {
          intList.Add(0);
          intList.Add(1);
        }
        this.AnimateElements((IEnumerable) intList, delta1, (Action) (() =>
        {
          this.MoveToNextOrPrevious(moveNext.Value);
          this.ArrangeElements();
          this._isAnimating = false;
        }), true);
        this.ChangeCurrentInd(moveNext.Value);
      }
      else
      {
        if (delta1 == 0.0)
          return;
        this._isAnimating = true;
        List<int> intList = new List<int>();
        intList.Add(0);
        intList.Add(1);
        intList.Add(2);
        double delta2 = delta1;
        Action completedCallback = (Action) (() => this._isAnimating = false);
        int num2 = 0;
        this.AnimateElements((IEnumerable) intList, delta2, completedCallback, num2 != 0);
      }
    }

    private void AnimateElements(IEnumerable indexes, double delta, Action completedCallback, bool moveNextOrPrevious)
    {
      int num = moveNextOrPrevious ? 200 : 175;
      List<AnimationInfo> animInfoList = new List<AnimationInfo>();
      foreach (int index in indexes)
      {
        if (index < this._elements.Count)
        {
          TranslateTransform elementTransform = this.GetElementTransform(index);
          animInfoList.Add(new AnimationInfo()
          {
            from = elementTransform.X,
            to = elementTransform.X + delta,
            propertyPath = TranslateTransform.XProperty,
            duration = num,
            target = (DependencyObject) elementTransform,
            easing = SlideView.ANIMATION_EASING
          });
        }
      }
      AnimationUtil.AnimateSeveral(animInfoList, new int?(0), completedCallback);
    }

    private void MoveToNextOrPrevious(bool next)
    {
      if (next)
      {
        this.SwapElements(0, 1);
        this.SwapElements(1, 2);
      }
      else
      {
        this.SwapElements(1, 2);
        this.SwapElements(0, 1);
      }
      this.UpdateSources(new bool?(next));
    }

    private void ChangeCurrentInd(bool next)
    {
      if (next)
      {
        this._selectedIndex = this._selectedIndex + 1;
        if (this.IsCycled && this._selectedIndex >= this.ItemsSource.Count)
          this._selectedIndex = 0;
      }
      else
      {
        this._selectedIndex = this._selectedIndex - 1;
        if (this.IsCycled && this._selectedIndex < 0)
          this._selectedIndex = this.ItemsSource.Count - 1;
      }
      // ISSUE: reference to a compiler-generated field
      if (this.SelectionChanged != null)
      {
        // ISSUE: reference to a compiler-generated field
        this.SelectionChanged(null, this._selectedIndex);
      }
      // ISSUE: method reference
      this.NotifyPropertyChanged<int>(() => this.SelectedIndex);
    }

    private void SwapElements(int index1, int index2)
    {
      FrameworkElement element = this._elements[index1];
      this._elements[index1] = this._elements[index2];
      this._elements[index2] = element;
    }

    private void StartAutoSlide()
    {
      this.StopAutoSlide();
      TimeSpan? autoSlideInterval = this.AutoSlideInterval;
      if (!autoSlideInterval.HasValue)
        return;
      if (this._autoSlideTimer == null)
        this._autoSlideTimer = new DispatcherTimer();
      this._autoSlideTimer.Interval = autoSlideInterval.Value;
      this._autoSlideTimer.Tick+=(new EventHandler(this.AutoSlideTimer_OnTick));
      this._autoSlideTimer.Start();
    }

    private void StopAutoSlide()
    {
      if (this._autoSlideTimer == null)
        return;
      this._autoSlideTimer.Tick-=(new EventHandler(this.AutoSlideTimer_OnTick));
      if (!this._autoSlideTimer.IsEnabled)
        return;
      this._autoSlideTimer.Stop();
    }

    private void AutoSlideTimer_OnTick(object sender, EventArgs eventArgs)
    {
      if (this.ItemsSource == null || this.ItemsSource.Count < 2)
        return;
      this.SlideElements(new bool?(true));
    }

    private void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
      if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
        return;
      this.RaisePropertyChanged(((MemberExpression) propertyExpression.Body).Member.Name);
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
      this.RaisePropertyChanged(propertyName);
    }

    private void RaisePropertyChanged(string property)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.PropertyChanged == null)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        // ISSUE: reference to a compiler-generated field
        PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
        if (propertyChanged == null)
          return;
        PropertyChangedEventArgs e = new PropertyChangedEventArgs(property);
        propertyChanged(this, e);
      }));
    }
  }
}
