using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Shared;
using VKClient.Common.Utils;
using VKClient.Video.VideoCatalog;

namespace VKClient.Common.VideoCatalog
{
  public class CatalogCategoryViewModel : ViewModelBase, IHandle<GroupMembershipStatusUpdated>, IHandle
  {
    private ObservableCollection<CatalogItemViewModel> _catalogItems = new ObservableCollection<CatalogItemViewModel>();
    private ObservableCollection<CatalogItemTwoInARowViewModel> _catalogItemsTwoInARow = new ObservableCollection<CatalogItemTwoInARowViewModel>();
    private CollectionObject<ListHeaderViewModel> _coHeader = new CollectionObject<ListHeaderViewModel>();
    private CollectionObject<CategoryMoreFooter> _coFooter = new CollectionObject<CategoryMoreFooter>();
    private CollectionObject<CatalogItemsHorizontalViewModel> _coCatalogItemsHorizontalVM = new CollectionObject<CatalogItemsHorizontalViewModel>();
    private MergedCollection _itemsWithHeaderAndFooter = new MergedCollection();
    public const int LOAD_COUNT = 3;
    public const int LOAD_MORE_COUNT = 5;
    private VideoCatalogCategory _videoCategory;
    private List<User> _knownUsers;
    private List<Group> _knownGroups;
    private bool _handledShowMore;
    private MenuItemData _midSubscribeUnsubscribe;

    public MergedCollection CatalogItemsWithHeaderAndFooter
    {
      get
      {
        return this._itemsWithHeaderAndFooter;
      }
    }

    public CollectionObject<ListHeaderViewModel> Header
    {
      get
      {
        return this._coHeader;
      }
    }

    public CollectionObject<CategoryMoreFooter> Footer
    {
      get
      {
        return this._coFooter;
      }
    }

    public CollectionObject<CatalogItemsHorizontalViewModel> CatalogItemsHorizontalVM
    {
      get
      {
        return this._coCatalogItemsHorizontalVM;
      }
    }

    public bool IsHorizontal
    {
      get
      {
        if (!(this._videoCategory.view == "horizontal"))
          return this._videoCategory.view == "horizontal_compact";
        return true;
      }
    }

    public bool IsVerticalCompact
    {
      get
      {
        return this._videoCategory.view == "vertical_compact";
      }
    }

    private long OwnerId
    {
      get
      {
        long result = 0;
        if (this._videoCategory != null)
          long.TryParse(this._videoCategory.id, out result);
        return result;
      }
    }

    private bool AllowFeedNavigateToAll
    {
      get
      {
        return this._videoCategory.items.Count > 1;
      }
    }

    public CatalogCategoryViewModel(VideoCatalogCategory c, List<User> knownUsers, List<Group> knownGroups)
    {
      this._videoCategory = c;
      this._knownUsers = knownUsers;
      this._knownGroups = knownGroups;
      EventAggregator.Current.Subscribe((object) this);
      this.Initialize();
    }

    private void Initialize()
    {
      this.Header.Data = this.CreateHeader();
      this._itemsWithHeaderAndFooter.Merge((INotifyCollectionChanged) this._coHeader.Coll);
      if (!this.IsHorizontal)
      {
        if (!this.IsVerticalCompact)
        {
          foreach (VideoCatalogItem videoCatalogItem in this._videoCategory.items.Take<VideoCatalogItem>(3))
            this._catalogItems.Add(this.CreateCatalogItemViewModel(videoCatalogItem));
          this._itemsWithHeaderAndFooter.Merge((INotifyCollectionChanged) this._catalogItems);
        }
        else
        {
          foreach (IEnumerable<VideoCatalogItem> source in this._videoCategory.items.Take<VideoCatalogItem>(6).Partition<VideoCatalogItem>(2))
          {
            CatalogItemTwoInARowViewModel twoInArowViewModel = new CatalogItemTwoInARowViewModel();
            twoInArowViewModel.Item1 = this.CreateCatalogItemViewModel(source.First<VideoCatalogItem>());
            if (source.Count<VideoCatalogItem>() > 1)
              twoInArowViewModel.Item2 = this.CreateCatalogItemViewModel(source.Skip<VideoCatalogItem>(1).First<VideoCatalogItem>());
            this._catalogItemsTwoInARow.Add(twoInArowViewModel);
          }
          this._itemsWithHeaderAndFooter.Merge((INotifyCollectionChanged) this._catalogItemsTwoInARow);
        }
        this._itemsWithHeaderAndFooter.Merge((INotifyCollectionChanged) CollectionObject<DividerSpaceDownViewModel>.CreateCollectionObject<DividerSpaceDownViewModel>(new DividerSpaceDownViewModel()).Coll);
      }
      else
      {
        this.CatalogItemsHorizontalVM.Data = new CatalogItemsHorizontalViewModel(this._videoCategory, this._knownUsers, this._knownGroups);
        this._itemsWithHeaderAndFooter.Merge((INotifyCollectionChanged) this.CatalogItemsHorizontalVM.Coll);
      }
      this.Footer.Data = this.CreateFooter();
      this._itemsWithHeaderAndFooter.Merge((INotifyCollectionChanged) this._coFooter.Coll);
    }

