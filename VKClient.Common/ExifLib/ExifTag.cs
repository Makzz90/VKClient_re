using System;
using System.Text;

namespace ExifLib
{
  public class ExifTag
  {
    private static int[] BytesPerFormat = new int[13]
    {
      0,
      1,
      1,
      2,
      4,
      8,
      1,
      1,
      2,
      4,
      8,
      4,
      8
    };

    public int Tag { get; private set; }

    public ExifTagFormat Format { get; private set; }

    public int Components { get; private set; }

    public byte[] Data { get; private set; }

    public bool LittleEndian { get; private set; }

    public long ValueOffset { get; set; }

    public bool IsValid { get; private set; }

    public bool IsNumeric
    {
      get
      {
        switch (this.Format)
        {
          case ExifTagFormat.STRING:
          case ExifTagFormat.UNDEFINED:
            return false;
          default:
            return true;
        }
      }
    }

    public ExifTag(byte[] section, int sectionOffset, int offsetBase, int length, bool littleEndian)
    {
      this.IsValid = false;
      this.Tag = (int) ExifIO.ReadUShort(section, sectionOffset, littleEndian);
      int index = (int) ExifIO.ReadUShort(section, sectionOffset + 2, littleEndian);
      if (index < 1 || index > 12)
        return;
      this.Format = (ExifTagFormat) index;
      this.Components = ExifIO.ReadInt(section, sectionOffset + 4, littleEndian);
      if (this.Components > 65536)
        return;
      this.LittleEndian = littleEndian;
      int length1 = this.Components * ExifTag.BytesPerFormat[index];
      int sourceIndex;
      if (length1 > 4)
      {
        int num = ExifIO.ReadInt(section, sectionOffset + 8, littleEndian);
        if (num + length1 > length)
          return;
        sourceIndex = offsetBase + num;
      }
      else
        sourceIndex = sectionOffset + 8;
      this.Data = new byte[length1];
      this.ValueOffset = (long) sourceIndex;
      Array.Copy((Array) section, sourceIndex, (Array) this.Data, 0, length1);
      this.IsValid = true;
    }

    private short ReadShort(int offset)
    {
      return ExifIO.ReadShort(this.Data, offset, this.LittleEndian);
    }

    private ushort ReadUShort(int offset)
    {
      return ExifIO.ReadUShort(this.Data, offset, this.LittleEndian);
    }

    private int ReadInt(int offset)
    {
      return ExifIO.ReadInt(this.Data, offset, this.LittleEndian);
    }

    private uint ReadUInt(int offset)
    {
      return ExifIO.ReadUInt(this.Data, offset, this.LittleEndian);
    }

    private float ReadSingle(int offset)
    {
      return ExifIO.ReadSingle(this.Data, offset, this.LittleEndian);
    }

    private double ReadDouble(int offset)
    {
      return ExifIO.ReadDouble(this.Data, offset, this.LittleEndian);
    }

    public int GetInt(int componentIndex)
    {
      return (int) this.GetNumericValue(componentIndex);
    }

    public double GetNumericValue(int componentIndex)
    {
      switch (this.Format)
      {
        case ExifTagFormat.BYTE:
          return (double) this.Data[componentIndex];
        case ExifTagFormat.USHORT:
          return (double) this.ReadUShort(componentIndex * 2);
        case ExifTagFormat.ULONG:
          return (double) this.ReadUInt(componentIndex * 4);
        case ExifTagFormat.URATIONAL:
          return (double) this.ReadUInt(componentIndex * 8) / (double) this.ReadUInt(componentIndex * 8 + 4);
        case ExifTagFormat.SBYTE:
          return (double) (sbyte) this.Data[componentIndex];
        case ExifTagFormat.SSHORT:
          return (double) this.ReadShort(componentIndex * 2);
        case ExifTagFormat.SLONG:
          return (double) this.ReadInt(componentIndex * 4);
        case ExifTagFormat.SRATIONAL:
          return (double) this.ReadInt(componentIndex * 8) / (double) this.ReadInt(componentIndex * 8 + 4);
        case ExifTagFormat.SINGLE:
          return (double) this.ReadSingle(componentIndex * 4);
        case ExifTagFormat.DOUBLE:
          return this.ReadDouble(componentIndex * 8);
        default:
          return 0.0;
      }
    }

    public string GetStringValue()
    {
      return this.GetStringValue(0);
    }

