namespace System.Windows.Media.Imaging
{
  public static class BitmapFactory
  {
    public static WriteableBitmap New(int pixelWidth, int pixelHeight)
    {
      return new WriteableBitmap(pixelWidth, pixelHeight);
    }
  }
}
