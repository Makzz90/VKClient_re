using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;

namespace VKClient.Common.UC
{
  public class FriendsRecommendationsUC : UserControl, IHandle<ContactsSyncEnabled>, IHandle
  {
      private readonly ObservableCollection<FriendRecommendationItem> _itemsSource = new ObservableCollection<FriendRecommendationItem>();
    private readonly List<long> _sendedToStatsRecommendationsIds = new List<long>();
    private readonly List<long> _recommendationsIds = new List<long>();
    private FriendRecommendationItem _contactsSyncPromoItem;
    private string _loadingNextFrom;
    private int _addedToStatsCount;
    private bool _isLoading;
    internal ExtendedLongListSelector ItemsList;
    private bool _contentLoaded;

    public FriendsRecommendationsUC(NewsItemDataWithUsersAndGroupsInfo newsItem)
    {
      //this.\u002Ector();
      this.InitializeComponent();
      EventAggregator.Current.Subscribe(this);
      this.ItemsList.ItemsSource = ((IList) this._itemsSource);
      this._loadingNextFrom = newsItem.NewsItem.next_from;
      foreach (User profile in newsItem.NewsItem.profiles)
      {
        if (!this._recommendationsIds.Contains(profile.id))
        {
          this._recommendationsIds.Add(profile.id);
          this._itemsSource.Add(new FriendRecommendationItem(profile));
          if (this._addedToStatsCount < 2)
          {
            StatsEventsTracker.Instance.Handle(new FriendRecommendationShowedEvent()
            {
              UserId = profile.id
            });
            this._sendedToStatsRecommendationsIds.Add(profile.id);
            this._addedToStatsCount = this._addedToStatsCount + 1;
          }
        }
      }
      if (!newsItem.NewsItem.account_import_block_pos.HasValue || AppGlobalStateManager.Current.GlobalState.AllowSendContacts)
        return;
      this._contactsSyncPromoItem = new FriendRecommendationItem( null);
      this._itemsSource.Insert(newsItem.NewsItem.account_import_block_pos.Value, this._contactsSyncPromoItem);
    }

    private void Recommendation_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.NavigateToUserProfile((((FrameworkElement) sender).DataContext as FriendRecommendationItem).UserId, "", "user_rec", false);
    }

