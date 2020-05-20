using System;
using System.IO;
using System.Text;

namespace VKClient.Common.CommonExtensions
{
  public static class WavExtensions
  {
    public static MemoryStream GetWavAsMemoryStream(this Stream data, int sampleRate, int audioChannels, int bitsPerSample)
    {
      MemoryStream memoryStream = new MemoryStream();
      WavExtensions.WriteHeader((Stream) memoryStream, sampleRate, audioChannels, bitsPerSample);
      WavExtensions.SeekPastHeader((Stream) memoryStream);
      data.Position = 0L;
      data.CopyTo((Stream) memoryStream);
      WavExtensions.UpdateHeader((Stream) memoryStream);
      return memoryStream;
    }

    private static void WriteHeader(Stream stream, int sampleRate, int audioChannels = 1, int bitsPerSample = 16)
    {
      int num = bitsPerSample / 8;
      Encoding utF8 = Encoding.UTF8;
      long position = stream.Position;
      stream.Seek(0, SeekOrigin.Begin);
      stream.Write(utF8.GetBytes("RIFF"), 0, 4);
      stream.Write(BitConverter.GetBytes(0), 0, 4);
      stream.Write(utF8.GetBytes("WAVE"), 0, 4);
      stream.Write(utF8.GetBytes("fmt "), 0, 4);
      stream.Write(BitConverter.GetBytes(16), 0, 4);
      stream.Write(BitConverter.GetBytes(1), 0, 2);
      stream.Write(BitConverter.GetBytes((short) audioChannels), 0, 2);
      stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
      stream.Write(BitConverter.GetBytes(sampleRate * num * audioChannels), 0, 4);
      stream.Write(BitConverter.GetBytes((short) num), 0, 2);
      stream.Write(BitConverter.GetBytes((short) bitsPerSample), 0, 2);
      stream.Write(utF8.GetBytes("data"), 0, 4);
      stream.Write(BitConverter.GetBytes(0), 0, 4);
      WavExtensions.UpdateHeader(stream);
      stream.Seek(position, SeekOrigin.Begin);
    }

    private static void SeekPastHeader(Stream stream)
    {
      if (!stream.CanSeek)
        throw new Exception("Can't seek stream to update wav header");
      stream.Seek(44L, SeekOrigin.Begin);
    }

    private static void UpdateHeader(Stream stream)
    {
      if (!stream.CanSeek)
        throw new Exception("Can't seek stream to update wav header");
      long position = stream.Position;
      stream.Seek(4L, SeekOrigin.Begin);
      stream.Write(BitConverter.GetBytes((int) stream.Length - 8), 0, 4);
      stream.Seek(40L, SeekOrigin.Begin);
      stream.Write(BitConverter.GetBytes((int) stream.Length - 44), 0, 4);
      stream.Seek(position, SeekOrigin.Begin);
    }
  }
}
