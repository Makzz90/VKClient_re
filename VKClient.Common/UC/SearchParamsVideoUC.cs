using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using VKClient.Common.Library;
using VKClient.Common.Localization;

namespace VKClient.Common.UC
{
  public class SearchParamsVideoUC : SearchParamsUCBase
  {
    private SearchParamsVideoViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    private bool _contentLoaded;

    public SearchParamsVideoUC()
    {
      this.InitializeComponent();
      this.ucHeader.HideSandwitchButton = true;
      this.ucHeader.TextBlockTitle.Text = CommonResources.PageTitle_UsersSearch_SearchParameters;
    }

    public override Dictionary<string, string> GetParameters()
    {
      return this._viewModel.Parameters;
    }

    public override void Initialize(Dictionary<string, string> parameters)
    {
      this._viewModel = new SearchParamsVideoViewModel(parameters);
      base.DataContext = this._viewModel;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/SearchParamsVideoUC.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
    }
  }
}
