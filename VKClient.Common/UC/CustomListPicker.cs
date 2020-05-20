using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
                return (string)this.GetValue(CustomListPicker.TitleProperty);
            }
            set
            {
                this.SetValue(CustomListPicker.TitleProperty, (object)value);
            }
        }

        public List<CustomListPickerItem> ItemsSource
        {
            get
            {
                return (List<CustomListPickerItem>)this.GetValue(CustomListPicker.ItemsSourceProperty);
            }
            set
            {
                this.SetValue(CustomListPicker.ItemsSourceProperty, (object)value);
            }
        }

        public CustomListPickerItem SelectedItem
        {
            get
            {
                return (CustomListPickerItem)this.GetValue(CustomListPicker.SelectedItemProperty);
            }
            set
            {
                this.SetValue(CustomListPicker.SelectedItemProperty, (object)value);
            }
        }

        public string Placeholder
        {
            get
            {
                return (string)this.GetValue(CustomListPicker.PlaceholderProperty);
            }
            set
            {
                this.SetValue(CustomListPicker.PlaceholderProperty, (object)value);
            }
        }

        public string SelectionTitle
        {
            get
            {
                return (string)this.GetValue(CustomListPicker.SelectionTitleProperty);
            }
            set
            {
                this.SetValue(CustomListPicker.SelectionTitleProperty, (object)value);
            }
        }

        public string SelectedItemPlaceholder
        {
            get
            {
                return (string)this.GetValue(CustomListPicker.SelectedItemPlaceholderProperty);
            }
            set
            {
                this.SetValue(CustomListPicker.SelectedItemPlaceholderProperty, (object)value);
            }
        }

        public bool IsArrowVisible
        {
            get
            {
                return (bool)this.GetValue(CustomListPicker.IsArrowVisibleProperty);
            }
            set
            {
                this.SetValue(CustomListPicker.IsArrowVisibleProperty, (object)value);
            }
        }

        public bool IsPopupSelection
        {
            get
            {
                return (bool)this.GetValue(CustomListPicker.IsPopupSelectionProperty);
            }
            set
            {
                this.SetValue(CustomListPicker.IsPopupSelectionProperty, (object)value);
            }
        }

        public double PopupSelectionWidth
        {
            get
            {
                return (double)this.GetValue(CustomListPicker.PopupSelectionWidthProperty);
            }
            set
            {
                this.SetValue(CustomListPicker.PopupSelectionWidthProperty, (object)value);
            }
        }

        public event EventHandler<GestureEventArgs> Click;

        public CustomListPicker()
        {
            this.InitializeComponent();
        }

        private void OnClicked(object sender, GestureEventArgs e)
        {
            if (!this.IsPopupSelection)
            {
                EventHandler<GestureEventArgs> eventHandler = this.Click;
                if (eventHandler != null)
                {
                    object sender1 = sender;
                    GestureEventArgs e1 = e;
                    eventHandler(sender1, e1);
                }
                if (e.Handled)
                    return;
                Navigator.Current.NavigateToCustomListPickerSelection(this);
            }
            else
            {
                FrameworkElement frameworkElement1 = (FrameworkElement)((ContentControl)Application.Current.RootVisual).Content;
                Point point1 = this.TransformToVisual((UIElement)frameworkElement1).Transform(new Point()
                {
                    X = -16.0,
                    Y = 0.0
                });
                if (point1.X + this.PopupSelectionWidth > 480.0)
                    point1.X = 480.0 - this.PopupSelectionWidth;
                ListPickerListItem listPickerListItem1 = (ListPickerListItem)null;
                ObservableCollection<ListPickerListItem> observableCollection = new ObservableCollection<ListPickerListItem>();
                foreach (CustomListPickerItem customListPickerItem in this.ItemsSource)
                {
                    ListPickerListItem listPickerListItem2 = new ListPickerListItem((object)customListPickerItem);
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
                FrameworkElement frameworkElement2 = frameworkElement1;
                listPickerItemsUc.ParentElement = frameworkElement2;
                Point point2 = point1;
                listPickerItemsUc.ShowPosition = point2;
                listPickerItemsUc.ItemTemplate = null;
                ListPickerItemsUC picker = listPickerItemsUc;
                Grid grid1 = new Grid();
                int num1 = 0;
                grid1.VerticalAlignment = (VerticalAlignment)num1;
                int num2 = 0;
                grid1.HorizontalAlignment = (HorizontalAlignment)num2;
                Grid grid2 = grid1;
                grid2.Children.Add((UIElement)picker);
                DialogService dialogService1 = new DialogService();
                dialogService1.AnimationType = DialogService.AnimationTypes.None;
                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
                dialogService1.BackgroundBrush = (Brush)solidColorBrush;
                Grid grid3 = grid2;
                dialogService1.Child = (FrameworkElement)grid3;
                DialogService dialogService = dialogService1;
                picker.listBox.Tap += (EventHandler<GestureEventArgs>)((o, args) =>
                {
                    ListPickerListItem listPickerListItem = picker.listBox.SelectedItem as ListPickerListItem;
                    if (listPickerListItem != null)
                        this.SelectedItem = this.ItemsSource[picker.ItemsSource.IndexOf(listPickerListItem)];
                    picker.AnimClipHide.Completed += (EventHandler)((s, a) => dialogService.Hide());
                    picker.AnimClipHide.Begin();
                });
                dialogService.Opened += (EventHandler)((o, args) => picker.AnimClip.Begin());
                picker.Setup();
                dialogService.Show((UIElement)null);
            }
        }

        private static void TitlePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((CustomListPicker)sender).TitleBlock.Text = (string)e.NewValue;
        }

        private static void ItemsSourcePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomListPicker customListPicker = (CustomListPicker)sender;
            List<CustomListPickerItem> source = (List<CustomListPickerItem>)e.NewValue;
            if (source != null)
                customListPicker.SelectedItem = source.Any<CustomListPickerItem>() ? source.First<CustomListPickerItem>() : (CustomListPickerItem)null;
            customListPicker.SelectedItem = (CustomListPickerItem)null;
        }

        private static void SelectedItemPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomListPicker customListPicker = (CustomListPicker)sender;
            CustomListPickerItem customListPickerItem = (CustomListPickerItem)e.NewValue;
            if (customListPickerItem != null && !customListPickerItem.IsUnknown)
            {
                customListPicker.SelectedItemTitleBlock.Text = customListPickerItem.Name;
                customListPicker.SelectedItemTitleBlock.Foreground = (Brush)Application.Current.Resources["PhoneContrastTitleBrush"];
            }
            else
            {
                customListPicker.SelectedItemTitleBlock.Text = customListPicker.Placeholder;
                customListPicker.SelectedItemTitleBlock.Foreground = (Brush)Application.Current.Resources["PhoneCommunityManagementSectionIconBrush"];
            }
        }

        private static void PlaceholderPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomListPicker customListPicker = (CustomListPicker)sender;
            string str = (string)e.NewValue;
            if (customListPicker.SelectedItem != null && !customListPicker.SelectedItem.IsUnknown)
                return;
            customListPicker.SelectedItemTitleBlock.Text = str;
            customListPicker.SelectedItemTitleBlock.Foreground = (Brush)Application.Current.Resources["PhoneCommunityManagementSectionIconBrush"];
        }

        private static void SelectedItemPlaceholderPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            CustomListPicker customListPicker1 = (CustomListPicker)sender;
            string str = (string)e.NewValue;
            CustomListPicker customListPicker2 = customListPicker1;
            CustomListPickerItem customListPickerItem = new CustomListPickerItem();
            customListPickerItem.Name = str;
            int num = customListPicker1.Placeholder == str ? 1 : (str == null ? 1 : 0);
            customListPickerItem.IsUnknown = num != 0;
            customListPicker2.SelectedItem = customListPickerItem;
        }

        private static void IsArrowVisiblePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((CustomListPicker)sender).Arrow.Visibility = ((bool)e.NewValue).ToVisiblity();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/CustomListPicker/CustomListPicker.xaml", UriKind.Relative));
            this.TitleBlock = (TextBlock)this.FindName("TitleBlock");
            this.SelectedItemTitleBlock = (TextBlock)this.FindName("SelectedItemTitleBlock");
            this.Arrow = (Border)this.FindName("Arrow");
        }
    }
}
