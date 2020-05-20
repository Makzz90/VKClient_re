using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Video.VideoCatalog
{
    public partial class CreateEditVideoAlbumUC : UserControl
  {

    private CreateEditVideoAlbumViewModel VM
    {
      get
      {
        return this.DataContext as CreateEditVideoAlbumViewModel;
      }
    }

    public CreateEditVideoAlbumUC()
    {
      this.InitializeComponent();
    }

    private void ucPrivacyHeaderAlbumView_Tap(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToEditPrivacy(new EditPrivacyPageInputData()
      {
        PrivacyForEdit = this.VM.AlbumPrivacyVM,
        UpdatePrivacyCallback = (Action<PrivacyInfo>) (pi => this.VM.AlbumPrivacyVM = new EditPrivacyViewModel(this.VM.AlbumPrivacyVM.PrivacyQuestion, pi, "", (List<string>) null))
      });
    }

  }
}
