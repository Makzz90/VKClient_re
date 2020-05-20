using System.Text;

namespace VKClient.Audio.Base.Core
{
  public interface IGZipEncoder
  {
    byte[] Compress(string stringToCompress, Encoding encoding);
  }
}
