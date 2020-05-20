using Microsoft.Phone.Controls;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Common.UC
{
  public class ListPickerControl : Control
  {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof (string), typeof (ListPickerControl), new PropertyMetadata( null));
    public static readonly DependencyProperty ParentElementProperty = DependencyProperty.Register("ParentElement", typeof (FrameworkElement), typeof (ListPickerControl), new PropertyMetadata(null));
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (IList), typeof (ListPickerControl), new PropertyMetadata( null));
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(ListPickerControl), new PropertyMetadata(new PropertyChangedCallback(ListPickerControl.SelectedItem_OnChanged)));
    public static readonly DependencyProperty SelectedItemStrProperty = DependencyProperty.Register("SelectedItemStr", typeof (string), typeof (ListPickerControl), new PropertyMetadata("Subtitle"));
    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof (DataTemplate), typeof (ListPickerControl), new PropertyMetadata(null));
    public static readonly DependencyProperty ItemPrefixProperty = DependencyProperty.Register("ItemPrefix", typeof (string), typeof (ListPickerControl), new PropertyMetadata(null));
    public static readonly DependencyProperty PickerMaxHeightProperty = DependencyProperty.Register("PickerMaxHeight", typeof (double), typeof (ListPickerControl), new PropertyMetadata(480.0));
    public static readonly DependencyProperty PickerWidthProperty = DependencyProperty.Register("PickerWidth", typeof (double), typeof (ListPickerControl), new PropertyMetadata(320.0));
    private const double DEFAULT_PICKER_MAX_HEIGHT = 480.0;
    private const double DEFAULT_PICKER_WIDTH = 320.0;
    private const double MAX_PICKER_RIGHT_BORDER_POSITION = 474.0;
    private PhoneApplicationPage _page;
    private Frame _frame;

    public string Title
    {
      get
      {
        return (string) base.GetValue(ListPickerControl.TitleProperty);
      }
      set
      {
        base.SetValue(ListPickerControl.TitleProperty, value);
      }
    }

    public FrameworkElement ParentElement
    {
      get
      {
        return (FrameworkElement) base.GetValue(ListPickerControl.ParentElementProperty);
      }
      set
      {
        base.SetValue(ListPickerControl.ParentElementProperty, value);
      }
    }

    public IList ItemsSource
    {
      get
      {
        return (IList) base.GetValue(ListPickerControl.ItemsSourceProperty);
      }
      set
      {
        base.SetValue(ListPickerControl.ItemsSourceProperty, value);
      }
    }

    public object SelectedItem
    {
      get
      {
        return base.GetValue(ListPickerControl.SelectedItemProperty);
      }
      set
      {
        base.SetValue(ListPickerControl.SelectedItemProperty, value);
      }
    }

    public string SelectedItemStr
    {
      get
      {
        return (string) base.GetValue(ListPickerControl.SelectedItemStrProperty);
      }
      set
      {
        base.SetValue(ListPickerControl.SelectedItemStrProperty, value);
      }
    }

    public DataTemplate ItemTemplate
    {
      get
      {
        return (DataTemplate) base.GetValue(ListPickerControl.ItemTemplateProperty);
      }
      set
      {
        base.SetValue(ListPickerControl.ItemTemplateProperty, value);
      }
    }

    public string ItemPrefix
    {
      get
      {
        return (string) base.GetValue(ListPickerControl.ItemPrefixProperty);
      }
      set
      {
        base.SetValue(ListPickerControl.ItemPrefixProperty, value);
      }
    }

    public double PickerMaxHeight
    {
      get
      {
        return (double) base.GetValue(ListPickerControl.PickerMaxHeightProperty);
      }
      set
      {
        base.SetValue(ListPickerControl.PickerMaxHeightProperty, value);
      }
    }

    public double PickerWidth
    {
      get
      {
        return (double) base.GetValue(ListPickerControl.PickerWidthProperty);
      }
      set
      {
        base.SetValue(ListPickerControl.PickerWidthProperty, value);
      }
    }

    private PhoneApplicationPage Page
    {
      get
      {
        return this._page ?? (this._page = VKClient.Common.Framework.CodeForFun.TemplatedVisualTreeExtensions.GetFirstLogicalChildByType<PhoneApplicationPage>((FrameworkElement) this.Frame, false));
      }
    }

    private Frame Frame
    {
      get
      {
        return this._frame ?? (this._frame = Application.Current.RootVisual as Frame);
      }
    }

    public ListPickerControl()
    {
      //base.\u002Ector();
        ((UIElement)this).Tap += (new EventHandler<System.Windows.Input.GestureEventArgs>(this.Picker_OnTap));
    }

    private static void SelectedItem_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      ((ListPickerControl) d).SelectedItemStr = e.NewValue != null ? e.NewValue.ToString() : "";
    }

    private void Picker_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement1 = this.ParentElement ?? (FrameworkElement) this.Page;
      Point point1 = base.TransformToVisual((UIElement) frameworkElement1).Transform(new Point(0.0, 0.0));
      // ISSUE: explicit reference operation
      // ISSUE: variable of a reference type
      double num1 = point1.X - 16.0;
      point1.X = num1;
      // ISSUE: explicit reference operation
      if (point1.X + this.PickerWidth > 474.0)
      {
        // ISSUE: explicit reference operation
        point1.X=(474.0 - this.PickerWidth);
      }
      Grid grid1 = new Grid();
      int num2 = 0;
      ((FrameworkElement) grid1).VerticalAlignment = ((VerticalAlignment) num2);
      int num3 = 0;
      ((FrameworkElement) grid1).HorizontalAlignment = ((HorizontalAlignment) num3);
      Grid grid2 = grid1;
      ObservableCollection<ListPickerListItem> observableCollection = new ObservableCollection<ListPickerListItem>();
      ListPickerListItem listPickerListItem1 =  null;
      foreach (object fromObj in (IEnumerable) this.ItemsSource)
      {
        ListPickerListItem listPickerListItem2 = new ListPickerListItem(fromObj)
        {
          Prefix = this.ItemPrefix
        };
        observableCollection.Add(listPickerListItem2);
        object selectedItem = this.SelectedItem;
        if (fromObj == selectedItem)
        {
          listPickerListItem2.IsSelected = true;
          listPickerListItem1 = listPickerListItem2;
        }
      }
      ListPickerItemsUC listPickerItemsUc = new ListPickerItemsUC();
      listPickerItemsUc.ItemsSource = observableCollection;
      listPickerItemsUc.SelectedItem = listPickerListItem1;
      DataTemplate itemTemplate = this.ItemTemplate;
      listPickerItemsUc.ItemTemplate = itemTemplate;
      double pickerMaxHeight = this.PickerMaxHeight;
      listPickerItemsUc.PickerMaxHeight = pickerMaxHeight;
      double pickerWidth = this.PickerWidth;
      listPickerItemsUc.PickerWidth = pickerWidth;
      FrameworkElement frameworkElement2 = frameworkElement1;
      listPickerItemsUc.ParentElement = frameworkElement2;
      Point point2 = point1;
      listPickerItemsUc.ShowPosition = point2;
      ListPickerItemsUC picker = listPickerItemsUc;
      ((PresentationFrameworkCollection<UIElement>) ((Panel) grid2).Children).Add((UIElement) picker);
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      Grid grid3 = grid2;
      dialogService.Child = (FrameworkElement) grid3;
      DialogService ds = dialogService;
      ((UIElement) picker.listBox).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>) ((o, args) =>
      {
        ListPickerListItem selectedItem = picker.listBox.SelectedItem as ListPickerListItem;
        if (selectedItem != null)
          this.SelectedItem = this.ItemsSource[picker.ItemsSource.IndexOf(selectedItem)];
        ((Timeline) picker.AnimClipHide).Completed += ((EventHandler) ((sender1, eventArgs) => ds.Hide()));
        picker.AnimClipHide.Begin();
      }));
      picker.Setup();
      ds.Opened += (EventHandler) ((o, args) => picker.AnimClip.Begin());
      ds.Show( null);
    }
  }
}
