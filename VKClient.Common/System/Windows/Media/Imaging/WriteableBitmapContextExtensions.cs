namespace System.Windows.Media.Imaging
{
  public static class WriteableBitmapContextExtensions
  {
    public static BitmapContext GetBitmapContext(this WriteableBitmap bmp)
    {
      return new BitmapContext(bmp);
    }

    public static BitmapContext GetBitmapContext(this WriteableBitmap bmp, ReadWriteMode mode)
    {
      return new BitmapContext(bmp, mode);
    }
  }
}
