using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GenericInfoUC : UserControl
  {
    private readonly DelayedExecutor _deHide;
    private const int DEFAULT_DELAY = 1000;
    internal Grid LayoutRoot;
    internal ScrollableTextBlock textBlockInfo;
    internal RichTextBox richTextBox;
    private bool _contentLoaded;

    public GenericInfoUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this._deHide = new DelayedExecutor(1000);
    }

    public GenericInfoUC(int delayToHide)
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this._deHide = new DelayedExecutor(delayToHide > 0 ? delayToHide : 1000);
    }

    public void ShowAndHideLater(string text, FrameworkElement elementToFadeout = null)
    {
      DialogService ds = new DialogService();
      this.textBlockInfo.Text = text;
      ds.IsOverlayApplied = false;
      ds.Child = (FrameworkElement) this;
      ds.KeepAppBar = true;
      ds.Show((UIElement) elementToFadeout);
      this._deHide.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() => ds.Hide()))));
    }

    public static void ShowBasedOnResult(ResultCode resultCode, string successString = "", VKRequestsDispatcher.Error error = null)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (resultCode.IsCancelResultCode() || resultCode == ResultCode.ValidationCancelledOrFailed)
          return;
        if (resultCode == ResultCode.Succeeded)
        {
          if (string.IsNullOrWhiteSpace(successString))
            return;
          new GenericInfoUC().ShowAndHideLater(successString,  null);
        }
        else
        {
          int delayToHide = 0;
          string text = CommonResources.Error;
          switch (resultCode)
          {
            case ResultCode.Processing:
              text = CommonResources.Registration_TryAgainLater;
              break;
            case ResultCode.ProductNotFound:
              text = CommonResources.CannotLoadProduct;
              delayToHide = 2000;
              break;
            case ResultCode.VideoNotFound:
              text = CommonResources.CannotLoadVideo;
              delayToHide = 2000;
              break;
            case ResultCode.WrongPhoneNumberFormat:
              text = CommonResources.Registration_InvalidPhoneNumber;
              break;
            case ResultCode.PhoneAlreadyRegistered:
              text = CommonResources.Registration_PhoneNumberIsBusy;
              break;
            case ResultCode.InvalidCode:
              text = CommonResources.Registration_WrongCode;
              break;
            case ResultCode.InvalidAudioFormat:
              text = CommonResources.InvalidAudioFormatError;
              delayToHide = 3000;
              break;
            case ResultCode.AudioIsExcludedByRightholder:
              text = CommonResources.AudioIsExcludedByRightholderError;
              delayToHide = 3000;
              break;
            case ResultCode.MaximumLimitReached:
              text = CommonResources.AudioFileSizeLimitReachedError;
              delayToHide = 3000;
              break;
            case ResultCode.UploadingFailed:
              text = CommonResources.FailedToConnectError;
              delayToHide = 3000;
              break;
            case ResultCode.CommunicationFailed:
              text = CommonResources.Error_Connection;
              delayToHide = 3000;
              break;
          }
          VKRequestsDispatcher.Error error1 = error;
          string str = error1 != null ? error1.error_text :  null;
          if (!string.IsNullOrWhiteSpace(str))
            text = str;
          new GenericInfoUC(delayToHide).ShowAndHideLater(text,  null);
        }
      }));
    }

    public static void ShowBasedOnResult(int resultCode, string successString = "", VKRequestsDispatcher.Error error = null)
    {
      GenericInfoUC.ShowBasedOnResult((ResultCode) resultCode, successString, error);
    }

    public static void ShowPhotoIsSavedInSavedPhotos()
    {
      GenericInfoUC genericInfoUc = new GenericInfoUC(3000);
      ((UIElement) genericInfoUc.richTextBox).Visibility = Visibility.Visible;
      ((UIElement) genericInfoUc.textBlockInfo).Visibility = Visibility.Collapsed;
      RichTextBox richTextBox = genericInfoUc.richTextBox;
      Paragraph paragraph = new Paragraph();
      string text1 = CommonResources.PhotoIsSaved.Replace(".", "") + " ";
      ((PresentationFrameworkCollection<Inline>) paragraph.Inlines).Add((Inline) BrowserNavigationService.GetRunWithStyle(text1, richTextBox));
      Hyperlink hyperlink = HyperlinkHelper.GenerateHyperlink(CommonResources.InTheAlbum, "", (Action<Hyperlink, string>) ((hl, str) => Navigator.Current.NavigateToPhotoAlbum(AppGlobalStateManager.Current.LoggedInUserId, false, "SavedPhotos", "", CommonResources.AlbumSavedPictures, 1, "", "", false, 0, false)), ((Control) richTextBox).Foreground, HyperlinkState.Normal);
      ((TextElement) hyperlink).FontSize=(((Control) richTextBox).FontSize);
      ((PresentationFrameworkCollection<Inline>) paragraph.Inlines).Add((Inline) hyperlink);
      ((PresentationFrameworkCollection<Block>) richTextBox.Blocks).Add((Block) paragraph);
      string text2 = "";
      // ISSUE: variable of the null type
      
      genericInfoUc.ShowAndHideLater(text2, null);
    }

    public static void ShowPublishResult(GenericInfoUC.PublishedObj publishedObj, long gid = 0, string groupName = "")
    {
      new GenericInfoUC(3000).ShowAndHideLater(GenericInfoUC.GetInfoStr(publishedObj, gid, groupName),  null);
    }

    private static string GetInfoStr(GenericInfoUC.PublishedObj publishedObj, long gid, string groupName)
    {
      string format = "";
      string str;
      if (gid == 0L)
      {
        switch (publishedObj)
        {
          case GenericInfoUC.PublishedObj.WallPost:
            format = CommonResources.ThePostIsPublishedOnWallFrm;
            break;
          case GenericInfoUC.PublishedObj.Photo:
            format = CommonResources.ThePhotoIsPublishedOnWallFrm;
            break;
          case GenericInfoUC.PublishedObj.Video:
            format = CommonResources.TheVideoIsPublishedOnWallFrm;
            break;
          case GenericInfoUC.PublishedObj.Doc:
            format = CommonResources.TheDocumentIsPublishedOnWallFrm;
            break;
        }
        str = string.Format(format, ("[id" + AppGlobalStateManager.Current.LoggedInUserId + "|" + CommonResources.Yours + "]"));
      }
      else
      {
        switch (publishedObj)
        {
          case GenericInfoUC.PublishedObj.WallPost:
            format = CommonResources.ThePostIsPublishedInCommunityFrm;
            break;
          case GenericInfoUC.PublishedObj.Photo:
            format = CommonResources.ThePhotoIsPublishedInCommunityFrm;
            break;
          case GenericInfoUC.PublishedObj.Video:
            format = CommonResources.TheVideoIsPublishedInCommunityFrm;
            break;
          case GenericInfoUC.PublishedObj.Doc:
            format = CommonResources.TheDocumentIsPublishedInCommunityFrm;
            break;
        }
        str = string.Format(format, ("[club" + gid + "|" + groupName + "]"));
      }
      return str;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/GenericInfoUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.textBlockInfo = (ScrollableTextBlock) base.FindName("textBlockInfo");
      this.richTextBox = (RichTextBox) base.FindName("richTextBox");
    }

    public enum PublishedObj
    {
      WallPost,
      Photo,
      Video,
      Doc,
    }
  }
}
