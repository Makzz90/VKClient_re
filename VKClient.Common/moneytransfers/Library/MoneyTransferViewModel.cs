using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.MoneyTransfers.Library
{
  public sealed class MoneyTransferViewModel : ViewModelBase, IHandle<MoneyTransferDeclinedEvent>, IHandle, IHandle<MoneyTransferAcceptedEvent>
  {
      public MoneyTransfer Transfer { get; private set; }

      public User User { get; private set; }

    public string Photo
    {
      get
      {
        return this.User.photo_max;
      }
    }

    public string Date
    {
      get
      {
        return UIStringFormatterHelper.FormatDateTimeForUI(Extensions.UnixTimeStampToDateTime((double) this.Transfer.date, true));
      }
    }

    public int Amount
    {
      get
      {
        return this.Transfer.amount.amount / 100;
      }
    }

    public string Title
    {
      get
      {
        if (!this.Transfer.IsOutbox)
          return string.Format("{0} {1}", CommonResources.By, this.User.id > 0L ? this.User.NameGen : this.User.Name);
        if (this.User.id <= 0L)
          return this.User.Name;
        return this.User.NameDat;
      }
    }

    public string DeltaSymbol
    {
      get
      {
        return !this.Transfer.IsOutbox ? "+" : "−";
      }
    }

    public string AmountDelta
    {
      get
      {
        return string.Format("{0} {1}", this.DeltaSymbol, this.Transfer.amount.text);
      }
    }

    public string Comment
    {
      get
      {
        string comment = this.Transfer.comment;
        if (comment == null)
          return  null;
        return Extensions.ForUI(comment);
      }
    }

    public Visibility CommentVisibility
    {
      get
      {
        if (string.IsNullOrWhiteSpace(this.Comment))
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public string AmountStr
    {
      get
      {
        NumberFormatInfo numberFormatInfo = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
        numberFormatInfo.NumberGroupSeparator = " ";
        return this.Amount.ToString("#,#", (IFormatProvider) numberFormatInfo);
      }
    }

    public SolidColorBrush AmountForeground
    {
      get
      {
        string str;
        switch (this.Transfer.status)
        {
          case 0:
            str = "PhoneCaptionGrayBrush";
            break;
          case 1:
            str = "PhoneAlmostBlackBrush";
            break;
          case 2:
            str = "PhoneAccentRedBrush";
            break;
          default:
            return  null;
        }
        return (SolidColorBrush) Application.Current.Resources[str];
      }
    }

    public string State
    {
      get
      {
        if (this.Transfer.status == 0)
        {
          if (!this.Transfer.IsOutbox)
            return "";
          return CommonResources.InGettingAwaits;
        }
        if (!this.Transfer.IsCompleted)
          return CommonResources.IsCancelled;
        return CommonResources.IsGetted;
      }
    }

    public Visibility StateVisibility
    {
      get
      {
        return (!string.IsNullOrWhiteSpace(this.State)).ToVisiblity();
      }
    }

    public SolidColorBrush StateForeground
    {
      get
      {
        string str;
        switch (this.Transfer.status)
        {
          case 0:
            str = "PhoneCaptionGrayBrush";
            break;
          case 1:
            str = "PhoneButtonGreenForegroundBrush";
            break;
          case 2:
            str = "PhoneAccentRedBrush";
            break;
          default:
            return  null;
        }
        return (SolidColorBrush) Application.Current.Resources[str];
      }
    }

    public string StateIcon
    {
      get
      {
        string str;
        switch (this.Transfer.status)
        {
          case 0:
            str = "Waiting";
            break;
          case 1:
            str = "Completed";
            break;
          case 2:
            str = "Cancelled";
            break;
          default:
            return  null;
        }
        return string.Format("/Resources/MoneyTransfers/{0}24px.png", str);
      }
    }

    public Visibility ButtonsVisibility
    {
      get
      {
        return this.Transfer.IsAwaits.ToVisiblity();
      }
    }

    public Visibility AcceptButtonVisibility
    {
      get
      {
        return (!this.Transfer.IsOutbox).ToVisiblity();
      }
    }

    public string DeclineButtonTitle
    {
      get
      {
        if (!this.Transfer.IsOutbox)
          return CommonResources.DeclineJoinRequest;
        return CommonResources.Cancel;
      }
    }

    public Visibility CardButtonsVisibility
    {
      get
      {
        return (!this.Transfer.IsOutbox && this.Transfer.IsAwaits).ToVisiblity();
      }
    }

    public Visibility ActionButtonVisibility
    {
      get
      {
        return this.Transfer.IsOutbox.ToVisiblity();
      }
    }

    public Visibility ActionButtonRepeatIconVisibility
    {
      get
      {
        return (!this.Transfer.IsAwaits).ToVisiblity();
      }
    }

    public string ActionButtonTitle
    {
      get
      {
        if (!this.Transfer.IsAwaits)
          return CommonResources.RepeatTransfer;
        return CommonResources.CancelTransfer;
      }
    }

    public string Status
    {
      get
      {
        if (!this.Transfer.IsAwaits)
          return CommonResources.IsCancelled;
        return CommonResources.Awaits;
      }
    }

    public Visibility StatusVisibility
    {
      get
      {
        return (this.Transfer.IsOutbox && this.Transfer.IsAwaits || this.Transfer.IsCancelled).ToVisiblity();
      }
    }

    public long UserId
    {
      get
      {
        return this.User.id;
      }
    }

    public bool IsOutbox
    {
      get
      {
        return this.Transfer.IsOutbox;
      }
    }

    public MoneyTransferViewModel(MoneyTransfer transfer, User user)
    {
      this.Transfer = transfer;
      this.User = user;
      EventAggregator.Current.Subscribe(this);
    }

    public void Handle(MoneyTransferDeclinedEvent message)
    {
      this.HandleUpdate(message.TransferId, message.FromId, message.ToId, false);
    }

    public void Handle(MoneyTransferAcceptedEvent message)
    {
      this.HandleUpdate(message.TransferId, message.FromId, message.ToId, true);
    }

    private void HandleUpdate(long transferId, long fromId, long toId, bool accepted)
    {
      if (this.Transfer == null || !this.Transfer.IsEquals(transferId, fromId, toId))
        return;
      this.Transfer.status = accepted ? 1 : 2;
      base.NotifyPropertyChanged<SolidColorBrush>(() => this.AmountForeground);
      base.NotifyPropertyChanged<string>(() => this.State);
      base.NotifyPropertyChanged<Visibility>(() => this.StateVisibility);
      base.NotifyPropertyChanged<SolidColorBrush>(() => this.StateForeground);
      base.NotifyPropertyChanged<string>(() => this.StateIcon);
      base.NotifyPropertyChanged<Visibility>(() => this.ButtonsVisibility);
      base.NotifyPropertyChanged<Visibility>(() => this.CardButtonsVisibility);
      base.NotifyPropertyChanged<Visibility>(() => this.ActionButtonRepeatIconVisibility);
      base.NotifyPropertyChanged<string>(() => this.ActionButtonTitle);
      base.NotifyPropertyChanged<string>(() => this.Status);
      base.NotifyPropertyChanged<Visibility>(() => this.StatusVisibility);
    }

    public void RetryTransfer()
    {
      Navigator.Current.NavigateToSendMoneyPage(this.User.id, this.User, this.Amount, this.Comment);
    }

    public void AcceptTransfer()
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
//      MoneyTransferViewModel.<>c__DisplayClass61_1 cDisplayClass611_1 = new MoneyTransferViewModel.<>c__DisplayClass61_1();
      // ISSUE: reference to a compiler-generated field
//      cDisplayClass611_1.<>4__this = this;
      if (this.Transfer == null)
        return;
      // ISSUE: reference to a compiler-generated field
      string acceptUrl = this.Transfer.accept_url;
      // ISSUE: reference to a compiler-generated field
      if (!string.IsNullOrEmpty(acceptUrl))
      {
        // ISSUE: reference to a compiler-generated field
        this.AcceptTransfer(acceptUrl);
      }
      else
      {
        FullscreenLoader loader = new FullscreenLoader();
        loader.Show( null, true);
        MoneyTransfersService.CheckTransfer(this.Transfer.id, this.Transfer.from_id, this.Transfer.to_id, (Action<BackendResult<MoneyTransfer, ResultCode>>) (result =>
        {
          loader.Hide(false);
          ResultCode resultCode = result.ResultCode;
          if (resultCode == ResultCode.Succeeded)
          {
            // ISSUE: variable of a compiler-generated type
//            MoneyTransferViewModel.<>c__DisplayClass61_1 cDisplayClass611_2 = cDisplayClass611_1;
            MoneyTransfer resultData = result.ResultData;
            string str = resultData != null ? resultData.accept_url :  null;
            // ISSUE: reference to a compiler-generated field
            acceptUrl = str;
            if (string.IsNullOrEmpty(acceptUrl))
              return;
            this.AcceptTransfer(acceptUrl);
          }
          else
            GenericInfoUC.ShowBasedOnResult(resultCode, "", null);
        }));
      }
    }

    private void AcceptTransfer(string acceptUrl)
    {
      if (string.IsNullOrEmpty(acceptUrl))
        return;
      Navigator.Current.NavigateToMoneyTransferAcceptConfirmation(acceptUrl, this.Transfer.id, this.Transfer.from_id, this.Transfer.to_id);
    }

    public void DeclineTransfer(Action callback = null)
    {
      FullscreenLoader loader = new FullscreenLoader();
      loader.Show( null, true);
      MoneyTransfersService.DeclineTransfer(this.Transfer.id, (Action<BackendResult<int, ResultCode>>) (result =>
      {
        loader.Hide(false);
        ResultCode resultCode = result.ResultCode;
        if (resultCode == ResultCode.Succeeded)
        {
          this.Transfer.status = 2;
          EventAggregator.Current.Publish(new MoneyTransferDeclinedEvent(this.Transfer));
          Action action = callback;
          if (action == null)
            return;
          action.Invoke();
        }
        else
          GenericInfoUC.ShowBasedOnResult(resultCode, "", (VKRequestsDispatcher.Error) null);
      }));
    }

    public void CancelTransfer(Action callback = null)
    {
      FullscreenLoader loader = new FullscreenLoader();
      loader.Show( null, true);
      MoneyTransfersService.DeclineTransfer(this.Transfer.id, (Action<BackendResult<int, ResultCode>>) (result =>
      {
        loader.Hide(false);
        ResultCode resultCode = result.ResultCode;
        if (resultCode == ResultCode.Succeeded)
        {
          this.Transfer.status = 2;
          EventAggregator.Current.Publish(new MoneyTransferDeclinedEvent(this.Transfer));
          Action action = callback;
          if (action == null)
            return;
          action.Invoke();
        }
        else
          GenericInfoUC.ShowBasedOnResult(resultCode, "", (VKRequestsDispatcher.Error) null);
      }));
    }
  }
}
