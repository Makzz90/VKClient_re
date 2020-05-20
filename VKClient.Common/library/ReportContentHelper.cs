using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using VKClient.Audio.Base;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public static class ReportContentHelper
  {
    public static ObservableCollection<PickableItem> GetPredefinedReportReasons()
    {
      ObservableCollection<PickableItem> observableCollection = new ObservableCollection<PickableItem>();
      observableCollection.Add(new PickableItem()
      {
        ID = 0,
        Name = CommonResources.ReportReasonSpam.Capitalize()
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 6L,
        Name = CommonResources.ReportReasonInsult.Capitalize()
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 5L,
        Name = CommonResources.ReportReasonAdult.Capitalize()
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 4L,
        Name = CommonResources.ReportReasonDrug.Capitalize()
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 1L,
        Name = CommonResources.ReportReasonChildPorn.Capitalize()
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 2L,
        Name = CommonResources.ReportReasonExtremism.Capitalize()
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 3L,
        Name = CommonResources.ReportReasonViolence.Capitalize()
      });
      return observableCollection;
    }

    public static ObservableCollection<PickableItem> GetPredefinedAdReportReasons()
    {
      ObservableCollection<PickableItem> observableCollection = new ObservableCollection<PickableItem>();
      observableCollection.Add(new PickableItem()
      {
        ID = 0,
        Name = CommonResources.ReportReasonSpam.Capitalize()
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 1L,
        Name = CommonResources.ReportReasonInsult.Capitalize()
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 2L,
        Name = CommonResources.ReportReasonAd_Porn.Capitalize()
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 3L,
        Name = CommonResources.ReportReasonAd_Fraud.Capitalize()
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 4L,
        Name = CommonResources.ReportReasonAd_Other.Capitalize()
      });
      return observableCollection;
    }

    public static ObservableCollection<PickableItem> GetPredefinedAppReportReasons()
    {
      ObservableCollection<PickableItem> observableCollection = new ObservableCollection<PickableItem>();
      observableCollection.Add(new PickableItem()
      {
        ID = 1L,
        Name = CommonResources.ReportReasonApp_ApplicationNotWorking
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 2L,
        Name = CommonResources.ReportReasonApp_MaliciousApplication
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 3L,
        Name = CommonResources.ReportReasonApp_IntrusiveAdvertising
      });
      observableCollection.Add(new PickableItem()
      {
        ID = 4L,
        Name = CommonResources.ReportReasonApp_FraudOrDeceit
      });
      return observableCollection;
    }

    public static void ReportWallPost(WallPost wallPost, string adData = "")
    {
      if (string.IsNullOrEmpty(adData))
      {
        PageBase currentPage = FramePageUtils.CurrentPage;
        if (currentPage != null && currentPage.NavigationContext != null && currentPage.NavigationContext.QueryString.ContainsKey("AdData"))
          adData = currentPage.NavigationContext.QueryString["AdData"];
      }
      if (string.IsNullOrEmpty(adData))
        PickerUC.ShowPickerFor(ReportContentHelper.GetPredefinedReportReasons(),  null, (Action<PickableItem>) (pi =>
        {
          if (wallPost.IsReply)
            WallService.Current.ReportComment(wallPost.from_id, wallPost.id, (ReportReason) pi.ID, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.ReportSent, (VKRequestsDispatcher.Error) null)));
          else
            WallService.Current.Report(wallPost.to_id, wallPost.id, (ReportReason) pi.ID, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.ReportSent, (VKRequestsDispatcher.Error) null)));
        }),  null,  null, CommonResources.PostContains);
      else
        ReportContentHelper.ReportAdWallPost(adData);
    }

    private static void ReportAdWallPost(string adData)
    {
      PickerUC.ShowPickerFor(ReportContentHelper.GetPredefinedAdReportReasons(),  null, (Action<PickableItem>) (pi => AdsIntService.ReportAd(adData, (ReportAdReason) pi.ID, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>) (res =>
      {
        EventAggregator.Current.Publish(new AdReportedEvent()
        {
          AdData = adData
        });
        GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.ReportSent, (VKRequestsDispatcher.Error) null);
      }))),  null,  null, CommonResources.PostContains);
    }

    public static void ReportApp(long appId, long ownerId = 0)
    {
      PickerUC.ShowPickerFor(ReportContentHelper.GetPredefinedAppReportReasons(),  null,  (pi => AppsService.Instance.Report(appId, (ReportAppReason) pi.ID, ownerId, "", (Action<BackendResult<int, ResultCode>>) (res => GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.ReportSent, (VKRequestsDispatcher.Error) null)))),  null,  null, CommonResources.Report.ToUpperInvariant());
    }

    internal static void ReportComment(long fromId, long id, LikeObjectType likeObjectType)
    {
      PickerUC.ShowPickerFor(ReportContentHelper.GetPredefinedReportReasons(),  null, (Action<PickableItem>) (pi =>
      {
        switch (likeObjectType)
        {
          case LikeObjectType.comment:
            WallService.Current.ReportComment(fromId, id, (ReportReason) pi.ID, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.ReportSent, (VKRequestsDispatcher.Error) null)));
            break;
          case LikeObjectType.photo_comment:
            PhotosService.Current.ReportComment(fromId, id, (ReportReason) pi.ID, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.ReportSent, (VKRequestsDispatcher.Error) null)));
            break;
          case LikeObjectType.video_comment:
            VideoService.Instance.ReportComment(fromId, id, (ReportReason) pi.ID, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.ReportSent, (VKRequestsDispatcher.Error) null)));
            break;
        }
      }),  null,  null, CommonResources.CommentContains);
    }

    public static void ReportPhoto(long ownerId, long photoId)
    {
      PickerUC.ShowPickerFor(ReportContentHelper.GetPredefinedReportReasons(),  null,  (pi => PhotosService.Current.Report(ownerId, photoId, (ReportReason) pi.ID, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.ReportSent, (VKRequestsDispatcher.Error) null)))),  null,  null, CommonResources.PhotoContains);
    }

    public static void ReportVideo(long ownerId, long videoId)
    {
      PickerUC.ShowPickerFor(ReportContentHelper.GetPredefinedReportReasons(),  null,  (pi => VideoService.Instance.Report(ownerId, videoId, (ReportReason) pi.ID, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.ReportSent, (VKRequestsDispatcher.Error) null)))),  null,  null, CommonResources.VideoContains);
    }
  }
}
