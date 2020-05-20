using System;

namespace MediaParsers
{
  public static class BitTools
  {
    private static readonly char[] DecToHexConv = new char[16]
    {
      '0',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
      'A',
      'B',
      'C',
      'D',
      'E',
      'F'
    };
    private const byte MaskTail = 15;
    private const byte MaskHead = 240;
    private const int SyncSafeIntegerSize = 4;
    private const int ByteSize = 8;

    public static int MaskBits(byte[] data, int firstBit, int maskSize)
    {
      if (data == null)
        throw new ArgumentNullException("data");
      if (data.Length == 0 || firstBit < 0 || maskSize <= 0)
        throw new ArgumentException("data array, firstBit, or maskSize are too small");
      if (firstBit + maskSize > data.Length * 8)
        throw new ArgumentException("Attempting to mask outside of the data array");
      if (maskSize > 32)
        throw new ArgumentException("maskSize is larger than an integer");
      int num1 = firstBit / 8;
      int num2 = (firstBit + maskSize - 1) / 8;
      int num3 = 0;
      for (int index = 0; index < maskSize; ++index)
        num3 |= 1 << index;
      long num4 = 0;
      for (int index = num1; index <= num2; ++index)
      {
        long num5 = (long) data[index] << (num2 - index) * 8;
        num4 |= num5;
      }
      return (int) (num4 >> (8 * (num2 + 1) - (firstBit + maskSize)) % 8 & (long) num3);
    }

    public static int ConvertSyncSafeToInt32(byte[] syncSafeData, int startIndex)
    {
      int num1 = 0;
      if (syncSafeData == null)
        throw new ArgumentNullException("syncSafeData");
      if (startIndex < 0 || startIndex >= syncSafeData.Length)
        throw new ArgumentOutOfRangeException("startIndex", "startIndex is outside of the syncSafeData array");
      if (syncSafeData.Length < 4)
        throw new ArgumentException("syncSafeData array is smaller than an integer(4 bytes)", "syncSafeData");
      if (startIndex + 4 - 1 >= syncSafeData.Length)
        throw new ArgumentOutOfRangeException("startIndex", "This startIndex is too close to the end of the data array");
      int num2;
      for (num2 = 0; num2 < 3; ++num2)
      {
        int num3 = (int) syncSafeData[startIndex + num2];
        int num4 = 7 * (3 - num2);
        num1 |= num3 << num4;
      }
      int num5 = (int) syncSafeData[startIndex + num2];
      return num1 | num5;
    }

    public static int FindBitPattern(byte[] data, byte[] pattern, byte[] mask, int startIndex)
    {
      if (data == null)
        throw new ArgumentNullException("data");
      if (pattern == null)
        throw new ArgumentNullException("pattern");
      if (mask == null)
        throw new ArgumentNullException("mask");
      if (pattern.Length == 0 || data.Length == 0 || (data.Length < pattern.Length || mask.Length != pattern.Length))
        return -1;
      if (startIndex < 0 || startIndex >= data.Length)
        throw new ArgumentOutOfRangeException("startIndex", "Start index must be in the range [0,data.Length-1]");
      int index1 = startIndex;
      int index2 = 0;
      while (index1 < data.Length)
      {
        if ((int) pattern[index2] == ((int) data[index1] & (int) mask[index2]))
          ++index2;
        else if ((int) pattern[index2] != ((int) data[index1] & (int) mask[index2]))
        {
          if (index2 > 0)
            index1 -= index2;
          index2 = 0;
        }
        ++index1;
        if (index2 == pattern.Length)
          return index1 - pattern.Length;
      }
      return -1;
    }

    public static int FindBitPattern(byte[] data, byte[] pattern, byte[] mask)
    {
      return BitTools.FindBitPattern(data, pattern, mask, 0);
    }

    public static int FindBytePattern(byte[] data, byte[] pattern, int startIndex)
    {
      if (pattern == null)
        throw new ArgumentNullException("pattern");
      byte[] mask = new byte[pattern.Length];
      for (int index = 0; index < pattern.Length; ++index)
        mask[index] = byte.MaxValue;
      return BitTools.FindBitPattern(data, pattern, mask, startIndex);
    }

    public static int FindBytePattern(byte[] data, byte[] pattern)
    {
      return BitTools.FindBytePattern(data, pattern, 0);
    }

    public static void ToHexHelper(byte sizeOfField, long fieldData, int startIndex, char[] chars)
    {
      int num = 0;
      while (num < (int) sizeOfField)
      {
        chars[startIndex] = BitTools.DecToHexConv[(fieldData & 240L) >> 4];
        chars[startIndex + 1] = BitTools.DecToHexConv[fieldData & 15L];
        num += 2;
        fieldData >>= 8;
        startIndex += 2;
      }
    }
  }
}
