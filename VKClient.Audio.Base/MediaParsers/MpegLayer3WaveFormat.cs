using System;
using System.Globalization;

namespace MediaParsers
{
  public class MpegLayer3WaveFormat
  {
    public WaveFormatExtensible WaveFormatExtensible { get; set; }

    public short Id { get; set; }

    public int BitratePaddingMode { get; set; }

    public short BlockSize { get; set; }

    public short FramesPerBlock { get; set; }

    public short CodecDelay { get; set; }

    public string ToHexString()
    {
      string hexString = this.WaveFormatExtensible.ToHexString();
      char[] chars = new char[24];
      BitTools.ToHexHelper((byte) 4, (long) this.Id, 0, chars);
      BitTools.ToHexHelper((byte) 8, (long) this.BitratePaddingMode, 4, chars);
      BitTools.ToHexHelper((byte) 4, (long) this.BlockSize, 12, chars);
      BitTools.ToHexHelper((byte) 4, (long) this.FramesPerBlock, 16, chars);
      BitTools.ToHexHelper((byte) 4, (long) this.CodecDelay, 20, chars);
      string str = new string(chars);
      return hexString + str;
    }

    public override string ToString()
    {
      return "MPEGLAYER3 " + this.WaveFormatExtensible.ToString() + string.Format((IFormatProvider) CultureInfo.InvariantCulture, "ID: {0}, Flags: {1}, BlockSize: {2}, FramesPerBlock {3}, CodecDelay {4}", this.Id, this.BitratePaddingMode, this.BlockSize, this.FramesPerBlock, this.CodecDelay);
    }
  }
}
