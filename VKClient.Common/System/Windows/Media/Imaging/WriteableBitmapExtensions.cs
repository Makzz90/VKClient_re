using System.IO;
using System.Reflection;

namespace System.Windows.Media.Imaging
{
  public static class WriteableBitmapExtensions
  {
    public static int[,] KernelGaussianBlur5x5 = new int[5, 5]
    {
      {
        1,
        4,
        7,
        4,
        1
      },
      {
        4,
        16,
        26,
        16,
        4
      },
      {
        7,
        26,
        41,
        26,
        7
      },
      {
        4,
        16,
        26,
        16,
        4
      },
      {
        1,
        4,
        7,
        4,
        1
      }
    };
    public static int[,] KernelGaussianBlur3x3 = new int[3, 3]
    {
      {
        16,
        26,
        16
      },
      {
        26,
        41,
        26
      },
      {
        16,
        26,
        16
      }
    };
    public static int[,] KernelSharpen3x3 = new int[3, 3]
    {
      {
        0,
        -2,
        0
      },
      {
        -2,
        11,
        -2
      },
      {
        0,
        -2,
        0
      }
    };
    internal const int SizeOfArgb = 4;

    private static int ConvertColor(Color color)
    {
      // ISSUE: explicit reference operation
      int num = (int) ((Color) @color).A + 1;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      return (int) ((Color) @color).A << 24 | (int) (byte) ((int) ((Color) @color).R * num >> 8) << 16 | (int) (byte) ((int) ((Color) @color).G * num >> 8) << 8 | (int) (byte) ((int) ((Color) @color).B * num >> 8);
    }

    public static void Clear(this WriteableBitmap bmp, Color color)
    {
      int num1 = WriteableBitmapExtensions.ConvertColor(color);
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int[] pixels = bitmapContext.Pixels;
        int width = bitmapContext.Width;
        int height = bitmapContext.Height;
        int num2 = width * 4;
        for (int index = 0; index < width; ++index)
          pixels[index] = num1;
        int num3 = 1;
        int num4 = 1;
        while (num4 < height)
        {
          BitmapContext.BlockCopy(bitmapContext, 0, bitmapContext, num4 * num2, num3 * num2);
          num4 += num3;
          num3 = Math.Min(2 * num3, height - num4);
        }
      }
    }

