using System;
using System.Text;

public sealed class MD5Core
{
  private MD5Core()
  {
  }

  public static byte[] GetHash(string input, Encoding encoding)
  {
    if (input == null)
      throw new ArgumentNullException("input", "Unable to calculate hash over null input data");
    if (encoding == null)
      throw new ArgumentNullException("encoding", "Unable to calculate hash over a string without a default encoding. Consider using the GetHash(string) overload to use UTF8 Encoding");
    return MD5Core.GetHash(encoding.GetBytes(input));
  }

  public static byte[] GetHash(string input)
  {
    return MD5Core.GetHash(input, (Encoding) new UTF8Encoding());
  }

  public static string GetHashString(byte[] input)
  {
    if (input == null)
      throw new ArgumentNullException("input", "Unable to calculate hash over null input data");
    return BitConverter.ToString(MD5Core.GetHash(input)).Replace("-", "");
  }

  public static string GetHashString(string input, Encoding encoding)
  {
    if (input == null)
      throw new ArgumentNullException("input", "Unable to calculate hash over null input data");
    if (encoding == null)
      throw new ArgumentNullException("encoding", "Unable to calculate hash over a string without a default encoding. Consider using the GetHashString(string) overload to use UTF8 Encoding");
    return MD5Core.GetHashString(encoding.GetBytes(input));
  }

  public static string GetHashString(string input)
  {
    return MD5Core.GetHashString(input, (Encoding) new UTF8Encoding());
  }

  public static byte[] GetHash(byte[] input)
  {
    if (input == null)
      throw new ArgumentNullException("input", "Unable to calculate hash over null input data");
    ABCDStruct ABCDValue = new ABCDStruct();
    ABCDValue.A = 1732584193U;
    ABCDValue.B = 4023233417U;
    ABCDValue.C = 2562383102U;
    ABCDValue.D = 271733878U;
    int ibStart = 0;
    while (ibStart <= input.Length - 64)
    {
      MD5Core.GetHashBlock(input, ref ABCDValue, ibStart);
      ibStart += 64;
    }
    return MD5Core.GetHashFinalBlock(input, ibStart, input.Length - ibStart, ABCDValue, (long) input.Length * 8L);
  }

  internal static byte[] GetHashFinalBlock(byte[] input, int ibStart, int cbSize, ABCDStruct ABCD, long len)
  {
    byte[] input1 = new byte[64];
    byte[] bytes = BitConverter.GetBytes(len);
    Array.Copy((Array) input, ibStart, (Array) input1, 0, cbSize);
    input1[cbSize] = (byte) 128;
    if (cbSize < 56)
    {
      Array.Copy((Array) bytes, 0, (Array) input1, 56, 8);
      MD5Core.GetHashBlock(input1, ref ABCD, 0);
    }
    else
    {
      MD5Core.GetHashBlock(input1, ref ABCD, 0);
      byte[] input2 = new byte[64];
      Array.Copy((Array) bytes, 0, (Array) input2, 56, 8);
      MD5Core.GetHashBlock(input2, ref ABCD, 0);
    }
    byte[] numArray = new byte[16];
    Array.Copy((Array) BitConverter.GetBytes(ABCD.A), 0, (Array) numArray, 0, 4);
    Array.Copy((Array) BitConverter.GetBytes(ABCD.B), 0, (Array) numArray, 4, 4);
    Array.Copy((Array) BitConverter.GetBytes(ABCD.C), 0, (Array) numArray, 8, 4);
    Array.Copy((Array) BitConverter.GetBytes(ABCD.D), 0, (Array) numArray, 12, 4);
    return numArray;
  }

