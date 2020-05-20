using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class GiftsSectionItem
  {
    private string _description;
    private string _priceStr;
    private string _realPriceStr;

    public Gift gift { get; set; }

    public string description
    {
      get
      {
        return this._description;
      }
      set
      {
        this._description = (value ?? "").ForUI();
      }
    }

    public int disabled { get; set; }

    public int price { get; set; }

    public string price_str
    {
      get
      {
        return this._priceStr;
      }
      set
      {
        this._priceStr = (value ?? "").ForUI();
      }
    }

    public int gifts_left { get; set; }

    public int real_price { get; set; }

    public string real_price_str
    {
      get
      {
        return this._realPriceStr;
      }
      set
      {
        this._realPriceStr = (value ?? "").ForUI();
      }
    }

    public bool IsDisabled
    {
      get
      {
        return this.disabled == 1;
      }
    }
  }
}
