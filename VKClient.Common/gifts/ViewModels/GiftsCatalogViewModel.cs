using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Gifts.ViewModels
{
  public class GiftsCatalogViewModel : ViewModelStatefulBase
  {
    private readonly long _userOrChatId;
    private readonly bool _isChat;
    private int _balance;
    private ObservableCollection<GiftsCatalogCategoryViewModel> _categories;

    public int Balance
    {
      get
      {
        return this._balance;
      }
      private set
      {
        this._balance = value;
        this.NotifyPropertyChanged("Balance");
				base.NotifyPropertyChanged<string>(() => this.BalanceStr);
				base.NotifyPropertyChanged<Visibility>(() => this.BalanceVisibility);
			}
    }

    public string BalanceStr
    {
      get
      {
        int balance = this.Balance;
        if (balance == 0)
          return "";
        return UIStringFormatterHelper.FormatNumberOfSomething(balance, CommonResources.OneVoteFrm, CommonResources.TwoFourVotesFrm, CommonResources.FiveVotesFrm, false,  null, false);
      }
    }

    public Visibility BalanceVisibility
    {
      get
      {
        return (this.Balance > 0).ToVisiblity();
      }
    }

    public ObservableCollection<GiftsCatalogCategoryViewModel> Categories
    {
      get
      {
        return this._categories;
      }
      private set
      {
        this._categories = value;
        this.NotifyPropertyChanged("Categories");
      }
    }

    public GiftsCatalogViewModel(long userOrChatId = 0, bool isChat = false)
    {
      this._userOrChatId = userOrChatId;
      this._isChat = isChat;
    }

    public override void Load(Action<ResultCode> callback)
    {
        GiftsService.Instance.GetCatalog(this._isChat ? 0L : this._userOrChatId, (Action<BackendResult<GiftsCatalogResponse, ResultCode>>)(result =>
        {
            ResultCode resultCode = result.ResultCode;
            int num1 = resultCode == ResultCode.Succeeded ? 1 : 0;
            GiftsCatalogResponse data = result.ResultData;
            if (num1 != 0 && data != null)
                Execute.ExecuteOnUIThread((Action)(() =>
                {
                    List<GiftsSection> catalog = data.catalog;
                    if (catalog != null)
                    {
                        this.Categories = new ObservableCollection<GiftsCatalogCategoryViewModel>();
                        foreach (GiftsSection section in catalog.Where<GiftsSection>((Func<GiftsSection, bool>)(s =>
                        {
                            List<GiftsSectionItem> items = s.items;
                            return (items != null ? (items.All<GiftsSectionItem>((Func<GiftsSectionItem, bool>)(item => item.IsDisabled)) ? 1 : 0) : 1) == 0;
                        })))
                            this.Categories.Add(new GiftsCatalogCategoryViewModel(section, this._userOrChatId, this._isChat));
                    }
                    this.Balance = data.balance;
                }));
            Action<ResultCode> action = callback;
            if (action == null)
                return;
            int num2 = (int)resultCode;
            action((ResultCode)num2);
        }));
    }
  }
}
