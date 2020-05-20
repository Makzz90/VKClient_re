using System;
using System.Collections.Generic;
using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Shared
{
  public class ListHeaderViewModel : ISupportMenuActions
  {
    public double Tilt
    {
      get
      {
        return this.OnHeaderTap == null ? 0.0 : 1.0;
      }
    }

    public string Title { get; set; }

    public Visibility ShowAllVisibility { get; set; }

    public Visibility ShowMoreActionsVisibility { get; set; }

    public string IconUri { get; set; }

    public string ImageUri { get; set; }

    public List<MenuItemData> MenuItemDataList { get; set; }

    public bool HaveIconOrImage
    {
      get
      {
        if (string.IsNullOrWhiteSpace(this.IconUri))
          return !string.IsNullOrWhiteSpace(this.ImageUri);
        return true;
      }
    }

    public Action OnHeaderTap { get; set; }

    public ListHeaderViewModel()
    {
      this.ShowAllVisibility = Visibility.Collapsed;
      this.ShowMoreActionsVisibility = Visibility.Collapsed;
    }

    public void HandleTap()
    {
      if (this.OnHeaderTap == null)
        return;
      this.OnHeaderTap();
    }

    public List<MenuItemData> GetMenuItemsData()
    {
      return this.MenuItemDataList;
    }
  }
}
