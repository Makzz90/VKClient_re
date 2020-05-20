using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public sealed class SelectionPageViewModel : ViewModelBase
  {
    private readonly CustomListPicker _parentPicker;

    public string SelectionTitle { get; private set; }

    public ObservableCollection<SelectionPageItem> Items { get; private set; }

    public SelectionPageViewModel(CustomListPicker parentPicker)
    {
      this._parentPicker = parentPicker;
      this.SelectionTitle = parentPicker.SelectionTitle;
      this.Items = new ObservableCollection<SelectionPageItem>((IEnumerable<SelectionPageItem>)Enumerable.Select<CustomListPickerItem, SelectionPageItem>(parentPicker.ItemsSource, (Func<CustomListPickerItem, SelectionPageItem>)(i =>
      {
        string str = i != parentPicker.SelectedItem ? "PhoneContrastTitleBrush" : "PhoneBlue300Brush";
        return new SelectionPageItem()
        {
          Title = i.Name,
          Source = i,
          Foreground = (SolidColorBrush) Application.Current.Resources[str]
        };
      })));
    }

    public void UpdateSelectedItem(SelectionPageItem item)
    {
      this._parentPicker.SelectedItem = item.Source;
      Navigator.Current.GoBack();
    }
  }
}
