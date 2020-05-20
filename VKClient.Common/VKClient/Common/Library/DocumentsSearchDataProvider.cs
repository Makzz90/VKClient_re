using System;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class DocumentsSearchDataProvider : ISearchDataProvider<Doc, DocumentHeader>
  {
      public IEnumerable<DocumentHeader> LocalItems { get; set; }

    public string GlobalGroupName
    {
      get
      {
        return CommonResources.GlobalSearch.ToUpperInvariant();
      }
    }

    public string LocalGroupName
    {
      get
      {
        return "";
      }
    }

    public Func<VKList<Doc>, ListWithCount<DocumentHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<Doc>, ListWithCount<DocumentHeader>>) (list => new ListWithCount<DocumentHeader>()
        {
          TotalCount = list.count,
          List = list.items.Select<Doc, DocumentHeader>((Func<Doc, DocumentHeader>) (i => new DocumentHeader(i, 0, false))).ToList<DocumentHeader>()
        });
      }
    }

    public DocumentsSearchDataProvider(IEnumerable<DocumentHeader> localItems)
    {
      this.LocalItems = localItems;
    }

    public void GetData(string q, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<Doc>, ResultCode>> callback)
    {
      DocumentsService.Current.Search(q, offset, count, callback);
    }

    public string GetFooterTextForCount(int count)
    {
      if (count <= 0)
        return CommonResources.Documents_NoDocuments;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneDocFrm, CommonResources.TwoFourDocumentsFrm, CommonResources.FiveDocumentsFrm, true, null, false);
    }
  }
}
