using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Gifts.ViewModels
{
  public class GiftsViewModel : ViewModelBase, ICollectionDataProvider<GiftsResponse, GiftHeader>, IHandle<GiftSentEvent>, IHandle
  {
    private readonly long _userId;
    private readonly string _firstName;
    private string _firstNameGen;

    public Visibility AccessDeniedVisibility { get; private set; }

    public bool IsCurrentUser { get;private set; }

    public GenericCollectionViewModel<GiftsResponse, GiftHeader> GiftsVM { get;private set; }

    public string Title
    {
      get
      {
        return (this._userId != AppGlobalStateManager.Current.LoggedInUserId ? (!string.IsNullOrEmpty(this._firstName) || !string.IsNullOrEmpty(this._firstNameGen) ? string.Format(CommonResources.UsersGiftsFrm, CultureHelper.GetCurrent() == CultureName.KZ ? this._firstName : this._firstNameGen) : CommonResources.Gifts) : CommonResources.MyGifts).ToUpperInvariant();
      }
    }

    public Func<GiftsResponse, ListWithCount<GiftHeader>> ConverterFunc
    {
      get
      {
        return (Func<GiftsResponse, ListWithCount<GiftHeader>>) (data =>
        {
          ListWithCount<GiftHeader> listWithCount = new ListWithCount<GiftHeader>();
          VKList<GiftItemData> gifts = data.gifts;
          List<GiftItemData> giftItemDataList = gifts != null ? gifts.items :  null;
          if (giftItemDataList == null)
            return listWithCount;
          listWithCount.TotalCount = gifts.count;
          List<User> users = data.users;
          List<Group> groups = data.groups;
          List<GiftItemData>.Enumerator enumerator = giftItemDataList.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              GiftItemData current = enumerator.Current;
              IProfile profile =  null;
              long fromId = current.from_id;
              if (fromId > 0L)
                profile = users != null ?  Enumerable.FirstOrDefault<User>(users, (Func<User, bool>) (user => user.id == fromId)) :  null;
              else if (fromId < 0L)
                profile = groups != null ?  Enumerable.FirstOrDefault<Group>(groups, (Func<Group, bool>) (group => group.id == -fromId)) :  null;
              listWithCount.List.Add(new GiftHeader(current, profile, this.IsCurrentUser));
            }
          }
          finally
          {
            enumerator.Dispose();
          }
          return listWithCount;
        });
      }
    }

    public GiftsViewModel(long userId, string firstName, string firstNameGen)
    {
        this.AccessDeniedVisibility = Visibility.Collapsed;


      this._userId = userId;
      this._firstName = firstName;
      this._firstNameGen = firstNameGen;
      this.IsCurrentUser = this._userId == AppGlobalStateManager.Current.LoggedInUserId;
      this.GiftsVM = new GenericCollectionViewModel<GiftsResponse, GiftHeader>((ICollectionDataProvider<GiftsResponse, GiftHeader>) this);
      EventAggregator.Current.Subscribe(this);
    }

    public void GetData(GenericCollectionViewModel<GiftsResponse, GiftHeader> caller, int offset, int count, Action<BackendResult<GiftsResponse, ResultCode>> callback)
    {
      GiftsService.Instance.Get(this._userId, count, offset, (Action<BackendResult<GiftsResponse, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          bool giftsDenied = GiftsViewModel.IsAccessToGiftsDenied(result);
          if (string.IsNullOrEmpty(this._firstNameGen))
          {
            User user = result.ResultData.user;
            // ISSUE: explicit non-virtual call
            this._firstNameGen = user != null ? user.first_name_gen :  null;
            // ISSUE: method pointer
            Execute.ExecuteOnUIThread((Action)(() => this.NotifyPropertyChanged<string>((Expression<Func<string>>)(() => this.Title))));
          }
          this.AccessDeniedVisibility = giftsDenied.ToVisiblity();
          // ISSUE: method pointer
          Execute.ExecuteOnUIThread((Action)(() => this.NotifyPropertyChanged<Visibility>((Expression<Func<Visibility>>)(() => this.AccessDeniedVisibility))));
        }
        Action<BackendResult<GiftsResponse, ResultCode>> action = callback;
        if (action == null)
          return;
        BackendResult<GiftsResponse, ResultCode> backendResult = result;
        action(backendResult);
      }));
    }

    private static bool IsAccessToGiftsDenied(BackendResult<GiftsResponse, ResultCode> result)
    {
      List<ExecuteError> executeErrors = result.ExecuteErrors;
      if (executeErrors != null)
          return Enumerable.Any<ExecuteError>(executeErrors, (Func<ExecuteError, bool>)new Func<ExecuteError, bool>(GiftsViewModel.IsAccessToGiftsDeniedError));
      return false;
    }

    private static bool IsAccessToGiftsDeniedError(ExecuteError error)
    {
      if (error.method == "gifts.get")
        return error.error_code == 15;
      return false;
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<GiftsResponse, GiftHeader> caller, int count)
    {
      if (count == 0)
        return CommonResources.NoGifts;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.Gifts_OneFrm, CommonResources.Gifts_TwoFourFrm, CommonResources.Gifts_FiveFrm, true,  null, false);
    }

    public void Delete(GiftHeader item)
    {
      if (MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.DeleteGift, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
        return;
      this.SetInProgress(true, "");
      GiftsService.Instance.Delete(item.Id, item.GiftHash, (Action<BackendResult<int, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
      {
          this.SetInProgress(false, "");
          if (result.ResultCode == ResultCode.Succeeded)
              this.GiftsVM.Delete(item);
          else
              VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
      }))));
    }

    public void NavigateToGiftsCatalog()
    {
      Navigator.Current.NavigateToGiftsCatalog(this._userId, false);
    }

    public void Handle(GiftSentEvent message)
    {
      if (message.UserIds == null || !message.UserIds.Contains(this._userId))
        return;
      this.GiftsVM.LoadData(true, false,  null, true);
    }
  }
}
