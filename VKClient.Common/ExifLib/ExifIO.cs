using System;

namespace ExifLib
{
  public static class ExifIO
  {
    public static short ReadShort(byte[] Data, int offset, bool littleEndian)
    {
      if (littleEndian && BitConverter.IsLittleEndian || !littleEndian && !BitConverter.IsLittleEndian)
        return BitConverter.ToInt16(Data, offset);
      return BitConverter.ToInt16(new byte[2]
      {
        Data[offset + 1],
        Data[offset]
      }, 0);
    }

    public static ushort ReadUShort(byte[] Data, int offset, bool littleEndian)
    {
      if (littleEndian && BitConverter.IsLittleEndian || !littleEndian && !BitConverter.IsLittleEndian)
        return BitConverter.ToUInt16(Data, offset);
      return BitConverter.ToUInt16(new byte[2]
      {
        Data[offset + 1],
        Data[offset]
      }, 0);
    }

    public static void WriteUShort(byte[] data, int offset, bool littleEndian, ushort val)
    {
      byte[] bytes = BitConverter.GetBytes(val);
      if (littleEndian && BitConverter.IsLittleEndian || !littleEndian && !BitConverter.IsLittleEndian)
      {
        data[offset] = bytes[0];
        data[offset + 1] = bytes[1];
      }
      else
      {
        data[offset] = bytes[1];
        data[offset + 1] = bytes[0];
      }
    }

    public static int ReadInt(byte[] Data, int offset, bool littleEndian)
    {
      if (littleEndian && BitConverter.IsLittleEndian || !littleEndian && !BitConverter.IsLittleEndian)
        return BitConverter.ToInt32(Data, offset);
      return BitConverter.ToInt32(new byte[4]
      {
        Data[offset + 3],
        Data[offset + 2],
        Data[offset + 1],
        Data[offset]
      }, 0);
    }

    public static uint ReadUInt(byte[] Data, int offset, bool littleEndian)
    {
      if (littleEndian && BitConverter.IsLittleEndian || !littleEndian && !BitConverter.IsLittleEndian)
        return BitConverter.ToUInt32(Data, offset);
      return BitConverter.ToUInt32(new byte[4]
      {
        Data[offset + 3],
        Data[offset + 2],
        Data[offset + 1],
        Data[offset]
      }, 0);
    }

    public static float ReadSingle(byte[] Data, int offset, bool littleEndian)
    {
      if (littleEndian && BitConverter.IsLittleEndian || !littleEndian && !BitConverter.IsLittleEndian)
        return BitConverter.ToSingle(Data, offset);
      return BitConverter.ToSingle(new byte[4]
      {
        Data[offset + 3],
        Data[offset + 2],
        Data[offset + 1],
        Data[offset]
      }, 0);
    }

    public static double ReadDouble(byte[] Data, int offset, bool littleEndian)
    {
      if (littleEndian && BitConverter.IsLittleEndian || !littleEndian && !BitConverter.IsLittleEndian)
        return BitConverter.ToDouble(Data, offset);
      return BitConverter.ToDouble(new byte[8]
      {
        Data[offset + 7],
        Data[offset + 6],
        Data[offset + 5],
        Data[offset + 4],
        Data[offset + 3],
        Data[offset + 2],
        Data[offset + 1],
        Data[offset]
      }, 0);
    }
  }
}
