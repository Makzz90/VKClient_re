using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library.Posts
{
  public abstract class OutboundAttachmentBase : ViewModelBase, IOutboundAttachment, INotifyPropertyChanged, IBinarySerializable
  {
    private bool _isOnPostPage;

    public bool IsOnPostPage
    {
      get
      {
        return this._isOnPostPage;
      }
      set
      {
        this._isOnPostPage = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsOnPostPage));
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.Width));
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.Height));
      }
    }

    public virtual Visibility IsFailedUploadVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public double Width
    {
      get
      {
        return this.IsOnPostPage ? 140.0 : 133.0;
      }
    }

    public double Height
    {
      get
      {
        return this.IsOnPostPage ? 105.0 : 100.0;
      }
    }

    public abstract OutboundAttachmentUploadState UploadState { get; set; }

    public abstract bool IsGeo { get; }

    public abstract string AttachmentId { get; }

    public abstract bool IsUploadAttachment { get; }

    public abstract void Upload(Action callback, Action<double> progressCallback = null);

    public abstract Attachment GetAttachment();

    public abstract void SetRetryFlag();

    public abstract void RemoveAndCancelUpload();

    public abstract void Write(BinaryWriter writer);

    public abstract void Read(BinaryReader reader);
  }
}
