using System;
using System.Windows;
using VKClient.Common.CommonExtensions;

namespace VKClient.Common.Framework
{
  public class GenericPageLoadInfoViewModel : ViewModelBase
  {
    private PageLoadingState _loadingState;

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
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.LoadingStatesVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.LoadingStateVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ErrorStateVisibility));
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
  }
}