    private CategoryMoreFooter CreateFooter()
    {
      if (this.IsHorizontal || (this.IsVerticalCompact || this._videoCategory.items.Count <= 3) && (!this.IsVerticalCompact || this._videoCategory.items.Count <= 6))
        return (CategoryMoreFooter) null;
      CategoryMoreFooter categoryMoreFooter = new CategoryMoreFooter();
      categoryMoreFooter.ShowMoreVisibility = Visibility.Visible;
      categoryMoreFooter.ShowAllVisibility = Visibility.Collapsed;
      Action action = (Action) (() =>
      {
        if (!this._handledShowMore)
        {
          if (!this.IsVerticalCompact)
          {
            foreach (VideoCatalogItem videoCatalogItem in this._videoCategory.items.Skip<VideoCatalogItem>(3).Take<VideoCatalogItem>(5))
              this._catalogItems.Add(this.CreateCatalogItemViewModel(videoCatalogItem));
          }
          else
          {
            foreach (IEnumerable<VideoCatalogItem> source in this._videoCategory.items.Skip<VideoCatalogItem>(6).Take<VideoCatalogItem>(10).Partition<VideoCatalogItem>(2))
            {
              CatalogItemTwoInARowViewModel twoInArowViewModel = new CatalogItemTwoInARowViewModel();
              twoInArowViewModel.Item1 = this.CreateCatalogItemViewModel(source.First<VideoCatalogItem>());
              if (source.Count<VideoCatalogItem>() > 1)
                twoInArowViewModel.Item2 = this.CreateCatalogItemViewModel(source.Skip<VideoCatalogItem>(1).First<VideoCatalogItem>());
              this._catalogItemsTwoInARow.Add(twoInArowViewModel);
            }
          }
          this.Footer.Data.ShowAllVisibility = Visibility.Visible;
          this.Footer.Data.ShowMoreVisibility = Visibility.Collapsed;
          this._handledShowMore = true;
        }
        else
          this.HandleShowAllAction();
      });
      categoryMoreFooter.HandleTap = action;
      return categoryMoreFooter;
    }

