using System;

namespace ExifLib
{
  public class JpegInfo
  {
    public double[] GpsLatitude = new double[3];
    public double[] GpsLongitude = new double[3];
    public string FileName;
    public int FileSize;
    public bool IsValid;
    public int Width;
    public int Height;
    public bool IsColor;
    public ExifOrientation Orientation;
    public double XResolution;
    public double YResolution;
    public ExifUnit ResolutionUnit;
    public string DateTime;
    public string Description;
    public string Make;
    public string Model;
    public string Software;
    public string Artist;
    public string Copyright;
    public string UserComment;
    public double ExposureTime;
    public double FNumber;
    public ExifFlash Flash;
    public ExifGpsLatitudeRef GpsLatitudeRef;
    public ExifGpsLongitudeRef GpsLongitudeRef;
    public int ThumbnailOffset;
    public int ThumbnailSize;
    public byte[] ThumbnailData;
    public TimeSpan LoadTime;

    public long OrientationOffset { get; set; }

    public bool LittleEndian { get; set; }
  }
}
