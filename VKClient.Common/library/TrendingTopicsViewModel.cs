using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class TrendingTopicsViewModel : ViewModelBase
  {
    private Visibility _trendsVisibility = Visibility.Collapsed;
    private bool _isLoading;
    private List<Trend> _trends;

    public bool IsLoading
    {
      get
      {
        return this._isLoading;
      }
      set
      {
        this._isLoading = value;
        this.NotifyPropertyChanged("IsLoading");
        this.NotifyPropertyChanged<Visibility>(() => this.IsLoadingVisibility);
      }
    }

    public Visibility IsLoadingVisibility
    {
      get
      {
        if (!this._isLoading)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public List<Trend> Trends
    {
      get
      {
        return this._trends;
      }
      set
      {
        this._trends = value;
        this.NotifyPropertyChanged("Trends");
      }
    }

    public Visibility TrendsVisibility
    {
      get
      {
        return this._trendsVisibility;
      }
      set
      {
        this._trendsVisibility = value;
        this.NotifyPropertyChanged("TrendsVisibility");
      }
    }

    public void Load()
    {
        this.IsLoading = true;
        SearchService.Instance.GetTrends(delegate(BackendResult<List<Trend>, ResultCode> result)
        {
            if (result.ResultCode == ResultCode.Succeeded)
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    this.Trends = result.ResultData;
                    if (this.Trends != null && this.Trends.Count > 0)
                    {
                        this.TrendsVisibility = 0;
                    }
                });
            }
            this.IsLoading = false;
        });
    }
  }
}
