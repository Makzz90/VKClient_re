using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library.Games;
using Windows.Foundation;

namespace VKClient.Common.UC
{
  public class GamesSlideView : UserControl, INotifyPropertyChanged, IHandle<GameDisconnectedEvent>, IHandle
  {
    private const double MOVE_TO_NEXT_VELOCITY_THRESHOLD = 100.0;
    private const double HIDE_VELOCITY_THRESHOLD = 100.0;
    private const int DURATION_BOUNCING = 175;
    private const int DURATION_MOVE_TO_NEXT = 200;
    private static readonly IEasingFunction ANIMATION_EASING;
    public static readonly DependencyProperty BackgroundColorProperty;
    private ObservableCollection<object> _items;
    private int _selectedIndex;
    private List<GameView> _elements;
    private static int _instanceCount;
    private bool _isReadyToHideFired;
    private bool _isAnimating;
    private bool _isInVerticalSwipe;
    private bool _isSwipeEnabled;
    internal Grid gridRoot;
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    public Func<GameView> CreateSingleElement { get; set; }

    public bool ChangeIndexBeforeAnimation { get; set; }

    public double NextElementMargin { get; set; }

    public double NextHeaderMargin { get; set; }

    public bool IsScrollListeningEnabled { get; set; }

    public bool AllowVerticalSwipe { get; set; }

    public Brush BackgroundColor
    {
      get
      {
        return (Brush) base.GetValue(GamesSlideView.BackgroundColorProperty);
      }
      set
      {
        base.SetValue(GamesSlideView.BackgroundColorProperty, value);
      }
    }

    public ObservableCollection<object> Items
    {
      get
      {
        return this._items;
      }
      set
      {
        if (this._items != null)
          this._items.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.ItemsOnCollectionChanged);
        this._items = value;
        if (this._items != null)
          this._items.CollectionChanged += new NotifyCollectionChangedEventHandler(this.ItemsOnCollectionChanged);
        this.OnPropertyChanged("Items");
        this.EnsureElements();
        this.ArrangeElements();
        this.SelectedIndex = 0;
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
        this.OnPropertyChanged("SelectedIndex");
        if (this._selectedIndex - selectedIndex == 2)
        {
          GamesSlideView.Swap(this._elements, 0, 2);
          this.UpdateSources(false, true, true);
          this.ArrangeElements();
        }
        else if (selectedIndex - this._selectedIndex == 2)
        {
          GamesSlideView.Swap(this._elements, 0, 2);
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
          this.UpdateSources(false, new bool?());
      }
    }

    private GameView CurrentElement
    {
      get
      {
        return this._elements[1];
      }
    }

    private double ArrangeWidth
    {
      get
      {
        return base.ActualWidth - this.NextElementMargin;
      }
    }

    public event TypedEventHandler<GamesSlideView, int> SelectionChanged;

    public event EventHandler ItemsCleared;

    //public event EventHandler SwipedToHide;

    public event PropertyChangedEventHandler PropertyChanged;

    static GamesSlideView()
    {
      CubicEase cubicEase = new CubicEase();
      int num = 0;
      ((EasingFunctionBase) cubicEase).EasingMode = ((EasingMode) num);
      GamesSlideView.ANIMATION_EASING = (IEasingFunction) cubicEase;
      // ISSUE: method pointer
      GamesSlideView.BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(GamesSlideView), new PropertyMetadata(new PropertyChangedCallback(GamesSlideView.OnBackgroundBrushChanged)));
    }

