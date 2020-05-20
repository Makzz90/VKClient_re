using VKClient.Common.Backend.DataObjects;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class DocumentsInfo
  {
    public VKList<Category> types { get; set; }

    public VKList<Doc> documents { get; set; }
  }
}
