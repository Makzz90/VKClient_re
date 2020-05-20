using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library.Posts;
using Windows.Storage.Pickers;

namespace VKClient.Common.UC
{
  public class DocumentPickerUC : UserControl
  {
    private DialogService _dialogService;
    private bool _contentLoaded;

    public DocumentPickerUC()
    {
      this.InitializeComponent();
    }

    public static void Show()
    {
      DocumentPickerUC documentPickerUc = new DocumentPickerUC();
      documentPickerUc._dialogService = new DialogService()
      {
        AnimationType = DialogService.AnimationTypes.None,
        AnimationTypeChild = DialogService.AnimationTypes.Swivel,
        HideOnNavigation = true,
        Child = (FrameworkElement) documentPickerUc
      };
      documentPickerUc._dialogService.Show(null);
    }

    private void FirstButton_OnClicked(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(1, false, true);
    }

    private void SecondButton_OnClicked(object sender, GestureEventArgs e)
    {
      FileOpenPicker fileOpenPicker = new FileOpenPicker();
      this._dialogService.Hide();
      foreach (string supportedDocExtension in VKConstants.SupportedDocExtensions)
        fileOpenPicker.FileTypeFilter.Add(supportedDocExtension);
      ((IDictionary<string, object>) fileOpenPicker.ContinuationData)["Operation"] = (object) AttachmentType.DocumentFromPhone.ToString("G");
      fileOpenPicker.PickSingleFileAndContinue();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/DocumentPickerUC.xaml", UriKind.Relative));
    }
  }
}
