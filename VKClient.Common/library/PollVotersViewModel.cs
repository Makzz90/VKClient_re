using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class PollVotersViewModel : ViewModelBase, ICollectionDataProvider<UsersListWithCount, FriendHeader>
  {
    private readonly long _ownerId;
    private readonly long _pollId;
    private readonly long _answerId;
    private readonly string _answerText;
    private readonly GenericCollectionViewModel<UsersListWithCount, FriendHeader> _votersVM;

    public string Title
    {
      get
      {
        return this._answerText.ToUpperInvariant();
      }
    }

    public GenericCollectionViewModel<UsersListWithCount, FriendHeader> VotersVM
    {
      get
      {
        return this._votersVM;
      }
    }

    public Func<UsersListWithCount, ListWithCount<FriendHeader>> ConverterFunc
    {
      get
      {
          return (Func<UsersListWithCount, ListWithCount<FriendHeader>>)(usersList => new ListWithCount<FriendHeader>() { TotalCount = usersList.count, List = new List<FriendHeader>((IEnumerable<FriendHeader>)Enumerable.Select<User, FriendHeader>(usersList.users, (Func<User, FriendHeader>)(u => new FriendHeader(u, false)))) });
      }
    }

    public PollVotersViewModel(long ownerId, long pollId, long answerId, string answerText)
    {
      this._ownerId = ownerId;
      this._pollId = pollId;
      this._answerId = answerId;
      this._answerText = answerText;
      this._votersVM = new GenericCollectionViewModel<UsersListWithCount, FriendHeader>((ICollectionDataProvider<UsersListWithCount, FriendHeader>) this);
    }

    public void LoadData()
    {
      this._votersVM.LoadData(false, false,  null, false);
    }

    public void GetData(GenericCollectionViewModel<UsersListWithCount, FriendHeader> caller, int offset, int count, Action<BackendResult<UsersListWithCount, ResultCode>> callback)
    {
      PollService.Current.GetVoters(this._ownerId, this._pollId, this._answerId, offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<UsersListWithCount, FriendHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoVotersYet;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true,  null, false);
    }
  }
}
