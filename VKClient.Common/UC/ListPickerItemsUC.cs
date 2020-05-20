using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class ListPickerItemsUC : UserControl
  {
    private const double MarginTop = 40.0;
    private const double MarginBottom = 16.0;
    private const double ElementHeight = 64.0;
    private const double ListMarginTop = 13.0;
    private const double ListMarginBottom = 27.0;
    private double _verticalOffset;
    internal Storyboard AnimClip;
    internal Storyboard AnimClipHide;
    internal Grid containerGrid;
    internal TranslateTransform transform;
    internal RectangleGeometry rectGeometry;
    internal ScaleTransform scaleTransform;
    internal ExtendedLongListSelector listBox;
    private bool _contentLoaded;

    public ObservableCollection<ListPickerListItem> ItemsSource { get; set; }

    public DataTemplate ItemTemplate { get; set; }

    public ListPickerListItem SelectedItem { get; set; }

    public double PickerMaxHeight { get; set; }

    public double PickerWidth { get; set; }

    public Point ShowPosition { get; set; }

    public FrameworkElement ParentElement { get; set; }

    public ListPickerItemsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      ((UIElement) this.listBox).Opacity = 0.0;
      this.scaleTransform.ScaleY = 0.0;
    }

    public void Setup()
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
//      ListPickerItemsUC.<>c__DisplayClass35_0 cDisplayClass350 = new ListPickerItemsUC.<>c__DisplayClass35_0();
      // ISSUE: reference to a compiler-generated field
//      cDisplayClass350.<>4__this = this;
      if (this.ItemTemplate != null)
        this.listBox.ItemTemplate = this.ItemTemplate;
      this.listBox.ItemsSource = ((IList) this.ItemsSource);
      // ISSUE: reference to a compiler-generated field
      int selectedIndex = this.ItemsSource.IndexOf(this.SelectedItem);
      double val2 = 13.0 + (double) this.ItemsSource.Count * 64.0 + 27.0;
      double num = this.PickerMaxHeight > 0.0 ? Math.Min(this.PickerMaxHeight, val2) : val2;
      double pickerWidth = this.PickerWidth;
      Point showPosition1 = this.ShowPosition;
      // ISSUE: explicit reference operation
      this._verticalOffset = ((Point) @showPosition1).Y;
      // ISSUE: reference to a compiler-generated field
      if (selectedIndex > -1)
      {
          this.listBox.SizeChanged+=(delegate(object o, SizeChangedEventArgs eventArgs)
          {
              this.listBox.SelectedItem=(this.ItemsSource[selectedIndex]);
              this.ScrollToSelectedItem(selectedIndex);
          });
          this._verticalOffset -= 64.0;
      }
      if (this._verticalOffset < 40.0)
        this._verticalOffset = 40.0;
      else if (this._verticalOffset + num > this.ParentElement.ActualHeight - 16.0)
        this._verticalOffset = this._verticalOffset - (this._verticalOffset + num - this.ParentElement.ActualHeight + 16.0);
      ((FrameworkElement) this.containerGrid).Height = num;
      ((FrameworkElement) this.containerGrid).Width = pickerWidth;
      Grid containerGrid = this.containerGrid;
      Point showPosition2 = this.ShowPosition;
      // ISSUE: explicit reference operation
      Thickness thickness = new Thickness(((Point) @showPosition2).X, this._verticalOffset, 0.0, 0.0);
      ((FrameworkElement) containerGrid).Margin = thickness;
      Point showPosition3 = this.ShowPosition;
      // ISSUE: explicit reference operation
      this.scaleTransform.CenterY=(((Point) @showPosition3).Y - this._verticalOffset);
      RectangleGeometry rectGeometry = this.rectGeometry;
      Rect rect1 = new Rect();
      // ISSUE: explicit reference operation
      rect1.X = 0.0;
      // ISSUE: explicit reference operation
      rect1.Y = 0.0;
      // ISSUE: explicit reference operation
      rect1.Height = num;
      // ISSUE: explicit reference operation
      rect1.Width = pickerWidth;
      Rect rect2 = rect1;
      rectGeometry.Rect = rect2;
    }

    private void ScrollToSelectedItem(int selectedIndex)
    {
      if (selectedIndex == this.ItemsSource.Count - 1)
      {
        this.listBox.ScrollTo(this.listBox.ItemsSource[selectedIndex]);
      }
      else
      {
        Point showPosition = this.ShowPosition;
        // ISSUE: explicit reference operation
        double num = ((Point) @showPosition).Y - this._verticalOffset;
        this.listBox.ScrollToPosition(ListPickerItemsUC.GetItemOffset(selectedIndex) - num);
      }
    }

    private static double GetItemOffset(int itemIndex)
    {
      return 13.0 + 64.0 * (double) itemIndex;
    }

    private void ListBox_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      foreach (ListPickerListItem listPickerListItem in (Collection<ListPickerListItem>) this.ItemsSource)
        listPickerListItem.IsSelected = false;
      ListPickerListItem selectedItem = this.listBox.SelectedItem as ListPickerListItem;
      if (selectedItem == null)
        return;
      selectedItem.IsSelected = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ListPickerItemsUC.xaml", UriKind.Relative));
      this.AnimClip = (Storyboard) base.FindName("AnimClip");
      this.AnimClipHide = (Storyboard) base.FindName("AnimClipHide");
      this.containerGrid = (Grid) base.FindName("containerGrid");
      this.transform = (TranslateTransform) base.FindName("transform");
      this.rectGeometry = (RectangleGeometry) base.FindName("rectGeometry");
      this.scaleTransform = (ScaleTransform) base.FindName("scaleTransform");
      this.listBox = (ExtendedLongListSelector) base.FindName("listBox");
    }
  }
}
