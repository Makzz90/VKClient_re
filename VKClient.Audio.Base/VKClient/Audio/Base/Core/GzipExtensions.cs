using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Resources;

namespace VKClient.Audio.Base.Core
{
  internal static class GzipExtensions
  {
    private static string GZIP = "gzip";

    public static void AddAcceptEncodingHeader(HttpWebRequest request)
    {
      if (request == null)
        throw new ArgumentNullException("request");
      request.Headers[HttpRequestHeader.AcceptEncoding] = GzipExtensions.GZIP;
    }

    public static IAsyncResult BeginGetCompressedResponse(this HttpWebRequest request, AsyncCallback callback, object state)
    {
      GzipExtensions.AddAcceptEncodingHeader(request);
      return request.BeginGetResponse(callback, state);
    }

    public static Stream GetCompressedResponseStream(this HttpWebResponse response)
    {
      if (response == null)
        throw new ArgumentNullException("response");
      Stream responseStream = ((WebResponse) response).GetResponseStream();
      if (!string.Equals(response.Headers[HttpRequestHeader.ContentEncoding], GzipExtensions.GZIP, StringComparison.OrdinalIgnoreCase))
        return responseStream;
      if (31 != responseStream.ReadByte() || 139 != responseStream.ReadByte() || 8 != responseStream.ReadByte())
        throw new NotSupportedException("Compressed data not in the expected format.");
      int num1 = responseStream.ReadByte();
      bool flag1 = (uint) (2 & num1) > 0U;
      int num2 = (uint) (4 & num1) > 0U ? 1 : 0;
      bool flag2 = (uint) (8 & num1) > 0U;
      bool flag3 = (uint) (16 & num1) > 0U;
      responseStream.ReadByte();
      responseStream.ReadByte();
      responseStream.ReadByte();
      responseStream.ReadByte();
      responseStream.ReadByte();
      responseStream.ReadByte();
      if (num2 != 0)
      {
        for (int index = responseStream.ReadByte() | responseStream.ReadByte() << 8; 0 < index; --index)
          responseStream.ReadByte();
      }
      if (flag2)
      {
        while (responseStream.ReadByte() != 0)
          ;
      }
      if (flag3)
      {
        while (responseStream.ReadByte() != 0)
          ;
      }
      if (flag1)
      {
        responseStream.ReadByte();
        responseStream.ReadByte();
      }
      List<byte[]> numArrayList = new List<byte[]>();
      byte[] buffer1 = new byte[4096];
      int offset = 0;
      int num3 = 0;
      int num4;
      do
      {
        if (buffer1.Length == offset)
        {
          numArrayList.Add(buffer1);
          buffer1 = new byte[buffer1.Length];
          offset = 0;
        }
        num4 = responseStream.Read(buffer1, offset, buffer1.Length - offset);
        offset += num4;
        num3 += num4;
      }
      while (0 < num4);
      numArrayList.Add(buffer1);
      int num5 = num3 - 4 - 4;
      if (num5 < 0)
        throw new NotSupportedException("Compressed data not in the expected format.");
      byte[] buffer2 = new byte[31 + num5 + 69];
      int num6 = 31;
      int val2 = num3;
      foreach (byte[] numArray1 in numArrayList)
      {
        int num7 = Math.Min(numArray1.Length, val2);
        int sourceIndex = 0;
        byte[] numArray2 = buffer2;
        int destinationIndex = num6;
        int length = num7;
        Array.Copy((Array) numArray1, sourceIndex, (Array) numArray2, destinationIndex, length);
        num6 += num7;
        val2 -= num7;
      }
      int index1 = 31 + num5;
      int num8 = (int) buffer2[index1] | (int) buffer2[index1 + 1] << 8 | (int) buffer2[index1 + 2] << 16 | (int) buffer2[index1 + 3] << 24;
      int num9 = (int) buffer2[index1 + 4] | (int) buffer2[index1 + 5] << 8 | (int) buffer2[index1 + 6] << 16 | (int) buffer2[index1 + 7] << 24;
      MemoryStream memoryStream = new MemoryStream(buffer2);
      BinaryWriter binaryWriter = new BinaryWriter((Stream) memoryStream);
      int num10 = 67324752;
      binaryWriter.Write((uint) num10);
      int num11 = 20;
      binaryWriter.Write((ushort) num11);
      int num12 = 0;
      binaryWriter.Write((ushort) num12);
      int num13 = 8;
      binaryWriter.Write((ushort) num13);
      int num14 = 0;
      binaryWriter.Write((ushort) num14);
      int num15 = 0;
      binaryWriter.Write((ushort) num15);
      int num16 = num8;
      binaryWriter.Write(num16);
      int num17 = num5;
      binaryWriter.Write(num17);
      int num18 = num9;
      binaryWriter.Write(num18);
      int num19 = 1;
      binaryWriter.Write((ushort) num19);
      int num20 = 0;
      binaryWriter.Write((ushort) num20);
      int num21 = (int) (byte) "f"[0];
      binaryWriter.Write((byte) num21);
      memoryStream.Seek((long) num5, SeekOrigin.Current);
      int num22 = 33639248;
      binaryWriter.Write((uint) num22);
      int num23 = 20;
      binaryWriter.Write((ushort) num23);
      int num24 = 20;
      binaryWriter.Write((ushort) num24);
      int num25 = 0;
      binaryWriter.Write((ushort) num25);
      int num26 = 8;
      binaryWriter.Write((ushort) num26);
      int num27 = 0;
      binaryWriter.Write((ushort) num27);
      int num28 = 0;
      binaryWriter.Write((ushort) num28);
      int num29 = num8;
      binaryWriter.Write(num29);
      int num30 = num5;
      binaryWriter.Write(num30);
      int num31 = num9;
      binaryWriter.Write(num31);
      int num32 = 1;
      binaryWriter.Write((ushort) num32);
      int num33 = 0;
      binaryWriter.Write((ushort) num33);
      int num34 = 0;
      binaryWriter.Write((ushort) num34);
      int num35 = 0;
      binaryWriter.Write((ushort) num35);
      int num36 = 0;
      binaryWriter.Write((ushort) num36);
      int num37 = 0;
      binaryWriter.Write((uint) num37);
      int num38 = 0;
      binaryWriter.Write((uint) num38);
      int num39 = (int) (byte) "f"[0];
      binaryWriter.Write((byte) num39);
      int num40 = 101010256;
      binaryWriter.Write((uint) num40);
      int num41 = 0;
      binaryWriter.Write((ushort) num41);
      int num42 = 0;
      binaryWriter.Write((ushort) num42);
      int num43 = 1;
      binaryWriter.Write((ushort) num43);
      int num44 = 1;
      binaryWriter.Write((ushort) num44);
      int num45 = 47;
      binaryWriter.Write((uint) num45);
      int num46 = 31 + num5;
      binaryWriter.Write((uint) num46);
      int num47 = 0;
      binaryWriter.Write((ushort) num47);
      memoryStream.Seek(0, SeekOrigin.Begin);
      Stopwatch stopwatch = Stopwatch.StartNew();
      Stream stream = Application.GetResourceStream(new StreamResourceInfo((Stream) memoryStream,  null), new Uri("f", UriKind.Relative)).Stream;
      stopwatch.Stop();
      GzipExtensions.LogCompression(memoryStream.Length, stream.Length);
      return stream;
    }

    private static void LogCompression(long compressedStreamLength, long decompressedStreamLength)
    {
      if (decompressedStreamLength <= 0L)
        return;
      double num = (double) compressedStreamLength / (double) decompressedStreamLength;
    }
  }
}