    public string GetStringValue(int componentIndex)
    {
      switch (this.Format)
      {
        case ExifTagFormat.UNDEFINED:
        case ExifTagFormat.STRING:
          return Encoding.UTF8.GetString(this.Data, 0, this.Data.Length).Trim(' ', '\t', '\r', '\n', char.MinValue);
        case ExifTagFormat.SRATIONAL:
          return this.ReadInt(componentIndex * 8).ToString() + "/" + this.ReadInt(componentIndex * 8 + 4).ToString();
        case ExifTagFormat.URATIONAL:
          return this.ReadUInt(componentIndex * 8).ToString() + "/" + this.ReadUInt(componentIndex * 8 + 4).ToString();
        default:
          return this.GetNumericValue(componentIndex).ToString();
      }
    }

    public virtual void Populate(JpegInfo info, ExifIFD ifd, long sectionStart)
    {
      if (ifd == ExifIFD.Exif)
      {
        switch ((ExifId) this.Tag)
        {
          case ExifId.FlashUsed:
            info.Flash = (ExifFlash) this.GetInt(0);
            break;
          case ExifId.UserComment:
            info.UserComment = this.GetStringValue();
            break;
          case ExifId.ExposureTime:
            info.ExposureTime = this.GetNumericValue(0);
            break;
          case ExifId.FNumber:
            info.FNumber = this.GetNumericValue(0);
            break;
          case ExifId.ThumbnailLength:
            info.ThumbnailSize = this.GetInt(0);
            break;
          case ExifId.Copyright:
            info.Copyright = this.GetStringValue();
            break;
          case ExifId.Artist:
            info.Artist = this.GetStringValue();
            break;
          case ExifId.ThumbnailOffset:
            info.ThumbnailOffset = this.GetInt(0);
            break;
          case ExifId.Software:
            info.Software = this.GetStringValue();
            break;
          case ExifId.DateTime:
            info.DateTime = this.GetStringValue();
            break;
          case ExifId.YResolution:
            info.YResolution = this.GetNumericValue(0);
            break;
          case ExifId.ResolutionUnit:
            info.ResolutionUnit = (ExifUnit) this.GetInt(0);
            break;
          case ExifId.Description:
            info.Description = this.GetStringValue();
            break;
          case ExifId.Make:
            info.Make = this.GetStringValue();
            break;
          case ExifId.Model:
            info.Model = this.GetStringValue();
            break;
          case ExifId.Orientation:
            info.OrientationOffset = sectionStart + this.ValueOffset;
            info.Orientation = (ExifOrientation) this.GetInt(0);
            break;
          case ExifId.XResolution:
            info.XResolution = this.GetNumericValue(0);
            break;
          case ExifId.ImageWidth:
            info.Width = this.GetInt(0);
            break;
          case ExifId.ImageHeight:
            info.Height = this.GetInt(0);
            break;
        }
      }
      else
      {
        if (ifd != ExifIFD.Gps)
          return;
        switch (this.Tag)
        {
          case 1:
            if (this.GetStringValue() == "N")
            {
              info.GpsLatitudeRef = ExifGpsLatitudeRef.North;
              break;
            }
            if (!(this.GetStringValue() == "S"))
              break;
            info.GpsLatitudeRef = ExifGpsLatitudeRef.South;
            break;
          case 2:
            if (this.Components != 3)
              break;
            info.GpsLatitude[0] = this.GetNumericValue(0);
            info.GpsLatitude[1] = this.GetNumericValue(1);
            info.GpsLatitude[2] = this.GetNumericValue(2);
            break;
          case 3:
            if (this.GetStringValue() == "E")
            {
              info.GpsLongitudeRef = ExifGpsLongitudeRef.East;
              break;
            }
            if (!(this.GetStringValue() == "W"))
              break;
            info.GpsLongitudeRef = ExifGpsLongitudeRef.West;
            break;
          case 4:
            if (this.Components != 3)
              break;
            info.GpsLongitude[0] = this.GetNumericValue(0);
            info.GpsLongitude[1] = this.GetNumericValue(1);
            info.GpsLongitude[2] = this.GetNumericValue(2);
            break;
        }
      }
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder(64);
      stringBuilder.Append("0x");
      stringBuilder.Append(this.Tag.ToString("X4"));
      stringBuilder.Append("-");
      stringBuilder.Append(((ExifId) this.Tag).ToString());
      if (this.Components > 0)
      {
        stringBuilder.Append(": (");
        stringBuilder.Append(this.GetStringValue(0));
        if (this.Format != ExifTagFormat.UNDEFINED && this.Format != ExifTagFormat.STRING)
        {
          for (int componentIndex = 1; componentIndex < this.Components; ++componentIndex)
            stringBuilder.Append(", " + this.GetStringValue(componentIndex));
        }
        stringBuilder.Append(")");
      }
      return stringBuilder.ToString();
    }
  }
}
