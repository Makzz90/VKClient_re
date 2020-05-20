using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;

namespace VKClient.Common.Balance.ViewModels
{
  public class VotesPackViewModel : ViewModelBase
  {
    private bool _canPurchase = true;

    public VotesPack VotesPack { get; private set; }

    public string Title
    {
      get
      {
        return this.VotesPack.Title;
      }
    }

    public string PriceStr
    {
      get
      {
        return this.VotesPack.PriceStr;
      }
    }

    public string IconUrl
    {
      get
      {
        return string.Format("/Resources/Balance/BalanceCoin{0}.png", this.IconCoinId);
      }
    }

    public int IconCoinId { get; set; }

    public Visibility PurchaseVisibility
    {
      get
      {
        return this.CanPurchase.ToVisiblity();
      }
    }

    public Visibility ErrorVisibility
    {
      get
      {
        return (!this.CanPurchase).ToVisiblity();
      }
    }

    public bool CanPurchase
    {
      get
      {
        return this._canPurchase;
      }
      set
      {
        this._canPurchase = value;
        this.NotifyPropertyChanged("CanPurchase");
        base.NotifyPropertyChanged<Visibility>(() => this.PurchaseVisibility);
        base.NotifyPropertyChanged<Visibility>(() => this.ErrorVisibility);
      }
    }

    public VotesPackViewModel(VotesPack votesPack)
    {
      this.VotesPack = votesPack;
    }
  }
}