  internal static void GetHashBlock(byte[] input, ref ABCDStruct ABCDValue, int ibStart)
  {
    uint[] numArray = MD5Core.Converter(input, ibStart);
    uint a = ABCDValue.A;
    uint b1 = ABCDValue.B;
    uint c = ABCDValue.C;
    uint d = ABCDValue.D;
    uint num1 = MD5Core.r1(a, b1, c, d, numArray[0], 7, 3614090360U);
    uint num2 = MD5Core.r1(d, num1, b1, c, numArray[1], 12, 3905402710U);
    uint num3 = MD5Core.r1(c, num2, num1, b1, numArray[2], 17, 606105819U);
    uint num4 = MD5Core.r1(b1, num3, num2, num1, numArray[3], 22, 3250441966U);
    uint num5 = MD5Core.r1(num1, num4, num3, num2, numArray[4], 7, 4118548399U);
    uint num6 = MD5Core.r1(num2, num5, num4, num3, numArray[5], 12, 1200080426U);
    uint num7 = MD5Core.r1(num3, num6, num5, num4, numArray[6], 17, 2821735955U);
    uint num8 = MD5Core.r1(num4, num7, num6, num5, numArray[7], 22, 4249261313U);
    uint num9 = MD5Core.r1(num5, num8, num7, num6, numArray[8], 7, 1770035416U);
    uint num10 = MD5Core.r1(num6, num9, num8, num7, numArray[9], 12, 2336552879U);
    uint num11 = MD5Core.r1(num7, num10, num9, num8, numArray[10], 17, 4294925233U);
    uint num12 = MD5Core.r1(num8, num11, num10, num9, numArray[11], 22, 2304563134U);
    uint num13 = MD5Core.r1(num9, num12, num11, num10, numArray[12], 7, 1804603682U);
    uint num14 = MD5Core.r1(num10, num13, num12, num11, numArray[13], 12, 4254626195U);
    uint num15 = MD5Core.r1(num11, num14, num13, num12, numArray[14], 17, 2792965006U);
    uint num16 = MD5Core.r1(num12, num15, num14, num13, numArray[15], 22, 1236535329U);
    uint num17 = MD5Core.r2(num13, num16, num15, num14, numArray[1], 5, 4129170786U);
    uint num18 = MD5Core.r2(num14, num17, num16, num15, numArray[6], 9, 3225465664U);
    uint num19 = MD5Core.r2(num15, num18, num17, num16, numArray[11], 14, 643717713U);
    uint num20 = MD5Core.r2(num16, num19, num18, num17, numArray[0], 20, 3921069994U);
    uint num21 = MD5Core.r2(num17, num20, num19, num18, numArray[5], 5, 3593408605U);
    uint num22 = MD5Core.r2(num18, num21, num20, num19, numArray[10], 9, 38016083U);
    uint num23 = MD5Core.r2(num19, num22, num21, num20, numArray[15], 14, 3634488961U);
    uint num24 = MD5Core.r2(num20, num23, num22, num21, numArray[4], 20, 3889429448U);
    uint num25 = MD5Core.r2(num21, num24, num23, num22, numArray[9], 5, 568446438U);
    uint num26 = MD5Core.r2(num22, num25, num24, num23, numArray[14], 9, 3275163606U);
    uint num27 = MD5Core.r2(num23, num26, num25, num24, numArray[3], 14, 4107603335U);
    uint num28 = MD5Core.r2(num24, num27, num26, num25, numArray[8], 20, 1163531501U);
    uint num29 = MD5Core.r2(num25, num28, num27, num26, numArray[13], 5, 2850285829U);
    uint num30 = MD5Core.r2(num26, num29, num28, num27, numArray[2], 9, 4243563512U);
    uint num31 = MD5Core.r2(num27, num30, num29, num28, numArray[7], 14, 1735328473U);
    uint num32 = MD5Core.r2(num28, num31, num30, num29, numArray[12], 20, 2368359562U);
    uint num33 = MD5Core.r3(num29, num32, num31, num30, numArray[5], 4, 4294588738U);
    uint num34 = MD5Core.r3(num30, num33, num32, num31, numArray[8], 11, 2272392833U);
    uint num35 = MD5Core.r3(num31, num34, num33, num32, numArray[11], 16, 1839030562U);
    uint num36 = MD5Core.r3(num32, num35, num34, num33, numArray[14], 23, 4259657740U);
    uint num37 = MD5Core.r3(num33, num36, num35, num34, numArray[1], 4, 2763975236U);
    uint num38 = MD5Core.r3(num34, num37, num36, num35, numArray[4], 11, 1272893353U);
    uint num39 = MD5Core.r3(num35, num38, num37, num36, numArray[7], 16, 4139469664U);
    uint num40 = MD5Core.r3(num36, num39, num38, num37, numArray[10], 23, 3200236656U);
    uint num41 = MD5Core.r3(num37, num40, num39, num38, numArray[13], 4, 681279174U);
    uint num42 = MD5Core.r3(num38, num41, num40, num39, numArray[0], 11, 3936430074U);
    uint num43 = MD5Core.r3(num39, num42, num41, num40, numArray[3], 16, 3572445317U);
    uint num44 = MD5Core.r3(num40, num43, num42, num41, numArray[6], 23, 76029189U);
    uint num45 = MD5Core.r3(num41, num44, num43, num42, numArray[9], 4, 3654602809U);
    uint num46 = MD5Core.r3(num42, num45, num44, num43, numArray[12], 11, 3873151461U);
    uint num47 = MD5Core.r3(num43, num46, num45, num44, numArray[15], 16, 530742520U);
    uint num48 = MD5Core.r3(num44, num47, num46, num45, numArray[2], 23, 3299628645U);
    uint num49 = MD5Core.r4(num45, num48, num47, num46, numArray[0], 6, 4096336452U);
    uint num50 = MD5Core.r4(num46, num49, num48, num47, numArray[7], 10, 1126891415U);
    uint num51 = MD5Core.r4(num47, num50, num49, num48, numArray[14], 15, 2878612391U);
    uint num52 = MD5Core.r4(num48, num51, num50, num49, numArray[5], 21, 4237533241U);
    uint num53 = MD5Core.r4(num49, num52, num51, num50, numArray[12], 6, 1700485571U);
    uint num54 = MD5Core.r4(num50, num53, num52, num51, numArray[3], 10, 2399980690U);
    uint num55 = MD5Core.r4(num51, num54, num53, num52, numArray[10], 15, 4293915773U);
    uint num56 = MD5Core.r4(num52, num55, num54, num53, numArray[1], 21, 2240044497U);
    uint num57 = MD5Core.r4(num53, num56, num55, num54, numArray[8], 6, 1873313359U);
    uint num58 = MD5Core.r4(num54, num57, num56, num55, numArray[15], 10, 4264355552U);
    uint num59 = MD5Core.r4(num55, num58, num57, num56, numArray[6], 15, 2734768916U);
    uint num60 = MD5Core.r4(num56, num59, num58, num57, numArray[13], 21, 1309151649U);
    uint num61 = MD5Core.r4(num57, num60, num59, num58, numArray[4], 6, 4149444226U);
    uint num62 = MD5Core.r4(num58, num61, num60, num59, numArray[11], 10, 3174756917U);
    uint b2 = MD5Core.r4(num59, num62, num61, num60, numArray[2], 15, 718787259U);
    uint num63 = MD5Core.r4(num60, b2, num62, num61, numArray[9], 21, 3951481745U);
    ABCDValue.A = num61 + ABCDValue.A;
    ABCDValue.B = num63 + ABCDValue.B;
    ABCDValue.C = b2 + ABCDValue.C;
    ABCDValue.D = num62 + ABCDValue.D;
  }

