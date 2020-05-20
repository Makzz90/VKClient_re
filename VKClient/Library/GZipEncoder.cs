using System.IO;
using System.IO.Compression;
using System.Text;
using VKClient.Audio.Base.Core;

namespace VKClient.Library
{
  public class GZipEncoder : IGZipEncoder
  {
    public byte[] Compress(string stringToCompress, Encoding encoding)
    {
      byte[] bytes = encoding.GetBytes(stringToCompress);
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (GZipStream gzipStream = new GZipStream((Stream) memoryStream, CompressionMode.Compress))
        {
          ((Stream) gzipStream).Write(bytes, 0, bytes.Length);
          ((Stream) gzipStream).Close();
          return memoryStream.ToArray();
        }
      }
    }
  }
}
