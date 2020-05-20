using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class CityPickerUC : UserControl
  {
    private CityPickerViewModel _viewModel;
    private long _countryId;
    internal TextBox textBoxSearch;
    internal TextBlock textBlockWatermarkText;
    internal ExtendedLongListSelector citiesList;
    private bool _contentLoaded;

    private City _selectedCity { get; set; }

    private bool _allowNoneSelection { get; set; }

    private event EventHandler<City> CityPicked;

    public CityPickerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.OnLoaded));
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      if (this._viewModel != null)
        return;
      this._viewModel = new CityPickerViewModel(this._countryId, this._selectedCity, this._allowNoneSelection);
      this._viewModel.SearchVM.LoadData(false, false, false, false,  null,  null, false);
      base.DataContext = this._viewModel;
    }

    private void TextBoxSearch_OnKeyUp(object sender, KeyEventArgs e)
    {
      ObservableCollection<CityListItem> collection = this._viewModel.SearchVM.Collection;
      if (e.Key != Key.Enter || collection == null || collection.Count <= 0)
        return;
      ((Control) this.citiesList).Focus();
      if (collection.Count != 1)
        return;
      this.PickItem(collection[0]);
    }

    private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((UIElement) this.textBlockWatermarkText).Opacity = (string.IsNullOrEmpty(this.textBoxSearch.Text) ? 1.0 : 0.0);
      this._viewModel.Query = this.textBoxSearch.Text;
    }

    private void TextBoxSearch_OnGotFocus(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(this.textBoxSearch.Text))
        return;
      this.textBoxSearch.SelectAll();
    }

    private void CitiesList_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      this._viewModel.SearchVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void CitiesList_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      ((Control) this.citiesList).Focus();
    }

    private void CitiesList_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      CityListItem selectedItem = this.citiesList.SelectedItem as CityListItem;
      this._viewModel.SelectItem(selectedItem);
      this.PickItem(selectedItem);
    }

    private void PickItem(CityListItem item)
    {
      // ISSUE: reference to a compiler-generated field
      if (item == null || this.CityPicked == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.CityPicked(this, item.City);
    }

    public static void Show(long countryId, City selectedCity = null, bool allowNoneSelection = true, Action<City> cityPickedCallback = null, Action pickerClosedCallback = null)
    {
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      dialogService.AnimationTypeChild = DialogService.AnimationTypes.SlideInversed;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      DialogService flyout = dialogService;
      CityPickerUC cityPickerUc = new CityPickerUC()
      {
        _countryId = countryId,
        _selectedCity = selectedCity,
        _allowNoneSelection = allowNoneSelection
      };
      cityPickerUc.CityPicked += (EventHandler<City>) ((sender, city) =>
      {
        if (cityPickedCallback != null)
          cityPickedCallback(city);
        flyout.Hide();
      });
      if (pickerClosedCallback != null)
        flyout.Closed += (EventHandler) ((sender, args) => pickerClosedCallback());
      flyout.Child = (FrameworkElement) cityPickerUc;
      flyout.Show( null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/CityPickerUC.xaml", UriKind.Relative));
      this.textBoxSearch = (TextBox) base.FindName("textBoxSearch");
      this.textBlockWatermarkText = (TextBlock) base.FindName("textBlockWatermarkText");
      this.citiesList = (ExtendedLongListSelector) base.FindName("citiesList");
    }
  }
}
