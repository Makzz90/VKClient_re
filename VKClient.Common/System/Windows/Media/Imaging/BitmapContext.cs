namespace System.Windows.Media.Imaging
{
  public struct BitmapContext : IDisposable
  {
    private readonly WriteableBitmap writeableBitmap;

    public WriteableBitmap WriteableBitmap
    {
      get
      {
        return this.writeableBitmap;
      }
    }

    public int Width
    {
      get
      {
        return ((BitmapSource) this.writeableBitmap).PixelWidth;
      }
    }

    public int Height
    {
      get
      {
        return ((BitmapSource) this.writeableBitmap).PixelHeight;
      }
    }

    public int[] Pixels
    {
      get
      {
        return this.writeableBitmap.Pixels;
      }
    }

    public int Length
    {
      get
      {
        return this.writeableBitmap.Pixels.Length;
      }
    }

    public BitmapContext(WriteableBitmap writeableBitmap)
    {
      this.writeableBitmap = writeableBitmap;
    }

    public BitmapContext(WriteableBitmap writeableBitmap, ReadWriteMode mode)
    {
      this = new BitmapContext(writeableBitmap);
    }

    public static void BlockCopy(BitmapContext src, int srcOffset, BitmapContext dest, int destOffset, int count)
    {
      Buffer.BlockCopy((Array) src.Pixels, srcOffset, (Array) dest.Pixels, destOffset, count);
    }

    public static void BlockCopy(Array src, int srcOffset, BitmapContext dest, int destOffset, int count)
    {
      Buffer.BlockCopy(src, srcOffset, (Array) dest.Pixels, destOffset, count);
    }

    public static void BlockCopy(BitmapContext src, int srcOffset, Array dest, int destOffset, int count)
    {
      Buffer.BlockCopy((Array) src.Pixels, srcOffset, dest, destOffset, count);
    }

    public void Clear()
    {
      int[] pixels = this.writeableBitmap.Pixels;
      Array.Clear((Array) pixels, 0, pixels.Length);
    }

    public void Dispose()
    {
    }
  }
}
