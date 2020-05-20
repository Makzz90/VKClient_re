using System.Collections.Generic;
using System.IO;
using VKClient.Common.Framework;

namespace VKClient.Audio.Base.DataObjects
{
  public sealed class DocPreviewPhoto : IBinarySerializable
  {
    public List<DocPreviewPhotoSize> sizes { get; set; }

    public void Write(BinaryWriter writer)
    {
      writer.Write(1);
      writer.WriteList<DocPreviewPhotoSize>((IList<DocPreviewPhotoSize>) this.sizes, 10000);
    }

    public void Read(BinaryReader reader)
    {
      reader.ReadInt32();
      this.sizes = reader.ReadList<DocPreviewPhotoSize>();
    }
  }
}
