using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;

namespace VKClient.Common.MoneyTransfers.Library
{
  public sealed class SendMoneyViewModel : ViewModelBase
  {
    private readonly long _targetId;
    private User _targetUser;
    private readonly string _initialAmount;
    private readonly string _initialComment;
    private bool _isLoading;
    private string _comment;
    private string _amount;
    private bool _isSending;

    public string Photo
    {
      get
      {
        User targetUser = this._targetUser;
        if (targetUser == null)
          return  null;
        return targetUser.photo_max;
      }
    }

    public string Title
    {
      get
      {
        User targetUser = this._targetUser;
        if (targetUser == null)
          return  null;
        return targetUser.first_name;
      }
    }

    public string Comment
    {
      get
      {
        return this._comment;
      }
      set
      {
        this._comment = value;
        this.NotifyPropertyChanged("Comment");
      }
    }

    public string Amount
    {
      get
      {
        return this._amount;
      }
      set
      {
        int result;
        int.TryParse(value, out result);
        this._amount = result != 0 ? result.ToString() : "";
        this.NotifyPropertyChanged<string>(() => this.Amount);
        this.NotifyPropertyChanged<string>(() => this.AmountLimitsHint);
        this.NotifyPropertyChanged<Brush>(() => this.AmountLimitsHintForeground);
      }
    }

    public string AmountLimitsHint
    {
      get
      {
        int result;
        int.TryParse(this.Amount, out result);
        int transferMinAmount = AppGlobalStateManager.Current.GlobalState.MoneyTransferMinAmount;
        int transferMaxAmount = AppGlobalStateManager.Current.GlobalState.MoneyTransferMaxAmount;
        string maxAmountString = SendMoneyViewModel.GetMaxAmountString(transferMaxAmount);
        if (result < transferMinAmount)
          return string.Format("{0} {1} {2}", CommonResources.Minimum, transferMinAmount, CommonResources.Currency_RUB);
        if (result > transferMaxAmount)
          return string.Format("{0} {1} {2}", CommonResources.Maximum, maxAmountString, CommonResources.Currency_RUB);
        return "";
      }
    }

    public Brush AmountLimitsHintForeground
    {
      get
      {
        int result;
        if (int.TryParse(this.Amount, out result) && result > AppGlobalStateManager.Current.GlobalState.MoneyTransferMaxAmount)
          return (Brush) Application.Current.Resources["PhoneAccentRedBrush"];
        return (Brush) Application.Current.Resources["PhoneGray300_Gray600Brush"];
      }
    }

    public int MaxAmountLength
    {
      get
      {
        return ((string) AppGlobalStateManager.Current.GlobalState.MoneyTransferMaxAmount.ToString()).Length + 1;
      }
    }

    public bool IsReadyForSending
    {
      get
      {
        if (!string.IsNullOrEmpty(this.Amount) && int.Parse(this.Amount) >= AppGlobalStateManager.Current.GlobalState.MoneyTransferMinAmount)
          return int.Parse(this.Amount) <= AppGlobalStateManager.Current.GlobalState.MoneyTransferMaxAmount;
        return false;
      }
    }

    public SendMoneyViewModel(long targetId, User targetUser, int amount, string comment)
    {
      this._targetId = targetId;
      this._targetUser = targetUser;
      this._initialAmount = amount.ToString();
      this._initialComment = comment;
    }

    public void Load()
    {
      if (this._isLoading)
        return;
      this.Amount = this._initialAmount;
      this.Comment = this._initialComment;
      if (this._targetUser != null)
        return;
      this._isLoading = true;
      if (this._targetId > 0L)
      {
        UsersService instance = UsersService.Instance;
        List<long> longList = new List<long>();
        long targetId = this._targetId;
        longList.Add(targetId);
        Action<BackendResult<List<User>, ResultCode>> action = (Action<BackendResult<List<User>, ResultCode>>) (result =>
        {
          this._isLoading = false;
          if (result.ResultCode != ResultCode.Succeeded)
            return;
          this._targetUser = result.ResultData.FirstOrDefault<User>();
          // ISSUE: method reference
          this.NotifyPropertyChanged<string>(() => this.Photo);
          // ISSUE: method reference
          this.NotifyPropertyChanged<string>(() => this.Title);
        });
        instance.GetUsers(longList, action);
      }
      else
        GroupsService.Current.GetCommunity(-this._targetId, "", (Action<BackendResult<List<Group>, ResultCode>>) (result =>
        {
          this._isLoading = false;
          if (result.ResultCode != ResultCode.Succeeded)
            return;
          Group group = result.ResultData.FirstOrDefault<Group>();
          if (group != null)
            this._targetUser = new User()
            {
              id = this._targetId,
              first_name = group.name,
              photo_max = group.photo_max
            };
          // ISSUE: method reference
          this.NotifyPropertyChanged<string>(() => this.Photo);
          // ISSUE: method reference
          this.NotifyPropertyChanged<string>(() => this.Title);
        }));
    }

    private static string GetMaxAmountString(int maxAmount)
    {
      NumberFormatInfo numberFormatInfo = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
      numberFormatInfo.NumberGroupSeparator = " ";
      return maxAmount.ToString("#,#", (IFormatProvider) numberFormatInfo);
    }

    public void Send()
    {
        if (this._isSending)
            return;
        this._isSending = true;
        this.SetInProgress(true, "");
        MoneyTransfersService.SendTransfer(this._targetId, int.Parse(this.Amount), this.Comment, (Action<BackendResult<MoneyTransferSendResponse, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
        {
            this._isSending = false;
            this.SetInProgress(false, "");
            if (result.ResultCode != ResultCode.Succeeded)
                VKClient.Common.UC.GenericInfoUC.ShowBasedOnResult((int)result.ResultCode, "", null);
            else
                Navigator.Current.NavigateToMoneyTransferSendConfirmation(result.ResultData.redirect_uri);
        }))));
    }
  }
}
