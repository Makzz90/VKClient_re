using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Library;
using VKClient.Audio.Localization;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using VKClient.Video.Library;

namespace VKMessenger.Library
{
  public class ConversationMaterialsViewModel : ViewModelBase, ICollectionDataProvider<VKList<ConversationMaterial>, AlbumPhotoHeaderFourInARow>, ICollectionDataProvider<VKList<ConversationMaterial>, VideoHeader>, ICollectionDataProvider<VKList<ConversationMaterial>, AudioHeader>, ICollectionDataProvider<VKList<ConversationMaterial>, DocumentHeader>, ICollectionDataProvider<VKList<ConversationMaterial>, LinkHeader>
  {
    private readonly long _peerId;
    private string _photosNextFrom;
    private string _videosNextFrom;
    private string _audiosNextFrom;
    private string _documentsNextFrom;
    private string _linksNextFrom;

    public GenericCollectionViewModel<VKList<ConversationMaterial>, AlbumPhotoHeaderFourInARow> PhotosVM { get; private set; }

    public GenericCollectionViewModel<VKList<ConversationMaterial>, VideoHeader> VideosVM { get; private set; }

    public GenericCollectionViewModel<VKList<ConversationMaterial>, AudioHeader> AudiosVM { get; private set; }

    public GenericCollectionViewModel<VKList<ConversationMaterial>, DocumentHeader> DocumentsVM { get; private set; }

    public GenericCollectionViewModel<VKList<ConversationMaterial>, LinkHeader> LinksVM { get; private set; }

    public string Title
    {
      get
      {
        return CommonResources.Messenger_Materials.ToUpperInvariant();
      }
    }

    public long PeerId
    {
      get
      {
        return this._peerId;
      }
    }

    public bool IsChat
    {
      get
      {
        return this.PeerId >= 2000000000L;
      }
    }

    Func<VKList<ConversationMaterial>, ListWithCount<AlbumPhotoHeaderFourInARow>> ICollectionDataProvider<VKList<ConversationMaterial>, AlbumPhotoHeaderFourInARow>.ConverterFunc
    {
      get
      {
        return (Func<VKList<ConversationMaterial>, ListWithCount<AlbumPhotoHeaderFourInARow>>) (list =>
        {
          ListWithCount<AlbumPhotoHeaderFourInARow> listWithCount = new ListWithCount<AlbumPhotoHeaderFourInARow>();
          IEnumerator<IEnumerable<ConversationMaterial>> enumerator = list.items.Partition<ConversationMaterial>(4).GetEnumerator();
          try
          {
            while (((IEnumerator) enumerator).MoveNext())
            {
              IEnumerable<ConversationMaterial> current = enumerator.Current;
              AlbumPhotoHeaderFourInARow headerFourInArow = new AlbumPhotoHeaderFourInARow((IEnumerable<Photo>)Enumerable.Select<ConversationMaterial, Photo>(current, (Func<ConversationMaterial, Photo>)(e => e.attachment.photo)), (IEnumerable<long>)Enumerable.Select<ConversationMaterial, long>(current, (Func<ConversationMaterial, long>)(e => e.message_id)));
              listWithCount.List.Add(headerFourInArow);
            }
          }
          finally
          {
            if (enumerator != null)
              ((IDisposable) enumerator).Dispose();
          }
          return listWithCount;
        });
      }
    }

