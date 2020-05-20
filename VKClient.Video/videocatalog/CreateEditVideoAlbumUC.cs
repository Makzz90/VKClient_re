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
  public class CreateEditVideoAlbumUC : UserControl
  {
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal TextBox textBoxName;
    internal PrivacyHeaderUC ucPrivacyHeaderAlbumView;
    private bool _contentLoaded;

    private CreateEditVideoAlbumViewModel VM
    {
      get
      {
        return base.DataContext as CreateEditVideoAlbumViewModel;
      }
    }

    public CreateEditVideoAlbumUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void ucPrivacyHeaderAlbumView_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.NavigateToEditPrivacy(new EditPrivacyPageInputData()
      {
        PrivacyForEdit = this.VM.AlbumPrivacyVM,
        UpdatePrivacyCallback = (Action<PrivacyInfo>) (pi => this.VM.AlbumPrivacyVM = new EditPrivacyViewModel(this.VM.AlbumPrivacyVM.PrivacyQuestion, pi, "",  null))
      });
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/CreateEditVideoAlbumUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.textBoxName = (TextBox) base.FindName("textBoxName");
      this.ucPrivacyHeaderAlbumView = (PrivacyHeaderUC) base.FindName("ucPrivacyHeaderAlbumView");
    }
  }
}
