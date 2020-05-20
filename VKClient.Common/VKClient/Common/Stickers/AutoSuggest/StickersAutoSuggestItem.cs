using VKClient.Common.Emoji;
using VKClient.Common.Utils;

namespace VKClient.Common.Stickers.AutoSuggest
{
  public class StickersAutoSuggestItem
  {
    private static readonly double PROMOTED_OPACITY = 0.6;
    private StickerItemData _stickerData;
    private bool _isPromoted;

    public string PreviewUri
    {
      get
      {
        if (ScaleFactor.GetScaleFactor() == 100)
          return this._stickerData.LocalPath;
        return this._stickerData.LocalPathBig;
      }
    }

    public string PreviewUriBig
    {
      get
      {
        if (ScaleFactor.GetRealScaleFactor() == 100)
          return this._stickerData.LocalPathBig;
        return this._stickerData.LocalPathExtraBig;
      }
    }

    public double Opacity
    {
      get
      {
        if (!this._isPromoted)
          return 1.0;
        return StickersAutoSuggestItem.PROMOTED_OPACITY;
      }
    }

    public StickerItemData StickerData
    {
      get
      {
        return this._stickerData;
      }
    }

    public bool IsPromoted
    {
      get
      {
        return this._isPromoted;
      }
    }

    public StickersAutoSuggestItem(StickerItemData stickerData, bool isPromoted)
    {
      this._stickerData = stickerData;
      this._isPromoted = isPromoted;
    }
  }
}
