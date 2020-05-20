using System.IO;

namespace VKClient.Common.Framework
{
  public interface IBinarySerializable
  {
    void Write(BinaryWriter writer);

    void Read(BinaryReader reader);
  }
}
