using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class CountryPickerUC : UserControl
  {
    private CountryPickerViewModel _viewModel;
    internal TextBox textBoxSearch;
    internal TextBlock textBlockWatermarkText;
    internal ExtendedLongListSelector countriesList;
    private bool _contentLoaded;

    private Country _selectedCountry { get; set; }

    private bool _allowNoneSelection { get; set; }

    private event EventHandler<Country> CountryPicked;

    public CountryPickerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler(this.OnLoaded));
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
      if (this._viewModel != null)
        return;
      this._viewModel = new CountryPickerViewModel(this._selectedCountry, this._allowNoneSelection);
      this._viewModel.LoadCountries();
      base.DataContext = this._viewModel;
    }

    private void TextBoxSearch_OnKeyUp(object sender, KeyEventArgs e)
    {
      List<CountryListItem> countries = this._viewModel.Countries;
      if (e.Key != Key.Enter || countries == null || countries.Count <= 0)
        return;
      ((Control) this.countriesList).Focus();
      if (countries.Count != 1)
        return;
      this.PickItem(this._viewModel.Countries[0]);
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

    private void CountriesList_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      ((Control) this.countriesList).Focus();
    }

    private void CountriesList_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      CountryListItem selectedItem = this.countriesList.SelectedItem as CountryListItem;
      this._viewModel.SelectItem(selectedItem);
      this.PickItem(selectedItem);
    }

    private void PickItem(CountryListItem item)
    {
      // ISSUE: reference to a compiler-generated field
      if (item == null || this.CountryPicked == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.CountryPicked(this, item.Country);
    }

    public static void Show(Country selectedCountry = null, bool allowNoneSelection = true, Action<Country> countryPickedCallback = null, Action pickerClosedCallback = null)
    {
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      dialogService.AnimationTypeChild = DialogService.AnimationTypes.SlideInversed;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      DialogService flyout = dialogService;
      CountryPickerUC countryPickerUc = new CountryPickerUC()
      {
        _selectedCountry = selectedCountry,
        _allowNoneSelection = allowNoneSelection
      };
      countryPickerUc.CountryPicked += (EventHandler<Country>) ((sender, country) =>
      {
        if (countryPickedCallback != null)
          countryPickedCallback(country);
        flyout.Hide();
      });
      if (pickerClosedCallback != null)
        flyout.Closed += (EventHandler) ((sender, args) => pickerClosedCallback());
      flyout.Child = (FrameworkElement) countryPickerUc;
      flyout.Show( null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/CountryPickerUC.xaml", UriKind.Relative));
      this.textBoxSearch = (TextBox) base.FindName("textBoxSearch");
      this.textBlockWatermarkText = (TextBlock) base.FindName("textBlockWatermarkText");
      this.countriesList = (ExtendedLongListSelector) base.FindName("countriesList");
    }
  }
}