    private void RecommendationHideButton_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FriendRecommendationItem recommendation = ((FrameworkElement) sender).DataContext as FriendRecommendationItem;
      e.Handled = true;
      string methodName = "friends.hideSuggestion";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("user_id", recommendation.UserId.ToString());
      Action<BackendResult<int, ResultCode>> callback = (Action<BackendResult<int, ResultCode>>) (c =>
      {
        if (c.ResultCode != ResultCode.Succeeded)
          return;
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (!this._itemsSource.Contains(recommendation))
            return;
          this._itemsSource.Remove(recommendation);
        }));
      });
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      // ISSUE: variable of the null type
      
      VKRequestsDispatcher.DispatchRequestToVK<int>(methodName, parameters, callback, (Func<string, int>) (json => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<int>>(json).response), num1 != 0, num2 != 0, cancellationToken, null);
    }

    private void RecommendationAddToFriendsButton_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FriendRecommendationItem recommendation = ((FrameworkElement) sender).DataContext as FriendRecommendationItem;
      e.Handled = true;
      if (recommendation.IsHandled)
      {
        Navigator.Current.NavigateToUserProfile(recommendation.UserId, "", "user_rec", false);
      }
      else
      {
        string methodName = "friends.add";
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("user_id", recommendation.UserId.ToString());
        Action<BackendResult<int, ResultCode>> callback = (Action<BackendResult<int, ResultCode>>) (c =>
        {
          if (c.ResultCode != ResultCode.Succeeded)
            return;
          EventAggregator.Current.Publish(new FriendRequestSent()
          {
            UserId = recommendation.UserId
          });
        });
        int num1 = 0;
        int num2 = 1;
        CancellationToken? cancellationToken = new CancellationToken?();
        // ISSUE: variable of the null type
        
        VKRequestsDispatcher.DispatchRequestToVK<int>(methodName, parameters, callback, (Func<string, int>) (json => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<int>>(json).response), num1 != 0, num2 != 0, cancellationToken, null);
      }
    }

    private void ItemsList_OnScrollPositionChanged(object sender, EventArgs e)
    {
      if (this._loadingNextFrom != null && !this._isLoading && this.ItemsList.ScrollPosition + 480.0 > 272.0 * (double) (this._itemsSource.Count - 4))
      {
        this._isLoading = true;
        Dictionary<string, string> dictionary = new Dictionary<string, string>()
        {
          {
            "count",
            "20"
          },
          {
            "start_from",
            this._loadingNextFrom
          },
          {
            "fields",
            "crop_photo"
          }
        };
        string methodName = "friends.getRecommendations";
        Dictionary<string, string> parameters = dictionary;
        Action<BackendResult<FriendsRecommendationsList, ResultCode>> callback = (Action<BackendResult<FriendsRecommendationsList, ResultCode>>) (result =>
        {
          if (result.ResultCode == ResultCode.Succeeded)
          {
            this._loadingNextFrom = result.ResultData.next_from;
            Execute.ExecuteOnUIThread((Action) (() =>
            {
              foreach (User user in result.ResultData.items)
              {
                if (!this._recommendationsIds.Contains(user.id))
                {
                  this._recommendationsIds.Add(user.id);
                  this._itemsSource.Add(new FriendRecommendationItem(user));
                }
              }
            }));
          }
          this._isLoading = false;
        });
        int num1 = 0;
        int num2 = 1;
        CancellationToken? cancellationToken = new CancellationToken?();
        // ISSUE: variable of the null type
        
        VKRequestsDispatcher.DispatchRequestToVK<FriendsRecommendationsList>(methodName, parameters, callback, (Func<string, FriendsRecommendationsList>) (result => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<FriendsRecommendationsList>>(result).response), num1 != 0, num2 != 0, cancellationToken, null);
      }
      if (this.ItemsList.ScrollPosition >= 8388608.0 || this.ItemsList.ScrollPosition + 480.0 <= 272.0 * ((double) this._addedToStatsCount + 1.55) || this._addedToStatsCount >= this._itemsSource.Count)
        return;
      FriendRecommendationItem recommendationItem = this._itemsSource[this._addedToStatsCount];
      if (recommendationItem.Type == FriendRecommendationItemType.Default && !this._sendedToStatsRecommendationsIds.Contains(recommendationItem.UserId))
      {
        StatsEventsTracker.Instance.Handle(new FriendRecommendationShowedEvent()
        {
          UserId = recommendationItem.UserId
        });
        this._sendedToStatsRecommendationsIds.Add(recommendationItem.UserId);
      }
      this._addedToStatsCount = this._addedToStatsCount + 1;
    }

    private void ContactsSyncStartButton_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ContactsSyncRequestUC.OpenFriendsImportContacts((Action) (() => Navigator.Current.NavigateToFriendsImportContacts()));
    }

    public void Handle(ContactsSyncEnabled message)
    {
      if (this._contactsSyncPromoItem == null || !this._itemsSource.Contains(this._contactsSyncPromoItem))
        return;
      this._itemsSource.Remove(this._contactsSyncPromoItem);
      this._contactsSyncPromoItem =  null;
    }

    private void ItemsList_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
    }

    private void ItemsList_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
    }

    private void ItemsList_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
    }

    private void RecommendationAddToFriendsButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    private void RecommendationHideButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/FriendsRecommendationsUC.xaml", UriKind.Relative));
      this.ItemsList = (ExtendedLongListSelector) base.FindName("ItemsList");
    }
  }
}