    public GamesSlideView()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.SizeChanged += (new SizeChangedEventHandler( this.OnSizeChanged));
      base.DataContext = this;
      EventAggregator.Current.Subscribe(this);
      ++GamesSlideView._instanceCount;
    }

    public GamesSlideView(double nextElementMargin)
      : this()
    {
      this.NextElementMargin = nextElementMargin;
    }

    ~GamesSlideView()
    {
      //try
      //{
        --GamesSlideView._instanceCount;
      //}
      //finally
      //{
      //  // ISSUE: explicit finalizer call
      //  // ISSUE: explicit non-virtual call
      //  this.Finalize();
      //}
    }

    private static void OnBackgroundBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesSlideView gamesSlideView = d as GamesSlideView;
      if (gamesSlideView == null)
        return;
      // ISSUE: explicit reference operation
      Brush newValue = e.NewValue as Brush;
      if (newValue == null)
        return;
      ((Panel) gamesSlideView.gridRoot).Background = newValue;
    }

    private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      this.UpdateSources(true, true, true);
      if (this.SelectedIndex >= this.Items.Count)
        this.SelectedIndex = this.Items.Count - 1;
      // ISSUE: reference to a compiler-generated field
      if (this.Items.Count != 0 || this.ItemsCleared == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.ItemsCleared(this, EventArgs.Empty);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.ArrangeElements();
    }

    private void ArrangeElements()
    {
      double num = this.SelectedIndex != 0 ? (this.SelectedIndex != this.Items.Count - 1 ? 0.0 : this.NextElementMargin) : 0.0;
      Canvas.SetZIndex((UIElement) this._elements[0], 0);
      Canvas.SetZIndex((UIElement) this._elements[1], 1);
      Canvas.SetZIndex((UIElement) this._elements[2], 2);
      (((UIElement) this._elements[0]).RenderTransform as TranslateTransform).X=(-this.ArrangeWidth + num);
      (((UIElement) this._elements[1]).RenderTransform as TranslateTransform).X = num;
      (((UIElement) this._elements[2]).RenderTransform as TranslateTransform).X=(this.ArrangeWidth + num);
      (((UIElement) this._elements[0].Header).RenderTransform as TranslateTransform).X = this.NextHeaderMargin;
      (((UIElement) this._elements[1].Header).RenderTransform as TranslateTransform).X = 0.0;
      (((UIElement) this._elements[2].Header).RenderTransform as TranslateTransform).X=(-this.NextHeaderMargin);
    }

    private void UpdateSources(bool update0, bool update1, bool update2)
    {
      object obj1 = this.GetItem(this._selectedIndex - 1);
      object obj2 = this.GetItem(this._selectedIndex);
      object obj3 = this.GetItem(this._selectedIndex + 1);
      if (update0)
        this._elements[0].SetDataContext(obj1);
      if (update1)
        this._elements[1].SetDataContext(obj2);
      if (update2)
        this._elements[2].SetDataContext(obj3);
      this._elements[0].SetNextDataContext(obj2);
      this._elements[1].SetNextDataContext(obj3);
      this._elements[2].SetNextDataContext(null);
      this._elements[0].SetState(false);
      this._elements[1].SetState(true);
      this._elements[2].SetState(false);
    }

    private void UpdateSources(bool keepCurrentAsIs = false, bool? movedForvard = null)
    {
      int num1 = !movedForvard.HasValue ? 1 : (movedForvard.Value ? 1 : 0);
      int num2 = !movedForvard.HasValue ? 1 : (!movedForvard.Value ? 1 : 0);
      object obj1 = this.GetItem(this._selectedIndex - 1);
      object obj2 = this.GetItem(this._selectedIndex);
      object obj3 = this.GetItem(this._selectedIndex + 1);
      if (!keepCurrentAsIs && !movedForvard.HasValue)
        this._elements[1].SetDataContext(obj2);
      if (num2 != 0)
        this._elements[0].SetDataContext(obj1);
      if (num1 != 0)
        this._elements[2].SetDataContext(obj3);
      this._elements[0].SetNextDataContext(obj2);
      this._elements[1].SetNextDataContext(obj3);
      this._elements[2].SetNextDataContext(null);
      this._elements[0].SetState(false);
      this._elements[1].SetState(true);
      this._elements[2].SetState(false);
    }

    private object GetItem(int ind)
    {
      if (ind < 0 || ind >= this.Items.Count)
        return null;
      return this.Items[ind];
    }

    private void EnsureElements()
    {
      if (this._elements != null)
        return;
      this._elements = new List<GameView>(3)
      {
        this.CreateSingleElement(),
        this.CreateSingleElement(),
        this.CreateSingleElement()
      };
      foreach (GameView element in this._elements)
      {
        ((UIElement) element).RenderTransform = ((Transform) new TranslateTransform());
        ((UIElement) element).CacheMode = ((CacheMode) new BitmapCache());
        ((UIElement) element).ManipulationStarted += (new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted));
        ((UIElement) element).ManipulationDelta += (new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta));
        ((UIElement) element).ManipulationCompleted += (new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted));
        ((PresentationFrameworkCollection<UIElement>) ((Panel) this.LayoutRoot).Children).Add((UIElement) element);
      }
    }

    private void Element_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
    }

    private void Element_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
      if (e.PinchManipulation != null)
        return;
      System.Windows.Point translation = e.DeltaManipulation.Translation;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      this.HandleDragDelta(translation.X, translation.Y);
    }

    private void Element_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
      System.Windows.Point linearVelocity = e.FinalVelocities.LinearVelocity;
      // ISSUE: explicit reference operation
      this.HandleDragCompleted(linearVelocity.X);
    }

    private void HandleDragDelta(double hDelta, double vDelta)
    {
      if (this._isAnimating)
        return;
      TranslateTransform renderTransform1 = ((UIElement) this.CurrentElement).RenderTransform as TranslateTransform;
      if (this._isInVerticalSwipe)
        return;
      if (this._selectedIndex == 0 && hDelta > 0.0 && renderTransform1.X > 0.0 || this._selectedIndex == this._items.Count - 1 && hDelta < 0.0 && renderTransform1.X < 0.0)
        hDelta /= 3.0;
      foreach (UIElement element in this._elements)
      {
        TranslateTransform renderTransform2 = element.RenderTransform as TranslateTransform;
        double num = renderTransform2.X + hDelta;
        renderTransform2.X = num;
      }
    }

    private void HandleDragCompleted(double hVelocity)
    {
      if (this._isAnimating)
        return;
      double num1 = hVelocity;
      bool? moveNext = new bool?();
      double x = (((UIElement) this.CurrentElement).RenderTransform as TranslateTransform).X;
      double num2 = num1;
      if ((num2 < -100.0 && x < 0.0 || x <= -base.Width / 2.0) && this._selectedIndex < this._items.Count - 1)
        moveNext = new bool?(true);
      else if ((num2 > 100.0 && x > 0.0 || x >= base.Width / 2.0) && this._selectedIndex > 0)
        moveNext = new bool?(false);
      bool flag1 = this.SelectedIndex <= 1;
      bool flag2 = this.SelectedIndex >= this.Items.Count - 2;
      bool? nullable1 = moveNext;
      bool flag3 = true;
      double num3;
      if ((nullable1.GetValueOrDefault() == flag3 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
      {
        num3 = !flag2 ? -this.ArrangeWidth : -this.ArrangeWidth + this.NextElementMargin;
      }
      else
      {
        bool? nullable2 = moveNext;
        bool flag4 = false;
        num3 = (nullable2.GetValueOrDefault() == flag4 ? (nullable2.HasValue ? 1 : 0) : 0) == 0 ? (this.SelectedIndex <= this.Items.Count - 2 ? (this.SelectedIndex >= 1 ? 0.0 : 0.0) : (this.Items.Count <= 1 ? 0.0 : this.NextElementMargin)) : (!flag1 ? this.ArrangeWidth : this.ArrangeWidth);
      }
      double delta = num3 - x;
      if (moveNext.HasValue && moveNext.Value)
      {
        this._isAnimating = true;
        (((UIElement) this._elements[1].Header).RenderTransform as TranslateTransform).X = 0.0;
        (((UIElement) this._elements[2].Header).RenderTransform as TranslateTransform).X = 0.0;
        this.AnimateTwoElementsOnDragComplete((FrameworkElement) this._elements[1], (FrameworkElement) this._elements[2], delta, (Action) (() =>
        {
          this.MoveToNextOrPrevious(moveNext.Value);
          this.ArrangeElements();
          this._isAnimating = false;
        }), moveNext.HasValue);
        this.ChangeCurrentInd(moveNext.Value);
      }
      else if (moveNext.HasValue && !moveNext.Value)
      {
        this._isAnimating = true;
        (((UIElement) this._elements[0].Header).RenderTransform as TranslateTransform).X = 0.0;
        (((UIElement) this._elements[1].Header).RenderTransform as TranslateTransform).X=(-this.NextHeaderMargin);
        this.AnimateTwoElementsOnDragComplete((FrameworkElement) this._elements[0], (FrameworkElement) this._elements[1], delta, (Action) (() =>
        {
          this.MoveToNextOrPrevious(moveNext.Value);
          this.ArrangeElements();
          this._isAnimating = false;
        }), moveNext.HasValue);
        this.ChangeCurrentInd(moveNext.Value);
      }
      else
      {
        if (delta == 0.0)
          return;
        this.AnimateElementOnDragComplete((FrameworkElement) this._elements[0], delta,  null, moveNext.HasValue);
        this.AnimateElementOnDragComplete((FrameworkElement) this._elements[1], delta,  null, moveNext.HasValue);
        this.AnimateElementOnDragComplete((FrameworkElement) this._elements[2], delta, new Action(this.ArrangeElements), moveNext.HasValue);
      }
    }

    private void AnimateTwoElementsOnDragComplete(FrameworkElement element1, FrameworkElement element2, double delta, Action completedCallback, bool movingToNextOrPrevious)
    {
      int num = movingToNextOrPrevious ? 200 : 175;
      List<AnimationInfo> animInfoList = new List<AnimationInfo>();
      TranslateTransform renderTransform1 = ((UIElement) element1).RenderTransform as TranslateTransform;
      TranslateTransform renderTransform2 = ((UIElement) element2).RenderTransform as TranslateTransform;
      animInfoList.Add(new AnimationInfo()
      {
        from = renderTransform1.X,
        to = renderTransform1.X + delta,
        propertyPath = TranslateTransform.XProperty,
        duration = num,
        target = (DependencyObject) renderTransform1,
        easing = GamesSlideView.ANIMATION_EASING
      });
      animInfoList.Add(new AnimationInfo()
      {
        from = renderTransform2.X,
        to = renderTransform2.X + delta,
        propertyPath = TranslateTransform.XProperty,
        duration = num,
        target = (DependencyObject) renderTransform2,
        easing = GamesSlideView.ANIMATION_EASING
      });
      int? startTime = new int?(0);
      Action completed = completedCallback;
      AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
    }

    private void AnimateElementOnDragComplete(FrameworkElement element, double delta, Action completedCallback, bool movingToNextOrPrevious)
    {
      int duration = movingToNextOrPrevious ? 200 : 175;
      TranslateTransform renderTransform = ((UIElement) element).RenderTransform as TranslateTransform;
      ((DependencyObject) renderTransform).Animate(renderTransform.X, renderTransform.X + delta, TranslateTransform.XProperty, duration, new int?(0), GamesSlideView.ANIMATION_EASING, completedCallback);
    }

    private void AnimateElementVerticalOnDragComplete(FrameworkElement element, double to)
    {
      int duration = 200;
      TranslateTransform renderTransform = ((UIElement) element).RenderTransform as TranslateTransform;
      ((DependencyObject) renderTransform).Animate(renderTransform.Y, to, TranslateTransform.YProperty, duration, new int?(0), GamesSlideView.ANIMATION_EASING,  null);
    }

    private void MoveToNextOrPrevious(bool next)
    {
      if (next)
      {
        GamesSlideView.Swap(this._elements, 0, 1);
        GamesSlideView.Swap(this._elements, 1, 2);
      }
      else
      {
        GamesSlideView.Swap(this._elements, 1, 2);
        GamesSlideView.Swap(this._elements, 0, 1);
      }
      this.UpdateSources(false, new bool?(next));
    }

    private void ChangeCurrentInd(bool next)
    {
      this._selectedIndex = !next ? this._selectedIndex - 1 : this._selectedIndex + 1;
      // ISSUE: reference to a compiler-generated field
      if (this.SelectionChanged != null)
      {
        // ISSUE: reference to a compiler-generated field
        this.SelectionChanged(this, this._selectedIndex);
      }
      this.OnPropertyChanged("SelectedIndex");
    }

    private static void Swap(List<GameView> elements, int ind1, int ind2)
    {
      GameView element = elements[ind1];
      elements[ind1] = elements[ind2];
      elements[ind2] = element;
    }

    public void EnableSwipe()
    {
      if (this._isSwipeEnabled)
        return;
      this.DisableSwipe();
      foreach (GameView element in this._elements)
      {
        EventHandler<ManipulationStartedEventArgs> eventHandler1 = new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted);
        ((UIElement) element).ManipulationStarted += (eventHandler1);
        EventHandler<ManipulationDeltaEventArgs> eventHandler2 = new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta);
        ((UIElement) element).ManipulationDelta += (eventHandler2);
        EventHandler<ManipulationCompletedEventArgs> eventHandler3 = new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted);
        ((UIElement) element).ManipulationCompleted += (eventHandler3);
      }
      this._isSwipeEnabled = true;
    }

    public void DisableSwipe()
    {
      if (!this._isSwipeEnabled)
        return;
      foreach (GameView element in this._elements)
      {
        EventHandler<ManipulationStartedEventArgs> eventHandler1 = new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted);
        ((UIElement) element).ManipulationStarted-=(eventHandler1);
        EventHandler<ManipulationDeltaEventArgs> eventHandler2 = new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta);
        ((UIElement) element).ManipulationDelta-=(eventHandler2);
        EventHandler<ManipulationCompletedEventArgs> eventHandler3 = new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted);
        ((UIElement) element).ManipulationCompleted-=(eventHandler3);
      }
      this._isSwipeEnabled = false;
    }

    public void PushScrollPosition(double sp)
    {
      if (!this.IsScrollListeningEnabled)
        return;
      if (sp > 0.0)
        this.DisableSwipe();
      else
        this.EnableSwipe();
    }

    private double CalculateOpacityFadeAwayBasedOnScroll(double sp)
    {
      return sp >= 10.0 ? (sp <= 60.0 ? 1.0 - (0.025 * sp - 0.25) : 0.0) : 1.0;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      // ISSUE: reference to a compiler-generated field
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Handle(GameDisconnectedEvent data)
    {
      foreach (object obj in (Collection<object>) this.Items)
      {
        GameHeader gameHeader = obj as GameHeader;
        if (gameHeader != null && gameHeader.Game.id == data.GameId)
        {
          this.Items.Remove(obj);
          break;
        }
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new System.Uri("/VKClient.Common;component/UC/GamesSlideView.xaml", UriKind.Relative));
      this.gridRoot = (Grid) base.FindName("gridRoot");
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
    }
  }
}
