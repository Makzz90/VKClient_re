using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public class CropPhoto
  {
    public Photo photo { get; set; }

    public CropRect crop { get; set; }

    public CropRect rect { get; set; }
  }
}
