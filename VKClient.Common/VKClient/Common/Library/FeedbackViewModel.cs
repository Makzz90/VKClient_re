using System;
using System.Linq;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class FeedbackViewModel : ViewModelBase, ICollectionDataProvider<NotificationData, IVirtualizable>, ICollectionDataProvider<NewsFeedData, IVirtualizable>
  {
    private string _from = "";
    private string _commentsFrom = "";
    private MyVirtualizingPanel2 _panelFeedback;
    private MyVirtualizingPanel2 _panelComments;
    private int _offset;

    public GenericCollectionViewModel<NotificationData, IVirtualizable> FeedbackVM { get; set; }

    public GenericCollectionViewModel<NewsFeedData, IVirtualizable> CommentsVM { get; set; }

    public string Title
    {
      get
      {
        return CommonResources.NOTIFICATIONS;
      }
    }

    public Func<NotificationData, ListWithCount<IVirtualizable>> ConverterFunc
    {
      get
      {
        return (Func<NotificationData, ListWithCount<IVirtualizable>>) (ndata =>
        {
          this._from = ndata.next_from;
          this._offset = ndata.new_offset;
          ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>();
          for (int index = 0; index < ndata.items.Count; ++index)
          {
            Notification notification = ndata.items[index];
            bool showEarliesReplies = notification.date > ndata.last_viewed && index < ndata.items.Count - 1 && ndata.items[index + 1].date < ndata.last_viewed;
            if (!this.FeedbackVM.Collection.Any<IVirtualizable>((Func<IVirtualizable, bool>) (f =>
            {
              if ((f as NewsFeedbackItem).Notification.date == notification.date)
                return (f as NewsFeedbackItem).Notification.NotType == notification.NotType;
              return false;
            })))
            {
              NewsFeedbackItem newsFeedbackItem = new NewsFeedbackItem(480.0, new Thickness(), notification, ndata.profiles, ndata.groups, showEarliesReplies);
              listWithCount.List.Add((IVirtualizable) newsFeedbackItem);
            }
          }
          return listWithCount;
        });
      }
    }

    Func<NewsFeedData, ListWithCount<IVirtualizable>> ICollectionDataProvider<NewsFeedData, IVirtualizable>.ConverterFunc
    {
      get
      {
        return (Func<NewsFeedData, ListWithCount<IVirtualizable>>) (ndata =>
        {
          this._commentsFrom = ndata.next_from;
          ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>();
          foreach (NewsItem newsItem in ndata.items)
          {
            double width = 480.0;
            Thickness margin = new Thickness();
            NewsItemDataWithUsersAndGroupsInfo newsItemData = new NewsItemDataWithUsersAndGroupsInfo();
            newsItemData.Groups = ndata.groups;
            newsItemData.NewsItem = newsItem;
            newsItemData.Profiles = ndata.profiles;
            Action<string> unsubscribedCallback = new Action<string>(this.OnUnsubscribedFromNews);
            NewsCommentsItem newsCommentsItem = new NewsCommentsItem(width, margin, newsItemData, unsubscribedCallback);
            listWithCount.List.Add((IVirtualizable) newsCommentsItem);
          }
          return listWithCount;
        });
      }
    }

    public FeedbackViewModel(MyVirtualizingPanel2 panelFeedback, MyVirtualizingPanel2 panelComments)
    {
      this._panelFeedback = panelFeedback;
      this._panelComments = panelComments;
      this.FeedbackVM = new GenericCollectionViewModel<NotificationData, IVirtualizable>((ICollectionDataProvider<NotificationData, IVirtualizable>) this)
      {
        NoContentText = CommonResources.NoContent_Feedback,
        NoContentImage = "../Resources/NoContentImages/Feedback.png"
      };
      this.CommentsVM = new GenericCollectionViewModel<NewsFeedData, IVirtualizable>((ICollectionDataProvider<NewsFeedData, IVirtualizable>) this);
    }

    public void LoadFeedback(bool refresh)
    {
      this.FeedbackVM.LoadData(refresh, false, (Action<BackendResult<NotificationData, ResultCode>>) null, false);
    }

    public void LoadComments(bool refresh)
    {
      this.CommentsVM.LoadData(refresh, false, (Action<BackendResult<NewsFeedData, ResultCode>>) null, false);
    }

    public void GetData(GenericCollectionViewModel<NotificationData, IVirtualizable> caller, int offset, int count, Action<BackendResult<NotificationData, ResultCode>> callback)
    {
      NewsFeedService.Current.GetNotifications(Extensions.DateTimeToUnixTimestamp(DateTime.UtcNow - TimeSpan.FromDays(30.0), true), 0, offset == 0 ? 0 : this._offset, offset == 0 ? "" : this._from, count, (Action<BackendResult<NotificationData, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded)
          NewsFeedService.Current.MarkAsViewed();
        callback(res);
      }));
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<NotificationData, IVirtualizable> caller, int count)
    {
      return FeedbackViewModel.GetNewsFooterText(count);
    }

    public void GetData(GenericCollectionViewModel<NewsFeedData, IVirtualizable> caller, int offset, int count, Action<BackendResult<NewsFeedData, ResultCode>> callback)
    {
      string fromStr = offset == 0 ? "" : this._commentsFrom;
      if (offset > 0 && string.IsNullOrWhiteSpace(this._commentsFrom))
        callback(new BackendResult<NewsFeedData, ResultCode>(ResultCode.Succeeded, new NewsFeedData()));
      else
        NewsFeedService.Current.GetNewsComments(0, 0, count, fromStr, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<NewsFeedData, IVirtualizable> caller, int count)
    {
      return FeedbackViewModel.GetNewsFooterText(count);
    }

    private void OnUnsubscribedFromNews(string itemId)
    {
      foreach (NewsCommentsItem newsCommentsItem in this.CommentsVM.Collection.Cast<NewsCommentsItem>())
      {
        if (newsCommentsItem != null && newsCommentsItem.ItemId == itemId)
        {
          this.CommentsVM.Delete((IVirtualizable) newsCommentsItem);
          break;
        }
      }
    }

    private static string GetNewsFooterText(int count)
    {
      if (count <= 0)
        return CommonResources.NoRecentNews;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneRecentNewsFrm, CommonResources.TwoRecentNewsFrm, CommonResources.FiveRecentNewsFrm, true, null, false);
    }
  }
}
