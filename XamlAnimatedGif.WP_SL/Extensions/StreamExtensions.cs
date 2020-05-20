using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace XamlAnimatedGif.Extensions
{
  internal static class StreamExtensions
  {
    public static async Task ReadAllAsync(this Stream stream, byte[] buffer, int offset, int count)
    {
      int totalRead = 0;
      while (totalRead < count)
      {
        int num = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead);
        if (num == 0)
          throw new EndOfStreamException();
        totalRead += num;
      }
    }

    public static void ReadAll(this Stream stream, byte[] buffer, int offset, int count)
    {
      int num1 = 0;
      while (num1 < count)
      {
        int num2 = stream.Read(buffer, offset + num1, count - num1);
        if (num2 == 0)
          throw new EndOfStreamException();
        num1 += num2;
      }
    }

    public static async Task<int> ReadByteAsync(this Stream stream, CancellationToken cancellationToken = default (CancellationToken))
    {
      byte[] buffer = new byte[1];
      return await stream.ReadAsync(buffer, 0, 1, cancellationToken) != 0 ? (int) buffer[0] : -1;
    }

    public static Stream AsBuffered(this Stream stream)
    {
      return stream;
    }
  }
}
