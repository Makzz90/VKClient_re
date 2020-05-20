using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace VKClient.Common.UC
{
  public class SearchParamsSummaryUC : UserControl
  {
      public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register("IsSeparatorVisible", typeof(bool), typeof(SearchParamsSummaryUC), new PropertyMetadata(true, new PropertyChangedCallback(SearchParamsSummaryUC.IsSeparatorVisible_OnChanged)));
    internal Rectangle rectSeparator;
    private bool _contentLoaded;

    public bool IsSeparatorVisible
    {
      get
      {
        return (bool) base.GetValue(SearchParamsSummaryUC.IsSeparatorVisibleProperty);
      }
      set
      {
        base.SetValue(SearchParamsSummaryUC.IsSeparatorVisibleProperty, value);
      }
    }

    private SearchParamsViewModel VM
    {
      get
      {
        return base.DataContext as SearchParamsViewModel;
      }
    }

    public event EventHandler ClearButtonTap;

    public SearchParamsSummaryUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void IsSeparatorVisible_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((UIElement) ((SearchParamsSummaryUC) d).rectSeparator).Visibility = ((bool) e.NewValue ? Visibility.Visible : Visibility.Collapsed);
    }

    private void OpenParamsPage(object sender, System.Windows.Input.GestureEventArgs e)
    {
      SearchParamsViewModel vm = this.VM;
      if (vm == null)
        return;
      vm.NavigateToParametersPage();
    }

    private void Clear_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      SearchParamsViewModel vm = this.VM;
      if (vm != null)
        vm.Clear();
      // ISSUE: reference to a compiler-generated field
      if (this.ClearButtonTap == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.ClearButtonTap(this, EventArgs.Empty);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/SearchParamsSummaryUC.xaml", UriKind.Relative));
      this.rectSeparator = (Rectangle) base.FindName("rectSeparator");
    }
  }
}