  private static uint r1(uint a, uint b, uint c, uint d, uint x, int s, uint t)
  {
    return b + MD5Core.LSR(a + (uint) ((int) b & (int) c | ((int) b ^ -1) & (int) d) + x + t, s);
  }

  private static uint r2(uint a, uint b, uint c, uint d, uint x, int s, uint t)
  {
    return b + MD5Core.LSR(a + (uint) ((int) b & (int) d | (int) c & ((int) d ^ -1)) + x + t, s);
  }

  private static uint r3(uint a, uint b, uint c, uint d, uint x, int s, uint t)
  {
    return b + MD5Core.LSR(a + (b ^ c ^ d) + x + t, s);
  }

  private static uint r4(uint a, uint b, uint c, uint d, uint x, int s, uint t)
  {
    return b + MD5Core.LSR(a + (c ^ (b | d ^ uint.MaxValue)) + x + t, s);
  }

  private static uint LSR(uint i, int s)
  {
    return i << s | i >> 32 - s;
  }

  private static uint[] Converter(byte[] input, int ibStart)
  {
    if (input == null)
      throw new ArgumentNullException("input", "Unable convert null array to array of uInts");
    uint[] numArray = new uint[16];
    for (int index = 0; index < 16; ++index)
    {
      numArray[index] = (uint) input[ibStart + index * 4];
      numArray[index] += (uint) input[ibStart + index * 4 + 1] << 8;
      numArray[index] += (uint) input[ibStart + index * 4 + 2] << 16;
      numArray[index] += (uint) input[ibStart + index * 4 + 3] << 24;
    }
    return numArray;
  }
}
