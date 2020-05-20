using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Library.FriendsImport;

namespace VKClient.Common.UC
{
  public class FriendsImportUC : UserControl
  {
      public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(FriendsImportUC), new PropertyMetadata(new PropertyChangedCallback(FriendsImportUC.OnTitleChanged)));
    private FriendsImportViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) base.GetValue(FriendsImportUC.TitleProperty);
      }
      set
      {
        base.SetValue(FriendsImportUC.TitleProperty, value);
      }
    }

    public FriendsImportUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      FriendsImportUC friendsImportUc = (FriendsImportUC) d;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      friendsImportUc.ucHeader.TextBlockTitle.Text = (!string.IsNullOrEmpty(newValue) ? newValue.ToUpperInvariant() : "");
    }

    public void SetFriendsImportProvider(IFriendsImportProvider provider)
    {
      this._viewModel = new FriendsImportViewModel(provider);
      this._viewModel.LoadData();
      base.DataContext = this._viewModel;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/FriendsImportUC.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
    }
  }
}
