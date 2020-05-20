using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Emoji
{
  public class EmojiSpritesCreator
  {
    private SpritesPack _spritesPack = new SpritesPack();
    private List<string> _localPaths = new List<string>();
    private bool _isInitialized;
    private int _id;

    private bool IsDarkTheme
    {
      get
      {
        return (Visibility) Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible;
      }
    }

    private string SpritesPackKey
    {
      get
      {
        return "emojiSpritePack9_" + this.IsDarkTheme.ToString();
      }
    }

    public SpritesPack SpritesPack
    {
      get
      {
        return this._spritesPack;
      }
    }

    public EmojiSpritesCreator(int id)
    {
      this._id = id;
    }

    public EmojiSpritesCreator(int id, List<string> localPaths)
    {
      this._id = id;
      this._localPaths = localPaths;
    }

    public bool TryDeserializeSpritePack()
    {
      if (this._isInitialized)
        return true;
      int num = CacheManager.TryDeserialize((IBinarySerializable) this._spritesPack, this.SpritesPackKey, CacheManager.DataType.CachedData) ? 1 : 0;
      if (num == 0)
        return num != 0;
      this._isInitialized = true;
      return num != 0;
    }

    public void CreateSprites()
    {
      if (!this.CreateEmojiSprites(this._spritesPack) || !CacheManager.TrySerialize((IBinarySerializable) this._spritesPack, this.SpritesPackKey, false, CacheManager.DataType.CachedData))
        return;
      this._isInitialized = true;
    }

    private bool CreateEmojiSprites(SpritesPack pack)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      List<EmojiDataItem> allDataItems = EmojiDataItem.GetAllDataItems();
      int num1 = EmojiConstants.ColumnsCountVerticalOrientation * EmojiConstants.VerticalSpriteRowsCount;
      int num2 = 0;
      string str1 = "emojiSpriteVertical" + this.IsDarkTheme.ToString() + ".jpg";
      SolidColorBrush backgroundColor1 = Application.Current.Resources["PhoneMenuBackgroundBrush"] as SolidColorBrush;
      foreach (IEnumerable<EmojiDataItem> source in allDataItems.Partition<EmojiDataItem>(num1))
      {
        SpriteDescription sprite = SpriteCreatorHelper.TryCreateSprite(num1, source.Select<EmojiDataItem, string>((Func<EmojiDataItem, string>) (d => d.Uri.OriginalString)).ToList<string>(), source.Select<EmojiDataItem, string>((Func<EmojiDataItem, string>) (d => d.String)).ToList<string>(), num2.ToString() + str1, EmojiConstants.ColumnsCountVerticalOrientation, EmojiConstants.EmojiWidthInPixels, EmojiConstants.EmojiHeightInPixels, EmojiConstants.VerticalSpriteWidthInPixels, EmojiConstants.VerticalSpriteHeightInPixels, backgroundColor1);
        ++num2;
        if (sprite == null)
          return false;
        pack.SpritesVertical.Add(sprite);
      }
      int num3 = EmojiConstants.ColumnsCountHorizontalOrientation * EmojiConstants.HorizontalSpriteRowsCount;
      int num4 = 0;
      string str2 = "emojiSprteHorizontal " + this.IsDarkTheme.ToString() + ".jpg";
      foreach (IEnumerable<EmojiDataItem> source in allDataItems.Partition<EmojiDataItem>(num3))
      {
        SpriteDescription sprite = SpriteCreatorHelper.TryCreateSprite(num3, source.Select<EmojiDataItem, string>((Func<EmojiDataItem, string>) (d => d.Uri.OriginalString)).ToList<string>(), source.Select<EmojiDataItem, string>((Func<EmojiDataItem, string>) (d => d.String)).ToList<string>(), num4.ToString() + str2, EmojiConstants.ColumnsCountHorizontalOrientation, EmojiConstants.EmojiWidthInPixels, EmojiConstants.EmojiHeightInPixels, EmojiConstants.HorizontalSpriteWidthInPixels, EmojiConstants.HorizontalSpriteHeightInPixels, backgroundColor1);
        ++num4;
        if (sprite == null)
          return false;
        pack.SpritesHorizontal.Add(sprite);
      }
      stopwatch.Stop();
      return true;
    }
  }
}