    Func<VKList<ConversationMaterial>, ListWithCount<VideoHeader>> ICollectionDataProvider<VKList<ConversationMaterial>, VideoHeader>.ConverterFunc
    {
      get
      {
        return (Func<VKList<ConversationMaterial>, ListWithCount<VideoHeader>>) (list =>
        {
          ListWithCount<VideoHeader> listWithCount = new ListWithCount<VideoHeader>();
          List<ConversationMaterial>.Enumerator enumerator = list.items.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              ConversationMaterial current = enumerator.Current;
              VideoHeader videoHeader = new VideoHeader(current.attachment.video,  null, list.profiles, list.groups, StatisticsActionSource.messages, "", false, 0, current.message_id);
              listWithCount.List.Add(videoHeader);
            }
          }
          finally
          {
            enumerator.Dispose();
          }
          return listWithCount;
        });
      }
    }

    Func<VKList<ConversationMaterial>, ListWithCount<AudioHeader>> ICollectionDataProvider<VKList<ConversationMaterial>, AudioHeader>.ConverterFunc
    {
      get
      {
        return (Func<VKList<ConversationMaterial>, ListWithCount<AudioHeader>>) (list =>
        {
          ListWithCount<AudioHeader> listWithCount = new ListWithCount<AudioHeader>();
          List<ConversationMaterial>.Enumerator enumerator = list.items.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              ConversationMaterial current = enumerator.Current;
              AudioHeader audioHeader = new AudioHeader(current.attachment.audio, current.message_id);
              listWithCount.List.Add(audioHeader);
            }
          }
          finally
          {
            enumerator.Dispose();
          }
          return listWithCount;
        });
      }
    }

    Func<VKList<ConversationMaterial>, ListWithCount<DocumentHeader>> ICollectionDataProvider<VKList<ConversationMaterial>, DocumentHeader>.ConverterFunc
    {
      get
      {
        return (Func<VKList<ConversationMaterial>, ListWithCount<DocumentHeader>>) (list =>
        {
          ListWithCount<DocumentHeader> listWithCount = new ListWithCount<DocumentHeader>();
          List<ConversationMaterial>.Enumerator enumerator = list.items.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              ConversationMaterial current = enumerator.Current;
              DocumentHeader documentHeader = new DocumentHeader(current.attachment.doc, 0, false, current.message_id);
              listWithCount.List.Add(documentHeader);
            }
          }
          finally
          {
            enumerator.Dispose();
          }
          return listWithCount;
        });
      }
    }

    Func<VKList<ConversationMaterial>, ListWithCount<LinkHeader>> ICollectionDataProvider<VKList<ConversationMaterial>, LinkHeader>.ConverterFunc
    {
      get
      {
        return (Func<VKList<ConversationMaterial>, ListWithCount<LinkHeader>>) (list =>
        {
          ListWithCount<LinkHeader> listWithCount = new ListWithCount<LinkHeader>();
          List<ConversationMaterial>.Enumerator enumerator = list.items.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              ConversationMaterial current = enumerator.Current;
              LinkHeader linkHeader = new LinkHeader(current.attachment.link, current.message_id);
              listWithCount.List.Add(linkHeader);
            }
          }
          finally
          {
            enumerator.Dispose();
          }
          return listWithCount;
        });
      }
    }

    public ConversationMaterialsViewModel(long peerId)
    {
      this._peerId = peerId;
      this.PhotosVM = new GenericCollectionViewModel<VKList<ConversationMaterial>, AlbumPhotoHeaderFourInARow>((ICollectionDataProvider<VKList<ConversationMaterial>, AlbumPhotoHeaderFourInARow>) this);
      this.VideosVM = new GenericCollectionViewModel<VKList<ConversationMaterial>, VideoHeader>((ICollectionDataProvider<VKList<ConversationMaterial>, VideoHeader>) this);
      this.AudiosVM = new GenericCollectionViewModel<VKList<ConversationMaterial>, AudioHeader>((ICollectionDataProvider<VKList<ConversationMaterial>, AudioHeader>) this);
      this.DocumentsVM = new GenericCollectionViewModel<VKList<ConversationMaterial>, DocumentHeader>((ICollectionDataProvider<VKList<ConversationMaterial>, DocumentHeader>) this);
      this.LinksVM = new GenericCollectionViewModel<VKList<ConversationMaterial>, LinkHeader>((ICollectionDataProvider<VKList<ConversationMaterial>, LinkHeader>) this);
      this.PhotosVM.LoadCount = 40;
      this.PhotosVM.ReloadCount = 80;
    }

    public void GetData(GenericCollectionViewModel<VKList<ConversationMaterial>, AlbumPhotoHeaderFourInARow> caller, int offset, int count, Action<BackendResult<VKList<ConversationMaterial>, ResultCode>> callback)
    {
      if (offset > 0 && this._photosNextFrom == null)
      {
        callback(new BackendResult<VKList<ConversationMaterial>, ResultCode>(ResultCode.Succeeded, new VKList<ConversationMaterial>()));
      }
      else
      {
        if (offset == 0 && this._photosNextFrom != null)
          this._photosNextFrom =  null;
        MessagesService.Instance.GetConversationMaterials(this._peerId, "photo", this._photosNextFrom, count, (Action<BackendResult<VKList<ConversationMaterial>, ResultCode>>) (result =>
        {
          this._photosNextFrom = result.ResultData.next_from;
          callback(result);
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<ConversationMaterial>, AlbumPhotoHeaderFourInARow> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoPhotos;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePhotoFrm, CommonResources.TwoFourPhotosFrm, CommonResources.FivePhotosFrm, true,  null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<ConversationMaterial>, VideoHeader> caller, int offset, int count, Action<BackendResult<VKList<ConversationMaterial>, ResultCode>> callback)
    {
      if (offset > 0 && this._videosNextFrom == null)
      {
        callback(new BackendResult<VKList<ConversationMaterial>, ResultCode>(ResultCode.Succeeded, new VKList<ConversationMaterial>()));
      }
      else
      {
        if (offset == 0 && this._videosNextFrom != null)
          this._videosNextFrom =  null;
        MessagesService.Instance.GetConversationMaterials(this._peerId, "video", this._videosNextFrom, count, (Action<BackendResult<VKList<ConversationMaterial>, ResultCode>>) (result =>
        {
          this._videosNextFrom = result.ResultData.next_from;
          callback(result);
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<ConversationMaterial>, VideoHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoVideos;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneVideoFrm, CommonResources.TwoFourVideosFrm, CommonResources.FiveVideosFrm, true,  null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<ConversationMaterial>, AudioHeader> caller, int offset, int count, Action<BackendResult<VKList<ConversationMaterial>, ResultCode>> callback)
    {
      if (offset > 0 && this._audiosNextFrom == null)
      {
        callback(new BackendResult<VKList<ConversationMaterial>, ResultCode>(ResultCode.Succeeded, new VKList<ConversationMaterial>()));
      }
      else
      {
        if (offset == 0 && this._audiosNextFrom != null)
          this._audiosNextFrom =  null;
        MessagesService.Instance.GetConversationMaterials(this._peerId, "audio", this._audiosNextFrom, count, (Action<BackendResult<VKList<ConversationMaterial>, ResultCode>>) (result =>
        {
          this._audiosNextFrom = result.ResultData.next_from;
          callback(result);
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<ConversationMaterial>, AudioHeader> caller, int count)
    {
      if (count <= 0)
        return AudioResources.NoTracks;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, AudioResources.OneTrackFrm, AudioResources.TwoFourTracksFrm, AudioResources.FiveTracksFrm, true,  null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<ConversationMaterial>, DocumentHeader> caller, int offset, int count, Action<BackendResult<VKList<ConversationMaterial>, ResultCode>> callback)
    {
      if (offset > 0 && this._documentsNextFrom == null)
      {
        callback(new BackendResult<VKList<ConversationMaterial>, ResultCode>(ResultCode.Succeeded, new VKList<ConversationMaterial>()));
      }
      else
      {
        if (offset == 0 && this._documentsNextFrom != null)
          this._documentsNextFrom =  null;
        MessagesService.Instance.GetConversationMaterials(this._peerId, "doc", this._documentsNextFrom, count, (Action<BackendResult<VKList<ConversationMaterial>, ResultCode>>) (result =>
        {
          this._documentsNextFrom = result.ResultData.next_from;
          callback(result);
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<ConversationMaterial>, DocumentHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.Documents_NoDocuments;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneDocFrm, CommonResources.TwoFourDocumentsFrm, CommonResources.FiveDocumentsFrm, true,  null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<ConversationMaterial>, LinkHeader> caller, int offset, int count, Action<BackendResult<VKList<ConversationMaterial>, ResultCode>> callback)
    {
      if (offset > 0 && this._linksNextFrom == null)
      {
        callback(new BackendResult<VKList<ConversationMaterial>, ResultCode>(ResultCode.Succeeded, new VKList<ConversationMaterial>()));
      }
      else
      {
        if (offset == 0 && this._linksNextFrom != null)
          this._linksNextFrom =  null;
        MessagesService.Instance.GetConversationMaterials(this._peerId, "link", this._linksNextFrom, count, (Action<BackendResult<VKList<ConversationMaterial>, ResultCode>>) (result =>
        {
          this._linksNextFrom = result.ResultData.next_from;
          callback(result);
        }));
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<ConversationMaterial>, LinkHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.Messenger_NoLinks;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneLinkFrm, CommonResources.TwoFourLinksFrm, CommonResources.FiveLinksFrm, true,  null, false);
    }
  }
}
