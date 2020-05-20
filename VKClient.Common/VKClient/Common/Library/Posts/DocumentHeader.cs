using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Posts
{
  public class DocumentHeader : ISearchableItemHeader<Doc>
  {
    private readonly string[] _splittedNameArray;
    private readonly int _documentType;

    public bool IsLocalItem
    {
      get
      {
        if (this.Document.owner_id == AppGlobalStateManager.Current.LoggedInUserId)
          return this.Document.type == this._documentType;
        return false;
      }
    }

    public Doc Document { get; set; }

    public string Name { get; set; }

    public string Extension { get; set; }

    public string Description { get; set; }

    public string FullDescription { get; set; }

    public string ThumbnailUri { get; set; }

    public bool HasThumbnail { get; set; }

    public Visibility ExtensionVisibility { get; set; }

    public bool IsMenuEnabled { get; set; }

    public bool IsGif
    {
      get
      {
        string ext = this.Document.ext;
        return (ext != null ? ext.ToLowerInvariant() : null) == "gif";
      }
    }

    public DocumentHeader(Doc document, int documentType = 0, bool isOwnerCommunityAdmined = false)
    {
      this.Document = document;
      this.Document.MakeSafety();
      this.Name = this.Document.title;
      this.Extension = this.Document.ext;
      this.Description = string.Format("{0} · {1}", (object) document.ext.ToUpperInvariant(), (object) this.GetSizeString());
      this.FullDescription = string.Format("{0} · {1}", (object) this.GetSizeString(), (object) UIStringFormatterHelper.FormatDateTimeForUI(this.Document.date));
      this.ThumbnailUri = this.Document.PreviewUri ?? "";
      this.HasThumbnail = this.ThumbnailUri != "";
      this.ExtensionVisibility = this.HasThumbnail ? Visibility.Collapsed : Visibility.Visible;
      this.IsMenuEnabled = document.owner_id == AppGlobalStateManager.Current.LoggedInUserId | isOwnerCommunityAdmined;
      this._splittedNameArray = (this.Name + " " + this.Extension).Split(' ');
      this._documentType = documentType;
    }

    public bool Matches(string searchString)
    {
      MatchStrings matchStrings = TransliterationHelper.GetMatchStrings(searchString);
      return this.MatchesAny(matchStrings.SearchStrings, matchStrings.LatinStrings, matchStrings.CyrillicStrings);
    }

    public bool MatchesAny(List<string> searchStrings, List<string> searchStringsLatin, List<string> searchStringsCyrillic)
    {
      if (!this.Matches(searchStrings) && !this.Matches(searchStringsLatin))
        return this.Matches(searchStringsCyrillic);
      return true;
    }

    private bool Matches(List<string> searchStrings)
    {
      if (searchStrings.Count == 0 || searchStrings.All<string>(new Func<string, bool>(string.IsNullOrWhiteSpace)))
        return false;
      foreach (string searchString in searchStrings)
      {
        bool flag = false;
        foreach (string splittedName in this._splittedNameArray)
        {
          flag = splittedName.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase);
          if (flag)
            break;
        }
        if (!flag)
          return false;
      }
      return true;
    }

    public long GetSize()
    {
      DocPreview preview = this.Document.preview;
      long? nullable;
      if (preview == null)
      {
        nullable = new long?();
      }
      else
      {
        DocPreviewVideo video = preview.video;
        nullable = video != null ? new long?(video.file_size) : new long?();
      }
      return nullable ?? this.Document.size;
    }

    public string GetSizeString()
    {
      double a = (double) this.GetSize();
      if (a < 1024.0)
        return Math.Round(a).ToString() + " B";
      if (a < 1048576.0)
        return Math.Round(a / 1024.0).ToString("#.#") + " KB";
      if (a < 1073741824.0)
        return (a / 1048576.0).ToString("#.#") + " MB";
      if (a < 1099511627776.0)
        return (a / 1073741824.0).ToString("#.#") + " GB";
      return (a / 1099511627776.0).ToString("#.#") + " TB";
    }
  }
}
