using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class SearchParamsViewModel : ViewModelBase
  {
    private readonly ISupportSearchParams _parametersProvider;
    private SearchParams _searchParams;

    public SearchParams SearchParams
    {
      get
      {
        return this._searchParams;
      }
      set
      {
        this._searchParams = value;
        this.NotifyUIProperties();
      }
    }

    public string ParamsStr
    {
      get
      {
        return this._parametersProvider.ParametersSummaryStr;
      }
    }

    public bool IsAnySet
    {
      get
      {
        return this._searchParams.IsAnySet;
      }
    }

    public Visibility AnySetVisibility
    {
      get
      {
        if (!this._searchParams.IsAnySet)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public Visibility SetParamsVisibility
    {
      get
      {
        if (!this._searchParams.IsAnySet)
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public SearchParamsViewModel(ISupportSearchParams parametersProvider)
    {
      this._parametersProvider = parametersProvider;
      this._searchParams = new SearchParams();
    }

    public void NavigateToParametersPage()
    {
      this._parametersProvider.OpenParametersPage();
    }

    public void Clear()
    {
      this._parametersProvider.ClearParameters();
      this.NotifyUIProperties();
    }

    private void NotifyUIProperties()
    {
      // ISSUE: method reference
      this.NotifyPropertyChanged<SearchParams>(() => this.SearchParams);
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.ParamsStr);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.AnySetVisibility);
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.SetParamsVisibility);
    }
  }
}
