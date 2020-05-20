using System;
using System.Globalization;

namespace MediaParsers
{
  public class WaveFormatExtensible
  {
    public short FormatTag { get; set; }

    public short Channels { get; set; }

    public int SamplesPerSec { get; set; }

    public int AverageBytesPerSecond { get; set; }

    public short BlockAlign { get; set; }

    public short BitsPerSample { get; set; }

    public short ExtraDataSize { get; set; }

    public string ToHexString()
    {
      char[] chars = new char[36];
      BitTools.ToHexHelper((byte) 4, (long) this.FormatTag, 0, chars);
      BitTools.ToHexHelper((byte) 4, (long) this.Channels, 4, chars);
      BitTools.ToHexHelper((byte) 8, (long) this.SamplesPerSec, 8, chars);
      BitTools.ToHexHelper((byte) 8, (long) this.AverageBytesPerSecond, 16, chars);
      BitTools.ToHexHelper((byte) 4, (long) this.BlockAlign, 24, chars);
      BitTools.ToHexHelper((byte) 4, (long) this.BitsPerSample, 28, chars);
      BitTools.ToHexHelper((byte) 4, (long) this.ExtraDataSize, 32, chars);
      return new string(chars);
    }

    public override string ToString()
    {
      return string.Format((IFormatProvider) CultureInfo.InvariantCulture, "WAVEFORMATEX FormatTag: {0}, Channels: {1}, SamplesPerSec: {2}, AvgBytesPerSec: {3}, BlockAlign: {4}, BitsPerSample: {5}, Size: {6} ", this.FormatTag, this.Channels, this.SamplesPerSec, this.AverageBytesPerSecond, this.BlockAlign, this.BitsPerSample, this.ExtraDataSize);
    }
  }
}
