using System.Collections;

namespace XamlAnimatedGif.Extensions
{
  internal static class BitArrayExtensions
  {
    public static short ToInt16(this BitArray bitArray)
    {
      short num = 0;
      for (int index = bitArray.Length - 1; index >= 0; --index)
        num = (short) (((int) num << 1) + (bitArray[index] ? 1 : 0));
      return num;
    }
  }
}