    private CatalogItemViewModel CreateCatalogItemViewModel(VideoCatalogItem item)
    {
      return new CatalogItemViewModel(item, this._knownUsers, this._knownGroups, false)
      {
        ActionSource = new StatisticsActionSource?(StatisticsActionSource.video_catalog),
        VideoContext = this._videoCategory.id
      };
    }
      /*
    private ListHeaderViewModel CreateHeader()
    {
      ListHeaderViewModel listHeaderViewModel = new ListHeaderViewModel();
      listHeaderViewModel.ShowAllVisibility = this.IsHorizontal ? Visibility.Visible : Visibility.Collapsed;
      listHeaderViewModel.OnHeaderTap = new Action(this.HandleShowAllAction);
      string id = this._videoCategory.id;
      // ISSUE: reference to a compiler-generated method
      uint stringHash = \u003CPrivateImplementationDetails\u003E.ComputeStringHash(id);
      if (stringHash <= 822911587U)
      {
        if (stringHash <= 434841374U)
        {
          if (stringHash <= 350953279U)
          {
            if ((int) stringHash != 216585232)
            {
              if ((int) stringHash != 334175660)
              {
                if ((int) stringHash != 350953279 || !(id == "19"))
                  goto label_54;
              }
              else if (!(id == "18"))
                goto label_54;
            }
            else if (!(id == "69"))
              goto label_54;
          }
          else if ((int) stringHash != 384361422)
          {
            if ((int) stringHash != 418063755)
            {
              if ((int) stringHash != 434841374 || !(id == "16"))
                goto label_54;
            }
            else if (!(id == "15"))
              goto label_54;
          }
          else if (!(id == "63"))
            goto label_54;
        }
        else if (stringHash <= 501951850U)
        {
          if ((int) stringHash != 451618993)
          {
            if ((int) stringHash != 468396612)
            {
              if ((int) stringHash != 501951850 || !(id == "12"))
                goto label_54;
            }
            else if (!(id == "10"))
              goto label_54;
          }
          else if (!(id == "17"))
            goto label_54;
        }
        else if ((int) stringHash != 518729469)
        {
          if ((int) stringHash != 806133968)
          {
            if ((int) stringHash != 822911587 || !(id == "4"))
              goto label_54;
          }
          else if (!(id == "5"))
            goto label_54;
        }
        else if (!(id == "13"))
          goto label_54;
      }
      else if (stringHash <= 1007465396U)
      {
        if (stringHash <= 873244444U)
        {
          if ((int) stringHash != 839689206)
          {
            if ((int) stringHash != 856466825)
            {
              if ((int) stringHash != 873244444 || !(id == "1"))
                goto label_54;
            }
            else if (!(id == "6"))
              goto label_54;
          }
          else if (!(id == "7"))
            goto label_54;
        }
        else if ((int) stringHash != 906799682)
        {
          if ((int) stringHash != 923577301)
          {
            if ((int) stringHash != 1007465396 || !(id == "9"))
              goto label_54;
          }
          else if (!(id == "2"))
            goto label_54;
        }
        else if (!(id == "3"))
          goto label_54;
      }
      else if (stringHash <= 1304646146U)
      {
        if ((int) stringHash != 1024243015)
        {
          if ((int) stringHash != 1278082707)
          {
            if ((int) stringHash == 1304646146 && id == "ugc")
            {
              listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosPopular.png";
              goto label_56;
            }
            else
              goto label_54;
          }
          else if (id == "my")
          {
            listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosMy.png";
            goto label_56;
          }
          else
            goto label_54;
        }
        else if (!(id == "8"))
          goto label_54;
      }
      else if ((int) stringHash != 1865670964)
      {
        if ((int) stringHash != -1422563803)
        {
          if ((int) stringHash == -591183440 && id == "series")
          {
            listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosShows.png";
            goto label_56;
          }
          else
            goto label_54;
        }
        else if (id == "feed")
        {
          if (!this.AllowFeedNavigateToAll)
          {
            listHeaderViewModel.ShowAllVisibility = Visibility.Collapsed;
            listHeaderViewModel.OnHeaderTap = (Action) null;
          }
          listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosUpdates.png";
          goto label_56;
        }
        else
          goto label_54;
      }
      else if (!(id == "108"))
        goto label_54;
      listHeaderViewModel.IconUri = string.Format("..\\Resources\\VideoCatalog\\Categories\\VideosCat{0}.png", (object) this._videoCategory.id);
      goto label_56;
label_54:
      if (this._videoCategory.type == "channel")
      {
        string imageUriFor = this.GetImageUriFor(this.OwnerId);
        listHeaderViewModel.ImageUri = imageUriFor;
      }
label_56:
      if (!string.IsNullOrWhiteSpace(this._videoCategory.name))
        listHeaderViewModel.Title = this._videoCategory.name;
      List<MenuItemData> menuItemDataList = new List<MenuItemData>();
      if (this._videoCategory.type == "channel" && this.OwnerId < 0L)
      {
        Group gr = this._knownGroups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this.OwnerId));
        if (gr != null)
        {
          this._midSubscribeUnsubscribe = new MenuItemData()
          {
            Title = gr.IsMember ? CommonResources.GroupPage_Unfollow.ToLowerInvariant() : CommonResources.Group_Follow.ToLowerInvariant()
          };
          this._midSubscribeUnsubscribe.OnTap = (Action) (() => new OwnerHeaderWithSubscribeViewModel(gr).SubscribeUnsubscribe((Action<bool>) null));
          menuItemDataList.Add(this._midSubscribeUnsubscribe);
        }
      }
      if (this._videoCategory.can_hide == 1)
      {
        MenuItemData menuItemData1 = new MenuItemData();
        menuItemData1.Title = CommonResources.VideoCatalog_HideCategory;
        Action action = (Action) (() =>
        {
          int result = 0;
          if (!int.TryParse(this._videoCategory.id, out result))
            return;
          VideoService.Instance.HideCatalogSection(result, (Action<BackendResult<ResponseWithId, ResultCode>>) (res => {}));
          this.CatalogItemsWithHeaderAndFooter.Clear();
        });
        menuItemData1.OnTap = action;
        MenuItemData menuItemData2 = menuItemData1;
        menuItemDataList.Add(menuItemData2);
      }
      if (menuItemDataList.Count > 0)
      {
        listHeaderViewModel.MenuItemDataList = menuItemDataList;
        listHeaderViewModel.ShowMoreActionsVisibility = Visibility.Visible;
      }
      return listHeaderViewModel;
    }
      */
    private ListHeaderViewModel CreateHeader()
    {
        ListHeaderViewModel listHeaderViewModel = new ListHeaderViewModel();
        listHeaderViewModel.ShowAllVisibility = this.IsHorizontal ? Visibility.Visible : Visibility.Collapsed;
        listHeaderViewModel.OnHeaderTap = new Action(this.HandleShowAllAction);
        string id = this._videoCategory.id;

        System.Diagnostics.Debug.WriteLine("ListHeaderViewModel.CreateHeader " + id);

        //uint stringHash = 0;// PrivateImplementationDetails.ComputeStringHash(id);
        /*if (stringHash <= 822911587U)
        {
            if (stringHash <= 434841374U)
            {
                if (stringHash <= 350953279U)
                {
                    if ((int)stringHash != 216585232)
                    {
                        if ((int)stringHash != 334175660)
                        {
                            if ((int)stringHash != 350953279 || !(id == "19"))
                                goto label_54;
                        }
                        else if (!(id == "18"))
                            goto label_54;
                    }
                    else if (!(id == "69"))
                        goto label_54;
                }
                else if ((int)stringHash != 384361422)
                {
                    if ((int)stringHash != 418063755)
                    {
                        if ((int)stringHash != 434841374 || !(id == "16"))
                            goto label_54;
                    }
                    else if (!(id == "15"))
                        goto label_54;
                }
                else if (!(id == "63"))
                    goto label_54;
            }
            else if (stringHash <= 501951850U)
            {
                if ((int)stringHash != 451618993)
                {
                    if ((int)stringHash != 468396612)
                    {
                        if ((int)stringHash != 501951850 || !(id == "12"))
                            goto label_54;
                    }
                    else if (!(id == "10"))
                        goto label_54;
                }
                else if (!(id == "17"))
                    goto label_54;
            }
            else if ((int)stringHash != 518729469)
            {
                if ((int)stringHash != 806133968)
                {
                    if ((int)stringHash != 822911587 || !(id == "4"))
                        goto label_54;
                }
                else if (!(id == "5"))
                    goto label_54;
            }
            else if (!(id == "13"))
                goto label_54;
        }
        else if (stringHash <= 1007465396U)
        {
            if (stringHash <= 873244444U)
            {
                if ((int)stringHash != 839689206)
                {
                    if ((int)stringHash != 856466825)
                    {
                        if ((int)stringHash != 873244444 || !(id == "1"))
                            goto label_54;
                    }
                    else if (!(id == "6"))
                        goto label_54;
                }
                else if (!(id == "7"))
                    goto label_54;
            }
            else if ((int)stringHash != 906799682)
            {
                if ((int)stringHash != 923577301)
                {
                    if ((int)stringHash != 1007465396 || !(id == "9"))
                        goto label_54;
                }
                else if (!(id == "2"))
                    goto label_54;
            }
            else if (!(id == "3"))
                goto label_54;
        }
        else if (stringHash <= 1304646146U)
        {
            if ((int)stringHash != 1024243015)
            {
                if ((int)stringHash != 1278082707)
                {
                    if ((int)stringHash == 1304646146 && id == "ugc")
                    {
                        listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosPopular.png";
                        goto label_56;
                    }
                    else
                        goto label_54;
                }
                else if (id == "my")
                {
                    listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosMy.png";
                    goto label_56;
                }
                else
                    goto label_54;
            }
            else if (!(id == "8"))
                goto label_54;
        }
        else if ((int)stringHash != 1865670964)
        {
            if ((int)stringHash != -1422563803)
            {
                if ((int)stringHash == -591183440 && id == "series")
                {
                    listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosShows.png";
                    goto label_56;
                }
                else
                    goto label_54;
            }
            else if (id == "feed")
            {
                if (!this.AllowFeedNavigateToAll)
                {
                    listHeaderViewModel.ShowAllVisibility = Visibility.Collapsed;
                    listHeaderViewModel.OnHeaderTap = null;
                }
                listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosUpdates.png";
                goto label_56;
            }
            else
                goto label_54;
        }*/
        if (id == "ugc")
        {
            listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosPopular.png";
        }
        else if (id == "my")
        {
            listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosMy.png";
        }
        else if (id == "series")
        {
            listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosShows.png";
        }
        else if (id == "feed")
        {
            if (!this.AllowFeedNavigateToAll)
            {
                listHeaderViewModel.ShowAllVisibility = Visibility.Collapsed;
                listHeaderViewModel.OnHeaderTap = null;
            }
            listHeaderViewModel.IconUri = "..\\Resources\\VideoCatalog\\VideosUpdates.png";
        }
        else if (this._videoCategory.type == "channel")
        {
            listHeaderViewModel.ImageUri = this.GetImageUriFor(this.OwnerId);
        }
        else
        {
            listHeaderViewModel.IconUri = string.Format("..\\Resources\\VideoCatalog\\Categories\\VideosCat{0}.png", this._videoCategory.id);
        }





        if (!string.IsNullOrWhiteSpace(this._videoCategory.name))
            listHeaderViewModel.Title = this._videoCategory.name;
        List<MenuItemData> menuItemDataList = new List<MenuItemData>();
        if (this._videoCategory.type == "channel" && this.OwnerId < 0)
        {
            Group gr = this._knownGroups.FirstOrDefault<Group>((Func<Group, bool>)(g => g.id == -this.OwnerId));
            if (gr != null)
            {
                this._midSubscribeUnsubscribe = new MenuItemData()
                {
                    Title = gr.IsMember ? CommonResources.GroupPage_Unfollow.ToLowerInvariant() : CommonResources.Group_Follow.ToLowerInvariant()
                };
                this._midSubscribeUnsubscribe.OnTap = (Action)(() => new OwnerHeaderWithSubscribeViewModel(gr).SubscribeUnsubscribe(null));
                menuItemDataList.Add(this._midSubscribeUnsubscribe);
            }
        }
        if (this._videoCategory.can_hide == 1)
        {
            MenuItemData menuItemData1 = new MenuItemData();
            menuItemData1.Title = CommonResources.VideoCatalog_HideCategory;
            Action action = (Action)(() =>
            {
                int result = 0;
                if (!int.TryParse(this._videoCategory.id, out result))
                    return;
                VideoService.Instance.HideCatalogSection(result, (Action<BackendResult<ResponseWithId, ResultCode>>)(res => { }));
                this.CatalogItemsWithHeaderAndFooter.Clear();
            });
            menuItemData1.OnTap = action;
            MenuItemData menuItemData2 = menuItemData1;
            menuItemDataList.Add(menuItemData2);
        }
        if (menuItemDataList.Count > 0)
        {
            listHeaderViewModel.MenuItemDataList = menuItemDataList;
            listHeaderViewModel.ShowMoreActionsVisibility = Visibility.Visible;
        }
        return listHeaderViewModel;
    }
    private void HandleShowAllAction()
    {
      bool flag = false;
      string id = this._videoCategory.id;
      if (!(id == "my"))
      {
        if (id == "feed")
          Navigator.Current.NavigateToNewsFeed(-100L, false);
        else if (this._videoCategory.type == "channel")
        {
          Navigator.Current.NavigateToVideo(false, Math.Abs(this.OwnerId), this.OwnerId < 0, false);
          flag = true;
        }
      }
      else
        Navigator.Current.NavigateToVideo(false, 0, false, false);
      if (flag)
        return;
      Navigator.Current.NavigateToVideoList(new VKList<VideoCatalogItem>()
      {
        items = this._videoCategory.items,
        profiles = this._knownUsers,
        groups = this._knownGroups
      }, 12, this._videoCategory.id.ToString(), this._videoCategory.id, this._videoCategory.next, this._videoCategory.name);
    }

    private string GetImageUriFor(long owner_id)
    {
      if (owner_id < 0L)
      {
        Group group = this._knownGroups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -owner_id));
        if (group != null)
          return group.photo_200;
      }
      else
      {
        User user = this._knownUsers.FirstOrDefault<User>((Func<User, bool>) (u => u.id == owner_id));
        if (user != null)
          return user.photo_max;
      }
      return "";
    }

    public void Handle(GroupMembershipStatusUpdated message)
    {
      if (message.GroupId != -this.OwnerId || this._midSubscribeUnsubscribe == null)
        return;
      Group group = this._knownGroups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this.OwnerId));
      if (group == null)
        return;
      group.is_member = message.Joined ? 1 : 0;
      this._midSubscribeUnsubscribe.Title = group.IsMember ? CommonResources.GroupPage_Unfollow.ToLowerInvariant() : CommonResources.Group_Follow.ToLowerInvariant();
    }
  }
}
