using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Common.UC
{
    public class CustomListPicker : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CustomListPicker), new PropertyMetadata((object)"", new PropertyChangedCallback(CustomListPicker.TitlePropertyChangedCallback)));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(List<CustomListPickerItem>), typeof(CustomListPicker), new PropertyMetadata(null, new PropertyChangedCallback(CustomListPicker.ItemsSourcePropertyChangedCallback)));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(CustomListPickerItem), typeof(CustomListPicker), new PropertyMetadata(null, new PropertyChangedCallback(CustomListPicker.SelectedItemPropertyChangedCallback)));
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(string), typeof(CustomListPicker), new PropertyMetadata((object)"", new PropertyChangedCallback(CustomListPicker.PlaceholderPropertyChangedCallback)));
        public static readonly DependencyProperty SelectionTitleProperty = DependencyProperty.Register("SelectionTitle", typeof(string), typeof(CustomListPicker), new PropertyMetadata((object)""));
        public static readonly DependencyProperty SelectedItemPlaceholderProperty = DependencyProperty.Register("SelectedItemPlaceholder", typeof(string), typeof(CustomListPicker), new PropertyMetadata(null, new PropertyChangedCallback(CustomListPicker.SelectedItemPlaceholderPropertyChangedCallback)));
        public static readonly DependencyProperty IsArrowVisibleProperty = DependencyProperty.Register("IsArrowVisible", typeof(bool), typeof(CustomListPicker), new PropertyMetadata((object)false, new PropertyChangedCallback(CustomListPicker.IsArrowVisiblePropertyChangedCallback)));
        public static readonly DependencyProperty IsPopupSelectionProperty = DependencyProperty.Register("IsPopupSelection", typeof(bool), typeof(CustomListPicker), new PropertyMetadata((object)false));
        public static readonly DependencyProperty PopupSelectionWidthProperty = DependencyProperty.Register("PopupSelectionWidth", typeof(double), typeof(CustomListPicker), new PropertyMetadata((object)320.0));
        internal TextBlock TitleBlock;
        internal TextBlock SelectedItemTitleBlock;
        internal Border Arrow;
        private bool _contentLoaded;

        public string Title
        {
            get
            {
                return (string)base.GetValue(CustomListPicker.TitleProperty);
            }
            set
            {
                base.SetValue(CustomListPicker.TitleProperty, value);
            }
        }

        public List<CustomListPickerItem> ItemsSource
        {
            get
            {
                return (List<CustomListPickerItem>)base.GetValue(CustomListPicker.ItemsSourceProperty);
            }
            set
            {
                base.SetValue(CustomListPicker.ItemsSourceProperty, value);
            }
        }

        public CustomListPickerItem SelectedItem
        {
            get
            {
                return (CustomListPickerItem)base.GetValue(CustomListPicker.SelectedItemProperty);
            }
            set
            {
                base.SetValue(CustomListPicker.SelectedItemProperty, value);
            }
        }

        public string Placeholder
        {
            get
            {
                return (string)base.GetValue(CustomListPicker.PlaceholderProperty);
            }
            set
            {
                base.SetValue(CustomListPicker.PlaceholderProperty, value);
            }
        }

        public string SelectionTitle
        {
            get
            {
                return (string)base.GetValue(CustomListPicker.SelectionTitleProperty);
            }
            set
            {
                base.SetValue(CustomListPicker.SelectionTitleProperty, value);
            }
        }

        public string SelectedItemPlaceholder
        {
            get
            {
                return (string)base.GetValue(CustomListPicker.SelectedItemPlaceholderProperty);
            }
            set
            {
                base.SetValue(CustomListPicker.SelectedItemPlaceholderProperty, value);
            }
        }

        public bool IsArrowVisible
        {
            get
            {
                return (bool)base.GetValue(CustomListPicker.IsArrowVisibleProperty);
            }
            set
            {
                base.SetValue(CustomListPicker.IsArrowVisibleProperty, value);
            }
        }

        public bool IsPopupSelection
        {
            get
            {
                return (bool)base.GetValue(CustomListPicker.IsPopupSelectionProperty);
            }
            set
            {
                base.SetValue(CustomListPicker.IsPopupSelectionProperty, value);
            }
        }

        public double PopupSelectionWidth
        {
            get
            {
                return (double)base.GetValue(CustomListPicker.PopupSelectionWidthProperty);
            }
            set
            {
                base.SetValue(CustomListPicker.PopupSelectionWidthProperty, value);
            }
        }

        public event EventHandler<System.Windows.Input.GestureEventArgs> Click;

        public CustomListPicker()
        {
            //base.\u002Ector();
            this.InitializeComponent();
        }

        private void OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!this.IsPopupSelection)
            {
                // ISSUE: reference to a compiler-generated field
                EventHandler<System.Windows.Input.GestureEventArgs> click = this.Click;
                if (click != null)
                {
                    object sender1 = sender;
                    GestureEventArgs e1 = e;
                    click(sender1, e1);
                }
                if (e.Handled)
                    return;
                Navigator.Current.NavigateToCustomListPickerSelection(this);
            }
            else
            {
                FrameworkElement content = (FrameworkElement)((ContentControl)Application.Current.RootVisual).Content;
                GeneralTransform visual = base.TransformToVisual((UIElement)content);
                Point point1 = new Point();
                // ISSUE: explicit reference operation
                point1.X = (-16.0);
                // ISSUE: explicit reference operation
                point1.Y = 0.0;
                Point point3 = visual.Transform(point1);
                // ISSUE: explicit reference operation
                if (point3.X + this.PopupSelectionWidth > 480.0)
                {
                    // ISSUE: explicit reference operation
                    point3.X = (480.0 - this.PopupSelectionWidth);
                }
                ListPickerListItem listPickerListItem1 = null;
                ObservableCollection<ListPickerListItem> observableCollection = new ObservableCollection<ListPickerListItem>();
                foreach (CustomListPickerItem customListPickerItem in this.ItemsSource)
                {
                    ListPickerListItem listPickerListItem2 = new ListPickerListItem(customListPickerItem);
                    observableCollection.Add(listPickerListItem2);
                    CustomListPickerItem selectedItem = this.SelectedItem;
                    if (customListPickerItem == selectedItem)
                    {
                        listPickerListItem2.IsSelected = true;
                        listPickerListItem1 = listPickerListItem2;
                    }
                }
                ListPickerItemsUC listPickerItemsUc = new ListPickerItemsUC();
                listPickerItemsUc.ItemsSource = observableCollection;
                listPickerItemsUc.SelectedItem = listPickerListItem1;
                listPickerItemsUc.PickerMaxHeight = 480.0;
                double popupSelectionWidth = this.PopupSelectionWidth;
                listPickerItemsUc.PickerWidth = popupSelectionWidth;
                FrameworkElement frameworkElement = content;
                listPickerItemsUc.ParentElement = frameworkElement;
                Point point4 = point3;
                listPickerItemsUc.ShowPosition = point4;
                // ISSUE: variable of the null type

                listPickerItemsUc.ItemTemplate = null;
                ListPickerItemsUC picker = listPickerItemsUc;
                Grid grid1 = new Grid();
                int num1 = 0;
                grid1.VerticalAlignment = ((VerticalAlignment)num1);
                int num2 = 0;
                grid1.HorizontalAlignment = ((HorizontalAlignment)num2);
                Grid grid2 = grid1;
                ((PresentationFrameworkCollection<UIElement>)((Panel)grid2).Children).Add((UIElement)picker);
                DialogService dialogService1 = new DialogService();
                dialogService1.AnimationType = DialogService.AnimationTypes.None;
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
                dialogService1.BackgroundBrush = (Brush)solidColorBrush;
                Grid grid3 = grid2;
                dialogService1.Child = (FrameworkElement)grid3;
                DialogService dialogService = dialogService1;
                ((UIElement)picker.listBox).Tap += ((EventHandler<System.Windows.Input.GestureEventArgs>)((o, args) =>
                {
                    ListPickerListItem selectedItem = picker.listBox.SelectedItem as ListPickerListItem;
                    if (selectedItem != null)
                        this.SelectedItem = this.ItemsSource[picker.ItemsSource.IndexOf(selectedItem)];
                    ((Timeline)picker.AnimClipHide).Completed += ((EventHandler)((s, a) => dialogService.Hide()));
                    picker.AnimClipHide.Begin();
                }));
                dialogService.Opened += (EventHandler)((o, args) => picker.AnimClip.Begin());
                picker.Setup();
                dialogService.Show(null);
            }
        }

        private static void TitlePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((CustomListPicker)sender).TitleBlock.Text = (string)e.NewValue;
        }

        private static void ItemsSourcePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomListPicker customListPicker = (CustomListPicker)sender;
            // ISSUE: explicit reference operation
            List<CustomListPickerItem> newValue = (List<CustomListPickerItem>)e.NewValue;
            if (newValue != null)
                customListPicker.SelectedItem = Enumerable.Any<CustomListPickerItem>(newValue) ? Enumerable.First<CustomListPickerItem>(newValue) : null;
            customListPicker.SelectedItem = null;
        }

        private static void SelectedItemPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomListPicker customListPicker = (CustomListPicker)sender;
            // ISSUE: explicit reference operation
            CustomListPickerItem newValue = (CustomListPickerItem)e.NewValue;
            if (newValue != null && !newValue.IsUnknown)
            {
                customListPicker.SelectedItemTitleBlock.Text = newValue.Name;
                customListPicker.SelectedItemTitleBlock.Foreground = ((Brush)Application.Current.Resources["PhoneContrastTitleBrush"]);
            }
            else
            {
                customListPicker.SelectedItemTitleBlock.Text = customListPicker.Placeholder;
                customListPicker.SelectedItemTitleBlock.Foreground = ((Brush)Application.Current.Resources["PhoneCommunityManagementSectionIconBrush"]);
            }
        }

        private static void PlaceholderPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomListPicker customListPicker = (CustomListPicker)sender;
            // ISSUE: explicit reference operation
            string newValue = (string)e.NewValue;
            if (customListPicker.SelectedItem != null && !customListPicker.SelectedItem.IsUnknown)
                return;
            customListPicker.SelectedItemTitleBlock.Text = newValue;
            customListPicker.SelectedItemTitleBlock.Foreground = ((Brush)Application.Current.Resources["PhoneCommunityManagementSectionIconBrush"]);
        }

        private static void SelectedItemPlaceholderPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomListPicker customListPicker1 = (CustomListPicker)sender;
            // ISSUE: explicit reference operation
            string newValue = (string)e.NewValue;
            CustomListPicker customListPicker2 = customListPicker1;
            CustomListPickerItem customListPickerItem = new CustomListPickerItem();
            customListPickerItem.Name = newValue;
            int num = customListPicker1.Placeholder == newValue ? 1 : (newValue == null ? 1 : 0);
            customListPickerItem.IsUnknown = num != 0;
            customListPicker2.SelectedItem = customListPickerItem;
        }

        private static void IsArrowVisiblePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // ISSUE: explicit reference operation
            ((UIElement)((CustomListPicker)sender).Arrow).Visibility = (((bool)e.NewValue).ToVisiblity());
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/CustomListPicker/CustomListPicker.xaml", UriKind.Relative));
            this.TitleBlock = (TextBlock)base.FindName("TitleBlock");
            this.SelectedItemTitleBlock = (TextBlock)base.FindName("SelectedItemTitleBlock");
            this.Arrow = (Border)base.FindName("Arrow");
        }
    }
}
