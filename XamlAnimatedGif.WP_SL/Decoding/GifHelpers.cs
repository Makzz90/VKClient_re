using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XamlAnimatedGif.Extensions;

namespace XamlAnimatedGif.Decoding
{
  internal static class GifHelpers
  {
    public static async Task<string> ReadStringAsync(Stream stream, int length)
    {
      byte[] bytes = new byte[length];
      await stream.ReadAllAsync(bytes, 0, length).ConfigureAwait(false);
      return GifHelpers.GetString(bytes);
    }

    public static async Task<byte[]> ReadDataBlocksAsync(Stream stream, bool discard)
    {
      MemoryStream ms = discard ?  null : new MemoryStream();
      byte[] numArray;
      using (ms)
      {
        int len;
        while ((len = stream.ReadByte()) > 0)
        {
          byte[] bytes = new byte[len];
          int num = await stream.ReadAsync(bytes, 0, len).ConfigureAwait(false);
          MemoryStream memoryStream = ms;
          if (memoryStream != null)
          {
            byte[] buffer = bytes;
            int offset = 0;
            int count = len;
            memoryStream.Write(buffer, offset, count);
          }
          bytes = (byte[]) null;
        }
        MemoryStream memoryStream1 = ms;
        numArray = memoryStream1 != null ? memoryStream1.ToArray() : (byte[]) null;
      }
      return numArray;
    }

    public static async Task CopyDataBlocksToStreamAsync(Stream sourceStream, Stream targetStream, CancellationToken cancellationToken = default (CancellationToken))
    {
      while (true)
      {
        int num1 = await sourceStream.ReadByteAsync(cancellationToken);
        int len;
        if ((len = num1) > 0)
        {
          byte[] bytes = new byte[len];
          int num2 = await sourceStream.ReadAsync(bytes, 0, len, cancellationToken).ConfigureAwait(false);
          await targetStream.WriteAsync(bytes, 0, len, cancellationToken);
          bytes = (byte[]) null;
        }
        else
          break;
      }
    }

    public static async Task<GifColor[]> ReadColorTableAsync(Stream stream, int size)
    {
      int count = 3 * size;
      byte[] bytes = new byte[count];
      await stream.ReadAllAsync(bytes, 0, count).ConfigureAwait(false);
      GifColor[] gifColorArray = new GifColor[size];
      for (int index = 0; index < size; ++index)
      {
        byte r = bytes[3 * index];
        byte g = bytes[3 * index + 1];
        byte b = bytes[3 * index + 2];
        gifColorArray[index] = new GifColor(r, g, b);
      }
      return gifColorArray;
    }

    public static bool IsNetscapeExtension(GifApplicationExtension ext)
    {
      if (ext.ApplicationIdentifier == "NETSCAPE")
        return GifHelpers.GetString(ext.AuthenticationCode) == "2.0";
      return false;
    }

    public static ushort GetRepeatCount(GifApplicationExtension ext)
    {
      if (ext.Data.Length >= 3)
        return BitConverter.ToUInt16(ext.Data, 1);
      return 1;
    }

    public static Exception UnexpectedEndOfStreamException()
    {
      return (Exception) new UnexpectedEndOfStreamException("Unexpected end of stream before trailer was encountered");
    }

    public static Exception UnknownBlockTypeException(int blockId)
    {
      return (Exception) new UnknownBlockTypeException("Unknown block type: 0x" + blockId.ToString("x2"));
    }

    public static Exception UnknownExtensionTypeException(int extensionLabel)
    {
      return (Exception) new UnknownExtensionTypeException("Unknown extension type: 0x" + extensionLabel.ToString("x2"));
    }

    public static Exception InvalidBlockSizeException(string blockName, int expectedBlockSize, int actualBlockSize)
    {
      return (Exception) new InvalidBlockSizeException(string.Format("Invalid block size for {0}. Expected {1}, but was {2}", blockName, expectedBlockSize, actualBlockSize));
    }

    public static Exception InvalidSignatureException(string signature)
    {
      return (Exception) new InvalidSignatureException("Invalid file signature: " + signature);
    }

    public static Exception UnsupportedVersionException(string version)
    {
      return (Exception) new UnsupportedGifVersionException("Unsupported version: " + version);
    }

    public static string GetString(byte[] bytes)
    {
      return GifHelpers.GetString(bytes, 0, bytes.Length);
    }

    public static string GetString(byte[] bytes, int index, int count)
    {
      return Encoding.UTF8.GetString(bytes, index, count);
    }
  }
}
