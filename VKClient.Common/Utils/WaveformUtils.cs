using System;
using System.Collections.Generic;
using System.Linq;

namespace VKClient.Common.Utils
{
  public static class WaveformUtils
  {
    public static List<int> GetWaveform(short[] sampleBuffer)
    {
      int[] numArray1 = new int[256];
      long num1 = 0;
      int num2 = 0;
      int num3 = Math.Max(1, sampleBuffer.Length / 128);
      int num4 = 0;
      int num5 = 0;
      foreach (short num6 in sampleBuffer)
      {
        num4 += (int) num6;
        if (num1++ % (long) num3 == 0L)
        {
          int num7 = (int) Math.Round((double) Math.Abs(num4) / (double) num3);
          numArray1[num5++] = num7;
          num4 = 0;
          if (num2 < num7)
            num2 = num7;
        }
      }
      int[] numArray2 = new int[128];
      int num8 = num2 / 31;
      if (num8 > 0)
      {
        for (int index = 0; index < 128; ++index)
          numArray2[index] = (int) Math.Min(31.0, Math.Round((double) numArray1[index] / (double) num8));
      }
      return (List<int>) Enumerable.ToList<int>(numArray2);
    }

    public static List<int> Resample(List<int> source, int targetLength)
    {
      if (source == null || source.Count == 0 || source.Count == targetLength)
        return source;
      int[] numArray = new int[targetLength];
      if (source.Count < targetLength)
      {
        double num = (double) source.Count / (double) targetLength;
        for (int index = 0; index < targetLength; ++index)
          numArray[index] = source[(int) ((double) index * num)];
      }
      else
      {
        double val2 = (double) source.Count / (double) targetLength;
        double num1 = 0.0;
        double num2 = 0.0;
        int index = 0;
        List<int>.Enumerator enumerator = source.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            int current = enumerator.Current;
            double num3 = Math.Min(num2 + 1.0, val2) - num2;
            num1 += (double) current * num3;
            num2 += num3;
            if (num2 >= val2 - 0.001)
            {
              numArray[index++] = (int) Math.Round(num1 / val2);
              if (num3 < 1.0)
              {
                num2 = 1.0 - num3;
                num1 = (double) current * num2;
              }
              else
              {
                num2 = 0.0;
                num1 = 0.0;
              }
            }
          }
        }
        finally
        {
          enumerator.Dispose();
        }
        if (num1 > 0.0 && index < targetLength)
          numArray[index] = (int) Math.Round(num1 / val2);
      }
      return (List<int>) Enumerable.ToList<int>(numArray);
    }
  }
}