    public static void Clear(this WriteableBitmap bmp)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Clear();
    }

    public static WriteableBitmap Clone(this WriteableBitmap bmp)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
      {
        WriteableBitmap bmp1 = BitmapFactory.New(bitmapContext1.Width, bitmapContext1.Height);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          BitmapContext.BlockCopy(bitmapContext1, 0, bitmapContext2, 0, bitmapContext1.Length * 4);
        return bmp1;
      }
    }

    public static void ForEach(this WriteableBitmap bmp, Func<int, int, Color> func)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int[] pixels = bitmapContext.Pixels;
        int width = bitmapContext.Width;
        int height = bitmapContext.Height;
        int num = 0;
        for (int index1 = 0; index1 < height; ++index1)
        {
          for (int index2 = 0; index2 < width; ++index2)
          {
            Color color = func(index2, index1);
            pixels[num++] = WriteableBitmapExtensions.ConvertColor(color);
          }
        }
      }
    }

    public static void ForEach(this WriteableBitmap bmp, Func<int, int, Color, Color> func)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int[] pixels = bitmapContext.Pixels;
        int width = bitmapContext.Width;
        int height = bitmapContext.Height;
        int index1 = 0;
        for (int index2 = 0; index2 < height; ++index2)
        {
          for (int index3 = 0; index3 < width; ++index3)
          {
            int num = pixels[index1];
            Color color = func(index3, index2, Color.FromArgb((byte) (num >> 24), (byte) (num >> 16), (byte) (num >> 8), (byte) num));
            pixels[index1++] = WriteableBitmapExtensions.ConvertColor(color);
          }
        }
      }
    }

    public static int GetPixeli(this WriteableBitmap bmp, int x, int y)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        return bitmapContext.Pixels[y * bitmapContext.Width + x];
    }

    public static Color GetPixel(this WriteableBitmap bmp, int x, int y)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int pixel = bitmapContext.Pixels[y * bitmapContext.Width + x];
        int num1;
        int num2 = num1 = (int) (byte) (pixel >> 24);
        if (num2 == 0)
          num2 = 1;
        int num3 = 65280 / num2;
        int num4 = (int) (byte) ((pixel >> 16 & (int) byte.MaxValue) * num3 >> 8);
        int num5 = (int) (byte) ((pixel >> 8 & (int) byte.MaxValue) * num3 >> 8);
        int num6 = (int) (byte) ((pixel & (int) byte.MaxValue) * num3 >> 8);
        return Color.FromArgb((byte) num1, (byte) num4, (byte) num5, (byte) num6);
      }
    }

    public static byte GetBrightness(this WriteableBitmap bmp, int x, int y)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
      {
        int pixel = bitmapContext.Pixels[y * bitmapContext.Width + x];
        int num1 = 16;
        byte num2 = (byte) (pixel >> num1);
        int num3 = 8;
        byte num4 = (byte) (pixel >> num3);
        byte num5 = (byte) pixel;
        return (byte) ((int) num2 * 6966 + (int) num4 * 23436 + (int) num5 * 2366 >> 15);
      }
    }

    public static void SetPixeli(this WriteableBitmap bmp, int index, byte r, byte g, byte b)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[index] = -16777216 | (int) r << 16 | (int) g << 8 | (int) b;
    }

    public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte r, byte g, byte b)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[y * bitmapContext.Width + x] = -16777216 | (int) r << 16 | (int) g << 8 | (int) b;
    }

    public static void SetPixeli(this WriteableBitmap bmp, int index, byte a, byte r, byte g, byte b)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[index] = (int) a << 24 | (int) r << 16 | (int) g << 8 | (int) b;
    }

    public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte a, byte r, byte g, byte b)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[y * bitmapContext.Width + x] = (int) a << 24 | (int) r << 16 | (int) g << 8 | (int) b;
    }

    public static void SetPixeli(this WriteableBitmap bmp, int index, Color color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[index] = WriteableBitmapExtensions.ConvertColor(color);
    }

    public static void SetPixel(this WriteableBitmap bmp, int x, int y, Color color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[y * bitmapContext.Width + x] = WriteableBitmapExtensions.ConvertColor(color);
    }

    public static void SetPixeli(this WriteableBitmap bmp, int index, byte a, Color color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int num = (int) a + 1;
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        bitmapContext.Pixels[index] = (int) a << 24 | (int) (byte) ((int) ((Color) @color).R * num >> 8) << 16 | (int) (byte) ((int) ((Color) @color).G * num >> 8) << 8 | (int) (byte) ((int) ((Color) @color).B * num >> 8);
      }
    }

    public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte a, Color color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int num = (int) a + 1;
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        bitmapContext.Pixels[y * bitmapContext.Width + x] = (int) a << 24 | (int) (byte) ((int) ((Color) @color).R * num >> 8) << 16 | (int) (byte) ((int) ((Color) @color).G * num >> 8) << 8 | (int) (byte) ((int) ((Color) @color).B * num >> 8);
      }
    }

    public static void SetPixeli(this WriteableBitmap bmp, int index, int color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[index] = color;
    }

    public static void SetPixel(this WriteableBitmap bmp, int x, int y, int color)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
        bitmapContext.Pixels[y * bitmapContext.Width + x] = color;
    }

    public static void Blit(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect, WriteableBitmapExtensions.BlendMode BlendMode)
    {
      bmp.Blit(destRect, source, sourceRect, Colors.White, BlendMode);
    }

    public static void Blit(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect)
    {
      bmp.Blit(destRect, source, sourceRect, Colors.White, WriteableBitmapExtensions.BlendMode.Alpha);
    }

    public static void Blit(this WriteableBitmap bmp, Point destPosition, WriteableBitmap source, Rect sourceRect, Color color, WriteableBitmapExtensions.BlendMode BlendMode)
    {
      Rect destRect=new Rect(destPosition, new Size(((Rect) @sourceRect).Width, ((Rect) @sourceRect).Height));
      bmp.Blit(destRect, source, sourceRect, color, BlendMode);
    }

    public static void Blit(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect, Color color, WriteableBitmapExtensions.BlendMode BlendMode)
    {
      // ISSUE: explicit reference operation
      if ((int) ((Color) @color).A == 0)
        return;
      // ISSUE: explicit reference operation
      int width1 = (int) ((Rect) @destRect).Width;
      // ISSUE: explicit reference operation
      int height1 = (int) ((Rect) @destRect).Height;
      using (BitmapContext bitmapContext1 = source.GetBitmapContext(ReadWriteMode.ReadOnly))
      {
        using (BitmapContext bitmapContext2 = bmp.GetBitmapContext())
        {
          int width2 = bitmapContext1.Width;
          int width3 = bitmapContext2.Width;
          int height2 = bitmapContext2.Height;
          Rect rect=new Rect(0.0, 0.0, (double) width3, (double) height2);
          // ISSUE: explicit reference operation
          rect.Intersect(destRect);
          // ISSUE: explicit reference operation
          if (rect.IsEmpty)
            return;
          int[] pixels1 = bitmapContext1.Pixels;
          int[] pixels2 = bitmapContext2.Pixels;
          int length1 = bitmapContext1.Length;
          int length2 = bitmapContext2.Length;
          // ISSUE: explicit reference operation
          int x1 = (int) ((Rect) @destRect).X;
          // ISSUE: explicit reference operation
          int y1 = (int) ((Rect) @destRect).Y;
          int num1 = 0;
          int num2 = 0;
          int num3 = 0;
          int num4 = 0;
          // ISSUE: explicit reference operation
          int a = (int) ((Color) @color).A;
          // ISSUE: explicit reference operation
          int r = (int) ((Color) @color).R;
          // ISSUE: explicit reference operation
          int g = (int) ((Color) @color).G;
          // ISSUE: explicit reference operation
          int b = (int) ((Color) @color).B;
          bool flag = (color!= Colors.White);
          // ISSUE: explicit reference operation
          int width4 = (int) ((Rect) @sourceRect).Width;
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          double num5 = ((Rect) @sourceRect).Width / ((Rect) @destRect).Width;
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          double num6 = ((Rect) @sourceRect).Height / ((Rect) @destRect).Height;
          // ISSUE: explicit reference operation
          int x2 = (int) ((Rect) @sourceRect).X;
          // ISSUE: explicit reference operation
          int y2 = (int) ((Rect) @sourceRect).Y;
          int num7 = -1;
          int num8 = -1;
          double num9 = (double) y2;
          int num10 = y1;
          for (int index1 = 0; index1 < height1; ++index1)
          {
            if (num10 >= 0 && num10 < height2)
            {
              double num11 = (double) x2;
              int index2 = x1 + num10 * width3;
              int num12 = x1;
              int num13 = pixels1[0];
              if (BlendMode == WriteableBitmapExtensions.BlendMode.None && !flag)
              {
                int num14 = (int) num11 + (int) num9 * width2;
                int num15 = num12 < 0 ? -num12 : 0;
                int num16 = num12 + num15;
                int num17 = width2 - num15;
                int num18 = num16 + num17 < width3 ? num17 : width3 - num16;
                if (num18 > width4)
                  num18 = width4;
                if (num18 > width1)
                  num18 = width1;
                BitmapContext.BlockCopy(bitmapContext1, (num14 + num15) * 4, bitmapContext2, (index2 + num15) * 4, num18 * 4);
              }
              else
              {
                for (int index3 = 0; index3 < width1; ++index3)
                {
                  if (num12 >= 0 && num12 < width3)
                  {
                    if ((int) num11 != num7 || (int) num9 != num8)
                    {
                      int index4 = (int) num11 + (int) num9 * width2;
                      if (index4 >= 0 && index4 < length1)
                      {
                        num13 = pixels1[index4];
                        num4 = num13 >> 24 & (int) byte.MaxValue;
                        num1 = num13 >> 16 & (int) byte.MaxValue;
                        num2 = num13 >> 8 & (int) byte.MaxValue;
                        num3 = num13 & (int) byte.MaxValue;
                        if (flag && num4 != 0)
                        {
                          num4 = num4 * a * 32897 >> 23;
                          num1 = (num1 * r * 32897 >> 23) * a * 32897 >> 23;
                          num2 = (num2 * g * 32897 >> 23) * a * 32897 >> 23;
                          num3 = (num3 * b * 32897 >> 23) * a * 32897 >> 23;
                          num13 = num4 << 24 | num1 << 16 | num2 << 8 | num3;
                        }
                      }
                      else
                        num4 = 0;
                    }
                    if (BlendMode == WriteableBitmapExtensions.BlendMode.None)
                      pixels2[index2] = num13;
                    else if (BlendMode == WriteableBitmapExtensions.BlendMode.ColorKeying)
                    {
                      num1 = num13 >> 16 & (int) byte.MaxValue;
                      num2 = num13 >> 8 & (int) byte.MaxValue;
                      num3 = num13 & (int) byte.MaxValue;
                      // ISSUE: explicit reference operation
                      // ISSUE: explicit reference operation
                      // ISSUE: explicit reference operation
                      if (num1 != (int) ((Color) @color).R || num2 != (int) ((Color) @color).G || num3 != (int) ((Color) @color).B)
                        pixels2[index2] = num13;
                    }
                    else if (BlendMode == WriteableBitmapExtensions.BlendMode.Mask)
                    {
                      int num14 = pixels2[index2];
                      int num15 = num14 >> 24 & (int) byte.MaxValue;
                      int num16 = num14 >> 16 & (int) byte.MaxValue;
                      int num17 = num14 >> 8 & (int) byte.MaxValue;
                      int num18 = num14 & (int) byte.MaxValue;
                      int num19 = num15 * num4 * 32897 >> 23 << 24 | num16 * num4 * 32897 >> 23 << 16 | num17 * num4 * 32897 >> 23 << 8 | num18 * num4 * 32897 >> 23;
                      pixels2[index2] = num19;
                    }
                    else if (num4 > 0)
                    {
                      int num14 = pixels2[index2];
                      int num15 = num14 >> 24 & (int) byte.MaxValue;
                      if ((num4 == (int) byte.MaxValue || num15 == 0) && (BlendMode != WriteableBitmapExtensions.BlendMode.Additive && BlendMode != WriteableBitmapExtensions.BlendMode.Subtractive) && BlendMode != WriteableBitmapExtensions.BlendMode.Multiply)
                      {
                        pixels2[index2] = num13;
                      }
                      else
                      {
                        int num16 = num14 >> 16 & (int) byte.MaxValue;
                        int num17 = num14 >> 8 & (int) byte.MaxValue;
                        int num18 = num14 & (int) byte.MaxValue;
                        if (BlendMode == WriteableBitmapExtensions.BlendMode.Alpha)
                          num14 = ((num4 << 8) + ((int) byte.MaxValue - num4) * num15 >> 8 << 24) + ((num1 << 8) + ((int) byte.MaxValue - num4) * num16 >> 8 << 16) + ((num2 << 8) + ((int) byte.MaxValue - num4) * num17 >> 8 << 8) + ((num3 << 8) + ((int) byte.MaxValue - num4) * num18 >> 8);
                        else if (BlendMode == WriteableBitmapExtensions.BlendMode.Additive)
                        {
                          int num19 = (int) byte.MaxValue <= num4 + num15 ? (int) byte.MaxValue : num4 + num15;
                          num14 = num19 << 24 | (num19 <= num1 + num16 ? num19 : num1 + num16) << 16 | (num19 <= num2 + num17 ? num19 : num2 + num17) << 8 | (num19 <= num3 + num18 ? num19 : num3 + num18);
                        }
                        else if (BlendMode == WriteableBitmapExtensions.BlendMode.Subtractive)
                          num14 = num15 << 24 | (num1 >= num16 ? 0 : num1 - num16) << 16 | (num2 >= num17 ? 0 : num2 - num17) << 8 | (num3 >= num18 ? 0 : num3 - num18);
                        else if (BlendMode == WriteableBitmapExtensions.BlendMode.Multiply)
                        {
                          int num19 = num4 * num15 + 128;
                          int num20 = num1 * num16 + 128;
                          int num21 = num2 * num17 + 128;
                          int num22 = num3 * num18 + 128;
                          int num23 = (num19 >> 8) + num19 >> 8;
                          int num24 = (num20 >> 8) + num20 >> 8;
                          int num25 = (num21 >> 8) + num21 >> 8;
                          int num26 = (num22 >> 8) + num22 >> 8;
                          num14 = num23 << 24 | (num23 <= num24 ? num23 : num24) << 16 | (num23 <= num25 ? num23 : num25) << 8 | (num23 <= num26 ? num23 : num26);
                        }
                        pixels2[index2] = num14;
                      }
                    }
                  }
                  ++num12;
                  ++index2;
                  num11 += num5;
                }
              }
            }
            num9 += num6;
            ++num10;
          }
        }
      }
    }

    public static byte[] ToByteArray(this WriteableBitmap bmp, int offset, int count)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        if (count == -1)
          count = bitmapContext.Length;
        int count1 = count * 4;
        byte[] numArray = new byte[count1];
        BitmapContext.BlockCopy(bitmapContext, offset, (Array) numArray, 0, count1);
        return numArray;
      }
    }

    public static byte[] ToByteArray(this WriteableBitmap bmp, int count)
    {
      return bmp.ToByteArray(0, count);
    }

    public static byte[] ToByteArray(this WriteableBitmap bmp)
    {
      return bmp.ToByteArray(0, -1);
    }

    public static WriteableBitmap FromByteArray(this WriteableBitmap bmp, byte[] buffer, int offset, int count)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        BitmapContext.BlockCopy((Array) buffer, offset, bitmapContext, 0, count);
        return bmp;
      }
    }

    public static WriteableBitmap FromByteArray(this WriteableBitmap bmp, byte[] buffer, int count)
    {
      return bmp.FromByteArray(buffer, 0, count);
    }

    public static WriteableBitmap FromByteArray(this WriteableBitmap bmp, byte[] buffer)
    {
      return bmp.FromByteArray(buffer, 0, buffer.Length);
    }

    public static void WriteTga(this WriteableBitmap bmp, Stream destination)
    {
      using (BitmapContext bitmapContext = bmp.GetBitmapContext())
      {
        int width = bitmapContext.Width;
        int height = bitmapContext.Height;
        int[] pixels = bitmapContext.Pixels;
        byte[] buffer1 = new byte[bitmapContext.Length * 4];
        int index1 = 0;
        int num1 = width << 2;
        int num2 = width << 3;
        int index2 = (height - 1) * num1;
        for (int index3 = 0; index3 < height; ++index3)
        {
          for (int index4 = 0; index4 < width; ++index4)
          {
            int num3 = pixels[index1];
            buffer1[index2] = (byte) (num3 & (int) byte.MaxValue);
            buffer1[index2 + 1] = (byte) (num3 >> 8 & (int) byte.MaxValue);
            buffer1[index2 + 2] = (byte) (num3 >> 16 & (int) byte.MaxValue);
            buffer1[index2 + 3] = (byte) (num3 >> 24);
            ++index1;
            index2 += 4;
          }
          index2 -= num2;
        }
        byte[] numArray = new byte[18];
        numArray[2] = (byte) 2;
        numArray[12] = (byte) (width & (int) byte.MaxValue);
        numArray[13] = (byte) ((width & 65280) >> 8);
        numArray[14] = (byte) (height & (int) byte.MaxValue);
        numArray[15] = (byte) ((height & 65280) >> 8);
        numArray[16] = (byte) 32;
        byte[] buffer2 = numArray;
        using (BinaryWriter binaryWriter = new BinaryWriter(destination))
        {
          binaryWriter.Write(buffer2);
          binaryWriter.Write(buffer1);
        }
      }
    }

    public static WriteableBitmap FromResource(this WriteableBitmap bmp, string relativePath)
    {
      string name = new AssemblyName(Assembly.GetCallingAssembly().FullName).Name;
      return bmp.FromContent(name + ";component/" + relativePath);
    }

    public static WriteableBitmap FromContent(this WriteableBitmap bmp, string relativePath)
    {
      using (Stream stream1 = Application.GetResourceStream(new Uri(relativePath, UriKind.Relative)).Stream)
      {
        BitmapImage bitmapImage = new BitmapImage();
        Stream stream2 = stream1;
        ((BitmapSource) bitmapImage).SetSource(stream2);
        int num = 0;
        bitmapImage.CreateOptions = ((BitmapCreateOptions) num);
        bmp = new WriteableBitmap((BitmapSource) bitmapImage);
        // ISSUE: variable of the null type
        
        bitmapImage.UriSource=(null);
        return bmp;
      }
    }

    public static WriteableBitmap Convolute(this WriteableBitmap bmp, int[,] kernel)
    {
      int kernelFactorSum = 0;
      int[,] numArray = kernel;
      int upperBound1 = numArray.GetUpperBound(0);
      int upperBound2 = numArray.GetUpperBound(1);
      for (int lowerBound1 = numArray.GetLowerBound(0); lowerBound1 <= upperBound1; ++lowerBound1)
      {
        for (int lowerBound2 = numArray.GetLowerBound(1); lowerBound2 <= upperBound2; ++lowerBound2)
        {
          int num = numArray[lowerBound1, lowerBound2];
          kernelFactorSum += num;
        }
      }
      return bmp.Convolute(kernel, kernelFactorSum, 0);
    }

    public static WriteableBitmap Convolute(this WriteableBitmap bmp, int[,] kernel, int kernelFactorSum, int kernelOffsetSum)
    {
      int num1 = kernel.GetUpperBound(0) + 1;
      int num2 = kernel.GetUpperBound(1) + 1;
      if ((num2 & 1) == 0)
        throw new InvalidOperationException("Kernel width must be odd!");
      if ((num1 & 1) == 0)
        throw new InvalidOperationException("Kernel height must be odd!");
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
      {
        int width = bitmapContext1.Width;
        int height = bitmapContext1.Height;
        WriteableBitmap bmp1 = BitmapFactory.New(width, height);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
        {
          int[] pixels1 = bitmapContext1.Pixels;
          int[] pixels2 = bitmapContext2.Pixels;
          int num3 = 0;
          int num4 = num2 >> 1;
          int num5 = num1 >> 1;
          for (int index1 = 0; index1 < height; ++index1)
          {
            for (int index2 = 0; index2 < width; ++index2)
            {
              int num6 = 0;
              int num7 = 0;
              int num8 = 0;
              int num9 = 0;
              for (int index3 = -num4; index3 <= num4; ++index3)
              {
                int num10 = index3 + index2;
                if (num10 < 0)
                  num10 = 0;
                else if (num10 >= width)
                  num10 = width - 1;
                for (int index4 = -num5; index4 <= num5; ++index4)
                {
                  int num11 = index4 + index1;
                  if (num11 < 0)
                    num11 = 0;
                  else if (num11 >= height)
                    num11 = height - 1;
                  int num12 = pixels1[num11 * width + num10];
                  int num13 = kernel[index4 + num4, index3 + num5];
                  num6 += (num12 >> 24 & (int) byte.MaxValue) * num13;
                  num7 += (num12 >> 16 & (int) byte.MaxValue) * num13;
                  num8 += (num12 >> 8 & (int) byte.MaxValue) * num13;
                  num9 += (num12 & (int) byte.MaxValue) * num13;
                }
              }
              int num14 = num6 / kernelFactorSum + kernelOffsetSum;
              int num15 = num7 / kernelFactorSum + kernelOffsetSum;
              int num16 = num8 / kernelFactorSum + kernelOffsetSum;
              int num17 = num9 / kernelFactorSum + kernelOffsetSum;
              byte num18 = num14 > (int) byte.MaxValue ? byte.MaxValue : (num14 < 0 ? (byte) 0 : (byte) num14);
              byte num19 = num15 > (int) byte.MaxValue ? byte.MaxValue : (num15 < 0 ? (byte) 0 : (byte) num15);
              byte num20 = num16 > (int) byte.MaxValue ? byte.MaxValue : (num16 < 0 ? (byte) 0 : (byte) num16);
              byte num21 = num17 > (int) byte.MaxValue ? byte.MaxValue : (num17 < 0 ? (byte) 0 : (byte) num17);
              pixels2[num3++] = (int) num18 << 24 | (int) num19 << 16 | (int) num20 << 8 | (int) num21;
            }
          }
          return bmp1;
        }
      }
    }

    public static WriteableBitmap Invert(this WriteableBitmap bmp)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        WriteableBitmap bmp1 = BitmapFactory.New(bitmapContext1.Width, bitmapContext1.Height);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
        {
          int[] pixels1 = bitmapContext2.Pixels;
          int[] pixels2 = bitmapContext1.Pixels;
          int length = bitmapContext1.Length;
          for (int index = 0; index < length; ++index)
          {
            int num1 = pixels2[index];
            int num2 = 24;
            int num3 = num1 >> num2 & (int) byte.MaxValue;
            int num4 = 16;
            int num5 = num1 >> num4 & (int) byte.MaxValue;
            int num6 = 8;
            int num7 = num1 >> num6 & (int) byte.MaxValue;
            int maxValue = (int) byte.MaxValue;
            int num8 = num1 & maxValue;
            int num9 = (int) byte.MaxValue - num5;
            int num10 = (int) byte.MaxValue - num7;
            int num11 = (int) byte.MaxValue - num8;
            pixels1[index] = num3 << 24 | num9 << 16 | num10 << 8 | num11;
          }
          return bmp1;
        }
      }
    }

    public static WriteableBitmap Crop(this WriteableBitmap bmp, int x, int y, int width, int height)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        int width1 = bitmapContext1.Width;
        int height1 = bitmapContext1.Height;
        if (x > width1 || y > height1)
          return BitmapFactory.New(0, 0);
        if (x < 0)
          x = 0;
        if (x + width > width1)
          width = width1 - x;
        if (y < 0)
          y = 0;
        if (y + height > height1)
          height = height1 - y;
        WriteableBitmap bmp1 = BitmapFactory.New(width, height);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
        {
          for (int index = 0; index < height; ++index)
          {
            int srcOffset = ((y + index) * width1 + x) * 4;
            int destOffset = index * width * 4;
            BitmapContext.BlockCopy(bitmapContext1, srcOffset, bitmapContext2, destOffset, width * 4);
          }
          return bmp1;
        }
      }
    }

    public static WriteableBitmap Crop(this WriteableBitmap bmp, Rect region)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      return bmp.Crop((int) ((Rect) @region).X, (int) ((Rect) @region).Y, (int) ((Rect) @region).Width, (int) ((Rect) @region).Height);
    }

    public static WriteableBitmap Resize(this WriteableBitmap bmp, int width, int height, Interpolation interpolation)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        int[] numArray = WriteableBitmapExtensions.Resize(bitmapContext1, bitmapContext1.Width, bitmapContext1.Height, width, height, interpolation);
        WriteableBitmap bmp1 = BitmapFactory.New(width, height);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          BitmapContext.BlockCopy((Array) numArray, 0, bitmapContext2, 0, 4 * numArray.Length);
        return bmp1;
      }
    }

    public static int[] Resize(BitmapContext srcContext, int widthSource, int heightSource, int width, int height, Interpolation interpolation)
    {
      int[] pixels = srcContext.Pixels;
      int[] numArray = new int[width * height];
      float num1 = (float) widthSource / (float) width;
      float num2 = (float) heightSource / (float) height;
      if (interpolation == Interpolation.NearestNeighbor)
      {
        int num3 = 0;
        for (int index1 = 0; index1 < height; ++index1)
        {
          for (int index2 = 0; index2 < width; ++index2)
          {
            float num4 = (float) index2 * num1;
            double num5 = (double) index1 * (double) num2;
            int num6 = (int) num4;
            int num7 = (int) num5;
            numArray[num3++] = pixels[num7 * widthSource + num6];
          }
        }
      }
      else if (interpolation == Interpolation.Bilinear)
      {
        int num3 = 0;
        for (int index1 = 0; index1 < height; ++index1)
        {
          for (int index2 = 0; index2 < width; ++index2)
          {
            float num4 = (float) index2 * num1;
            double num5 = (double) index1 * (double) num2;
            int num6 = (int) num4;
            int num7 = (int) num5;
            float num8 = num4 - (float) num6;
            double num9 = (double) num7;
            float num10 = (float) (num5 - num9);
            double num11 = 1.0 - (double) num8;
            float num12 = 1f - num10;
            int num13 = num6 + 1;
            if (num13 >= widthSource)
              num13 = num6;
            int num14 = num7 + 1;
            if (num14 >= heightSource)
              num14 = num7;
            int num15 = pixels[num7 * widthSource + num6];
            int num16 = 24;
            byte num17 = (byte) (num15 >> num16);
            int num18 = 16;
            byte num19 = (byte) (num15 >> num18);
            int num20 = 8;
            byte num21 = (byte) (num15 >> num20);
            byte num22 = (byte) num15;
            int num23 = pixels[num7 * widthSource + num13];
            int num24 = 24;
            byte num25 = (byte) (num23 >> num24);
            int num26 = 16;
            byte num27 = (byte) (num23 >> num26);
            int num28 = 8;
            byte num29 = (byte) (num23 >> num28);
            byte num30 = (byte) num23;
            int num31 = pixels[num14 * widthSource + num6];
            int num32 = 24;
            byte num33 = (byte) (num31 >> num32);
            int num34 = 16;
            byte num35 = (byte) (num31 >> num34);
            int num36 = 8;
            byte num37 = (byte) (num31 >> num36);
            byte num38 = (byte) num31;
            int num39 = pixels[num14 * widthSource + num13];
            int num40 = 24;
            byte num41 = (byte) (num39 >> num40);
            int num42 = 16;
            byte num43 = (byte) (num39 >> num42);
            int num44 = 8;
            byte num45 = (byte) (num39 >> num44);
            byte num46 = (byte) num39;
            double num47 = (double) num17;
            float num48 = (float) (num11 * num47 + (double) num8 * (double) num25);
            double num49 = (double) num33;
            float num50 = (float) (num11 * num49 + (double) num8 * (double) num41);
            byte num51 = (byte) ((double) num12 * (double) num48 + (double) num10 * (double) num50);
            double num52 = (double) num19;
            float num53 = (float) (num11 * num52 * (double) num17 + (double) num8 * (double) num27 * (double) num25);
            double num54 = (double) num35;
            float num55 = (float) (num11 * num54 * (double) num33 + (double) num8 * (double) num43 * (double) num41);
            float num56 = (float) ((double) num12 * (double) num53 + (double) num10 * (double) num55);
            double num57 = (double) num21;
            float num58 = (float) (num11 * num57 * (double) num17 + (double) num8 * (double) num29 * (double) num25);
            double num59 = (double) num37;
            float num60 = (float) (num11 * num59 * (double) num33 + (double) num8 * (double) num45 * (double) num41);
            float num61 = (float) ((double) num12 * (double) num58 + (double) num10 * (double) num60);
            double num62 = (double) num22;
            float num63 = (float) (num11 * num62 * (double) num17 + (double) num8 * (double) num30 * (double) num25);
            double num64 = (double) num38;
            float num65 = (float) (num11 * num64 * (double) num33 + (double) num8 * (double) num46 * (double) num41);
            float num66 = (float) ((double) num12 * (double) num63 + (double) num10 * (double) num65);
            if ((int) num51 > 0)
            {
              num56 /= (float) num51;
              num61 /= (float) num51;
              num66 /= (float) num51;
            }
            byte num67 = (byte) num56;
            byte num68 = (byte) num61;
            byte num69 = (byte) num66;
            numArray[num3++] = (int) num51 << 24 | (int) num67 << 16 | (int) num68 << 8 | (int) num69;
          }
        }
      }
      return numArray;
    }

    public static WriteableBitmap Rotate(this WriteableBitmap bmp, int angle)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        int width = bitmapContext1.Width;
        int height = bitmapContext1.Height;
        int[] pixels1 = bitmapContext1.Pixels;
        int index1 = 0;
        angle %= 360;
        WriteableBitmap bmp1;
        if (angle > 0 && angle <= 90)
        {
          bmp1 = BitmapFactory.New(height, width);
          using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          {
            int[] pixels2 = bitmapContext2.Pixels;
            for (int index2 = 0; index2 < width; ++index2)
            {
              for (int index3 = height - 1; index3 >= 0; --index3)
              {
                int index4 = index3 * width + index2;
                pixels2[index1] = pixels1[index4];
                ++index1;
              }
            }
          }
        }
        else if (angle > 90 && angle <= 180)
        {
          bmp1 = BitmapFactory.New(width, height);
          using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          {
            int[] pixels2 = bitmapContext2.Pixels;
            for (int index2 = height - 1; index2 >= 0; --index2)
            {
              for (int index3 = width - 1; index3 >= 0; --index3)
              {
                int index4 = index2 * width + index3;
                pixels2[index1] = pixels1[index4];
                ++index1;
              }
            }
          }
        }
        else if (angle > 180 && angle <= 270)
        {
          bmp1 = BitmapFactory.New(height, width);
          using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          {
            int[] pixels2 = bitmapContext2.Pixels;
            for (int index2 = width - 1; index2 >= 0; --index2)
            {
              for (int index3 = 0; index3 < height; ++index3)
              {
                int index4 = index3 * width + index2;
                pixels2[index1] = pixels1[index4];
                ++index1;
              }
            }
          }
        }
        else
          bmp1 = bmp.Clone();
        return bmp1;
      }
    }

    public static WriteableBitmap RotateFree(this WriteableBitmap bmp, double angle, bool crop = true)
    {
      double num1 = -1.0 * Math.PI / 180.0 * angle;
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        int width1 = bitmapContext1.Width;
        int height = bitmapContext1.Height;
        int pixelWidth;
        int pixelHeight;
        if (crop)
        {
          pixelWidth = width1;
          pixelHeight = height;
        }
        else
        {
          double num2 = angle / (180.0 / Math.PI);
          pixelWidth = (int) Math.Ceiling(Math.Abs(Math.Sin(num2) * (double) height) + Math.Abs(Math.Cos(num2) * (double) width1));
          pixelHeight = (int) Math.Ceiling(Math.Abs(Math.Sin(num2) * (double) width1) + Math.Abs(Math.Cos(num2) * (double) height));
        }
        int num3 = width1 / 2;
        int num4 = height / 2;
        int num5 = pixelWidth / 2;
        int num6 = pixelHeight / 2;
        WriteableBitmap bmp1 = BitmapFactory.New(pixelWidth, pixelHeight);
        using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
        {
          int[] pixels1 = bitmapContext2.Pixels;
          int[] pixels2 = bitmapContext1.Pixels;
          int width2 = bitmapContext1.Width;
          for (int index1 = 0; index1 < pixelHeight; ++index1)
          {
            for (int index2 = 0; index2 < pixelWidth; ++index2)
            {
              int num2 = index2 - num5;
              int num7 = num6 - index1;
              double num8 = Math.Sqrt((double) (num2 * num2 + num7 * num7));
              double num9;
              if (num2 == 0)
              {
                if (num7 == 0)
                {
                  pixels1[index1 * pixelWidth + index2] = pixels2[num4 * width2 + num3];
                  continue;
                }
                num9 = num7 >= 0 ? Math.PI / 2.0 : 3.0 * Math.PI / 2.0;
              }
              else
                num9 = Math.Atan2((double) num7, (double) num2);
              double num10 = num9 - num1;
              double num11 = num8 * Math.Cos(num10);
              double num12 = num8 * Math.Sin(num10);
              double num13 = num11 + (double) num3;
              double num14 = (double) num4 - num12;
              int x1 = (int) Math.Floor(num13);
              int y1 = (int) Math.Floor(num14);
              int x2 = (int) Math.Ceiling(num13);
              int y2 = (int) Math.Ceiling(num14);
              if (x1 >= 0 && x2 >= 0 && (x1 < width1 && x2 < width1) && (y1 >= 0 && y2 >= 0 && (y1 < height && y2 < height)))
              {
                double num15 = num13 - (double) x1;
                double num16 = num14 - (double) y1;
                Color pixel1 = bmp.GetPixel(x1, y1);
                Color pixel2 = bmp.GetPixel(x2, y1);
                Color pixel3 = bmp.GetPixel(x1, y2);
                Color pixel4 = bmp.GetPixel(x2, y2);
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                double num17 = (1.0 - num15) * (double) ((Color) @pixel1).R + num15 * (double) ((Color) @pixel2).R;
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                double num18 = (1.0 - num15) * (double) ((Color) @pixel1).G + num15 * (double) ((Color) @pixel2).G;
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                double num19 = (1.0 - num15) * (double) ((Color) @pixel1).B + num15 * (double) ((Color) @pixel2).B;
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                double num20 = (1.0 - num15) * (double) ((Color) @pixel1).A + num15 * (double) ((Color) @pixel2).A;
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                double num21 = (1.0 - num15) * (double) ((Color) @pixel3).R + num15 * (double) ((Color) @pixel4).R;
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                double num22 = (1.0 - num15) * (double) ((Color) @pixel3).G + num15 * (double) ((Color) @pixel4).G;
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                double num23 = (1.0 - num15) * (double) ((Color) @pixel3).B + num15 * (double) ((Color) @pixel4).B;
                // ISSUE: explicit reference operation
                // ISSUE: explicit reference operation
                double num24 = (1.0 - num15) * (double) ((Color) @pixel3).A + num15 * (double) ((Color) @pixel4).A;
                int num25 = (int) Math.Round((1.0 - num16) * num17 + num16 * num21);
                int num26 = (int) Math.Round((1.0 - num16) * num18 + num16 * num22);
                int num27 = (int) Math.Round((1.0 - num16) * num19 + num16 * num23);
                int num28 = (int) Math.Round((1.0 - num16) * num20 + num16 * num24);
                if (num25 < 0)
                  num25 = 0;
                if (num25 > (int) byte.MaxValue)
                  num25 = (int) byte.MaxValue;
                if (num26 < 0)
                  num26 = 0;
                if (num26 > (int) byte.MaxValue)
                  num26 = (int) byte.MaxValue;
                if (num27 < 0)
                  num27 = 0;
                if (num27 > (int) byte.MaxValue)
                  num27 = (int) byte.MaxValue;
                if (num28 < 0)
                  num28 = 0;
                if (num28 > (int) byte.MaxValue)
                  num28 = (int) byte.MaxValue;
                int num29 = num28 + 1;
                pixels1[index1 * pixelWidth + index2] = num28 << 24 | (int) (byte) (num25 * num29 >> 8) << 16 | (int) (byte) (num26 * num29 >> 8) << 8 | (int) (byte) (num27 * num29 >> 8);
              }
            }
          }
          return bmp1;
        }
      }
    }

    public static WriteableBitmap Flip(this WriteableBitmap bmp, FlipMode flipMode)
    {
      using (BitmapContext bitmapContext1 = bmp.GetBitmapContext())
      {
        int width = bitmapContext1.Width;
        int height = bitmapContext1.Height;
        int[] pixels1 = bitmapContext1.Pixels;
        int index1 = 0;
        WriteableBitmap bmp1 =  null;
        if (flipMode == FlipMode.Horizontal)
        {
          bmp1 = BitmapFactory.New(width, height);
          using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          {
            int[] pixels2 = bitmapContext2.Pixels;
            for (int index2 = height - 1; index2 >= 0; --index2)
            {
              for (int index3 = 0; index3 < width; ++index3)
              {
                int index4 = index2 * width + index3;
                pixels2[index1] = pixels1[index4];
                ++index1;
              }
            }
          }
        }
        else if (flipMode == FlipMode.Vertical)
        {
          bmp1 = BitmapFactory.New(width, height);
          using (BitmapContext bitmapContext2 = bmp1.GetBitmapContext())
          {
            int[] pixels2 = bitmapContext2.Pixels;
            for (int index2 = 0; index2 < height; ++index2)
            {
              for (int index3 = width - 1; index3 >= 0; --index3)
              {
                int index4 = index2 * width + index3;
                pixels2[index1] = pixels1[index4];
                ++index1;
              }
            }
          }
        }
        return bmp1;
      }
    }

    public enum BlendMode
    {
      Alpha,
      Additive,
      Subtractive,
      Mask,
      Multiply,
      ColorKeying,
      None,
    }
  }
}
