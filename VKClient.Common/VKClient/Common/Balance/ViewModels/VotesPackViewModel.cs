using System;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;

namespace VKClient.Common.Balance.ViewModels
{
  public class VotesPackViewModel : ViewModelBase
  {
    private bool _canPurchase = true;

    public VotesPack VotesPack { get; set; }

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
        return string.Format("/Resources/Balance/BalanceCoin{0}.png", (object) this.IconCoinId);
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
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.PurchaseVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ErrorVisibility));
      }
    }

    public VotesPackViewModel(VotesPack votesPack)
    {
      this.VotesPack = votesPack;
    }
  }
}
