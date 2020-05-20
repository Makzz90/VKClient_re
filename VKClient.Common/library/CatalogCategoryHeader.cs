using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class CatalogCategoryHeader
  {
    private GroupCatalogCategoryPreview _gccp;

    public int CategoryId
    {
      get
      {
        return this._gccp.id;
      }
    }

    public string Title
    {
      get
      {
        return this._gccp.name;
      }
    }

    public string Subtitle
    {
      get
      {
        return UIStringFormatterHelper.FormatNumberOfSomething(this._gccp.page_count, CommonResources.OnePageFrm, CommonResources.TwoFourPagesFrm, CommonResources.FivePagesFrm, true,  null, false);
      }
    }

    public string Preview1
    {
      get
      {
        return this.GetGroupAvatar(0);
      }
    }

    public string Preview2
    {
      get
      {
        return this.GetGroupAvatar(1);
      }
    }

    public string Preview3
    {
      get
      {
        return this.GetGroupAvatar(2);
      }
    }

    public CatalogCategoryHeader(GroupCatalogCategoryPreview gccp)
    {
      this._gccp = gccp;
    }

    private string GetGroupAvatar(int ind)
    {
      if (this._gccp.page_previews.Count > ind)
        return this._gccp.page_previews[ind].photo_200;
      return "";
    }
  }
}
