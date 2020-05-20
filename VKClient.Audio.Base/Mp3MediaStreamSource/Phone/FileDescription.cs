namespace Mp3MediaStreamSource.Phone
{
  public class FileDescription
  {
    public string FileId { get; set; }

    public string FilePath { get; set; }

    public long FromByte { get; set; }

    public long ToByte { get; set; }

    public bool IsWholeFile { get; set; }

    public long Length
    {
      get
      {
        return this.ToByte - this.FromByte + 1L;
      }
    }
  }
}
