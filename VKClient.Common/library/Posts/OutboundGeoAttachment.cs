using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.Posts
{
  public class OutboundGeoAttachment : OutboundAttachmentBase
  {
    private string _geoDescription = string.Empty;

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }

    public string MapUrl
    {
      get
      {
        return MapsService.Current.GetMapUri(this.Latitude, this.Longitude, 15, 210, 1.8).ToString();
      }
    }

    public override OutboundAttachmentUploadState UploadState
    {
      get
      {
        return OutboundAttachmentUploadState.Completed;
      }
      set
      {
      }
    }

    public Visibility IsUploadingVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public string IconSource
    {
      get
      {
        return "/Resources/NewPost/MiniAttachmentsPlace.png";
      }
    }

    public override bool IsGeo
    {
      get
      {
        return true;
      }
    }

    public override string AttachmentId
    {
      get
      {
        return "";
      }
    }

    public string GeoDescription
    {
      get
      {
        return this._geoDescription;
      }
      set
      {
        this._geoDescription = value;
        this.NotifyPropertyChanged<string>(() => this.GeoDescription);
        this.NotifyPropertyChanged<string>(() => this.Subtitle);
      }
    }

    public string Title
    {
      get
      {
        return CommonResources.Conversations_Location;
      }
    }

    public string Subtitle
    {
      get
      {
        return this.GeoDescription;
      }
    }

    public override bool IsUploadAttachment
    {
      get
      {
        return false;
      }
    }

    public OutboundGeoAttachment(double latitude, double longitude)
    {
      this.Latitude = latitude;
      this.Longitude = longitude;
      this.FetchDescription();
    }

    public OutboundGeoAttachment()
    {
    }

    public override void Write(BinaryWriter writer)
    {
      writer.Write(VKConstants.SerializationVersion);
      writer.Write(this.Latitude);
      writer.Write(this.Longitude);
      writer.WriteString(this._geoDescription);
    }

    public override void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.Latitude = reader.ReadDouble();
      this.Longitude = reader.ReadDouble();
      this._geoDescription = reader.ReadString();
      if (!(this._geoDescription == string.Empty))
        return;
      this.FetchDescription();
    }

    public override void Upload(Action callback, Action<double> progressCallback = null)
    {
      callback.Invoke();
    }

    private void FetchDescription()
    {
      MapsService.Current.ReverseGeocodeToAddress(this.Latitude, this.Longitude, (Action<BackendResult<string, ResultCode>>) (res =>
      {
        if (res.ResultCode != ResultCode.Succeeded)
          return;
        this.GeoDescription = res.ResultData;
      }));
    }

    public override Attachment GetAttachment()
    {
      throw new NotImplementedException();
    }

    public override void SetRetryFlag()
    {
    }

    public override void RemoveAndCancelUpload()
    {
    }
  }
}
