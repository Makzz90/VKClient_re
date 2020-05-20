using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Gifts.ViewModels
{
  public class GiftsCatalogCategoryPageViewModel : ViewModelStatefulBase
  {
    private readonly long _userOrChatId;
    private readonly bool _isChat;
    private readonly string _categoryName;
    private ObservableCollection<GiftsThreeInARowViewModel> _giftsRows;

    public string Title { get;set; }

    public ObservableCollection<GiftsThreeInARowViewModel> GiftsRows
    {
      get
      {
        return this._giftsRows;
      }
      private set
      {
        this._giftsRows = value;
        this.NotifyPropertyChanged("GiftsRows");
      }
    }

    public GiftsCatalogCategoryPageViewModel(string categoryName, string title, long userOrChatId = 0, bool isChat = false)
    {
      this._categoryName = categoryName;
      this._userOrChatId = userOrChatId;
      this._isChat = isChat;
      this.Title = title.ToUpperInvariant();
    }
      /*
    public override void Load(Action<ResultCode> callback)
    {
      GiftsService.Instance.GetCatalog(this._userOrChatId, this._categoryName, (Action<BackendResult<List<GiftsSection>, ResultCode>>) (result =>
      {
        // ISSUE: object of a compiler-generated type is created
        // ISSUE: variable of a compiler-generated type
//        GiftsCatalogCategoryPageViewModel cDisplayClass110 = new GiftsCatalogCategoryPageViewModel();
        // ISSUE: reference to a compiler-generated field
//        cDisplayClass110 = this;


        ResultCode resultCode = result.ResultCode;
        int num1 = resultCode == ResultCode.Succeeded ? 1 : 0;
        List<GiftsSection> resultData = result.ResultData;
        List<GiftsSectionItem> giftsSectionItemList;
        if (resultData == null)
        {
          giftsSectionItemList =  null;
        }
        else
        {
          GiftsSection m0 = Enumerable.FirstOrDefault<GiftsSection>(resultData);
          giftsSectionItemList = m0 != null ? ((GiftsSection) m0).items :  null;
        }
        // ISSUE: reference to a compiler-generated field
        List<GiftsSectionItem> sectionItems = giftsSectionItemList;
        // ISSUE: reference to a compiler-generated field
        if (num1 != 0 && sectionItems != null)
        {
          // ISSUE: method pointer
            Execute.ExecuteOnUIThread((Action)(() =>
            {
                this.GiftsRows = new ObservableCollection<GiftsThreeInARowViewModel>();
                //foreach (IEnumerable<GiftsSectionItem> source in sectionItems.Where<GiftsSectionItem>(  (Func<GiftsSectionItem, bool>)(item => !item.IsDisabled)  ).Partition<GiftsSectionItem>(3))
                foreach (IEnumerable<GiftsSectionItem> source in sectionItems.Where<GiftsSectionItem>(VKClient.Common.Utils.ListExtensions.Partition <GiftsSectionItem>(     ((Func<GiftsSectionItem, bool>)(item => !item.IsDisabled)),3     )))
                {
                    List<GiftsSectionItem> list = source.ToList<GiftsSectionItem>();
                    if (list.Count != 0)
                    {
                        GiftsSectionItem section1 = list[0];
                        GiftsSectionItem section2 = null;
                        GiftsSectionItem section3 = null;
                        if (list.Count > 1)
                            section2 = list[1];
                        if (list.Count > 2)
                            section3 = list[2];
                        this.GiftsRows.Add(new GiftsThreeInARowViewModel(this._categoryName, section1, section2, section3, this._userOrChatId, this._isChat));
                    }
                }
            }));
        }
        Action<ResultCode> action = callback;
        if (action == null)
          return;
        int num2 = (int) resultCode;
        action((ResultCode) num2);
      }));
    }*/

      public override void Load(Action<ResultCode> callback)
{
	//GiftsCatalogCategoryPageViewModel.<>c__DisplayClass11_1 <>c__DisplayClass11_ = new GiftsCatalogCategoryPageViewModel.<>c__DisplayClass11_1();
	//<>c__DisplayClass11_.<>4__this = this;
	//<>c__DisplayClass11_.callback = callback;
	GiftsService.Instance.GetCatalog(this._userOrChatId, this._categoryName, delegate(BackendResult<List<GiftsSection>, ResultCode> result)
	{
		GiftsCatalogCategoryPageViewModel c__DisplayClass11_2 = this;
		//<>c__DisplayClass11_2.CS$<>8__locals1 = <>c__DisplayClass11_;
		ResultCode resultCode = result.ResultCode;
		bool arg_3C_0 = resultCode == ResultCode.Succeeded;
		GiftsCatalogCategoryPageViewModel arg_37_0 = c__DisplayClass11_2;
		List<GiftsSection> expr_1F = result.ResultData;
		List<GiftsSectionItem> arg_37_1;
		if (expr_1F == null)
		{
			arg_37_1 = null;
		}
		else
		{
			GiftsSection expr_2B = Enumerable.FirstOrDefault<GiftsSection>(expr_1F);
			arg_37_1 = ((expr_2B != null) ? expr_2B.items : null);
		}
		//arg_37_0.sectionItems = arg_37_1;
        
		if (arg_3C_0 && arg_37_1 != null)
		{
			Execute.ExecuteOnUIThread(delegate
			{
				this.GiftsRows = new ObservableCollection<GiftsThreeInARowViewModel>();
				IEnumerable<GiftsSectionItem> arg_3A_0 = arg_37_1;
				Func<GiftsSectionItem, bool> arg_3A_1 = new Func<GiftsSectionItem, bool>((item)=>{return !item.IsDisabled;});
				
				//using (IEnumerator<IEnumerable<GiftsSectionItem>> enumerator = Enumerable.Where<GiftsSectionItem>(arg_3A_0, arg_3A_1).Partition(3).GetEnumerator())
                
                using (IEnumerator<IEnumerable<GiftsSectionItem>> enumerator = VKClient.Common.Utils.ListExtensions.Partition<GiftsSectionItem>(arg_3A_0, 3).GetEnumerator())//todo: bug
				{
					while (enumerator.MoveNext())
					{
						List<GiftsSectionItem> list = Enumerable.ToList<GiftsSectionItem>(enumerator.Current);
						if (list.Count != 0)
						{
							GiftsSectionItem section = list[0];
							GiftsSectionItem section2 = null;
							GiftsSectionItem section3 = null;
							if (list.Count > 1)
							{
								section2 = list[1];
							}
							if (list.Count > 2)
							{
								section3 = list[2];
							}
							this.GiftsRows.Add(new GiftsThreeInARowViewModel(this._categoryName, section, section2, section3, this._userOrChatId, this._isChat));
						}
					}
				}
			});
		}
		Action<ResultCode> expr_5D = callback;
		if (expr_5D == null)
		{
			return;
		}
		expr_5D.Invoke(resultCode);
	});
}
  }
}
