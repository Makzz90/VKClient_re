namespace VKClient.Photos.ImageEditor
{
  public class CropRegion
  {
    public int X { get; set; }

    public int Y { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public override string ToString()
    {
      return this.X.ToString() + "_" + this.Y + "_" + this.Width + "_" + this.Height;
    }
  }
}
