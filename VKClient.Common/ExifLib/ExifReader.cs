using System;
using System.IO;

namespace ExifLib
{
  public class ExifReader
  {
    public bool littleEndian;

    public JpegInfo info { get; private set; }

    public ExifReader(Stream stream)
    {
      this.info = new JpegInfo();
      if (stream.ReadByte() != (int) byte.MaxValue || stream.ReadByte() != 216)
        return;
      this.info.IsValid = true;
      while (true)
      {
        int num1 = 0;
        int num2 = 0;
        int marker;
        while (true)
        {
          marker = stream.ReadByte();
          if (marker == (int) byte.MaxValue || num1 != (int) byte.MaxValue)
          {
            num1 = marker;
            ++num2;
          }
          else
            break;
        }
        long position = stream.Position;
        int num3 = stream.ReadByte();
        int num4 = stream.ReadByte();
        int length = num3 << 8 | num4;
        byte[] numArray = new byte[length];
        numArray[0] = (byte) num3;
        numArray[1] = (byte) num4;
        if (stream.Read(numArray, 2, length - 2) == length - 2)
        {
          switch (marker)
          {
            case 192:
            case 193:
            case 194:
            case 195:
            case 197:
            case 198:
            case 199:
            case 201:
            case 202:
            case 203:
            case 205:
            case 206:
            case 207:
              this.ProcessSOF(numArray, marker);
              break;
            case 217:
              goto label_10;
            case 218:
              goto label_7;
            case 225:
              if ((int) numArray[2] == 69 && (int) numArray[3] == 120 && ((int) numArray[4] == 105 && (int) numArray[5] == 102))
              {
                this.ProcessExif(numArray, position);
                break;
              }
              break;
          }
          GC.Collect();
        }
        else
          break;
      }
      return;
label_7:
      return;
label_10:;
    }

    public static JpegInfo ReadJpeg(FileInfo fi)
    {
      DateTime now = DateTime.Now;
      using (FileStream fileStream = fi.OpenRead())
        return new ExifReader((Stream) fileStream)
        {
          info = {
            FileSize = ((int) fi.Length),
            FileName = fi.Name,
            LoadTime = (DateTime.Now - now)
          }
        }.info;
    }

    private void ProcessExif(byte[] section, long sectionStart)
    {
      int num1 = 6;
      byte[] numArray1 = section;
      int index1 = num1;
      int num2 = 1;
      int num3 = index1 + num2;
      if ((int) numArray1[index1] != 0)
        return;
      byte[] numArray2 = section;
      int index2 = num3;
      int num4 = 1;
      int index3 = index2 + num4;
      if ((int) numArray2[index2] != 0)
        return;
      if ((int) section[index3] == 73 && (int) section[index3 + 1] == 73)
      {
        this.littleEndian = true;
      }
      else
      {
        if ((int) section[index3] != 77 || (int) section[index3 + 1] != 77)
          return;
        this.littleEndian = false;
      }
      this.info.LittleEndian = this.littleEndian;
      int offset1 = index3 + 2;
      int num5 = (int) ExifIO.ReadUShort(section, offset1, this.littleEndian);
      int offset2 = offset1 + 2;
      if (num5 != 42)
        return;
      int num6 = ExifIO.ReadInt(section, offset2, this.littleEndian);
      int num7 = offset2 + 4;
      if ((num6 < 8 || num6 > 16) && (num6 < 16 || num6 > section.Length - 16))
        return;
      this.ProcessExifDir(section, num6 + 8, 8, section.Length - 8, 0, ExifIFD.Exif, sectionStart);
    }

    private int DirOffset(int start, int num)
    {
      return start + 2 + 12 * num;
    }

    private void ProcessExifDir(byte[] section, int offsetDir, int offsetBase, int length, int depth, ExifIFD ifd, long sectionStart)
    {
      if (depth > 4)
        return;
      ushort num1 = ExifIO.ReadUShort(section, offsetDir, this.littleEndian);
      if (offsetDir + 2 + 12 * (int) num1 >= offsetDir + length)
        return;
      for (int num2 = 0; num2 < (int) num1; ++num2)
      {
        int sectionOffset = this.DirOffset(offsetDir, num2);
        ExifTag exifTag = new ExifTag(section, sectionOffset, offsetBase, length, this.littleEndian);
        if (exifTag.IsValid)
        {
          switch (exifTag.Tag)
          {
            case 34665:
              int offsetDir1 = offsetBase + exifTag.GetInt(0);
              if (offsetDir1 <= offsetBase + length)
              {
                this.ProcessExifDir(section, offsetDir1, offsetBase, length, depth + 1, ExifIFD.Exif, sectionStart);
                continue;
              }
              continue;
            case 34853:
              int offsetDir2 = offsetBase + exifTag.GetInt(0);
              if (offsetDir2 <= offsetBase + length)
              {
                this.ProcessExifDir(section, offsetDir2, offsetBase, length, depth + 1, ExifIFD.Gps, sectionStart);
                continue;
              }
              continue;
            default:
              exifTag.Populate(this.info, ifd, sectionStart);
              continue;
          }
        }
      }
      if (this.DirOffset(offsetDir, (int) num1) + 4 <= offsetBase + length)
      {
        int num2 = ExifIO.ReadInt(section, offsetDir + 2 + 12 * (int) num1, this.littleEndian);
        if (num2 > 0)
        {
          int offsetDir1 = offsetBase + num2;
          if (offsetDir1 <= offsetBase + length && offsetDir1 >= offsetBase)
            this.ProcessExifDir(section, offsetDir1, offsetBase, length, depth + 1, ifd, sectionStart);
        }
      }
      if (this.info.ThumbnailData != null || this.info.ThumbnailOffset <= 0 || this.info.ThumbnailSize <= 0)
        return;
      this.info.ThumbnailData = new byte[this.info.ThumbnailSize];
      Array.Copy((Array) section, offsetBase + this.info.ThumbnailOffset, (Array) this.info.ThumbnailData, 0, this.info.ThumbnailSize);
    }

    private void ProcessSOF(byte[] section, int marker)
    {
      this.info.Height = (int) section[3] << 8 | (int) section[4];
      this.info.Width = (int) section[5] << 8 | (int) section[6];
      this.info.IsColor = (int) section[7] == 3;
    }
  }
}
