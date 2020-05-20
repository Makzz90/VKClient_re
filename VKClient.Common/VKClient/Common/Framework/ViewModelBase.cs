using Microsoft.Phone.Controls;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using VKClient.Common.Framework.CodeForFun;

namespace VKClient.Common.Framework
{
  public class ViewModelBase : INotifyPropertyChanged
  {
    private Visibility _genericErrorVisibility = Visibility.Collapsed;
    private string _isInProgressText = "";
    private bool _isInProgress;

    public Visibility GenericErrorVisibility
    {
      get
      {
        return this._genericErrorVisibility;
      }
      set
      {
        this._genericErrorVisibility = value;
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.GenericErrorVisibility));
      }
    }

    public string InProgressText
    {
      get
      {
        return this._isInProgressText;
      }
      private set
      {
        this._isInProgressText = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.InProgressText));
      }
    }

    public bool IsInProgress
    {
      get
      {
        return this._isInProgress;
      }
      private set
      {
        this._isInProgress = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsInProgress));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsInProgressVisibility));
      }
    }

    public Visibility IsInProgressVisibility
    {
      get
      {
        return !this.IsInProgress ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    private ViewModelBase MainVM
    {
      get
      {
        if (this.Page != null)
          return this.Page.DataContext as ViewModelBase;
        return (ViewModelBase) null;
      }
    }

    private PhoneApplicationPage Page
    {
      get
      {
        try
        {
          return this.RootVisual.GetFirstLogicalChildByType<PhoneApplicationPage>(false);
        }
        catch
        {
        }
        return null;
      }
    }

    private Frame RootVisual
    {
      get
      {
        return Application.Current.RootVisual as Frame;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged(string property)
    {
      if (this.PropertyChanged == null)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.PropertyChanged == null)
          return;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(property));
      }));
    }

    protected void NotifyPropertyChanged<T>(System.Linq.Expressions.Expression<Func<T>> propertyExpression)
    {
      if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
        return;
      this.RaisePropertyChanged((propertyExpression.Body as MemberExpression).Member.Name);
    }

    protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
      this.RaisePropertyChanged(propertyName);
    }

    public void SetInProgress(bool isInProgress, string inProgressText = "")
    {
      this.IsInProgress = isInProgress;
      this.InProgressText = this.IsInProgress ? inProgressText : "";
    }

    public void SetInProgressMain(bool isInProgress, string message = "")
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.MainVM == null)
          return;
        this.MainVM.SetInProgress(isInProgress, message);
      }));
    }
  }
}
