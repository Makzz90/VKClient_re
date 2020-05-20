using System;
using System.IO;

namespace MediaParsers
{
  public class MpegFrame
  {
    private static int[,] bitrateTable = new int[5, 15]
    {
      {
        0,
        32,
        64,
        96,
        128,
        160,
        192,
        224,
        256,
        288,
        320,
        352,
        384,
        416,
        448
      },
      {
        0,
        32,
        48,
        56,
        64,
        80,
        96,
        112,
        128,
        160,
        192,
        224,
        256,
        320,
        384
      },
      {
        0,
        32,
        40,
        48,
        56,
        64,
        80,
        96,
        112,
        128,
        160,
        192,
        224,
        256,
        320
      },
      {
        0,
        32,
        48,
        56,
        64,
        80,
        96,
        112,
        128,
        144,
        160,
        176,
        192,
        224,
        256
      },
      {
        0,
        8,
        16,
        24,
        32,
        40,
        48,
        56,
        64,
        80,
        96,
        112,
        128,
        144,
        160
      }
    };
    private static int[,] samplingRateTable = new int[3, 3]
    {
      {
        44100,
        48000,
        32000
      },
      {
        22050,
        24000,
        16000
      },
      {
        11025,
        12000,
        8000
      }
    };
    public const int FrameHeaderSize = 4;
    private const int SyncValue = 2047;
    private byte[] frameHeader;

    public int Version { get; private set; }

    public int Layer { get; private set; }

    public bool IsProtected { get; private set; }

    public int BitrateIndex { get; private set; }

    public int SamplingRateIndex { get; private set; }

    public int Padding { get; private set; }

    public Channel Channels { get; private set; }

    public int Bitrate
    {
      get
      {
        if (this.BitrateIndex > 14 || this.BitrateIndex < 0)
          return -2;
        switch (this.Version)
        {
          case 1:
            switch (this.Layer)
            {
              case 1:
                return MpegFrame.bitrateTable[0, this.BitrateIndex] * 1000;
              case 2:
                return MpegFrame.bitrateTable[1, this.BitrateIndex] * 1000;
              case 3:
                return MpegFrame.bitrateTable[2, this.BitrateIndex] * 1000;
              default:
                return -2;
            }
          case 2:
          case 3:
            switch (this.Layer)
            {
              case 1:
                return MpegFrame.bitrateTable[3, this.BitrateIndex] * 1000;
              case 2:
              case 3:
                return MpegFrame.bitrateTable[4, this.BitrateIndex] * 1000;
              default:
                return -2;
            }
          default:
            return -2;
        }
      }
    }

    public int SamplingRate
    {
      get
      {
        if (this.SamplingRateIndex < 0 || this.SamplingRateIndex >= 3)
          return -1;
        switch (this.Version)
        {
          case 1:
            return MpegFrame.samplingRateTable[0, this.SamplingRateIndex];
          case 2:
            return MpegFrame.samplingRateTable[1, this.SamplingRateIndex];
          case 3:
            return MpegFrame.samplingRateTable[2, this.SamplingRateIndex];
          default:
            return -1;
        }
      }
    }

    public int FrameSize
    {
      get
      {
        switch (this.Layer)
        {
          case 1:
            return (12 * this.Bitrate / this.SamplingRate + this.Padding) * 4;
          case 2:
          case 3:
            return (this.Version == 1 ? 144 : 72) * this.Bitrate / this.SamplingRate + this.Padding;
          default:
            return -1;
        }
      }
    }

    public MpegFrame(Stream stream)
      : this(stream, (byte[]) null)
    {
    }

    public MpegFrame(Stream stream, byte[] data)
    {
      this.frameHeader = new byte[4];
      if (data != null)
      {
        for (int index = 0; index < this.frameHeader.Length; ++index)
          this.frameHeader[index] = data[index];
      }
      else if (stream.Read(this.frameHeader, 0, 4) != 4)
        goto label_7;
      if (BitTools.MaskBits(this.frameHeader, 0, 11) == 2047)
      {
        this.Version = MpegFrame.ParseVersion(this.frameHeader);
        this.Layer = MpegFrame.ParseLayer(this.frameHeader);
        this.IsProtected = BitTools.MaskBits(this.frameHeader, 15, 1) != 1;
        this.BitrateIndex = BitTools.MaskBits(this.frameHeader, 16, 4);
        this.SamplingRateIndex = BitTools.MaskBits(this.frameHeader, 20, 2);
        this.Padding = BitTools.MaskBits(this.frameHeader, 22, 1);
        this.Channels = MpegFrame.ParseChannel(this.frameHeader);
        return;
      }
label_7:
      this.frameHeader = (byte[]) null;
    }

    public override string ToString()
    {
      return string.Empty + "FrameSize\t" + this.FrameSize + "\n" + "BitRate\t" + this.Bitrate + "\n" + "SamplingRate" + this.SamplingRate + "\n";
    }

    public void CopyHeader(byte[] destinationBuffer)
    {
      this.frameHeader.CopyTo((Array) destinationBuffer, 0);
    }

    private static int ParseVersion(byte[] frameHeader)
    {
      int num;
      switch (BitTools.MaskBits(frameHeader, 11, 2))
      {
        case 0:
          num = 3;
          break;
        case 2:
          num = 2;
          break;
        case 3:
          num = 1;
          break;
        default:
          num = -1;
          break;
      }
      return num;
    }

    private static int ParseLayer(byte[] frameHeader)
    {
      int num;
      switch (BitTools.MaskBits(frameHeader, 13, 2))
      {
        case 1:
          num = 3;
          break;
        case 2:
          num = 2;
          break;
        case 3:
          num = 1;
          break;
        default:
          num = -1;
          break;
      }
      return num;
    }

    private static Channel ParseChannel(byte[] frameHeader)
    {
      Channel channel;
      switch (BitTools.MaskBits(frameHeader, 24, 2))
      {
        case 0:
          channel = Channel.Stereo;
          break;
        case 1:
          channel = Channel.JointStereo;
          break;
        case 2:
          channel = Channel.DualChannel;
          break;
        case 3:
          channel = Channel.SingleChannel;
          break;
        default:
          channel = Channel.SingleChannel;
          break;
      }
      return channel;
    }
  }
}
