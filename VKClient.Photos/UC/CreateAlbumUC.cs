using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;
using VKClient.Photos.Library;


namespace VKClient.Photos.UC
{
  public class CreateAlbumUC : UserControl
  {
    internal Grid LayoutRoot;
    internal PrivacyHeaderUC ucPrivacyHeaderAlbumView;
    private bool _contentLoaded;

    private CreateEditAlbumViewModel VM
    {
      get
      {
        return base.DataContext as CreateEditAlbumViewModel;
      }
    }

    public CreateAlbumUC()
    {
        this.InitializeComponent();
        this.ucPrivacyHeaderAlbumView.OnTap = delegate
        {
            Navigator.Current.NavigateToEditPrivacy(new EditPrivacyPageInputData
            {
                PrivacyForEdit = this.VM.PrivacyViewVM,
                UpdatePrivacyCallback = delegate(PrivacyInfo pi)
                {
                    this.VM.PrivacyViewVM = new EditPrivacyViewModel(this.VM.PrivacyViewVM.PrivacyQuestion, pi, "", null);
                }
            });
        };
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      (base.DataContext as CreateEditAlbumViewModel).Save();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Photos;component/UC/CreateAlbumUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucPrivacyHeaderAlbumView = (PrivacyHeaderUC) base.FindName("ucPrivacyHeaderAlbumView");
    }
  }
}
