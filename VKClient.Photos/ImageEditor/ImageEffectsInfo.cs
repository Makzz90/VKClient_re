using ExifLib;

namespace VKClient.Photos.ImageEditor
{
  public class ImageEffectsInfo
  {
    public byte[] Exif { get; set; }

    public JpegInfo ParsedExif { get; set; }

    public string AlbumId { get; set; }

    public int SeqNo { get; set; }

    public string Text { get; set; }

    public string Filter { get; set; }

    public double RotateAngle { get; set; }

    public CropRegion CropRect { get; set; }

    public bool Contrast { get; set; }

    public bool AppliedAny
    {
      get
      {
        if (string.IsNullOrEmpty(this.Text) && !(this.Filter != "Normal") && (this.RotateAngle == 0.0 && this.CropRect == null))
          return this.Contrast;
        return true;
      }
    }

    public ImageEffectsInfo()
    {
      this.Filter = "Normal";
    }

    public string GetUniqueKeyForFiltering()
    {
      return this.AlbumId + "_" + this.SeqNo + "_" + this.RotateAngle + "_" + (this.CropRect == null ? "NoCrop" : this.CropRect.ToString()) + this.Contrast.ToString();
    }
  }
}
