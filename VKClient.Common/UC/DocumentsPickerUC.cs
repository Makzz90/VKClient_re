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
  public class DocumentsPickerUC : UserControl
  {
    private DialogService _dialogService;
    private int _maxSelectionCount;
    private bool _contentLoaded;

    public DocumentsPickerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    public static void Show(int maxSelectionCount)
    {
      DocumentsPickerUC documentsPickerUc = new DocumentsPickerUC();
      documentsPickerUc._maxSelectionCount = maxSelectionCount;
      documentsPickerUc._dialogService = new DialogService()
      {
        AnimationType = DialogService.AnimationTypes.None,
        AnimationTypeChild = DialogService.AnimationTypes.Swivel,
        HideOnNavigation = true,
        Child = (FrameworkElement) documentsPickerUc
      };
      documentsPickerUc._dialogService.Show( null);
    }

    private void FirstButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(this._maxSelectionCount, false, true);
    }

    private void SecondButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FileOpenPicker fileOpenPicker = new FileOpenPicker();
      this._dialogService.Hide();
      foreach (string supportedDocExtension in VKConstants.SupportedDocExtensions)
        fileOpenPicker.FileTypeFilter.Add(supportedDocExtension);
      ((IDictionary<string, object>) fileOpenPicker.ContinuationData)["Operation"] = AttachmentType.DocumentFromPhone.ToString("G");
      fileOpenPicker.PickSingleFileAndContinue();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/DocumentPickerUC.xaml", UriKind.Relative));
    }
  }
}
