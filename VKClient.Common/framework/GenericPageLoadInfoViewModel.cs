using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Common.CommonExtensions;

namespace VKClient.Common.Framework
{
  public class GenericPageLoadInfoViewModel : ViewModelBase
  {
    private PageLoadingState _loadingState;
    private string _error;

    public PageLoadingState LoadingState
    {
      get
      {
        return this._loadingState;
      }
      set
      {
        if (this._loadingState == value)
          return;
        this._loadingState = value;
        this.NotifyPropertyChanged("LoadingState");
        base.NotifyPropertyChanged<Visibility>(() => this.LoadingStatesVisibility);
        base.NotifyPropertyChanged<Visibility>(() => this.LoadingStateVisibility);
        base.NotifyPropertyChanged<Visibility>(() => this.ErrorStateVisibility);
        Action stateChangedCallback = this.LoadingStateChangedCallback;
        if (stateChangedCallback == null)
          return;
        stateChangedCallback();
      }
    }

    public bool IsLoadingState
    {
      get
      {
        return this.LoadingState == PageLoadingState.Loading;
      }
    }

    public Visibility LoadingStatesVisibility
    {
      get
      {
        return (this.LoadingState != PageLoadingState.Loaded).ToVisiblity();
      }
    }

    public Visibility LoadingStateVisibility
    {
      get
      {
        return this.IsLoadingState.ToVisiblity();
      }
    }

    public Visibility ErrorStateVisibility
    {
      get
      {
        return (this.LoadingState == PageLoadingState.LoadingFailed).ToVisiblity();
      }
    }

    public Action LoadingStateChangedCallback { get; set; }

    public string Error
    {
      get
      {
        return this._error;
      }
      set
      {
        this._error = value;
        this.NotifyPropertyChanged("Error");
      }
    }
  }
}
