using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class ShareInternalContentDataProvider : IShareContentDataProvider
  {
    private const string MESSAGE_KEY = "NewMessageContents";
    private const string WALLPOST_KEY = "WallPostAttachment";
    private const string PHOTO_KEY = "PickedPhoto";
    private const string VIDEO_KEY = "PickedVideo";
    private const string DOC_KEY = "PickedDocument";
    private const string PRODUCT_KEY = "ShareProduct";
    private const string FORWARDED_MESSAGES_KEY = "MessagesToForward";

    public string Message { get; set; }

    public WallPost WallPost { get; set; }

    public Photo Photo { get; set; }

    public VKClient.Common.Backend.DataObjects.Video Video { get; set; }

    public Product Product { get; set; }

    public List<VKMessenger.Backend.Message> ForwardedMessages { get; set; }

    public Doc Document { get; set; }

    public void StoreDataToRepository()
    {
      ParametersRepository.SetParameterForId("NewMessageContents", (object) (this.Message ?? (this.Message = "")));
      ParametersRepository.SetParameterForId("WallPostAttachment", (object) this.WallPost);
      ParametersRepository.SetParameterForId("PickedPhoto", (object) this.Photo);
      ParametersRepository.SetParameterForId("PickedVideo", (object) this.Video);
      ParametersRepository.SetParameterForId("ShareProduct", (object) this.Product);
      ParametersRepository.SetParameterForId("MessagesToForward", (object) this.ForwardedMessages);
      ParametersRepository.SetParameterForId("PickedDocument", (object) this.Document);
    }
  }
}
