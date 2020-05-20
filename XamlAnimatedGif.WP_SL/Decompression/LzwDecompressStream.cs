using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace XamlAnimatedGif.Decompression
{
  internal class LzwDecompressStream : Stream
  {
    private const int MaxCodeLength = 12;
    private readonly BitReader _reader;
    private readonly LzwDecompressStream.CodeTable _codeTable;
    private int _prevCode;
    private byte[] _remainingBytes;
    private bool _endOfStream;

    public override bool CanRead
    {
      get
      {
        return true;
      }
    }

    public override bool CanSeek
    {
      get
      {
        return false;
      }
    }

    public override bool CanWrite
    {
      get
      {
        return true;
      }
    }

    public override long Length
    {
      get
      {
        throw new NotSupportedException();
      }
    }

    public override long Position
    {
      get
      {
        throw new NotSupportedException();
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    public LzwDecompressStream(byte[] compressedBuffer, int minimumCodeLength)
    {
      this._reader = new BitReader(compressedBuffer);
      this._codeTable = new LzwDecompressStream.CodeTable(minimumCodeLength);
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      if (this._endOfStream)
        return 0;
      int read = 0;
      this.FlushRemainingBytes(buffer, offset, count, ref read);
      while (read < count)
      {
        if (!this.ProcessCode(this._reader.ReadBits(this._codeTable.CodeLength), buffer, offset, count, ref read))
        {
          this._endOfStream = true;
          break;
        }
      }
      return read;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }

    private void InitCodeTable()
    {
      this._codeTable.Reset();
      this._prevCode = -1;
    }

    private static byte[] CopySequenceToBuffer(byte[] sequence, byte[] buffer, int offset, int count, ref int read)
    {
      int num = Math.Min(sequence.Length, count - read);
      Buffer.BlockCopy((Array) sequence, 0, (Array) buffer, offset + read, num);
      read += num;
      byte[] numArray = (byte[]) null;
      if (num < sequence.Length)
      {
        int count1 = sequence.Length - num;
        numArray = new byte[count1];
        Buffer.BlockCopy((Array) sequence, num, (Array) numArray, 0, count1);
      }
      return numArray;
    }

    private void FlushRemainingBytes(byte[] buffer, int offset, int count, ref int read)
    {
      if (this._remainingBytes == null)
        return;
      this._remainingBytes = LzwDecompressStream.CopySequenceToBuffer(this._remainingBytes, buffer, offset, count, ref read);
    }

    [Conditional("DISABLED")]
    private static void ValidateReadArgs(byte[] buffer, int offset, int count)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (offset < 0)
        throw new ArgumentOutOfRangeException("offset", "Offset can't be negative");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", "Count can't be negative");
      if (offset + count > buffer.Length)
        throw new ArgumentException("Buffer is to small to receive the requested data");
    }

    private bool ProcessCode(int code, byte[] buffer, int offset, int count, ref int read)
    {
        if (code < this._codeTable.Count)
        {
            LzwDecompressStream.Sequence sequence = this._codeTable[code];
            if (sequence.IsStopCode)
                return false;
            if (sequence.IsClearCode)
            {
                this.InitCodeTable();
                return true;
            }
            this._remainingBytes = LzwDecompressStream.CopySequenceToBuffer(sequence.Bytes, buffer, offset, count, ref read);
            if (this._prevCode >= 0)
                this._codeTable.Add(this._codeTable[this._prevCode].Append(sequence.Bytes[0]));
        }
        else
        {
            LzwDecompressStream.Sequence sequence1 = this._codeTable[this._prevCode];
            LzwDecompressStream.Sequence local = sequence1;
            //int num = (int)local.Bytes[0];
            LzwDecompressStream.Sequence sequence2 = (local).Append(/*(byte)num*/local.Bytes[0]);
            this._codeTable.Add(sequence2);
            this._remainingBytes = LzwDecompressStream.CopySequenceToBuffer(sequence2.Bytes, buffer, offset, count, ref read);
        }
        this._prevCode = code;
        return true;
    }

    private struct Sequence
    {
      public static LzwDecompressStream.Sequence ClearCode = new LzwDecompressStream.Sequence(true, false);

      public static LzwDecompressStream.Sequence StopCode = new LzwDecompressStream.Sequence(false, true);

      public byte[] Bytes { get; set; }

      public bool IsClearCode { get; set; }

      public bool IsStopCode { get; set; }

      public Sequence(byte[] bytes)
      {
        this = new LzwDecompressStream.Sequence();
        this.Bytes = bytes;
      }

      private Sequence(bool isClearCode, bool isStopCode)
      {
        this = new LzwDecompressStream.Sequence();
        this.IsClearCode = isClearCode;
        this.IsStopCode = isStopCode;
      }

      public LzwDecompressStream.Sequence Append(byte b)
      {
        byte[] bytes = new byte[this.Bytes.Length + 1];
        this.Bytes.CopyTo((Array) bytes, 0);
        bytes[this.Bytes.Length] = b;
        return new LzwDecompressStream.Sequence(bytes);
      }
    }

    private class CodeTable
    {
      private readonly int _minimumCodeLength;
      private readonly LzwDecompressStream.Sequence[] _table;
      private int _count;
      private int _codeLength;

      public LzwDecompressStream.Sequence this[int index]
      {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get
        {
          return this._table[index];
        }
      }

      public int Count
      {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get
        {
          return this._count;
        }
      }

      public int CodeLength
      {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get
        {
          return this._codeLength;
        }
      }

      public CodeTable(int minimumCodeLength)
      {
        this._minimumCodeLength = minimumCodeLength;
        this._codeLength = this._minimumCodeLength + 1;
        int num = 1 << minimumCodeLength;
        this._table = new LzwDecompressStream.Sequence[4096];
        for (int index1 = 0; index1 < num; ++index1)
        {
          LzwDecompressStream.Sequence[] table = this._table;
          int count = this._count;
          this._count = count + 1;
          int index2 = count;
          LzwDecompressStream.Sequence sequence = new LzwDecompressStream.Sequence(new byte[1]{ (byte) index1 });
          table[index2] = sequence;
        }
        this.Add(LzwDecompressStream.Sequence.ClearCode);
        this.Add(LzwDecompressStream.Sequence.StopCode);
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Reset()
      {
        this._count = (1 << this._minimumCodeLength) + 2;
        this._codeLength = this._minimumCodeLength + 1;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Add(LzwDecompressStream.Sequence sequence)
      {
        if (this._count >= this._table.Length)
          return;
        LzwDecompressStream.Sequence[] table = this._table;
        int count = this._count;
        this._count = count + 1;
        int index = count;
        LzwDecompressStream.Sequence sequence1 = sequence;
        table[index] = sequence1;
        if ((this._count & this._count - 1) != 0 || this._codeLength >= 12)
          return;
        this._codeLength = this._codeLength + 1;
      }
    }
  }
}
