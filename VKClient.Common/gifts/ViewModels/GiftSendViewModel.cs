using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Gifts.ViewModels
{
  public class GiftSendViewModel : ViewModelStatefulBase
  {
    private readonly List<long> _userIds = new List<long>();
    private readonly List<string> _giftSendStepsPages = new List<string>() { "GiftsCatalogPage", "GiftsCatalogCategoryPage" };
    private readonly long _giftId;
    private readonly string _categoryName;
    private readonly bool _isProduct;
    private readonly int _price;
    private readonly int _giftsLeft;
    private readonly NavigationService _navigationService;
    private readonly int _randomId;
    private int _balance;
    private bool _isSending;

    public bool AreNameAndTextPublic = true;

    private int TotalPrice
    {
      get
      {
        return this._userIds.Count * this._price;
      }
    }

    private bool CanAddRecipient
    {
      get
      {
        if (this._giftsLeft > 0)
          return this._userIds.Count < this._giftsLeft;
        return true;
      }
    }

    public string ImageUrl { get;set; }

    public string Description { get;set; }

    public Visibility DescriptionVisibility
    {
      get
      {
        return (!string.IsNullOrEmpty(this.Description)).ToVisiblity();
      }
    }

    public string TotalPriceStr
    {
      get
      {
        if (this._giftsLeft > 0)
          return CommonResources.Free;
        int totalPrice = this.TotalPrice;
        return UIStringFormatterHelper.FormatNumberOfSomething(totalPrice > 0 ? totalPrice : this._price, CommonResources.OneVoteFrm, CommonResources.TwoFourVotesFrm, CommonResources.FiveVotesFrm, true,  null, false);
      }
    }

    public string VotesStr
    {
      get
      {
        int giftsLeft = this._giftsLeft;
        if (giftsLeft > 0)
          return string.Format(CommonResources.GiftsLeftFrm, giftsLeft);
        if (this._balance != 0)
          return UIStringFormatterHelper.FormatNumberOfSomething(this._balance, CommonResources.YouHaveOneVoteFrm, CommonResources.YouHaveTwoFourVotesFrm, CommonResources.YouHaveFiveVotesFrm, true,  null, false);
        return CommonResources.YouHaveNoVotes;
      }
    }

    public ObservableCollection<User> Users { get; private set; }

    public Visibility AddRecipientVisibility
    {
      get
      {
        return this.CanAddRecipient.ToVisiblity();
      }
    }

    public string Message { get; set; }

    private GiftSendViewModel(long giftId, string categoryName, bool isProduct, string description, string imageUrl, int price, int giftsLeft, NavigationService navigationService)
    {
      this._giftId = giftId;
      this._categoryName = categoryName;
      this._isProduct = isProduct;
      this._navigationService = navigationService;
      this._randomId = new Random().Next(0, int.MaxValue);
      this.Description = description;
      this.ImageUrl = imageUrl;
      this._price = price;
      this._giftsLeft = giftsLeft;
    }

    public GiftSendViewModel(long giftId, string categoryName, bool isProduct, string description, string imageUrl, int price, int giftsLeft, NavigationService navigationService, List<long> userIds)
      : this(giftId, categoryName, isProduct, description, imageUrl, price, giftsLeft, navigationService)
    {
      if (userIds == null)
        return;
      this._userIds.AddRange((IEnumerable<long>) Enumerable.Distinct<long>(userIds));
    }

    public GiftSendViewModel(long giftId, string categoryName, bool isProduct, string description, string imageUrl, int price, int giftsLeft, NavigationService navigationService, List<long> userIds, string message, bool areNameAndTextPublic)
      : this(giftId, categoryName, isProduct, description, imageUrl, price, giftsLeft, navigationService, userIds)
    {
      this.Message = message;
      this.AreNameAndTextPublic = areNameAndTextPublic;
    }

    public override void Load(Action<ResultCode> callback)
        {
            GiftsService.Instance.GetGiftInfo(this._userIds, (Action<BackendResult<GiftResponse, ResultCode>>)(result =>
            {
                ResultCode resultCode = result.ResultCode;
                if (resultCode == ResultCode.Succeeded)
                    Execute.ExecuteOnUIThread((Action)(() =>
                    {
                        GiftResponse resultData = result.ResultData;
                        this._balance = resultData.balance;
                        this.UpdateUsers((IEnumerable<User>)resultData.users);
                        this.NotifyProperties();
                    }));
                Action<ResultCode> action = callback;
                if (action == null)
                    return;
                int num = (int)resultCode;
                action((ResultCode)num);
            }));
        }

    private void UpdateUsers(IEnumerable<User> usersList)
    {
      List<User> userList = new List<User>(usersList);
      if (this._giftsLeft > 0)
      {
        int giftsLeft = this._giftsLeft;
        while (userList.Count > giftsLeft)
          userList.RemoveAt(userList.Count - 1);
      }
      this._userIds.Clear();
      this.Users = new ObservableCollection<User>();
      List<User>.Enumerator enumerator = userList.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          User current = enumerator.Current;
          this._userIds.Add(current.id);
          ((Collection<User>) this.Users).Add(current);
        }
      }
      finally
      {
        enumerator.Dispose();
      }
    }

    private void NotifyProperties()
        {
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.ImageUrl));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.Description));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.VotesStr));
            this.NotifyPropertyChanged<ObservableCollection<User>>((Expression<Func<ObservableCollection<User>>>)(() => this.Users));
            this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.TotalPriceStr));
            this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.AddRecipientVisibility));
        }

    public void AddRecipient(User user)
        {
            if (!this.CanAddRecipient)
                return;
            long userId = user.id;
            if (!this._userIds.Contains(userId))
                this._userIds.Add(userId);
            if (this.Users.FirstOrDefault<User>((Func<User, bool>)(u => u.id == userId)) == null)
                this.Users.Add(user);
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.TotalPriceStr));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.AddRecipientVisibility));
            }));
        }

    public void RemoveRecipient(long userId)
        {
            if (this._userIds.Contains(userId))
                this._userIds.Remove(userId);
            User user = this.Users.FirstOrDefault<User>((Func<User, bool>)(u => u.id == userId));
            if (user != null)
                this.Users.Remove(user);
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.TotalPriceStr));
                this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.AddRecipientVisibility));
            }));
        }

    public GiftSendPageState GetState()
    {
      GiftSendPageState giftSendPageState = new GiftSendPageState();
      giftSendPageState.Description = this.Description;
      giftSendPageState.ImageUrl = this.ImageUrl;
      giftSendPageState.Price = this._price;
      giftSendPageState.GiftsLeft = this._giftsLeft;
      giftSendPageState.UserIds = this._userIds;
      giftSendPageState.Message = this.Message;
      int num = this.AreNameAndTextPublic ? 1 : 0;
      giftSendPageState.AreNameAndTextPublic = num != 0;
      return giftSendPageState;
    }

    public void NavigateToAddRecipient()
    {
      if (this._isProduct)
        Navigator.Current.NavigateToPickUser(-this._giftId);
      else
        Navigator.Current.NavigateToPickUser(false, 0, true, 0, PickUserMode.PickWithSearch, CommonResources.MoneyTransfers_ChooseRecipient, 0, true);
    }

    public void Send()
    {
        if (this._isSending || this._userIds.Count == 0 || this._giftId == 0L)
            return;
        this._isSending = true;
        this.SetInProgress(true, "");
        EventAggregator.Current.Publish((object)new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.gift_page, GiftPurchaseStepsAction.purchase_click));
        List<long> userIds = this._userIds;
        long giftId = this._giftId;
        string message = this.Message;
        GiftPrivacy privacy = this.AreNameAndTextPublic ? GiftPrivacy.VisibleToAll : GiftPrivacy.VisibleToRecipient;
        string categoryName = this._categoryName;
        int randomId = this._randomId;

        GiftsService.Instance.Send(userIds, giftId, randomId, message, privacy, categoryName, (Action<BackendResult<VKList<GiftSentResponse>, ResultCode>>)(result =>
        {
            this._isSending = false;
            this.SetInProgress(false, "");
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                ResultCode resultCode = result.ResultCode;
                if (resultCode == ResultCode.Succeeded)
                {
                    EventAggregator.Current.Publish((object)new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.gift_page, GiftPurchaseStepsAction.success));
                    EventAggregator.Current.Publish((object)new GiftSentEvent(giftId, userIds));
                    this.RemoveGiftSendStepsPages();
                    if (this._navigationService.CanGoBack)
                        Navigator.Current.GoBack();
                    else
                        Navigator.Current.NavigateToMainPage();
                    new DelayedExecutor(1000).AddToDelayedExecution((Action)(() => Execute.ExecuteOnUIThread((Action)(() => new VKClient.Common.UC.GenericInfoUC().ShowAndHideLater(userIds.Count > 1 ? CommonResources.GiftsSent : CommonResources.GiftSent, null)))));
                }
                else
                    VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult(resultCode, "", null);
            }));
        }));
    }

    private void RemoveGiftSendStepsPages()
    {
      try
      {
        while (true)
        {
          JournalEntry journalEntry = (JournalEntry) Enumerable.FirstOrDefault<JournalEntry>(this._navigationService.BackStack);
          if (journalEntry != null && this.GetIsGiftSendStepsPage(journalEntry.Source.ToString()))
            this._navigationService.RemoveBackEntry();
          else
            break;
        }
      }
      catch
      {
      }
    }

    private bool GetIsGiftSendStepsPage(string uri)
    {
      // ISSUE: method pointer
        return Enumerable.Any<string>(this._giftSendStepsPages, (Func<string, bool>)new Func<string, bool>(uri.Contains));
    }
  }
}
