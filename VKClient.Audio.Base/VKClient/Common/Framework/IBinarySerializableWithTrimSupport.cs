using System.IO;

namespace VKClient.Common.Framework
{
  public interface IBinarySerializableWithTrimSupport : IBinarySerializable
  {
    void WriteTrimmed(BinaryWriter writer);
  }
}
