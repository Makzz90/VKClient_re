using System;
using System.ComponentModel;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Posts
{
  public interface IOutboundAttachment : INotifyPropertyChanged, IBinarySerializable
  {
    OutboundAttachmentUploadState UploadState { get; }

    bool IsGeo { get; }

    string AttachmentId { get; }

    bool IsUploadAttachment { get; }

    bool IsOnPostPage { get; set; }

    Visibility IsFailedUploadVisibility { get; }

    void Upload(Action callback, Action<double> progressCallback = null);

    Attachment GetAttachment();

    void SetRetryFlag();

    void RemoveAndCancelUpload();
  }
}
