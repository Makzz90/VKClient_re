using System.Collections.Generic;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.Posts
{
  public static class AttachmentTypes
  {
    public static readonly List<NamedAttachmentType> AttachmentPhotoSubtypes = new List<NamedAttachmentType>() { new NamedAttachmentType() { AttachmentType = AttachmentType.PhotoFromPhone, Name = CommonResources.Attachments_UploadFromGallery }, new NamedAttachmentType() { AttachmentType = AttachmentType.PhotoMy, Name = CommonResources.Attachments_MyPhotos } };
    public static readonly List<NamedAttachmentType> AttachmentVideoSubtypes = new List<NamedAttachmentType>() { new NamedAttachmentType() { AttachmentType = AttachmentType.VideoFromPhone, Name = CommonResources.Attachments_UploadFromGallery }, new NamedAttachmentType() { AttachmentType = AttachmentType.VideoMy, Name = CommonResources.Attachments_MyVideo } };
    public static readonly List<NamedAttachmentType> AttachmentTypesWithPhotoFromGallery = new List<NamedAttachmentType>() { new NamedAttachmentType() { AttachmentType = AttachmentType.Photo, Name = CommonResources.AttachmentType_Photo }, new NamedAttachmentType() { AttachmentType = AttachmentType.Audio, Name = CommonResources.AttachmentType_Audio }, new NamedAttachmentType() { AttachmentType = AttachmentType.Video, Name = CommonResources.AttachmentType_Video }, new NamedAttachmentType() { AttachmentType = AttachmentType.Document, Name = CommonResources.AttachmentType_Document } };
    public static readonly List<NamedAttachmentType> AttachmentTypesWithPhotoFromGalleryAndLocation = new List<NamedAttachmentType>() { new NamedAttachmentType() { AttachmentType = AttachmentType.Photo, Name = CommonResources.AttachmentType_Photo }, new NamedAttachmentType() { AttachmentType = AttachmentType.Audio, Name = CommonResources.AttachmentType_Audio }, new NamedAttachmentType() { AttachmentType = AttachmentType.Video, Name = CommonResources.AttachmentType_Video }, new NamedAttachmentType() { AttachmentType = AttachmentType.Document, Name = CommonResources.AttachmentType_Document }, new NamedAttachmentType() { AttachmentType = AttachmentType.Location, Name = CommonResources.AttachmentType_Location } };
    public static readonly List<NamedAttachmentType> AttachmentTypesWithPhotoFromGalleryGraffitiAndLocation;
    public static readonly NamedAttachmentType PollAttachmentType;
    public static readonly NamedAttachmentType TimerAttachmentType;
    public static readonly NamedAttachmentType MoneyAttachmentType;
    public static readonly NamedAttachmentType GiftAttachmentType;
    public static readonly NamedAttachmentType PhotoCommunityType;
    public static readonly NamedAttachmentType VideoCommunityType;

    static AttachmentTypes()
    {
      List<NamedAttachmentType> namedAttachmentTypeList = new List<NamedAttachmentType>();
      namedAttachmentTypeList.Add(new NamedAttachmentType()
      {
        AttachmentType = AttachmentType.Photo,
        Name = CommonResources.AttachmentType_Photo
      });
      namedAttachmentTypeList.Add(new NamedAttachmentType()
      {
        AttachmentType = AttachmentType.Audio,
        Name = CommonResources.AttachmentType_Audio
      });
      namedAttachmentTypeList.Add(new NamedAttachmentType()
      {
        AttachmentType = AttachmentType.Video,
        Name = CommonResources.AttachmentType_Video
      });
      NamedAttachmentType namedAttachmentType1 = new NamedAttachmentType();
      namedAttachmentType1.AttachmentType = AttachmentType.Graffiti;
      string lowerInvariant1 = CommonResources.Graffiti.ToLowerInvariant();
      namedAttachmentType1.Name = lowerInvariant1;
      namedAttachmentTypeList.Add(namedAttachmentType1);
      namedAttachmentTypeList.Add(new NamedAttachmentType()
      {
        AttachmentType = AttachmentType.Document,
        Name = CommonResources.AttachmentType_Document
      });
      namedAttachmentTypeList.Add(new NamedAttachmentType()
      {
        AttachmentType = AttachmentType.Location,
        Name = CommonResources.AttachmentType_Location
      });
      AttachmentTypes.AttachmentTypesWithPhotoFromGalleryGraffitiAndLocation = namedAttachmentTypeList;
      NamedAttachmentType namedAttachmentType2 = new NamedAttachmentType();
      namedAttachmentType2.AttachmentType = AttachmentType.Poll;
      string lowerInvariant2 = CommonResources.Poll.ToLowerInvariant();
      namedAttachmentType2.Name = lowerInvariant2;
      AttachmentTypes.PollAttachmentType = namedAttachmentType2;
      AttachmentTypes.TimerAttachmentType = new NamedAttachmentType()
      {
        AttachmentType = AttachmentType.Timer,
        Name = CommonResources.AttachmentType_Timer
      };
      AttachmentTypes.MoneyAttachmentType = new NamedAttachmentType()
      {
        AttachmentType = AttachmentType.MoneyTransfer,
        Name = CommonResources.Money
      };
      AttachmentTypes.GiftAttachmentType = new NamedAttachmentType()
      {
        AttachmentType = AttachmentType.Gift,
        Name = CommonResources.Gift
      };
      AttachmentTypes.PhotoCommunityType = new NamedAttachmentType()
      {
        AttachmentType = AttachmentType.PhotoCommunity,
        Name = CommonResources.Attachments_CommunityPhotos
      };
      AttachmentTypes.VideoCommunityType = new NamedAttachmentType()
      {
        AttachmentType = AttachmentType.VideoCommunity,
        Name = CommonResources.Attachments_CommunityVideos
      };
    }
  }
}
