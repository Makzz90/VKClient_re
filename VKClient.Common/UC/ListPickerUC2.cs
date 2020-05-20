using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Common.UC
{
  public class ListPickerUC2 : UserControl
  {
    public static readonly DependencyProperty ListHeaderHeightProperty = DependencyProperty.Register("ListHeaderHeight", typeof (double), typeof (ListPickerUC2), new PropertyMetadata(8.0));
    public static readonly DependencyProperty ListFooterHeightProperty = DependencyProperty.Register("ListFooterHeight", typeof (double), typeof (ListPickerUC2), new PropertyMetadata(8.0));
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (IList), typeof (ListPickerUC2), new PropertyMetadata(null));
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof (object), typeof (ListPickerUC2), new PropertyMetadata(null));
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof (DataTemplate), typeof (ListPickerUC2), new PropertyMetadata(null));
    public static readonly DependencyProperty PickerMaxHeightProperty = DependencyProperty.Register("PickerMaxHeight", typeof (double), typeof (ListPickerUC2), new PropertyMetadata(0.0));
    public static readonly DependencyProperty PickerMaxWidthProperty = DependencyProperty.Register("PickerMaxWidth", typeof (double), typeof (ListPickerUC2), new PropertyMetadata(0.0));
    public static readonly DependencyProperty PickerMarginProperty = DependencyProperty.Register("PickerMargin", typeof(Thickness), typeof(ListPickerUC2), new PropertyMetadata(null));
    public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof (Brush), typeof (ListPickerUC2), new PropertyMetadata(null));
    private const double ITEM_DEFAULT_HEIGHT = 64.0;
    private const double LIST_HEADER_DEFAULT_HEIGHT = 8.0;
    private const double LIST_FOOTER_DEFAULT_HEIGHT = 8.0;
    private Point _position;
    private FrameworkElement _container;
    private static int _instancesCount;
    private DialogService _flyout;
    private bool _flyoutOpened;
    internal Storyboard AnimClip;
    internal Storyboard AnimClipHide;
    internal Grid containerGrid;
    internal TranslateTransform transform;
    internal RectangleGeometry rectGeometry;
    internal ScaleTransform scaleTransform;
    internal Grid gridListBoxContainer;
    internal ExtendedLongListSelector listBox;
    internal Canvas listHeader;
    internal Canvas listFooter;
    internal Border borderDisable;
    private bool _contentLoaded;

    public double ListHeaderHeight
    {
      get
      {
        return (double) base.GetValue(ListPickerUC2.ListHeaderHeightProperty);
      }
      set
      {
        base.SetValue(ListPickerUC2.ListHeaderHeightProperty, value);
      }
    }

    public double ListFooterHeight
    {
      get
      {
        return (double) base.GetValue(ListPickerUC2.ListFooterHeightProperty);
      }
      set
      {
        base.SetValue(ListPickerUC2.ListFooterHeightProperty, value);
      }
    }

    public IList ItemsSource
    {
      get
      {
        return (IList) base.GetValue(ListPickerUC2.ItemsSourceProperty);
      }
      set
      {
        base.SetValue(ListPickerUC2.ItemsSourceProperty, value);
      }
    }

    public object SelectedItem
    {
      get
      {
        return base.GetValue(ListPickerUC2.SelectedItemProperty);
      }
      set
      {
        base.SetValue(ListPickerUC2.SelectedItemProperty, value);
      }
    }

    public DataTemplate ItemTemplate
    {
      get
      {
        return (DataTemplate) base.GetValue(ListPickerUC2.ItemTemplateProperty);
      }
      set
      {
        base.SetValue(ListPickerUC2.ItemTemplateProperty, value);
      }
    }

    public double PickerMaxHeight
    {
      get
      {
        return (double) base.GetValue(ListPickerUC2.PickerMaxHeightProperty);
      }
      set
      {
        base.SetValue(ListPickerUC2.PickerMaxHeightProperty, value);
      }
    }

    public double PickerMaxWidth
    {
      get
      {
        return (double) base.GetValue(ListPickerUC2.PickerMaxWidthProperty);
      }
      set
      {
        base.SetValue(ListPickerUC2.PickerMaxWidthProperty, value);
      }
    }

    public Thickness PickerMargin
    {
      get
      {
        return (Thickness) base.GetValue(ListPickerUC2.PickerMarginProperty);
      }
      set
      {
        base.SetValue(ListPickerUC2.PickerMarginProperty, value);
      }
    }

    public Brush BackgroundColor
    {
      get
      {
        return (Brush) base.GetValue(ListPickerUC2.BackgroundColorProperty);
      }
      set
      {
        base.SetValue(ListPickerUC2.BackgroundColorProperty, value);
      }
    }

    public event EventHandler<object> ItemTapped;

    public event EventHandler Closed;

    public ListPickerUC2()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((UIElement) this.listBox).Opacity = 1.0;
      this.scaleTransform.ScaleY = 1.0;
      ++ListPickerUC2._instancesCount;
    }

    //~ListPickerUC2()
    //{
    //  try
    //  {
    //  }
    //  finally
    //  {
    //    // ISSUE: explicit finalizer call
    //    // ISSUE: explicit non-virtual call
    //    this.Finalize();
    //  }
    //}

    public void Show(Point position, FrameworkElement container)
    {
      this._position = position;
      this._container = container;
      ((UIElement) this.listBox).Opacity = 0.0;
      this.scaleTransform.ScaleY = 0.0;
      this.Setup();
      Grid grid1 = new Grid();
      int num1 = 0;
      ((FrameworkElement) grid1).VerticalAlignment = ((VerticalAlignment) num1);
      int num2 = 0;
      ((FrameworkElement) grid1).HorizontalAlignment = ((HorizontalAlignment) num2);
      Grid grid2 = grid1;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) grid2).Children).Add((UIElement) this);
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      Grid grid3 = grid2;
      dialogService.Child = (FrameworkElement) grid3;
      this._flyout = dialogService;
      this._flyout.Opened += (EventHandler) ((sender, args) =>
      {
        this.UpdatePopupSize();
        this._flyoutOpened = true;
        this.AnimClip.Begin();
      });
      this._flyout.Closed += (EventHandler) ((sender, args) =>
      {
        this._flyoutOpened = false;
        this.ItemsSource =  null;
        // ISSUE: reference to a compiler-generated field
        EventHandler closed = this.Closed;
        if (closed == null)
          return;
        EventArgs empty = EventArgs.Empty;
        closed(this, empty);
      });
      this._flyout.Show( null);
    }

    public void Hide()
    {
      ((Timeline) this.AnimClipHide).Completed += ((EventHandler) ((sender, eventArgs) =>
      {
        DialogService flyout = this._flyout;
        if (flyout == null)
          return;
        // ISSUE: explicit non-virtual call
        flyout.Hide();
      }));
      this.AnimClipHide.Begin();
    }

    private void Setup()
    {
      if (this.ItemTemplate != null)
        this.listBox.ItemTemplate = this.ItemTemplate;
      this.listBox.ItemsSource = this.ItemsSource;
      ((FrameworkElement) this.listHeader).Height = this.ListHeaderHeight;
      ((FrameworkElement) this.listFooter).Height = this.ListFooterHeight;
      if (this.BackgroundColor == null)
        return;
      ((Panel) this.containerGrid).Background = this.BackgroundColor;
    }

    private void UpdatePopupSize()
    {
      this.UpdatePopupHeight();
      this.containerGrid.Width = this.PickerMaxWidth;
      this.containerGrid.Margin = new Thickness(this._position.X, this._position.Y, 0.0, 0.0);
      this.scaleTransform.CenterY = this._position.Y;
      this.rectGeometry.Rect = new Rect()
      {
        X = 0.0,
        Y = 0.0,
        Width = this.containerGrid.Width,
        Height = this.containerGrid.Height
      };
    }

    private void UpdatePopupHeight()
    {
      double val1_1 = this._container.ActualHeight - this.PickerMargin.Top - this.PickerMargin.Bottom;
      double val2 = Math.Min(val1_1, this.listBox.ActualHeight);
      double val1_2 = this.PickerMaxHeight > 0.0 ? Math.Min(this.PickerMaxHeight, val2) : val2;
      this.gridListBoxContainer.Height = val1_1;
      this.containerGrid.Height = Math.Max(val1_2, 0.0);
    }

    private void ScrollToSelectedItem(int selectedIndex)
    {
      if (selectedIndex == this.ItemsSource.Count - 1)
      {
        this.listBox.ScrollTo(this.listBox.ItemsSource[selectedIndex]);
      }
      else
      {
        double y = this._position.Y;
        this.listBox.ScrollToPosition(ListPickerUC2.GetItemOffset(selectedIndex) - y);
      }
    }

    private static double GetItemOffset(int itemIndex)
    {
      return 8.0 + 64.0 * (double) itemIndex;
    }

    public void DisableContent()
    {
      ((UIElement) this.borderDisable).Visibility = Visibility.Visible;
    }

    private void ListBox_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      object selectedItem1 = this.listBox.SelectedItem;
      if (selectedItem1 != null)
      {
        this.SelectedItem = this.ItemsSource[this.ItemsSource.IndexOf(selectedItem1)];
        // ISSUE: reference to a compiler-generated field
        EventHandler<object> itemTapped = this.ItemTapped;
        if (itemTapped != null)
        {
          object selectedItem2 = this.SelectedItem;
          itemTapped(this, selectedItem2);
        }
      }
      this.Hide();
    }

    private void ListBox_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (this._flyoutOpened)
        this.UpdatePopupSize();
      int selectedIndex = this.ItemsSource.IndexOf(this.SelectedItem);
      if (selectedIndex <= -1)
        return;
      this.listBox.SelectedItem = (this.ItemsSource[selectedIndex]);
      this.ScrollToSelectedItem(selectedIndex);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ListPickerUC2.xaml", UriKind.Relative));
      this.AnimClip = (Storyboard) base.FindName("AnimClip");
      this.AnimClipHide = (Storyboard) base.FindName("AnimClipHide");
      this.containerGrid = (Grid) base.FindName("containerGrid");
      this.transform = (TranslateTransform) base.FindName("transform");
      this.rectGeometry = (RectangleGeometry) base.FindName("rectGeometry");
      this.scaleTransform = (ScaleTransform) base.FindName("scaleTransform");
      this.gridListBoxContainer = (Grid) base.FindName("gridListBoxContainer");
      this.listBox = (ExtendedLongListSelector) base.FindName("listBox");
      this.listHeader = (Canvas) base.FindName("listHeader");
      this.listFooter = (Canvas) base.FindName("listFooter");
      this.borderDisable = (Border) base.FindName("borderDisable");
    }
  }
}
