using System.IO;
using VKClient_Opus;

namespace VKClient.Common.CommonExtensions
{
  public static class OpusExtensions
  {
    public static MemoryStream GetAsWavStream(string filePath, int sampleRate, int audioChannels, int bitsPerSample)
    {
      OpusRuntimeComponent runtimeComponent = new OpusRuntimeComponent();
      if (runtimeComponent.InitPlayer(filePath) != 1)
        return  null;
      byte[] buffer = new byte[16384];
      int[] args = new int[3];
      MemoryStream data = new MemoryStream();
      int num1;
      do
      {
        runtimeComponent.FillBuffer(buffer, buffer.Length, args);
        int count = args[0];
        int num2 = args[1];
        num1 = args[2] == 1 ? 1 : 0;
        data.Write(buffer, 0, count);
      }
      while (num1 == 0);
      return data.GetWavAsMemoryStream(sampleRate, audioChannels, bitsPerSample);
    }
  }
}
