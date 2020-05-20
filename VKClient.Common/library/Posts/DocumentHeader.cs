using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Library.Posts
{
  public class DocumentHeader : ViewModelBase, ISearchableItemHeader<Doc>
  {
    private string _name = "";
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

    public Doc Document { get; private set; }

    public long MessageId { get; private set; }

    public string Name
    {
      get
      {
        return this._name;
      }
      set
      {
        this._name = value;
        this.NotifyPropertyChanged<string>(() => this.Name);
      }
    }

    public string Extension { get; private set; }

    public string Description { get; private set; }

    public string FullDescription { get; private set; }

    public string ThumbnailUri { get; private set; }

    public bool HasThumbnail { get; private set; }

    public Visibility ExtensionVisibility { get; private set; }

    public bool IsMenuEnabled { get; private set; }

    public bool IsGif
    {
      get
      {
        string ext = this.Document.ext;
        return (ext != null ? ( ext).ToLowerInvariant() :  null) == "gif";
      }
    }

    public DocumentHeader(Doc document, int documentType = 0, bool isOwnerCommunityAdmined = false, long messageId = 0)
    {
      this.Document = document;
      this.Document.MakeSafety();
      this.MessageId = messageId;
      this.Name = this.Document.title;
      this.Extension = this.Document.ext;
      this.Description = string.Format("{0} · {1}", ((string) document.ext).ToUpperInvariant(), this.GetSizeString());
      this.FullDescription = string.Format("{0} · {1}", this.GetSizeString(), UIStringFormatterHelper.FormatDateTimeForUI(this.Document.date));
      this.ThumbnailUri = this.Document.PreviewUri ?? "";
      this.HasThumbnail = this.ThumbnailUri != "";
      this.ExtensionVisibility = this.HasThumbnail ? Visibility.Collapsed : Visibility.Visible;
      this.IsMenuEnabled = document.owner_id == AppGlobalStateManager.Current.LoggedInUserId | isOwnerCommunityAdmined;
      this._splittedNameArray = ((string) string.Concat(this.Name, " ", this.Extension)).Split((char[]) new char[1]
      {
        ' '
      });
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
      // ISSUE: method pointer
        if (searchStrings.Count == 0 || Enumerable.All<string>(searchStrings, (Func<string, bool>)new Func<string, bool>(string.IsNullOrWhiteSpace)))
        return false;
      List<string>.Enumerator enumerator = searchStrings.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          string current = enumerator.Current;
          bool flag = false;
          foreach (string splittedName in this._splittedNameArray)
          {
            flag = splittedName.StartsWith(current, (StringComparison) 3);
            if (flag)
              break;
          }
          if (!flag)
            return false;
        }
      }
      finally
      {
        enumerator.Dispose();
      }
      return true;
    }

    public long GetSize()
    {
      DocPreview preview = this.Document.preview;
      long? nullable1;
      if (preview == null)
      {
        nullable1 = new long?();
      }
      else
      {
        DocPreviewVideo video = preview.video;
        nullable1 = video != null ? new long?(video.file_size) : new long?();
      }
      long? nullable2 = nullable1;
      if (!nullable2.HasValue)
        return this.Document.size;
      return nullable2.GetValueOrDefault();
    }

    public string GetSizeString()
    {
      double size = (double) this.GetSize();
      if (size < 1024.0)
        return string.Concat(Math.Round(size), " B");
      if (size < 1048576.0)
        return string.Concat(Math.Round(size / 1024.0).ToString("#.#"), " KB");
      if (size < 1073741824.0)
        return string.Concat((size / 1048576.0).ToString("#.#"), " MB");
      if (size < 1099511627776.0)
        return string.Concat((size / 1073741824.0).ToString("#.#"), " GB");
      return string.Concat((size / 1099511627776.0).ToString("#.#"), " TB");
    }
  }
}
